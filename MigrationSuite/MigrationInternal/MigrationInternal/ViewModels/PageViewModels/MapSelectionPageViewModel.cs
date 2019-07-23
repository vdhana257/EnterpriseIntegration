using MapMigration;
using Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    class MapSelectionPageViewModel : SelectionPageViewModel<MapSelectionItemViewModel, MapDetails>
    {
        private bool _dataGridEnabled;
        private bool _filterdataGridEnabled;
        private bool _mapSearchEnabled;
        public override string Title
        {
            get
            {
                return Resources.MapSelectionPageTitle;
            }
        }

        private string mapFilter;
        public string MapFilter
        {
            get
            {
                return mapFilter;
            }
            set
            {
                mapFilter = value;
                ShowFilteredList();
                this.RaisePropertyChanged("MapFilter");
            }
        }
        public bool MapDataGridEnabled
        {
            get { return _dataGridEnabled; }
            set
            {
                _dataGridEnabled = value;
                this.RaisePropertyChanged("MapDataGridEnabled");
            }
        }

        public bool FilterDataGridEnabled
        {
            get
            {
                return _filterdataGridEnabled;
            }
            set
            {
                _filterdataGridEnabled = value;
                this.RaisePropertyChanged("FilterDataGridEnabled");
            }
        }

        public bool SearchBoxEnabled
        {
            get
            {
                _mapSearchEnabled = MapDataGridEnabled | FilterDataGridEnabled;
                return _mapSearchEnabled;
            }
            set
            {
                _mapSearchEnabled = value;
                this.RaisePropertyChanged("SearchBoxEnabled");
            }
        }
        protected override string SelectionItemsContextPropertyName
        {
            get { return AppConstants.AllMapsContextPropertyName; }
        }

        protected override void CheckItemsList()
        {
            if (this.SelectionItems == null)
            {
                MessageBox.Show("No Map(s) found. Please try again.");
                this.MapDataGridEnabled = false;
                this.FilterDataGridEnabled = false;
                this.SearchBoxEnabled = FilterDataGridEnabled | MapDataGridEnabled;
            }
            else if (this.SelectionItems.Count == 0)
            {
                MessageBox.Show("No Map(s) found. Please try again.");
                this.MapDataGridEnabled = false;
                this.FilterDataGridEnabled = false;
                this.SearchBoxEnabled = FilterDataGridEnabled | MapDataGridEnabled;
            }
            else
            {
                this.MapDataGridEnabled = true;
                this.FilterDataGridEnabled = false;
                this.SearchBoxEnabled = FilterDataGridEnabled | MapDataGridEnabled;
            }
        }

        private ObservableCollection<MapSelectionItemViewModel> filterItems;
        public ObservableCollection<MapSelectionItemViewModel> FilterItems
        {
            get
            {
                return this.filterItems;
            }

            set
            {
                this.filterItems = value;
                this.RaisePropertyChanged("FilterItems");
            }

        }
        private void ShowFilteredList()
        {
            var maps = this.ApplicationContext.GetProperty(AppConstants.AllMapsContextPropertyName) as ObservableCollection<MapSelectionItemViewModel>;
            if (maps != null && maps.Count != 0)
            {
                if (MapFilter != "" && MapFilter != null)
                {
                    this.FilterItems = new ObservableCollection<MapSelectionItemViewModel>(maps.Where(x => x.MigrationEntity.mapFullName.ToLower().Contains(MapFilter.ToLower()) || x.MigrationEntity.assemblyFullyQualifiedName.ToLower().Contains(MapFilter.ToLower())));
                    this.FilterDataGridEnabled = true;
                    this.MapDataGridEnabled = false;
                    this.SearchBoxEnabled = FilterDataGridEnabled | MapDataGridEnabled;
                }
                else
                {
                    this.FilterDataGridEnabled = false;
                    this.MapDataGridEnabled = true;
                    this.SearchBoxEnabled = FilterDataGridEnabled | MapDataGridEnabled;
                }
            }
            else
            {
                this.FilterDataGridEnabled = false;
                this.MapDataGridEnabled = false;
                this.SearchBoxEnabled = FilterDataGridEnabled | MapDataGridEnabled;
            }
        }
        protected override Task<IEnumerable<MapDetails>> GetEntitiesAsync()
        {
            return Task.Factory.StartNew<IEnumerable<MapDetails>>(() =>
            {
                try
                {
                    var appDetails = ApplicationContext.GetProperty("SelectedApps") as ObservableCollection<ApplicationDetails>;
                    string parameter = "";
                    foreach (var item in appDetails)
                    {
                        if (item.isSelected)
                            parameter += "'" + item.nID + "',";
                    }
                    parameter = parameter.Remove(parameter.Length - 1);

                    var maps = this.ApplicationContext.GetProperty(AppConstants.AllMapsContextPropertyName) as ObservableCollection<MapSelectionItemViewModel>;
                    List<MapDetails> mapsList = new List<MapDetails>();
                    if (maps != null && maps.Count != 0)
                    {
                        foreach (var mapItem in maps)
                        {
                            mapsList.Add(mapItem.MigrationEntity);
                        }
                        return mapsList;
                    }
                    else
                    {
                        ActionsOnMaps action = new ActionsOnMaps();
                        string connectionString = this.ApplicationContext.GetProperty("DatabaseConnectionString") as string;
                        mapsList = action.GetListOfMaps(connectionString, Resources.JsonMapFilesLocalPath, parameter);
                        this.ApplicationContext.SetProperty(AppConstants.MapNamespaceVersionList, action.mapNamespaceVersionDict);
                        return mapsList;
                    }

                }

                catch (Exception ex)
                {
                    TraceProvider.WriteLine(string.Format(CultureInfo.InvariantCulture, "Not able to retrieve maps. {0}", ExceptionHelper.GetExceptionMessage(ex)));
                    var statusBarViewModel = this.ApplicationContext.GetService<StatusBarViewModel>();
                    statusBarViewModel.StatusInfoType = StatusInfoType.Error;
                    statusBarViewModel.ShowError("Error. Failed to retrieve maps from Biztalk Server. Reason: " + ExceptionHelper.GetExceptionMessage(ex));
                    return new List<MapDetails>();
                }
            });
        }
    }
}
