using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReverseWords
{
    public class Phrase : TableEntity
    {
        public Phrase(string normal, string reverse)
        {
            this.PartitionKey = normal;
            this.RowKey = reverse;
        }

        public Phrase() { }

        public string normal { get; set; }

        public string reverse { get; set; }
    }
}