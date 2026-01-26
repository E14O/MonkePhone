using System.IO;
using GorillaNetworking;
using MonkePhone.Behaviours.Apps;
using MonkePhone.Models;
using UnityEngine;

namespace MonkePhone.Behaviours
{
    public class DebugManager : MonoBehaviour
    {
        private bool _MonkeGramOpen = false;
        private bool _ScoreBoardOpen = false;
        private bool _GUIShow = false;
        private bool _Phone = true;
        private string _RandomFile;

        /// <summary>
        /// This is for testing the post system and other systems on computer. (It does not send the photo to a server it only works if a webhook is set)
        /// </summary>

        public void OnGUI()
        {
            if (_GUIShow)
            {
                float xAxis = 20f;
                float yAxis = 100f;

                float width = 150f;
                float height = 35f;
                float spacing = 10f;

                if (GUI.Button(new Rect(xAxis, yAxis + (height + spacing) * 0, width, height), "Phone"))
                {
                    if (!_Phone)
                    {
                        Phone(PhoneManager.Instance.Phone.transform, Constants.Debug);
                        _Phone = true;
                    }
                    else
                    {
                        Phone(PhoneManager.Instance.Phone.transform, Constants.Debug);
                        _Phone = false;
                    }
                }

                if (GUI.Button(new Rect(xAxis, yAxis + (height + spacing) * 1, width, height), "SetWallpaper"))
                {
                    GetRandomImage();
                    PhoneManager.Instance.ApplyWallpaper(false, _RandomFile);
                }

                if (GUI.Button(new Rect(xAxis, yAxis + (height + spacing) * 2, width, height), "Post Sample"))
                {
                    GetRandomImage();
                    byte[] bytes = File.ReadAllBytes(_RandomFile);

                    Photo photo = new()
                    {
                        Name = Path.GetFileName(_RandomFile),
                        Bytes = bytes,
                        Summary = $"{GorillaComputer.instance.currentName} posted a photo:"
                    };

                    PhoneManager.Instance.GetApp<GalleryApp>().RelativePhotos.Add(photo);
                    PhoneManager.Instance.GetApp<GalleryApp>().SendWebhook(photo.Summary, photo.Name, photo.Bytes);
                }

                if (GUI.Button(new Rect(xAxis, yAxis + (height + spacing) * 3, width, height), "PowerButton")) PhoneManager.Instance.TogglePower();

                if (GUI.Button(new Rect(xAxis, yAxis + (height + spacing) * 4, width, height), "HomeButton")) PhoneManager.Instance.SetHome();

                if (GUI.Button(new Rect(xAxis, yAxis + (height + spacing) * 5, width, height), "ActionButton")) PhoneLockScreen.SetActionButton();

                if (GUI.Button(new Rect(xAxis, yAxis + (height + spacing) * 6, width, height), "MonkeGram"))
                {
                    if (!_MonkeGramOpen)
                    {
                        PhoneManager.Instance.OpenApp(PhoneManager.Instance.GetApp<MonkeGramApp>().AppId);
                        _MonkeGramOpen = true;
                    }
                    else
                    {
                        PhoneManager.Instance.CloseApp(PhoneManager.Instance.GetApp<MonkeGramApp>().AppId);
                        _MonkeGramOpen = false;
                    }
                }

                if (GUI.Button(new Rect(xAxis, yAxis + (height + spacing) * 7, width, height), "ScoreBoard"))
                {
                    if (!_ScoreBoardOpen)
                    {
                        PhoneManager.Instance.OpenApp(PhoneManager.Instance.GetApp<ScoreboardApp>().AppId);
                        _ScoreBoardOpen = true;
                    }
                    else
                    {
                        PhoneManager.Instance.CloseApp(PhoneManager.Instance.GetApp<ScoreboardApp>().AppId);
                        _ScoreBoardOpen = false;
                    }
                }

                if (GUI.Button(new Rect(xAxis, yAxis + (height + spacing) * 8, width, height), "Messaging"))
                {
                    if (!_MonkeGramOpen)
                    {
                        PhoneManager.Instance.OpenApp(PhoneManager.Instance.GetApp<MessagingApp>().AppId);
                        _MonkeGramOpen = true;
                    }
                    else
                    {
                        PhoneManager.Instance.CloseApp(PhoneManager.Instance.GetApp<MessagingApp>().AppId);
                        _MonkeGramOpen = false;
                    }
                }
            }
        }

        private void GetRandomImage()
        {
            string _Folder = PhoneManager.Instance.PhotosPath;

            var _Files = Directory.GetFiles(_Folder, "*.png");

            if (_Files.Length == 0) return;

            _RandomFile = _Files[Random.Range(0, _Files.Length)];
        }
        private void Phone(Transform phone, ObjectPosition position)
        {
            //TODO: Make gui toggle to move phone towards main camera
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
