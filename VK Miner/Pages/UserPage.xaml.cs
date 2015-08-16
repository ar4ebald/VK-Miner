using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using VK_Miner.DataModel;
using VK_Miner.VK;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace VK_Miner.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UserPage : Page
    {
        public long UserId { get; private set; }

        private UserViewModel _model;

        public UserPage()
        {
            this.InitializeComponent();
            this.DataContext = null;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            UserId = (long)e.Parameter;

            _model = await UserViewModel.CreateAsync(UserId);
            DataContext = _model;
        }

        private async void AllPhotosImage_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var offset = ((UserViewModel.PhotoMenuItem)(((FrameworkElement)sender).DataContext)).Offset;
            var model = new PhotoDialogViewModel(RequestData.Creator<PhotoDialogViewModel.CreatorDelegate>("photos.getAll", new RequestArgs()
            {
                ["owner_id"] = _model.UserId,
                ["count"] = 50,
                ["photo_sizes"] = 1,
                ["skip_hidden"] = 1
            }), offset, _model.FirstPhotos);

            var dialog = new PhotosDialog(model);
            await dialog.ShowAsync();
        }
    }
}
