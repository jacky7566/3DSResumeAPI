﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class MadridTestEntities : DbContext
    {
        public MadridTestEntities()
            : base("name=MadridTestEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<tbl_bin_code_file_status> tbl_bin_code_file_status { get; set; }
        public virtual DbSet<tbl_serial_wat> tbl_serial_wat { get; set; }
        public virtual DbSet<tbl_serial_wat_history> tbl_serial_wat_history { get; set; }
    }
}
