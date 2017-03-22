using GalaSoft.MvvmLight;

namespace Monitor.ViewModel.Panels
{
    public class RollingStatisticsItemViewModel : ViewModelBase
    {
        private decimal _m1;

        public decimal M1
        {
            get { return _m1; }
            set
            {
                _m1 = value;
                RaisePropertyChanged();
            }
        }

        public decimal M3 { get; set; }
        public decimal M6 { get; set; }
        public decimal M12 { get; set; }
    }
}