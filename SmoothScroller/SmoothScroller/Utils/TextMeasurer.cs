using System;
using System.Text;
using Avalonia;
using Avalonia.Controls;

namespace SmoothPanelSample
{
    internal class TextMeasurer
    {
        private static double _lineHeight = -1;

        private static double _charWidth = -1;

        private static TextBlock textBlock = new();

        public static double GetEstimatedHeight(string text, double width)
        {
            CheckInitialization();

            double height = 0;

            var startChar = 0;

            var charsPerLine = (int)(width / _charWidth) - 5; // 5 is rough average extra characters for word wrap
            charsPerLine = Math.Max(1, charsPerLine);

            while (startChar < text.Length)
            {
                var endChar = text.IndexOf('\n', startChar);
                if (endChar < 0)
                {
                    endChar = text.Length;
                }

                var charCount = Math.Max(1, endChar - startChar);
                startChar = endChar + 1;

                height += _lineHeight * Math.Ceiling((double)charCount / charsPerLine);
            }

            return height;
        }

        private static void CheckInitialization()
        {
            if (_lineHeight >= 0)
            {
                return;
            }

            var text = new StringBuilder();
            for (var ch = 'a'; ch <= 'z'; ch++)
            {
                text.Append(ch);
            }

            text.Append("\r\n");
            for (var ch = 'A'; ch <= 'Z'; ch++)
            {
                text.Append(ch);
            }

            textBlock.Text = text.ToString();
            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            _lineHeight = textBlock.DesiredSize.Height / 2;
            _charWidth = textBlock.DesiredSize.Width / 26;
        }
    }
}