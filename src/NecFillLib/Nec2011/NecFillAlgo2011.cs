using static NecFillLib.TrayBottom;

namespace NecFillLib.Nec2011
{
    public static class NecFillAlgo2011
    {
        #region Types

        /// <summary>
        /// Properties related to a single cable spec
        /// </summary>
        public record NecCableFillInfo
        {
            public bool IsSingleCond {get; init; }
            public bool IsMultiCond {get; init; }
            public bool IsMultiCondSmall {get; init; }
            public bool IsMultiCondLarge {get; init; }
            public bool IsSingleCondSmall {get; init; }
            public bool IsSingleCondLarge {get; init; }
            public bool IsM2 {get; init; }
            public bool IsM3 {get; init; }
            public bool IsHV {get; init; }
            public bool IsLV {get; init; }
            public bool IsSignal { get; init; }
        }

        /// <summary>
        /// Properties related to all the cables
        /// in the tray for an NEC category.
        /// </summary>
        public record NecTrayFillInfo(CableFill TrayFill)
        {
            readonly NecFillLib.CableFill tf = TrayFill;

            #region Services

            public bool IsHighVoltage => (tf.NO_ALL - tf.NO_LV - tf.NO_SIG) > 0;
            public bool IsLowVoltage => tf.NO_LV > 0;
            public bool IsControlAndSignal => (tf.NO_SIG > 0);
            public bool IsControlAndSignalOnly => (tf.NO_SIG > 0 && tf.NO_SIG == tf.NO_ALL);
            public bool IsMixPowerAndControl => (tf.NO_SIG > 0 && tf.NO_SIG < tf.NO_ALL);
            public bool IsPowerOnly => tf.NO_SIG == 0;
            public bool IsPower => tf.NO_ALL - tf.NO_SIG > 0;

            #endregion

            #region Multi-Conductor

            const int err = 4;

            public bool IsMultiCond => (Math.Round(tf.SA_MC, err) > 0);
            public bool IsMultiCondOnly => (!IsSingleCond && IsMultiCond);
            public bool IsLargeMultiCond => (Math.Round(tf.SD_MC_LG, err) > 0);
            public bool IsLargeMultiCondOnly => (!IsSmallMultiCond && IsLargeMultiCond);
            public bool IsSmallMultiCond => (Math.Round(tf.SA_MC_SM, err) > 0);
            public bool IsSmallMultiCondOnly => (IsSmallMultiCond && !IsLargeMultiCond);
            public bool IsOneLVMultiCondOnly => (IsMultiCondOnly && tf.NO_LV == 1);

            #endregion

            #region Single Conductor

            public bool IsSingleCond => (Math.Round(tf.SD_1C_ALL, err) > 0);
            public bool IsSingleCondOnly => (IsSingleCond && !IsMultiCond);
            public bool IsSmallSingleCond => (Math.Round(tf.SD_1C_ALL - tf.SD_1C_LG, err) > 0);
            public bool IsSmallSingleCondOnly => (!IsLargeSingleCond && IsSmallSingleCond);
            public bool IsLargeSingleCond => (Math.Round(tf.SD_1C_LG, err) > 0);
            public bool IsLargeSingleCondOnly => (IsLargeSingleCond && !IsSmallSingleCond);
            public bool IsMedium2SingleCond => (tf.NO_1C_M2 > 0);
            public bool IsMedium2SingleCondOnly => (tf.NO_1C_M2 == tf.NO_1C);
            public bool IsMedium3SingleCond => (tf.NO_1C_M3 > 0);
            public bool IsMedium3SingleCondOnly => (tf.NO_1C_M3 == tf.NO_1C);

            #endregion

            #region Nec Rule Category

            /// <summary>
            /// Determine the NEC rule category this total fill in the tray.
            /// Note, it must match exactly one category. Otherwise, it is an error.
            /// </summary>
            public bool IsNecRuleA => !IsZero && IsLowVoltage && IsMultiCond;

            /// <summary>
            /// Determine the NEC rule category this total fill in the tray.
            /// Note, it must match exactly one category. Otherwise, it is an error.
            /// </summary>
            public bool IsNecRuleB => !IsZero && IsLowVoltage && !IsMultiCond && IsPower;

            /// <summary>
            /// Determine the NEC rule category this total fill in the tray.
            /// Note, it must match exactly one category. Otherwise, it is an error.
            /// </summary>
            public bool IsNecRuleC => !IsZero && IsHighVoltage;

