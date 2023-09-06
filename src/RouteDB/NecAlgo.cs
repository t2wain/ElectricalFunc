using NecFillLib;
using NecFillLib.Nec2011;
using static NecFillLib.Nec2011.NecFillAlgo2011;
using static RouteDB.LookUp;

namespace RouteDB
{
    public static class NecAlgo
    {
        public static IDictionary<string, int> GetCondSize()
        {
            var (Size, _) = LookUp.NecConductorSizes
                .Aggregate((Size: new Dictionary<string, int>(), Idx: 1), (agg, s) =>
                {
                    agg.Size[s] = agg.Idx;
                    // assign index based on relative conductor size
                    return (agg.Size, agg.Idx + 1); 
                });
            return Size;
        }

        public static NecCable GetNecCable(this CableSpec cable) =>
            GetNecCable(new[] { cable }).First();

        // mapp project data to NEC library
        public static IEnumerable<NecCable> GetNecCable(IEnumerable<CableSpec> cables)
        {
            var sizes = GetCondSize();
            int sz(string s) => sizes[s]; // convenient function
            
            var specs = cables
                .Select(c => new NecCable()
                {
                    SpecId = c.ID,
                    OutsideDiameter = c.OD,
                    CrossSectionArea = Math.Pow((c.OD / 2.0), 2) * Math.PI,
                    Weight = 1.0,
                    // all single conductor will be consided as power
                    // for tray fill calculation
                    IsPower = c.Service == SVPower || c.CondForm == CSingle,
                    IsLowVoltage = c.InsulVolt < 2000,
                    IsMultiConductor = c.CondForm != CSingle,
                    // compare conductor size by relative index
                    IsCondSizeGE1000 = sz(c.CondSize) >= sz(CZ1000), 
                    IsCondSizeGE4O = sz(c.CondSize) >= sz(CZ4_0),
                    IsCondSizeGTE1OAndLTE4O = sz(c.CondSize) >= sz(CZ1_0) && sz(c.CondSize) <= sz(CZ4_0),
                    IsCondSizeGTE250AndLTE900 = sz(c.CondSize) >= sz(CZ250) && sz(c.CondSize) <= sz(CZ900)
                }).ToList();

            var errors = specs
                .Select(c => c.CalcCableInfo())
                .Select(ci => ci.ValidateCableInfo()) // data validation
                .Where(err => !err.Success);

            if (errors.Count() > 0)
                throw new Exception("Cable specs errors");

            return specs;

        }

        public static NecTray GetNecTray(this TraySpec tray) =>
            GetNecTray(new[] { tray }).First();

        // mapp project data to NEC library
        public static IEnumerable<NecTray> GetNecTray(this IEnumerable<TraySpec> trays) =>
            trays
                .Select(t => new NecTray()
                { 
                    SpecId = t.ID,
                    TrayWidth = t.Width,
                    TrayDepth = t.Height,
                    TrayArea = t.Width * t.Height,
                    BottomType = t.BottomType,
                    AllowableWeight = 100,
                    IsMetric = false
                });

        // calculate tray fill based on the total
        // number of cables in the tray
        public static Result<TrayFill> GetTrayFill(Tray rw)
        {
            // calculate the fills for each cable
            var fills = rw.Cables
                .Select(c => c.CableSpec.GetNecCable().CalcFillValueOfCable() * c.Qty);

            // get tray spec
            var t1a = rw.TraySpec.GetNecTray();

            // default tray fill value
            Result<TrayFill> tfRes = new(new TrayFill() with
            {
                TrayId = "Test",
                TraySpec = rw.TraySpec.GetNecTray(),
                TraySpecId = t1a.SpecId
            }, "", true);

            // sum up tray fill
            return fills.Aggregate(tfRes, (res, fill) =>
                res.Success switch
                {
                    true => res.Value.AddCableToTray(fill), // add fill to tray
                    _ => res
                });
        }
    }
}
