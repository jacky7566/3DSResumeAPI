//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LumMVC_WebAPI.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class TBL_SHIPLOGIC_CONFIG
    {
        public System.Guid Id { get; set; }
        public string GroupName { get; set; }
        public string SLCType { get; set; }
        public string SLCKey { get; set; }
        public Nullable<int> IsDisplay { get; set; }
        public Nullable<int> IsCompare { get; set; }
        public string CmpType { get; set; }
        public string CmpCondtion { get; set; }
        public string CmpKey { get; set; }
        public Nullable<int> Status { get; set; }
        public Nullable<System.DateTime> Creation_Date { get; set; }
        public string Attribute1 { get; set; }
        public string Attribute2 { get; set; }
        public string Created_By { get; set; }
        public string CmpParamType { get; set; }
        public string DisplayName { get; set; }
        public string ErrorGroup { get; set; }
        public string ErrorSeq { get; set; }
    }
}