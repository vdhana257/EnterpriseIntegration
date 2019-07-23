using MigrationTool;
using System;
using System.Collections.Generic;
using System.Configuration;
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
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : UserControl
    {
        private string accessToken { get; set; }

        List<Subscriptions> UserSubscriptions = new List<Subscriptions>();
        List<Workflows> UserWorkflows = new List<Workflows>();
        List<ResourceGroups> UserResourceGroups = new List<ResourceGroups>();
        LogicAppsContext laContext = new LogicAppsContext();
        LogicAppDetails userDetails = new LogicAppDetails();
        UriHelper helper = new UriHelper();

        public LoginPage()
        {
            
            InitializeComponent();
           
        }

        private void HomePage_Click(object sender, RoutedEventArgs e)
        {
            //MessageBoxResult result = MessageBox.Show("Do you want to close the tool and navigate to Home Page?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question);
            //if (result == MessageBoxResult.Yes)
            //{
                HomePage hp = new HomePage();
                main.Children.Clear();
                main.Children.Add(hp);
            //}
        }
        private void Loginbtn_Click(object sender, RoutedEventArgs e)
        {
            
            var authresult = laContext.GetAccessTokenFromAAD();
            

            if (authresult != null)
            {
                string token = authresult.AccessToken;        
                accessToken = token;
                UserSubscriptions = laContext.GetSubscription(helper.getSubscriptionUri(), token); 

                SubscriptionComboBox.ItemsSource = UserSubscriptions;
                SubscriptionDetails.Visibility = Visibility.Visible;
            }


        }

        private void SubscriptionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
            string subscriptionId = UserSubscriptions[SubscriptionComboBox.SelectedIndex].subscriptionId;
            userDetails.SubscriptionId = subscriptionId;
            string uri = helper.getResourceGroupUri(subscriptionId);
            UserResourceGroups = laContext.GetResourceGroups(uri,accessToken);
            ResourceComboBox.ItemsSource = UserResourceGroups;
            ResourceDetails.Visibility = Visibility.Visible;
        }

        private void ResourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string resourceGroup = UserResourceGroups[ResourceComboBox.SelectedIndex].name;
            userDetails.ResourceGroup = resourceGroup;
            string uri = helper.getWorkflowUri(userDetails.SubscriptionId, resourceGroup);
            UserWorkflows = laContext.GetWorkflows(uri, accessToken);
            WorkflowComboBox.ItemsSource = UserWorkflows;
            WorkflowDetails.Visibility = Visibility.Visible;
            Next.Visibility = Visibility.Visible;

        }

        private void VersionSelection_Click(object sender, RoutedEventArgs e)
        {
            string workflow = UserWorkflows[WorkflowComboBox.SelectedIndex].name;
            userDetails.Workflow = workflow;
            VersionSelection vs = new VersionSelection(userDetails,accessToken);
            main.Children.Clear();
            main.Children.Add(vs);

        }
    }
}
