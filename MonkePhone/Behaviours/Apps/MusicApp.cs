using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonkePhone.Behaviours.UI;
using MonkePhone.Extensions;
using MonkePhone.Models;
using MonkePhone.Tools;
using MonkePhone.Utilities;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


namespace MonkePhone.Behaviours.Apps
{
    public class MusicApp : PhoneApp
    {
        public override string AppId => "Music";

        private List<string> _musicList;
        private Dictionary<string, AudioClip> _musicComparison = new();
        private Dictionary<string, float> _songLengths = new();

        private int _currentMusic, _currentPage;
        private bool _isLoadingMusic, _wasPlaying = true, _inDownloadView = true;

        private Transform _vinylDisk;
        private PhoneSlider _timelineSlider;

        private Text _songTitle, _songMissing, _songTimePosition, _songLengthText;
        public static AudioSource _MusicSource;

        private List<StreamableMusicComponent> _streamableMusicComponents = new();

        public override void Initialize()
        {
            _MusicSource = GorillaLocomotion.GTPlayer.Instance.gameObject.AddComponent<AudioSource>();

            _songTitle = transform.Find("CurrentlyPlayingContents/AudioTitle").GetComponent<Text>();
            _songTimePosition = transform.Find("CurrentlyPlayingContents/Timeline/Slider/Text (Legacy)").GetComponent<Text>();
            _timelineSlider = (PhoneSlider)GetObject("Timeline");
            _vinylDisk = transform.Find("CurrentlyPlayingContents/VinylRecord");
            _songMissing = transform.Find("CurrentlyPlayingContents/NoMusicWarning")?.GetComponent<Text>();
            _songLengthText = transform.Find("CurrentlyPlayingContents/Timeline/Song Length")?.GetComponent<Text>();

            SetVolumeMultiplier(Configuration.MusicMultiplier.Value);
            SetSpatialBlend(Configuration.UseSpatialBlend.Value);

            EvaluateMusicList();
            _inDownloadView = _musicList.Count == 0;
        }

        public void Update()
        {
            if (_MusicSource && _MusicSource.clip != null)
            {
                float _progress = _MusicSource.time / _MusicSource.clip.length;
                _timelineSlider.Value = _progress;
                _timelineSlider.UpdatePosition();

                _songTimePosition.text = TimeSpan.FromSeconds(_MusicSource.time).ToString(@"mm\:ss");

                if (_songLengthText != null)
                    _songLengthText.text = TimeSpan.FromSeconds(_MusicSource.clip.length).ToString(@"mm\:ss");
            }
        }


        public override void AppOpened()
        {
            EvaluateMusicList();
            RefreshSuitableContainer();
        }

        private void EvaluateMusicList()
        {
            var current = _musicList;
            _musicList = Directory.GetFiles(PhoneManager.Instance.MusicPath)
                .Where(file => file.ToLower().EndsWith(".mp3") || file.ToLower().EndsWith(".ogg") || file.ToLower().EndsWith(".wav"))
                .ToList();

            var missingItems = current?.Where(str => !_musicList.Contains(str) && _musicComparison.ContainsKey(str)) ?? Enumerable.Empty<string>();
            foreach (var str in missingItems)
                _musicComparison.Remove(str);
        }

        private void RefreshSuitableContainer()
        {
            Logging.Log($"[MusicApp] Refreshing container. _inDownloadView={_inDownloadView}");
            transform.Find("MusicDownloadContainer").gameObject.SetActive(_inDownloadView);
            transform.Find("MusicPlayerContainer").gameObject.SetActive(!_inDownloadView);
            if (_inDownloadView)
            {
                RefreshDownloadables();
            }
            else
            {
                RefreshSongList();
                RefreshApp();
            }
        }

        private void RefreshDownloadables()
        {
            _currentPage = MathEx.Wrap(_currentPage, 0, Mathf.CeilToInt(PhoneManager.Instance.Data.songs.Length / 3f));
            Song[] songs = {
                PhoneManager.Instance.Data.songs.ElementAtOrDefault((_currentPage * 3) + 0),
                PhoneManager.Instance.Data.songs.ElementAtOrDefault((_currentPage * 3) + 1),
                PhoneManager.Instance.Data.songs.ElementAtOrDefault((_currentPage * 3) + 2)
            };

            Transform table = transform.Find("MusicDownloadContainer/Table");
            _streamableMusicComponents.Clear();

            for (int i = 0; i < table.childCount; i++)
            {
                Transform item = table.GetChild(i);
                Song song = songs.ElementAtOrDefault(i);

                bool noSong = song == null || song == default;
                item.gameObject.SetActive(!noSong);
                if (noSong) continue;

                if (item.TryGetComponent(out StreamableMusicComponent component))
                    Destroy(component);

                component = item.AddComponent<StreamableMusicComponent>();
                component.song = song;
                component.coverArt = item.Find("AlbumCover").GetComponent<RawImage>();
                component.nameText = item.Find("AudioTitle").GetComponent<Text>();
                component.buttonText = item.FindChildRecursive("download").GetComponentInChildren<Text>();
                _streamableMusicComponents.Add(component);
            }
        }

