using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using SystemLibrary.Utility;

namespace LumMVC_WebAPI.Utils
{
    public static class IOHelper
    {
        public static HttpResponseMessage LogAndResponse(HttpContent hc, HttpStatusCode hsc = HttpStatusCode.OK, 
            string message = "", Exception ex = null)
        {
            var resp = new HttpResponseMessage(hsc);
            if (string.IsNullOrEmpty(message) == false)
            {
                LogHelper.WriteLine(message);
                resp.Content = new StringContent(message);
            }
            else
            {
                resp.Content = hc;
            }

            return resp;
        }

        public static DateTime ConvertToDate(string date)
        {
            DateTime parseDate = DateTime.ParseExact(date, "yyyy/MM/dd", null);
            return parseDate;
        }
    }
}