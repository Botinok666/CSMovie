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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Movies.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private int Page = 1;
        private object filterParam;
        private bool firstLoad = true;
        public PagedResult<MovieData> Movies { get; set; }
        public MainPage()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(CheckCredentials);
            filterCB.ItemsSource = FilterOption.FilterOptions;
            filterCB.DisplayMemberPath = "Description";
            dgv.Columns[0].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            dgv.Columns[1].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            dgv.Columns[2].Width = DataGridLength.Auto;
            dgv.Columns[3].Width = DataGridLength.Auto;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            CheckCredentials(new object(), new RoutedEventArgs());
        }
        private void CheckCredentials(object sender, RoutedEventArgs e)
        {
            if (UAC.GetInstance().UserId == -1)
            {
                using (var db = new Context())
                    if (db.Users.Count() < 1)
                    {
                        db.Users.Add(new User() { ID = 1, Name = "admin", Pwd = "p@ssw0rd", Role = Roles.ROLE_ADMIN });
                        db.SaveChanges();
                    }
            //    Frame.Navigate(typeof(Login));
                UAC.GetInstance().Authorize("admin", "p@ssw0rd");
            }
            if (firstLoad)
            {
                firstLoad = false;
                filterCB.SelectedIndex = 0;
            }
        }
        private void UpdateDG(int page)
        {
            Movies = MoviesController.GetInstance()
                .GetMovies(filterParam, page, 10, (filterCB.SelectedItem as FilterOption).Filter);
            currentPage.Text = string.Format("{0}/{1}", Movies.CurrentPage, Movies.PageCount);
            leftPage.IsEnabled = Movies.CurrentPage > 1;
            rightPage.IsEnabled = Movies.CurrentPage < Movies.PageCount;
            dgv.ItemsSource = Movies.Results;
            Page = page;
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
            Page = 1;
            switch ((filterCB.SelectedItem as FilterOption).Filter)
            {
                case Filters.GetAll:
                    filterParam = null;
                    paramTB.Visibility = Visibility.Collapsed;
                    paramCB.Visibility = Visibility.Collapsed;
                    break;
                case Filters.GetByActor:
                case Filters.GetByDirector:
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
                case Filters.GetByUser:
                    paramTB.Visibility = Visibility.Collapsed;
                    paramCB.Visibility = Visibility.Collapsed;
                    filterParam = UAC.GetInstance().UserId;
                    break;
            }
            UpdateDG(Page);
        }

        private void ParamCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Page = 1;
            filterParam = paramCB.SelectedItem;
            UpdateDG(Page);
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
                    Page = 1;
                    UpdateDG(Page);
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
                    Page = 1;
                    filterParam = (sender as TextBox).Text;
                    UpdateDG(Page);
                }
            }
        }
    }
}
