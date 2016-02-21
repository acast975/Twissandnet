using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Twissandnet.Models
{
    public class Tweet
    {
        public string username;
        public string originalText;
        public List<string> hashtags;
        public long timestamp;

        public Tweet()
        {
            hashtags = new List<string>();
        }
    }
}