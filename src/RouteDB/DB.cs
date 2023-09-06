using RW = RouteLib.RWTypeEnum;
using static RouteDB.LookUp;
using static NecFillLib.TrayBottom;
using RouteLib;

namespace RouteDB
{
    /// <summary>
    /// An example of how a project might store data
    /// and map the project data to data types
    /// in various libraries
    /// </summary>
    public static class DB
    {
        #region For NEC tray fill

        // example of project cable specs
        static IDictionary<string, CableSpec> _cables;

        // example of project tray specs
        static IDictionary<string, TraySpec> _trays;

        public static IDictionary<string, CableSpec> GetCableSpecs()
        {
            if (_cables == null)
                _cables = new List<CableSpec>
                {
                    new() { ID = "1", CondSize = "10awg", InsulVolt = 600, CondForm = "multicond", OD = 0.1, Service = SVPower },
                    new() { ID = "2", CondSize = "2awg", InsulVolt = 600, CondForm = "3/c", OD = 0.1, Service = SVPower },
                    new() { ID = "3", CondSize = "2/0awg", InsulVolt = 600, CondForm = "3/c", OD = 0.1, Service = SVPower  },
                    new() { ID = "4", CondSize = CZ250, InsulVolt = 600, CondForm = "3/c", OD = 0.1, Service = SVPower  },
                    new() { ID = "5", CondSize = "500kcmil", InsulVolt = 600, CondForm = "3/c", OD = 2.270, Service = SVPower  },
                    new() { ID = "6", CondSize = CZ1000, InsulVolt = 600, CondForm = CSingle, OD = 0.1, Service = SVPower  },

                    new() { ID = "GL01N4-0", CondSize = CZ4_0, InsulVolt = 600, CondForm = CSingle, OD = 0.619, Service = "Grounding"  },
                    new() { ID = "GL01N250", CondSize = CZ250, InsulVolt = 600, CondForm = CSingle, OD = 0.702, Service = "Grounding"  },

                    new() { ID = "PM03A002", CondSize = "2awg", InsulVolt = 5000, CondForm = "3/c", OD = 1.79, Service = SVPower  },
                    new() { ID = "PM03A2-0", CondSize = "2/0awg", InsulVolt = 5000, CondForm = "3/c", OD = 2.16, Service = SVPower  },
                    new() { ID = "9", CondSize = CZ250, InsulVolt = 5000, CondForm = "3/c", OD = 0.1, Service = SVPower  },
                    new() { ID = "PM03A500", CondSize = "500kcmil", InsulVolt = 5000, CondForm = "3/c", OD = 3.15, Service = SVPower  },
                    new() { ID = "11", CondSize = CZ1000, InsulVolt = 5000, CondForm = "3/c", OD = 0.1, Service = SVPower  },

                    new() { ID = "12", CondSize = CZ250, InsulVolt = 5000, CondForm = CSingle, OD = 0.1, Service = SVPower  },
                    new() { ID = "13", CondSize = "500kcmil", InsulVolt = 5000, CondForm = CSingle, OD = 0.1, Service = SVPower  },

                    new() { ID = "PD01N750", CondSize = "750kcmil", InsulVolt = 15000, CondForm = "1/c", OD = 1.665, Service = SVPower  },

                    new() { ID = "14", CondSize = "22awg", InsulVolt = 300, CondForm = "1pr", OD = 0.1, Service = "Instrument"  },
                    new() { ID = "IP16N016", CondSize = "16awg", InsulVolt = 600, CondForm = "multipair", OD = 1.23, Service = "Instrument"  },
                    new() { ID = "OT01N016", CondSize = "16awg", InsulVolt = 600, CondForm = "1pr", OD = 0.303, Service = "Instrument"  },
                    new() { ID = "IP01N018", CondSize = "18awg", InsulVolt = 600, CondForm = "1pr", OD = 0.3, Service = "Instrument"  },
                    new() { ID = "IP06N018", CondSize = "18awg", InsulVolt = 600, CondForm = "multipair", OD = 0.69, Service = "Instrument"  },
                    new() { ID = "IT06N018", CondSize = "18awg", InsulVolt = 600, CondForm = "multipair", OD = 0.82, Service = "Instrument"  },

                    new() { ID = "CL04N010", CondSize = "10awg", InsulVolt = 600, CondForm = "multicond", OD = 0.56, Service = "Control" },
                    new() { ID = "CL04N012", CondSize = "12awg", InsulVolt = 600, CondForm = "multicond", OD = 0.47, Service = "Control" },
                    new() { ID = "CL02N012", CondSize = "12awg", InsulVolt = 600, CondForm = "multicond", OD = 0.44, Service = "Control" },
                    new() { ID = "CL02N014", CondSize = "14awg", InsulVolt = 600, CondForm = "multicond", OD = 0.37, Service = "Control" },
                    new() { ID = "CL04N014", CondSize = "14awg", InsulVolt = 600, CondForm = "multicond", OD = 0.43, Service = "Control" },
                    new() { ID = "CL04N016", CondSize = "16awg", InsulVolt = 600, CondForm = "multicond", OD = 0.33, Service = "Control" },
                    new() { ID = "CL06N016", CondSize = "16awg", InsulVolt = 600, CondForm = "multicond", OD = 0.438, Service = "Control" },
                    new() { ID = "CL08N016", CondSize = "16awg", InsulVolt = 600, CondForm = "multicond", OD = 0.447, Service = "Control" },
                }.ToDictionary(cs => cs.ID);

            return _cables;
        }

