//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Voxalia.Shared;
#if WINDOWS
using System.Speech.Synthesis;
#endif

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
#if WINDOWS
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
#endif
                    TrySpeech = false;
                }
                catch (Exception ex)
                {
                    Utilities.CheckException(ex);
                    TrySpeech = false;
                }
                if (!TrySpeech)
                {
                    String addme = male ? " -p 40" : " -p 95";
                    Process p = Process.Start("espeak", "\"" + text.Replace("\"", " quote ") + "\"" + addme);
                    Console.WriteLine(p.MainModule.FileName);
                }
            });
        }
    }
}
