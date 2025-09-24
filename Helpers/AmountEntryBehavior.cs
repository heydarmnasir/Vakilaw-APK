using Microsoft.Maui.Controls;
using System.Linq;

namespace Vakilaw.Helpers
{
    public class AmountEntryBehavior : Behavior<Entry>
    {
        private bool _isUpdating;

        protected void OnAttachedTo(Editor bindable)
        {
            base.OnAttachedTo(bindable);
        }

        protected override void OnAttachedTo(Entry bindable)
        {
            base.OnAttachedTo(bindable);
            bindable.TextChanged += OnTextChanged;
        }

        protected override void OnDetachingFrom(Entry bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.TextChanged -= OnTextChanged;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdating) return;

            var entry = (Entry)sender;
            if (string.IsNullOrWhiteSpace(entry.Text))
                return;

            // فقط رقم‌ها رو نگه می‌داریم
            string numeric = new string(entry.Text.Where(char.IsDigit).ToArray());

            if (string.IsNullOrEmpty(numeric))
            {
                entry.Text = "";
                return;
            }

            if (long.TryParse(numeric, out long value))
            {
                _isUpdating = true;

                string formatted = string.Format("{0:N0}", value);
                entry.Text = formatted;

                // ست کردن مکان‌نما
                int newPos = formatted.Length;
                if (newPos <= entry.Text.Length)
                    entry.CursorPosition = newPos;

                _isUpdating = false;
            }
        }
    }
}