using MapMigration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    class MapMigrationItemViewModel : MigrationItemViewModel<MapDetails>
    {
        public MapMigrationItemViewModel(MapSelectionItemViewModel mapItem) : base(mapItem)
        {
            MapDetails map = mapItem.MigrationEntity as MapDetails;
            base.Name = map.fullNameOfMapToUpload;
        }

        public override string Name
        {
            get
            {
                return base.Name;
            }

            set
            {
                base.Name = value;
            }
        }

    }
}
