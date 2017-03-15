using System;

namespace QuantConnect.Lean.Monitor.ViewModel.Panels
{
    public class ProfitLossItemViewModel
    {
        public DateTime DateTime { get; set; }
        public decimal Profit { get; set; }
    }
}