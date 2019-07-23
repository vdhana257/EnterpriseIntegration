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
    /// Interaction logic for CompareResult.xaml
    /// </summary>
    public partial class CompareResult : UserControl
    {
        WorkflowVersion version1;
        WorkflowVersion version2;
        public CompareResult(WorkflowVersion Version1, WorkflowVersion Version2)
        {
            
            this.version1 = Version1;
            this.version2 = Version2;
            InitializeComponent();
            aux();
        }
        
        public void aux()
        {
           // MessageBox.Show(version1.ToString());
            bool res1 = JToken.DeepEquals(version1.outputs, version2.outputs);
            if(res1 == true)
            {
                outputsStatus.Text = "Consistent";
                outputsStatus.Foreground = Brushes.Green;
            }
            else
            {
                outputsStatus.Text = "Inconsistent";
                outputsStatus.Foreground = Brushes.Red;
            }

            bool res2 = JToken.DeepEquals(version1.triggers, version2.triggers);
           // MessageBox.Show(version1.triggers.ToString());
           // MessageBox.Show(version2.triggers.ToString());
            if (res2 == true)
            {
                triggersStatus.Text = "Consistent";
                triggersStatus.Foreground = Brushes.Green;
            }
            else
            {
                triggersStatus.Text = "Inconsistent";
                triggersStatus.Foreground = Brushes.Red;
            }

            bool res3 = JToken.DeepEquals(version1.parameters, version2.parameters);
            if (res3 == true)
            {
                parametersStatus.Text = "Consistent";
                parametersStatus.Foreground = Brushes.Green;
            }
            else
            {
                parametersStatus.Text = "Inconsistent";
                parametersStatus.Foreground = Brushes.Red;
            }

            bool res4 = JToken.DeepEquals(version1.definition_parameters, version2.definition_parameters);
            if (res4 == true)
            {
                definition_parameterStatus.Text = "Consistent";
                definition_parameterStatus.Foreground = Brushes.Green;
            }
            else
            {
                definition_parameterStatus.Text = "Inconsistent";
                definition_parameterStatus.Foreground = Brushes.Red;
            }

            bool res5 = JToken.DeepEquals(version1.endpointsConfiguration, version2.endpointsConfiguration);
            if (res5 == true)
            {
                endpointsConfigurationStatus.Text = "Consistent";
                endpointsConfigurationStatus.Foreground = Brushes.Green;
            }
            else
            {
                endpointsConfigurationStatus.Text = "Inconsistent";
                endpointsConfigurationStatus.Foreground = Brushes.Red;
            }

            bool res6 = JToken.DeepEquals(version1.actions, version2.actions);
            if (res6 == true)
            {
                actionsStatus.Text = "Consistent";
                actionsStatus.Foreground = Brushes.Green;
            }
            else
            {
                actionsStatus.Text = "Inconsistent";
                actionsStatus.Foreground = Brushes.Red;
            }

            if(res1 && res2 && res3 && res4 && res5 && res6)
            {
                status.Text = "Compliant";
                status.Foreground = Brushes.DarkGreen;
            }
            else
            {
                status.Text = "Non-Compliant";
                status.Foreground = Brushes.DarkRed;
            }

             
        }

        private void parameterInfo(Object sender,RoutedEventArgs e)
        {
            ResultsInfo ri = new ResultsInfo(version1.parameters, version2.parameters,version1,version2);
            main.Children.Clear();
            main.Children.Add(ri);
            ri.version1Name.Text = "Version : " + version1.version;
            ri.version1Date.Text = "Changed Time : " + version1.changedTime;
            ri.version2Name.Text = "Version : " + version2.version;
            ri.version2Date.Text = "Changed Time : " + version2.changedTime;

        }

        private void definition_parameterInfo(Object sender, RoutedEventArgs e)
        {
            ResultsInfo ri = new ResultsInfo(version1.definition_parameters, version2.definition_parameters, version1, version2);
            main.Children.Clear();
            main.Children.Add(ri);
            ri.version1Name.Text = "Version : " + version1.version;
            ri.version1Date.Text = "Changed Time : " + version1.changedTime;
            ri.version2Name.Text = "Version : " + version2.version;
            ri.version2Date.Text = "Changed Time : " + version2.changedTime;

        }

        private void triggerInfo(Object sender, RoutedEventArgs e)
        {
            ResultsInfo ri = new ResultsInfo(version1.triggers, version2.triggers, version1, version2);
            main.Children.Clear();
            main.Children.Add(ri);
            ri.version1Name.Text = "Version : " + version1.version;
            ri.version1Date.Text = "Changed Time : " + version1.changedTime;
            ri.version2Name.Text = "Version : " + version2.version;
            ri.version2Date.Text = "Changed Time : " + version2.changedTime;
        }

        private void actionInfo(Object sender, RoutedEventArgs e)
        {
            ResultsInfo ri = new ResultsInfo(version1.actions, version2.actions, version1, version2);
            main.Children.Clear();
            main.Children.Add(ri);
            ri.version1Name.Text = "Version : " + version1.version;
            ri.version1Date.Text = "Changed Time : " + version1.changedTime;
            ri.version2Name.Text = "Version : " + version2.version;
            ri.version2Date.Text = "Changed Time : " + version2.changedTime;
        }

        private void outputsInfo(Object sender, RoutedEventArgs e)
        {
            ResultsInfo ri = new ResultsInfo(version1.outputs, version2.outputs, version1, version2);
            main.Children.Clear();
            main.Children.Add(ri);
            ri.version1Name.Text = "Version : " + version1.version;
            ri.version1Date.Text = "Changed Time : " + version1.changedTime;
            ri.version2Name.Text = "Version : " + version2.version;
            ri.version2Date.Text = "Changed Time : " + version2.changedTime;
        }

        private void endpointsConfigurationInfo(Object sender, RoutedEventArgs e)
        {
            ResultsInfo ri = new ResultsInfo(version1.endpointsConfiguration, version2.endpointsConfiguration, version1, version2);
            main.Children.Clear();
            main.Children.Add(ri);
            ri.version1Name.Text = "Version : " + version1.version;
            ri.version1Date.Text = "Changed Time : " + version1.changedTime;
            ri.version2Name.Text = "Version : " + version2.version;
            ri.version2Date.Text = "Changed Time : " + version2.changedTime;
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
    }
}
