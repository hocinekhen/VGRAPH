﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace VGRAPH
{
    public class DraggableCanvas : Canvas
    {

        #region Data 


        private UIElement elementBeingDragged;
        private Point origCursorLocation;
        private double origHorizOffset, origVertOffset;
        private bool modifyLeftOffset, modifyTopOffset;
        private bool isDragInProgress;
        private bool isDraggable;
        protected bool IsDraggable
        {
            get { return this.isDraggable; }
            set
            {
                if (value == false) this.PreviewMouseLeftButtonDown -= Drag;
                else if (!isDraggable)
                    this.PreviewMouseLeftButtonDown += Drag;
                this.isDraggable = value;
            }
        }
        #endregion
        #region Static Constructor 

        static DraggableCanvas()
        {
            AllowDraggingProperty = DependencyProperty.Register(
            "AllowDragging",
            typeof(bool),
            typeof(DraggableCanvas),
            new PropertyMetadata(true));

            AllowDragOutOfViewProperty = DependencyProperty.Register(
            "AllowDragOutOfView",
            typeof(bool),
            typeof(DraggableCanvas),
            new UIPropertyMetadata(false));

            CanBeDraggedProperty = DependencyProperty.RegisterAttached(
            "CanBeDragged",
            typeof(bool),
            typeof(DraggableCanvas),
            new UIPropertyMetadata(true));
        }

        #endregion
        #region Constructor 
        public DraggableCanvas()
        {

        }

        #endregion

        #region Attached Properties 

        #region CanBeDragged 


        public static readonly DependencyProperty CanBeDraggedProperty;
        public static bool GetCanBeDragged(UIElement uiElement)
        {
            if (uiElement == null)
                return false;
            if (uiElement is Path || uiElement is Polygon || uiElement is EdgeLabel)
                return false;
            return (bool)uiElement.GetValue(CanBeDraggedProperty);
        }
        public static void SetCanBeDragged(UIElement uiElement, bool value)
        {
            if (uiElement != null)
                uiElement.SetValue(CanBeDraggedProperty, value);
        }

        #endregion

        #endregion

        #region Interface 

        #region AllowDragging 
        public static readonly DependencyProperty AllowDraggingProperty;
        public bool AllowDragging
        {
            get { return (bool)base.GetValue(AllowDraggingProperty); }
            set { base.SetValue(AllowDraggingProperty, value); }
        }

        #endregion

        #region AllowDragOutOfView 
        public static readonly DependencyProperty AllowDragOutOfViewProperty;
        public bool AllowDragOutOfView
        {
            get { return (bool)GetValue(AllowDragOutOfViewProperty); }
            set { SetValue(AllowDragOutOfViewProperty, value); }
        }

        #endregion
        #region BringToFront / SendToBack 
        public void BringToFront(UIElement element)
        {
            this.UpdateZOrder(element, true);
        }
        public void SendToBack(UIElement element)
        {
            this.UpdateZOrder(element, false);
        }

        #endregion

        #region ElementBeingDragged 
        public UIElement ElementBeingDragged
        {
            get
            {
                if (!this.AllowDragging)
                    return null;
                else
                    return this.elementBeingDragged;
            }
            protected set
            {
                if (this.elementBeingDragged != null)
                    this.elementBeingDragged.ReleaseMouseCapture();

                if (!this.AllowDragging)
                    this.elementBeingDragged = null;
                else
                {
                    if (DraggableCanvas.GetCanBeDragged(value))
                    {
                        this.elementBeingDragged = value;
                        this.elementBeingDragged.CaptureMouse();
                    }
                    else
                        this.elementBeingDragged = null;
                }
            }
        }

        #endregion

        #region FindCanvasChild 
        public UIElement FindCanvasChild(DependencyObject depObj)
        {
            while (depObj != null)
            {
                UIElement elem = depObj as UIElement;
                if (elem != null && base.Children.Contains(elem))
                    break;
                if (depObj is Visual || depObj is Visual3D)
                    depObj = VisualTreeHelper.GetParent(depObj);
                else
                    depObj = LogicalTreeHelper.GetParent(depObj);
            }
            return depObj as UIElement;
        }

        #endregion

        #endregion

        #region Overrides 

        #region OnPreviewMouseLeftButtonDown 

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

        }
        protected void Drag(object sender, MouseButtonEventArgs e)
        {
            this.isDragInProgress = false;
            this.origCursorLocation = e.GetPosition(this);
            this.ElementBeingDragged = this.FindCanvasChild(e.Source as DependencyObject);
            if (this.ElementBeingDragged == null)
                return;

            double left = Canvas.GetLeft(this.ElementBeingDragged);
            double right = Canvas.GetRight(this.ElementBeingDragged);
            double top = Canvas.GetTop(this.ElementBeingDragged);
            double bottom = Canvas.GetBottom(this.ElementBeingDragged);
            this.origHorizOffset = ResolveOffset(left, right, out this.modifyLeftOffset);
            this.origVertOffset = ResolveOffset(top, bottom, out this.modifyTopOffset);
            e.Handled = true;

            this.isDragInProgress = true;
        }
        #endregion

        #region OnPreviewMouseMove 
        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);
            if (this.ElementBeingDragged == null || !this.isDragInProgress)
                return;
            Point cursorLocation = e.GetPosition(this);
            double newHorizontalOffset, newVerticalOffset;

            #region Calculate Offsets 
            if (this.modifyLeftOffset)
                newHorizontalOffset = this.origHorizOffset + (cursorLocation.X - this.origCursorLocation.X);
            else
                newHorizontalOffset = this.origHorizOffset - (cursorLocation.X - this.origCursorLocation.X);
            if (this.modifyTopOffset)
                newVerticalOffset = this.origVertOffset + (cursorLocation.Y - this.origCursorLocation.Y);
            else
                newVerticalOffset = this.origVertOffset - (cursorLocation.Y - this.origCursorLocation.Y);

            #endregion

            if (!this.AllowDragOutOfView)
            {
                #region Verify Drag Element Location 

                Rect elemRect = this.CalculateDragElementRect(newHorizontalOffset, newVerticalOffset);
                bool leftAlign = elemRect.Left < 0;
                bool rightAlign = elemRect.Right > this.ActualWidth;

                if (leftAlign)
                    newHorizontalOffset = modifyLeftOffset ? 0 : this.ActualWidth - elemRect.Width;
                else if (rightAlign)
                    newHorizontalOffset = modifyLeftOffset ? this.ActualWidth - elemRect.Width : 0;

                bool topAlign = elemRect.Top < 0;
                bool bottomAlign = elemRect.Bottom > this.ActualHeight;

                if (topAlign)
                    newVerticalOffset = modifyTopOffset ? 0 : this.ActualHeight - elemRect.Height;
                else if (bottomAlign)
                    newVerticalOffset = modifyTopOffset ? this.ActualHeight - elemRect.Height : 0;

                #endregion
            }

            #region Move Drag Element 
            if (!(this.elementBeingDragged is Vertice))
            {
                if (this.modifyLeftOffset)
                    Canvas.SetLeft(this.ElementBeingDragged, newHorizontalOffset);
                else
                    Canvas.SetRight(this.ElementBeingDragged, newHorizontalOffset);

                if (this.modifyTopOffset)
                    Canvas.SetTop(this.ElementBeingDragged, newVerticalOffset);
                else
                    Canvas.SetBottom(this.ElementBeingDragged, newVerticalOffset);
            }
            else
            {
                Vertice bt = (Vertice)this.elementBeingDragged;
                if (this.modifyLeftOffset)
                    bt.X = (newHorizontalOffset);
                else
                    Canvas.SetRight(this.ElementBeingDragged, newHorizontalOffset);

                if (this.modifyTopOffset)
                    bt.Y = (newVerticalOffset);
                else
                    Canvas.SetBottom(this.ElementBeingDragged, newVerticalOffset);
            }

            #endregion
        }

        #endregion

        #region OnHostPreviewMouseUp 
        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);
            this.ElementBeingDragged = null;
        }

        #endregion
        #endregion
        #region Private Helpers 
        #region CalculateDragElementRect 
        private Rect CalculateDragElementRect(double newHorizOffset, double newVertOffset)
        {
            if (this.ElementBeingDragged == null)
                throw new InvalidOperationException("ElementBeingDragged is null.");
            Size elemSize = this.ElementBeingDragged.RenderSize;
            double x, y;
            if (this.modifyLeftOffset)
                x = newHorizOffset;
            else
                x = this.ActualWidth - newHorizOffset - elemSize.Width;
            if (this.modifyTopOffset)
                y = newVertOffset;
            else
                y = this.ActualHeight - newVertOffset - elemSize.Height;
            Point elemLoc = new Point(x, y);
            return new Rect(elemLoc, elemSize);
        }
        #endregion
        #region ResolveOffset      
        private static double ResolveOffset(double side1, double side2, out bool useSide1)
        {

            useSide1 = true;
            double result;
            if (Double.IsNaN(side1))
            {
                if (Double.IsNaN(side2))
                {
                    result = 0;
                }
                else
                {
                    result = side2;
                    useSide1 = false;
                }
            }
            else
            {
                result = side1;
            }
            return result;
        }
        #endregion
        #region UpdateZOrder 
        private void UpdateZOrder(UIElement element, bool bringToFront)
        {
            #region Safety Check 

            if (element == null)
                throw new ArgumentNullException("element");

            if (!base.Children.Contains(element))
                throw new ArgumentException("Must be a child element of the Canvas.", "element");

            #endregion

            #region Calculate Z-Indici And Offset 
            int elementNewZIndex = -1;
            if (bringToFront)
            {
                foreach (UIElement elem in base.Children)
                    if (elem.Visibility != Visibility.Collapsed)
                        ++elementNewZIndex;
            }
            else
            {
                elementNewZIndex = 0;
            }
            int offset = (elementNewZIndex == 0) ? +1 : -1;

            int elementCurrentZIndex = Canvas.GetZIndex(element);
            #endregion
            #region Update Z-Indici 
            foreach (UIElement childElement in base.Children)
            {
                if (childElement == element)
                    Canvas.SetZIndex(element, elementNewZIndex);
                else
                {
                    int zIndex = Canvas.GetZIndex(childElement);
                    if (bringToFront && elementCurrentZIndex < zIndex ||
                    !bringToFront && zIndex < elementCurrentZIndex)
                    {
                        Canvas.SetZIndex(childElement, zIndex + offset);
                    }
                }
            }
            #endregion
        }
        #endregion
        #endregion
    }
}
