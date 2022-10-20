 

using System;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.VisualTree;

namespace Devart.Controls;

/// <summary>
///     A base class that provides access to information that is useful for panels that with to implement virtualization.
/// </summary>
public abstract class VirtualizingPanel : Panel
{
    static VirtualizingPanel()
    {
        AffectsMeasure<VirtualizingPanel>(VirtualizationModeProperty, CacheLengthUnitProperty );
    }

    /// <summary>
    ///     The default constructor.
    /// </summary>
    protected VirtualizingPanel() : base()
    {
    }

    public bool CanHierarchicallyScrollAndVirtualize
    {
        get { return CanHierarchicallyScrollAndVirtualizeCore; }
    }

    protected virtual bool CanHierarchicallyScrollAndVirtualizeCore
    {
        get { return false; }
    }

    public double GetItemOffset(Control child)
    {
        return GetItemOffsetCore(child);
    }

    /// <summary>
    ///     Fetch the logical/item offset for this child with respect to the top of the
    ///     panel. This is similar to a TransformToAncestor operation. Just works
    ///     in logical units.
    /// </summary>
    protected virtual double GetItemOffsetCore(Control child)
    {
        return 0;
    }


    public static readonly AttachedProperty<bool> IsVirtualizingProperty =
        AvaloniaProperty.RegisterAttached<IAvaloniaObject, bool>("IsVirtualizing", typeof(VirtualizingPanel), true);

    public static bool GetIsVirtualizing(IAvaloniaObject obj)
    {
        return (bool)obj.GetValue(IsVirtualizingProperty);
    }

    public static void SetIsVirtualizing(IAvaloniaObject obj, bool value)
    {
        obj.SetValue(IsVirtualizingProperty, value);
    }



    /// <summary>
    ///     Attached property for use on the ItemsControl that is the host for the items being
    ///     presented by this panel. Use this property to modify the virtualization mode.
    ///
    ///     Note that this property can only be set before the panel has been initialized
    /// </summary>
    public static readonly AttachedProperty<VirtualizationMode> VirtualizationModeProperty =
        AvaloniaProperty.RegisterAttached<IAvaloniaObject, VirtualizationMode>("VirtualizationMode",
            typeof(VirtualizingPanel));

    public static VirtualizationMode GetVirtualizationMode(IAvaloniaObject obj)
    {
        return (VirtualizationMode)obj.GetValue(VirtualizationModeProperty);
    }

    public static void SetVirtualizationMode(IAvaloniaObject obj, VirtualizationMode value)
    {
        obj.SetValue(VirtualizationModeProperty, value);
    }

    /// <summary>
    ///     Attached property for use on the ItemsControl that is the host for the items being
    ///     presented by this panel. Use this property to turn virtualization on/off when grouping.
    /// </summary>
    /// 
    public static readonly AttachedProperty<bool> IsVirtualizingWhenGroupingProperty =
        AvaloniaProperty.RegisterAttached<IAvaloniaObject, bool>("IsVirtualizingWhenGrouping",
            typeof(VirtualizingPanel));

    public static bool GetIsVirtualizingWhenGrouping(IAvaloniaObject obj)
    {
        return (bool)obj.GetValue(IsVirtualizingWhenGroupingProperty);
    }

    public static void SetIsVirtualizingWhenGrouping(IAvaloniaObject obj, bool value)
    {
        obj.SetValue(IsVirtualizingWhenGroupingProperty, value);
    }

    /// <summary>
    ///     Attached property for use on the ItemsControl that is the host for the items being
    ///     presented by this panel. Use this property to switch between pixel and item scrolling.
    /// </summary>

    public static readonly AttachedProperty<ScrollUnit> ScrollUnitProperty =
        AvaloniaProperty.RegisterAttached<IAvaloniaObject, ScrollUnit>("ScrollUnit", typeof(VirtualizingPanel));


    public static ScrollUnit GetScrollUnit(IAvaloniaObject obj)
    {
        return (ScrollUnit)obj.GetValue(ScrollUnitProperty);
    }

