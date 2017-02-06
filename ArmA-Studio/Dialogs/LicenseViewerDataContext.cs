using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace ArmA.Studio.Dialogs
{
    public class LicenseViewerDataContext
    {
        public ObservableCollection<License> Licenses { get; set; }
        public LicenseViewerDataContext()
        {
            var arr = App.Current.TryFindResource("Licenses") as Array;
            this.Licenses = new ObservableCollection<License>(arr.Cast<License>());
        }
    }
}
