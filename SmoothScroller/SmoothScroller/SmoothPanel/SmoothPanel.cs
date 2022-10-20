// --------------------------------------------------------------------------
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// --------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics; 
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Threading;
using Devart.Controls;
using DynamicData.Binding;

namespace Devart.Controls
{
    /// <summary>
    /// Panel that virtualizes child collection and supports smooth scrolling.
    /// </summary>
    public partial class SmoothPanel : VirtualizingPanel, IScrollable
    {
        // /// <summary>
        // /// Using a AvaloniaProperty as the backing store for Templates.
        // /// </summary>
        // public static readonly AvaloniaProperty TemplatesProperty;

        // /// <summary>
        // /// The line scroll value
        // /// </summary>
        // private const double LineScrollValue = 16;
        //
        // /// <summary>
        // /// The wheel scroll value
        // /// </summary>
        // private const double WheelScrollValue = 64;

        /// <summary>
        /// Dependency property identifier for limited write access to a Templates property.
        /// </summary>
        // private static readonly AvaloniaPropertyKey _templatesPropertyKey;

        /// <summary>
        /// The <see cref="SmoothPanelChildren"/>
        /// </summary>
        private readonly SmoothPanelChildren _children;

        /// <summary>
        /// A <see cref="T:ScrollViewer" /> element that controls scrolling behavior.
        /// </summary>
        private ScrollViewer _scrollOwner;

        /// <summary>
        /// The size of scroll extent.
        /// </summary>
        private Size _scrollExtent;

        /// <summary>
        /// The size of the viewport.
        /// </summary>
        private Size _scrollViewport;

        /// <summary>
        /// The vertical offset of the scrolled content.
        /// </summary>
        private double _scrollOffset;

        /// <summary>
        /// The first visible item index.
        /// </summary>
        private int _firstItemIndex;

        /// <summary>
        /// The ratio of clipped part of first visible item to its height.
        /// </summary>
        private double _firstItemClippedRatio;
 

        public static readonly StyledProperty<ObservableCollection<SmoothPanelTemplate>> TemplatesProperty =
            AvaloniaProperty.Register<SmoothPanel, ObservableCollection<SmoothPanelTemplate>>(nameof(Templates), new ObservableCollection<SmoothPanelTemplate>());

        public ObservableCollection<SmoothPanelTemplate> Templates   
        {
            get => GetValue(TemplatesProperty);
            set => SetValue(TemplatesProperty, value);
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SmoothPanel"/> class.
        /// </summary>
        public SmoothPanel() : base()
        { 
             _children = new SmoothPanelChildren(this);
            
            Templates.CollectionChanged += (sender, args) =>  TemplatesOnCollectionChanged();
        }
 

        private void TemplatesOnCollectionChanged()
        {
            _children.ResetViewCaches();
        }

        // /// <summary>
        // /// Gets the templates.
        // /// </summary>
        // /// <value>
        // /// The templates.
        // /// </value>
        // public Collection<SmoothPanelTemplate> Templates
        // {
        //     get
        //     {
        //         return (Collection<SmoothPanelTemplate>)GetValue(TemplatesProperty);
        //     }
        // }
        //
        // /// <summary>
        // /// Causes the item to scroll into view.
        // /// </summary>
        // /// <param name="itemIndex">Index of the item.</param>
        // public void ScrollIntoView(int itemIndex)
        // {
        //     ScrollIntoView(itemIndex, null);
        // }
        //
        // /// <summary>
        // /// Causes the item to scroll into view.
        // /// </summary>
        // /// <param name="itemIndex">Index of the item.</param>
        // /// <param name="afterScrollAction">An action that will be called after scrolling item into view.</param>
        // public void ScrollIntoView(int itemIndex, Action<Control> afterScrollAction)
        // {
        //     var items = _children.GetItems();
        //
        //     if (items == null || itemIndex < 0 || itemIndex >= items.Count)
        //     {
        //         return;
        //     }
        //
        //     var element = _children.GetElement(itemIndex);
        //     if (element != null && System.Windows.Media.VisualTreeHelper.GetParent(element) == this)
        //     {
        //         // Child already created, just ensure its visibility
        //         ((IScrollInfo)this).MakeVisible(element, new Rect(0, 0, element.ActualWidth, element.ActualHeight));
        //         if (afterScrollAction != null)
        //         {
        //             afterScrollAction(element);
        //         }
        //     }
        //     else
        //     {
        //         // Scroll to specified item
        //         SetFirstVisibleItem(itemIndex, 0);
        //         InvalidateMeasure();
        //
        //         if (afterScrollAction != null)
        //         {
        //             // Item will be visible only after rearrangement, so use BeginInvoke to call the action.
        //             Dispatcher.BeginInvoke(
        //                 (Action)(() =>
        //                 {
        //                     var newElement = _children.GetElement(itemIndex);
        //                     if (newElement != null)
        //                     {
        //                         afterScrollAction(newElement);
        //                     }
        //                 }),
        //                 DispatcherPriority.Loaded);
        //         }
        //     }
        // }

        /// <summary>
        /// Sets the vertical offset.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="invalidateMeasure">If set to <c>true</c> measure will be invalidated.</param>
        public void SetVerticalOffset(double offset, bool invalidateMeasure)
        {
            // Limit offset value
            if (offset < 0 || _scrollViewport.Height >= _scrollExtent.Height)
            {
                offset = 0;
            }
            else if (offset > _scrollExtent.Height - _scrollViewport.Height)
            {
                offset = _scrollExtent.Height - _scrollViewport.Height;
            }

            if (Math.Abs(offset - _scrollOffset) > double.Epsilon)
            {
                _scrollOffset = offset;

                if (_scrollOwner != null)
                {
                    // Update scrollbar
                    _scrollOwner.InvalidateVisual();
                }

                if (invalidateMeasure)
                {
                    // First item should be found by new scroll position
                    _firstItemIndex = -1;
                    InvalidateMeasure();
                }
            }
        }

        /// <summary>
        /// When overridden in a derived class, measures the size in layout required for child elements and
        /// determines a size for the <see cref="T:System.Windows.Control" />-derived class.
        /// </summary>
        /// <param name="availableSize">The available size that this element can give to child elements.</param>
        /// <returns>
        /// The size that this element determines it needs during layout, based on its calculations of child element sizes.
        /// </returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (double.IsInfinity(availableSize.Width))
            {
                Debug.Assert(false, "Infinite width is not supported");
                availableSize = new Size(100, availableSize.Height);
            }

            if (double.IsInfinity(availableSize.Height))
            {
                Debug.Assert(false, "Infinite height is not supported");
                availableSize = new Size(availableSize.Width, 100);
            }

            _children.AvailableWidth = availableSize.Width;

            var measurer = new SmoothPanelMeasurer(this, availableSize);
            measurer.Measure();

            return availableSize;
        }

