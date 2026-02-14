using System;
using System.Collections.Generic;
using System.Threading;
using GorillaLocomotion;
using MonkePhone.Behaviours.UI;
using MonkePhone.Extensions;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

namespace MonkePhone.Behaviours.Apps
{
    public class ScoreboardApp : PhoneApp
    {
        public override string AppId => "Scoreboard";

        private readonly List<Text> _playerNameTexts = [];
        private readonly List<Image> _playerSwatches = [];
        private readonly List<GameObject> _playerLines = [];
        private readonly List<GameObject> _playerMics = [];
        private readonly List<GameObject> _phoneOwner = [];
        private readonly List<NetPlayer> _playerRefs = [];
        private readonly List<GameObject> _playerMute = [];

        private readonly Dictionary<GameObject, int> _muteButtonIndex = new();


        private Text _roomIdText;
        private Text _roomNotice;

        private GameObject _templateLine;

        private const int MaxPlayers = 10;

        private float _micUpdateTimer = 0f;
        private const float MicUpdateInterval = 0.1f;

        public override void Initialize()
        {
            _roomIdText = transform.Find("Roominfo").GetComponent<Text>();
            _roomNotice = transform.Find("Notice").GetComponent<Text>();
            _templateLine = transform.Find("Grid/Person (1)").gameObject;

            _templateLine.SetActive(false);

            _playerNameTexts.Clear();
            _playerSwatches.Clear();
            _playerLines.Clear();
            _playerMics.Clear();
            _playerRefs.Clear();

            for (int i = 0; i < MaxPlayers; i++)
            {
                GameObject line = i == 0 ? _templateLine : Instantiate(_templateLine, transform.Find("Grid"));

                line.name = $"Person ({i + 1})";
                line.SetActive(false);

                GameObject _grid = line.transform.Find("Grid").gameObject;
                _grid.SetActive(true);
                foreach (Transform _child in _grid.transform)
                {
                    _child.gameObject.SetActive(false);
                }

                _playerLines.Add(line);
                _playerNameTexts.Add(line.transform.Find("Name").GetComponent<Text>());
                _playerSwatches.Add(line.transform.Find("Swatch").GetComponent<Image>());

                GameObject _muteButton = line.transform.Find("Mute").gameObject;
                _playerMute.Add(_muteButton);
                _muteButtonIndex[_muteButton] = i;

                _phoneOwner.Add(line.transform.Find("Grid/Phone").gameObject);

                _playerMics.Add(line.transform.Find("Mic").gameObject);

                _playerRefs.Add(null);
            }
        }

        public override void AppOpened()
        {
            base.AppOpened();

            RoomSystem.JoinedRoomEvent += JoinedRoomEvent;
            RoomSystem.LeftRoomEvent += LeftRoomEvent;
            RoomSystem.PlayerJoinedEvent += PlayerJoinedEvent;
            RoomSystem.PlayerLeftEvent += PlayerLeftEvent;

            RefreshApp();
        }

        public override void AppClosed()
        {
            base.AppClosed();

            RoomSystem.JoinedRoomEvent -= JoinedRoomEvent;
            RoomSystem.LeftRoomEvent -= LeftRoomEvent;
            RoomSystem.PlayerJoinedEvent -= PlayerJoinedEvent;
            RoomSystem.PlayerLeftEvent -= PlayerLeftEvent;
        }

        private void Update()
        {
            if (!PhotonNetwork.InRoom) return;

            _micUpdateTimer += Time.deltaTime;

            if (_micUpdateTimer < MicUpdateInterval) return;

            _micUpdateTimer = 0f;

            for (int i = 0; i < _playerLines.Count; i++)
            {
                NetPlayer player = _playerRefs[i];

                if (player == null || player.IsNull) continue;

                if (GorillaParent.instance.vrrigDict.TryGetValue(player, out VRRig rig))
                {
                    SetSwatchColour(_playerSwatches[i], rig);
                }
            }

            for (int i = 0; i < _playerLines.Count; i++)
            {
                NetPlayer player = _playerRefs[i];
                if (player == null || player.IsNull)
                {
                    _playerMics[i].SetActive(false);
                    continue;
                }

                _playerMics[i].SetActive(player.IsTalking());
            }
        }

