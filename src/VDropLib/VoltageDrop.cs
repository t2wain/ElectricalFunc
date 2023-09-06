using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection.Metadata.Ecma335;

namespace VDropLib
{
    public static class VoltageDrop
    {
        delegate Result<VDrop> CableVDrop(Length length);

        public static IEnumerable<MaxCableLength> CalcVDropTable(VoltAC source, IEnumerable<Load> loads, 
            IEnumerable<Cable> cables, CableSizingParams sizeParams, double maxLengthLimit)
        {

            List<MaxCableLength> CalcMaxLength(IEnumerable<Load> ploads, Cable pcable, Length initLength)
            {
                var lst = new List<MaxCableLength>();
                var cbl = pcable;
                var length = initLength;
                foreach (var l in ploads)
                {
                    if (!pcable.IsAmpacityOK(l, sizeParams).Success) break;
                    else if (TryCalcMaxLength(source, cbl, l, sizeParams, length) is (var maxLength, true))
                    {
                        lst.Add(new MaxCableLength(l, pcable, maxLength));
                        length = maxLength;
                    }
                }
                return lst;
            }

            var lstLoad = loads.OrderBy(l => l.FLA.Value).ToList();
            var lstCable = cables.OrderBy(c => c.RatedAmpacity().Value).ToList();
            var lstLength = new List<MaxCableLength>();
            var initLength = new Length(1.0);
            foreach (var cable in lstCable)
            {
                var lstRes = CalcMaxLength(lstLoad, cable, initLength);
                if (lstLength.Count > 0)
                {
                    lstRes = lstRes.Where(l => l.Length.Value < maxLengthLimit).ToList();
                    initLength = lstRes.Count > 0 ? new(lstRes.Max(l => l.Length.Value)) : initLength;
                }
                lstLength.AddRange(lstRes);
                if (lstRes.FirstOrDefault() is { Load: var load })
                    lstLoad = lstLoad.SkipWhile(pl => pl.Name != load.Name).ToList();
            }

            return lstLength;
        }

        #region Calculate max cable length

        public static Result<Length> TryCalcMaxLength(VoltAC source, Cable cable, 
            Load load, CableSizingParams sizeParams, Length initialLength)
        {
            // calc running length
            CableVDrop fvdrop = InitCableVDropCalc(source, load.FLA, cable);
            var (runLength, runRes) = initialLength.TryCableMaxLength(fvdrop, sizeParams.MaxRunVDrop);
            if (!(runRes && load is MotorLoad m))
                return new(runLength, runRes);

            // run length OK, load is a motor
            // check starting vdrop with run length
            fvdrop = InitCableVDropCalc(source, m.LRC, cable);
            if (runLength.IsVDropOK(fvdrop, sizeParams.MaxStartVDrop) is (_, true))
                return new(runLength, runRes); // run length also OK for starting

            // otherwise, calc start length
            // return the shorter length (run vs start)
            return runLength.TryCableMaxLength(fvdrop, sizeParams.MaxStartVDrop) switch
            {
                (var starLength, true) => new(runLength.Value < starLength.Value ? runLength : starLength, true),
                var r => r
            };
        }

        private static Result<Length> TryCableMaxLength(this Length length, CableVDrop fvdrop, VDrop maxVDrop)
        {
            // calc max length
            switch (CableMaxLengthLoop(0, fvdrop, maxVDrop.Value, length)) 
            {
                case (_, false):
                    return new(new(double.NaN), false); // failed
                case ({ Value: > 0 } l, true):
                    return new(l, true); // found
            }

            // if length is negative, then try another way
            return CableMaxLengthLoop(0, fvdrop, -maxVDrop.Value, length) switch
            {
                ({ Value: > 0 } l, true) => new(l, true), // found
                _ => new(new(double.NaN), false) // failed
            };
        }

        // looping until length is converge or reaching max iteration
        private static Result<Length> CableMaxLengthLoop(int idx, CableVDrop fvdrop, double maxVDrop, Length length)
        {
            if (idx > 30) return new(length, false);
            var (vdrop, ok) = fvdrop(length);
            if (!ok) return new(length, false); // error in vdrop calc
            var vdiff = vdrop.Value - maxVDrop;
            var (newLength, ok2) = length.CalcNextLength(fvdrop, vdiff);
            if (!ok2) return new(length, false); // error in vdrop calc
            return newLength.Success ?
                newLength : // length found, length converged
                CableMaxLengthLoop(idx + 1, fvdrop, maxVDrop, newLength.Value); // loop again
        }

