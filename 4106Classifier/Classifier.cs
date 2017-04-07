using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4106Classifier {
    class Classifier {
        public int SequenceLength { get; set; }
        public int Classes { get; set; }
        public List<Filter> Filters { get; set; }
        public double DropoutChance { get; set; }

        /// <summary>
        /// Save current configuration to JSON
        /// </summary>
        /// <param name="path">Path to configuration file</param>
        public void ToJSON(string path) {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            System.IO.File.WriteAllText(path, json);
        }

        /// <summary>
        /// Restore saved configuration
        /// </summary>
        /// <param name="path">Path to configuration file</param>
        /// <returns>A configured instance</returns>
        public static Classifier FromJSON(string path) {
            string json = System.IO.File.ReadAllText(path);
            return JsonConvert.DeserializeObject<Classifier>(json);
        }

        /// <summary>
        /// Filters for the convololutional layer
        /// </summary>
        public class Filter {
            public int Size { get; set; }
            public int Count { get; set; }
        } 
    }
}
