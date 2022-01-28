using Oreru.Map;

namespace Oreru.Parsing {
    public interface IBeatmapParser {
        Beatmap Parse(string source);
    }
}