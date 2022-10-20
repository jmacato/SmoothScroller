namespace SmoothScroller;

/// <summary>
/// Enumeration that specifies the virtualization mode of the VirtualizingPanel. 
/// Used by <see cref="VirtualizingPanel.VirtualizationModeProperty" />.
/// </summary>
public enum VirtualizationMode
{
    /// <summary>
    ///     Standard virtualization mode -- containers are thrown away when offscreen.
    /// </summary>
    Standard,

    /// <summary>
    ///     Recycling virtualization mode -- containers are re-used when offscreen.
    /// </summary>
    Recycling
}