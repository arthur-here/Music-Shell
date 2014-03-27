using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace VK_Player
{
    class User
    {
        public string uid { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string photo_50 { get; set; }

        public void setAvatar(Image im)
        {
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(photo_50);
            bi.EndInit();

            im.Source = bi;
        }
    }
}
