using MapMigration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    class MapSelectionItemViewModel : SelectionItemViewModel<MapDetails>
    {
        public MapSelectionItemViewModel(MapDetails map) : base(map)
        {
        }

    }
}
