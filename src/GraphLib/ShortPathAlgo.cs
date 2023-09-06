using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace GraphLib
{
    public static class ShortPathAlgo
    {
        #region Types

        /// <summary>
        /// This function should block the out-edges that is not a valid path 
        /// to the target vertex and return adjusted weight edges to each of the next target vertices
        /// </summary>
        /// <param name="outEdges">Out-edges from the source vertex</param>
        /// <param name="source">Source vertex of the out-edges</param>
        /// <returns></returns>
        public delegate IEnumerable<ScaleEdge> CalcEdgeWeight(IEnumerable<IEdge> outEdges, IVertex source);

        public record ScaleEdge(IEdge Edge, ScaleWeight Weight);

        public record SPResult(IEnumerable<IEdge> Path, IVertex Source, IVertex Target, EdgeWeight PathWeight, bool Success);

        public record SPEdge(IVertex Target, IEdge InEdge);

        public record ScaleWeight(double Value, double ScaledValue)
        {
            public static ScaleWeight operator +(ScaleWeight d1, ScaleWeight d2)
            {
                return new ScaleWeight(
                    d1.Value + d2.Value,
                    d1.ScaledValue + d2.ScaledValue);
            }
        }

        public interface ISPRouteSpec
        {
            IVertex Source { get; }
            IVertex Target { get; }
            double CutOffLength { get; }
        }

        public record SPRouteSpec : ISPRouteSpec
        {
            public IVertex Source { get; init; }
            public IVertex Target { get; init; }
            public double CutOffLength { get; init; } = 0.0;
        }


        record SPPath(Dictionary<IVertex, SPEdge> Value);

        record FringeVertex(IVertex Target, ScaleWeight Weight);

        record FringeVertexPQ(PriorityQueue<FringeVertex, double> Value);


        #endregion

        public static SPResult SingleSourceDijkstra(this IGraph g, CalcEdgeWeight ew,
            IVertex source, IVertex target, int cutOffLength = 0) =>
            g.SingleSourceDijkstra(new SPRouteSpec { Source = source, 
                Target = target, CutOffLength = cutOffLength }, ew);

        public static SPResult SingleSourceDijkstra(this IGraph g, ISPRouteSpec rs, CalcEdgeWeight ew)
        {
            #region Setup data structure for the algorithm

            // dictionary of final distances to all the visited vertices
            // relative from the source vertex
            var spVertexW = new Dictionary<IVertex, ScaleWeight>();

            // The distance calculated so far to all the visited vertices
            // relative from the source vertex
            var vertexW = new Dictionary<IVertex, ScaleWeight>();

            // track in-edge to each vertex in a path
            // from source to target
            var spPath = new SPPath(new());

            // Priority queue of vertices to be evaluate
            var vertexPQ = new FringeVertexPQ(new());


            #endregion

            #region Initial values

            vertexW[rs.Source] = new(0.0, 0.0); // weight to source as target
            vertexPQ.PushFringe(new(rs.Source, new(0.0,0.0))); // start with source

            #endregion

            #region Main algorithm

            while (vertexPQ.Value.Count > 0)
            {
                // get next lowest weight vertex
                var fv = vertexPQ.PopFringe();

                // shortest path way to this node already found.
                if (spVertexW.ContainsKey(fv.Target))
                    continue;

                spVertexW[fv.Target] = fv.Weight;
                if (fv.Target.Equals(rs.Target))
                    break; // path is found to target

                // get best edges to neighbor vertices based on custom logic.
                // return the best edges with each edge to the next fringe vertex.
                var edata = ew(g.GetOutEdges(fv.Target), fv.Target);

                // process each edge to the next vertex
                foreach (var ed in edata)
                {
                    // next fringe vertex
                    var neighborVertex = ed.Edge.GetOtherVertex(fv.Target);
                    // new distance to neighbor vertex
                    var distNeighborVertex = spVertexW[fv.Target] + ed.Weight;

                    if (rs.CutOffLength > 0 && distNeighborVertex.Value > rs.CutOffLength)
                        continue;

                    if (spVertexW.ContainsKey(neighborVertex))  {
                        // some how there is another shorter pathway; logical error
                        if (distNeighborVertex.ScaledValue < spVertexW[neighborVertex].ScaledValue)
                            throw new Exception();
                    }
                    else if (!vertexW.ContainsKey(neighborVertex) /* vertex not yet visited */
                        || distNeighborVertex.ScaledValue < vertexW[neighborVertex].ScaledValue) /* shorter path to vertex found */
                    {
                        // neighbor vertex is new and never visited, or
                        // another shorter path way is found to the neighbor vertex
                        vertexW[neighborVertex] = distNeighborVertex;

                        // vertex to be process by the loop in subsequent iteration
                        vertexPQ.PushFringe(new(neighborVertex, distNeighborVertex));

                        // track in-edge to the neighbor vertex
                        spPath.Value[neighborVertex] = new(neighborVertex, ed.Edge);
                    }

                }

            }

            #endregion

            return spPath.GetPath(rs.Source, rs.Target, 
                spVertexW.ContainsKey(rs.Target) ? new(spVertexW[rs.Target].Value) : new(double.NaN));
        }

        #region Methods

        static SPResult GetPath(this SPPath path, IVertex source, IVertex target, EdgeWeight weight)
        {
            var res = new List<IEdge>();
            var d = path.Value;
            var v = target;
            var s = false;
            while (d.ContainsKey(v) && !s)
            {
                var e = d[v];
                res.Add(e.InEdge);
                v = e.InEdge.GetOtherVertex(e.Target);
                s = source.Equals(v);
            }
            res.Reverse();
            return s ? 
                new(res, source, target, weight, s) : 
                new(new List<IEdge>(), source, target, weight, s);
        }

        static void PushFringe(this FringeVertexPQ pq, FringeVertex v) =>
            pq.Value.Enqueue(v, v.Weight.ScaledValue);

        static FringeVertex PopFringe(this FringeVertexPQ pq) =>
            pq.Value.Dequeue();

        #endregion
    }
}
