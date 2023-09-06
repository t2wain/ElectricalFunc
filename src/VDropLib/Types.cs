using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDropLib
{
    public record Circuit(VoltAC Source, Cable Cable, LoadAmp Load);

    public record ZImp(double R, double X);

    public record PowerFactor(double Value, bool IsLead = false);

    public record LoadAmp(double Value, PowerFactor PF);

    public record Load(string Name, LoadAmp FLA);

    public record MotorLoad(string Name, LoadAmp FLA, LoadAmp LRC) : Load(Name, FLA);

    public record NECMotorLoad(string Name, LoadAmp FLA, LoadAmp NecFLA, LoadAmp LRC) : MotorLoad(Name, FLA, LRC);

    public record VoltAC(double Value, int Phase);

    public record CableZBase(double R, double X);

    public record Cable(string Name, CableZBase ZBase, Ampacity RatedAmpacity,
        int NumberOfParallel);

    public record VDrop(double Value)
    {
        public double ValuePerc => Value * 100;
    };

    public record Ampacity(double Value);

    public record Length(double Value);

    public record CableSizingParams(VDrop MaxRunVDrop, 
        VDrop MaxStartVDrop, double CableDeratingFactor, double SizingFLAFactor);

    public record Result<T>(T Value, bool Success);

    public record MaxCableLength(Load Load, Cable Cable, Length Length);

}
