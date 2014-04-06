using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using System.Web;
using System.Windows.Threading;
using System.Windows.Resources;

namespace Music_Shell
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    enum MediaState { Play, Pause }
    enum AppState { UserSongs, Friends, GlobalSearch}

    public partial class MainWindow : Window
    {
        private User user;
        private MediaPlayer player;
        private DispatcherTimer timer;
        private MediaState state = MediaState.Pause;
        private AppState appState = AppState.UserSongs;
        DispatcherTimer titleTimer = new DispatcherTimer();
        double offset = 0.0;

        public MainWindow()
        {
            InitializeComponent();
            leftListBox.SelectionChanged += new SelectionChangedEventHandler(leftListBox_SelectionChanged);
            rightListBox.SelectionChanged += new SelectionChangedEventHandler(rightListBox_SelectionChanged);
            player = new MediaPlayer();
            timer = new DispatcherTimer();
            user = new User();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += new EventHandler(timer_Tick);
            player.MediaOpened += new EventHandler(player_MediaOpened);
            player.MediaEnded += new EventHandler(player_MediaEnded);
            player.MediaFailed += new EventHandler<ExceptionEventArgs>(player_MediaFailed);
            trackSlider.Tag = true;
            titleTimer.Tick += new EventHandler(t_Tick);
            titleTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            titleTimer.Start(); 
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (searchTextBox.IsFocused)
            {
                searchTextBox.Visibility = Visibility.Hidden;
            }
            this.DragMove();
        }

        private void authButton_Click(object sender, RoutedEventArgs e)
        {
            AuthForm authorizationForm = new AuthForm();
            authorizationForm.ShowDialog();

            if (user.auth(usernameLabel, avatarImage, leftListBox, rightListBox))
                authButton.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));     
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            TrayIcon.Icon = null;
            Application.Current.Shutdown();
        }

        void leftListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            rightListBox.Items.Clear();
            ListBox list = sender as ListBox;
            if (list.Items.Count != 0)
            {
                switch (appState)
                {
                    case AppState.UserSongs:
                        if (list.SelectedIndex == 0)
                            user.loadAllSongs(200, rightListBox);
                        else
                        {
                            Album selectedAlbum = user.albums[list.SelectedIndex - 1];
                            user.loadSongsFromAlbum(selectedAlbum, rightListBox);
                        }
                        break;
                    case AppState.Friends:
                        user.currentFriendIndex = list.SelectedIndex;
                        user.friends[user.currentFriendIndex].loadAllSongs(200, rightListBox, user.tracks);
                        break;
                }
            }    
        }

        void rightListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox list = sender as ListBox;
            
            if (list.Items.Count != 0)
            {
                user.currentSongIndex = list.SelectedIndex;
                player.Open(new Uri(user.tracks[user.currentSongIndex].url));

                player.Play();
                state = MediaState.Play;
                playButton.Style = FindResource("pauseButton") as Style;
            }
        }
        
        void player_MediaOpened(object sender, EventArgs e)
        {
            MediaPlayer mp = sender as MediaPlayer;
            String time = mp.NaturalDuration.ToString();
            startTimeLabel.Content = "00:00";
            offset = 0.0;

            state = MediaState.Play;

            time = time.Remove(time.IndexOf('.'));
            time = time.Remove(0, 3);
            finishTimeLabel.Content = time;

            titleLabel.Content = user.tracks[user.currentSongIndex].artist + " – " + user.tracks[user.currentSongIndex].title;

            trackSlider.Maximum = mp.NaturalDuration.TimeSpan.TotalSeconds;
            trackSlider.Value = 0;

            timer.Start();
            mp.Play();
        }

        void player_MediaEnded(object sender, EventArgs e)
        {
            if (user.currentSongIndex != user.tracks.Count()-1)
            {
                user.currentSongIndex++;
                player.Open(new Uri(user.tracks[user.currentSongIndex].url));
                rightListBox.SelectedIndex = user.currentSongIndex;
            }
        }

        void player_MediaFailed(object sender, ExceptionEventArgs e)
        {
            MessageBox.Show("Error while loading audio", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (Convert.ToBoolean(trackSlider.Tag) != false)
            {
                trackSlider.Value = player.Position.TotalSeconds;
            }
            int currentSeconds = Convert.ToInt32(player.Position.TotalSeconds);
            int currentMinutes = currentSeconds / 60;
            currentSeconds = currentSeconds % 60;
            string min = currentMinutes.ToString();
            string sec = currentSeconds.ToString();
            min = (min.Length > 1) ? min : ("0" + min);
            sec = (sec.Length > 1) ? sec : ("0" + sec);
            startTimeLabel.Content = min + ":" + sec;
        }

        private void trackSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            trackSlider.Tag = true;
            player.Pause();
            player.Position = TimeSpan.FromSeconds(trackSlider.Value);
            player.Play();
        }
        
        private void trackSlider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            trackSlider.Tag = false;
        }

        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (user.checkToken())
            {
                if (user.auth(usernameLabel, avatarImage, leftListBox, rightListBox))
                    authButton.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            }
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if (state == MediaState.Pause)
            {
                user.currentSongIndex = (user.currentSongIndex < 0) ? 0 : user.currentSongIndex;

                if (player.Source != null)
                {
                    player.Play();
                    state = MediaState.Play;
                    playButton.Style = FindResource("pauseButton") as Style;
                } else if (user.tracks[user.currentSongIndex] != null)
                {
                    player.Open(new Uri(user.tracks[user.currentSongIndex].url));
                    state = MediaState.Play;
                    playButton.Style = FindResource("pauseButton") as Style;
                }
            }
            else
            {
                state = MediaState.Pause;
                player.Pause();
                playButton.Style = FindResource("playButton") as Style;
            }
        }

        private void soundButton_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double startVolume = player.Volume;
            if (e.Delta > 0 && player.Volume < 1.0)
            {
                player.Volume += 0.05;
                volumeLabel.Content = Convert.ToInt16(player.Volume * 100).ToString();
                if (player.Volume > 0 && startVolume == 0)
                    soundButton.Style = FindResource("soundButton1") as Style;
                else if (player.Volume > 0.33 && startVolume < 0.33)
                    soundButton.Style = FindResource("soundButton2") as Style;
                else if (player.Volume > 0.66 && startVolume < 0.66)
                    soundButton.Style = FindResource("soundButton3") as Style;
                else if (player.Volume == 0)
                    soundButton.Style = FindResource("soundButton0") as Style;
            } 
            else if (e.Delta < 0 && player.Volume > 0.0)
            {
                player.Volume -= 0.05;
                volumeLabel.Content = Convert.ToInt16(player.Volume * 100).ToString();
                if (player.Volume < 100 && startVolume == 100)
                    soundButton.Style = FindResource("soundButton3") as Style;
                else if (player.Volume < 0.66 && startVolume > 0.66)
                    soundButton.Style = FindResource("soundButton2") as Style;
                else if (player.Volume < 0.33 && startVolume > 0.33)
                    soundButton.Style = FindResource("soundButton1") as Style;
                else if (player.Volume == 0)
                    soundButton.Style = FindResource("soundButton0") as Style;
            }
        }

        private void prevButton_Click(object sender, RoutedEventArgs e)
        {
            if (player.Position.TotalSeconds > 5)
            {
                player.Position = TimeSpan.FromMilliseconds(0);
            }
            else if (user.currentSongIndex != 0)
            {
                user.currentSongIndex--;
                player.Open(new Uri(user.tracks[user.currentSongIndex].url));
                rightListBox.SelectedIndex = user.currentSongIndex;
            }
            else
            {
                player.Pause();
                player.Position = TimeSpan.FromMilliseconds(0);
                state = MediaState.Pause;
                playButton.Style = FindResource("playButton") as Style;
            }
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if (user.currentSongIndex < user.tracks.Count()-1)
            {
                user.currentSongIndex++;
                player.Open(new Uri(user.tracks[user.currentSongIndex].url));
                rightListBox.SelectedIndex = user.currentSongIndex;
            } else
            {
                user.currentSongIndex = 0;
                player.Open(new Uri(user.tracks[user.currentSongIndex].url));
                rightListBox.SelectedIndex = user.currentSongIndex;
            }
        }

        private void friendsButton_Click(object sender, RoutedEventArgs e)
        {
            leftListBox.Items.Clear();
            rightListBox.Items.Clear();
            user.loadFriends(leftListBox);
            leftLabel.Content = "друзья";
            appState = AppState.Friends;
        }

        private void userTracksButton_Click(object sender, RoutedEventArgs e)
        {
            leftListBox.Items.Clear();
            rightListBox.Items.Clear();
            user.loadAlbums(leftListBox);
            appState = AppState.UserSongs;
            leftLabel.Content = "альбомы";
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            searchTextBox.Visibility = Visibility.Visible;
            searchTextBox.Focus();
            searchTextBox.SelectAll();
        }

        private void searchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                searchTextBox.Visibility = Visibility.Hidden;

                if (searchTextBox.Text != "")
                {
                    leftListBox.Items.Clear();
                    rightListBox.Items.Clear();
                    user.globalSearch(searchTextBox.Text, 200, rightListBox);
                }
            }
        }

        private void searchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            searchTextBox.Visibility = Visibility.Hidden;
        }

        private void downloadButton_Click(object sender, RoutedEventArgs e)
        {
            user.save(rightListBox.SelectedIndex);
        }

        #region Tray
        // override для расширения абстрактной реализации метода( унаследовано от абстрактного класса) поэтому override
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e); // базовый функционал приложения в момент запуска для возможности последующего возврата
            createTrayIcon();
        }
        private System.Windows.Forms.NotifyIcon TrayIcon = null;
        private System.Windows.Controls.ContextMenu TrayMenu = null;

        private bool createTrayIcon()
        {
            bool result = false;
            if (TrayIcon == null)
            {
                TrayIcon = new System.Windows.Forms.NotifyIcon();
                TrayIcon.Icon = Music_Shell.Properties.Resources.icon32;
                TrayIcon.Text = (titleLabel.Content.ToString() == "") ? ("Music Shell") : (titleLabel.Content.ToString());
                TrayMenu = Resources["TrayMenu"] as System.Windows.Controls.ContextMenu;// создание контекстного меню трея
                TrayIcon.Click += delegate(object sender, EventArgs e)//Делегат обрабатывающий щелчок мыши
                {
                    if ((e as System.Windows.Forms.MouseEventArgs).Button == System.Windows.Forms.MouseButtons.Left)
                    {
                        ShowHideMainWindow(sender, null);
                    }
                    else
                    {
                        TrayMenu.IsOpen = true;
                        Activate();
                    }
                };
                result = true;
            }
            else
            {
                result = true;
            }
            TrayIcon.Visible = true;
            return result;
        }
        private void ShowHideMainWindow(object sender, RoutedEventArgs e)
        {
            TrayMenu.IsOpen = false;
            if (IsVisible)
            {
                Hide();

                (TrayMenu.Items[0] as System.Windows.Controls.MenuItem).Header = "Показать";//Изменение надписи на пункте контектного меню
            }
            else
            {
                Show();

                (TrayMenu.Items[0] as System.Windows.Controls.MenuItem).Header = "Скрыть";//Изменение надписи на пункте контектного меню
                WindowState = CurrentWindowState;
                Activate();
            }
        }
        //Отмена отображения вкладки на панели задач
        private WindowState fCurrentWindowState = WindowState.Normal;
        public WindowState CurrentWindowState
        {
            get { return fCurrentWindowState; }
            set { fCurrentWindowState = value; }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            if (this.WindowState == System.Windows.WindowState.Minimized)
            {
                Hide();//Если минимизированно окно

                (TrayMenu.Items[0] as System.Windows.Controls.MenuItem).Header = "Показать";
            }
            else
            {
                CurrentWindowState = WindowState;//Запоминание текущего состояния окна
            }
        }
        #endregion

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow sw = new SettingsWindow();
            sw.Left = Application.Current.MainWindow.Left + 20;
            sw.Top = Application.Current.MainWindow.Top + 30;
            sw.ShowDialog();
            sw = null;
        }

        
        private void titleLabel_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        void t_Tick(object sender, EventArgs e)
        {
            if (offset < titleLabelScroll.ScrollableWidth)
            {
                offset += 3;
                titleLabelScroll.ScrollToHorizontalOffset(offset);
            } else if (titleLabel.Width < titleLabelScroll.Width)
            {
                ;
            } else
            {
                offset = 0;
            }
        }
    }
}
