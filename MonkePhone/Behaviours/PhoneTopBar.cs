using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace MonkePhone.Behaviours
{
    public class PhoneTopBar : MonoBehaviour
    {
        private Text _TimeText;
        private Slider _BatteryLevel;
        private Image _BatteryGraphic;

        private DateTime _Now => DateTime.Now;
        private string _Current => _Now.ToString("hh:mm tt");
        private float _Battery
        {
            get
            {
                ControllerInputPoller.instance.headDevice.TryGetFeatureValue(CommonUsages.batteryLevel, out float value);
                return value;
            }
        }

        public void Awake()
        {
            _TimeText = transform.Find("Time Text").GetComponent<Text>();
            _BatteryLevel = transform.Find("Slider").GetComponent<Slider>();
            _BatteryGraphic = transform.Find("Slider/Fill Area/Fill").GetComponent<Image>();
        }

        public void Update()
        {
            _TimeText.text = _Current;
            _BatteryLevel.value = _Battery;
            _BatteryGraphic.color = Color.Lerp(Color.red, Color.green, _Battery);
        }
    }
}