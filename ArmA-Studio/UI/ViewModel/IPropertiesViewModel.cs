using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ArmA.Studio.UI.ViewModel
{
    public interface IPropertiesViewModel : IViewModel
    {
        DataTemplate Template { get; }
    }
}
