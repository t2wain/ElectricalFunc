using RouteLib;
using RW = RouteLib.RWTypeEnum;
using RouteDB;
using static GraphLib.GraphAction;
using GraphLib;
using NecFillLib;
using NecFillLib.Nec2011;

namespace UnitTestProject
{
    public class RouteTest
    {
        IDictionary<string, Raceway> _raceways;
        RouteSpec _rtspec;
        Router _rt;

        public RouteTest()
        {
            DB.GetTraySpecs();
            DB.GetCableSpecs();

            _raceways = DB.GetRaceways();
            _rtspec = new RouteSpec()
            {
                CableID = "C1",
                SegSystemID = "LV",
                FromNode = new("E1"),
                ToNode = new("E4"),
            };

            var edges = _raceways.Values.Cast<IEdge>();
            _rt = new()
            {
                Network = edges.BuildGraph(isDirected: false),
                RWSystems = DB.GetTraySegs(),
            };
        }

        [Fact]
        public void Should_prefer_rw()
        {
            // arrange
            var rw1 = _raceways["R6B"];
            var rw2 = _raceways["R6A"];
            var rs = _rtspec with { PreferRWs = new() { rw1.ID } };

            // act
            var f = RouteAlgo.IsPreferFunc(rs.PreferRWs);

            // assert
            Assert.True(f(rw1));
            Assert.False(f(rw2));
        }

        [Fact]
        public void Should_allow_rw()
        {
            // arrange
            var rw1 = _raceways["R6A"];
            var rw2 = _raceways["R3A"];
            var rw3 = _raceways["R1"];
            var rt = _rt with { BlockedRWs = new() { rw1.ID } };
            var rs = _rtspec with { ExcludeRWs = new() { rw2.ID } };

            // act
            var f = RouteAlgo.AllowRWFunc(rt.BlockedRWs, rs.ExcludeRWs);

            // arrange
            Assert.False(f(rw1)); // blocked
            Assert.False(f(rw2)); // blocked
            Assert.True(f(rw3));
        }

        [Fact]
        public void Should_match_seg_system()
        {
            // arrange
            var rw1 = _raceways["R1"];
            var d1 = _raceways["D1"];
            var j1 = _raceways["J1"];
            var rs2 = _rtspec with { SegSystemID = "IN" };

            // act
            var f = RouteAlgo.ValidateSegSystemFunc(_rt.RWSystems, _rtspec.SegSystemID);
            var f2 = RouteAlgo.ValidateSegSystemFunc(_rt.RWSystems, rs2.SegSystemID);

            // assert
            Assert.True(f(rw1));
            Assert.True(f(d1));
            Assert.True(f(j1));

            Assert.False(f2(rw1));
            Assert.True(f2(d1));
            Assert.True(f2(j1));
        }

        [Fact]
        public void Should_validate_drop()
        {
            // arrange
            var rw1 = _raceways["R1"];
            var d1 = _raceways["D1"];
            var d2 = _raceways["D2"];
            var d4 = _raceways["D4"];

            // act
            var f = _rtspec.ValidateDropFunc();

            // assert
            Assert.True(f(d1));
            Assert.False(f(d2));
            Assert.True(f(d4));
            Assert.True(f(rw1));
        }

        [Fact]
        public void Should_is_drop()
        {
            // arrange
            var rw1 = _raceways["R1"];
            var d1 = _raceways["D1"];
            var j1 = _raceways["J1"];

            // assert
            Assert.True(d1.IsDrop());
            Assert.False(rw1.IsDrop());
            Assert.False(j1.IsDrop());
        }

        [Fact]
        public void Should_is_tray()
        {
            // arrange
            var rw1 = _raceways["R1"];
            var d1 = _raceways["D1"];
            var j1 = _raceways["J1"];

            // assert
            Assert.False(d1.IsTray());
            Assert.True(rw1.IsTray());
            Assert.False(j1.IsTray());
        }

        [Fact]
        public void Should_scale_weight()
        {
            {
                // arrange
                var rw1 = _raceways["R6B"];
                var rw2 = _raceways["R6A"];
                var rs = _rtspec with { PreferRWs = new() { rw1.ID } };

                // act
                var f = RouteAlgo.IsPreferFunc(rs.PreferRWs);
                var f2 = RouteAlgo.ScaleWeightFunc(f);

                // assert
                Assert.Equal(rw1.Weight.Value, f2(rw1));
                Assert.NotEqual(rw2.Weight.Value, f2(rw2));
            }
        }

        [Fact]
        public void Should_calc_best_edges()
        {
            // arrange
            var rw1 = _raceways["R3A"];
            var rw2 = _raceways["R3B"];
            var rws = new List<Raceway> { rw1, rw2 };

            // act
            var rs = _rtspec with { PreferRWs = new() { rw2.ID } };
            var f2 = _rt.BuildGraphWeightFunc(rs, rs.CableFills);
            var edges = f2(rws, rw1.FromNode);
            var se1 = edges.Where(e => e.Edge.ID == rw1.ID).First();
            var se2 = edges.Where(e => e.Edge.ID == rw2.ID).First();

            //assert
            Assert.True(rw1.Weight.Value < se1.Weight.ScaledValue);
            Assert.Equal(rw2.Weight.Value, se2.Weight.ScaledValue);
        }

