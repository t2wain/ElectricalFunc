using GraphLib;
using GraphLib.Example;
using V = GraphLib.Example.TestData.V;

namespace UnitTestProject
{
    public class GraphTest
    {

        [Fact]
        public void Should_build_valid_graph()
        {
            // arrange
            var edges = TestData.GetEdges();
            var e = edges.First();
            var g = edges.BuildGraph(false);

            // assert
            Assert.True(e.HasVertex(e.FromVertex));
            Assert.True(e.HasVertex(e.ToVertex));
            Assert.True(e.GetOtherVertex(e.FromVertex).Equals(e.ToVertex));

            Assert.Equal(2, g.Value[new V("s")].Keys.Count);
            Assert.Equal(3, g.Value[new V("w")].Keys.Count);
            Assert.Equal(4, g.Value[new V("x")].Keys.Count);

            Assert.Equal("1", g.Value[new V("r")][new V("v")].First().ID);
            Assert.Equal("1", g.Value[new V("v")][new V("r")].First().ID);

            Assert.Equal(8, g.GetVertices().Count());
            Assert.Equal(4, g.GetNeigborVertices(new V("x")).Count());

            Assert.Equal(10, g.GetEdges().Count());
            Assert.Equal(4, g.GetOutEdges(new V("x")).Count());
        }

        [Fact]
        public void Should_calc_BFS()
        {
            // action
            var edges = TestData.GetEdges();
            var g = edges.BuildGraph(false);
            var res = g.BFS(new V("s")).ToList();

            // assert
            Assert.Equal(7, res.Count());
            Assert.Equal(7, res.Select(e => e.ChildVertex).Distinct().Count());
            Assert.Equal(7, res.Select(e => e.ChildVertex).Count());
        }

        [Fact]
        public void Should_calc_DFS()
        {
            // action
            var edges = TestData.GetEdges2();
            var g = edges.BuildGraph(true);
            var res = g.DFS().ToList();

            // assert
            Assert.Equal(8, res.Count);
        }

        [Fact]
        public void Should_calc_ShortPath()
        {
            // action
            var res = TestData.ShortPath();

            // assert
            Assert.True(res.Success);
            Assert.Equal(11, res.PathWeight.Value);
            Assert.True(res.Path.First().HasVertex(res.Source));
            Assert.True(res.Path.Last().HasVertex(res.Target));
        }

        [Fact]
        public void Should_calc_ShortPath_with_edge_ID_3()
        {
            // action
            var res = TestData.ShortPath2();

            // assert
            Assert.True(res.Success);
            Assert.Equal(11, res.PathWeight.Value);
            Assert.True(res.Path.Any(e => e.ID == "3"));
            Assert.True(res.Path.First().HasVertex(res.Source));
            Assert.True(res.Path.Last().HasVertex(res.Target));
        }

    }
}
