using System.IO;
using Oreru.Map;

namespace Oreru.Parsing {
    public class BeatmapParser {
        private readonly IBeatmapParser _beatmapParser;
        
        public BeatmapParser(IBeatmapParser parser) {
            _beatmapParser = parser;
        }

        public Beatmap Parse(string source) {
            return _beatmapParser.Parse(source);
        }

        public Beatmap ParseFile(string path) {
            try {
                // TODO read async
                var content = File.ReadAllText(path);
                return Parse(content);
            } catch (IOException e) {
                throw new IOException($"Failed to open beatmap: {path}", e);
            }
        }
    }
}