using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SchemaMigration;
using Newtonsoft.Json;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    class SchemaSelectionPageViewModel : SelectionPageViewModel<SchemaSelectionItemViewModel, SchemaDetails>
    {
        private bool _dataGridEnabled;
        private bool _filterdataGridEnabled;
        private bool _schemaSearchEnabled;
        public override string Title
        {
            get
            {
                return Resources.SchemaSelectionPageTitle;
            }
        }

        private string schemaFilter;
        public string SchemaFilter
        {
            get
            {
                return schemaFilter;
            }
            set
            {
                schemaFilter = value;
                ShowFilteredList();
                this.RaisePropertyChanged("SchemaFilter");
            }
        }
        public bool SchemaDataGridEnabled
        {
            get { return _dataGridEnabled; }
            set
            {
                _dataGridEnabled = value;
                this.RaisePropertyChanged("SchemaDataGridEnabled");
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
                _schemaSearchEnabled = SchemaDataGridEnabled | FilterDataGridEnabled;
                return _schemaSearchEnabled;
            }
            set
            {
                _schemaSearchEnabled = value;
                this.RaisePropertyChanged("SearchBoxEnabled");
            }
        }
        protected override string SelectionItemsContextPropertyName
        {
            get { return AppConstants.AllSchemasContextPropertyName; }
        }

        protected override void CheckItemsList()
        {
            if (this.SelectionItems == null)
            {
                MessageBox.Show("No Schema(s) found. Please try again.");
                this.SchemaDataGridEnabled = false;
                this.FilterDataGridEnabled = false;
                this.SearchBoxEnabled = FilterDataGridEnabled | SchemaDataGridEnabled;
            }
            else if (this.SelectionItems.Count == 0)
            {
                MessageBox.Show("No Schema(s) found. Please try again.");
                this.SchemaDataGridEnabled = false;
                this.FilterDataGridEnabled = false;
                this.SearchBoxEnabled = FilterDataGridEnabled | SchemaDataGridEnabled;
            }
            else
            {
                this.SchemaDataGridEnabled = true;
                this.FilterDataGridEnabled = false;
                this.SearchBoxEnabled = FilterDataGridEnabled | SchemaDataGridEnabled;
            }
        }

        private ObservableCollection<SchemaSelectionItemViewModel> filterItems;
        public ObservableCollection<SchemaSelectionItemViewModel> FilterItems
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
            var schemas = this.ApplicationContext.GetProperty(AppConstants.AllSchemasContextPropertyName) as ObservableCollection<SchemaSelectionItemViewModel>;
            if(schemas != null && schemas.Count != 0)
            {
                if (SchemaFilter != "" && schemaFilter != null)
                {
                    this.FilterItems = new ObservableCollection<SchemaSelectionItemViewModel>( schemas.Where(x => x.MigrationEntity.schemaFullName.ToLower().Contains(SchemaFilter.ToLower()) || x.MigrationEntity.assemblyFullyQualifiedName.ToLower().Contains(SchemaFilter.ToLower())));
                    this.FilterDataGridEnabled = true;
                    this.SchemaDataGridEnabled = false;
                    this.SearchBoxEnabled = FilterDataGridEnabled | SchemaDataGridEnabled;
                }
                else
                {
                    this.FilterDataGridEnabled = false;
                    this.SchemaDataGridEnabled = true;
                    this.SearchBoxEnabled = FilterDataGridEnabled | SchemaDataGridEnabled;
                }
            }
            else
            {
                this.FilterDataGridEnabled = false;
                this.SchemaDataGridEnabled = false;
                this.SearchBoxEnabled = FilterDataGridEnabled | SchemaDataGridEnabled;
            }
        }
        protected override Task<IEnumerable<SchemaDetails>> GetEntitiesAsync()
        {
            return Task.Factory.StartNew<IEnumerable<SchemaDetails>>(() =>
            {
                try
                {
                    var schemas = this.ApplicationContext.GetProperty(AppConstants.AllSchemasContextPropertyName) as ObservableCollection<SchemaSelectionItemViewModel>;
                    List<SchemaDetails> schemasList = new List<SchemaDetails>();
                    if (schemas != null && schemas.Count != 0)
                    {
                        foreach (var schemaItem in schemas)
                        {
                            schemasList.Add(schemaItem.MigrationEntity);
                        }
                        return schemasList;
                    }
                    else
                    {
                        ActionOnSchemas action = new ActionOnSchemas();
                        string connectionString = this.ApplicationContext.GetProperty("DatabaseConnectionString") as string;
                        schemasList = action.GetListOfSchemas(connectionString, Resources.JsonSchemaFilesLocalPath);
                        this.ApplicationContext.SetProperty(AppConstants.SchemaNamespaceVersionList, action.schemaNamespaceVersionDict);
                        return schemasList;
                    }

                }

                catch (Exception ex)
                {
                    TraceProvider.WriteLine(string.Format(CultureInfo.InvariantCulture, "Not able to retrieve schemas. {0}", ExceptionHelper.GetExceptionMessage(ex)));
                    var statusBarViewModel = this.ApplicationContext.GetService<StatusBarViewModel>();
                    statusBarViewModel.StatusInfoType = StatusInfoType.Error;
                    statusBarViewModel.ShowError("Error. Failed to retrieve schemas from Biztalk Server. Reason: " + ExceptionHelper.GetExceptionMessage(ex));
                    return new List<SchemaDetails>();
                }
            });
        }

    }
}