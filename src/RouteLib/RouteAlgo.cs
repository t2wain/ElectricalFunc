﻿using GraphLib;
using NecFillLib;
using System.Reflection.Metadata.Ecma335;
using System.Xml.Linq;
using static GraphLib.ShortPathAlgo;
using static NecFillLib.Nec2011.NecFillAlgo2011;

namespace RouteLib
{
    public static class RouteAlgo
    {
        #region Types

        /// <summary>
        /// These are the functions used in the
        /// weight calculation of edges within 
        /// the short path algorithm.
        /// </summary>
        public record GraphWeightParams
        {
            public RWFilter IsPrefer { get; init; }
            public RWFilter AllowRW { get; init; }
            public RWFilter ValidateSegSystem { get; init; }
            public RWFilter ValidateDrop { get; init; }
            public Func<Raceway, double> ScaleWeight { get; init; }
            public RWFilter CheckFill { get; init; }
        }

        /// <summary>
        /// The result of the tray fill calculated during routing
        /// of the cables.
        /// </summary>
        public class TrayFillResult : Dictionary<string, Result<TrayFill>> { }

        #endregion

        /// <summary>
        /// Build a graph data structure from a set of raceway
        /// </summary>
        public static IGraph BuildNetwork(this IEnumerable<Raceway> raceways)
        {
            var q = raceways
                .Where(r => r.Weight.Value >= 0) // no negative edges
                .Where(r => r.FromNode.ID != r.ToNode.ID); // no self loop edges

            return q.BuildGraph(false);
        }

        #region Routing

        /// <summary>
        /// Route the cable with a custom weight function.
        /// </summary>
        /// <param name="ew">Custom weight function used by the short path algorithm. This function
        /// should already encapsultes all the requirement specified by the Router and RouteSpec
        /// objects.</param>
        public static SPResult RouteCable(this IGraph graph, ISPRouteSpec c, CalcEdgeWeight ew) =>
           graph.SingleSourceDijkstra(c, ew);

        /// <summary>
        /// Route the cable.
        /// </summary>
        /// <param name="cableFills">Multiple cable fills values can be included 
        /// if those cables must have a same route path.</param>
        static SPResult RouteCableSimple(this Router rt, RouteSpec c, IEnumerable<CableFill> cableFills)
        {
            var f = rt.BuildWeightFuncParams(c, cableFills);
            var ew = BuildGraphWeightFunc(f);
            return rt.Network.RouteCable(c, ew);
        }

        /// <summary>
        /// Route the cable and also return the tray fill result calculated while routing.
        /// </summary>
        public static (SPResult Route, TrayFillResult TrayFills) RouteCable2(this Router rt, 
            RouteSpec c, IEnumerable<CableFill> cableFills)
        {
            var f = rt.BuildWeightFuncParams(c, cableFills);
            var f2 = f with { CheckFill = RouteAlgo.BuildNecCheckFillFunc(rt.RWFills, cableFills, out var tfRes) };
            var ew = BuildGraphWeightFunc(f2);
            return (rt.Network.RouteCable(c, ew), tfRes);
        }

        public static SPResult RouteCable(this Router rt, RouteSpec c, IEnumerable<CableFill> cableFills)
        {
            // simple case
            if (c.IncludeRWs.Count == 0)
                return rt.RouteCableSimple(c, cableFills);

            // track the route result of each iteration
            var lstPath = new List<SPResult>();

            // the complete route
            var lstRoute = new List<IEdge>();

            #region Inner functions

            ExcludeRW BuildExclRW() =>
                c.ExcludeRWs.Concat(lstRoute.Select(e => e.ID))
                    .Aggregate(new ExcludeRW(), (agg, id) => { agg.Add(id); return agg; });

            RouteSpec BuildRouteSpec(Node fromNode, Node toNode) =>
                c with
                {
                    FromNode = fromNode,
                    ToNode = toNode,
                    ExcludeRWs = BuildExclRW(),
                    IncludeRWs = new()
                };

            (bool Success, Node StartNode) RouteNext(RouteSpec rs, Raceway rw)
            {
                var path = rt.RouteCableSimple(rs, cableFills);
                lstPath.Add(path);
                if (!path.Success) return (false, null);

                lstRoute.AddRange(path.Path);
                var nextNode = rw.ToNode;
                if (!path.Path.Any(e => e.ID == rw.ID))
                {
                    var ew = rt.BuildGraphWeightFunc(rs, cableFills);
                    if (ew(new[] { rw }, rw.ToNode).Count() != 1) 
                        return (false, null);

                    lstRoute.Add(rw);
                    nextNode = rw.GetOtherVertex(rw.ToNode) as Node;
                }
                return (true, nextNode);
            }

            SPResult BuildResult(bool success)
            {
                var length = lstRoute.Sum(e => e.Weight.Value);
                return new(lstRoute, c.FromNode, c.ToNode,
                    success ? new(length) : new(0.0), success);
            }

            #endregion

            #region Loop

            // required raceway in the route
            var inclRW = rt.Network.Edges
                .Where(e => c.IncludeRWs.Contains(e.ID))
                .Cast<Raceway>()
                .ToList();

            var startNode = c.FromNode;
            foreach (var rw in inclRW)
            {
                var rs = BuildRouteSpec(startNode, rw.ToNode);
                var res = RouteNext(rs, rw);
                if (!res.Success) 
                    return BuildResult(false);
                else startNode = res.StartNode;
            };

            #endregion

            #region Final route segment

            if (startNode.ID != c.ToNode.ID)
            {
                var rs = BuildRouteSpec(startNode, c.ToNode);
                var res = rt.RouteCableSimple(c, cableFills);
                lstPath.Add(res);
                if (!res.Success)
                    return BuildResult(false);
                lstRoute.AddRange(res.Path);
            }

            #endregion

            return BuildResult(true);
        }