            // Test that it contains cable fills that match only one
            // NEC category. It is an error if it match none or mutiple
            // categories.
            public bool IsSingleNecRule =>
                (IsNecRuleA ? 1 : 0) + (IsNecRuleB ? 1 : 0) + (IsNecRuleC ? 1 : 0) == 1;

            #endregion

            public bool IsSingleAndMultiCond => IsSingleCond && IsMultiCond;

            public bool IsZero => TrayFill.NO_ALL == 0;
        }

        public record Result<T>(T Value, string Message, bool Success);

        /// <summary>
        /// Properties derived from the tray spec that are used
        /// in the calculation
        /// </summary>
        public record NecTraySpecInfo(NecTray traySpec)
        {
            public bool IsLadder => (traySpec.BottomType == LADDER);
            public bool IsSolid => (traySpec.BottomType == SOLID);
            public bool IsVentilated => (traySpec.BottomType == VENT); // perforated
            public bool IsChannel => (traySpec.BottomType == CHAN_SOLID || traySpec.BottomType == CHAN_VENT);
            public bool IsSolidChannel => (traySpec.BottomType == CHAN_SOLID);
            public bool IsVentilatedChannel => (traySpec.BottomType == CHAN_VENT);
            public bool IsSmallChannel => traySpec.IsMetric ? (traySpec.TrayWidth >= 70 && traySpec.TrayWidth <= 105)
                : (traySpec.TrayWidth == 3 || traySpec.TrayWidth == 4);
            public bool IsLargeChannel => (traySpec.TrayWidth == 6);
            public bool IsShallow => traySpec.TrayDepth <= 6;
            public bool IsDeep => traySpec.TrayDepth > 6;
        }

        public record CableValRes(IEnumerable<string> erros, NecCableFillInfo cable, bool Success);

        #endregion

        #region Cable

        /// <summary>
        /// Calculate more information about the cable fill on a per cable spec basis.
        /// </summary>
        public static NecCableFillInfo CalcCableInfo(this NecCable cableSpec) =>
            cableSpec.CalcCableInfo(cableSpec.IsPower, cableSpec.IsLowVoltage);

        /// <summary>
        /// Calculate more information about the cable fill on a per cable spec basis.
        /// </summary>
        public static NecCableFillInfo CalcCableInfo(this NecCable cableSpec, 
            bool isPower, bool isLowVoltage)
        {
            // all non-power cable will be considered as multi-conductor
            var isSingleCond = !cableSpec.IsMultiConductor && isPower;
            var isMultiCond = !isSingleCond;

            var isMultiCondSmall = isMultiCond && !cableSpec.IsCondSizeGE4O;
            var isMultiCondLarge = isMultiCond && cableSpec.IsCondSizeGE4O;

            var isSingleCondSmall = isSingleCond && !cableSpec.IsCondSizeGE1000;
            var isSingleCondLarge = isSingleCond && cableSpec.IsCondSizeGE1000;

            var isM2 = isSingleCond && cableSpec.IsCondSizeGTE1OAndLTE4O;
            var isM3 = isSingleCond && cableSpec.IsCondSizeGTE250AndLTE900;

            var isHV = !isLowVoltage; // cableSpec.IsLowVoltage;
            var isLV = isLowVoltage; // cableSpec.IsLowVoltage;

            var isSignal = !isPower;

            var c = new NecCableFillInfo()
            {
                IsSingleCond = isSingleCond,
                IsMultiCond = isMultiCond,
                IsMultiCondSmall = isMultiCondSmall,
                IsMultiCondLarge = isMultiCondLarge,
                IsSingleCondSmall = isSingleCondSmall,
                IsSingleCondLarge = isSingleCondLarge,
                IsM2 = isM2,
                IsM3 = isM3,
                IsHV = isHV,
                IsLV = isLV,
                IsSignal = isSignal
            };

            //if (c.ValidateCableInfo() is (var err, false))
            //    throw new Exception(string.Join("\n", err.ToArray()));

            return c;
        }

        /// <summary>
        /// Pre-calculate all the parameters that might be used in the tray fill
        /// calculation on a per cable spec basis.
        /// </summary>
        public static CableFill CalcFillValueOfCable(this NecCable cableSpec) =>
            cableSpec.CalcFillValueOfCable(cableSpec.IsPower, cableSpec.IsLowVoltage);

