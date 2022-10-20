// --------------------------------------------------------------------------
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// --------------------------------------------------------------------------

using System; 
using Avalonia; 

namespace SmoothScroller
{
    /// <summary>
    /// Maps type of view model to type of visual element.
    /// </summary>
    public class SmoothPanelTemplate : AvaloniaObject
    {
        public static readonly StyledProperty<Type> ViewModelProperty =
            AvaloniaProperty.Register<SmoothPanelTemplate, Type>("ViewModel");

        public static readonly StyledProperty<Type> ViewProperty =
            AvaloniaProperty.Register<SmoothPanelTemplate, Type>("View");

        public Type ViewModel
        {
            get => GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public Type View
        {
            get => GetValue(ViewProperty);
            set => SetValue(ViewProperty, value);
        }
    }
}