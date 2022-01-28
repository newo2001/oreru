using System;
using System.Linq;
using Oreru.Map;
using BPM;

namespace Oreru.Parsing {
    public class BpmParser : IBeatmapParser {
        public Beatmap Parse(string source) {
            var map = Parser.ParseBeatmap(source);

            var beatmap = new Beatmap();

            var hitObjects = map.hitObjects.Select<BPM.Map.HitObject, HitObject>(x => {
                switch (x.Tag) {
                    case BPM.Map.HitObject.Tags.Circle:
                        var circle = ((BPM.Map.HitObject.Circle) x).Item;
                        return new HitCircle((circle.x, circle.y), circle.time, circle.newCombo);
                    
                    //case BPM.Map.HitObject.Tags.Slider:
                        //var slider = ((BPM.Map.HitObject.Slider) x).Item;
                        //return new Slider((slider.x, slider.y), slider.time, slider.newCombo);
                    
                    case BPM.Map.HitObject.Tags.Spinner:
                        var spinner = ((BPM.Map.HitObject.Spinner) x).Item;
                        return new Spinner(spinner.startTime, spinner.newCombo);
                }

                throw new ArgumentException("Invalid HitObject Type!");
            });
            
            //beatmap.HitObjects.AddRange(hitObjects);

            return beatmap;
        }
    }
}