﻿// --------------------------------------------------------------------------
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// --------------------------------------------------------------------------

using Avalonia.Media;
using SmoothScroller;
using ReactiveUI;
using SmoothPanelSample;
using SmoothScroller.ViewModels;

namespace SmoothScroller.Views
{
    public class FooViewModel : ViewModelBase, IHeightMeasurer
    {
        private string _text;

        private double _estimatedHeight = -1;

        private double _estimatedWidth;

        public FooViewModel(string text, Color color)
        {
            Text = text;
            Brush = new SolidColorBrush(color);
        }

        public SolidColorBrush Brush { get; set; }
        
        public string Text
        {
            get => _text;
            set => this.RaiseAndSetIfChanged(ref _text, value);
        }
        
        public double GetEstimatedHeight(double width)
        {
            // Do not recalc height if text and width are unchanged
            if (_estimatedHeight < 0 || _estimatedWidth != width)
            {
                _estimatedWidth = width;
                
                _estimatedHeight = TextMeasurer.GetEstimatedHeight(_text, width) + 20; // Add margin
            }
            return _estimatedHeight;
        }
    }
}