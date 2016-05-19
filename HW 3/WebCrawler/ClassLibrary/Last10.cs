using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace ClassLibrary
{
    public class Last10 : TableEntity
    {
        public Last10(string urls)
        {
            this.PartitionKey = "last10";
            this.RowKey = "last10";
            this.urls = urls;
        }

        public Last10() { }

        public string urls { get; set; }

        public void updateUrls(string url)
        {
            if (urls != null)
            {
                var list = urls.Split('|')
                    .ToList();
                list.Insert(0, url);
                if (list.Count() > 10)
                {
                    list.RemoveAt(list.Count() - 1);
                }
                urls = String.Join("|", list);
            }
            else
            {
                urls = url;
            }
        }
    }
}
