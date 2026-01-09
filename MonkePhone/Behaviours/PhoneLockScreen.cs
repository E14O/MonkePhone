using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using MonkePhone.Behaviours.UI;

namespace MonkePhone.Behaviours
{
    public class LockScreen : MonoBehaviour
    {
        private const string V = "Gallery";
        private GameObject _ActionButton;
        private Text _timeText, _dayText;
        private DateTime Now => DateTime.Now;
        private string Current => Now.ToString("hh:mm tt");
        private string CurrentDay => Now.ToString("dddd, dd MMMM");

        public string ABFunction = "";

        public void Awake()
        {
            _ActionButton = transform.Find("").gameObject;
            _timeText = transform.Find("time").GetComponent<Text>();
            _dayText = transform.Find("day").GetComponent<Text>();
        }

        public void Update()
        {
            _timeText.text = Current; 
            _dayText.text = CurrentDay;
        }

        public void SetActionButton()
        {
            switch (ABFunction)
            {
                case "Camera":
                    break;

                case V:
                    break;

                case "Settings":
                    break;

            }
        }
    }
}