        private void RefreshSongList()
        {
            Transform _table = transform.Find("MusicPlayerContainer/Table");
            Transform _noSongs = transform.Find("MusicPlayerContainer/NoMusic");

            bool _hasSongs = _musicList.Count > 0;
            _table.gameObject.SetActive(_hasSongs);
            _noSongs.gameObject.SetActive(!_hasSongs);
            if (!_hasSongs) return;

            _currentPage = MathEx.Wrap(_currentPage, 0, Mathf.CeilToInt(_musicList.Count / 3f));
            string[] _songs = {
                _musicList.ElementAtOrDefault((_currentPage * 3) + 0),
                _musicList.ElementAtOrDefault((_currentPage * 3) + 1),
                _musicList.ElementAtOrDefault((_currentPage * 3) + 2)
            };

            for (int i = 0; i < _table.childCount; i++)
            {
                Transform _item = _table.GetChild(i);
                string _song = _songs.ElementAtOrDefault(i);

                bool _noSong = string.IsNullOrEmpty(_song);
                _item.gameObject.SetActive(!_noSong);
                if (_noSong) continue;

                _item.Find("AudioTitle").GetComponent<Text>().text = Path.GetFileNameWithoutExtension(_song);
                _item.Find("Format Label").GetComponent<Text>().text = Path.GetExtension(_song).Replace(".", "").ToUpper();

                Text _lengthText = _item.Find("Length Label").GetComponent<Text>(); _lengthText.text = "--:--";

                StartCoroutine(GetSongLength(_song, _lengthText));
            }
        }

        private IEnumerator GetSongLength(string _path, Text _text)
        {
            if (_songLengths.TryGetValue(_path, out float _time))
            {
                _text.text = TimeSpan.FromSeconds(_time).ToString(@"mm\:ss");
                yield break;
            }

            AudioType _audioType = AudioType.UNKNOWN;

            if (_path.EndsWith(".mp3")) _audioType = AudioType.MPEG;
            else if (_path.EndsWith(".ogg")) _audioType = AudioType.OGGVORBIS;
            else if (_path.EndsWith(".wav")) _audioType = AudioType.WAV;

            using (UnityWebRequest _unityWebRequest = UnityWebRequestMultimedia.GetAudioClip(_path, _audioType))
            {
                yield return _unityWebRequest.SendWebRequest();

                if (_unityWebRequest.result == UnityWebRequest.Result.Success)
                {
                    var _clip = DownloadHandlerAudioClip.GetContent(_unityWebRequest);
                    float _length = _clip.length;

                    _songLengths[_path] = _length;
                    _text.text = TimeSpan.FromSeconds(_length).ToString(@"mm\:ss");
                    Destroy(_clip);
                }
                else
                    _text.text = "--:--";
            }
        }



        public override async void ButtonClick(PhoneUIObject phoneUIObject, bool isLeftHand)
        {
            if (phoneUIObject.name.StartsWith("download"))
            {
                int index = int.Parse(phoneUIObject.name[^1].ToString()) - 1;
                var component = _streamableMusicComponents.ElementAtOrDefault(index);
                if (!component) return;

                component.UpdateText(component.song.currentState != Song.DownloadState.Downloaded ? Song.DownloadState.Awaiting : Song.DownloadState.Downloaded);
                var state = await component.song.Download();
                if (state == Song.DownloadState.None) return;

                EvaluateMusicList();
                component.UpdateText(state);
                PlaySound(state == Song.DownloadState.Downloaded ? "RequestSuccess" : "RequestDenied");
                return;
            }

            switch (phoneUIObject.name)
            {
                case "tab library":
                    _inDownloadView = false;
                    _currentPage = 0;
                    RefreshSuitableContainer();
                    break;
                case "tag catalog":
                    _inDownloadView = true;
                    _currentPage = 0;
                    RefreshSuitableContainer();
                    break;
                case "pageleft":
                    _currentPage--;
                    RefreshSuitableContainer();
                    break;
                case "pageright":
                    _currentPage++;
                    RefreshSuitableContainer();
                    break;
                case "Music Last":
                    _currentMusic--;
                    RefreshApp();
                    break;
                case "Music Next":
                    _currentMusic++;
                    RefreshApp();
                    break;
                case "play1":
                    HandlePlayButton(_currentPage * 3 + 0);
                    break;
                case "play2":
                    HandlePlayButton(_currentPage * 3 + 1);
                    break;
                case "play3":
                    HandlePlayButton(_currentPage * 3 + 2);
                    break;
                case "music toggle":
                    if (_MusicSource.clip != null && !_isLoadingMusic)
                    {
                        if (_MusicSource.isPlaying)
                            _MusicSource.Pause();
                        else
                            _MusicSource.Play();
                    }
                    break;
            }
        }

