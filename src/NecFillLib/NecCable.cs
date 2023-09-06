using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NecFillLib
{
    /// <summary>
    /// Tranform the physical properties of the cable
    /// and its usage into properties that will
    /// be used in the NEC fill calculation.
    /// </summary>
    public record NecCable()
    {
        public string SpecId { get; init; } = "";
        /// <summary>
        /// Outside diameter
        /// </summary>
        public double OutsideDiameter { get; init; }
        /// Cross-sectional area
        /// </summary>
        public double CrossSectionArea { get; init; }
        /// <summary>
        /// Weight per unit length
        /// </summary>
        public double Weight { get; init; }
        /// Power cable
        /// </summary>
        public bool IsPower { get; init; }
        /// <summary>
        /// Insulation voltage is LT 2000V
        /// </summary>
        public bool IsLowVoltage { get; init; }
        /// <summary>
        /// A multi-conductor cable
        /// </summary>
        public bool IsMultiConductor { get; init; }
        /// <summary>
        /// The cable conductor size is GE 1000kcmil
        /// </summary>
        public bool IsCondSizeGE1000 { get; init; }
        /// <summary>
        /// The cable conductor size is GE 4/0 and LT 1000kcmil
        /// </summary>
        public bool IsCondSizeGE4O { get; init; }
        /// <summary>
        /// The single conductor size GTE 1/0 and LTE 4/0
        /// </summary>
        public bool IsCondSizeGTE1OAndLTE4O { get; init; }
        /// The single conductor size GTE 250 kcmil and LTE 900kcmil
        /// </summary>
        public bool IsCondSizeGTE250AndLTE900 { get; init; }
    };
}
