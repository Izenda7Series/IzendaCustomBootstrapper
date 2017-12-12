using System.Collections.Generic;
using Izenda.BI.Framework.Models;

namespace IzendaCustomBootstrapper.Models
{
    public class ReportListResponse
    {
        public List<Category> Data { get; set; }

        public string Hashcode { get; set; }

        public int? NumOfChilds { get; set; }

        public int? NumOfCheckedChilds { get; set; }

        public bool? Indeterminate { get; set; }

        public int? TotalItems { get; set; }

        public bool? IsLastPage { get; set; }
    }
}