        /// <summary>
        /// Pre-calculate all the parameters that might be used in the tray fill
        /// calculation on a per cable spec basis.
        /// </summary>
        public static CableFill CalcFillValueOfCable(this NecCable cableSpec, 
            bool isPower, bool isLowVoltage)
        {

            var od = cableSpec.OutsideDiameter;
            var area = cableSpec.CrossSectionArea;
            var cat = cableSpec.CalcCableInfo(isPower, isLowVoltage);

            return new CableFill
            {
                SD_HV = (cat.IsHV ? od : 0),
                NO_SIG = (cat.IsSignal ? 1 : 0),
                NO_LV = (cat.IsLV ? 1 : 0),

                SA_ALL = area,
                SD_ALL = od,

                SA_MC = (cat.IsMultiCond ? area : 0),
                SA_MC_SM = (cat.IsMultiCondSmall ? area : 0),
                SD_MC_LG = (cat.IsMultiCondLarge ? od : 0),

                SD_1C_ALL = (cat.IsSingleCond ? od : 0),
                SA_1C_SM = (cat.IsSingleCondSmall ? area : 0),
                SD_1C_LG = (cat.IsSingleCondLarge ? od : 0),
                NO_1C_M2 = (cat.IsM2 ? 1 : 0),
                NO_1C_M3 = (cat.IsM3 ? 1 : 0),
                NO_1C = (cat.IsSingleCond ? 1 : 0),

                NO_ALL = 1,
                WEIGHT = cableSpec.Weight
            };
        }

        /// <summary>
        /// Ensure that there is no inconsistency about the cable fill value on a per cable spec basis.
        /// </summary>
        public static CableValRes ValidateCableInfo(this NecCableFillInfo cableFillInfo)
        {
            var ci = cableFillInfo;
            var err = new List<string>();

            // can't be both
            if (!(ci.IsMultiCond ^ ci.IsSingleCond))
                err.Add("Condutor must be either single or multiple.");

            // can't be both
            if (ci.IsMultiCondLarge && ci.IsMultiCondSmall)
                err.Add("Multi conductor cannot be both small and large.");

            // can't be both
            if (ci.IsSingleCondSmall && ci.IsSingleCondLarge)
                err.Add("Single conductor cannot be both small and large.");

            // can't be both
            if (ci.IsM2 && ci.IsM3)
                err.Add("Single conductor cannot be both small and large.");

            // can't be both
            if (!(ci.IsHV ^ ci.IsLV))
                err.Add("Service must be LV or HV.");

            // all single condutor must be of power service
            if (ci.IsSignal && ci.IsSingleCond)
                err.Add("Non-power cable must be multi conductor");

            return new(err, cableFillInfo, err.Count == 0);
        }

        #endregion

        #region Tray

        /// <summary>
        /// Calculate tray fill result for a tray without considering the tray current fill.
        /// </summary>
        /// <returns>Cable fill for the tray</returns>
        public static Result<TrayFill> CalcTrayFill(this NecTray traySpec, CableFill cableFill, string trayId)
        {
            var tfi = cableFill.CalcTrayFillInfo();
            var tsi = traySpec.CalcTraySpecInfo();
            var rules = tsi.CalcTrayFillRule(tfi);

            if (!rules.Success)
                return new(new(), rules.Message, false);

            var fillWidth = traySpec.CalcEquivalentTrayFillWidth(cableFill);
            if (!fillWidth.Success)
                return new(new(), fillWidth.Message, false);

            return new(new()
            {
                TrayId = trayId,
                TraySpec = traySpec,
                CableFillA = tfi.IsNecRuleA ? cableFill : new(),
                CableFillB = tfi.IsNecRuleB ? cableFill : new(),
                CableFillC = tfi.IsNecRuleC ? cableFill : new(),
                RuleNames = rules.Value,
                FillPercentage = fillWidth.Value / traySpec.TrayWidth * 100
            }, "", true);
        }

        /// <summary>
        /// Calculate tray fill result for a tray without considering the tray current fill.
        /// </summary>
        /// <returns>Cable fill for the tray</returns>
        public static Result<TrayFill> CalcTrayFill(this NecTray traySpec, IEnumerable<CableFill> cableFills, string trayId) =>
            new TrayFill() 
            {
                TrayId = trayId,
                TraySpecId = traySpec.SpecId,
                TraySpec = traySpec,
            }.AddCableToTray(cableFills);

