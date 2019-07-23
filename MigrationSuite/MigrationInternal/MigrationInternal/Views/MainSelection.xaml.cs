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

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.Views
{
    /// <summary>
    /// Interaction logic for MainSelection.xaml
    /// </summary>
    public partial class MainSelection : UserControl
    {
        public MainSelection()
        {
            InitializeComponent();
        }
        private void HomePage_Click(object sender, RoutedEventArgs e)
        {
            HomePage hp = new HomePage();
            Main.Children.Clear();
            Main.Children.Add(hp);
        }

        private void Operation_Selected(object sender, RoutedEventArgs e)
        {
            if (AppSelect.IsChecked == true)
            {
                ApplicationManagementWorkspaceView ap = new ApplicationManagementWorkspaceView();
                Main.Children.Clear();
                Main.Children.Add(ap);
            }
            else
            {
                ManagementWorkspaceView managementWorkspaceView = new ManagementWorkspaceView();
                Main.Children.Clear();
                Main.Children.Add(managementWorkspaceView);

            }
        }
    }
}
