using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace GraphLib
{
    #region Type aliases

    using EdgeDict = IDictionary<IVertex, IEnumerable<IEdge>>;
    using EdgeMatrix = IDictionary<IVertex, IDictionary<IVertex, IEnumerable<IEdge>>>;
    using VEList = IEnumerable<(IVertex V, IEdge E)>;
    using KVDict = KeyValuePair<IVertex, IEnumerable<IEdge>>;
    using KVMatrix = KeyValuePair<IVertex, IDictionary<IVertex, IEnumerable<IEdge>>>;
    using GetOtherVertex = Func<(IVertex V, IEdge E), IVertex>;

    #endregion

    public static class GraphAction
    {
        #region Build graph

        class Graph : IGraph
        {
            public IEnumerable<IEdge> Edges { get; init; }
            public EdgeFilter EdgeFilter { get; init; }
            public EdgeMatrix Value { get; init; }
            public bool IsDirected { get; init; }
        }

        public static IGraph BuildGraph(this IEnumerable<IEdge> edges, bool isDirected)
        {
            var q = edges.Select(e => (V: e.FromVertex, E: e))
                .Concat(edges.Select(e => (V: e.ToVertex, E: e)))
                .ToList();
            var g = q.BuildMatrix();
            return new Graph() { Value = g, EdgeFilter = e => true, Edges = edges, IsDirected = isDirected };
        }

        private static EdgeMatrix BuildMatrix(this VEList edges) {
            var q1 = edges.GroupBy(t => t.V)
                .Select(g => new KVMatrix(g.Key,
                    g.GroupEdges(t => t.E.GetOtherVertex(g.Key)))).ToList();
            return ImmutableDictionary<IVertex, EdgeDict>.Empty.AddRange(q1);
        }

        private static EdgeDict GroupEdges(this VEList edges, GetOtherVertex fkey)
        {
            var q = edges.GroupBy(fkey).Select(g => 
                new KVDict(g.Key, 
                    g.Select(t => t.E).ToImmutableArray())).ToList();
            return ImmutableDictionary<IVertex, IEnumerable<IEdge>>.Empty.AddRange(q);
        }

        /// <summary>
        /// WARNING: this function return a private internal implementation of the graph object
        /// </summary>
        /// <returns>A private internal implementation graph object</returns>
        public static IGraph SetFilter(this IGraph g, EdgeFilter filter) =>
            new Graph() { Value = g.Value, EdgeFilter = filter, Edges = g.Edges, IsDirected = g.IsDirected };

        #endregion

        #region Edge

        public static IEnumerable<IEdge> GetEdges(this IGraph g) => g.GetEdges(g.EdgeFilter);

        public static IEnumerable<IEdge> GetEdges(this IGraph g, EdgeFilter filter) =>
            g.Edges.Where(filter.Invoke).ToImmutableArray();

        public static IEnumerable<IEdge> GetOutEdges(this IGraph g, IVertex fromVertex) =>
            g.GetOutEdges(fromVertex, g.EdgeFilter);

        public static IEnumerable<IEdge> GetOutEdges(this IGraph g, IVertex fromVertex, EdgeFilter filter)
        {
            bool isOutEdge(IEdge e) => !g.IsDirected || fromVertex.Equals(e.FromVertex);
            var f = new EdgeFilter(e => filter(e) && isOutEdge(e));
            return GetInOutEdges(g, fromVertex, f);
        }

        public static IEnumerable<IEdge> GeInEdges(this IGraph g, IVertex fromVertex) =>
            g.GeInEdges(fromVertex, g.EdgeFilter);

        public static IEnumerable<IEdge> GeInEdges(this IGraph g, IVertex fromVertex, EdgeFilter filter)
        {
            bool isInEdge(IEdge e) => !g.IsDirected || fromVertex.Equals(e.ToVertex);
            var f = new EdgeFilter(e => filter(e) && isInEdge(e));
            return GetInOutEdges(g, fromVertex, f);
        }

        static IEnumerable<IEdge> GetInOutEdges(this IGraph g, IVertex fromVertex, EdgeFilter filter)
        {
            if (g.Value.TryGetValue(fromVertex, out var d))
                return d.Values.SelectMany(kv => kv)
                    .Where(filter.Invoke)
                    .ToImmutableArray();
            else return ImmutableArray<IEdge>.Empty;
        }

        #endregion

        #region Vertex

        public static IEnumerable<IVertex> GetVertices(this IGraph g) =>
            g.GetVertices(g.EdgeFilter);

        public static IEnumerable<IVertex> GetVertices(this IGraph g, EdgeFilter filter) =>
            g.Edges
            .Where(filter.Invoke)
            .SelectMany(e => new IVertex[] { e.FromVertex, e.ToVertex })
            .Distinct()
            .ToImmutableArray();

        public static IEnumerable<IVertex> GetNeigborVertices(this IGraph g, IVertex v) =>
            g.GetNeigborVertices(v, g.EdgeFilter);

        public static IEnumerable<IVertex> GetNeigborVertices(this IGraph g, IVertex v, EdgeFilter filter)
        {
            bool isOutEdge(IEdge e) => !g.IsDirected || v.Equals(e.FromVertex);
            var f = new EdgeFilter(e => filter(e) && isOutEdge(e));
            return g.GetPredecessorNeigborVertices(v, f);
        }

        public static IEnumerable<IVertex> GetPredecessorVertices(this IGraph g, IVertex v) =>
            g.GetPredecessorVertices(v, g.EdgeFilter);

        public static IEnumerable<IVertex> GetPredecessorVertices(this IGraph g, IVertex v, EdgeFilter filter)
        {
            bool isInEdge(IEdge e) => !g.IsDirected || v.Equals(e.ToVertex);
            var f = new EdgeFilter(e => filter(e) && isInEdge(e));
            return g.GetPredecessorNeigborVertices(v, f);
        }

        static IEnumerable<IVertex> GetPredecessorNeigborVertices(this IGraph g, IVertex v, EdgeFilter filter)
        {
            if (g.Value.TryGetValue(v, out var d)) // get edges
                return d.Where(kv => kv.Value.Any(filter.Invoke))
                    .Select(kv => kv.Key)
                    .ToImmutableArray();
            else return ImmutableArray<IVertex>.Empty;
        }

        public static bool HasVertex(this IEdge e, IVertex v) => v.Equals(e.FromVertex) || v.Equals(e.ToVertex);

        public static IVertex GetOtherVertex(this IEdge e, IVertex v) => v.Equals(e.FromVertex) ? e.ToVertex 
            : v.Equals(e.ToVertex) ? e.FromVertex
            : throw new Exception("Vertex not exist");


        #endregion
    }
}
