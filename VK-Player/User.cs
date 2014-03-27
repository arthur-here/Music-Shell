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

        public void setAvatar(Image im)
        {
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(photo_50);
            bi.EndInit();

            im.Source = bi;
        }

        public void getAlbums(ListBox lb)
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

            foreach(Album al in this.albums)
            {
                lb.Items.Add(al.title);
            }
        }

    }
}
