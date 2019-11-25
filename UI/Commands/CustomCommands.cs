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
        public static readonly RoutedUICommand PastePlain = new RoutedUICommand(
            text: "Paste Plain",
            name: "PastePlain",
            ownerType: typeof(CustomCommands),
            inputGestures: new InputGestureCollection() { new KeyGesture(Key.Q, ModifierKeys.Control) }
            );
        public static readonly RoutedUICommand Dummy = new RoutedUICommand(
            text: "Dummy",
            name: "Dummy",
            ownerType: typeof(CustomCommands)
            );
        public static readonly RoutedUICommand Find = new RoutedUICommand(
            text: "Find",
            name: "Find",
            ownerType: typeof(CustomCommands),
            inputGestures: new InputGestureCollection() { new KeyGesture(Key.F, ModifierKeys.Control) }
            );
    }
}
