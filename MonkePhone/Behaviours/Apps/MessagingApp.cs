using System;
using System.Collections.Generic;
using MonkePhone.Extensions;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;

namespace MonkePhone.Behaviours.Apps
{
    public class MessagingApp : PhoneApp
    {
        public override string AppId => "Messaging";

        private readonly List<GameObject> _messageablePlayers = new();
        private readonly List<Text> _playerName = new();
        private readonly List<Text> _playerStatus = new();

        public GameObject _chatBox;
        private GameObject _templateLine;
        private GameObject _mainGrid;

        public override void Initialize()
        {
            _mainGrid = transform.Find("Chat Messages").gameObject;
            _templateLine = _mainGrid.transform.Find("Message (1)").gameObject;
            _chatBox = transform.Find("Chat Box").gameObject;

            foreach (Transform _child in _templateLine.transform.parent)
            {
                if (_child.name != _templateLine.name)
                {
                    Destroy(_child.gameObject);
                }
            }

            _templateLine.SetActive(false);
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
            RoomSystem.JoinedRoomEvent -= JoinedRoomEvent;
            RoomSystem.LeftRoomEvent -= LeftRoomEvent;
            RoomSystem.PlayerJoinedEvent -= PlayerJoinedEvent;
            RoomSystem.PlayerLeftEvent -= PlayerLeftEvent;
        }

        void RefreshApp()
        {
            for (int i = 0; i < _messageablePlayers.Count; i++)
            {
                Destroy(_messageablePlayers[i]);
            }

            _messageablePlayers.Clear();
            _playerName.Clear();
            _playerStatus.Clear();

            if (!PhotonNetwork.InRoom)
                return;

            NetPlayer[] _players = NetworkSystem.Instance.AllNetPlayers;
            Array.Sort(_players, (a, b) => a.ActorNumber.CompareTo(b.ActorNumber));

            foreach (NetPlayer _player in _players)
            {
                GorillaParent.instance.vrrigDict.TryGetValue(_player, out VRRig rig);

                if (_player == null || _player.IsNull || !_player.InRoom)
                    continue;

                if (_player.IsLocal)
                    return;

                Player _photonPlayer = PhotonNetwork.CurrentRoom.GetPlayer(_player.ActorNumber);

                if (_photonPlayer.CustomProperties.ContainsKey(Constants.CustomProperty))
                    return;

                GameObject _line = Instantiate(_templateLine, _mainGrid.transform);
                _line.SetActive(true);
                _line.name = $"MessageablePlayer ({_player.ActorNumber})";

                Text _nameText = _line.transform.Find("PlayerName").GetComponent<Text>();
                Text _statusText = _line.transform.Find("Contents").GetComponent<Text>();

                _nameText.text = _player.GetName();
                _statusText.text = "No Current Messages...";

                _messageablePlayers.Add(_line);
                _playerName.Add(_nameText);
                _playerStatus.Add(_statusText);
            }
        }


        private void JoinedRoomEvent() => RefreshApp();

        private void LeftRoomEvent() => RefreshApp();

        private void PlayerJoinedEvent(NetPlayer player) => RefreshApp();

        private void PlayerLeftEvent(NetPlayer player) => RefreshApp();
    }
}
