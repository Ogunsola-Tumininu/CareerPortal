//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RecruitmentManagement.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class EducationDetail
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string University { get; set; }
        public string Location { get; set; }
        public string Course { get; set; }
        public Nullable<int> YearOfGraduation { get; set; }
        public string Grade { get; set; }
        public string Qualification { get; set; }
        public Nullable<decimal> CGPA { get; set; }
    }
}