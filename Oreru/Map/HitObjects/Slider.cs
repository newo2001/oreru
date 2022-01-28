using System;
using System.Collections.Generic;
using System.Linq;
using Oreru.Utils;

namespace Oreru.Map {
    public class Slider : HitObject {
        public List<SliderAnchor> Anchors { get; } = new List<SliderAnchor>();
        public SliderCurveType CurveType { get; set; }
        
        public double Length { get; set; }
        public Timestamp Duration => throw new NotImplementedException("Fuck off");

        public Coordinate TailPosition => Anchors.Last().Position;

        public uint ReturnCount { get; set; } = 0;
        public bool IsReturnSlider => ReturnCount > 0;

        public Slider(Coordinate position, Timestamp time, double length, bool newCombo = false) : base(position, time, newCombo) {
            Length = length;
        }

        public override string ToString() {
            return $"<Slider> {Position} @ {Timestamp}";
        }
    }
}