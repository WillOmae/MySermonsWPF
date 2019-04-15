using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MySermonsWPF.UI
{
    /// <summary>
    /// Interaction logic for MSTabItemHeader.xaml
    /// </summary>
    public partial class MSTabItemHeader:UserControl
    {
        public MSTabItemHeader()
        {
            this.InitializeComponent();
        }

        private void MSTabItemHeaderClose_MouseEnter(object sender, MouseEventArgs e)
        {
            if(sender is Image image)
            {
                this.SetCloseImageOpacity(image);
            }
        }

        private void MSTabItemHeaderClose_MouseLeave(object sender, MouseEventArgs e)
        {
            if(sender is Image image)
            {
                this.RemoveCloseImageOpacity(image);
            }
        }
        /// <summary>
        /// Set visual effect on hover.
        /// </summary>
        /// <param name="image">Image to be manipulated.</param>
        private void SetCloseImageOpacity(Image image)
        {
            image.OpacityMask = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0));
        }
        /// <summary>
        /// Unset visual effect set on hover.
        /// </summary>
        /// <param name="image">Image to be manipulated.</param>
        private void RemoveCloseImageOpacity(Image image)
        {
            image.OpacityMask = null;
        }
    }
}
