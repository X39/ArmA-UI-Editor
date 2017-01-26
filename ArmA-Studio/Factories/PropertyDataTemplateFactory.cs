using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ArmA.Studio.Factories
{
    public static class PropertyDataTemplateFactory
    {
        public static DataTemplate GetDatatemplate(this INotifyPropertyChanging obj)
        {
            DataTemplate dt = new DataTemplate(obj.GetType());
            throw new NotImplementedException();
        }
    }
}
