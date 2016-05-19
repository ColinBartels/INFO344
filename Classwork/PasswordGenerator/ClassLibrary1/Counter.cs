using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;


namespace ClassLibrary1
{
    public class Counter : TableEntity
    {
        public Counter(string count)
        {
            this.PartitionKey = "counter";
            this.RowKey = "counter";
        }

        public Counter() { }

        public int count { get; set; }
    }
}
