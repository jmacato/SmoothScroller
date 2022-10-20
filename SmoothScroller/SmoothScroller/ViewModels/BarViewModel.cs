// --------------------------------------------------------------------------
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// --------------------------------------------------------------------------
 
using SmoothScroller;
using ReactiveUI;
using SmoothScroller.ViewModels;

namespace SmoothScroller.Views
{
    public class BarViewModel : ViewModelBase, IHeightMeasurer
    {
        private string _text;

        public BarViewModel(string text)
        {
            Text = text;
        }

        public string Text
        {
            get => _text;
            set => this.RaiseAndSetIfChanged(ref _text, value);
        }

        public double GetEstimatedHeight(double width)
        {
            return 16;
        }
    }
}