//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MappingProject.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class AspNetManager_Drivers
    {
        public int Id { get; set; }
        public string DriverID { get; set; }
        public string ManagerID { get; set; }
        public Nullable<bool> IsDriverActive { get; set; }
    
        public virtual AspNetUser AspNetUser { get; set; }
        public virtual AspNetUser AspNetUser1 { get; set; }
    }
}
