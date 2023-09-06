using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDropLib
{
    public static class TestData
    {
        public static ImmutableArray<Cable> GetCablesLV() =>
            ImmutableArray<Cable>.Empty.AddRange(
                new Cable[]
                {
                    new("12awg", new(2.0 / 1000.0, 0.054 / 1000.0), new(25.0), 1),
                    new("10awg", new(1.2 / 1000.0, 0.05 / 1000.0), new(35.0), 1),
                    new("8awg", new(0.78 / 1000.0, 0.052 / 1000.0), new(50.0), 1),
                    new("6awg", new(0.49 / 1000.0, 0.051 / 1000.0), new(65.0), 1),
                    new("4awg", new(0.31 / 1000.0, 0.048 / 1000.0), new(85.0), 1),
                    new("2awg", new(0.2 / 1000.0, 0.045 / 1000.0), new(115.0), 1),
                    new("1awg", new(0.16 / 1000.0, 0.046 / 1000.0), new(130.0), 1),

                    new("1/0awg", new(0.13 / 1000.0, 0.044 / 1000.0), new(150.0), 1),
                    new("2/0awg", new(0.1 / 1000.0, 0.043 / 1000.0), new(175.0), 1),
                    new("4/0awg", new(0.067 / 1000.0, 0.041 / 1000.0), new(230.0), 1),

                    new("250kcmil", new(0.057 / 1000.0, 0.041 / 1000.0), new(255.0), 1),
                    new("350kcmil", new(0.043 / 1000.0, 0.040 / 1000.0), new(310.0), 1),
                    new("500kcmil", new(0.032 / 1000.0, 0.039 / 1000.0), new(380.0), 1),

                    new("2x4/0awg", new(0.067 / 1000.0, 0.041 / 1000.0), new(230.0), 2),
                    new("2x350kcmil", new(0.043 / 1000.0, 0.040 / 1000.0), new(310.0), 2),
                    new("2x500kcmil", new(0.032 / 1000.0, 0.039 / 1000.0), new(380.0), 2),
                    new("3x500kcmil", new(0.032 / 1000.0, 0.039 / 1000.0), new(380.0), 3),
                });


        public static ImmutableArray<MotorLoad> GetMotorLV() =>
            ImmutableArray<MotorLoad>.Empty.AddRange(
                new MotorLoad[]
                {
                    new("1hp", new(2.1,new(0.54)), new(15.0, new(0.15))),
                    new("1.5hp", new(3.0,new(0.557)), new(20.0, new(0.15))),
                    new("2hp", new(3.4,new(0.656)), new(25.0, new(0.15))),
                    new("3hp", new(4.8,new(0.669)), new(32.0, new(0.15))),
                    new("5hp", new(7.6,new(0.704)), new(46.0, new(0.15))),
                    new("7.5hp", new(11.0,new(0.713)), new(64.0, new(0.15))),
                    new("10hp", new(14.0,new(0.747)), new(81.0, new(0.15))),
                    new("15hp", new(21.0,new(0.729)), new(116.0, new(0.15))),
                    new("20hp", new(27.0,new(0.756)), new(145.0, new(0.15))),
                    new("25hp", new(34.0,new(0.74)), new(183.0, new(0.15))),
                    new("30hp", new(40.0,new(0.75)), new(218.0, new(0.15))),
                    new("40hp", new(52.0,new(0.769)), new(290.0, new(0.15))),
                    new("50hp", new(65.0,new(0.765)), new(363.0, new(0.15))),
                    new("60hp", new(77.0,new(0.772)), new(435.0, new(0.15))),
                    new("75hp", new(96.0,new(0.774)), new(543.0, new(0.15))),
                    new("100hp", new(124.0,new(0.795)), new(725.0, new(0.15))),
                    new("125hp", new(156.0,new(0.785)), new(908.0, new(0.15))),
                    new("150hp", new(180.0,new(0.818)), new(1085.0, new(0.15))),
                    new("200hp", new(240.0,new(0.809)), new(1450.0, new(0.15))),
                    new("250hp", new(302.0,new(0.825)), new(1825.0, new(0.15))),
                });

    }
}
