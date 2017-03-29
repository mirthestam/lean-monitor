using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Monitor.Model;
using Monitor.Model.Api;
using Monitor.Properties;

namespace Monitor.ViewModel.NewSession
{
    public class NewApiSessionViewModel : ViewModelBase
    {
        private readonly IApiClient _apiClient;

        private ProjectViewModel _selectedProject;
        private InstanceViewModel _selectedInstance;

        private ObservableCollection<ProjectViewModel> _projects = new ObservableCollection<ProjectViewModel>();
        private ObservableCollection<InstanceViewModel> _instances = new ObservableCollection<InstanceViewModel>();

        public NewApiSessionViewModel(IApiClient apiClient)
        {
            _apiClient = apiClient;
            RefreshCommand = new RelayCommand(RefreshProjectsAsync);
            ConnectCommand = new RelayCommand(Connect);

            //  Try to initially make a connection
            if (ValidateConnectionSettings())
            {
                Connect();
            }
        }

        public int UserId
        {
            get { return Settings.Default.ApiUserId; }
            set
            {
                if (Settings.Default.ApiUserId == value) return;
                Settings.Default.ApiUserId = value;
                Settings.Default.Save();
            }
        }

        public string EndpointAddress
        {
            get { return Settings.Default.ApiBaseUrl; }
            set
            {
                if (Settings.Default.ApiBaseUrl == value) return;
                Settings.Default.ApiBaseUrl = value;
                Settings.Default.Save();
            }
        }

        public string AccessToken
        {
            get { return Settings.Default.ApiAccessToken; }
            set
            {
                if (Settings.Default.ApiAccessToken == value) return;
                Settings.Default.ApiAccessToken = value;
                Settings.Default.Save();
            }
        }

        private bool ValidateConnectionSettings()
        {
            var endpointAddress = !string.IsNullOrWhiteSpace(EndpointAddress);
            var accessToken = !string.IsNullOrWhiteSpace(AccessToken);
            return endpointAddress && accessToken;
        }

        public RelayCommand ConnectCommand { get; private set; }
        public RelayCommand RefreshCommand { get; private set; }

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
            get
            {
                return _instances;
            }
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
            }
        }

        private void Connect()
        {
            _apiClient.Initialize(UserId, AccessToken, EndpointAddress);
            RaisePropertyChanged(() => IsConnected);
            if (IsConnected)
            {
                RefreshProjectsAsync();
            }
            else
            {
                Projects.Clear();
                Instances.Clear();
            }
        }

        public bool IsConnected => _apiClient.Connected;

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
            }

            var instances = await _apiClient.GetBacktestsAsync(SelectedProject.ProjectId);
            Instances = new ObservableCollection<InstanceViewModel>(instances.Select(i => new InstanceViewModel
            {
                Name = i.Name,
                Id = i.BacktestId,
                Type = ResultType.Backtest,
                Note = i.Note,
                Progress = i.Progress
            }));
            SelectedInstance = Instances.FirstOrDefault();
        }
    }
}