        private void RefreshApp()
        {
            _roomIdText.text = PhotonNetwork.CurrentRoom.GetCurrentRoomInfo();

            if (!PhotonNetwork.InRoom || _playerLines.Count == 0)
            {
                for (int i = 0; i < _playerLines.Count; i++)
                {
                    _playerLines[i].SetActive(false);
                    _playerRefs[i] = null;
                }

                _roomNotice.gameObject.SetActive(true);

                return;
            }

            NetPlayer[] players = NetworkSystem.Instance.AllNetPlayers;
            Array.Sort(players, (a, b) => a.ActorNumber.CompareTo(b.ActorNumber));

            for (int i = 0; i < _playerLines.Count; i++)
            {
                if (i >= players.Length)
                {
                    _playerLines[i].SetActive(false);
                    _playerRefs[i] = null;
                    continue;
                }

                NetPlayer player = players[i];
                if (player == null || player.IsNull || (!player.IsLocal && !player.InRoom))
                {
                    _playerLines[i].SetActive(false);
                    _playerRefs[i] = null;
                    continue;
                }

                if (player.IsLocal)
                {
                    _playerMute[i].SetActive(false);
                }
                else
                {
                    _playerMute[i].SetActive(true);
                }

                _playerLines[i].SetActive(true);
                _playerRefs[i] = player;

                _roomNotice.gameObject.SetActive(false);

                GorillaParent.instance.vrrigDict.TryGetValue(player, out VRRig rig);
                Photon.Realtime.Player _player = PhotonNetwork.CurrentRoom.GetPlayer(player.ActorNumber);

                GetOwner(_phoneOwner[i], _player, "Phone");

                _playerNameTexts[i].text = player.GetName();
                _playerNameTexts[i].color = rig != null ? rig.playerText1.color : UnityEngine.Color.white;

                SetSwatchColour(_playerSwatches[i], rig);
            }
        }
        public override void ButtonClick(PhoneUIObject phoneUIObject, bool isLeftHand)
        {
            base.ButtonClick(phoneUIObject, isLeftHand);

            if (phoneUIObject.name != "Mute") return;

            if (!_muteButtonIndex.TryGetValue(phoneUIObject.gameObject, out int i))
                return;

            NetPlayer _netPlayer = _playerRefs[i];
            GorillaParent.instance.vrrigDict.TryGetValue(_netPlayer, out VRRig rig);

            ToggleMute(rig, phoneUIObject);
            RefreshApp();
        }
        public void ToggleMute(VRRig _player, PhoneUIObject _button)
        {
            foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines)
            {
                if (line.playerVRRig == _player)
                {
                    line.PressButton(_player, GorillaPlayerLineButton.ButtonType.Mute);
                    break;
                }
            }
        }
        private void GetOwner(GameObject _phoneIcon, Photon.Realtime.Player _player, string _property)
        {
            bool _hasProperty = false;

            switch (_property)
            {
                case "Phone":
                    _hasProperty = _player.CustomProperties.ContainsKey(Constants.CustomProperty);
                    break;

                case "Watch":
                    _hasProperty = _player.CustomProperties.ContainsKey(Constants.CustomProperty);
                    break;
            }

            _phoneIcon.SetActive(_hasProperty);
        }

        private void SetSwatchColour(Image swatch, VRRig rig)
        {
            if (swatch == null || rig == null)
            {
                if (swatch != null)
                {
                    swatch.material = null;
                    swatch.color = Color.white;
                }
                return;
            }

            int index = rig.setMatIndex;
            Material material = (index > 0 && rig.materialsToChangeTo != null && index < rig.materialsToChangeTo.Length)
                ? rig.materialsToChangeTo[index]
                : rig.scoreboardMaterial;

            Color color = (index == 0) ? rig.playerColor : (material != null ? material.color : rig.playerColor);

            if (color.a < 0.1f) color.a = 1f;

            swatch.material = material;
            swatch.color = color;
        }

        private void JoinedRoomEvent() => RefreshApp();

        private void LeftRoomEvent() => RefreshApp();

        private void PlayerJoinedEvent(NetPlayer player) => RefreshApp();

        private void PlayerLeftEvent(NetPlayer player) => RefreshApp();
    }
}