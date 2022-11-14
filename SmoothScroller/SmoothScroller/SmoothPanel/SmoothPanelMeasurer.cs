using System;
using System.Collections;
using System.Diagnostics;
using Avalonia;

namespace SmoothScroller
{
    /// <content>
    /// Nested type is used.
    /// </content>
    public partial class SmoothPanel
    {
        public static readonly StyledProperty<bool> AutoScrollOnNewItemProperty = AvaloniaProperty.Register<SmoothPanel, bool>("AutoScrollOnNewItem");

        /// <summary>
        /// Temporary instances of this type are used for measuring in
        /// <see cref="M:SmoothPanel.MeasureOverride"/> method.
        /// </summary>
        private class SmoothPanelMeasurer
        {
            /// <summary>
            /// The panel.
            /// </summary>
            private readonly SmoothPanel _panel;

            /// <summary>
            /// The collection of children.
            /// </summary>
 
            /// <summary>
            /// The view model collection.
            /// </summary>
 
            /// <summary>
            /// The available size.
            /// </summary>
 
            /// <summary>
            /// Value indicating whether first visible item should remain in its position or first
            /// item should be based on scroll position.
            /// </summary>
 
            /// <summary>
            /// The total estimated height of all visual elements.
            /// </summary>
            private double _totalHeight;

            /// <summary>
            /// The last visible item index.
            /// </summary>
 
            /// <summary>
            /// The ratio of clipped part of last visible item to its height.
            /// </summary>
            private double _lastItemClippedRatio;

            private bool _keepFirstItem;
            private int _lastItemIndex;
            private Size _availableSize;

            /// <summary>
            /// Initializes a new instance of the <see cref="SmoothPanelMeasurer"/> class.
            /// </summary>
            /// <param name="panel">The panel.</param>
            /// <param name="availableSize">The available size.</param>
            public SmoothPanelMeasurer(SmoothPanel panel)
            {
                _panel = panel;
            }
            
            /// <summary>
            /// Gets the index of first visible item.
            /// </summary>
            /// <value>
            /// The index of first visible item.
            /// </value>
            private int FirstItemIndex => _panel._firstItemIndex;

            /// <summary>
            /// Gets the ratio of clipped part of last visible item to its height.
            /// </summary>
            /// <value>
            /// The ratio of clipped part of last visible item to its height.
            /// </value>
            private double FirstItemClippedRatio => _panel._firstItemClippedRatio;

            /// <summary>
            /// Gets or sets the reverse vertical offset of the scrolled content.
            /// </summary>
            /// <value>
            /// The reverse vertical scroll offset.
            /// </value>
            private double ReverseScrollOffset
            {
                get => _panel._scrollExtent.Height - _availableSize.Height - _panel._scrollOffset;
                set
                {
                    var offset = _panel._scrollExtent.Height - _availableSize.Height - value;
                    _panel.SetVerticalOffset(offset, false);
                }
            }

            /// <summary>
            /// Gets or sets the vertical offset of the scrolled content.
            /// </summary>
            /// <value>
            /// The vertical scroll offset.
            /// </value>
            private double ScrollOffset
            {
                get => _panel._scrollOffset;
                set => _panel.SetVerticalOffset(value, false);
            }

            /// <summary>
            /// Measures child elements and prepares panel layout.
            /// </summary>
            public void Measure(Size availableSize)
            {
                
                
                
                    var _items = _panel._children.GetItems();
                 _keepFirstItem = FirstItemIndex >= 0;
                _lastItemIndex = -1;
                _availableSize = availableSize;
                
                
                // Some unexpected artifacts are better than infinite loop.
                var lastChance = false;
                int lastIndex;
                do
                {
                    if (_items == null || _items.Count == 0)
                    {
                        // No items - do nothing
                        _panel.UpdateScrollInfo(_availableSize, 0);
                        return;
                    }

                    _panel._children.CreateTopmostElements(_availableSize);

                    _totalHeight = GetTotalHeight(_items, out _lastItemIndex, out _lastItemClippedRatio);

                    if (!_keepFirstItem && _lastItemIndex >= 0)
                    {
                        // Get first visible item by last one
                        GetFirstItem(_items);
                    }

                    if (FirstItemIndex < 0)
                    {
                        Debug.Assert(false, "First visible item should be determined");

                        // Some unexpected result - just reset to top.
                        _panel.SetFirstVisibleItem(0, 0);
                        _panel.SetVerticalOffset(0, false);
                    }

                    // Generate all visible items.
                    double bottom;
                    GenerateChildren(_items, out lastIndex, out bottom);

                    var scrollChanged = false;

                    var extent = _panel._scrollExtent.Height;
                    var viewPort = _panel._scrollViewport.Height;

                    if (extent < viewPort)
                    {
                        if (FirstItemIndex != 0 || FirstItemClippedRatio != 0 || ScrollOffset != 0)
                        {
                            // Scrollbar disappears
                            _panel.SetFirstVisibleItem(0, 0);
                            _panel.SetVerticalOffset(0, false);
                            scrollChanged = true;
                        }
                    }
                    else
                    {
                        if (_lastItemIndex < 0)
                        {
                            if (extent > viewPort)
                            {
                                // Prevent precision problems
                                if (Math.Abs(bottom - viewPort) >= 0.5)
                                {
                                    ReverseScrollOffset = 0;
                                    _keepFirstItem = false;
                                    scrollChanged = true;
                                }
                            }
                        }
                        else if (_keepFirstItem)
                        {
                            // Scroll position can be changed when first visible item is fixed.
                            double newReverseScrollOffset = 0;
                            if (lastIndex < _items.Count)
                            {
                                newReverseScrollOffset = _lastItemClippedRatio * _panel._children.GetEstimatedHeight(lastIndex);
                                for (var i = lastIndex + 1; i < _items.Count; i++)
                                {
                                    newReverseScrollOffset +=_panel. _children.GetEstimatedHeight(i);
                                }
                            }

                            var oldScrollOffset = ScrollOffset;
                            ReverseScrollOffset = newReverseScrollOffset;
                            scrollChanged = oldScrollOffset != ScrollOffset;
                        }
                    }

                    if (!scrollChanged)
                    {
                        break;
                    }

                    // Recalculate positions if scroll changed.
                    lastChance = !lastChance;
                    Debug.Assert(lastChance, "SmoothPanel measuring failed.");
                }
                while (lastChance);

                // Remove odd children.
                _panel._children.TrimElements(FirstItemIndex, lastIndex);
            }

