using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphLib.Example
{
    using static GraphLib.ShortPathAlgo;
    using EdgeMatrix = IDictionary<IVertex, IDictionary<IVertex, IEnumerable<IEdge>>>;

    public static class TestData
    {
        public record V(string ID) : IVertex { }

        public record E(string ID, IVertex FromVertex, IVertex ToVertex, EdgeWeight Weight) : IEdge { }

        public record Graph(IEnumerable<IEdge> Edges, EdgeFilter EdgeFilter, EdgeMatrix Value) : IGraph { }

        public static IEnumerable<IEdge> GetEdges()
        {
            return new List<E>()
            {
                new("1", new V("r"), new V("v"), new(1.0)),
                new("2", new V("r"), new V("s"), new(1.0)),
                new("3", new V("s"), new V("w"), new(1.0)),
                new("4", new V("w"), new V("t"), new(1.0)),
                new("5", new V("w"), new V("x"), new(1.0)),
                new("6", new V("t"), new V("x"), new(1.0)),
                new("7", new V("t"), new V("u"), new(1.0)),
                new("8", new V("x"), new V("u"), new(1.0)),
                new("9", new V("x"), new V("y"), new(1.0)),
                new("10", new V("u"), new V("y"), new(1.0)),
            };
        }

        public static IEnumerable<IEdge> GetEdges2()
        {
            return new List<E>()
            {
                new("1", new V("u"), new V("v"), new(1.0)),
                new("2", new V("u"), new V("x"), new(1.0)),
                new("3", new V("x"), new V("v"), new(1.0)),
                new("4", new V("y"), new V("x"), new(1.0)),
                new("5", new V("w"), new V("y"), new(1.0)),
                new("6", new V("w"), new V("z"), new(1.0)),
                new("7", new V("z"), new V("z"), new(1.0)),
                new("8", new V("v"), new V("y"), new(1.0)),
            };
        }

        public static IEnumerable<IEdge> GetEdges3()
        {
            return new List<E>()
            {
                new("1", new V("s"), new V("t"), new(3.0)),
                new("2", new V("s"), new V("y"), new(5.0)),
                new("3", new V("t"), new V("y"), new(2.0)),
                new("4", new V("y"), new V("t"), new(1.0)),
                new("5", new V("t"), new V("x"), new(6.0)),
                new("6", new V("y"), new V("x"), new(4.0)),
                new("7", new V("y"), new V("z"), new(6.0)),
                new("8", new V("x"), new V("z"), new(2.0)),
                new("9", new V("z"), new V("s"), new(3.0)),
                new("10", new V("z"), new V("x"), new(7.0)),
            };
        }

        public static SPResult ShortPath()
        {
            // custom edge weight algorithm
            IEnumerable<ScaleEdge> ew(IEnumerable<IEdge> outEdges, IVertex source) =>
                outEdges.Select(e => new ScaleEdge(e, new(e.Weight.Value, e.Weight.Value * 100)));

            var edges = GetEdges3();
            var g = edges.BuildGraph(true);
            var res = g.SingleSourceDijkstra(ew, new V("s"), new V("z"), 0);
            return res;
        }

        public static SPResult ShortPath2()
        {
            // specify a weight preference for edge 3
            double Scaled(IEdge e) =>
                e.ID == "3" ? e.Weight.Value : e.Weight.Value * 100;

            // custom edge weight algorithm
            IEnumerable<ScaleEdge> ew(IEnumerable<IEdge> outEdges, IVertex source) =>
                outEdges.Select(e => new ScaleEdge(e, new(e.Weight.Value, Scaled(e))));

            var edges = GetEdges3();
            var g = edges.BuildGraph(true);
            var res = g.SingleSourceDijkstra(ew, new V("s"), new V("z"), 0);
            return res;
        }
    }
}
