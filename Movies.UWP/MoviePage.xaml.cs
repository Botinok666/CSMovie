using Movies.UWP.Controller;
using Movies.UWP.Data;
using Movies.UWP.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
        public List<DescriptionData> MovieDescr { get; set; }
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
            //Массив для представления всех характеристик фильма
            MovieDescr = new List<DescriptionData>
            {
                new DescriptionData() { Name = "Год:", Value = movie.Year.ToString() },
                new DescriptionData() { Name = "Страна:", Value = string.Join(", ", movie.Countries.Select(x => x.Name)) },
                new DescriptionData() { Name = "Режиссёр:", Value = string.Join(", ", movie.Directors.Select(x => x.Name)) },
                new DescriptionData() { Name = "Сценарий:", Value = string.Join(", ", movie.Screenwriters.Select(x => x.Name)) },
                new DescriptionData() { Name = "Слоган:", Value = movie.TagLine },
                new DescriptionData() { Name = "Жанр:", Value = string.Join(", ", movie.Genres.Select(x => x.Name)) },
                new DescriptionData() { Name = "Длительность:", Value = TimeSpan.FromMinutes(movie.Runtime).ToString(@"h\:mm") },
                new DescriptionData() { Name = "В главных ролях:", Value = string.Join(", ", movie.Actors.Select(x => x.Name)) },
                new DescriptionData() { Name = "Сюжет:", Value = movie.Storyline.Replace("<br>", Environment.NewLine) },
                new DescriptionData() { Name = "Рейтинг KP:", Value = movie.RatingKP.ToString("F3") },
                new DescriptionData() { Name = "Рейтинг IMDB:", Value = movie.RatingIMDB.ToString("F1") }
            };
            //Добавим данные о просмотрах, если они были
            if (!isNew)
            {
                List<ViewingData> viewings = MoviesController.GetInstance().GetViewings(movie.ID, UAC.GetInstance().UserId);
                if (viewings.Count > 0)
                    MovieDescr.Add(new DescriptionData()
                    {
                        Name = "Просмотры:",
                        Value = string.Join(
                            ", ", viewings.Select(x => string.Format("{0} ({1:F1})", x.Date.ToShortDateString(), x.Rating)))
                    });
            }
            dgv.ItemsSource = MovieDescr;

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
    }
    public class DescriptionData
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
