using System.Collections.Generic;

namespace Monitor.ViewModel.Grids
{
    /// <summary>
    /// Abstract view model for grid views (i.e. Generic grid, Strategy Equity grid)
    /// </summary>
    public abstract class GridPanelViewModelBase : DocumentViewModel
    {
        private List<GridSerie> _series;

        private GridSerie _selectedSeries;

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
                RaisePropertyChanged();
            }
        }      
    }
}