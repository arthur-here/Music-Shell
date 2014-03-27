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
        User user;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
            leftListBox.Items.Add("Hello");
        }

        private void authButton_Click(object sender, RoutedEventArgs e)
        {
            AuthForm authorizationForm = new AuthForm();
            authorizationForm.ShowDialog();

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

            user = returnedUsers[0];

            usernameLabel.Content = user.first_name + " " + user.last_name;

            user.setAvatar(avatarImage);
        }

        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        

    }
}
