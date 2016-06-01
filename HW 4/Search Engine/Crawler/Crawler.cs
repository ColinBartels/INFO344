using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WorkerRole1
{
    class Crawler
    {
        private static DateTime compareDate = new DateTime(2016, 3, 1);
        private string lastPageTitle;

        public Crawler() { }

        public string getLastPageTitle()
        {
            return lastPageTitle;
        }

        public List<string> parseXMLSitemapIndex(XmlDocument doc)
        {
            lastPageTitle = "SitemapIndex";
            List<string> results = new List<string>();

            XmlNodeList sitemaps = doc.GetElementsByTagName("sitemap");
            foreach (XmlNode sitemap in sitemaps)
            {
                XmlNodeList children = sitemap.ChildNodes;
                string loc = children[0].InnerText;
                string lastmod = children[1].InnerText;
                lastmod = lastmod.Substring(0, 10);
                DateTime siteDate = DateTime.Parse(lastmod);

                if (DateTime.Compare(siteDate, compareDate) > 0)
                {
                    results.Add(loc);
                }
            }
            return results;
        }

        public List<string> parseXMLUrlset(XmlDocument doc)
        {
            lastPageTitle = "Urlset";
            List<string> results = new List<string>();

            XmlNodeList urls = doc.GetElementsByTagName("url");
            foreach (XmlNode url in urls)
            {
                XmlNode loc = url["loc"];
                XmlNode lastmod = url["lastmod"];
                XmlNode news = url["news:news"];
                XmlNode video = url["video:video"];

                //Finds where date is stored
                if (lastmod != null && news == null && video == null)
                {
                    DateTime siteDate = DateTime.Parse(lastmod.InnerText.Substring(0, 10));
                    if (DateTime.Compare(siteDate, compareDate) > 0)
                    {
                        results.Add(loc.InnerText);
                    }
                }
                else if (lastmod == null && news != null && video == null)
                {
                    DateTime siteDate = DateTime.Parse(news["news:publication_date"].InnerText.Substring(0, 10));
                    if (DateTime.Compare(siteDate, compareDate) > 0)
                    {
                        results.Add(loc.InnerText);
                    }
                }
                else if (lastmod == null && news == null && video != null)
                {
                    DateTime siteDate = DateTime.Parse(video["video:publication_date"].InnerText.Substring(0, 10));
                    if (DateTime.Compare(siteDate, compareDate) > 0)
                    {
                        results.Add(loc.InnerText);
                    }
                }
                else
                {
                    results.Add(loc.InnerText);
                }
            }
            return results;
        }

        public List<string> parseHTML(HtmlDocument doc)
        {
            List<string> results = new List<string>();

            if (doc.DocumentNode != null)
            {
                HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//head//title");
                if (titleNode != null)
                {
                    lastPageTitle = titleNode.InnerHtml;
                }
                else
                {
                    lastPageTitle = "Title not found";
                }
                var links = doc.DocumentNode.SelectNodes("//a[@href]");
                if (links != null)
                {
                    foreach (HtmlNode link in links)
                    {
                        results.Add(link.Attributes["href"].Value);
                    }
                }
            }
            return results;
        }
    }
}
