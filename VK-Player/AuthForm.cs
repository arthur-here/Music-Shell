using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace VK_Player
{
    public partial class AuthForm : Form
    {
        private int appId = 3921202;

        private enum appScope
        {
            notify = 1,
            friends = 2,
            photos = 4,
            audio = 8,
            video = 16,
            offers = 32,
            questions = 64,
            pages = 128,
            link = 256,
            notes = 2048,
            messages = 4096,
            wall = 8192,
            docs = 131072
        }

        private int scope = (int)(appScope.audio | appScope.docs | appScope.friends |
            appScope.link | appScope.messages | appScope.notes | appScope.notify |
            appScope.offers | appScope.pages | appScope.photos | appScope.questions |
            appScope.video | appScope.wall);

        public AuthForm()
        {
            InitializeComponent();
        }

        private void AuthForm_Shown(object sender, EventArgs e)
        {
            webBrowser1.Navigate(String.Format("http://api.vkontakte.ru/oauth/authorize?client_id={0}&scope={1}&display=popup&response_type=token",
                appId, scope));
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (e.Url.ToString().IndexOf("access_token") != -1)
            {
                String accessToken = "";
                int userId = 0;
                Regex myReg = new Regex(@"(?<name>[\w\d\x5f]+)=(?<value>[^\x26\s]+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                foreach (Match m in myReg.Matches(e.Url.ToString()))
                {
                    if (m.Groups["name"].Value == "access_token")
                    {
                        accessToken = m.Groups["value"].Value;
                    }
                    else if (m.Groups["name"].Value == "user_id")
                    {
                        userId = Convert.ToInt32(m.Groups["value"].Value);
                    }
                }

                if (String.IsNullOrEmpty(accessToken))
                {
                    System.Windows.MessageBox.Show("Error. Key is not found");
                    this.Close();
                    return;
                }
                
                Properties.Settings.Default.id = Convert.ToString(userId);
                Properties.Settings.Default.token = accessToken;
                Properties.Settings.Default.auth = true;
                this.Close();
            }
        }
    }
}
