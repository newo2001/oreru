namespace Oreru.Utils {
    public class UnicodeString {

        private string _unicode;
        public string Unicode {
            get => _unicode ?? Romanised;
            set => _unicode = value;
        }
        
        public string Romanised { get; set; }

        public UnicodeString(string romanised, string unicode = "") {
            Romanised = romanised;
            _unicode = unicode;
        }
        
        public static implicit operator UnicodeString((string, string) pair) {
            var (romanised, unicode) = pair;
            return new UnicodeString(romanised, unicode);
        }

        public void Deconstruct(out string romanised, out string unicode) {
            romanised = Romanised;
            unicode = _unicode;
        }

        public override string ToString() => Unicode;
    }
}