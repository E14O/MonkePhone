using UnityEngine;

#if PLUGIN
using HarmonyLib;
using System.Reflection;
using MonkePhone.Tools;
#endif

namespace MonkePhone.Behaviours
{
    public class PhoneBehaviour : MonoBehaviour
    {
#if PLUGIN

        public void Vibration(bool isLeftHand, float amplitude, float duration)
        {
            if (!Configuration.AppHaptics.Value)
            {
                return;
            }

            GorillaTagger.Instance.StartVibration(isLeftHand, amplitude, duration);
        }

        public void PlaySound(string soundId, float volume = 1f) => InvokeMethod("PlaySound", soundId, volume);

        public static void InvokeMethod(string methodName, params object[] parameters)
        {
            MethodInfo method = AccessTools.Method(typeof(PhoneManager), methodName);
            method?.Invoke(PhoneManager.Instance, parameters);
        }

        public static void App(string _AppState, string _AppID)
        {
            switch (_AppState)
            {
                case "Open":
                    InvokeMethod("OpenApp", _AppID);
                    break;

                case "Close":
                    InvokeMethod("CloseApp", _AppID);
                    break;
            }
        }
#endif
    }
}
