using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;


namespace ClassLibrary
{
    public class Sum : TableEntity
    {
        public int sum;

        public Sum(int sum)
        {
            this.sum = sum;
            this.PartitionKey = sum.ToString();
            this.RowKey = Guid.NewGuid().ToString();
        }

        public Sum()
        {

        }
    }
}
