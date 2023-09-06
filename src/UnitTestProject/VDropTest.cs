using VDropLib;

namespace UnitTestProject
{
    public class VDropTest
    {
        IList<Cable> _cables;
        IList<MotorLoad> _motors;
        CableSizingParams _szParams;
        VoltAC _source;

        public VDropTest()
        {
            _cables = TestData.GetCablesLV();
            _motors = TestData.GetMotorLV();
            _szParams = new CableSizingParams(new(0.05), new(0.15), 0.88, 1.25);
            _source = new VoltAC(480, 3);
        }

        [Fact]
        public void Should_calculate_vdrop()
        {
            // arrange
            var cable = _cables[0];
            var load = _motors[0];

            // act
            var vdrop = VoltageDrop.TryCalcVDrop(_source, cable, load.LRC, new(3640));
            var amacityOK = cable.IsAmpacityOK(load, _szParams);
            var maxLength = VoltageDrop.TryCalcMaxLength(_source, cable, load, _szParams, new(1.0));
            var cableOK = VoltageDrop.CheckCableSize(_source, cable, load, _szParams, new(Math.Floor(maxLength.Value.Value)));

            // assert
            Assert.True(vdrop.Success);
            Assert.True(amacityOK.Success);
            Assert.True(maxLength.Success);
            Assert.True(cableOK);
        }

        [Fact]
        public void Should_calculate_vdrop_table()
        {
            // arrange

            // act
            var lstLength = VoltageDrop.CalcVDropTable(_source, _motors, _cables, _szParams, 10000);

            // assert
            Assert.Equal(169, lstLength.Count());
        }

        [Fact]
        public void Should_select_minimum_cable_size()
        {
            // arrange
            var load = _motors[15];

            // act
            var minCable = VoltageDrop.TrySelectMinCableSize(_source, load, _cables, _szParams, new(1800));

            // assert
            Assert.Equal("2x4/0awg", minCable.Value.Name);
        }
    }
}