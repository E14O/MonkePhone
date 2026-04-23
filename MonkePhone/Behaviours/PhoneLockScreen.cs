using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using MonkePhone.Behaviours.UI;
using static MonkePhone.Behaviours.PhoneBehaviour;
using static MonkePhone.Tools.Configuration;
using BepInEx.Configuration;
using MonkePhone.Behaviours.Apps;

namespace MonkePhone.Behaviours
{
    public class PhoneLockScreen : MonoBehaviour
    {
        private Text _timeText, _dayText;
        private DateTime Now => DateTime.Now;
        private string Current => Now.ToString("hh:mm tt");
        private string CurrentDay => Now.ToString("dddd, dd MMMM");

        public void Awake()
        {
            _timeText = transform.Find("time").GetComponent<Text>();
            _dayText = transform.Find("day").GetComponent<Text>();
        }

        public void Update()
        {
            _timeText.text = Current; 
            _dayText.text = CurrentDay;
        }

        public static void SetActionButton()
        {
            switch (AButton.Value)
            {   
                case ActionButton.CameraApp:
                    PhoneManager.Instance.OpenApp(PhoneManager.Instance.GetApp<MonkeGramApp>().AppId);
                    break;

                case ActionButton.GalleryApp:
                    PhoneManager.Instance.OpenApp(PhoneManager.Instance.GetApp<GalleryApp>().AppId);
                    break;

                case ActionButton.ConfigurationApp:
                    PhoneManager.Instance.OpenApp(PhoneManager.Instance.GetApp<ConfigurationApp>().AppId);
                    break;

                case ActionButton.MessagingApp:
                    PhoneManager.Instance.OpenApp(PhoneManager.Instance.GetApp<MessagingApp>().AppId);
                    break;

                case ActionButton.ScoreboardApp:
                    PhoneManager.Instance.OpenApp(PhoneManager.Instance.GetApp<ScoreboardApp>().AppId);
                    break;

                case ActionButton.CreditsApp:
                    PhoneManager.Instance.OpenApp(PhoneManager.Instance.GetApp<CreditsApp>().AppId);
                    break;

                case ActionButton.MusicApp:
                    PhoneManager.Instance.OpenApp(PhoneManager.Instance.GetApp<MusicApp>().AppId);
                    break;
            }
        }
    }
}
