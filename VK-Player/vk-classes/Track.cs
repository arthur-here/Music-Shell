using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music_Shell
{
    class Track
    {
        public string id;
        public string artist;
        public string title;
        public int duration
        {
            set
            {
                Convert.ToInt32(value);
            }
        }
        public string url;
    }
}
