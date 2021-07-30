using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LumMVC_WebAPI.Models
{
    public class NonChopClass
    {
        public string Wafer_Id { get; set; }
        public string RW_Wafer_Id { get; set; }
        public string EPI_Type { get; set; }
        public string Is_Valid { get; set; }
        public string Is_Complete { get; set; }
        public DateTime Updated_Date { get; set; }
        public string Remap_Required { get; set; }
    }
}