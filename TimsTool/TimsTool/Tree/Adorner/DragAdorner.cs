using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TimsTool.Tree.Adorner
{
    //public class DragAdorner : System.Windows.Documents.Adorner
    //{
        //Rect renderRect;
        //Brush renderBrush;
        //public Point CenterOffset;
        //public DragAdorner(UIElement adornedElement) : base(adornedElement)
        //{
        //    renderRect = new Rect(adornedElement.RenderSize);
        //    this.IsHitTestVisible = false;
        //    //Clone so that it can be modified with on modifying the original
        //    renderBrush = adornedElement.Background.Clone();
        //    CenterOffset = new Point(-renderRect.Width / 2, -renderRect.Height / 2);
        //}
        //protected override void OnRender(DrawingContext drawingContext)
        //{
        //    drawingContext.DrawRectangle(renderBrush, null, renderRect);
        //}
    //}
}
