using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDropLib;

namespace AppConsole
{
    internal class VDropTest
    {
        Cable[] _cables;
        ImmutableArray<MotorLoad> _loads;
        VoltAC _source;
        CableSizingParams _szParams;

        public VDropTest()
        {
            _cables = TestData.GetCablesLV().ToArray();
            _loads = TestData.GetMotorLV();
            _source = new VoltAC(480, 3);
            _szParams = new CableSizingParams(new(0.05), new(0.15), 0.88, 1.25);
        }

        public void Run()
        {
            CalcVDropTable();
            CalcVDrop();
            SelectMinCableSize();
        }

        void CalcVDropTable()
        {
            Console.WriteLine("Calculate voltage drop table");
            Console.WriteLine("================================");
            Console.WriteLine();

            var vdropTable = VoltageDrop.CalcVDropTable(_source, _loads, _cables, _szParams, 10000);
            var q = vdropTable.GroupBy(i => i.Cable);
            foreach (var g in q)
            {
                Console.WriteLine("Cable: {0}", g.Key.Name);
                Console.WriteLine();
                foreach (var l in g)
                    Console.WriteLine("\t{0} : {1}", l.Load.Name, Math.Round(l.Length.Value));
                Console.WriteLine();
            }
        }

        void CalcVDrop()
        {
            Console.WriteLine("Calculate voltage drop");
            Console.WriteLine("=========================");
            Console.WriteLine();

            var load = _loads[0];
            var cable = _cables[0];
            var length = new Length(3640);

            Console.WriteLine("Source: {0}V, Load: {1}, Cable: {2}, Length: {3}ft",
                _source.Value, load.Name, cable.Name, length.Value);

            var runVDrop = VoltageDrop.TryCalcVDrop(_source, cable, load.FLA, length);
            var startVDrop = VoltageDrop.TryCalcVDrop(_source, cable, load.LRC, length);
            Console.WriteLine("Voltage drop, Running: {0}%, Starting: {1}%", 
                Math.Round(runVDrop.Value.ValuePerc,1), 
                Math.Round(startVDrop.Value.ValuePerc,1));
            Console.WriteLine();
        }

        void SelectMinCableSize()
        {
            Console.WriteLine("Select minimum cable size");
            Console.WriteLine("=============================");
            Console.WriteLine();


            var load = _loads[15];
            var length = new Length(1800);

            Console.WriteLine("Source: {0}V, Load: {1}, Length: {2}ft",
                _source.Value, load.Name, length.Value);

            Console.WriteLine("VDrop criteria, Max run: {0}%, Max start: {1}%", 
                _szParams.MaxRunVDrop.ValuePerc, 
                _szParams.MaxStartVDrop.ValuePerc);

            var cable = VoltageDrop.TrySelectMinCableSize(_source, load, _cables, _szParams, length);
            Console.WriteLine("Minimum cable size: {0}", cable.Value.Name);
            Console.WriteLine();
        }
    }
}
