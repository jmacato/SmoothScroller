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
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace SmoothScroller
{
    /// <summary>
    /// Panel that virtualizes child collection and supports smooth scrolling.
    /// </summary>
    public partial class SmoothPanel : Panel, ILogicalScrollable
    {
        static SmoothPanel()
        {
            AffectsRender<SmoothPanel>( BoundsProperty);
        }
        
        /// <summary>
        /// The line scroll value
        /// </summary>
        private const double LineScrollValue = 32;

        /// <summary>
        /// The wheel scroll value
        /// </summary>
        private const double WheelScrollValue = 64;

        /// <summary>
        /// The <see cref="SmoothPanelChildren"/>
        /// </summary>
        private readonly SmoothPanelChildren _children;

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
            AvaloniaProperty.Register<SmoothPanel, ObservableCollection<SmoothPanelTemplate>>(nameof(Templates),
                new ObservableCollection<SmoothPanelTemplate>());

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

            Templates.CollectionChanged += (sender, args) => TemplatesOnCollectionChanged();
        }

        private void TemplatesOnCollectionChanged()
        {
            _children.ResetViewCaches();
        }

        /// <summary>
        /// Causes the item to scroll into view.
        /// </summary>
        /// <param name="itemIndex">Index of the item.</param>
        public void ScrollIntoView(int itemIndex)
        {
            ScrollIntoView(itemIndex, null);
        }
        
        protected internal virtual void BringIndexIntoView(int index)
        {
            ScrollIntoView(index, null);
        }
 
        /// <summary>
        /// Causes the item to scroll into view.
        /// </summary>
        /// <param name="itemIndex">Index of the item.</param>
        /// <param name="afterScrollAction">An action that will be called after scrolling item into view.</param>
        public void ScrollIntoView(int itemIndex, Action<Control> afterScrollAction)
        {
            var items = _children.GetItems();

            if (items is {  } || itemIndex < 0 || itemIndex >= items.Count)
            {
                return;
            }

            var element = _children.GetElement(itemIndex);
            if (element != null && element.GetVisualParent() == this)
            {
                // Child already created, just ensure its visibility
                this.BringIntoView(element, new Rect(0, 0, element.Bounds.Width, element.Bounds.Height));
                if (afterScrollAction != null)
                {
                    afterScrollAction(element);
                }
            }
            else
            {
                // Scroll to specified item
                SetFirstVisibleItem(itemIndex, 0);
                InvalidateMeasure();

                if (afterScrollAction != null)
                {
                    // Item will be visible only after rearrangement, so use BeginInvoke to call the action.
                    Dispatcher.UIThread.Post(
                        () =>
                        {
                            var newElement = _children.GetElement(itemIndex);
                            if (newElement != null)
                            {
                                afterScrollAction(newElement);
                            }
                        },
                        DispatcherPriority.Loaded);
                }
            }
        }
        
        
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
 
                ScrollInvalidated?.Invoke(this, EventArgs.Empty);
 
                if (invalidateMeasure)
                {
                    // First item should be found by new scroll position
                    _firstItemIndex = -1;
                    InvalidateMeasure();
                }
            }
        }

        private bool isMeasureArrangeHappening = false;
        
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
                Control? element = _children.GetElement(index);

                if (element is not null)
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

            return finalSize;
        }

        protected override void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            
            // InvalidateMeasure();
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
                ScrollInvalidated?.Invoke(this, EventArgs.Empty);
            }
        }
        
        
        /// <summary>
        /// Forces content to scroll until the coordinate space of a <see cref="T:System.Windows.Media.Visual" /> object is visible.
        /// </summary>
        /// <param name="visual">A <see cref="T:System.Windows.Media.Visual" /> that becomes visible.</param>
        /// <param name="rectangle">A bounding rectangle that identifies the coordinate space to make visible.</param>
        /// <returns>
        /// A <see cref="T:System.Windows.Rect" /> that is visible.
        /// </returns>
        public bool BringIntoView(IControl target, Rect targetRect)
        {
            var topDelta = target.TransformToVisual(this)!.Value.Transform(targetRect.Position).Y;
            var bottomDelta = topDelta + targetRect.Height - _scrollViewport.Height;

            if (topDelta < 0)
            {
                // Top part is out of scroll
                SetVerticalOffset(_scrollOffset + topDelta, true);
            }
            else if (bottomDelta > 0)
            {
                // Bottom part is out of scroll
                SetVerticalOffset(_scrollOffset + bottomDelta, true);
            }

            return true;
        }

        public IControl? GetControlInDirection(NavigationDirection direction, IControl? from)
        {
            return null; 
        }

        public void RaiseScrollInvalidated(EventArgs e)
        { 
        }

        public bool CanHorizontallyScroll
        {
            get => false;
            set
            {
            }
        }

        public bool CanVerticallyScroll 
        {
            get => true;
            set
            {
            }
        }
        
        public bool IsLogicalScrollEnabled => true;

        public Size ScrollSize => new Size(_scrollExtent.Width, LineScrollValue);

        public Size PageScrollSize => new Size(_scrollExtent.Width, WheelScrollValue);

        public event EventHandler? ScrollInvalidated;
        
        public Size Extent => _scrollExtent;

        public Vector Offset
        {
            get => new Vector(0, _scrollOffset);
            set => SetVerticalOffset(value.Y, true);
        }

        public Size Viewport => _scrollViewport;
    }
}