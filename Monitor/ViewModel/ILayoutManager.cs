using Xceed.Wpf.AvalonDock;

namespace Monitor.ViewModel
{
    public interface ILayoutManager
    {
        void LoadLayout(DockingManager manager);
        void ResetLayout(DockingManager manager);
        void SaveLayout(DockingManager manager);
    }
}