    public static void SetScrollUnit(IAvaloniaObject obj, ScrollUnit value)
    {
        obj.SetValue(ScrollUnitProperty, value);
    }

    /// <summary>
    ///     Attached property for use on the ItemsControl that is the host for the items being
    ///     presented by this panel. Use this property to configure the dimensions of the cache
    ///     before and after the viewport when virtualizing. Please note that the unit of these dimensions
    ///     is determined by the value of the <see cref="CacheLengthUnitProperty"/>.
    /// </summary>
    public static readonly AttachedProperty<VirtualizationCacheLength> CacheLengthProperty =
        AvaloniaProperty.RegisterAttached<IAvaloniaObject, VirtualizationCacheLength>("CacheLength",
            typeof(VirtualizingPanel), new VirtualizationCacheLength(1.0));


    public static VirtualizationCacheLength GetCacheLength(IAvaloniaObject obj)
    {
        return (VirtualizationCacheLength)obj.GetValue(CacheLengthProperty);
    }

    public static void SetCacheLength(IAvaloniaObject obj, VirtualizationCacheLength value)
    {
        obj.SetValue(CacheLengthProperty, value);
    }

    /// <summary>
    ///     Attached property for use on the ItemsControl that is the host for the items being
    ///     presented by this panel. Use this property to configure the unit portion of the before
    ///     and after cache sizes.
    /// </summary>
    public static readonly AttachedProperty<VirtualizationCacheLengthUnit> CacheLengthUnitProperty =
        AvaloniaProperty.RegisterAttached<IAvaloniaObject, VirtualizationCacheLengthUnit>("CacheLengthUnit", typeof(VirtualizingPanel), VirtualizationCacheLengthUnit.Page);

    public static VirtualizationCacheLengthUnit GetCacheLengthUnit(IAvaloniaObject obj)
    {
        return (VirtualizationCacheLengthUnit)obj.GetValue(CacheLengthUnitProperty);
    }

    public static void SetCacheLengthUnit(IAvaloniaObject obj, VirtualizationCacheLengthUnit value)
    {
        obj.SetValue(CacheLengthUnitProperty, value);
    }




    /// <summary>
    ///     Attached property for use on a container being presented by this panel. The parent panel
    ///     is expected to honor this property and not virtualize containers that are designated non-virtualizable.
    /// </summary>
    public static readonly AttachedProperty<bool> IsContainerVirtualizableProperty =
        AvaloniaProperty.RegisterAttached<VirtualizingPanel, VirtualizingPanel, bool>("IsContainerVirtualizable", true);

    public static void SetIsContainerVirtualizable(VirtualizingPanel obj, bool value) => obj.SetValue(IsContainerVirtualizableProperty, value);
    public static bool GetIsContainerVirtualizable(VirtualizingPanel obj) => obj.GetValue(IsContainerVirtualizableProperty);
     

    /// <summary>
    ///     Attached property for use on a container being presented by this panel. The parent panel
    ///     is expected to honor this property and not cache container sizes that are designated such.
    /// </summary>
    public static readonly AttachedProperty<bool> ShouldCacheContainerSizeProperty =
        AvaloniaProperty.RegisterAttached<VirtualizingPanel, VirtualizingPanel, bool>("ShouldCacheContainerSize", true);

    public static void SetShouldCacheContainerSize(VirtualizingPanel obj, bool value) => obj.SetValue(ShouldCacheContainerSizeProperty, value);

