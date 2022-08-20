using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskFunction
{
    public class ApiCallLog
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTime Timetamp { get; set; }
        public string Description { get; set; }
    }
}
