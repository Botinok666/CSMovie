using Movies.UWP.Controller;
using Movies.UWP.Data;
using Movies.UWP.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Movies.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MoviePage : Page
    {
        public MoviePage()
        {
            InitializeComponent();
            KeyboardAccelerator GoBack = new KeyboardAccelerator
            {
                Key = VirtualKey.GoBack
            };
            GoBack.Invoked += BackInvoked;
            KeyboardAccelerator AltLeft = new KeyboardAccelerator
            {
                Key = VirtualKey.Left
            };
            AltLeft.Invoked += BackInvoked;
            KeyboardAccelerators.Add(GoBack);
            KeyboardAccelerators.Add(AltLeft);
            // ALT routes here
            AltLeft.Modifiers = VirtualKeyModifiers.Menu;
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            // Set XAML element as a draggable region.
            AppTitleBar.Height = coreTitleBar.Height;
            Window.Current.SetTitleBar(AppTitleBar);
            // Register a handler for when the size of the overlaid caption control changes.
            // For example, when the app moves to a screen with a different DPI.
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;

            // Register a handler for when the title bar visibility changes.
            // For example, when the title bar is invoked in full screen mode.
            coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdateTitleBarLayout(sender);
        }

        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar)
        {
            // Get the size of the caption controls area and back button 
            // (returned in logical pixels), and move your content around as necessary.
            LeftPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayLeftInset);
            RightPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayRightInset);

            // Update title bar control size as needed to account for system size changes.
            AppTitleBar.Height = coreTitleBar.Height;
        }

        private void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            if (sender.IsVisible)
            {
                AppTitleBar.Visibility = Visibility.Visible;
            }
            else
            {
                AppTitleBar.Visibility = Visibility.Collapsed;
            }
        }
        protected async override void OnNavigatedTo(NavigationEventArgs args)
        {
            if (!(args.Parameter is int))
                return;
            MovieData movie = await MoviesController.GetInstance().GetMovie((int)args.Parameter);
            bool isNew = movie == null;
            //Название, если есть только в одном варианте, выводится без слэша
            titleText.Text = string.IsNullOrWhiteSpace(movie.LocalizedTitle) || string.IsNullOrWhiteSpace(movie.OriginalTitle) ?
                    movie.OriginalTitle + movie.LocalizedTitle :
                    movie.OriginalTitle + " / " + movie.LocalizedTitle;
            //Все харакетеристики
            tYear.Text = movie.Year.ToString();
            tCountry.Text = string.Join(", ", movie.Countries.Select(x => x.Name));
            tDirector.Text = string.Join(", ", movie.Directors.Select(x => x.Name));
            tScreenwriter.Text = string.Join(", ", movie.Screenwriters.Select(x => x.Name));
            tTagline.Text = movie.TagLine;
            tGenre.Text = string.Join(", ", movie.Genres.Select(x => x.Name));
            tRuntime.Text = TimeSpan.FromMinutes(movie.Runtime).ToString(@"h\:mm");
            tActors.Text = string.Join(", ", movie.Actors.Select(x => x.Name));
            tPlot.Text = movie.Storyline.Replace("<br>", Environment.NewLine);
            tRateKP.Text = movie.RatingKP.ToString("F3");
            tRateIMDB.Text = movie.RatingIMDB.ToString("F1");
            //Добавим данные о просмотрах, если они были
            if (!isNew)
            {
                List<ViewingData> viewings = MoviesController.GetInstance().GetViewings(movie.ID, UAC.GetInstance().UserId);
                if (viewings.Count > 0)
                    tViewings.Text = string.Join(
                            ", ", viewings.Select(x => string.Format("{0} ({1:F1})", x.Date.ToShortDateString(), x.Rating)));
            }

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values["picturesFolder"] is string folderToken)
            {
                StorageFolder nfolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken);
                if (await nfolder.TryGetItemAsync(Path.GetFileName(movie.PosterLink)) is StorageFile image)
                {
                    using (var stream = await image.OpenReadAsync())
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        await bitmapImage.SetSourceAsync(stream);
                        poster.Source = bitmapImage;
                    }
                }
            }
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            On_BackRequested();
        }

        // Handles system-level BackRequested events and page-level back button Click events
        private bool On_BackRequested()
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
                return true;
            }
            return false;
        }
        private void BackInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            On_BackRequested();
            args.Handled = true;
        }

        private void ActionButton_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }
    }
}
