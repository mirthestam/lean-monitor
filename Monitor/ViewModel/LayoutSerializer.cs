using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantConnect.Data.Fundamental;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

namespace Monitor.ViewModel
{
    public class LayoutManager : ILayoutManager
    {
        public void LoadLayout(DockingManager manager)
        {            
            if (File.Exists(LayoutFileName))
            {
                LoadLayout(manager, LayoutFileName);
            }
            else
            {
                ResetLayout(manager);
            }
        }

        public void ResetLayout(DockingManager manager)
        {
            if (!File.Exists(DefaultLayoutFileName)) return;

            LoadLayout(manager, DefaultLayoutFileName);
        }

        public void SaveLayout(DockingManager manager)
        {
            // Craete the folder if it does not exist yet
            if (!Directory.Exists(DataFolder)) Directory.CreateDirectory(DataFolder);
            if (File.Exists(LayoutFileName)) File.Delete(LayoutFileName);

            // Serialize the layout
            var serializer = new XmlLayoutSerializer(manager);
            serializer.Serialize(LayoutFileName);
        }

        private static void LoadLayout(DockingManager manager, string fileName)
        {
            var serializer = new XmlLayoutSerializer(manager);
            serializer.Deserialize(fileName);
        }

        private static string DataFolder => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Lean" + Path.DirectorySeparatorChar + "Monitor";

        private static string LayoutFileName => DataFolder + Path.DirectorySeparatorChar + "layout.xml";

        private static string DefaultLayoutFileName => "layout.default.xml";
    }
}
