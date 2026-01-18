using System.Collections.Generic;
using System.Linq;
using MonkePhone.Tools;
using UnityEngine;

namespace MonkePhone.Behaviours
{
    public class DummyPhoneManager : MonoBehaviour
    {
        private readonly Dictionary<string, GameObject> _dummyApps = [];

        private static readonly HashSet<string> Ignored =
        [
            "Lock Screen",
            "Home Screen",
            "Top Bar",
            "MonkeGramApp"
        ];

        private Transform _canvas;

        public void Initialize(GameObject phone)
        {
            _canvas = phone.transform.Find("Canvas");

            if (_canvas == null) return;

            foreach (Transform child in _canvas)
            {
                if (child.name != "Logo" && child.name != "X")
                {
                    _dummyApps[child.name] = child.gameObject;
                    child.gameObject.SetActive(false);
                }
            }
        }
        
        public void OpenDummyApp(string _appId)
        {
            CloseDummyApps();

            if (string.IsNullOrEmpty(_appId)) return;

            string _appKey = _appId;

            if (!Ignored.Contains(_appId))
                _appKey += "App";

            if (_dummyApps.TryGetValue(_appKey, out GameObject app))
            {
                _canvas.Find("Top Bar")?.gameObject.SetActive(true);

                app.SetActive(true);
            }
            else
            {
                Logging.Warning($"Unable to open dummy app containing phone appId: {_appId}");
            }
        }

        public void CloseDummyApps() => _dummyApps.Values.ToList().ForEach(app => app.SetActive(false));
    }
}