        #endregion

        #region Raceway filter functions

        /// <summary>
        /// Check if the raceway is a prefer raceway
        /// specified by the route spec.
        /// </summary>
        public static RWFilter IsPreferFunc(PreferRW preferRWs) =>
            r => preferRWs.Contains(r.ID);

        /// <summary>
        /// Check that the raceway is not blocked or excluded
        /// by the router or the route spec.
        /// </summary>
        public static RWFilter AllowRWFunc(BlockRW blockedRWs, ExcludeRW excludeRWs) =>
            r => !blockedRWs.Contains(r.ID) // block by raceway network
                && !excludeRWs.Contains(r.ID);

        /// <summary>
        /// Check that the raceway segregation system
        /// match that of the route spec. 
        /// </summary>
        public static RWFilter ValidateSegSystemFunc(TraySegSystem rwSystems, string cableSegSystem) =>
            r => !r.IsTray() // non-tray does have seg system designation
                || rwSystems.Contains((r.ID, cableSegSystem));

        /// <summary>
        /// Is the raceway of a DROP type
        /// </summary>
        public static bool IsDrop(this Raceway r) => r.Type == RWTypeEnum.DROP;

        /// <summary>
        /// Is the raceway of a TRAY type
        /// </summary>
        public static bool IsTray(this Raceway r) => r.Type == RWTypeEnum.TRAY;

        /// <summary>
        /// Only DROP to the equipments connected to the cable is valid.
        /// Other DROPs in the raceway network are not valid.
        /// </summary>
        public static bool IsValidDrop(this ISPRouteSpec c, Raceway r) => 
            r.IsDrop() && (r.HasVertex(c.Source) || r.HasVertex(c.Target));

        /// <summary>
        /// If the raceway is a DROP, then check the DROP is valid.
        /// </summary>
        public static RWFilter ValidateDropFunc(this ISPRouteSpec c) =>
            r => !r.IsDrop() || c.IsValidDrop(r);

        #endregion

        #region Graph weight function

        /// <param name="rt">Encapsulate the data needed by the weight function</param>
        /// <param name="rtSpec">Encapsute the routing requirement needed by the weight function</param>
        /// <returns>The short path weight function is built based on a series of these
        /// raceway filter functions, edge weight function, and tray fill function.</returns>
        public static GraphWeightParams BuildWeightFuncParams(this Router rt, RouteSpec rtSpec, IEnumerable<CableFill> cableFills)
        {
            var IsPreferF = IsPreferFunc(rtSpec.PreferRWs);
            return new()
            {
                IsPrefer = IsPreferF,
                AllowRW = AllowRWFunc(rt.BlockedRWs, rtSpec.ExcludeRWs),
                ValidateSegSystem = ValidateSegSystemFunc(rt.RWSystems, rtSpec.SegSystemID),
                ValidateDrop = rtSpec.ValidateDropFunc(),
                ScaleWeight = ScaleWeightFunc(IsPreferF),
                CheckFill = BuildNecCheckFillFunc(rt.RWFills, cableFills)
            };
        }