    public static bool GetShouldCacheContainerSize(VirtualizingPanel obj)
    {
        
        if (obj == null)
        {
            throw new ArgumentNullException("element");
        }

        
            // this property can cause infinite loops.  Suppose element X sets this
            // to false, so that we don't cache the size of X.  When X leaves
            // the viewport, we will estimate its size using the average container
            // size (that average doesn't include X).  When it returns, we will
            // use the actual size.  This difference can cause infinite re-measure
            // or bad scroll result (scroll to the wrong offset) when X is near
            // the edge of the viewport.
            //
            // The property is only set on the DataGridRow that hosts the
            // NewItemPlaceholder.  The intent was to avoid treating a
            // DataGrid as having non-uniform containers only on account of the
            // NewItem row.   While this helps a common case (a non-grouped
            // DataGrid whose containers are all the same, except the placeholder
            // which is different), it doesn't justify breaking other cases.
            //
            // Ignore the value (always return true).  This fixes the loops and
            // bad scrolls, and only increases perf in the case mentioned above,
            // and even then only memory consumption (hashtable lookup is O(1)),
            // and only proportional to the number of items the user actually
            // scrolls into view.
            return true;
        
    } 
     
    
    // private static bool ValidateCacheSizeBeforeOrAfterViewport(object value)
    // {
    //     VirtualizationCacheLength cacheLength = (VirtualizationCacheLength)value;
    //     return DoubleUtil.GreaterThanOrClose(cacheLength.CacheBeforeViewport, 0.0) &&
    //            DoubleUtil.GreaterThanOrClose(cacheLength.CacheAfterViewport, 0.0);
    // }
    //
    // private static object CoerceIsVirtualizingWhenGrouping(DependencyObject d, object baseValue)
    // {
    //     bool isVirtualizing = GetIsVirtualizing(d);
    //     return isVirtualizing && (bool)baseValue;
    // }

    // internal static void OnVirtualizationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    // {
    //     ItemsControl ic = d as ItemsControl;
    //     if (ic != null)
    //     {
    //         Panel p = ic.ItemsHost;
    //         if (p != null)
    //         {
    //             p.InvalidateMeasure();
    //             ItemsPresenter? itemsPresenter =  p.GetVisualParent() as ItemsPresenter;
    //             itemsPresenter?.InvalidateMeasure();
    //
    //             if (d is TreeView)
    //             {
    //                 var dp = e.Property;
    //                 if ( 
    //                     dp == VirtualizingPanel.IsVirtualizingWhenGroupingProperty ||
    //                    
    //                     dp == VirtualizingPanel.ScrollUnitProperty)
    //                 {
    //                     // VirtualizationPropertyChangePropagationRecursive(ic, p);
    //                 }
    //             }
    //         }
    //     }
    // }

    // private static void VirtualizationPropertyChangePropagationRecursive(AvaloniaObject parent, Panel itemsHost)
    // {
    //     Avalonia.Controls.Controls children = itemsHost.Children;
    //     int childrenCount = children.Count;
    //     for (int i = 0; i < childrenCount; i++)
    //     {
    //         IHierarchicalVirtualizationAndScrollInfo virtualizingChild =
    //             children[i] as IHierarchicalVirtualizationAndScrollInfo;
    //         if (virtualizingChild != null)
    //         {
    //             TreeViewItem.IsVirtualizingPropagationHelper(parent, (DependencyObject)virtualizingChild);
    //
    //             Panel childItemsHost = virtualizingChild.ItemsHost;
    //             if (childItemsHost != null)
    //             {
    //                 VirtualizationPropertyChangePropagationRecursive((DependencyObject)virtualizingChild,
    //                     childItemsHost);
    //             }
    //         }
    //     }
    // }

    // /// <summary>
    // ///     The generator associated with this panel.
    // /// </summary>
    // public IItemContainerGenerator ItemContainerGenerator
    // {
    //     get { return Generator; }
    // }
    //
    // internal override void GenerateChildren()
    // {
    //     // Do nothing. Subclasses will use the exposed generator to generate children.
    // }

    /// <summary>
    ///     Adds a child to the InternalChildren collection.
    ///     This method is meant to be used when a virtualizing panel
    ///     generates a new child. This method circumvents some validation
    ///     that occurs in ControlCollection.Add.
    /// </summary>
    /// <param name="child">Child to add.</param>
    protected void AddInternalChild(Control child)
    {
        AddInternalChild(Children, child);
    }