        [Fact]
        public void Should_route_cable()
        {
            // arrange
            var f = _rt.BuildGraphWeightFunc(_rtspec, _rtspec.CableFills);

            // act
            var path = _rt.Network.RouteCable(_rtspec, f);

            // assert
            Assert.Equal(11.0, path.PathWeight.Value);
            Assert.Equal(11, path.Path.Count());
            Assert.True(path.Path.Any(r => r.ID == "D1"));
            Assert.True(path.Path.Any(r => r.ID == "D4"));
            Assert.True(path.Path.Any(r => r.ID == "R6A"));
        }

        [Fact]
        public void Should_route_cable_2()
        {
            // arrange
            var rs = _rtspec with { ExcludeRWs = new() { "R6A" } };
            var f = _rt.BuildGraphWeightFunc(rs, rs.CableFills);

            // act
            var path = _rt.Network.RouteCable(rs, f);

            // assert
            Assert.Equal(12.0, path.PathWeight.Value);
            Assert.Equal(11, path.Path.Count());
            Assert.True(path.Path.Any(r => r.ID == "D1"));
            Assert.True(path.Path.Any(r => r.ID == "D4"));
            Assert.True(path.Path.Any(r => r.ID == "R6A1"));
        }

        [Fact]
        public void Should_route_cable_3()
        {
            // arrange
            var rs = _rtspec with { PreferRWs = new() { "R6B", "R7B" } };
            var f = _rt.BuildGraphWeightFunc(rs, rs.CableFills);

            // act
            var path = _rt.Network.RouteCable(rs, f);

            // assert
            Assert.Equal(13.0, path.PathWeight.Value);
            Assert.Equal(11, path.Path.Count());
            Assert.True(path.Path.Any(r => r.ID == "D1"));
            Assert.True(path.Path.Any(r => r.ID == "D4"));
            Assert.True(path.Path.Any(r => r.ID == "R6B"));
            Assert.True(path.Path.Any(r => r.ID == "R7B"));
        }

        [Fact]
        public void Should_route_cable_4()
        {
            // arrange
            var tray = DB.GetCableInTray("T4");
            var tf = new TrayFill() { TrayId = "R6A", TraySpecId = tray.TraySpec.ID, TraySpec = tray.TraySpec.GetNecTray() };
            var rt = _rt with { RWFills = new() { { tf.TrayId, tf } } };

            var rs = _rtspec with { CableFills = tray.Cables.Select(t => t.CableSpec.GetNecCable().CalcFillValueOfCable() * t.Qty).ToList() };
            var f = rt.BuildGraphWeightFunc(rs, rs.CableFills);

            // act
            var path = rt.Network.RouteCable(rs, f);

            // assert
            Assert.Equal(12.0, path.PathWeight.Value);
            Assert.Equal(11, path.Path.Count());
            Assert.True(path.Path.Any(r => r.ID == "R6A1"));
        }

        [Fact]
        public void Should_route_cable_5()
        {
            // arrange
            var tray = DB.GetCableInTray("T2");
            var tf = new TrayFill() { TrayId = "R6A", TraySpecId = tray.TraySpec.ID, TraySpec = tray.TraySpec.GetNecTray() };
            var rt = _rt with { RWFills = new() { { tf.TrayId, tf } } };

            var rs = _rtspec with { CableFills = tray.Cables.Select(t => t.CableSpec.GetNecCable().CalcFillValueOfCable() * t.Qty).ToList() };
            var f = rt.BuildGraphWeightFunc(rs, rs.CableFills);

            // act
            var path = rt.Network.RouteCable(rs, f);

            // assert
            Assert.Equal(11.0, path.PathWeight.Value);
            Assert.Equal(11, path.Path.Count());
            Assert.True(path.Path.Any(r => r.ID == "R6A"));
        }

        [Fact]
        public void Should_route_cable_6()
        {
            // arrange
            var tray = DB.GetCableInTray("T2");
            var tf = new TrayFill() { TrayId = "R6A", TraySpecId = tray.TraySpec.ID, TraySpec = tray.TraySpec.GetNecTray() };
            var rt = _rt with { RWFills = new() { { tf.TrayId, tf } } };

            var rs = _rtspec with { CableFills = tray.Cables.Select(t => t.CableSpec.GetNecCable().CalcFillValueOfCable() * t.Qty).ToList() };

            // act
            var (path, tfRes) = rt.RouteCable2(rs, rs.CableFills);

            // assert
            Assert.Equal(11.0, path.PathWeight.Value);
            Assert.Equal(11, path.Path.Count());
            Assert.True(path.Path.Any(r => r.ID == "R6A"));
            Assert.True(tfRes.ContainsKey("R6A"));
        }


        [Fact]
        public void Should_route_cable_7()
        {
            // arrange
            var tray = DB.GetCableInTray("T4");
            var tf = new TrayFill() { TrayId = "R6A", TraySpecId = tray.TraySpec.ID, TraySpec = tray.TraySpec.GetNecTray() };
            var rt = _rt with { RWFills = new() { { tf.TrayId, tf } } };

            var rs = _rtspec with {
                CableFills = tray.Cables.Select(t => t.CableSpec.GetNecCable().CalcFillValueOfCable() * t.Qty).ToList(),
                PreferRWs = new() { "R6A1" },
                IncludeRWs = new() { "J1" }
            };

            // act
            var path = rt.RouteCable(rs, rs.CableFills);
            var errs = rt.ValidateRoute(rs, path.Path.Cast<Raceway>());

            // assert
            Assert.True(path.Path.Any(r => r.ID == "R6A1"));
            Assert.True(path.Path.Any(r => r.ID == "J1"));
            Assert.Empty(errs);
        }

    }
}
