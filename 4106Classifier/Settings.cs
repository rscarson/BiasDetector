using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4106Classifier {
    class Settings {
        public const string ConfigPath = "config.json";
        private static Settings _instance;
        public static Settings Instance {
            get {
                if (_instance == null) _instance = FromJSON(ConfigPath);
                return _instance;
            }
        }

        public RedditConfig Reddit { get; set; }
        public class RedditConfig {
            public string Username { get; set; }
            public string Password { get; set; }
            public string ClientID { get; set; }
            public string Secret { get; set; }
            public List<string> BannedDomains { get; set; }

            public int PostsToFetch { get; set; }
        }

        public MercuryConfig Mercury { get; set; }
        public class MercuryConfig {
            public string Key { get; set; }
        }
        
        /// <summary>
        /// Load config from JSON
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Settings FromJSON(string path) {
            string json = System.IO.File.ReadAllText(path);
            return JsonConvert.DeserializeObject<Settings>(json);
        }
    }
}
