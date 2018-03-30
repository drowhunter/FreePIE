using System;
using System.Linq;
using System.Speech.Synthesis;

namespace JonesCorp
{
    public static class Voice
    {
        private static SpeechSynthesizer _synth;
        private static SpeechSynthesizer synth
        {
            get
            {
                if (_synth == null)
                {
                    _synth = new SpeechSynthesizer();
                    var femaieVoice = _synth.GetInstalledVoices().FirstOrDefault(v => v.VoiceInfo.Gender == VoiceGender.Female);
                    if (femaieVoice != null)
                    {
                        var name = femaieVoice.VoiceInfo.Name;
                        _synth.SelectVoice(name);

                    }
                }

                return _synth;
            }
        }
        public static void Speak(this string self)
        {
            if (!String.IsNullOrWhiteSpace(self) && synth.State == SynthesizerState.Ready && synth.State != SynthesizerState.Speaking)
                synth.SpeakAsync(self);
        }

        public static void DisposeVoice()
        {
            try
            {
                if (_synth != null)
                {
                    ((IDisposable)_synth).Dispose();
                    _synth = null;
                }
            }
            catch { }
        }
    }
}
