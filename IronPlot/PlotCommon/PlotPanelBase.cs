﻿// Copyright (c) 2010 Joe Moorhouse

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Windows.Markup;
using System.Collections.Specialized;
using System.Collections;

namespace IronPlot
{
    /// <summary>
    /// Panel including basic functionality for Panels used in Plot2D and Plot3D.
    /// </summary>
    [ContentProperty("Annotations")]
    public class PlotPanelBase : Panel
    {
        // Annotation regions
        internal StackPanel AnnotationsLeft;
        internal StackPanel AnnotationsRight;
        internal StackPanel AnnotationsTop;
        internal StackPanel AnnotationsBottom;

        // Whether or not legends are shown:
        protected bool showAnnotationsLeft = false;
        protected bool showAnnotationsRight = false;
        protected bool showAnnotationsTop = false;
        protected bool showAnnotationsBottom = false;

        // Thickness required for the legends.
        protected Thickness legendRegion;
        
        // Location of just the axes region
        protected Rect axesRegionLocation;

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.RegisterAttached(
            "Position",
            typeof(Position),
            typeof(PlotPanelBase),
            new PropertyMetadata(Position.Right, OnPositionPropertyChanged));

        public PlotPanelBase()
            : base()
        {
            CreateLegends();
        }

        private static void OnPositionPropertyChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            PlotPanel parent = VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(element)) as PlotPanel;
            if (parent != null)
            {
                parent.AddOrRemoveAnnotation((UIElement)element, (Position)e.OldValue, AddOrRemove.Remove);
                parent.AddOrRemoveAnnotation((UIElement)element, (Position)e.NewValue, AddOrRemove.Add);
            }
        }

        public static Position GetPosition(UIElement element)
        {
            return (Position)element.GetValue(PositionProperty);
        }

        public static void SetPosition(UIElement element, Position position)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(PositionProperty, position);
        }

        public Collection<UIElement> Annotations
        {
            get { return annotations; }
            set { throw new NotSupportedException(); }
        }

        internal UniqueObservableCollection<UIElement> annotations = new UniqueObservableCollection<UIElement>();

        protected void annotations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (UIElement element in e.OldItems)
                {
                    AddOrRemoveAnnotation(element, (Position)element.GetValue(PositionProperty), AddOrRemove.Remove);
                }
            }
            if (e.NewItems != null)
            {
                foreach (UIElement element in e.NewItems)
                {
                    AddOrRemoveAnnotation(element, (Position)element.GetValue(PositionProperty), AddOrRemove.Add);
                }
            }
        }

        protected enum AddOrRemove { Add, Remove };
        protected void AddOrRemoveAnnotation(UIElement element, Position position, AddOrRemove action)
        {
            StackPanel parent;
            switch (position)
            {
                case Position.Left:
                    parent = AnnotationsLeft;
                    if (element is FrameworkElement)
                    {
                        (element as FrameworkElement).HorizontalAlignment = HorizontalAlignment.Right;
                        (element as FrameworkElement).VerticalAlignment = VerticalAlignment.Center;
                    }
                    break;
                case Position.Right:
                    parent = AnnotationsRight;
                    if (element is FrameworkElement)
                    {
                        (element as FrameworkElement).HorizontalAlignment = HorizontalAlignment.Left;
                        (element as FrameworkElement).VerticalAlignment = VerticalAlignment.Center;
                    }
                    break;
                case Position.Top:
                    parent = AnnotationsTop;
                    if (element is FrameworkElement)
                    {
                        (element as FrameworkElement).HorizontalAlignment = HorizontalAlignment.Center;
                        (element as FrameworkElement).VerticalAlignment = VerticalAlignment.Bottom;
                    }
                    break;
                default:
                    parent = AnnotationsBottom;
                    if (element is FrameworkElement)
                    {
                        (element as FrameworkElement).HorizontalAlignment = HorizontalAlignment.Center;
                        (element as FrameworkElement).VerticalAlignment = VerticalAlignment.Top;
                    }
                    break;

            }
            if (action == AddOrRemove.Add)
                parent.Children.Add(element);
            else parent.Children.Remove(element);
        }

        protected virtual void CreateLegends()
        {
            annotations.CollectionChanged += new NotifyCollectionChangedEventHandler(annotations_CollectionChanged);
            
            // Left annotations
            AnnotationsLeft = new StackPanel();
            AnnotationsLeft.Orientation = Orientation.Horizontal;
            //AnnotationsLeft.FlowDirection = FlowDirection.RightToLeft;
            this.Children.Add(AnnotationsLeft);
            //
            // Right legend
            AnnotationsRight = new StackPanel();
            AnnotationsRight.Orientation = Orientation.Horizontal;
            this.Children.Add(AnnotationsRight);
            //
            // Top legend
            AnnotationsTop = new StackPanel();
            this.Children.Add(AnnotationsTop);
            //
            // Bottom legend
            AnnotationsBottom = new StackPanel();
            this.Children.Add(AnnotationsBottom);
            //
        }

        /// <summary>
        /// Determine whether annotations are shown and return size of region required for legends.
        /// Note that this process is required for Measure and possible for Arrange passes.
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns></returns>
        protected Rect PlaceAnnotations(Size availableSize)
        {
            showAnnotationsLeft = false;
            showAnnotationsRight = false;
            showAnnotationsTop = false;
            showAnnotationsBottom = false;
            double startX = 0; double startY = 0;
            double endX = availableSize.Width; double endY = availableSize.Height;
            legendRegion = new Thickness();

            startX += AnnotationsLeft.DesiredSize.Width;
            legendRegion.Left += AnnotationsLeft.DesiredSize.Width;
            endX -= AnnotationsRight.DesiredSize.Width;
            legendRegion.Right += AnnotationsRight.DesiredSize.Width;
            startY += AnnotationsTop.DesiredSize.Height;
            legendRegion.Top += AnnotationsTop.DesiredSize.Height;
            endY -= AnnotationsBottom.DesiredSize.Height;
            legendRegion.Bottom += AnnotationsBottom.DesiredSize.Height;
            showAnnotationsLeft = showAnnotationsRight = showAnnotationsTop = showAnnotationsBottom = true;

            //if ((endX - startX) > (AnnotationsLeft.DesiredSize.Width + 1))
            //{
            //    showAnnotationsLeft = true;
            //    startX += AnnotationsLeft.DesiredSize.Width;
            //    legendRegion.Left += AnnotationsLeft.DesiredSize.Width;
            //}
            //if ((endX - startX) > (AnnotationsRight.DesiredSize.Width + 1))
            //{
            //    showAnnotationsRight = true;
            //    endX -= AnnotationsRight.DesiredSize.Width;
            //    legendRegion.Right += AnnotationsRight.DesiredSize.Width;            
            //}
            //if ((endY - startY) > (AnnotationsTop.DesiredSize.Height + 1))
            //{
            //    showAnnotationsTop = true;
            //    startY += AnnotationsTop.DesiredSize.Height;
            //    legendRegion.Top += AnnotationsTop.DesiredSize.Height;
            //}
            //if ((endY - startY) > (AnnotationsBottom.DesiredSize.Height + 1))
            //{
            //    showAnnotationsBottom = true;
            //    endY -= AnnotationsBottom.DesiredSize.Height;
            //    legendRegion.Bottom += AnnotationsBottom.DesiredSize.Height;
            //}
            Rect available = new Rect(startX, startY, Math.Max(endX - startX, 1), Math.Max(endY - startY, 1)); // new Rect(startX, 0, endX - startX, endY - startY);
            return available;
        }

        /// <summary>
        /// Arrange the annotations. Note that these are arranged around the axes region:
        /// axesRegionLocation is used for this (therefore must be correctly set when this is called).
        /// </summary>
        internal void ArrangeAnnotations(Size finalSize)
        {
            if (showAnnotationsLeft)
            {
                //Rect annotationsLeftRect = new Rect(new Point(axesRegionLocation.Left - AnnotationsLeft.DesiredSize.Width, axesRegionLocation.Top),
                //    new Point(axesRegionLocation.Left, axesRegionLocation.Bottom));
                Rect annotationsLeftRect = new Rect(new Point(0, 0),
                    new Point(axesRegionLocation.Left, finalSize.Height));
                AnnotationsLeft.Arrange(annotationsLeftRect);
            }
            if (showAnnotationsRight)
            {
                //Rect annotationsRightRect = new Rect(new Point(axesRegionLocation.Right, axesRegionLocation.Top),
                //    new Point(axesRegionLocation.Right + AnnotationsRight.DesiredSize.Width, axesRegionLocation.Bottom));
                Rect annotationsRightRect = new Rect(new Point(axesRegionLocation.Right, 0),
                    new Point(finalSize.Width, finalSize.Height));
                AnnotationsRight.Arrange(annotationsRightRect);
            }
            else AnnotationsRight.Arrange(new Rect());
            if (showAnnotationsTop)
            {
                //Rect annotationsTopRect = new Rect(new Point(axesRegionLocation.Left, axesRegionLocation.Top - AnnotationsTop.DesiredSize.Height),
                //    new Point(axesRegionLocation.Right, axesRegionLocation.Top));
                Rect annotationsTopRect = new Rect(new Point(0, 0),
                    new Point(finalSize.Width, axesRegionLocation.Top));
                AnnotationsTop.Arrange(annotationsTopRect);
            }
            if (showAnnotationsBottom)
            {
                //Rect annotationsBottomRect = new Rect(new Point(axesRegionLocation.Left, axesRegionLocation.Bottom),
                //    new Point(axesRegionLocation.Right, axesRegionLocation.Bottom + AnnotationsBottom.DesiredSize.Height));
                Rect annotationsBottomRect = new Rect(new Point(0, axesRegionLocation.Bottom),
                    new Point(finalSize.Width, finalSize.Height));
                AnnotationsBottom.Arrange(annotationsBottomRect);
            }
        }

    }
}