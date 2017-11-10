using System.Collections.Generic;
using Izenda.BI.Framework.Models;

namespace IzendaCustomBootstrapper.Models
{
    public class ReportList
    {
        public List<Category> Data { get; set; }

        public string HashCode { get; set; }

        public string TotalItems { get; set;}

        public int NumOfChilds { get; set; }

        public int NumOfCheckedChilds { get; set; }

        public bool Indeterminate { get; set; }

        public bool IsLastPage { get; set; }
    }
}