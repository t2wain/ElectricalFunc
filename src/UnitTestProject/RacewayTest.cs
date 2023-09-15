using Newtonsoft.Json.Bson;
using RacewayLib;
using RouteDB;

namespace UnitTestProject
{
    public class RacewayTest
    {
        Branch _br;

        public RacewayTest()
        {
            _br = DB.GetBranch();
        }

        [Fact]
        public void Should_get_length()
        {
            // act
            var l = _br.GetLength();

            // assert
            Assert.Equal(10, l);
        }

        [Fact]
        public void Shoud_get_nodes()
        {
            // act
            var nodes = _br.GetNodeList();

            // assert
            Assert.Equal(9, nodes.Count());
        }

        [Fact]
        public void Shoud_get_merge_nodes()
        {
            // act
            var nodes = _br.GetNodeListMerge();

            // assert
            Assert.Equal(9, nodes.Count());
        }

        [Fact]
        public void Shoud_be_branch_nodes()
        {
            // arrange
            var nodes = _br.GetNodeListMerge();

            // act
            var (res, br) = RacewayAction.IsBranch(nodes);

            // assert
            Assert.True(res);
            Assert.True(br.StartNode.NodeType == NodeTypeEnum.Start);
            Assert.True(br.EndNode.NodeType == NodeTypeEnum.End);
        }

        [Fact]
        public void Shoud_calc_raceway()
        {
            // act
            var lstRW = _br.CalcRaceway();
            var l = lstRW.Sum(r => r.Length);

            // assert
            Assert.Equal(8, lstRW.Count());
            Assert.Equal(10, l);
        }

        [Fact]
        public void Shoud_raceway_is_branch()
        {
            // arrange
            var lstRW = _br.CalcRaceway();

            // act
            var (res, br) = RacewayAction.IsBranch(lstRW);

            // assert
            Assert.True(res);
            Assert.Equal("N1", br.StartNode.ID);
            Assert.Equal("N9", br.EndNode.ID);
        }

        [Fact]
        public void Should_insert_node()
        {
            // arrange
            var lstNodes = _br.GetNodeList();
            var n = lstNodes.FirstOrDefault(n => n.ID == "N5");

            // act
            var (res, nodes)  = n.AddNode("N5.1", 1);

            // assert
            var n1 = nodes.First(n => n.ID == "N5");
            var n2 = nodes.First(n => n.ID == "N5.1");
            Assert.True(res);
            Assert.Equal(2, nodes.Count());
            Assert.Equal(1, n1.Length);
            Assert.Equal("N5.1", n1.NextNode.ID);
            Assert.Equal(n.NextNode.ID, n2.NextNode.ID);

        }

        [Fact]
        public void Should_split_raceway()
        {
            // arrange
            var nodes = _br.GetNodeList();
            var fn = nodes.First(n => n.ID == "N2");
            var tn = nodes.First(n => n.ID == "N6");
            var rw = new Raceway() 
            { 
                ID = fn.ID, 
                BranchID = fn.BranchID, 
                FromNode = fn, 
                ToNode = tn, 
                Length = 5 
            };

            // act
            var (res, lstRW) = _br.ToRaceway(rw);
            var l = lstRW.Sum(r => r.Length);

            // assert
            Assert.True(res);
            Assert.Equal(4, lstRW.Count());
            Assert.Equal(5, l);
        }

    }
}
