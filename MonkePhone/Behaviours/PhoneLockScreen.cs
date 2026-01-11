using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using MonkePhone.Behaviours.UI;
using static MonkePhone.Behaviours.PhoneBehaviour;
using static MonkePhone.Tools.Configuration;
using BepInEx.Configuration;

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
                    App("Open", "MonkeGram");
                    break;

                case ActionButton.GalleryApp:
                    App("Open", "Gallery");
                    break;

                case ActionButton.ConfigurationApp:
                    App("Open", "Configuration");
                    break;

                case ActionButton.MessagingApp:
                    App("Open", "Messaging");
                    break;

                case ActionButton.ScoreboardApp:
                    App("Open", "Scoreboard");
                    break;

                case ActionButton.CreditsApp:
                    App("Open", "Credits");
                    break;

                case ActionButton.MusicApp:
                    App("Open", "Music");
                    break;
            }
        }
    }
}
