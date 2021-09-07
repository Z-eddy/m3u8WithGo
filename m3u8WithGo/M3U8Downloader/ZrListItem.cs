using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3U8Downloader
{
    class ZrListItem
    {
        private string filename_;
        private string url_;

        public string Filename
        {
            get
            {
                return filename_;
            }
            set
            {
                filename_ = value;
            }
        }

        public string Url 
        {
            get
            {
                return url_;
            }
            set
            {
                url_ = value;
            }
        }

        public ZrListItem(string filename, string url)
        {
            filename_ = filename;
            url_ = url;
        }

        public override string ToString()
        {
            return filename_;
        }
    }
}
