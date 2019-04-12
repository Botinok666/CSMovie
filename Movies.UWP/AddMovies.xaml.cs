using Microsoft.Toolkit.Uwp.UI.Controls;
using Movies.UWP.Controller;
using Movies.UWP.Data;
using Movies.UWP.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Movies.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddMovies : Page
    {
        public List<MovieDisplay> Movies { get; set; }
        private readonly DatePicker startDate = new DatePicker()
        {
            Name = "startDate",
            Header = "Начальная дата"
        };
        private readonly DatePicker endDate = new DatePicker()
        {
            Name = "endDate",
            Header = "Конечная дата"
        };
        public AddMovies()
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
            dgv.Columns[0].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            dgv.Columns[1].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            dgv.Columns[2].Width = DataGridLength.Auto;
            dgv.Columns[3].Width = DataGridLength.Auto;
            dgv.Columns[4].Width = DataGridLength.Auto;
            dgv.Columns[5].Width = DataGridLength.Auto;
            dgv.Columns[6].Width = DataGridLength.Auto;
        }

        private async void AddBtn_Click(object param, RoutedEventArgs e)
        {
            AddBtn.IsEnabled = false;
            FileOpenPicker diag = new FileOpenPicker();
            diag.FileTypeFilter.Add(".zip");
            StorageFile file = await diag.PickSingleFileAsync();
            AddBtn.IsEnabled = true;
            if (file == null)
                return;

            StringBuilder builder = new StringBuilder("");
            List<string> html;
            int count = 0;
            using (Stream stream = await file.OpenStreamForReadAsync())
            using (ZipArchive zip = new ZipArchive(stream,
                ZipArchiveMode.Read, false))
            {
                count = zip.Entries.Count;
                html = zip.Entries
                    .OrderBy(entry => entry.LastWriteTime.DateTime)
                    .Select(entry =>
                    {
                        var streamR = new StreamReader(
                            entry.Open(), CodePagesEncodingProvider.Instance.GetEncoding(1251));
                        string result = streamR.ReadToEnd();
                        streamR.Close();
                        return result;
                    })
                    .ToList();
            }
            List<MovieData> movies = html
                .AsParallel()
                .Select(s => 
                {
                    try
                    {
                        MovieData temp = KPParser.ParseString(s);
                        return temp;
                    }
                    catch (FormatException exc)
                    {
                        builder.Append(exc.Message + Environment.NewLine);
                        return null;
                    }
                })
                .Where(movie => movie != null)
                .ToList();
            
            builder.AppendFormat("{0}/{1} entries processed{2}", movies.Count(),
                count, Environment.NewLine);

            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (!(localSettings.Values["picturesFolder"] is string folderToken))
            {
                FolderPicker folderPicker = new FolderPicker();
                folderPicker.FileTypeFilter.Add("*");
                StorageFolder folder = await folderPicker.PickSingleFolderAsync();
                if (folder == null)
                    return;
                folderToken = StorageApplicationPermissions.FutureAccessList.Add(folder);
                localSettings.Values["picturesFolder"] = folderToken;
            }
            StorageFolder nfolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(folderToken);
            var downloads = (await Task.WhenAll(movies
                    .Select(async movie => new Tuple<string, IStorageItem>(
                        movie.PosterLink, await nfolder.TryGetItemAsync(Path.GetFileName(movie.PosterLink)))))
                    )
                .Where(tuple => tuple.Item2 == null);
            int required = downloads.Count();
            count = (await Task.WhenAll(downloads
                .Select(async tuple =>
                {
                    try
                    {
                        Uri source = new Uri(tuple.Item1);
                        StorageFile destinationFile = await nfolder.CreateFileAsync(Path.GetFileName(tuple.Item1));
                        BackgroundDownloader downloader = new BackgroundDownloader();
                        DownloadOperation download = downloader.CreateDownload(source, destinationFile);
                        await download.StartAsync();
                        return true;
                    }
                    catch (Exception)
                    {
                        builder.Append("Download failed: " + tuple.Item1 + Environment.NewLine);
                        return false;
                    }
                })))
                .Count(x => x);
            builder.AppendFormat("{0}/{1} posters downloaded{2}", count, required, Environment.NewLine);

            if (InfoFlyout.IsOpen)
                InfoFlyout.Hide();
            InfoText.Text = builder.ToString();
            InfoFlyout.ShowAt(dgv);
            //await new Windows.UI.Popups.MessageDialog(builder.ToString()).ShowAsync();
            Movies = movies
                .Select(movie => new MovieDisplay(movie, DateTime.Now.Date, movie.RatingIMDB))
                .ToList();
            dgv.ItemsSource = Movies;
            SaveBtn.IsEnabled = true;
            if (!UpperRibbon.Children.Contains(startDate))
            {
                startDate.DateChanged += Date_DateChanged;
                UpperRibbon.Children.Add(startDate);
                endDate.DateChanged += Date_DateChanged;
                UpperRibbon.Children.Add(endDate);
            }
        }
        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (InfoFlyout.IsOpen)
                InfoFlyout.Hide();
            SaveBtn.IsEnabled = false;
            MoviesController controller = MoviesController.GetInstance();
            int result = await controller.SaveMovies(Movies.Select(x => x.Movie).ToList());
            int userId = UAC.GetInstance().UserId;
            int viewings = await controller.SaveViewings(Movies.Select(
                x => new ViewingData(x.Movie.ID, userId, DateTime.Parse(x.Date), x.Rate)).ToList());
            //await new Windows.UI.Popups.MessageDialog(
            InfoText.Text = string.Format("Сохранено {0}/{1} фильмов{2}Добавлено {3}/{1} просмотров",
                result, Movies.Count, Environment.NewLine, viewings);
            InfoFlyout.ShowAt(dgv);
            //.ShowAsync();
            Movies.Clear();
        }
        private void Date_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            if (Movies == null || Movies.Count < 1 ||
                startDate.Date > DateTime.Now || endDate.Date > DateTime.Now)
                return;
            TimeSpan difference = endDate.Date.Subtract(startDate.Date);
            if (difference.Days <= 0)
                return;
            float offset = difference.Days / (float)Movies.Count, current = 0;
            foreach (var row in Movies)
            {
                row.Date = startDate.Date.AddDays(current).ToString();
                current += offset;
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
    public class MovieDisplay : INotifyDataErrorInfo
    {
        public MovieData Movie { get; set; }
        private DateTime date;
        public string Date
        {
            get => date.ToShortDateString();
            set
            {
                if (!_errors.TryGetValue("Date", out List<string> errorsForDate))
                    errorsForDate = new List<string>();
                else
                    errorsForDate.Clear();
                if (!DateTime.TryParse(value, out date))
                    errorsForDate.Add("Дата имеет неверный формат");
                _errors["Date"] = errorsForDate;
                if (errorsForDate.Count > 0)
                    RaiseErrorsChanged("Date");
            }
        }
        private float rate;
        public float Rate
        {
            get => rate;
            set
            {
                if (rate == value) return;
                if (!_errors.TryGetValue("Rate", out List<string> errorsForRate))
                    errorsForRate = new List<string>();
                else
                    errorsForRate.Clear();
                if (value <= 0 || value >= 10)
                    errorsForRate.Add("Оценка должна быть от 0 до 10");
                _errors["Rate"] = errorsForRate;
                if (errorsForRate.Count > 0)
                    RaiseErrorsChanged("Rate");
                rate = value;
            }
        }
        public MovieDisplay(MovieData movie, DateTime date, float rate)
        {
            Movie = movie;
            Date = date.ToShortDateString();
            Rate = rate;
        }
        private readonly Dictionary<string, List<string>> _errors =
            new Dictionary<string, List<string>>();
        public bool HasErrors => _errors.Values.FirstOrDefault(l => l.Count > 0) != null;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        public void RaiseErrorsChanged(string propertyName)
        {
            EventHandler<DataErrorsChangedEventArgs> handler = ErrorsChanged;
            if (handler == null) return;
            var arg = new DataErrorsChangedEventArgs(propertyName);
            handler.Invoke(this, arg);
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return _errors;
            else
            {
                _errors.TryGetValue(propertyName, out List<string> errors);
                return errors;
            }
        }
    }
}
