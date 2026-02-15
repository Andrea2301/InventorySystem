using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace InventorySystem.Helpers
{
    public static class TextBoxBehavior
    {
        public enum RestrictionType
        {
            None,
            Numeric,
            Letters,
            Phone
        }

        public static readonly DependencyProperty RestrictionProperty =
            DependencyProperty.RegisterAttached("Restriction", typeof(RestrictionType), typeof(TextBoxBehavior), new PropertyMetadata(RestrictionType.None, OnRestrictionChanged));

        public static RestrictionType GetRestriction(DependencyObject obj) => (RestrictionType)obj.GetValue(RestrictionProperty);
        public static void SetRestriction(DependencyObject obj, RestrictionType value) => obj.SetValue(RestrictionProperty, value);

        // Keep legacy property for compatibility if already used
        public static readonly DependencyProperty IsNumericOnlyProperty =
            DependencyProperty.RegisterAttached("IsNumericOnly", typeof(bool), typeof(TextBoxBehavior), new PropertyMetadata(false, OnIsNumericOnlyChanged));

        public static bool GetIsNumericOnly(DependencyObject obj) => (bool)obj.GetValue(IsNumericOnlyProperty);
        public static void SetIsNumericOnly(DependencyObject obj, bool value) => obj.SetValue(IsNumericOnlyProperty, value);

        private static void OnIsNumericOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox && (bool)e.NewValue)
                SetRestriction(textBox, RestrictionType.Numeric);
        }

        private static void OnRestrictionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                textBox.PreviewTextInput -= TextBox_PreviewTextInput;
                DataObject.RemovePastingHandler(textBox, OnPaste);

                if ((RestrictionType)e.NewValue != RestrictionType.None)
                {
                    textBox.PreviewTextInput += TextBox_PreviewTextInput;
                    DataObject.AddPastingHandler(textBox, OnPaste);
                }
            }
        }

        private static void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                var restriction = GetRestriction(textBox);
                e.Handled = !IsTextAllowed(e.Text, restriction);
            }
        }

        private static void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (sender is TextBox textBox && e.DataObject.GetDataPresent(DataFormats.Text))
            {
                string text = (string)e.DataObject.GetData(DataFormats.Text);
                var restriction = GetRestriction(textBox);
                if (!IsTextAllowed(text, restriction)) e.CancelCommand();
            }
        }

        private static bool IsTextAllowed(string text, RestrictionType restriction)
        {
            return restriction switch
            {
                RestrictionType.Numeric => Regex.IsMatch(text, @"^[0-9.]+$"),
                RestrictionType.Letters => Regex.IsMatch(text, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s-]+$"),
                RestrictionType.Phone => Regex.IsMatch(text, @"^[0-9+\s-()]+$"),
                _ => true
            };
        }
    }
}
