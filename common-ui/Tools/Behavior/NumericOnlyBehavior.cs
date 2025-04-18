using Microsoft.Xaml.Behaviors;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace UI.Tools.Behavior
{
    public class NumericOnlyBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            AssociatedObject.PreviewTextInput += AssociatedObject_PreviewTextInput;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewTextInput -= AssociatedObject_PreviewTextInput;
            base.OnDetaching();
        }

        private void AssociatedObject_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9-]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
