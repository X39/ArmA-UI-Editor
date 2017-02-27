using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;

namespace ArmA.Studio
{
    class LayoutUpdateStrategy : ILayoutUpdateStrategy
    {
        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown)
        {
        }

        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument anchorableShown)
        {
            
        }

        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer)
        {
            var dockableBase = (DockableBase)anchorableToShow.Content;
            if(string.IsNullOrWhiteSpace(dockableBase.ContentId))
            {
                dockableBase.ContentId = Guid.NewGuid().ToString();
            }
            var layoutContent = layout.Descendents().OfType<LayoutContent>().FirstOrDefault(x =>
            {
                if(string.IsNullOrWhiteSpace(x.ContentId))
                {
                    x.ContentId = Guid.NewGuid().ToString();
                }
                return x.ContentId == dockableBase.ContentId;
            });
            if (layoutContent == null)
                return false;
            layoutContent.Content = anchorableToShow.Content;
            // Add layoutContent to it's previous container
            var layoutContainer = layoutContent.GetType().GetProperty("PreviousContainer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(layoutContent, null) as ILayoutContainer;
            if (layoutContainer is LayoutAnchorablePane)
                (layoutContainer as LayoutAnchorablePane).Children.Add(layoutContent as LayoutAnchorable);
            else if (layoutContainer is LayoutDocumentPane)
                (layoutContainer as LayoutDocumentPane).Children.Add(layoutContent);
            else
                return false;
            return true;
        }

        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument anchorableToShow, ILayoutContainer destinationContainer)
        {
            return false;
        }
    }
}
