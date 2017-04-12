using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Monitor.Model;
using Monitor.Model.Api;
using Monitor.Model.Sessions;
using Monitor.Properties;

namespace Monitor.ViewModel.NewSession
{
    public class NewApiSessionViewModel : ViewModelBase, IDataErrorInfo, INewSessionViewModel
    {
        private readonly IApiClient _apiClient;
        private readonly ISessionService _sessionService;

        private ProjectViewModel _selectedProject;
        private InstanceViewModel _selectedInstance;

        private ObservableCollection<ProjectViewModel> _projects = new ObservableCollection<ProjectViewModel>();
        private ObservableCollection<InstanceViewModel> _instances = new ObservableCollection<InstanceViewModel>();
        private string _userId;
        private string _endpointAddress;
        private string _accessToken;

        public NewApiSessionViewModel(IApiClient apiClient, ISessionService sessionService)
        {
            _apiClient = apiClient;
            _sessionService = sessionService;
            ConnectCommand = new RelayCommand(Connect, IsConnectionSettingsValid);
            OpenCommand = new RelayCommand(Open, CanOpen);

            LoadFromSettings();

            //  Try to initially make a connection
            if (IsConnectionSettingsValid())
            {
                Connect();
            }
        }

        private void Open()
        {
            _sessionService.OpenApi(new ApiSessionParameters
            {
                InstanceId = SelectedInstance.Id,
                ProjectId = SelectedProject.ProjectId,
                InstanceType = SelectedInstance.Type
            });
        }

        private bool CanOpen()
        {
            var fieldsToValidate = new[]
            {
                nameof(SelectedProject),
                nameof(SelectedInstance),
            };

            return fieldsToValidate.All(field => string.IsNullOrEmpty(this[field]));
        }

        public string UserId
        {
            get { return _userId; }
            set
            {
                _userId = value;
                RaisePropertyChanged();
                ConnectCommand.RaiseCanExecuteChanged();
            }
        }

        public string EndpointAddress
        {
            get { return _endpointAddress; }
            set
            {
                _endpointAddress = value;
                RaisePropertyChanged();
                ConnectCommand.RaiseCanExecuteChanged();
            }
        }

        public string AccessToken
        {
            get { return _accessToken; }
            set
            {
                _accessToken = value;
                RaisePropertyChanged();
                ConnectCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand ConnectCommand { get; }

        public RelayCommand OpenCommand { get; }

        public ObservableCollection<ProjectViewModel> Projects
        {
            get { return _projects; }
            private set
            {
                _projects = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<InstanceViewModel> Instances
        {
            get { return _instances; }
            set
            {
                _instances = value;
                RaisePropertyChanged();
            }
        }

        public ProjectViewModel SelectedProject
        {
            get { return _selectedProject; }
            set
            {
                _selectedProject = value;
                RaisePropertyChanged();

                // Refresh instances when SelectedProject changed
                RefreshInstancesAsync();
            }
        }

        public InstanceViewModel SelectedInstance
        {
            get { return _selectedInstance; }
            set
            {
                _selectedInstance = value;
                RaisePropertyChanged();
                OpenCommand.RaiseCanExecuteChanged();
            }
        }

        public string this[string columnName]
        {
            get
            {
                string result = string.Empty;
                switch (columnName)
                {
                    case nameof(UserId):
                        int userId;
                        if (!int.TryParse(UserId, out userId)) result = "User Id should be a numeric value.";
                        break;

                    case nameof(EndpointAddress):
                        if (string.IsNullOrWhiteSpace(EndpointAddress)) result = "Endpoint Address is required.";
                        Uri uri;
                        if (!Uri.TryCreate(EndpointAddress, UriKind.Absolute, out uri)) result = "Address is invalid";
                        break;

                    case nameof(AccessToken):
                        if (string.IsNullOrWhiteSpace(AccessToken)) result = "Access token is required.";
                        break;

                    case nameof(SelectedProject):
                        if (SelectedProject == null) result = "Project is required";
                        break;

                    case nameof(SelectedInstance):
                        if (SelectedInstance == null) result = "Instance is required";
                        else if (!SelectedInstance.IsCompleted)
                            result = "Due to API restrictions, only completed instances can be opened";
                        break;
                }

                return result;
            }
        }

        public string Error => null;

        public bool IsConnectionSettingsValid()
        {
            var fieldsToValidate = new[]
            {
                nameof(UserId),
                nameof(EndpointAddress),
                nameof(AccessToken)
            };

            return fieldsToValidate.All(field => string.IsNullOrEmpty(this[field]));
        }

        public bool IsConnected => _apiClient.Connected;

        private void Connect()
        {
            _apiClient.Initialize(int.Parse(UserId), AccessToken, EndpointAddress);
            RaisePropertyChanged(() => IsConnected);
            if (IsConnected)
            {
                // Save the connection settings, as this connection is now valid
                SaveConnectionSettings();
                RefreshProjectsAsync();
            }
            else
            {
                Projects.Clear();
                Instances.Clear();
            }
        }

        private void LoadFromSettings()
        {
            var settings = Settings.Default;
            UserId = settings.ApiUserId.ToString();
            EndpointAddress = settings.ApiBaseUrl;
            AccessToken = settings.ApiAccessToken;
        }

        private void SaveConnectionSettings()
        {
            var settings = Settings.Default;
            settings.ApiUserId = int.Parse(UserId);
            settings.ApiBaseUrl = EndpointAddress;
            settings.ApiAccessToken = AccessToken;
        }

        private async void RefreshProjectsAsync()
        {
            var projects = await _apiClient.GetProjectsAsync();
            Projects = new ObservableCollection<ProjectViewModel>(projects.OrderByDescending(p => p.Modified).Select(p => new ProjectViewModel
            {
                Name = p.Name,
                ProjectId = p.ProjectId,
                Created = p.Created,
                Modified = p.Modified
            }));
            SelectedProject = Projects.FirstOrDefault();
        }

        private async void RefreshInstancesAsync()
        {
            if (SelectedProject == null)
            {
                SelectedInstance = null;
                Instances.Clear();
                return;
            }

            var instances = await _apiClient.GetBacktestsAsync(SelectedProject.ProjectId);

            Instances = new ObservableCollection<InstanceViewModel>(instances.Where(i => i.Completed).Select(i => new InstanceViewModel
            {
                Name = i.Name,
                Id = i.BacktestId,
                Type = ResultType.Backtest,
                Note = i.Note,
                Progress = i.Progress
            }));

            SelectedInstance = Instances.FirstOrDefault();
        }

        public string Header { get; } = "From API";
    }
}