            /// <summary>
            /// Generates the children.
            /// </summary>
            /// <param name="lastIndex">The last index.</param>
            /// <param name="bottom">The bottom.</param>
            private void GenerateChildren(IList _items, out int lastIndex, out double bottom)
            {
                _lastItemIndex = -1;
                double top = 0;
                int itemIndex;
                for (itemIndex = FirstItemIndex; itemIndex < _items.Count; itemIndex++)
                {
                    var child = _panel._children.GetMeasuredChild(_items, itemIndex);

                    if (itemIndex == FirstItemIndex)
                    {
                        top = -child.DesiredSize.Height * FirstItemClippedRatio;
                    }

                    var childHeight = child.DesiredSize.Height;
                    top += childHeight;
                    if (top >= _availableSize.Height)
                    {
                        _lastItemIndex = itemIndex;
                        _lastItemClippedRatio = (top - _availableSize.Height) / childHeight;
                        break;
                    }
                }
                bottom = top;
                lastIndex = itemIndex;
            }

            /// <summary>
            /// Gets the first item by last one.
            /// </summary>
            private void GetFirstItem(IList _items)
            {
                double bottomHeight = 0;
                for (var i = _lastItemIndex; i >= 0; i--)
                {
                    var child = _panel._children.GetMeasuredChild(_items, i);
                    var childHeight = child.DesiredSize.Height;
                    if (i == _lastItemIndex)
                    {
                        // Consider clipping part
                        bottomHeight = -childHeight * _lastItemClippedRatio;
                    }
                    bottomHeight += childHeight;
                    if (bottomHeight >= _availableSize.Height)
                    {
                        // All above elements are out of visible area
                        _panel.SetFirstVisibleItem(i, (bottomHeight - _availableSize.Height) / childHeight);
                        return;
                    }
                }

                _panel.SetFirstVisibleItem(0, 0);
            }

            /// <summary>
            /// Gets the total height.
            /// </summary>
            /// <param name="lastItemIndex">Last visible item index.</param>
            /// <param name="lastItemClippedRatio">The ratio of clipped part of last visible item to its height.</param>
            /// <returns>A <see cref="double"/> that represents a total estimated height of all children.</returns>
            private double GetTotalHeight(IList _items, out int lastItemIndex, out double lastItemClippedRatio)
            {
                double totalHeight = 0;
                if (_keepFirstItem)
                {
                    // No need to determine last visible item
                    for (var i = 0; i < _items.Count; i++)
                    {
                        var itemHeight = _panel._children.GetEstimatedHeight(i);
                        totalHeight += itemHeight;
                    }
                    _panel.UpdateScrollInfo(_availableSize, totalHeight);

                    lastItemIndex = -1;
                    lastItemClippedRatio = 0;
                    return totalHeight;
                }

                var lastChance = false;
                do
                {
                    totalHeight = 0;
                    lastItemIndex = -1;
                    lastItemClippedRatio = 0;

                    var reverseScrollOffset = ReverseScrollOffset;

                    // Determine last visible item
                    for (var i = _items.Count - 1; i >= 0; i--)
                    {
                        var itemHeight = _panel._children.GetEstimatedHeight(i);
                        totalHeight += itemHeight;
                        if (lastItemIndex < 0 && totalHeight > reverseScrollOffset)
                        {
                            lastItemIndex = i;
                            lastItemClippedRatio = Math.Max(0, 1 - ((totalHeight - reverseScrollOffset) / itemHeight));
                        }
                    }

                    // Scrolled to bottom
                    if (lastItemIndex < 0)
                    {
                        lastItemIndex = _items.Count - 1;
                        lastItemClippedRatio = 0;
                    }

                    var oldScrollOffset = ScrollOffset;
                    _panel.UpdateScrollInfo(_availableSize, totalHeight);
                    if (oldScrollOffset == _panel._scrollOffset)
                    {
                        break;
                    }
                    lastChance = !lastChance;
                    Debug.Assert(lastChance, "Total height is not determined.");
                }
                while (lastChance);

                return totalHeight;
            }
        }

        public bool     AutoScrollOnNewItem
        {
            get { return (bool)GetValue(AutoScrollOnNewItemProperty); }
            set { SetValue(AutoScrollOnNewItemProperty, value); }
        }
    }
}