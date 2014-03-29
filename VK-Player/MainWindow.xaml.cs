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

namespace VK_Player
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
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
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

        private void changeSoundButtonImage(int state)
        {
            Uri resourceUri = new Uri("images/soundButton-" + state.ToString() + ".png", UriKind.Relative);
            StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);

            BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
            var brush = new ImageBrush();
            brush.ImageSource = temp;

            soundButton.Background = brush;
        }

        private void soundButton_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double startVolume = player.Volume;
            if (e.Delta > 0 && player.Volume < 1.0)
            {
                player.Volume += 0.05;
                volumeLabel.Content = Convert.ToInt16(player.Volume * 100).ToString();
                if (player.Volume > 0 && startVolume == 0)
                    changeSoundButtonImage(1);
                else if (player.Volume > 0.33 && startVolume < 0.33)
                    changeSoundButtonImage(2);
                else if (player.Volume > 0.66 && startVolume < 0.66)
                    changeSoundButtonImage(3);
                else if (player.Volume == 0)
                    changeSoundButtonImage(0);
            } 
            else if (e.Delta < 0 && player.Volume > 0.0)
            {
                player.Volume -= 0.05;
                volumeLabel.Content = Convert.ToInt16(player.Volume * 100).ToString();
                if (player.Volume < 100 && startVolume == 100)
                    changeSoundButtonImage(3);
                else if (player.Volume < 0.66 && startVolume > 0.66)
                    changeSoundButtonImage(2);
                else if (player.Volume < 0.33 && startVolume > 0.33)
                    changeSoundButtonImage(1);
                else if (player.Volume == 0)
                    changeSoundButtonImage(0);
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


    }
}
