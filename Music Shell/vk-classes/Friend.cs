using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Net;
using System.IO;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Music_Shell
{
    class Friend
    {
        public string uid;
        public string first_name;
        public string last_name;

        public List<Track> tracks;
        public int currentSongIndex = -1;

        public void loadAllSongs(int numOfLoadedSongs, ListBox lb, List<Track> tracksList)
        {
            try 
            {
                WebRequest tracksRequestServer = WebRequest.Create("https://api.vk.com/method/audio.get?owner_id=" + uid + "&count=" + numOfLoadedSongs + "&access_token=" + Properties.Settings.Default.token);
                WebResponse tracksResponseServer = tracksRequestServer.GetResponse();
                Stream dataStream = tracksResponseServer.GetResponseStream();
                StreamReader dataReader = new StreamReader(dataStream);
                string tracksResponse = dataReader.ReadToEnd();
                dataReader.Close();
                dataStream.Close();

                tracksResponse = HttpUtility.HtmlDecode(tracksResponse);

                JToken token = JToken.Parse(tracksResponse);

                this.tracks = token["response"].Children().Skip(1).Select(c => c.ToObject<Track>()).ToList<Track>();

                tracksList.Clear();
                foreach (Track tr in this.tracks)
                {
                    lb.Items.Add(tr.artist + " – " + tr.title);
                    tracksList.Add(tr);
                }
            } catch(Exception ex)
            {
            }
            
        }
    }
}
