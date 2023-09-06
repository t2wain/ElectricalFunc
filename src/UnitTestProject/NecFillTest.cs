using RouteDB;
using static NecFillLib.Nec2011.NecFillAlgo2011;
using static System.Math;
using static RouteDB.NecAlgo;
using NecFillLib.Nec2011;
using NecFillLib;

namespace UnitTestProject
{
    public class NecFillTest
    {

        IDictionary<string, CableSpec> _cables;
        IDictionary<string, TraySpec> _trays;

        public NecFillTest()
        {
            _cables = DB.GetCableSpecs();
            _trays = DB.GetTraySpecs();
        }

        [Fact]
        public void Should_get_tray_fill()
        {
            // arrange
            var rw = DB.GetCableInTray("T1");
            var fills = rw.Cables
                .Select(c => c.CableSpec.GetNecCable().CalcFillValueOfCable() * c.Qty);
            var t1a = rw.TraySpec.GetNecTray();

            // act
            var tfRes = t1a.CalcTrayFill(fills, rw.TraySpec.ID);
            var fillPct = tfRes.Value.FillPercentage;
            var rules = tfRes.Value.RuleNames;

            // assert
            Assert.True(tfRes.Success);
            Assert.Equal(124, Math.Round(fillPct, 0));
            Assert.Equal(2, rules.Count());
            Assert.Contains(NecRule.C, rules);
            Assert.Contains(NecRule.B1d, rules);

        }

        [Fact]
        public void Should_get_tray_fill_2()
        {
            // arrange
            var rw = DB.GetCableInTray("T1");

            // act
            var tfRes = GetTrayFill(rw);
            var fillPct = tfRes.Value.FillPercentage;
            var rules = tfRes.Value.RuleNames;

            // assert
            Assert.True(tfRes.Success);
            Assert.Equal(124, Math.Round(fillPct, 0));
            Assert.Equal(2, rules.Count());
            Assert.Contains(NecRule.C, rules);
            Assert.Contains(NecRule.B1d, rules);

        }

        [Fact]
        public void Should_get_tray_fill_3()
        {
            // arrange
            var rw = DB.GetCableInTray("T2");

            // act
            var tfRes = GetTrayFill(rw);
            var fillPct = tfRes.Value.FillPercentage;
            var rules = tfRes.Value.RuleNames;

            // assert
            Assert.True(tfRes.Success);
            Assert.Equal(86, Math.Round(fillPct, 0));
            Assert.Equal(3, rules.Count());
            Assert.Contains(NecRule.C, rules);
            Assert.Contains(NecRule.B1b, rules);
            Assert.Contains(NecRule.A2a, rules);

        }

        [Fact]
        public void Should_get_tray_fill_4()
        {
            // arrange
            var rw = DB.GetCableInTray("T3");

            // act
            var tfRes = GetTrayFill(rw);
            //var fillPct = tfRes.Value.FillPercentage;
            var rules = tfRes.Value.RuleNames;

            // assert
            Assert.True(tfRes.Success);
            //Assert.Equal(86, Math.Round(fillPct, 0));
            Assert.Single(rules);
            Assert.Contains(NecRule.A2a, rules);

        }

        [Fact]
        public void Should_get_cable_fill()
        {
            // arrange
            var c5 = _cables["5"]; // 500kcmil, 600v, 3/c, power

            var c5a = NecAlgo.GetNecCable(c5);

            // act
            var f = c5a.CalcFillValueOfCable();

            // assert
            Assert.Equal(f.SA_ALL, c5a.CrossSectionArea);
            Assert.Equal(f.SD_ALL, c5a.OutsideDiameter);
            Assert.Equal(f.SD_MC_LG, c5a.OutsideDiameter);
            Assert.Equal(1, f.NO_ALL);
            Assert.Equal(1, f.NO_LV);

            Assert.Equal(0, f.SA_MC_SM);
            Assert.Equal(0, f.SA_1C_SM);
            Assert.Equal(0, f.SD_1C_ALL);
            Assert.Equal(0, f.SD_1C_LG);
            Assert.Equal(0, f.SD_HV);
            Assert.Equal(0, f.NO_SIG);
            Assert.Equal(0, f.NO_1C);
        }

        [Fact]
        public void Should_map_to_NEC_tray()
        {
            // arrange
            var t1 = _trays["36"]; // 36in, ladder

            // act
            var t1a = t1.GetNecTray();

            // assert
            Assert.Equal(t1a.BottomType, t1.BottomType);
            Assert.Equal(t1a.TrayWidth, t1.Width);
            Assert.Equal(t1a.TrayDepth, t1.Height);
            Assert.Equal(Round(t1a.TrayArea, 5), 
                Round(t1.Width * t1.Height, 5));
        }

        [Fact]
        public void Should_map_to_NEC_cable()
        {
            // arrange
            var c1 = _cables["5"]; // 500kcmil, 600v, 3/c

            // act
            var c1a = c1.GetNecCable();

            // assert
            Assert.Equal(Round(c1a.CrossSectionArea, 5), 
                Round(Pow(c1.OD / 2, 2) * PI, 5));
            Assert.Equal(c1a.OutsideDiameter, c1.OD);

            Assert.True(c1a.IsCondSizeGTE250AndLTE900);
            Assert.True(c1a.IsLowVoltage);
            Assert.True(c1a.IsMultiConductor);
            Assert.True(c1a.IsPower);
            Assert.True(c1a.IsCondSizeGE4O);

            Assert.False(c1a.IsCondSizeGE1000);
            Assert.False(c1a.IsCondSizeGTE1OAndLTE4O);

        }
    }
}
