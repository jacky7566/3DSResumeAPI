using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LumMVC_WebAPI.Models.Classes
{
    public class WaferResumeResult
    {
        //public System.Guid Id { get; set; }
        //public string Level { get; set; }
        //public string Type { get; set; }
        public string Box_Id { get; set; }
        public string Wafer_Id { get; set; }
        public string RW_Wafer_Id { get; set; }
        public string Good_Die_Qty { get; set; }
        //public Nullable<int> Status { get; set; }
        public System.DateTime Creation_Date { get; set; }
        public string Shipable { get; set; }
        //public string Attribute2 { get; set; }
        //public string Created_By { get; set; }
        //public string LastUpdated_By { get; set; }
        public System.DateTime Last_Updated_Date { get; set; }
        //public Nullable<System.DateTime> Sys_Updated_Date { get; set; }
        //public List<WaferResumeItem> list { get; set; }
        public Dictionary<string, string> Details { get; set; }
        public Dictionary<string, string> ErrorReason { get; set; }
    }
}