        public static IDictionary<string, TraySpec> GetTraySpecs()
        {
            if (_trays == null)
                _trays = new List<TraySpec>
                {
                    new() { ID = "36", BottomType = LADDER, Width = 36, Height = 6 },
                    new() { ID = "30", BottomType = LADDER, Width = 30, Height = 6 },
                    new() { ID = "24", BottomType = LADDER, Width = 24, Height = 6 },
                    new() { ID = "12", BottomType = LADDER, Width = 12, Height = 6 },
                    new() { ID = "6", BottomType = LADDER, Width = 6, Height = 6 },
                }.ToDictionary(ts => ts.ID);

            return _trays;
        }

        // examples of cables in tray
        public static Tray GetCableInTray(string trayId) =>
            trayId switch {
                "T1" => new(_trays["36"], // 36in, ladder
                    new[] {
                    (_cables["PM03A002"], 11), // 2awg, 5000v, 3/c, power
                    (_cables["PM03A2-0"], 4), // 2/0awg, 5000v, 3/c, power
                    (_cables["PM03A500"], 5), // 500kcmil, 5000v, 3/c, power
                    (_cables["GL01N4-0"], 1) // 4/0awg, 600v, 1/c, grounding
                    }),

                "T2" => new(_trays["36"],
                    new[] {
                        (_cables["PD01N750"], 18), // 750kcmil, 15000v, 1/c, power
                        (_cables["GL01N250"], 2), // 250kcmil, 600v, 1/c, grounding
                        (_cables["IP16N016"], 1), // #16awg, 16 prs, 600v, 3/c, instrument
                    }),

                "T3" => new(_trays["30"],
                    new[] {
                        (_cables["CL02N012"], 2),
                        (_cables["CL02N014"], 3),
                        (_cables["CL04N010"], 4),
                        (_cables["CL04N012"], 4),
                        (_cables["CL04N014"], 2),
                        (_cables["CL04N016"], 3),
                        (_cables["CL06N016"], 1),
                        (_cables["CL08N016"], 2),
                        (_cables["IP01N018"], 1),
                        (_cables["IP06N018"], 1),
                        (_cables["IT06N018"], 5),
                        (_cables["OT01N016"], 2),
                    }),

                "T4" => new(_trays["12"], // 12in, ladder
                    new[] {
                        (_cables["PM03A002"], 11), // 2awg, 5000v, 3/c, power
                        (_cables["PM03A2-0"], 4), // 2/0awg, 5000v, 3/c, power
                        (_cables["PM03A500"], 5), // 500kcmil, 5000v, 3/c, power
                        (_cables["GL01N4-0"], 1) // 4/0awg, 600v, 1/c, grounding
                    }),

                _ => new(_trays["36"],
                    new[] {
                        (_cables["PD01N750"], 18), // 750kcmil, 15000v, 1/c, power
                        (_cables["GL01N250"], 2), // 250kcmil, 600v, 1/c, grounding
                        (_cables["IP16N016"], 1), // #16awg, 16 prs, 600v, 3/c, instrument
                    }),
            };

        #endregion

        #region For cable routing

        static IDictionary<string, Raceway> _raceways;

        static TraySegSystem _rwsegs;

        public static IDictionary<string, Raceway> GetRaceways()
        {
            if (_raceways == null)
                _raceways = new List<Raceway>()
                {
                    new() { ID = "R1", FromNode = new("N1"), ToNode = new("N2") },
                    new() { ID = "R2", FromNode = new("N2"), ToNode = new("N3") },

                    // parallel edges
                    new() { ID = "R3A", FromNode = new("N3"), ToNode = new("N4") },
                    new() { ID = "R3B", FromNode = new("N3"), ToNode = new("N4"), Weight = new(2.0) },

                    new() { ID = "R4", FromNode = new("N4"), ToNode = new("N5") },
                    new() { ID = "R5", FromNode = new("N5"), ToNode = new("N6") },

                    // parallel R6 and R7
                    new() { ID = "R6A", FromNode = new("N6"), ToNode = new("N7A") },
                    new() { ID = "R6A1", FromNode = new("N6"), ToNode = new("N7A"), Weight = new(2.0) },
                    new() { ID = "R7A", FromNode = new("N7A"), ToNode = new("N8") },
                    new() { ID = "R6B", FromNode = new("N6"), ToNode = new("N7B"), Weight = new(2.0) },
                    new() { ID = "R7B", FromNode = new("N7B"), ToNode = new("N8"), Weight = new(2.0) },

                    new() { ID = "R8", FromNode = new("N8"), ToNode = new("N9") },
                    new() { ID = "R9", FromNode = new("N9"), ToNode = new("N10") },

                    new() { ID = "J1", FromNode = new("N7A"), ToNode = new("N7B"), Type = RW.OTHER },

                    new() { ID = "D1", FromNode = new("E1"), ToNode = new("N1"), Type = RW.DROP },
                    new() { ID = "D2", FromNode = new("E2"), ToNode = new("N7A"), Type = RW.DROP },
                    new() { ID = "D3", FromNode = new("E3"), ToNode = new("N7B"), Type = RW.DROP },
                    new() { ID = "D4", FromNode = new("E4"), ToNode = new("N10"), Type = RW.DROP },

                }.ToDictionary(r => r.ID);

            return _raceways;
        }

        public static TraySegSystem GetTraySegs()
        {
            if (_rwsegs == null)
            {
                var lvSys = GetRaceways().Values.Where(rw => rw.IsTray()).Select(rw => (rw.ID, "LV"));
                _rwsegs = lvSys.Aggregate(new TraySegSystem(), (agg, i) => { agg.Add(i); return agg; });
            }

            return _rwsegs;
        }

        #endregion
    }
}