using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace ClassLibrary
{
    public class SystemStats : TableEntity
    {
        public SystemStats(string memory, string cpu)
        {
            this.PartitionKey = "stats";
            this.RowKey = "stats";
            this.memory = memory;
            this.cpu = cpu;
        }

        public SystemStats() { }

        public string memory { get; set; }
        public string cpu { get; set; }
    }
}