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

namespace VK_Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private User user;
        private MediaPlayer player;
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            leftListBox.SelectionChanged += new SelectionChangedEventHandler(leftListBox_SelectionChanged);
            rightListBox.SelectionChanged += new SelectionChangedEventHandler(rightListBox_SelectionChanged);
            player = new MediaPlayer();
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += new EventHandler(timer_Tick);
            player.MediaOpened += new EventHandler(player_MediaOpened);
            player.MediaFailed += new EventHandler<ExceptionEventArgs>(player_MediaFailed);
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void authButton_Click(object sender, RoutedEventArgs e)
        {
            AuthForm authorizationForm = new AuthForm();
            authorizationForm.ShowDialog();

            this.user = loginUser();

            usernameLabel.Content = user.first_name + " " + user.last_name;

            user.setAvatar(avatarImage);

            leftListBox.Items.Add("Все аудиозаписи");

            user.loadAlbums(leftListBox);

            authButton.Visibility = Visibility.Hidden;
        }

        private User loginUser()
        {
            WebRequest usernameRequestServer = WebRequest.Create("https://api.vk.com/method/users.get?user_ids=" + Properties.Settings.Default.id + "&fields=uid,first_name,last_name,photo_50");
            WebResponse usernameResponseServer = usernameRequestServer.GetResponse();
            Stream dataStream = usernameResponseServer.GetResponseStream();
            StreamReader dataReader = new StreamReader(dataStream);
            string usernameResponse = dataReader.ReadToEnd();
            dataReader.Close();
            dataStream.Close();

            usernameResponse = HttpUtility.HtmlDecode(usernameResponse);

            JToken token = JToken.Parse(usernameResponse);

            List<User> returnedUsers = token["response"].Children().Select(c => c.ToObject<User>()).ToList<User>();

            return returnedUsers[0];
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        void leftListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            rightListBox.Items.Clear();

            ListBox list = sender as ListBox;

            if (list.SelectedIndex == 0)
                user.loadAllSongs(200, rightListBox);
            else
            {
                Album selectedAlbum = user.albums[list.SelectedIndex - 1];
                user.loadSongsFromAlbum(selectedAlbum, rightListBox);
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
            }
        }
        
        void player_MediaOpened(object sender, EventArgs e)
        {
            MediaPlayer mp = sender as MediaPlayer;
            String time = mp.NaturalDuration.ToString();
            startTimeLabel.Content = "00:00";
            
            time = time.Remove(time.IndexOf('.'));
            time = time.Remove(0, 3);
            finishTimeLabel.Content = time;

            titleLabel.Content = user.tracks[user.currentSongIndex].artist + " – " + user.tracks[user.currentSongIndex].title;

            trackSlider.Maximum = mp.NaturalDuration.TimeSpan.TotalSeconds;
            trackSlider.Value = 0;

            timer.Start();
        }

        void player_MediaFailed(object sender, ExceptionEventArgs e)
        {
            MessageBox.Show("Error while loading audio", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            trackSlider.Value = player.Position.TotalSeconds;
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
            player.Pause();
            player.Position = TimeSpan.FromSeconds(trackSlider.Value);
            player.Play();
        }
    }
}
