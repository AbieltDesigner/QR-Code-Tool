using System.Windows.Input;

namespace QR_Code_Tool_App.VievModels.Behavior
{
    public class EnterKeyArgs
    {
        public object SelectedItem { get; }
        public KeyEventArgs KeyArgs { get; }

        public EnterKeyArgs(object item, KeyEventArgs args)
        {
            SelectedItem = item;
            KeyArgs = args;
        }
    }
}