        /// <summary>
        /// Build a weight function that encapsulates the data and the logic to be used
        /// in the SingleSourceDijkstra short path algorithm.
        /// </summary>
        /// <param name="rt">Encapsulate the data needed by the weight function</param>
        /// <param name="rtSpec">Encapsute the routing requirement needed by the weight function</param>
        /// <returns>
        /// Return the weight function that encapsulates the data and the logic to be used
        /// in the SingleSourceDijkstra short path algorithm
        /// </returns>
        public static CalcEdgeWeight BuildGraphWeightFunc(this Router rt, RouteSpec rtSpec, IEnumerable<CableFill> cableFills)
        {
            var fp = rt.BuildWeightFuncParams(rtSpec, cableFills);
            return BuildGraphWeightFunc(fp);
        }

        /// <summary>
        /// Build a weight function that encapsulates the data and the logic to be used
        /// in the SingleSourceDijkstra short path algorithm.
        /// </summary>
        /// <param name="fparams">The short path weight function is built based on a series of these
        /// raceway filter functions, edge weight function, and tray fill function.</param>
        public static CalcEdgeWeight BuildGraphWeightFunc(GraphWeightParams fparams)
        {
            var f = fparams;
            ScaleEdge ScaleLength(Raceway r) => new ScaleEdge(r, new(r.Weight.Value, f.ScaleWeight(r)));

            // custom edge weight algorithm
            IEnumerable<ScaleEdge> ew(IEnumerable<IEdge> outEdges, IVertex source) =>
                outEdges
                    .Cast<Raceway>()
                    .Where(f.AllowRW.Invoke) // exclude block raceway
                    .Where(f.ValidateSegSystem.Invoke) // only matching seg system between cable and raceway
                    .Where(f.ValidateDrop.Invoke) // exclude DROPs not connected to equipments of the cable
                    // exclude raceway if route cause over fill
                    .Where(f.CheckFill.Invoke)
                    .Select(ScaleLength) // apply raceway preference specify by route spec
                    .ToList(); 

            return ew;
        }
        
        /// <summary>
        /// Change the edge weight used by the short path algorithm
        /// when the raceway is a prefer raceway specified by
        /// the route spec.
        /// </summary>
        public static Func<Raceway, double> ScaleWeightFunc(RWFilter isPrefer) =>
            r => isPrefer(r) ? r.Weight.Value // preferred by route spec
                : r.Weight.Value * 10000; // regular

        #endregion

        #region Tray fill function

        /// <summary>
        /// Build a tray fill check function that encapsulate the data and logic
        /// to be used in the weight function check.
        /// </summary>
        /// <param name="rt">Encapsulate the data needed by the weight function</param>
        /// <param name="cableFills">The fill values of cables to be routed</param>
        /// <returns>Return a fill check function to be used in the weight function</returns>
        public static RWFilter BuildNecCheckFillFunc(NecTrayFill rwFills, IEnumerable<CableFill> cableFills)
        {
            // check if cable can be routed through the tray without overfill.
            bool ValidateTrayFill(Raceway r)
            {
                // fill calculation is only needed for TRAY
                if (r.IsTray() && cableFills.Count() > 0 && rwFills.TryGetValue(r.ID, out var tf))
                {
                    var tfRes = tf.AddCableToTray(cableFills);
                    return tfRes.Success ? tfRes.Value.FillPercentage <= 100 : false;
                }
                else return true; // non-tray
            }

            return ValidateTrayFill;
        }

        public static RWFilter BuildNecCheckFillFunc(NecTrayFill rwFills, IEnumerable<CableFill> cableFills, 
            out TrayFillResult trayfills)
        {
            // return the result of the 
            // tray fill calculation during
            // cable routing.
            var d = new TrayFillResult();
            trayfills = d;

            // check if cable can be routed through the tray without overfill.
            bool ValidateTrayFill(Raceway r)
            {
                // fill calculation is only needed for TRAY
                if (r.IsTray() && cableFills.Count() > 0 && rwFills.TryGetValue(r.ID, out var tf))
                {
                    // check if the result has been cached
                    // from earlier calculuation.
                    if (!d.TryGetValue(r.ID, out var tfRes))
                    {
                        tfRes = tf.AddCableToTray(cableFills);
                        // save the result
                        d[tfRes.Value.TrayId] = tfRes;
                    }
                    return tfRes.Success ? tfRes.Value.FillPercentage <= 100 : false;
                }
                else return true; // non-tray
            }

            return ValidateTrayFill;
        }

        #endregion
    }
}
