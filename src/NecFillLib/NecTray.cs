using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NecFillLib
{
    /// <summary>
    /// Pre-calculate parameters of the tray
    /// that are used in the NEC fill calculation.
    /// </summary>
    public record class NecTray()
    {
        public string SpecId { get; init; }
        public double TrayWidth { get; init; }
        public double TrayDepth { get; init; }
        public double TrayArea { get; init; }
        public double AllowableWeight { get; init; }
        public string BottomType { get; init; }
        public bool IsMetric { get; init; }
    }
}
