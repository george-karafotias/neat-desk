using System;
using System.Windows;
using System.Windows.Controls;

namespace NeatDesk
{
    public class TextBoxLogger : ILogger
    {
        private const int MAX_CHARACTERS = 5000;
        private TextBox textBox;

        public TextBoxLogger(TextBox textBox)
        {
            this.textBox = textBox;
        }

        public void Log(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!string.IsNullOrEmpty(textBox.Text) && textBox.Text.Length > MAX_CHARACTERS)
                {
                    textBox.Clear();
                }

                textBox.AppendText(message);
                textBox.AppendText(Environment.NewLine);
                textBox.ScrollToEnd();
            });
        }
    }
}
