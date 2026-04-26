using MonkePhone.Behaviours.Apps;
using UnityEngine;

namespace MonkePhone.Watch
{
    public class WatchOS : MonoBehaviour
    {
        #region MusicFunction
        public static string GetCurrentlyPlayingMusic(string _info)
        {
            // we can display the currently playing song on the watch and how long the song duration is.
            var _currentSong = MusicApp._MusicSource.clip;
            switch (_info)
            {
                case "SongName":
                    if (_currentSong != null)
                        return _currentSong.name;
                    else
                        return null;

                case "SongDuration":
                    if (_currentSong != null)
                    {
                        float _length = _currentSong.length;
                        int _mins = Mathf.FloorToInt(_length / 60);
                        int _seconds = Mathf.FloorToInt(_length % 60);
                        return $"{_mins:00}:{_seconds:00}";
                    }
                    else
                        return null;
            }
            return null;
        }
        #endregion

        #region NotificaionHandler
        public static void CheckNotifications()
        {
            // if it returns a notificaion then the watch can vibrate and notify the user of a new notificaion.
        }
        #endregion

        #region OtherFunctions
        #endregion
    }
}
