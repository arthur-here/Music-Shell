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

namespace VK_Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private User user;
        private MediaPlayer player;

        public MainWindow()
        {
            InitializeComponent();
            leftListBox.SelectionChanged += new SelectionChangedEventHandler(leftListBox_SelectionChanged);
            rightListBox.SelectionChanged += new SelectionChangedEventHandler(rightListBox_SelectionChanged);
            player = new MediaPlayer();
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

            player.Open(new Uri(user.tracks[list.SelectedIndex].url));
            player.Play();
        }
        

    }
}
