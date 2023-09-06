using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NecFillLib.TrayBottom;

namespace RouteDB
{
    public static class LookUp
    {
        // these constants are used in the calculation logic
        public const string CZ1_0 = "1/0awg";
        public const string CZ4_0 = "4/0awg";
        public const string CZ250 = "250kcmil";
        public const string CZ900 = "900kcmil";
        public const string CZ1000 = "1000kcmil";
        public const string SVPower = "Power";
        public const string CSingle = "1/c";

        // all conductor sizes in relative order
        // from small to large
        public static string[] NecConductorSizes = new string[] 
        { 
            "22awg", "20awg", "18awg", "16awg", "14awg",
            "12awg", "10awg", "8awg", "6awg", "4awg", "2awg",
            CZ1_0, "2/0awg", "3/0awg", CZ4_0,
            CZ250, "300kcmil", "350kcmil", "400kcmil",
            "500kcmil", "600kcmil", "700kcmil", "750kcmil", 
            "800kcmil", CZ900, CZ1000, "1250kcmil",
            "1500kcmil", "1750kcmil", "2000kcmil"
        };

        public static string[] CondutorFormation = new string[] 
        {
            CSingle, "2/c", "3/c", "multicond",
            "1pr", "multipair"
        };

        public static string[] SegSystems = new string[]
        {
            "LV", "HV", "IN"
        };

        public static string[] Services = new string[]
        {
            SVPower, "Control", "Instrument", "Grounding"
        };

        // constants used in NEC library
        public static string[] TrayBottom = new string[]
        {
            LADDER, 
            SOLID, 
            VENT, // perforated
            CHAN_SOLID, // channel
            CHAN_VENT
        };
    }

    #region Project specific data types

    public record CableSpec
    {
        public string ID { get; init; }
        public string CondSize { get; init; }
        public int InsulVolt { get; init; }
        public string CondForm { get; init; }
        public double OD { get; set; }
        public string Service { get; set; }
    }

    public record Cable
    {
        public string ID { get; init; }
        public string SpecID { get; set; }
        public string FromEquipment { get; set; }
        public string ToEquipment { get; set; }
        public string SegSystem { get; set; }
        public string Service { get; set; }
    }

    public record TraySpec
    {
        public string ID { get; set; }
        public string BottomType { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }

    //public record Raceway
    //{
    //    public string ID { get; set; }
    //    public string SpecID { get; set; }
    //    public string RWType { get; set; }
    //    public string FromNode { get; set; }
    //    public string ToNode { get; set; }
    //    public IEnumerable<string> SegSystems { get; init; }
    //}

    public record Tray(TraySpec TraySpec, IEnumerable<(CableSpec CableSpec, int Qty)> Cables);

    #endregion

}
