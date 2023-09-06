using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NecFillLib.Nec2011
{
    /// <summary>
    /// Tables from the NEC code to be
    /// used in the tray fill calculation
    /// </summary>
    public class NecTable
    {
        public static double Lookup(string columnName, double trayWidth)
        {
            switch (columnName)
            {
                case "TC1":
                    return TC1(trayWidth);
                case "TC3":
                    return TC3(trayWidth);
                case "T2C1":
                    return T2C1(trayWidth);
                case "T2C2":
                    return T2C2(trayWidth);
                case "T3C1":
                    return T3C1(trayWidth);
                case "T3C2":
                    return T3C2(trayWidth);
                case "T4C1":
                    return T4C1(trayWidth);
                default:
                    return 0.1;
            }
        }

        // Table 392.22(A) columns 1 and 2
        // 392.22(A)(1)(c)
        static double TC1(double tw)
        {
            double res = 0;
            if (tw >= 36) res = 42;
            else if (tw >= 30) res = 35;
            else if (tw >= 24) res = 28;
            else if (tw >= 20) res = 23.5;
            else if (tw >= 18) res = 21;
            else if (tw >= 16) res = 18.5;
            else if (tw >= 12) res = 14;
            else if (tw >= 9) res = 10.5;
            else if (tw >= 8) res = 9.5;
            else if (tw >= 6) res = 7;
            else if (tw >= 4) res = 4.5;
            else if (tw >= 2) res = 2.5;
            else res = 0.1;
            return res;
        }

        // Table 392.22(A) columns 3 and 4
        // 392.22(A)(3)(c)
        static double TC3(double tw)
        {
            double res = 0;
            if (tw >= 36) res = 33;
            else if (tw >= 30) res = 27.5;
            else if (tw >= 24) res = 22;
            else if (tw >= 20) res = 18.5;
            else if (tw >= 18) res = 16.5;
            else if (tw >= 16) res = 14.5;
            else if (tw >= 12) res = 11;
            else if (tw >= 9) res = 8;
            else if (tw >= 8) res = 7;
            else if (tw >= 6) res = 5.5;
            else if (tw >= 4) res = 3.5;
            else if (tw >= 2) res = 2;
            else res = 0.1;
            return res;
        }

        // Table 392.22(A)(5) Column 1
        // 392.22(A)(5)(a)
        static double T2C1(double tw)
        {
            double res = 0;
            if (tw >= 6) res = 7;
            else if (tw >= 4) res = 4.5;
            else if (tw >= 3) res = 2.3;
            else res = 0.1;
            return res;
        }

        // Table 392.22(A)(5) Column 2
        // 392.22(A)(5)(b)
        static double T2C2(double tw)
        {
            double res = 0;
            if (tw >= 6) res = 3.8;
            else if (tw >= 4) res = 2.5;
            else if (tw >= 3) res = 1.3;
            else res = 0.1;
            return res;
        }

        // Table 392.22(A)(6) Column 1
        // 392.22(A)(6)(a)
        static double T3C1(double tw)
        {
            double res = 0;
            if (tw >= 6) res = 5.5;
            else if (tw >= 4) res = 3.7;
            else if (tw >= 3) res = 2;
            else if (tw >= 2) res = 1.3;
            else res = 0.1;
            return res;
        }

        // Table 392.22(A)(6) Column 2
        // 392.22(A)(6)(b)
        static double T3C2(double tw)
        {
            double res = 0;
            if (tw >= 6) res = 3.2;
            else if (tw >= 4) res = 2.1;
            else if (tw >= 3) res = 1.1;
            else if (tw >= 2) res = 0.8;
            else res = 0.1;
            return res;
        }

        // Table 392.22(B)(1) columns 1 and 2
        // 392.22(B)(1)(b), 392.22(B)(1)(c)
        static double T4C1(double tw)
        {
            double res = 0;
            if (tw >= 36) res = 39;
            else if (tw >= 30) res = 32.5;
            else if (tw >= 24) res = 26;
            else if (tw >= 20) res = 21.5;
            else if (tw >= 18) res = 19.5;
            else if (tw >= 16) res = 17.5;
            else if (tw >= 12) res = 13;
            else if (tw >= 9) res = 9.5;
            else if (tw >= 8) res = 8.5;
            else if (tw >= 6) res = 6.5;
            else if (tw >= 4) res = 4.5;
            else if (tw >= 2) res = 2;
            else res = 0.1;
            return res;
        }

    }
}
