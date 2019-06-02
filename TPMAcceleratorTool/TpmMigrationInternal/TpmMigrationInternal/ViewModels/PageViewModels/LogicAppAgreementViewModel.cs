using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    class LogicAppAgreementViewModel : BaseViewModel, IPageViewModel
    {

        public string Title
        {
            get
            {
                return Resources.WelcomePageTitle;
            }
        }

        public string WelcomeText
        {
            get
            {
                return Resources.WelcomeText;
            }
        }

    }
}
