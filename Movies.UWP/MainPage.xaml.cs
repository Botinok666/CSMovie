using Movies.Model;
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
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.AccessCache;
using Windows.ApplicationModel.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Movies.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private object filterParam;
        private bool firstLoad = true;
        public PagedResult<MovieData> Movies { get; set; }
        public MainPage()
        {
            InitializeComponent();
            filterCB.ItemsSource = FilterOption.FilterOptions;
            filterCB.DisplayMemberPath = "Description";
            sortCB.ItemsSource = SortOption.SortOptions;
            sortCB.DisplayMemberPath = "Description";
            dgv.Columns[0].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            dgv.Columns[1].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            dgv.Columns[2].Width = DataGridLength.Auto;
            dgv.Columns[3].Width = DataGridLength.Auto;

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

            Loaded += new RoutedEventHandler(CheckCredentials);
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
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            CheckCredentials(new object(), new RoutedEventArgs());
        }
        private async void CheckCredentials(object sender, RoutedEventArgs e)
        {
            if (UAC.GetInstance().UserId == -1)
            {
                Frame.Navigate(typeof(Login));
                return;
            }
            if (firstLoad)
            {
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                if (!(localSettings.Values["picturesFolder"] is string))
                {
                    await new Windows.UI.Popups.MessageDialog("Please select folder where posters will be stored").ShowAsync();
                    FolderPicker folderPicker = new FolderPicker();
                    folderPicker.FileTypeFilter.Add("*");
                    StorageFolder folder = await folderPicker.PickSingleFolderAsync();
                    if (folder == null)
                        return;
                    string folderToken = StorageApplicationPermissions.FutureAccessList.Add(folder);
                    localSettings.Values["picturesFolder"] = folderToken;
                }
                firstLoad = false;
                sortCB.SelectedIndex = SortOption.SortOptions.FindIndex(j => j.SortProperty == SortProperties.ViewDate);
                filterCB.SelectedIndex = FilterOption.FilterOptions.FindIndex(j => j.Filter == Filters.GetAll);
            }
        }
        private void UpdateDG(int page)
        {
            Movies = MoviesController.GetInstance().GetMovies(filterParam, page, 25, 
                (filterCB.SelectedItem as FilterOption).Filter,
                (sortCB.SelectedItem as SortOption).SortProperty);
            currentPage.Text = string.Format("{0}/{1}", Movies.CurrentPage, Movies.PageCount);
            leftPage.IsEnabled = Movies.CurrentPage > 1;
            rightPage.IsEnabled = Movies.CurrentPage < Movies.PageCount;
            dgv.ItemsSource = Movies.Results;
        }
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AddMovies));
        }

        private void LeftPage_Click(object sender, RoutedEventArgs e)
        {
            UpdateDG(Movies.CurrentPage - 1);
        }

        private void RightPage_Click(object sender, RoutedEventArgs e)
        {
            UpdateDG(Movies.CurrentPage + 1);
        }

        private void Dgv_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (dgv.SelectedItem != null)
                Frame.Navigate(typeof(MoviePage), (dgv.SelectedItem as MovieData).ID);
        }

        private void FilterCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((filterCB.SelectedItem as FilterOption).Filter)
            {
                case Filters.GetAll:
                    filterParam = null;
                    paramTB.Visibility = Visibility.Collapsed;
                    paramCB.Visibility = Visibility.Collapsed;
                    break;
                case Filters.GetByActor:
                case Filters.GetByDirector:
                case Filters.GetByScreenwriter:
                    paramTB.Visibility = Visibility.Collapsed;
                    paramCB.IsEditable = true;
                    paramCB.Visibility = Visibility.Visible;
                    break;
                case Filters.GetByCountry:
                    paramTB.Visibility = Visibility.Collapsed;
                    paramCB.ItemsSource = MoviesController.GetInstance().GetAllCountries();
                    paramCB.IsEditable = false;
                    paramCB.Visibility = Visibility.Visible;
                    break;
                case Filters.GetByGenre:
                    paramTB.Visibility = Visibility.Collapsed;
                    paramCB.ItemsSource = MoviesController.GetInstance().GetAllGenres();
                    paramCB.IsEditable = false;
                    paramCB.Visibility = Visibility.Visible;
                    break;
                case Filters.GetByStoryline:
                case Filters.GetByYearPeriod:
                case Filters.GetByTitle:
                    paramCB.Visibility = Visibility.Collapsed;
                    paramTB.Visibility = Visibility.Visible;
                    break;
            }
            UpdateDG(1);
        }

        private void ParamCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            filterParam = paramCB.SelectedItem;
            UpdateDG(1);
        }

        private void ParamCB_TextSubmitted(ComboBox sender, ComboBoxTextSubmittedEventArgs args)
        {
            if (!(filterParam is PersonData) || args.Text != (filterParam as PersonData).Name)
            {
                sender.ItemsSource = MoviesController.GetInstance().GetPersons(args.Text);
                sender.IsDropDownOpen = true;
            }
        }

        private void ParamTB_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != Windows.System.VirtualKey.Enter)
                return;
            if ((filterCB.SelectedItem as FilterOption).Filter == Filters.GetByYearPeriod)
            {
                string[] vs = (sender as TextBox).Text.Split(new char[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);
                if (vs != null && vs.Length == 2
                    && short.TryParse(vs[0], out short start) && short.TryParse(vs[1], out short end)
                    && end > start)
                {
                    filterParam = new Tuple<short, short>(start, end);
                    UpdateDG(1);
                }
                else
                {
                    if (!InfoFlyout.IsOpen)
                    {
                        InfoText.Text = "Пожалуйста, введите правильный диапазон" + Environment.NewLine + 
                            "Например: 1989-2007";
                        InfoFlyout.ShowAt(sender as TextBox);
                    }
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace((sender as TextBox).Text))
                {
                    filterParam = (sender as TextBox).Text;
                    UpdateDG(1);
                }
            }
        }

        private void SortCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (filterCB.SelectedItem == null)
                return;
            UpdateDG(1);
        }

        private void CurrentPage_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key != Windows.System.VirtualKey.Enter)
                return;
            if (!int.TryParse((sender as TextBox).Text, out int pageNum) || pageNum > Movies.PageCount || pageNum < 1)
            {
                (sender as TextBox).Text = string.Format("{0}/{1}", Movies.CurrentPage, Movies.PageCount);
                return;
            }
            UpdateDG(pageNum);
        }
        private void CurrentPage_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).SelectAll();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Profile));
        }
    }
}
