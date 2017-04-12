using System.Collections.Generic;

namespace Monitor.ViewModel.Grids
{
    /// <summary>
    /// Abstract view model for grid views (i.e. Generic grid, Strategy Equity grid)
    /// </summary>
    public abstract class GridPanelViewModelBase : DocumentPaneViewModel
    {
        private List<GridSerie> _series;

        private string _chartName;

        private GridSerie _selectedSeries;

        public string ChartName
        {
            get { return _chartName; }
            set
            {
                _chartName = value;
                FormatName();
                RaisePropertyChanged();
            }
        }

        public List<GridSerie> Series
        {
            get
            {
                return _series;
            }
            set
            {
                _series = value;
                RaisePropertyChanged();
            }
        }

        public GridSerie SelectedSeries
        {
            get
            {
                return _selectedSeries;
            }
            set
            {
                _selectedSeries = value;
                FormatName();
                RaisePropertyChanged();
            }
        }

        private void FormatName()
        {
            Name = $"{ChartName} [{SelectedSeries.Name}]";
        }
    }
}