using MixedReality.Toolkit;
using MixedReality.Toolkit.Subsystems;
using UnityEngine;

namespace DefaultNamespace
{
    public static class Utils
    {
        private static readonly TextToSpeechSubsystem TextToSpeechSubsystem = XRSubsystemHelpers.GetFirstRunningSubsystem<TextToSpeechSubsystem>();
        
        
        public static void Speak(AudioSource audioSource, string text)
        {
            if (TextToSpeechSubsystem != null && audioSource != null) TextToSpeechSubsystem.TrySpeak(text, audioSource);
        }
    }
}