using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace ClassLibrary
{
    public class Size : TableEntity
    {
        public Size(int size, string type)
        {
            this.PartitionKey = "size";
            this.RowKey = type;
            this.size = size;
        }

        public Size() { }

        public int size { get; set; }
    }
}
