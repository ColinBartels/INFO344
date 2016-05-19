using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace SQLAzure
{
    /// <summary>
    /// Summary description for SQLAzure
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class SQLAzure : System.Web.Services.WebService
    {

        [WebMethod]
        public List<string> GetAllProducts()
        {
            List<string> output = new List<string>();
            using (SqlConnection conn = new SqlConnection(
                "Server=tcp:sqlcolin.database.windows.net,1433;Database=SQLTest;User ID=colin@sqlcolin;Password=feather1!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"))
            {
                SqlCommand command = new SqlCommand("SELECT * FROM eBayProducts;", conn);
                command.Connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        output.Add(reader["name"] + ": " + reader["price"]);
                    }
                }
            }
            return output;
        }
    }
}
