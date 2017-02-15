using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace ClassLibrary
{
    public class Error : TableEntity
    {
        public Error(string error, string url)
        {
            this.PartitionKey = "error";
            this.RowKey = Guid.NewGuid().ToString();
            this.url = url;
            this.error = error;
        }

        public Error() { }

        public string url { get; set; }
        public string error { get; set; }
    }
}
