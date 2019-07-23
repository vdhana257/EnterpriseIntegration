using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.LogicConfiguration
{
    public class WorkflowVersion
    {
        public WorkflowVersion(string json)
        {
            JObject jObject = JObject.Parse(json);
            JToken props = jObject["properties"];
            changedTime = (string)props["changedTime"];
            version = (string)props["version"];
            JToken definition = props["definition"];
            parameters = props["parameters"];
            endpointsConfiguration = props["endpointsConfiguration"];
            definition_parameters = definition["parameters"];
            triggers = definition["triggers"];
            actions = definition["actions"];
            outputs = definition["outputs"];
           // MessageBox.Show(actions.ToString());


        }

        public string version { get; set; }
        public string changedTime { get; set; }
        public JToken definition_parameters { get; set; }
        public JToken triggers{ get; set; }
        public JToken actions { get; set; }
        public JToken outputs { get; set; }
        public JToken endpointsConfiguration { get; set; }
        public JToken parameters { get; set; }



        public override string ToString()
        {
            return version + "  " + changedTime;
        }
    }
}
