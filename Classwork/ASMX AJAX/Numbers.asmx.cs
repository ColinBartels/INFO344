using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Script.Services;
using System.Web.Script.Serialization;

namespace WebApplication2
{
    /// <summary>
    /// Summary description for Numbers
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
    [System.Web.Script.Services.ScriptService]
    public class Numbers : System.Web.Services.WebService
    {
        public static List<int> numbers = new List<int>();


        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetNumbersThatStartsWith(int input)
        {
            if (numbers.Count() == 0)
            {
                Random gen = new Random();
                for (int i = 0; i < 10000; i++)
                {
                    numbers.Add(gen.Next(int.MaxValue));
                }
            }
            List<int> output = new List<int>();
            foreach (int number in numbers)
            {
                string num = number.ToString();
                if (num.StartsWith(input.ToString()))
                {
                    output.Add(number);
                }
                if (output.Count() == 10)
                {
                    return new JavaScriptSerializer().Serialize(output);
                }
            }
            return new JavaScriptSerializer().Serialize(output);
        }
    }
}
