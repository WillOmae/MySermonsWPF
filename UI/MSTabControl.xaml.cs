using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MySermonsWPF.UI
{
    /// <summary>
    /// Interaction logic for MSTabControl.xaml
    /// </summary>
    public partial class MSTabControl:UserControl
    {
        public ObservableCollection<TabItem> Items = new ObservableCollection<TabItem>();

        public MSTabControl()
        {
            this.InitializeComponent();
            this.Items.CollectionChanged += this.Items_CollectionChanged;

        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach(var item in e.NewItems)
                    {
                        if(item is TabItem tabItem)
                        {
                            if(tabItem.Header is string headerText)
                            {
                                MSTabItemHeader tabItemHeader = new MSTabItemHeader();
                                tabItemHeader.BaseTabItemHeaderText.Content = headerText;
                                tabItem.Header = tabItemHeader;
                                tabItemHeader.MouseDoubleClick += delegate
                                {
                                    this.Items.Remove(tabItem);
                                };
                                tabItemHeader.BaseTabItemHeader.MouseDoubleClick += delegate
                                {
                                    this.Items.Remove(tabItem);
                                }; tabItemHeader.BaseTabItemHeaderClose.MouseLeftButtonUp += delegate
                                {
                                    this.Items.Remove(tabItem);
                                };
                            }
                            this.BaseTabControl.Items.Add(tabItem);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach(var item in e.OldItems)
                    {
                        this.BaseTabControl.Items.Remove(item);
                    }
                    break;
            }
        }

        private void TabItemHeader_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MessageBox.Show("Clicked");
        }

        private void SetDoubleClickToClose(TabItem tabItem)
        {
            if(tabItem == null) return;
            tabItem.MouseDoubleClick += (s, e) => this.Items.Remove(tabItem);
        }

        private void CloseCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            if(this.BaseTabControl.SelectedItem is TabItem tabItem)
            {
                this.Items.Remove(tabItem);
            }
        }
        private void CloseCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.Items.Count > 0 ? true : false;
        }
        private void CloseAllCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            this.Items.Clear();
            this.BaseTabControl.Items.Clear();
        }
        private void CloseAllCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.Items.Count > 0 ? true : false;
        }
        public void SetSelectedItem(TabItem tabItem)
        {
            BaseTabControl.SelectedItem = tabItem;
        }
    }
}
