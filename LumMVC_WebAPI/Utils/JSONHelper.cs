using LumMVC_WebAPI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace LumMVC_WebAPI.Utils
{
    public class JSONHelper
    {
        public string TransferHiveFormat(List<TBL_WAFER_RESUME_ITEM> list)
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);
            writer.WriteStartObject();
            foreach (var item in list)
            {
                writer.WritePropertyName(item.Key);
                writer.WriteValue(item.Value);
            }
            writer.WriteEndObject();

            return sw.ToString();
        }

        public string JsonTextBuilder(Dictionary<string, string> resultDic)
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);
            writer.WriteStartObject();
            foreach (var kvp in resultDic)
            {
                writer.WritePropertyName(kvp.Key);
                writer.WriteValue(kvp.Value);
            }
            writer.WriteEndObject();
            return sw.ToString();
        }
    }
}