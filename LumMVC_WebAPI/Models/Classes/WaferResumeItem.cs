using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LumMVC_WebAPI.Models.Classes
{
    public class WaferResumeItem
    {
        //public System.Guid Id { get; set; }
        //public System.Guid Header_Id { get; set; }
        public string Type { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Good_Die_Qty { get; set; }
        //public int Status { get; set; }
        //public System.DateTime Creation_Date { get; set; }
        //public System.DateTime Last_Updated_Date { get; set; }
        //public string Attribute1 { get; set; }
        public string Box_Id { get; set; }
        public string Wafer_Id { get; set; }
        public string RW_Wafer_Id { get; set; }
        public Dictionary<string, string> Details { get; set; }
        public string RW_Prefix { get; set; }
    }
}