using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArmA_UI_Editor.Code.AddInUtil;

namespace ArmA_UI_Editor.Code.UI.DragDrop
{
    internal class UiElementsListBoxData
    {
        public UIElement ElementData;

        public UiElementsListBoxData(UIElement elementData)
        {
            this.ElementData = elementData;
        }
    }
}
