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
    
    public partial class TBL_PRODUCT_INFO
    {
        public System.Guid Id { get; set; }
        public string ProductName { get; set; }
        public string MaskId { get; set; }
        public string MP_RW_Wafer_Id { get; set; }
        public string NPI_RW_Wafer_Id { get; set; }
        public Nullable<int> Status { get; set; }
        public System.DateTime Creation_Date { get; set; }
        public string Attribute1 { get; set; }
        public string Attribute2 { get; set; }
        public string Created_By { get; set; }
    }
}
