using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ArmA_UI_Editor.Code.Interface
{
    public interface ISnapWindow
    {
        
        void UnloadSnap();
        void LoadSnap();
        int AllowedCount { get; }
        string Title { get; }
        Dock DefaultDock { get; }
    }
}
