using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Voxalia.Shared;
using System.Speech.Synthesis;

namespace Voxalia.ClientGame.AudioSystem
{
    public class TextToSpeech
    {
        public static bool TrySpeech = true;

        public static void Speak(string text, bool male)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (TrySpeech)
                    {
                        SpeechSynthesizer speech = new SpeechSynthesizer();
                        VoiceInfo vi = null;
                        foreach (InstalledVoice v in speech.GetInstalledVoices())
                        {
                            if (!v.Enabled)
                            {
                                continue;
                            }
                            if (vi == null)
                            {
                                vi = v.VoiceInfo;
                            }
                            else if ((male && v.VoiceInfo.Gender == VoiceGender.Male) || (!male && v.VoiceInfo.Gender == VoiceGender.Female))
                            {
                                vi = v.VoiceInfo;
                                break;
                            }
                        }
                        if (vi == null)
                        {
                            TrySpeech = false;
                        }
                        else
                        {
                            speech.SelectVoice(vi.Name);
                            speech.Speak(text);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utilities.CheckException(ex);
                    TrySpeech = false;
                    Process p = Process.Start("espeak", "\"" + text.Replace("\"", " quote ") + "\"");
                    Console.WriteLine(p.MainModule.FileName);
                }
            });
        }
    }
}
