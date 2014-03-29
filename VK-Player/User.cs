using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using System.Web;

namespace VK_Player
{
    class User
    {
        public string uid { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string photo_50 { get; set; }

        public List<Album> albums;
        public List<Track> tracks;
        public List<Friend> friends;

        public int currentSongIndex = -1;
        public int currentFriendIndex = -1;

        public bool auth(Label usernameLabel, Image avatarImage, ListBox albumsListBox, ListBox songsListBox)
        {
            try
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

                this.uid = returnedUsers[0].uid;
                this.first_name = returnedUsers[0].first_name;
                this.last_name = returnedUsers[0].last_name;
                this.photo_50 = returnedUsers[0].photo_50;

                usernameLabel.Content = this.first_name + " " + this.last_name;

                this.setAvatar(avatarImage);

                albumsListBox.Items.Clear();
                this.loadAlbums(albumsListBox);

                songsListBox.Items.Clear();
                this.loadAllSongs(200, songsListBox);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void setAvatar(Image im)
        {
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(photo_50);
            bi.EndInit();

            im.Source = bi;
        }

        public void loadAlbums(ListBox lb)
        {
            WebRequest albumsRequestServer = WebRequest.Create("https://api.vk.com/method/audio.getAlbums?owner_id=" + Properties.Settings.Default.id + "&access_token=" + Properties.Settings.Default.token);
            WebResponse albumsResponseServer = albumsRequestServer.GetResponse();
            Stream dataStream = albumsResponseServer.GetResponseStream();
            StreamReader dataReader = new StreamReader(dataStream);
            string albumsResponse = dataReader.ReadToEnd();
            dataReader.Close();
            dataStream.Close();

            albumsResponse = HttpUtility.HtmlDecode(albumsResponse);

            JToken token = JToken.Parse(albumsResponse);

            this.albums = token["response"].Children().Skip(1).Select(c => c.ToObject<Album>()).ToList<Album>();

            lb.Items.Add("Все аудиозаписи");

            foreach(Album al in this.albums)
            {
                lb.Items.Add(al.title);
            }
        }

        public void loadSongsFromAlbum(Album al, ListBox lb)
        {
            WebRequest tracksRequestServer = WebRequest.Create("https://api.vk.com/method/audio.get?owner_id=" + Properties.Settings.Default.id + "&album_id=" + al.album_id + "&access_token=" + Properties.Settings.Default.token);
            WebResponse tracksResponseServer = tracksRequestServer.GetResponse();
            Stream dataStream = tracksResponseServer.GetResponseStream();
            StreamReader dataReader = new StreamReader(dataStream);
            string tacksResponse = dataReader.ReadToEnd();
            dataReader.Close();
            dataStream.Close();

            tacksResponse = HttpUtility.HtmlDecode(tacksResponse);

            JToken token = JToken.Parse(tacksResponse);

            this.tracks = token["response"].Children().Skip(1).Select(c => c.ToObject<Track>()).ToList<Track>();

            foreach(Track tr in this.tracks)
            {
                lb.Items.Add(tr.artist + " – " + tr.title);
            }
        }

        public void loadAllSongs(int numOfLoadedSongs, ListBox lb)
        {
            WebRequest tracksRequestServer = WebRequest.Create("https://api.vk.com/method/audio.get?owner_id=" + Properties.Settings.Default.id + "&count=" + numOfLoadedSongs + "&access_token=" + Properties.Settings.Default.token);
            WebResponse tracksResponseServer = tracksRequestServer.GetResponse();
            Stream dataStream = tracksResponseServer.GetResponseStream();
            StreamReader dataReader = new StreamReader(dataStream);
            string tracksResponse = dataReader.ReadToEnd();
            dataReader.Close();
            dataStream.Close();

            tracksResponse = HttpUtility.HtmlDecode(tracksResponse);

            JToken token = JToken.Parse(tracksResponse);

            this.tracks = token["response"].Children().Skip(1).Select(c => c.ToObject<Track>()).ToList<Track>();

            foreach (Track tr in this.tracks)
            {
                lb.Items.Add(tr.artist + " – " + tr.title);
            }
        }

        public void loadFriends(ListBox lb)
        {
            WebRequest friendsRequestServer = WebRequest.Create("https://api.vk.com/method/friends.get?user_id=" + Properties.Settings.Default.id + "&fields=city" + "&access_token=" + Properties.Settings.Default.token);
            WebResponse friendsResponseServer = friendsRequestServer.GetResponse();
            Stream dataStream = friendsResponseServer.GetResponseStream();
            StreamReader dataReader = new StreamReader(dataStream);
            string friendsResponse = dataReader.ReadToEnd();
            dataReader.Close();
            dataStream.Close();

            friendsResponse = HttpUtility.HtmlDecode(friendsResponse);

            JToken token = JToken.Parse(friendsResponse);

            this.friends = token["response"].Children().Skip(1).Select(c => c.ToObject<Friend>()).ToList<Friend>();

            foreach (Friend fr in this.friends)
            {
                lb.Items.Add(fr.first_name + " " + fr.last_name);
            }
        }

        public bool checkToken()
        {
            try
            {
                WebRequest tracksRequestServer = WebRequest.Create("https://api.vk.com/method/audio.get?owner_id=" + Properties.Settings.Default.id + "&count=" + 1 + "&access_token=" + Properties.Settings.Default.token);
                WebResponse tracksResponseServer = tracksRequestServer.GetResponse();
                Stream dataStream = tracksResponseServer.GetResponseStream();
                StreamReader dataReader = new StreamReader(dataStream);
                string tacksResponse = dataReader.ReadToEnd();
                dataReader.Close();
                dataStream.Close();

                tacksResponse = HttpUtility.HtmlDecode(tacksResponse);

                JToken token = JToken.Parse(tacksResponse);

                List<Track> tTracks = token["response"].Children().Skip(1).Select(c => c.ToObject<Track>()).ToList<Track>();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