        /// <summary>
        /// Calculate the total tray fill result for a tray by adding to the tray current fill.
        /// </summary>
        /// <returns>Cable fill for the tray</returns>
        public static Result<TrayFill> AddCableToTray(this TrayFill tray, CableFill cableFill)
        {
            return tray.AddCableToTray(new CableFill[] { cableFill });
        }

        /// <summary>
        /// Calculate the total tray fill result for a tray by adding to the tray current fill.
        /// </summary>
        /// <returns>Cable fill for the tray</returns>
        public static Result<TrayFill> AddCableToTray(this TrayFill tray, IEnumerable<CableFill> cableFills)
        {
            // each cable fill must belong to only one NEC rule category
            var isError = cableFills.Aggregate(false, (agg, c) =>
                agg || !c.CalcTrayFillInfo().IsSingleNecRule);

            if (isError)
                return new(new(), "Error: cable fill matching none or more than one NEC rule categories", false);

            var fills = (A: tray.CableFillA, B: tray.CableFillB, C: tray.CableFillC);

            // The fill calculation of each cable is based
            // on one of theses 3 NEC rule categories,
            // 392.22(A), 392.22(B), or 392.22(C). Each cable fill
            // must be added to the correct NEC category based
            // on the cable construction and usage.
            fills = cableFills.Aggregate(fills, (agg, c) => 
                (A: c.CalcTrayFillInfo().IsNecRuleA ? agg.A + c : agg.A, 
                 B: c.CalcTrayFillInfo().IsNecRuleB ? agg.B + c : agg.B, 
                 C: c.CalcTrayFillInfo().IsNecRuleC ? agg.C + c : agg.C));

            // aggregate variables
            var rules = new List<string>();
            var fillPct = 0.0;
            var messages = new List<string>();

            bool isErr = false;
            // iterate through each fill category and
            // perform the calculation separately
            // for each NEC rule category
            foreach (var f in new[] { fills.A, fills.B, fills.C })
            {
                if (f.CalcTrayFillInfo().IsZero)
                    continue;
                // perform fill calculation
                var tf = tray.TraySpec.CalcTrayFill(f, tray.TrayId);
                isErr = isErr || !tf.Success;
                // track the rule used for each fill category
                rules.AddRange(tf.Value.RuleNames);
                messages.Add(tf.Message);
                // combine the result from each fill category
                fillPct += tf.Value.FillPercentage;
            }

            var res = tray with { RuleNames = rules.ToHashSet<string>(), FillPercentage = fillPct };
            return isErr ?
                new(res, string.Join("; ", messages.ToArray()), false) :
                new(new()
                {
                    TrayId = tray.TrayId,
                    TraySpecId = tray.TraySpecId,
                    TraySpec = tray.TraySpec,
                    CableFillA = fills.A,
                    CableFillB = fills.B,
                    CableFillC = fills.C,
                    RuleNames = rules.ToHashSet(),
                    FillPercentage = fillPct
                }, "", true);
        }

        /// <summary>
        /// NEC tray fill calculation is based on either the diameter or cross-sectional area of cable and tray.
        /// This function transform the result of the calculation into an equivalent tray width as a common base for
        /// further calculation and comparison.
        /// </summary>
        /// <returns>Equivalent tray width</returns>
        public static Result<double> CalcEquivalentTrayFillWidth(this NecTray traySpec, CableFill cableFill)
        {
            var tfi = cableFill.CalcTrayFillInfo();
            var tsi = traySpec.CalcTraySpecInfo();
            var rules = tsi.CalcTrayFillRule(tfi);

            return rules.Success switch
            {
                true => new(traySpec.CalcEquivalentTrayFillWidth(rules.Value.First(), cableFill), "", true),
                _ => new(0, rules.Message, false)
            };
        }

