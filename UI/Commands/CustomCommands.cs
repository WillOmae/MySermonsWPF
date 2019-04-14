using System.Windows.Input;

namespace MySermonsWPF.UI.Commands
{
    public static class CustomCommands
    {
        public static readonly RoutedUICommand Exit = new RoutedUICommand(
            text: "Exit",
            name: "Exit",
            ownerType: typeof(CustomCommands),
            inputGestures: new InputGestureCollection() { new KeyGesture(Key.F4, ModifierKeys.Alt) }
            );
        public static readonly RoutedUICommand CloseAll = new RoutedUICommand(
            text: "Close All",
            name: "CloseAll",
            ownerType: typeof(CustomCommands),
            inputGestures: new InputGestureCollection() { new KeyGesture(Key.F4, ModifierKeys.Control) }
            );
    }
}
