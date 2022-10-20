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