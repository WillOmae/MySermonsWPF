using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

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
                                tabItemHeader.MSTabItemHeaderText.Content = headerText;
                                tabItem.Header = tabItemHeader;
                                tabItemHeader.MouseDoubleClick += delegate
                                {
                                    this.Items.Remove(tabItem);
                                };
                                tabItemHeader.MSTabItemHeaderBase.MouseDoubleClick += delegate
                                {
                                    this.Items.Remove(tabItem);
                                }; tabItemHeader.MSTabItemHeaderClose.MouseLeftButtonUp += delegate
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
    }
}
