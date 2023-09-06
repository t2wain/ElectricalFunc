using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NecFillLib
{
    /// <summary>
    /// Pre-calculate parameters of the cable
    /// that are used in the NEC fill calculation 
    /// based on the determined NEC rule.
    /// </summary>
    public record class CableFill()
    {
        #region Properties

        /// <summary>
        /// Sum of area for all cables
        /// </summary>
        public double SA_ALL { get; init; }
        /// Sum of area for multi-conductor cables
        /// </summary>
        public double SA_MC { get; init; }
        /// <summary>
        /// Sum of area for multi-conductor cables 
        /// with conductor size LT 4/0
        /// </summary>
        public double SA_MC_SM { get; init; }
        /// <summary>
        /// Sum of area for single-conductor cables
        /// with conductor size LT 1000 kcmil
        /// </summary>
        public double SA_1C_SM { get; init; }
        /// <summary>
        /// Sum of diameter for all cables
        /// </summary>
        public double SD_ALL { get; init; }
        /// <summary>
        /// Sum of diameter for multi-conductor cables
        /// with conductor size GE 4/0
        /// </summary>
        public double SD_MC_LG { get; init; }
        /// <summary>
        /// Sum of diameter for single-conductor cables
        /// </summary>
        public double SD_1C_ALL { get; init; }
        /// <summary>
        /// Sum of diameter for single-conductor cables
        /// with conductor size GE 1000 kcmil
        /// </summary>
        public double SD_1C_LG { get; init; }
        /// <summary>
        /// Sum of diameter for cables with
        /// insulation voltage GE 2000V
        /// </summary>
        public double SD_HV { get; init; }
        /// <summary>
        /// Sum of cable weight
        /// </summary>
        public double WEIGHT { get; init; }
        /// <summary>
        /// Number of all cables
        /// </summary>
        public int NO_ALL { get; init; }
        /// Number of non-power cable
        /// </summary>
        public int NO_SIG { get; init; }
        /// <summary>
        /// Number of cable with
        /// with insulation voltage LT 2000V
        /// </summary>
        public int NO_LV { get; init; }

        #endregion

        #region Nec2011

        /// <summary>
        /// Number of single conductor cable with
        /// cable size GTE 1/0 and LTE 4/0
        /// </summary>
        public int NO_1C_M2 { get; init; }
        /// <summary>
        /// Number of single conductor cable with
        /// cable size GTE 250kcmil and LTE 900kcmil
        /// </summary>
        public int NO_1C_M3 { get; init; }
        public int NO_1C { get; init; }

        #endregion

        #region Operator overloads

        public static CableFill operator +(CableFill a, CableFill b) =>
            new CableFill
            {
                SD_HV = a.SD_HV + b.SD_HV,
                NO_SIG = a.NO_SIG + b.NO_SIG,
                NO_LV = a.NO_LV + b.NO_LV,

                SA_ALL = a.SA_ALL + b.SA_ALL,
                SD_ALL = a.SD_ALL + b.SD_ALL,

                SA_MC = a.SA_MC + b.SA_MC,
                SA_MC_SM = a.SA_MC_SM + b.SA_MC_SM,
                SD_MC_LG = a.SD_MC_LG + b.SD_MC_LG,

                SD_1C_ALL = a.SD_1C_ALL + b.SD_1C_ALL,
                SA_1C_SM = a.SA_1C_SM + b.SA_1C_SM,
                SD_1C_LG = a.SD_1C_LG + b.SD_1C_LG,
                NO_1C_M2 = a.NO_1C_M2 + b.NO_1C_M2,
                NO_1C_M3 = a.NO_1C_M3 + b.NO_1C_M3,
                NO_1C = a.NO_1C + b.NO_1C,

                NO_ALL = a.NO_ALL + b.NO_ALL,
                WEIGHT = a.WEIGHT + b.WEIGHT
            };

        public static CableFill operator *(CableFill a, int b) =>
            new CableFill
            {
                SD_HV = a.SD_HV * b,
                NO_SIG = a.NO_SIG * b,
                NO_LV = a.NO_LV * b,

                SA_ALL = a.SA_ALL * b,
                SD_ALL = a.SD_ALL * b,

                SA_MC = a.SA_MC * b,
                SA_MC_SM = a.SA_MC_SM * b,
                SD_MC_LG = a.SD_MC_LG * b,

                SD_1C_ALL = a.SD_1C_ALL * b,
                SA_1C_SM = a.SA_1C_SM * b,
                SD_1C_LG = a.SD_1C_LG * b,
                NO_1C_M2 = a.NO_1C_M2 * b,
                NO_1C_M3 = a.NO_1C_M3 * b,
                NO_1C = a.NO_1C * b,

                NO_ALL = a.NO_ALL * b,
                WEIGHT = a.WEIGHT * b
            };

        public static CableFill operator *(int b, CableFill a) => a * b;

        public static CableFill operator -(CableFill a, CableFill b) => a + (b * -1);

        #endregion
    }
}