        /// <summary>
        /// NEC tray fill calculation is based on either the diameter or cross-sectional area of cable and tray.
        /// This function transform the result of the calculation into an equivalent tray width as a common base for
        /// further calculation and comparison.
        /// </summary>
        /// <returns>Equivalent tray width</returns>
        public static double CalcEquivalentTrayFillWidth(this NecTray traySpec, string ruleName, CableFill cableFill)
        {
            double pctFill = 0;
            var f = cableFill;
            var tw = traySpec.TrayWidth;
            var td = traySpec.TrayDepth;
            var ta = traySpec.TrayArea;
            var tbl = new NecTable();
            double lf = 0;

            switch (ruleName)
            {
                case NecRule.A1a: // ladder, mix service, 4/0 and larger
                    pctFill = f.SD_MC_LG / tw;
                    break;
                case NecRule.A1b: // ladder, mix service, less than 4/0
                    lf = NecTable.Lookup("TC1", tw);
                    pctFill = f.SA_MC_SM / lf;
                    break;
                case NecRule.A1c: // ladder, mix service, mix size
                    lf = NecTable.Lookup("TC1", tw);
                    pctFill = (f.SA_MC_SM / lf) + (1.2 * f.SD_MC_LG / lf);
                    break;
                case NecRule.A2a: // ladder, non-power only, shallow tray
                    pctFill = f.SA_MC / (0.5 * ta);
                    //pctFill = f.SA_MC / (6 * tw);
                    break;
                case NecRule.A2b: // ladder, non-power only, deep tray
                    pctFill = f.SA_MC / (6 * tw);
                    break;
                case NecRule.A3a: // solid, mix service, 4/0 and larger
                    pctFill = f.SD_MC_LG / (0.9 * tw);
                    break;
                case NecRule.A3b: // solid, mix service, less than 4/0
                    lf = NecTable.Lookup("TC3", tw);
                    pctFill = f.SA_MC_SM / lf;
                    break;
                case NecRule.A3c: // solid, mix service, mix size
                    lf = NecTable.Lookup("TC3", tw);
                    pctFill = (f.SA_MC_SM / lf) + (f.SD_MC_LG / lf);
                    break;
                case NecRule.A4a: // solid, non-power, shallow
                    pctFill = f.SA_MC / (0.4 * ta);
                    break;
                case NecRule.A4b: // solid, non-power, deep
                    pctFill = f.SA_MC / (6 * tw);
                    break;
                case NecRule.A5a: // ventilated channel, one cable only
                    lf = NecTable.Lookup("T2C1", tw);
                    pctFill = f.SA_MC / lf;
                    break;
                case NecRule.A5b: // ventilated channel
                    lf = NecTable.Lookup("T2C2", tw);
                    pctFill = f.SA_MC / lf;
                    break;
                case NecRule.A6a: // solid channel, one cable only
                    lf = NecTable.Lookup("T3C1", tw);
                    pctFill = f.SA_MC / lf;
                    break;
                case NecRule.A6b: // solid channel
                    lf = NecTable.Lookup("T3C2", tw);
                    pctFill = f.SA_MC / lf;
                    break;
                case NecRule.B1a: // ladder, 1000KCMIL and greater only
                    pctFill = f.SD_1C_LG / tw;
                    break;
                case NecRule.B1b: // ladder, GTE 250kcmil and LTE 900kcmil only
                    lf = NecTable.Lookup("T4C1", tw);
                    pctFill = f.SA_1C_SM / lf;
                    break;
                case NecRule.B1c: // ladder, mix single conductor cable sizes
                    lf = NecTable.Lookup("T4C1", tw);
                    pctFill = (f.SA_1C_SM / lf) + (1.1 * f.SD_1C_LG / lf);
                    break;
                case NecRule.B1d: // ladder, GTE 1/0 and LTE 4/0 only
                    pctFill = f.SD_1C_ALL / tw;
                    break;
                case NecRule.B2: // ventilated channel
                    pctFill = f.SD_1C_ALL / tw;
                    break;
                case NecRule.C: // high voltage
                    pctFill = f.SD_HV / tw;
                    break;
                default:
                    pctFill = f.SD_ALL / tw;
                    break;
            }

            return pctFill * tw;
        }

        /// <summary>
        /// Calculate more info about the total cable fill in the tray per NEC rule category
        /// </summary>
        /// <param name="cableFill">Total tray fill per NEC rule category</param>
        public static NecTrayFillInfo CalcTrayFillInfo(this CableFill cableFill) => new(cableFill);

        /// <summary>
        /// Calculate for info about the tray spec
        /// </summary>
        public static NecTraySpecInfo CalcTraySpecInfo(this NecTray traySpec) => new(traySpec);

