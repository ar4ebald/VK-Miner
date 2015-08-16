using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using VK_Miner.DataModel;
using VK_Miner.Pages;
using VK_Miner.VK;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace VK_Miner
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AppShell : Page
    {
        private const int SearchInputDelay = 400;

        public Frame AppFrame => this.frame;

        private int _suggestCharIndex;

        public AppShell()
        {
            this.InitializeComponent();

            AppFrame.Navigated += AppFrameOnNavigated;

            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

            var titleBar = ApplicationView.GetForCurrentView().TitleBar;

            var bColor = Color.FromArgb(0xFF, 0x2A, 0x52, 0xBE);//#2A52BE

            titleBar.ButtonBackgroundColor = bColor;
            titleBar.InactiveForegroundColor = bColor;
            titleBar.ButtonInactiveBackgroundColor = bColor;
            titleBar.ButtonHoverBackgroundColor = Color.FromArgb(0xFF, 0x22, 0x42, 0x98); //#224298
            titleBar.ButtonPressedBackgroundColor = Color.FromArgb(0xFF, 0x19, 0x31, 0x72); //#193172

            var foreground = Colors.White;
            titleBar.ButtonForegroundColor = foreground;
            titleBar.ButtonInactiveForegroundColor = foreground;
            titleBar.ButtonHoverForegroundColor = foreground;
            titleBar.ButtonPressedForegroundColor = foreground;
        }

        private delegate RequestData GetHintsCreatorDelegate(string q);
        private static readonly GetHintsCreatorDelegate GetHintsCreator = RequestData.Creator<GetHintsCreatorDelegate>("execute.getHints", RequestArgs.Empty);

        private async Task<List<HintItemViewModel>> TryGetHintsAsync(string q)
        {
            bool doRequest;
            if (Api.IsBusy)
            {
                var index = _suggestCharIndex + 1;
                Interlocked.Increment(ref _suggestCharIndex);

                await Task.Delay(SearchInputDelay);

                doRequest = (_suggestCharIndex == index);
            }
            else
                doRequest = true;

            return doRequest
                ? (await Api.Execute(GetHintsCreator(q)))
                    .Select(i => new HintItemViewModel(i))
                    .OrderBy(i => i.Section)
                    .ToList()
                : null;
        }

        private void AppFrameOnNavigated(object sender, NavigationEventArgs navigationEventArgs)
        {
            BackButton.IsEnabled = AppFrame.CanGoBack;
            ForwardButton.IsEnabled = AppFrame.CanGoForward;
        }

        private void AppShell_OnLoaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SetTitleBar(BackgroundElement);
        }

        private void LoggedUserButton_OnClick(object sender, RoutedEventArgs e)
        {
            var userPage = AppFrame.Content as UserPage;
            if (userPage == null || userPage.UserId != VK.Api.LoggedUser.Id)
                AppFrame.Navigate(typeof(UserPage), VK.Api.LoggedUser.Id);
        }

        private async void ExitButton_OnClick(object sender, RoutedEventArgs e)
        {
            await VK.Api.LogoutAsync();
            this.DataContext = VK.Api.LoggedUser;
        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (AppFrame.CanGoBack)
                AppFrame.GoBack();
        }

        private void ForwardButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (AppFrame.CanGoForward)
                AppFrame.GoForward();
        }

        private void ReloadButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (AppFrame.BackStack.Any())
            {
                var last = AppFrame.BackStack.Last();
                AppFrame.Navigate(last.SourcePageType, last.Parameter);
                AppFrame.BackStack.Remove(last);
            }
        }

        private async void AutoSuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput)
                return;

            var items = await TryGetHintsAsync(sender.Text);
            if (items != null)
                sender.ItemsSource = items;
        }

        private void AutoSuggestBox_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var item = args.ChosenSuggestion as HintItemViewModel;

            if (item != null)
            {
                if (item.Type == "profile")
                    AppFrame.Navigate(typeof(UserPage), item.Id);
                else
                {
                    // TODO: Add navigation to group page    
                }
            }

            sender.Text = string.Empty;
        }
    }
}