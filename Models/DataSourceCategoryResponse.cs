using Izenda.BI.Framework.Models.ReportDesigner;
using System;
using System.Collections.Generic;

namespace Models
{
    public class DataSourceCategoryResponse
    {
        public List<ReportDataSourceCategory> Data { get; set; }
        public int TotalItems { get; set; }
        public int NumOfChildes { get; set; }
        public int NumOfCheckedChildes { get; set; }
        public Boolean Indeterminate { get; set; }
        public Boolean IsLatePage { get; set; }

    }
}