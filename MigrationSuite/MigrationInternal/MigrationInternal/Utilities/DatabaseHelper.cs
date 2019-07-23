using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    public class DatabaseHelper
    {
        public static Dictionary<string, string> FindAgreementsToBeConsolidated(string sqlConnectionString, List<string> guestPartnerTpmIds)
        {
            Dictionary<string, string> originalAndTpmIdMapping = new Dictionary<string, string>();

            using (SqlConnection cn = new SqlConnection(sqlConnectionString))
            {
                try
                {
                    var query = ConfigurationManager.AppSettings["PartnerQuery"];
                    query = string.Format(query, string.Join(",", guestPartnerTpmIds));
                    using (var cmd = new SqlCommand(query, cn))
                    {
                        if (cn.State == System.Data.ConnectionState.Closed)
                        {
                            try { cn.Open(); }
                            catch (Exception e)
                            {
                                string message = $"ERROR! Unable to establish connection to the ebisDB database. \nErrorMessage:{e.Message}";
                                TraceProvider.WriteLine($"{message} \nStackTrace:{e.StackTrace}");
                                throw new Exception(message);
                            }
                        }
                        using (var rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                if (!originalAndTpmIdMapping.ContainsKey(rdr["TPMPartnerID"].ToString()))
                                {
                                    originalAndTpmIdMapping.Add(rdr["TPMPartnerID"].ToString(), rdr["PartnerID"].ToString());
                                }
                            }
                        }
                        return originalAndTpmIdMapping;
                    }
                }
                catch(Exception ex)
                {
                    throw ex;
                }

            }
        }
    }
}
