using System;
using System.Collections;

namespace Oreru.Map {
    public class HitSound {
        [Flags]
        public enum Sound {
            Normal  = 0b00000001,
            Whistle = 0b00000010,
            Finish  = 0b00000100,
            Clap    = 0b00001000
        }

        private Sound _sound;
        
        public HitSound(Sound sound) {
            _sound = sound;
        }
        
        public bool HasSounds(Sound sound) => _sound.HasFlag(sound);

        public void AddSounds(Sound sound) => _sound |= sound;

        public void RemoveSounds(Sound sound) => _sound &= ~sound;
    }
}