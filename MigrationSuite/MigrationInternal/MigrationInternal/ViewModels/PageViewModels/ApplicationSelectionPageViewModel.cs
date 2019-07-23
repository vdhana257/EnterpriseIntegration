using Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.ViewModels.PageViewModels
{
    class ApplicationSelectionPageViewModel : BaseViewModel, IPageViewModel
    {
        private ObservableCollection<ApplicationDetails> appDetails = new ObservableCollection<ApplicationDetails>();
        public ObservableCollection<ApplicationDetails> AppDetails
        {
            get { return appDetails; }
            set { appDetails = value; }
        }

        public string connectionString;
        public string ConnectionString
        {
            get
            {
                return connectionString;
            }
            set
            {
                connectionString = value;
                this.RaisePropertyChanged("ConnectionString");
            }
        }

        public string Title
        {
            get
            {
                return Resources.ApplicationSelectionPageTitle;
            }
        }

        public static DateTime ObjectToDateTime(object o, DateTime defaultValue)
        {
            if (o == null) return defaultValue;

            DateTime dt;
            if (DateTime.TryParse(o.ToString(), out dt))
                return dt;
            else
                return defaultValue;
        }

        public static bool StrToBool(string str)
        {
            if (str == "True")
                return true;
            else return false;
        }

        public override void Initialize(IApplicationContext applicationContext)
        {
            base.Initialize(applicationContext);
            ConnectionString = this.ApplicationContext.GetProperty("DatabaseConnectionString") as string;
            SqlCommand AppCommand = new SqlCommand();
            SqlConnection conn = new SqlConnection(ConnectionString);
            conn.Open();
            AppCommand.Connection = conn;
            AppCommand.CommandText = "select * from bts_application";
            try
            {
                SqlDataReader myReader = null;
                myReader = AppCommand.ExecuteReader();
                while (myReader.Read())
                {
                    ApplicationDetails temp = new ApplicationDetails();
                    temp.nID = myReader["nID"].ToString();
                    temp.nvcName = myReader["nvcName"].ToString();
                    temp.nvcDescription = myReader["nvcDescription"].ToString();
                    temp.isSystem = StrToBool(myReader["isSystem"].ToString());
                    temp.isDefault = StrToBool(myReader["isDefault"].ToString());
                    temp.DateModified = ObjectToDateTime(myReader["DateModified"], DateTime.Now);
                    appDetails.Add(temp);
                }
                conn.Close();
                applicationContext.SetProperty("SelectedApps", appDetails);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}
