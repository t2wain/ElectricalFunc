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
        public void Should_get_out_edges()
        {            
            // arrange
            var edges = TestData.GetEdges();
            var e = edges.First();
            var g = edges.BuildGraph(true);

            // act
            var oe = g.GetOutEdges(new V("w"));

            // assert
            Assert.Equal(2, oe.Count());
            Assert.Contains(oe, e => e.ID == "4");
            Assert.Contains(oe, e => e.ID == "5");

        }

        [Fact]
        public void Should_get_in_edges()
        {
            // arrange
            var edges = TestData.GetEdges();
            var e = edges.First();
            var g = edges.BuildGraph(true);

            // act
            var ie = g.GeInEdges(new V("w"));

            // assert
            Assert.Single(ie);
            Assert.Contains(ie, e => e.ID == "3");

        }

        [Fact]
        public void Should_get_neighbor()
        {
            // arrange
            var edges = TestData.GetEdges();
            var e = edges.First();
            var g = edges.BuildGraph(true);

            // act
            var nv = g.GetNeigborVertices(new V("w"));

            // assert
            Assert.Equal(2, nv.Count());
            Assert.Contains(nv, v => v.ID == "t");
            Assert.Contains(nv, v => v.ID == "x");

        }

        [Fact]
        public void Should_get_predecessor()
        {
            // arrange
            var edges = TestData.GetEdges();
            var e = edges.First();
            var g = edges.BuildGraph(true);

            // act
            var pv = g.GetPredecessorVertices(new V("w"));

            // assert
            Assert.Single(pv);
            Assert.Contains(pv, v => v.ID == "s");

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
