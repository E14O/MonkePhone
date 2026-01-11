using System.IO;
using GorillaNetworking;
using MonkePhone.Behaviours.Apps;
using MonkePhone.Behaviours.UI;
using MonkePhone.Models;
using static MonkePhone.Behaviours.PhoneBehaviour;
using UnityEngine;

namespace MonkePhone.Behaviours
{
    public class PhoneStandBy : MonoBehaviour
    {
        private bool _MonkeGramOpen = false;
        private bool _GUIShow = false;

        /// <summary>
        /// This is for testing the post system and other systems on computer. (It does not send the photo to a server it only works if a webhook is set)
        /// </summary>
        public void OnGUI()
        {
            if (_GUIShow)
            {
                if (GUI.Button(new Rect(128f, 210f, 150f, 35f), "HomeButton")) PhoneManager.Instance.SetHome();

                if (GUI.Button(new Rect(128f, 170f, 150f, 35f), "PowerButton")) PhoneManager.Instance.TogglePower();

                if (GUI.Button(new Rect(128f, 250f, 150f, 35f), "ActionButton")) PhoneLockScreen.SetActionButton();

                if (GUI.Button(new Rect(128f, 290f, 150f, 35f), "MonkeGram"))
                {
                    if (!_MonkeGramOpen)
                    {
                        App("Open", "MonkeGram");
                        _MonkeGramOpen = true;
                    }
                    else
                    {
                        App("Close", "MonkeGram");
                        _MonkeGramOpen = false;
                    }
                }


                bool post = GUI.Button(new Rect(128f, 130f, 150f, 35f), "Post Sample");
                if (post)
                {
                    string _Folder = PhoneManager.Instance.PhotosPath;

                    var _Files = Directory.GetFiles(_Folder, "*.png");

                    if (_Files.Length == 0) return;

                    string _RandomFile = _Files[Random.Range(0, _Files.Length)];

                    byte[] bytes = File.ReadAllBytes(_RandomFile);

                    Photo photo = new()
                    {
                        Name = Path.GetFileName(_RandomFile),
                        Bytes = bytes,
                        Summary = $"{GorillaComputer.instance.currentName} posted a photo:"
                    };

                    PhoneManager.Instance.GetApp<GalleryApp>().RelativePhotos.Add(photo);
                    PhoneManager.Instance.GetApp<GalleryApp>().SendWebhook(photo.Summary, photo.Name, photo.Bytes);
                    PhoneManager.Instance.ApplyWallpaper(false, _RandomFile);
                }
            }
        } 

        private void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current.tabKey.wasPressedThisFrame)
            {
                _GUIShow = !_GUIShow;
            }
        }
    }
}
