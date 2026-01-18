using System;
using System.Collections.Generic;
using MonkePhone.Extensions;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace MonkePhone.Behaviours.Apps
{
    public class MessagingApp : PhoneApp
    {
        public override string AppId => "Messaging";

        /* Plan: As defualt you can message other monke phone users in the same code (kinda useless) the overall plan is going to try and make cross server
                 messanging. This would need alot of work to make sure everyones messages are secure and the server not to be attacked. */

        // check if player has monkephone
        // create a line "PLAYERNAME" "No Current Messages..."

        /*private readonly List<GameObject> _messageablePlayers;
        private readonly List<Text> _playerName;
        private readonly List<Text> _playerStatus;

        public GameObject _chatBox;
        private GameObject _templateLine;
        int MaxPlayers = 8;

        public override void Initialize()
        {
            _templateLine = transform.Find("Chat Messages/Message (1)").gameObject;
            _chatBox = transform.Find("Chat Box").gameObject;

            foreach (Transform _child in _templateLine.transform.parent)
            {
                _child.gameObject.SetActive(false);
            }

            for (int i = 0; i < MaxPlayers; i++)
            {
                GameObject line = i == 0 ? _templateLine : Instantiate(_templateLine, transform.Find("Grid"));

                line.name = $"MessageablePlayer ({i + 1})";
                line.SetActive(false);

                _messageablePlayers.Add(line);
                _playerName.Add(line.transform.Find("PlayerName").GetComponent<Text>());
                _playerStatus.Add(line.transform.Find("Contents").GetComponent<Text>());
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
            RoomSystem.JoinedRoomEvent -= JoinedRoomEvent;
            RoomSystem.LeftRoomEvent -= LeftRoomEvent;
            RoomSystem.PlayerJoinedEvent -= PlayerJoinedEvent;
            RoomSystem.PlayerLeftEvent -= PlayerLeftEvent;
        }

        void RefreshApp()
        {
            if (!PhotonNetwork.InRoom || _messageablePlayers.Count == 0)
            {
                for (int i = 0; i < _messageablePlayers.Count; i++)
                {
                    _messageablePlayers[i].SetActive(false);
                }
                return;
            }

            foreach (Player _player in PhotonNetwork.PlayerListOthers)
            {
                if (_player.CustomProperties.ContainsKey(Constants.CustomProperty)) // we now know they are a MonkePhone user.
                    continue;

                NetPlayer[] players = NetworkSystem.Instance.AllNetPlayers;
                Array.Sort(players, (a, b) => a.ActorNumber.CompareTo(b.ActorNumber));

                for (int i = 0; i < _messageablePlayers.Count; i++)
                {
                    if (i >= players.Length)
                    {
                        _messageablePlayers[i].SetActive(false);
                        continue;
                    }

                    NetPlayer player = players[i];
                    if (player == null || player.IsNull || (!player.IsLocal && !player.InRoom))
                    {
                        _messageablePlayers[i].SetActive(false);
                        continue;
                    }

                    _messageablePlayers[i].SetActive(true);

                    GorillaParent.instance.vrrigDict.TryGetValue(player, out VRRig rig);

                    _playerName[i].text = player.GetName();
                }
            }

        }

        private void JoinedRoomEvent() => RefreshApp();

        private void LeftRoomEvent() => RefreshApp();

        private void PlayerJoinedEvent(NetPlayer player) => RefreshApp();

        private void PlayerLeftEvent(NetPlayer player) => RefreshApp();*/
    }
}
