using MigrationTool;
using System;
using System.Collections.Generic;
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

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.LogicConfiguration
{
    /// <summary>
    /// Interaction logic for VersionSelection.xaml
    /// </summary>

    public partial class VersionSelection : UserControl
    {
        private string accessToken;

        private string subscriptionId;

        private string resourceGroup;

        private string workflow;

        List<WorkflowVersion> userWorkflowVersions = new List<WorkflowVersion>();

        UriHelper helper = new UriHelper();

        LogicAppsContext laContext = new LogicAppsContext();

        public string SubscriptionId
        {
            get
            {
                return this.subscriptionId;
            }
            set
            {
                this.subscriptionId = value;
            }
        }
        public string ResourceGroup
        {
            get
            {
                return this.resourceGroup;
            }
            set
            {
                this.resourceGroup = value;
            }
        }
        public string Workflow
        {
            get
            {
                return this.workflow;
            }
            set
            {
                this.workflow = value;
            }
        }
        public VersionSelection(LogicAppDetails userDetails, string Token)
        {
            Workflow = userDetails.Workflow;
            ResourceGroup = userDetails.ResourceGroup;
            SubscriptionId = userDetails.SubscriptionId;
            accessToken = Token;
            InitializeComponent();
            aux();


        }

        private void HomePage_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you want to close the tool and navigate to Home Page?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                HomePage hp = new HomePage();
                main.Children.Clear();
                main.Children.Add(hp);
            }
        }
        public void aux()
        {
            string uri = helper.getWorkflowVersionsUri(subscriptionId, resourceGroup, workflow);
            userWorkflowVersions = laContext.GetWorkflowVersions(uri, accessToken);
            Version1_ComboBox.ItemsSource = userWorkflowVersions;
            Version2_ComboBox.ItemsSource = userWorkflowVersions;
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (Version1_ComboBox.SelectedIndex > -1 && Version2_ComboBox.SelectedIndex > -1)
            {
                CompareResult cr = new CompareResult(userWorkflowVersions[Version1_ComboBox.SelectedIndex], userWorkflowVersions[Version2_ComboBox.SelectedIndex]);
                main.Children.Clear();
                main.Children.Add(cr);

            }
            else
                MessageBox.Show("Select Versions to compare.");
        }


    }
}
