using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NecFillLib
{
    /// <summary>
    /// Track the sum of cable fills of all the cables
    /// in the tray and the result of the fill calculation.
    /// </summary>
    public record TrayFill()
    {
        public string TrayId { get; init; } = "";
        public string TraySpecId { get; init; } = "";
        public NecTray TraySpec { get; init; }
        /// <summary>
        /// Sum of cable fills matching 392.22(A) rule category.
        /// It is important that each cable spec must match only
        /// one NEC rule category and added to the proper category.
        /// Each NEC category will be evaluated independently from
        /// other categories and will generate a rule for its own
        /// category.
        /// </summary>
        public CableFill CableFillA { get; init; } = new();
        /// <summary>
        /// Sum of cable fills matching 392.22(B) rule category.
        /// It is important that each cable spec must match only
        /// one NEC rule category and added to the proper category.
        /// Each NEC category will be evaluated independently from
        /// other categories and will generate a rule for its own
        /// category.
        /// </summary>
        public CableFill CableFillB { get; init; } = new();
        /// <summary>
        /// Sum of cable fills matching 392.22(C) rule category.
        /// It is important that each cable spec must match only
        /// one NEC rule category and added to the proper category.
        /// Each NEC category will be evaluated independently from
        /// other categories and will generate a rule for its own
        /// category.
        /// </summary>
        public CableFill CableFillC { get; init; } = new();
        /// <summary>
        /// The fill percentage calculation is based on
        /// the combination of one or more NEC rules, one rule
        /// per NEC rule category.
        /// </summary>
        public HashSet<string> RuleNames { get; init; } = new();
        public double FillPercentage { get; init; } = 0.0;

    }
}