        /// <summary>
        /// When overridden in a derived class, positions child elements and determines a size for
        /// a <see cref="T:System.Windows.Control" /> derived class.
        /// </summary>
        /// <param name="finalSize">
        /// The final area within the parent that this element should use to arrange itself and its children.
        /// </param>
        /// <returns>
        /// The actual size used.
        /// </returns>
        ///
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_children.ItemsCount == 0)
            {
                return finalSize;
            }

            double width = finalSize.Width;
            double top = 0;
            bool isFirst = true;

            if (_firstItemIndex < 0)
            {
                Debug.Assert(false, "First visible item should be specified before arrangement.");
                SetFirstVisibleItem(0, 0);
                SetVerticalOffset(0, false);
            }

            for (int index = _firstItemIndex; index < _children.ItemsCount; index++)
            {
                Control element = _children.GetElement(index);

                if (element != null)
                {
                    double height = element.DesiredSize.Height;

                    if (isFirst)
                    {
                        // First visible item can be clipped
                        top = -height * _firstItemClippedRatio;
                        isFirst = false;
                    }

                    element.Arrange(new Rect(0, top, width, height));

                    top += height;

                    if (top >= finalSize.Height)
                    {
                        // Out of view
                        break;
                    }
                }
            }
            
            Debug.WriteLine($"{_children.GetRealizedElements} Realized Elements. {_children.GetTotalElements} Total Items.");

            return finalSize;
        }

        protected override void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // _children.ResetItems();
            
            InvalidateMeasure();
            base.ChildrenChanged(sender, e);
        }
 
        /// <summary>
        /// Sets the first visible item.
        /// </summary>
        /// <param name="index">The item index.</param>
        /// <param name="clippedRatio">The ratio of clipped part to entire height.</param>
        private void SetFirstVisibleItem(int index, double clippedRatio)
        {
            _firstItemIndex = index;
            _firstItemClippedRatio = clippedRatio;
        }

        /// <summary>
        /// Updates the scroll information.
        /// </summary>
        /// <param name="newScrollViewport">The new scroll viewport.</param>
        /// <param name="totalItemsHeight">Total height of the items.</param>
        private void UpdateScrollInfo(Size newScrollViewport, double totalItemsHeight)
        {
            var newScrollExtent = new Size(newScrollViewport.Width, totalItemsHeight);

            if (newScrollExtent != _scrollExtent || newScrollViewport != _scrollViewport)
            {
                _scrollExtent = newScrollExtent;
                _scrollViewport = newScrollViewport;
                double maxOffset = _scrollExtent.Height - _scrollViewport.Height;
                _scrollOffset = Math.Max(0, Math.Min(_scrollOffset, maxOffset));

                if (_scrollOwner != null)
                {
                    _scrollOwner.InvalidateVisual();
                }
            }
        }

        public Size Extent
        {
            get => _scrollExtent;
        }
        public Vector Offset
        {
            get => new Vector(0, _scrollOffset);
            set => SetVerticalOffset(value.Y, true);
        }

        public Size Viewport
        {
            get => _scrollViewport;
        }
    }
}