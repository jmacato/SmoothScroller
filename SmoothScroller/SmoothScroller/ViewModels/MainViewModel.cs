using System;
using System.Collections;
using System.Collections.ObjectModel;
using Avalonia.Media;
using SmoothScroller.Views;

namespace SmoothScroller.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            Items = GetTestItems(5000);
        }

        public IEnumerable Items { get; }

        private IEnumerable GetTestItems(int count)
        {
            const string LoremIpsumText =
                @"Sed ut perspiciatis, unde omnis iste natus error sit voluptatem accusantium doloremque laudantium,
totam rem aperiam eaque ipsa, quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt,
explicabo. Nemo enim ipsam voluptatem, quia voluptas sit, aspernatur aut odit aut fugit,
sed quia consequuntur magni dolores eos, qui ratione voluptatem sequi nesciunt, neque porro quisquam est,
qui dolorem ipsum, quia dolor sit, amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt,
ut labore et dolore magnam aliquam quaerat voluptatem.";

            Random _rnd = new Random(0);

            var items = new ObservableCollection<ViewModelBase>();

            for (int i = 0; i < count; i++)
            {
                if (i % 10 == 0)
                {
                    items.Add(new BarViewModel("Section " + (i / 10 + 1)));
                }

                var text = LoremIpsumText.Substring(0, _rnd.Next(LoremIpsumText.Length));

                // Add two very big items
                if (i == count - 2 || i == count / 2)
                {
                    for (int j = 0; j < 200; j++)
                    {
                        text += "\r\nLine " + j;
                    }
                }

                var color = Color.FromArgb(
                    255,
                    (byte)(240 - _rnd.Next(50)),
                    (byte)(240 - _rnd.Next(50)),
                    (byte)(240 - _rnd.Next(50)));

                items.Add(new FooViewModel(text, color));
            }

            return items;
        }
    }
}