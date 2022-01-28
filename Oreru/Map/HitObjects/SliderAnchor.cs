using Oreru.Utils;

namespace Oreru.Map {
    public class SliderAnchor {
        private Coordinate _position;
        public Coordinate Position {
            get => _position;
            set => MoveTo(value);
        }
        
        public bool IsRed { get; private set; }
        
        public SliderAnchor(Coordinate position, bool red = false) {
            _position = position;
            IsRed = red;
        }

        public void MoveTo(Coordinate pos) {
            _position = pos;
        }

        public void MakeRed() => IsRed = true;

        public void MakeWhite() => IsRed = false;
    }
}