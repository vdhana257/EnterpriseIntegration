using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using MigrationTool;
using Newtonsoft.Json.Linq;
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
    /// Interaction logic for ResultsInfo.xaml
    /// </summary>
    public partial class ResultsInfo : UserControl
    {
        JToken v1;
        JToken v2;
        WorkflowVersion version1;
        WorkflowVersion version2;
        string str1;
        string str2;
        public ResultsInfo(JToken version1, JToken version2, WorkflowVersion Version1, WorkflowVersion Version2)
        {
            InitializeComponent();
            v1 = version1;
            v2 = version2;
            this.version1 = Version1;
            this.version2 = Version2;
            //display();
            str1 = v1.ToString();
            str2 = v2.ToString();
            StringBuilder sb = new StringBuilder();
            var d = new Differ();
            var builder = new InlineDiffBuilder(d);
            var result = builder.BuildDiffModel(str1, str2);

            foreach (var line in result.Lines)
            {
                if (line.Type == ChangeType.Inserted)
                {
                    sb.Append("- ");
                    TextRange tr = new TextRange(ver2.Document.ContentEnd, ver2.Document.ContentEnd);
                    tr.Text ="- "+ line.Text;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, System.Windows.Media.Brushes.Red);
                    ver2.AppendText(System.Environment.NewLine);
                    ver2.ScrollToEnd();
                }
                else if (line.Type == ChangeType.Deleted)
                {
                    sb.Append("+ ");
                    TextRange tr = new TextRange(ver1.Document.ContentEnd, ver1.Document.ContentEnd);
                    tr.Text = "+ "+line.Text;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, System.Windows.Media.Brushes.Green);
                    ver1.AppendText(System.Environment.NewLine);
                    ver1.ScrollToEnd();
                }
                else if (line.Type == ChangeType.Modified)
                {
                    sb.Append("* ");
                }
                else if (line.Type == ChangeType.Imaginary)
                {
                    sb.Append("? ");
                }
                else if (line.Type == ChangeType.Unchanged)
                {
                    sb.Append("  ");
                    TextRange tr = new TextRange(ver1.Document.ContentEnd, ver1.Document.ContentEnd);
                    tr.Text = line.Text;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, System.Windows.Media.Brushes.Black);
                    ver1.AppendText(System.Environment.NewLine);
                    ver1.ScrollToEnd();

                    TextRange tr1 = new TextRange(ver2.Document.ContentEnd, ver2.Document.ContentEnd);
                    tr1.Text = line.Text;
                    tr1.ApplyPropertyValue(TextElement.ForegroundProperty, System.Windows.Media.Brushes.Black);
                    ver2.AppendText(System.Environment.NewLine);
                    ver2.ScrollToEnd();
                }

                sb.Append(line.Text + "\n");
            }
           // string str = sb.ToString();
            //MessageBox.Show(str);


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

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            CompareResult cr = new CompareResult(version1,version2);
            main.Children.Clear();
            main.Children.Add(cr);
        }

        public void display()
        {
            ver1.AppendText(v1.ToString());
            ver2.AppendText(v2.ToString());
        }


    }
}
