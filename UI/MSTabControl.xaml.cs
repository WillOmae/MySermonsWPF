using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace MySermonsWPF.UI
{
    /// <summary>
    /// Interaction logic for MSTabControl.xaml
    /// </summary>
    public partial class MSTabControl : UserControl
    {
        public ObservableCollection<TabItem> Items = new ObservableCollection<TabItem>();

        public MSTabControl()
        {
            this.InitializeComponent();
            this.Items.CollectionChanged += this.Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems)
                    {
                        if (item is TabItem tabItem)
                        {
                            if (tabItem.Header is string headerText)
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
                            // prevent duplicate viewers
                            if (tabItem.Content is MSViewer viewer)
                            {
                                var sermon = viewer.GetSermon();
                                ItemCollection collection = this.BaseTabControl.Items;
                                TabItem found = (from TabItem collectionItem in collection
                                                 where (collectionItem.Content as MSViewer) != null && (collectionItem.Content as MSViewer).GetSermon().Equals(sermon)
                                                 select collectionItem).FirstOrDefault();
                                if (found == null)
                                {
                                    this.BaseTabControl.Items.Add(tabItem);
                                    BaseTabControl.SelectedItem = tabItem;
                                }
                                else
                                {
                                    this.Items.Remove(tabItem);
                                    BaseTabControl.SelectedItem = found;
                                }
                            }
                            // prevent duplicate editors
                            else if (tabItem.Content is MSRichTextBoxAdv richTextBox)
                            {
                                var sermon = richTextBox.GetSermon();
                                var collection = this.BaseTabControl.Items.OfType<TabItem>();
                                TabItem found = collection.Where(t => t.Content is MSRichTextBoxAdv mSRichTextBoxAdv
                                                                  && mSRichTextBoxAdv.GetSermon() != null
                                                                  && mSRichTextBoxAdv.GetSermon().Equals(sermon)).FirstOrDefault();
                                if (found == null)
                                {
                                    this.BaseTabControl.Items.Add(tabItem);
                                    BaseTabControl.SelectedItem = tabItem;
                                }
                                else
                                {
                                    this.Items.Remove(tabItem);
                                    BaseTabControl.SelectedItem = found;
                                }
                            }
                            else
                            {
                                this.BaseTabControl.Items.Add(tabItem);
                                BaseTabControl.SelectedItem = tabItem;
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        this.BaseTabControl.Items.Remove(item);
                    }
                    break;
            }
        }

        private void CloseCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            if (this.BaseTabControl.SelectedItem is TabItem tabItem)
            {
                this.Items.Remove(tabItem);
            }
        }
        private void CloseCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.Items.Any();
        }
        private void CloseAllCommandExecuted(object target, ExecutedRoutedEventArgs e)
        {
            this.Items.Clear();
            this.BaseTabControl.Items.Clear();
        }
        private void CloseAllCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.Items.Any();
        }
        public void SetSelectedItem(TabItem tabItem)
        {
            this.BaseTabControl.SelectedItem = tabItem;
        }
    }
}
