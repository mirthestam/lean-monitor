using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Monitor.ViewModel;

namespace Monitor.View
{
    public class PaneStyleSelector : StyleSelector
    {
        public Style DocumentStyle { get; set; }
        public Style ToolStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is DocumentPaneViewModel)
                return DocumentStyle;

            if (item is ToolPaneViewModel)
                return ToolStyle;            

            return base.SelectStyle(item, container);
        }
    }
}
