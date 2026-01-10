using System.IO;
using GorillaNetworking;
using MonkePhone.Behaviours.Apps;
using MonkePhone.Models;
using UnityEngine;

namespace MonkePhone.Behaviours
{
    public class PhoneStandBy : MonoBehaviour
    {
        /// <summary>
        /// This is for testing the post system on computer. (It does not send the photo to a server it only works with the webhook)
        /// </summary>
        public void OnGUI()
        {
            if (GUI.Button(new Rect(128f, 210f, 150f, 35f), "HomeButton")) PhoneManager.Instance.SetHome();

            if (GUI.Button(new Rect(128f, 170f, 150f, 35f), "PowerButton")) PhoneManager.Instance.TogglePower();

            bool open = GUI.Button(new Rect(128f, 250f, 150f, 35f), "GorillaGram");
            if (!open) PhoneManager.Instance.OpenApp("GorillaGram");
            else PhoneManager.Instance.CloseApp("GorillaGram");

            bool post = GUI.Button(new Rect(128f, 130f, 150f, 35f), "Post Sample");
            if (post)
            {
                string _Folder = PhoneManager.Instance.PhotosPath;

                var _Files = Directory.GetFiles(_Folder, "*.png");

                if (_Files.Length == 0) return;

                string _RandomFile = _Files[UnityEngine.Random.Range(0, _Files.Length)];

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
        }

        public void ImtootiredtoeventhinkicantevenstartonhowtodothismoostasssimplecodebroplslikebrotherlookatthisScriptNameitscalledphonestandbylikeWhoevencallsaCSfilethat()
        {

        }

        private void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current.tabKey.wasPressedThisFrame))
            {

            }
            else
            {

            }
        }
    }
}
