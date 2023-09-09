using GraphLib;
using NecFillLib;
using static GraphLib.Example.TestData;

namespace RouteLib
{
    public static class RacewayAlgo
    {
        public class VertexNetwork : Dictionary<IVertex, int> { }

        public static VertexNetwork CalcNetworkConnection(this IGraph g, EdgeFilter filter)
        {
            bool f(IEdge e) => g.EdgeFilter(e) && filter(e);
            var g2 = g.SetFilter(f);
            var res = g2.DFS().ToList();
            return res.Aggregate(new VertexNetwork(), (agg, e) => {
                agg.TryAdd(e.FromVertex, e.Component);
                agg.TryAdd(e.ToVertex, e.Component);
                return agg;
            });
        }

        public static bool IsConnected(this VertexNetwork vertexNW, ISet<IVertex> vertices) =>
            vertices.Where(n => vertexNW.ContainsKey(n)).Count() == vertices.Count()
            && vertices.Select(n => vertexNW[n]).Distinct().Count() == 1;

        public static bool IsConnected(this VertexNetwork vertexNW, IEnumerable<IEdge> edges) =>
            vertexNW.IsConnected(edges.Select(e => e.FromVertex)
                .Concat(edges.Select(e => e.FromVertex)).ToHashSet());

    }
}