        private static (Result<Length> Result, bool Success) CalcNextLength(this Length length, 
            CableVDrop fvdrop, double vdropDiff)
        {
            double delta = 0.00001;
            var (v1, ok1) = fvdrop(length.AddDeltaLength(delta));
            var (v2, ok2) = fvdrop(length);
            if (ok1 && ok2)
            {
                var lprime = (v1.Value - v2.Value) / delta;
                var ldelta = vdropDiff / lprime;
                var newLength = length.AddDeltaLength(-ldelta);
                return new(new(newLength, Math.Abs(ldelta) <= 0.1), true);
            }
            else return new(new(default, false), false); // error id vdrop calc
        }

        private static Length AddDeltaLength(this Length length, double lengthDelta) =>
            new(length.Value + lengthDelta);

        #endregion

        #region Select min cable size

        public static Result<Cable> TrySelectMinCableSize(VoltAC source, Load load,
            IEnumerable<Cable> cables, CableSizingParams sizeParams, Length cableLength)
        {
            var c = cables
                //.Select(c => c with { Length = new(cableLength) })
                .OrderBy(c => c.RatedAmpacity().Value)
                .FirstOrDefault(c => CheckCableSize(source, c, load, sizeParams, cableLength));
            return new(c, c != null);
        }

        public static Result<Ampacity> IsAmpacityOK(this Cable cable,
            Load load, CableSizingParams szParam)
        {
            var amp = load switch
            {
                NECMotorLoad mtr => mtr.NecFLA,
                _ => load.FLA
            };
            var ampacity = cable.DeratedAmpacity(szParam.CableDeratingFactor);
            return new(ampacity, ampacity.Value >= amp.Value * szParam.SizingFLAFactor);
        }

        private static Result<VDrop> IsVDropOK(this Length length, CableVDrop fvdrop, VDrop maxVDrop)
        {
            var (vdrop, ok) = fvdrop(length);
            return new(vdrop, ok && vdrop!.Value <= maxVDrop.Value);
        }

        public static bool CheckCableSize(VoltAC source, Cable cable, Load load, CableSizingParams sizeParams, Length length)
        {
            if (!cable.IsAmpacityOK(load, sizeParams).Success) return false;

            CableVDrop fvdrop = InitCableVDropCalc(source, load.FLA, cable);
            var (_, isRunVDropOK) = length.IsVDropOK(fvdrop, sizeParams.MaxRunVDrop);
            if (!(isRunVDropOK && load is MotorLoad m))
                return isRunVDropOK;

            //var m = load as MotorLoad;
            fvdrop = InitCableVDropCalc(source, m.LRC, cable);
            return length.IsVDropOK(fvdrop, sizeParams.MaxStartVDrop).Success;
        }

        public static Ampacity RatedAmpacity(this Cable cable) =>
            new(cable.RatedAmpacity.Value * cable.NumberOfParallel);

        public static Ampacity DeratedAmpacity(this Cable cable, double deratingFactor) =>
            new(cable.RatedAmpacity.Value * deratingFactor * cable.NumberOfParallel);

        #endregion

        #region Calculate voltage drop

        public static Result<VDrop> TryCalcVDrop(VoltAC source, Cable cable, LoadAmp load, Length length)
        {
            #region Local functions

            (double R, double X) AmpZPF()
            {
                var z = cable.TotalZImp(length);
                return (load.Value * z.R * load.PF.Value, load.Value * z.X * load.PF.Value);
            }

            (double R, double X) AmpZSinPF()
            {
                var z = cable.TotalZImp(length);
                var sinPF = Math.Sign(Math.Acos(load.PF.Value)) * (load.PF.IsLead ? -1 : 1);
                return (load.Value * z.R * sinPF, load.Value * z.X * sinPF);
            }

            double VLN() => source.Value / Math.Sqrt(3);

            double PhaseFactor() => source.Phase == 1 ? 2 / Math.Sqrt(3) : 1;

            #endregion

            var mpf = AmpZPF();
            var msinpf = AmpZSinPF();
            var vln = VLN();
            var v1 = vln + mpf.R + msinpf.X;
            var v2 = Math.Pow(vln, 2) - Math.Pow(mpf.X - msinpf.R, 2);
            if (v2 < 0) 
                return new(new(double.NaN), false); // error
            else
            {
                var vdrop = (v1 - Math.Sqrt(v2)) / vln * PhaseFactor();
                return new(new(vdrop), true);
            }
        }

        public static ZImp TotalZImp(this Cable cable, Length length) =>
            new(cable.ZBase.R * length.Value / cable.NumberOfParallel, 
                cable.ZBase.X * length.Value / cable.NumberOfParallel);

        #endregion

        private static CableVDrop InitCableVDropCalc(VoltAC source, LoadAmp amp, Cable cable) =>
            length => TryCalcVDrop(source, cable, amp, length);
    }
}
