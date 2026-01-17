using System.Collections.Generic;
using System.Linq;
using MonkePhone.Networking;
using MonkePhone.Tools;
using UnityEngine;

namespace MonkePhone.Behaviours
{
    public class DummyPhoneManager : MonoBehaviour
    {
        // right now its not working cause im pretty sure its to do with the canvas or something i have a idea on why its not working ask me tomorrow.
        // also we should move all dummy phone stuff here to do with the pages yk like the flip zoom grab stuff. just look what i set up in the cs this way will be so much better.

        private readonly Dictionary<string, GameObject> _dummyApps = [];

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

        public void OpenDummyApp(string appId)
        {
            CloseDummyApps();

            if (string.IsNullOrEmpty(appId)) return;

            string _appKey = appId;

            if (appId != "Lock Screen" && appId != "Home Screen" && appId != "MonkeGramApp")
            {
                _appKey = _appKey + "App";
            }

            if (_dummyApps.TryGetValue(_appKey, out GameObject app))
            {
                _canvas.transform.Find("Top Bar").gameObject.SetActive(true);
                app.SetActive(true);
            }
            else
            {
                Logging.Warning($"Unable to open dummy app containing appId: {appId}");
            }
        }

        public void CloseDummyApps() => _dummyApps.Values.ToList().ForEach(app => app.SetActive(false));
    }
}
