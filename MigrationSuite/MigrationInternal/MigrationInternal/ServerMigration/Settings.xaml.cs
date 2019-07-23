using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace MigrationTool
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {

        #region Variables
        string appPath, configFile;//changed
        private BizTalkAdminOperations biztalkAdminOperations;
        //
        #endregion

        #region Constructors
        public Settings()
        {
            Loaded += Settings_Load;
            InitializeComponent();

            //    Settings_Load();

        }
        //changed
        public Settings(BizTalkAdminOperations BiztalkAdminOperations)
        {
            Loaded += Settings_Load;
            InitializeComponent();
            //    Settings_Load();
            biztalkAdminOperations = BiztalkAdminOperations;

        }
        #endregion

        #region Events
        private void Settings_Load(object sender, EventArgs e)
        {

            appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            configFile = System.IO.Path.Combine(appPath, "MigrationSuite.exe.config");
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
            configFileMap.ExeConfigFilename = configFile;
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            // MessageBox.Show(config.AppSettings.Settings["CertPass"].Value);
            txtAppToRefer.Text = config.AppSettings.Settings["AppToRefer"].Value;
            txtBiztalkAppToIgnore.Text = config.AppSettings.Settings["BizTalkAppToIgnore"].Value;
            txtWindowsServiceToIgnore.Text = config.AppSettings.Settings["WindowsServiceToIgnore"].Value;
            txtFoldersToCopyNoFiles.Text = config.AppSettings.Settings["FoldersToCopyNoFiles"].Value;
            txtFoldersToCopy.Text = config.AppSettings.Settings["FoldersToCopy"].Value;
            txtCustomDllToInclude.Text = config.AppSettings.Settings["CustomDllToInclude"].Value;
            txtTemporaryFolder.Text = config.AppSettings.Settings["RemoteRootFolder"].Value;
            txtCertPass.Text = config.AppSettings.Settings["CertPass"].Value;
            txtWebSitesDrive.Text = config.AppSettings.Settings["WebSitesDriveDestination"].Value;
            txtFoldersDrive.Text = config.AppSettings.Settings["FoldersDriveDestination"].Value;
            txtServicesDrive.Text = config.AppSettings.Settings["ServicesDriveDestination"].Value;

        }


        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            try
            {

                //biztalkAdminOperations.LogInfoInLogFile("Settings:Update Started");

                appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                configFile = System.IO.Path.Combine(appPath, "MigrationSuite.exe.config");
                ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
                configFileMap.ExeConfigFilename = configFile;
                Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
                config.AppSettings.Settings["AppToRefer"].Value = txtAppToRefer.Text;
                config.AppSettings.Settings["RemoteRootFolder"].Value = txtTemporaryFolder.Text;
                // config.AppSettings.Settings["BamExePath"].Value = Environment.GetEnvironmentVariable("BTSINSTALLPATH") + @"Tracking\bm.exe";
                config.AppSettings.Settings["CertPass"].Value = txtCertPass.Text;
                config.AppSettings.Settings["FoldersToCopyNoFiles"].Value = txtFoldersToCopyNoFiles.Text;
                config.AppSettings.Settings["FoldersToCopy"].Value = txtFoldersToCopy.Text;
                config.AppSettings.Settings["BizTalkAppToIgnore"].Value = txtBiztalkAppToIgnore.Text;
                config.AppSettings.Settings["CustomDllToInclude"].Value = txtCustomDllToInclude.Text;
                config.AppSettings.Settings["WindowsServiceToIgnore"].Value = txtWindowsServiceToIgnore.Text;
                config.AppSettings.Settings["WebSitesDriveDestination"].Value = txtWebSitesDrive.Text;
                config.AppSettings.Settings["FoldersDriveDestination"].Value = txtFoldersDrive.Text;
                config.AppSettings.Settings["ServicesDriveDestination"].Value = txtServicesDrive.Text;
                config.Save();
                biztalkAdminOperations.UpdateSettings();
                //added
                BizTalkAdminOperations goBack = new BizTalkAdminOperations();
                loopback.Children.Clear();
                loopback.Children.Add(goBack);
                //this.close();



            }
            catch (Exception ex)
            {
                //changed
                BizTalkAdminOperations biztalkAdminOperations = new BizTalkAdminOperations();
                biztalkAdminOperations.LogInfoInLogFile("Error while Updating Settings to ConfigFile " + ex.Message + ", " + ex.StackTrace);
                //added
                BizTalkAdminOperations goBack = new BizTalkAdminOperations();
                loopback.Children.Clear();
                loopback.Children.Add(goBack);
                // this.Close();
            }
        }
        #endregion
        private void btnBack_Click(object sender, EventArgs e)
        {
            BizTalkAdminOperations goBack = new BizTalkAdminOperations();
            loopback.Children.Clear();
            loopback.Children.Add(goBack);
        }
    }
}



