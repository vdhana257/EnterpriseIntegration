using Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration;
using Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.LogicConfiguration;
using Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.Views;
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
using TestPublisherHelper;

namespace MigrationTool
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : UserControl
    {
        public HomePage()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                BizTalkAdminOperations bz = new BizTalkAdminOperations();
                Main.Children.Clear();
                Main.Children.Add(bz);
            }));
        }

        private void TPM_Click(object sender, RoutedEventArgs e)
        {
            MainSelection ms = new MainSelection();
            Main.Children.Clear();
            Main.Children.Add(ms);

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            LoginPage lp = new LoginPage();
            Main.Children.Clear();
            Main.Children.Add(lp);

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            SelectionPage help = new SelectionPage();
            Main.Children.Clear();
            Main.Children.Add(help);

        }
    }

        
}
