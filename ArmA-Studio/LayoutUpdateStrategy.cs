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
        private bool BeforeInsert_(LayoutRoot layout, LayoutContent anchorableToShow)
        {
            var dockable = (DockableBase)anchorableToShow.Content;
            var layoutContent = layout.Descendents().OfType<LayoutContent>().FirstOrDefault(x => x.ContentId == dockable.ContentId);
            if (layoutContent == null)
                return false;
            layoutContent.Content = anchorableToShow.Content;


            var layoutContainer = layoutContent.GetType().GetProperty("PreviousContainer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(layoutContent, null) as ILayoutContainer;
            if (layoutContainer is LayoutAnchorablePane)
                (layoutContainer as LayoutAnchorablePane).Children.Add(layoutContent as LayoutAnchorable);
            else if (layoutContainer is LayoutDocumentPane)
                (layoutContainer as LayoutDocumentPane).Children.Add(layoutContent);
            else
                return false;
            return true;
        }

        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown)
        {

        }

        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument anchorableShown)
        {
            
        }

        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer)
        {
            return this.BeforeInsert_(layout, anchorableToShow);
        }

        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument anchorableToShow, ILayoutContainer destinationContainer)
        {
            return this.BeforeInsert_(layout, anchorableToShow);
        }
    }
}