        private void HandlePlayButton(int trackIndex)
        {
            if (_isLoadingMusic || _musicList.Count <= trackIndex) return;

            if (_currentMusic != trackIndex)
            {
                _MusicSource.clip = null;
                _currentMusic = trackIndex;
                _songTitle.text = Path.GetFileNameWithoutExtension(_musicList[_currentMusic]);
            }

            if (_MusicSource.clip == null || _MusicSource.clip.name != Path.GetFileName(_musicList[_currentMusic]))
            {
                _isLoadingMusic = true;
                StartCoroutine(SetTrack(_musicList[_currentMusic]));
                return;
            }

            if (_MusicSource.isPlaying)
                _MusicSource.Pause();
            else
                _MusicSource.Play();
        }

        public override void ButtonTicked(PhoneUIObject phoneUIObject, bool currentValue, bool isLeftHand)
        {
            if (phoneUIObject.name == "music toggle")
            {
                if (_MusicSource != null && _MusicSource.clip != null)
                {
                    if (_MusicSource.isPlaying)
                        _MusicSource.Pause();
                    else
                        _MusicSource.Play();
                }
            }
        }

        public void RefreshApp()
        {
            try
            {
                if (_songMissing != null)
                    _songMissing.gameObject.SetActive(_musicList.Count == 0);

                if (_musicList.Count == 0)
                {
                    _songTitle.text = "";
                    _timelineSlider.gameObject.SetActive(false);
                    return;
                }

                _currentMusic = MathEx.Wrap(_currentMusic, 0, _musicList.Count);
                _songTitle.text = Path.GetFileNameWithoutExtension(_musicList[_currentMusic]);
                _timelineSlider.gameObject.SetActive(true);

                if (_timelineSlider.Value != 0 && _MusicSource.clip != null && _MusicSource.clip.name != Path.GetFileNameWithoutExtension(_musicList[_currentMusic]))
                {
                    _timelineSlider.Value = 0;
                    _timelineSlider.UpdatePosition();
                    _songTimePosition.text = "00:00";
                }
            }
            catch (Exception ex)
            {
                Logging.Error($"[MusicApp] Error when refreshing app: {ex}");
            }
        }

        public IEnumerator SetTrack(string path)
        {
            Logging.Log($"[MusicApp] Setting track: {path}");

            AudioType audioType = AudioType.UNKNOWN;
            if (path.EndsWith(".mp3")) audioType = AudioType.MPEG;
            else if (path.EndsWith(".ogg")) audioType = AudioType.OGGVORBIS;
            else if (path.EndsWith(".wav")) audioType = AudioType.WAV;

            DownloadHandlerAudioClip downloadHandler = new DownloadHandlerAudioClip(path, audioType);
            UnityWebRequest webRequest = new UnityWebRequest(path, "GET", downloadHandler, null);

            yield return webRequest.SendWebRequest();
            _isLoadingMusic = false;

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Logging.Log("[MusicApp] SetTrack successful, starting playback");
                downloadHandler.audioClip.name = Path.GetFileName(path);
                _MusicSource.clip = downloadHandler.audioClip;

                if (_wasPlaying)
                {
                    _MusicSource.volume = 0.1f * Configuration.MusicMultiplier.Value;
                    _MusicSource.Play();
                    _MusicSource.time = 0f;
                }
            }
            else
                Logging.Error($"[MusicApp] Error setting track: {webRequest.error}");
        }

        public void SetVolumeMultiplier(float multiplier) => _MusicSource.volume = 0.1f * multiplier;

        public void SetSpatialBlend(bool useSpatialBlend) => _MusicSource.spatialBlend = (useSpatialBlend ? 1f : 0f);

        public class StreamableMusicComponent : MonoBehaviour
        {
            public Song song;
            public Text nameText, buttonText;
            public RawImage coverArt;

            public async void Start()
            {
                nameText.text = song.title;
                coverArt.texture = null;

                if (!song.IsDownloaded && song.currentState == Song.DownloadState.Downloaded)
                {
                    song.currentState = Song.DownloadState.None;
                }

                UpdateText(song.currentState);

                if (string.IsNullOrWhiteSpace(song.coverUrl)) return;

                UnityWebRequest request = UnityWebRequestTexture.GetTexture(song.coverUrl);
                await YieldUtils.Yield(request);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Texture tex = ((DownloadHandlerTexture)request.downloadHandler).texture;
                    coverArt.texture = tex;
                }
            }

            public void UpdateText(Song.DownloadState dlState)
            {
                switch (dlState)
                {
                    case Song.DownloadState.None:
                        buttonText.text = "Download";
                        break;
                    case Song.DownloadState.Awaiting:
                        buttonText.text = "<color=grey>Progressing..</color>";
                        break;
                    case Song.DownloadState.Downloaded:
                        buttonText.text = $"<color=green>Saved {song.title}!</color>";
                        break;
                    case Song.DownloadState.Failed:
                        buttonText.text = "<color=red>Music could not be saved.</color>";
                        break;
                }
            }

            public void Download() { }
        }
    }
}