    /// <summary>
    ///     Inserts a child into the InternalChildren collection.
    ///     This method is meant to be used when a virtualizing panel
    ///     generates a new child. This method circumvents some validation
    ///     that occurs in ControlCollection.Insert.
    /// </summary>
    /// <param name="index">The index at which to insert the child.</param>
    /// <param name="child">Child to insert.</param>
    protected void InsertInternalChild(int index, Control child)
    {
        InsertInternalChild(Children, index, child);
    }

    /// <summary>
    ///     Removes a child from the InternalChildren collection.
    ///     This method is meant to be used when a virtualizing panel
    ///     re-virtualizes a new child. This method circumvents some validation
    ///     that occurs in ControlCollection.RemoveRange.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="range"></param>
    protected void RemoveInternalChildRange(int index, int range)
    {
        RemoveInternalChildRange(Children, index, range);
    }

    // This is internal as an optimization for VirtualizingStackPanel (so it doesn't need to re-query InternalChildren repeatedly)
    internal static void AddInternalChild(Avalonia.Controls.Controls children, Control child)
    {
        children.Add(child);
    }

    // This is internal as an optimization for VirtualizingStackPanel (so it doesn't need to re-query InternalChildren repeatedly)
    internal static void InsertInternalChild(Avalonia.Controls.Controls children, int index, Control child)
    {
        children.Insert(index, child);
    }

    // This is internal as an optimization for VirtualizingStackPanel (so it doesn't need to re-query InternalChildren repeatedly)
    internal static void RemoveInternalChildRange(Avalonia.Controls.Controls children, int index, int range)
    {
        children.RemoveRange(index, range);
    }

 

    // public bool ShouldItemsChangeAffectLayout(bool areItemChangesLocal, ItemsChangedEventArgs args)
    // {
    //     return ShouldItemsChangeAffectLayoutCore(areItemChangesLocal, args);
    // }

    // /// <summary>
    // ///     Returns whether an Items collection change affects layout for this panel.
    // /// </summary>
    // /// <param name="args">Event arguments</param>
    // /// <param name="areItemChangesLocal">Says if this notification represents a direct change to this Panel's collection</param>
    // protected virtual bool ShouldItemsChangeAffectLayoutCore(bool areItemChangesLocal, ItemsChangedEventArgs args)
    // {
    //     return true;
    // }
    //
    // /// <summary>
    // ///     Called when the UI collection of children is cleared by the base Panel class.
    // /// </summary>
    // protected virtual void OnClearChildren()
    // {
    // }

    /// <summary>
    ///     This is the public accessor for protected method BringIndexIntoView.
    /// </summary>
    public void BringIndexIntoViewPublic(int index)
    {
        BringIndexIntoView(index);
    }

    /// <summary>
    /// Generates the item at the specified index and calls BringIntoView on it.
    /// </summary>
    /// <param name="index">Specify the item index that should become visible</param>
    protected internal virtual void BringIndexIntoView(int index)
    {
    }


    // protected override void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    // {
    //     
    //     switch (e.Action)
    //     {
    //         case NotifyCollectionChangedAction.Add:
    //         case NotifyCollectionChangedAction.Remove:
    //         case NotifyCollectionChangedAction.Replace:
    //         case NotifyCollectionChangedAction.Move:
    //             // Don't allow Panel's code to run for add/remove/replace/move
    //             break;
    //
    //         default: 
    //             break;
    //     }
    //     
    //     base.ChildrenChanged(sender, e);
    // }


    // // This method returns a bool to indicate if or not the panel layout is affected by this collection change
    // internal override bool OnItemsChangedInternal(object sender, ItemsChangedEventArgs args)
    // {
    //     //
    //     // OnItemsChanged(sender, args);
    //
    //     return ShouldItemsChangeAffectLayout(true /*areItemChangesLocal*/, args);
    // }

    // internal override void OnClearChildrenInternal()
    // {
    //     OnClearChildren();
    // }

}