        /// <summary>
        /// Calculate the NEC rule based on the combination of tray spec 
        /// and the total cable fill in the tray on a per NEC rule category.
        /// </summary>
        /// <param name="trayFillInfo">Note, the tray fill must be of the total fill in the tray 
        /// on a per NEC rule category basis</param>
        public static Result<HashSet<string>> CalcTrayFillRule(this NecTraySpecInfo traySpecInfo, NecTrayFillInfo trayFillInfo)
        {
            var rules = new HashSet<string>();
            var ti = trayFillInfo;
            var ts = traySpecInfo;

            /* 
             * NEC only lists three exlusive calculation categories.
             * Some scenarios do not fit entirely into these categories, such as:
             *   1. Single conductor non-power cable
             *   2. Single conductor grounding cable
             *   3. Mix categories calculations
             *   
             * For the purpose of fill calcuation, my thought is that
             * single grounding condutor should be categorized with 
             * the same service, voltage, and single/multi properties
             * as the circuit power conductors to avoid triggering
             * multiple NEC rule categories in the same tray.
             * 
             */

            #region High voltage 392.22(C)

            if (ti.IsHighVoltage)
            {
                // medium voltage
                rules.Add("392.22(C)"); // NecRule.R3
            }

            #endregion

            #region Multi-conductor, low voltage, 392.22(A)

            // multi-conductor, low voltage
            // 392.22(A)
            else if (ti.IsLowVoltage && ti.IsMultiCond)
            {
                // ladder bottom tray
                if (ts.IsLadder || ts.IsVentilated)
                {
                    // 392.22(A)(2)
                    // non-power only
                    if (ti.IsControlAndSignalOnly)
                    {
                        if (ts.IsShallow)
                            rules.Add(NecRule.A2a);
                        else rules.Add(NecRule.A2b);
                    }
                    // 392.22(A)(1)
                    else if (ti.IsLargeMultiCondOnly)
                        rules.Add(NecRule.A1a); // mix service, 4/0 and larger, NecRule.R1a1)
                    else if (ti.IsSmallMultiCondOnly)
                        rules.Add(NecRule.A1b); // mix service, less than 4/0, NecRule.R1a2
                    else
                        rules.Add(NecRule.A1c); // mix service, mix size, NecRule.R1a3
                }
                else if (ts.IsSolid)
                {
                    // 392.22(A)(4)
                    // non-power only
                    if (ti.IsControlAndSignalOnly) // NecRule.R1d1
                    {
                        if (ts.IsShallow)
                            rules.Add(NecRule.A4a);
                        else rules.Add(NecRule.A4b);
                    }
                    // 392.22(A)(3)
                    else if (ti.IsLargeMultiCondOnly)
                        rules.Add(NecRule.A3a); // mix service, 4/0 and larger, NecRule.R1c1
                    else if (ti.IsSmallMultiCond)
                        rules.Add(NecRule.A3b); // mix service, less than 4/0, NecRule.R1c2
                    else
                        rules.Add(NecRule.A3c); // mix service, mix size, NecRule.R1c3
                }
                // 392.22(A)(5)
                else if (ts.IsVentilatedChannel)
                {
                    if (ti.IsOneLVMultiCondOnly)
                        rules.Add(NecRule.A5a);
                    else rules.Add(NecRule.A5b);
                }
                // 392.22(A)(6)
                else // ti.SolidChannel
                {
                    if (ti.IsOneLVMultiCondOnly)
                        rules.Add(NecRule.A6a);
                    else rules.Add(NecRule.A6b);
                }
            }

            #endregion

            #region Single conductor, low voltage, power 392.22(B)

            // 392.22(B)
            // single conductor, low voltage, power
            else if (ti.IsPower && ti.IsSingleCond && ti.IsLowVoltage)
            {
                // 392.22(B)(1)
                if (ts.IsLadder || ts.IsVentilated)
                {
                    if (ti.IsLargeSingleCondOnly)
                        rules.Add(NecRule.B1a); // 1000KCMIL and greater only
                    else if (ti.IsMedium3SingleCondOnly)
                        rules.Add(NecRule.B1b); // GTE 250kcmil and LTE 900kcmil only
                    else if (ti.IsMedium2SingleCondOnly)
                        rules.Add(NecRule.B1d); // GTE 1/0 and LTE 4/0 only
                    else
                        rules.Add(NecRule.B1c); // mix single conductor cable sizes
                }
                else // ti.VentilatedChannel
                {
                    // single conductor, low voltage, channel tray
                    rules.Add(NecRule.B2); // NecRule.R2b
                }
            }

            #endregion

            return rules.Count switch
            {
               1 => new(rules, rules.First(), true),
               > 1 => new(rules, "Error: Multiple rules found", false),
               _ => new(rules, "Error: No rule found", false)
            };
        }


        #endregion
    }
}
