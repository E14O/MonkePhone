using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
		private List<string> _musicList;
		private Dictionary<string, AudioClip> _musicComparison = new();
		private int _currentMusic;
		private Transform _viynlDisk;
		private PhoneSlider _timelineSlider;
		private Text _songTitle;
		private Text _songMissing;
		private Text _songTimePosition;
		public AudioSource MusicSource;
		private bool _isLoadingMusic;
		private bool _wasPlaying = true;
		private bool _inDownloadView = true;
		private int _currentPage;
		private List<StreamableMusicComponent> _streambleMusicComponents = new();

		public override string AppId => "Music";

		public override void Initialize()
		{
			Debug.Log("[MusicApp] Initialize called");

			MusicSource = GorillaLocomotion.GTPlayer.Instance.gameObject.AddComponent<AudioSource>();
			Debug.Log("[MusicApp] AudioSource created and added to player");

			_songTitle = transform.Find("CurrentlyPlayingContents/AudioTitle").GetComponent<Text>();
			_songTimePosition = transform.Find("CurrentlyPlayingContents/Timeline/Slider/Text (Legacy)").GetComponent<Text>();
			_timelineSlider = (PhoneSlider)GetObject("Timeline");
			_viynlDisk = transform.Find("CurrentlyPlayingContents/VinylRecord");
			_songMissing = transform.Find("CurrentlyPlayingContents/NoMusicWarning")?.GetComponent<Text>();

			SetVolumeMultiplier(Configuration.MusicMultiplier.Value);
			SetSpatialBlend(Configuration.UseSpatialBlend.Value);

			Debug.Log("[MusicApp] Finished setting up UI references and audio settings");

			EvaluateMusicList();
			_inDownloadView = _musicList.Count == 0;

			Debug.Log($"[MusicApp] Music list evaluated. Found {_musicList.Count} songs.");
		}

		public void Update()
		{
			if (MusicSource && MusicSource.clip != null)
			{
				
				if (MusicSource.isPlaying)
				{
					float progress = MusicSource.time / MusicSource.clip.length;
					var parameters = _timelineSlider.Parameters;
					parameters.z = progress * 100;
					_timelineSlider.Parameters = parameters;
					_timelineSlider.UpdatePosition();

					
					_songTimePosition.text = MusicSource.time.ToString("F2"); 
				}

				
				var songLengthText = transform.Find("CurrentlyPlayingContents/Timeline/Song Length")?.GetComponent<Text>();
				if (songLengthText != null)
				{
					songLengthText.text = MusicSource.clip.length.ToString("F2");
				}
			}
		}



		public override void AppOpened()
		{
			Debug.Log("[MusicApp] AppOpened called");
			EvaluateMusicList();
			RefreshSuitableContainer();
		}

		private void EvaluateMusicList()
		{
			Debug.Log("[MusicApp] Evaluating music list...");
			var current = _musicList;
			_musicList = Directory.GetFiles(PhoneManager.Instance.MusicPath)
				.Where(file => file.ToLower().EndsWith(".mp3") || file.ToLower().EndsWith(".ogg") || file.ToLower().EndsWith(".wav"))
				.ToList();

			var missingItems = current?.Where(str => !_musicList.Contains(str) && _musicComparison.ContainsKey(str)) ?? Enumerable.Empty<string>();
			foreach (var str in missingItems)
			{
				_musicComparison.Remove(str);
			}

			Debug.Log($"[MusicApp] Music list evaluation complete. Found {_musicList.Count} files.");
		}

		private void RefreshSuitableContainer()
		{
			Debug.Log($"[MusicApp] Refreshing container. _inDownloadView={_inDownloadView}");
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
			Debug.Log("[MusicApp] Refreshing downloadable songs");
			_currentPage = MathEx.Wrap(_currentPage, 0, Mathf.CeilToInt(PhoneManager.Instance.Data.songs.Length / 3f));
			Song[] songs = {
				PhoneManager.Instance.Data.songs.ElementAtOrDefault((_currentPage * 3) + 0),
				PhoneManager.Instance.Data.songs.ElementAtOrDefault((_currentPage * 3) + 1),
				PhoneManager.Instance.Data.songs.ElementAtOrDefault((_currentPage * 3) + 2)
			};

			Transform table = transform.Find("MusicDownloadContainer/Table");
			_streambleMusicComponents.Clear();

			for (int i = 0; i < table.childCount; i++)
			{
				Transform item = table.GetChild(i);
				Song song = songs.ElementAtOrDefault(i);
				bool noSong = song == null || song == default;
				item.gameObject.SetActive(!noSong);
				if (noSong) continue;

				if (item.TryGetComponent(out StreamableMusicComponent component))
				{
					Destroy(component);
				}

				component = item.AddComponent<StreamableMusicComponent>();
				component.song = song;
				component.coverArt = item.Find("AlbumCover").GetComponent<RawImage>();
				component.nameText = item.Find("AudioTitle").GetComponent<Text>();
				component.buttonText = item.FindChildRecursive("download").GetComponentInChildren<Text>();
				_streambleMusicComponents.Add(component);
			}
		}

		private void RefreshSongList()
		{
			Debug.Log("[MusicApp] Refreshing local song list UI");
			Transform table = transform.Find("MusicPlayerContainer/Table");
			Transform noSongs = transform.Find("MusicPlayerContainer/NoMusic");
			table.gameObject.SetActive(_musicList.Count > 0);
			noSongs.gameObject.SetActive(_musicList.Count == 0);
			if (_musicList.Count == 0) return;

			_currentPage = MathEx.Wrap(_currentPage, 0, Mathf.CeilToInt(_musicList.Count / 3f));
			string[] songs = {
				_musicList.ElementAtOrDefault((_currentPage * 3) + 0),
				_musicList.ElementAtOrDefault((_currentPage * 3) + 1),
				_musicList.ElementAtOrDefault((_currentPage * 3) + 2)
			};

			for (int i = 0; i < table.childCount; i++)
			{
				Transform item = table.GetChild(i);
				string song = songs.ElementAtOrDefault(i);
				bool noSong = song == null || song == default;
				item.gameObject.SetActive(!noSong);
				if (noSong) continue;

				async void HandleButton()
				{
					if (!_musicComparison.ContainsKey(song))
					{
						Debug.Log($"[MusicApp] Loading track for UI: {song}");
						await LoadTrack(song);
					}

					item.Find("AudioTitle").GetComponent<Text>().text = Path.GetFileNameWithoutExtension(song);
					item.Find("Format Label").GetComponent<Text>().text = Path.GetExtension(song).Replace(".", "").ToUpper();
					item.Find("Length Label").GetComponent<Text>().text = TimeSpan.FromSeconds(_musicComparison[song].length).ToString(@"mm\:ss");
				}
				HandleButton();
			}
		}

		public override async void ButtonClick(PhoneUIObject phoneUIObject, bool isLeftHand)
		{
			Debug.Log($"[MusicApp] Button clicked: {phoneUIObject.name}");
			if (phoneUIObject.name.StartsWith("download"))
			{
				int index = int.Parse(phoneUIObject.name[^1].ToString()) - 1;
				var component = _streambleMusicComponents.ElementAtOrDefault(index);
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
				//this is a horrid way to do this although it works so no touch
				case "play1":
					{
						int trackIndex = _currentPage * 3 + 0;
						if (!_isLoadingMusic && _musicList.Count > trackIndex)
						{
							if (_currentMusic != trackIndex)
							{
								MusicSource.clip = null;
								_currentMusic = trackIndex;
							}

							Debug.Log("[MusicApp] Music toggle clicked");

							if (MusicSource.clip == null || MusicSource.clip.name != Path.GetFileName(_musicList[_currentMusic]))
							{
								Debug.Log("[MusicApp] Setting new track...");
								_isLoadingMusic = true;
								StartCoroutine(SetTrack(_musicList[_currentMusic]));
								return;
							}

							if (MusicSource.isPlaying)
							{
								Debug.Log("[MusicApp] Pausing music");
								MusicSource.Pause();
							}
							else
							{
								Debug.Log("[MusicApp] Playing music");
								MusicSource.Play();
							}
						}
						break;
					}

				case "play2":
					{
						int trackIndex = _currentPage * 3 + 1;
						if (!_isLoadingMusic && _musicList.Count > trackIndex)
						{
							if (_currentMusic != trackIndex)
							{
								MusicSource.clip = null;
								_currentMusic = trackIndex;
							}

							Debug.Log("[MusicApp] Music toggle clicked");

							if (MusicSource.clip == null || MusicSource.clip.name != Path.GetFileName(_musicList[_currentMusic]))
							{
								Debug.Log("[MusicApp] Setting new track...");
								_isLoadingMusic = true;
								StartCoroutine(SetTrack(_musicList[_currentMusic]));
								return;
							}

							if (MusicSource.isPlaying)
							{
								Debug.Log("[MusicApp] Pausing music");
								MusicSource.Pause();
							}
							else
							{
								Debug.Log("[MusicApp] Playing music");
								MusicSource.Play();
							}
						}
						break;
					}

				case "play3":
					{
						int trackIndex = _currentPage * 3 + 2;
						if (!_isLoadingMusic && _musicList.Count > trackIndex)
						{
							if (_currentMusic != trackIndex)
							{
								MusicSource.clip = null;
								_currentMusic = trackIndex;
							}

							Debug.Log("[MusicApp] Music toggle clicked");

							if (MusicSource.clip == null || MusicSource.clip.name != Path.GetFileName(_musicList[_currentMusic]))
							{
								Debug.Log("[MusicApp] Setting new track...");
								_isLoadingMusic = true;
								StartCoroutine(SetTrack(_musicList[_currentMusic]));
								return;
							}

							if (MusicSource.isPlaying)
							{
								Debug.Log("[MusicApp] Pausing music");
								MusicSource.Pause();
							}
							else
							{
								Debug.Log("[MusicApp] Playing music");
								MusicSource.Play();
							}
						}
						break;
					}

				case "music toggle":
					{
						if (MusicSource.clip != null && !_isLoadingMusic)
						{
							if (MusicSource.isPlaying)
							{
								Debug.Log("[MusicApp] Pausing music");
								MusicSource.Pause();
							}
							else
							{
								Debug.Log("[MusicApp] Playing music");
								MusicSource.Play();
							}
						}
						break;
					}


			}
		}

		public override void ButtonTicked(PhoneUIObject phoneUIObject, bool currentValue, bool isLeftHand)
		{

			if (phoneUIObject.name == "music toggle")
			{
				if (MusicSource != null && MusicSource.clip != null)
				{
					if (MusicSource.isPlaying)
						MusicSource.Pause();
					else
						MusicSource.Play();
				}
			}
		}

		public void RefreshApp()
		{
			try
			{
				Debug.Log("[MusicApp] Refreshing app view");
				if (_songMissing != null)
				{
					_songMissing.gameObject.SetActive(_musicList.Count == 0);
				}

				if (_musicList.Count == 0)
				{
					_songTitle.text = "";
					_timelineSlider.gameObject.SetActive(false);
					return;
				}

				_currentMusic = MathEx.Wrap(_currentMusic, 0, _musicList.Count);
				_songTitle.text = Path.GetFileNameWithoutExtension(_musicList[_currentMusic]);
				_timelineSlider.gameObject.SetActive(true);

				if (_timelineSlider.Value != 0 && MusicSource.clip != null && MusicSource.clip.name != Path.GetFileNameWithoutExtension(_musicList[_currentMusic]))
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

		public async Task LoadTrack(string path)
		{
			if (_musicComparison.ContainsKey(path)) return;

			Debug.Log($"[MusicApp] Loading track: {path}");

			AudioType audioType = AudioType.UNKNOWN;
			if (path.EndsWith(".mp3")) audioType = AudioType.MPEG;
			else if (path.EndsWith(".ogg")) audioType = AudioType.OGGVORBIS;
			else if (path.EndsWith(".wav")) audioType = AudioType.WAV;

			var downloadHandler = new DownloadHandlerAudioClip(path, audioType);
			var webRequest = new UnityWebRequest(path, "GET", downloadHandler, null);
			await YieldUtils.Yield(webRequest);

			if (webRequest.result == UnityWebRequest.Result.Success)
			{
				_musicComparison.Add(path, downloadHandler.audioClip);
				Debug.Log("[MusicApp] Track loaded successfully");
				SetTrack(path);
			}
			else
			{
				Logging.Error($"[MusicApp] Failed to load track: {webRequest.error}");
			}
		}

		public IEnumerator SetTrack(string path)
		{
			Debug.Log($"[MusicApp] Setting track: {path}");

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
				Debug.Log("[MusicApp] SetTrack successful, starting playback");
				downloadHandler.audioClip.name = Path.GetFileName(path);
				MusicSource.clip = downloadHandler.audioClip;

				if (_wasPlaying)
				{
					MusicSource.volume = 0.5f;
					MusicSource.Play();
					MusicSource.time = 0f;
				}
			}
			else
			{
				Logging.Error($"[MusicApp] Error setting track: {webRequest.error}");
			}
		}

		public void SetVolumeMultiplier(float multiplier)
		{
			Debug.Log($"[MusicApp] Setting volume multiplier to {multiplier}");
			MusicSource.volume = 0.1f * multiplier;
		}

		public void SetSpatialBlend(bool useSpatialBlend)
		{
			Debug.Log($"[MusicApp] Setting spatial blend: {useSpatialBlend}");
			MusicSource.spatialBlend = (useSpatialBlend ? 1f : 0f);
		}

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