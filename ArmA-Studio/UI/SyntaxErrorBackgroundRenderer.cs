using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using ArmA.Studio.DataContext;

namespace ArmA.Studio.UI
{
    internal class SyntaxErrorBackgroundRenderer : IBackgroundRenderer
    {

        public SyntaxErrorBackgroundRenderer()
        {
        }
        public KnownLayer Layer { get { return KnownLayer.Selection; } }

        public IEnumerable<SyntaxError> SyntaxErrors { get; internal set; }

        private IEnumerable<Point> GetPoints(Rect rect, double offset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new Point(rect.BottomLeft.X + (i * offset), rect.BottomLeft.Y - ((i + 1) % 2 == 0 ? offset : 0));
            }
        }


        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            var color = new SolidColorBrush(ConfigHost.Coloring.SyntaxErrorColor);
            color.Freeze();
            var pen = new Pen(color, 1);
            pen.Freeze();
            textView.EnsureVisualLines();
            foreach(var segment in this.SyntaxErrors)
            {
                foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, segment))
                {
                    var geometry = new StreamGeometry();
                    const double vOffset = 2.5;
                    var count = (int)((rect.BottomRight.X - rect.BottomLeft.X) / vOffset) + 1;

                    using (var streamGeo = geometry.Open())
                    {
                        streamGeo.BeginFigure(rect.BottomLeft, false, false);
                        streamGeo.PolyLineTo(GetPoints(rect, vOffset, count).ToArray(), true, false);
                        
                    }
                    geometry.Freeze();
                    if (geometry != null)
                    {
                        drawingContext.DrawGeometry(null, pen, geometry);
                    }
                }
            }
        }
    }
}