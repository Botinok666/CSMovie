using Movies.UWP.Controller;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class Profile : Page
    {
        public Profile()
        {
            InitializeComponent();
            if (Util.UAC.GetInstance().UserRole == Model.Roles.ROLE_USER)
                addUserPanel.Visibility = Visibility.Collapsed;

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

        private void ShowStats_Click(object sender, RoutedEventArgs e)
        {
            ShowStats.Visibility = Visibility.Collapsed;
            var statistics = MoviesController.GetInstance().GetUserStatistics();
            tMovieCnt.Text = statistics.MoviesCount.ToString();
            tViewCnt.Text = statistics.ViewingsCount.ToString();
            tTotalRuntime.Text = string.Format("{0} д. {1} ч.", 
                (int)statistics.TotalRuntime.TotalDays, statistics.TotalRuntime.Hours);
            tGenresRate.Text = string.Join(", ", statistics.GenrePercentage
                .Select(x => string.Format("{0} ({1:P1})", x.Item1, x.Item2 / statistics.MoviesCount)));
            StatsGrid.Visibility = Visibility.Visible;
        }
        private void ChangePwd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(newPass.Password))
            {
                InfoText.Text = "Поле не должно быть пустым";
                InfoFlyout.ShowAt(newPass);
                return;
            }
            if (!MoviesController.GetInstance().ChangeUserPwd(oldPass.Password, newPass.Password))
            {
                InfoText.Text = "Неверный старый пароль";
                InfoFlyout.ShowAt(oldPass);
                oldPass.Password = "";
            }
            else
            {
                InfoText.Text = "Установлен новый пароль";
                InfoFlyout.ShowAt(newPass);
                oldPass.Password = "";
                newPass.Password = "";
            }
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(userName.Text))
            {
                InfoText.Text = "Поле не должно быть пустым";
                InfoFlyout.ShowAt(userName);
                return;
            }
            if (!MoviesController.GetInstance().AddUser(userName.Text))
                InfoText.Text = "Логин уже занят";
            else
                InfoText.Text = "Пользователь создан с пустым паролем";
            InfoFlyout.ShowAt(userName);
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
}
