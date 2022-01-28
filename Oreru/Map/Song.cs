using System.Collections.Generic;
using Oreru.Utils;

namespace Oreru.Map {
    public class Song {
        public string AudioFileName { get; set; }
        
        public Timestamp AudioOffset { get; set; }
        
        public UnicodeString Title { get; set; }
        
        public UnicodeString Artist { get; set; }
        
        public string Source { get; set; }

        public List<string> Tags { get; set; } = new List<string>();

        public Song(string audioFile, Timestamp audioOffset, UnicodeString title, UnicodeString artist, string source = "") {
            AudioFileName = audioFile;
            AudioOffset = audioOffset;
            Source = source;
            Title = title;
            Artist = artist;
        }
    }
}