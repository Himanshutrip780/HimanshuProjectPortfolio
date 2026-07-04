using System;
using System.Collections.Generic;
using ATS.Domain.Common;

namespace ATS.Domain.Entities
{
    public class Department : BaseEntity, IMultiTenant
    {
        public string Name { get; set; }
        public Guid CompanyId { get; set; }
        public Company Company { get; set; }
        public Guid? ParentId { get; set; }
        public Department Parent { get; set; }

        public ICollection<Department> SubDepartments { get; set; } = new List<Department>();
        public ICollection<Job> Jobs { get; set; } = new List<Job>();
    }
}
