using System;

namespace Monitor.ViewModel.Panels
{
    public class ProfitLossItemViewModel
    {
        public DateTime DateTime { get; set; }
        public decimal Profit { get; set; }
        public bool IsNegative { get; set; }
    }
}