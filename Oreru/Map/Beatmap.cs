using System;
using System.Collections.Generic;
using System.Linq;
using Oreru.Utils;

namespace Oreru.Map {
    public class Beatmap {
        public List<Bookmark> Bookmarks { get; } = new List<Bookmark>();

        private List<HitObject> hitObjects = new List<HitObject>();
        
        public IReadOnlyList<HitObject> HitObjects => hitObjects;

        //private SortedList<RedTimingPoint> redTimingPoints = new SortedList<RedTimingPoint>(Comparer<RedTimingPoint>.Default);

        //public IReadOnlyList<RedTimingPoint> RedTimingPoints => redTimingPoints;
        
        //public List<GreenTimingPoint> GreenTimingPoints => TimingPoints.OfType<GreenTimingPoint>().ToList();
        
        public string Mapper { get; set; }
        public string DifficultyName { get; set; }

        // TODO un-uglify this
        /*public List<HitObject> GetComboOfObject(HitObject hitObject) {
            var index = HitObjects.IndexOf(hitObject);

            var nc = false;
            var preceding = HitObjects.Take(index).Reverse().TakeWhile(x => {
                var passed = nc;
                nc = x.IsNewCombo;
                return !passed;
            }); 
            
            var following = HitObjects.Skip(index + 1).TakeWhile(x => !x.IsNewCombo);
            return preceding.Append(HitObjects[index]).Concat(following).ToList();
        }*/

        /*public GreenTimingPoint GreenTimingPointAt(Timestamp timestamp) {
            return GreenTimingPoints[GreenTimingPoints.FindIndex(x => x.Timestamp > timestamp) - 1] ??
                   GreenTimingPoints.First();
        }

        public RedTimingPoint RedTimingPointAt(Timestamp timestamp) {
            return RedTimingPoints[RedTimingPoints.FindIndex(x => x.Timestamp > timestamp) - 1] ??
                   RedTimingPoints.First();
        }

        public BeatDivider? GetBeatDividerAt(Timestamp timestamp) {
            var timeSignature = RedTimingPointAt(timestamp);
            var offset = timeSignature.Timestamp;
            var msPerBeat = timeSignature.MillisecondsPerBeat;

            if ((timestamp.Millis - offset.Millis) % msPerBeat == 0) return BeatDivider.FullBeat;
            //TODO: implement other beat dividers

            return null;
        }

        public bool IsObjectSnapped(HitObject hitObject, BeatDivider precision = BeatDivider.OneFourth) {
            //TODO: implement precision
            return GetBeatDividerAt(hitObject.Timestamp) != null;
        }*/

        public bool IsObjectSnapped(Slider slider) {
            throw new NotImplementedException("Fuck off");
        }
        
        public Beatmap() {
            //HitObjects.Add(new HitCircle((15, 5), 1000));
        }
    }
}