using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Web;
using SystemLibrary.Utility;

namespace LumMVC_WebAPI.Utils
{
    public class Operation<T> where T : IComparable
    {
        /// <summary>
        /// Compare function
        /// </summary>
        /// <param name="op">Compare key</param>
        /// <param name="x">Check Item</param>
        /// <param name="y">Lookup Item</param>
        /// <returns></returns>
        public static bool Compare(string op, T x, T y) 
        {
            try
            {
                if (x == null || y == null) return false;
                switch (op)
                {
                    case "==": return x.CompareTo(y) == 0;
                    case "!=": return x.CompareTo(y) != 0;
                    case ">": return x.CompareTo(y) > 0;
                    case ">=": return x.CompareTo(y) >= 0;
                    case "<": return x.CompareTo(y) < 0;
                    case "<=": return x.CompareTo(y) <= 0;
                    case "%": return y.ToString().Contains(x.ToString()) ? false : true;
                    case "=?": return x.CompareTo(y) == 0;
                    //"ABC DEF [SYL abnormal! Yield rate:] TTTEST", "[SYL abnormal! Yield rate:]"
                    default: return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}