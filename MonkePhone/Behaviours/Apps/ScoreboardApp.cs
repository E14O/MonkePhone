using System;
using System.Collections.Generic;
using MonkePhone.Extensions;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace MonkePhone.Behaviours.Apps
{
    public class ScoreboardApp : PhoneApp
    {
        public override string AppId => "Scoreboard";

        private readonly List<Text> _playerNameTexts = [];
        private readonly List<Image> _playerSwatches = [];
        private readonly List<GameObject> _playerLines = [];

        private Text _roomIdText;
        private Text _roomNotice;

        private GameObject _templateLine;

        private const int MaxPlayers = 10;

        public override void Initialize()
        {
            _roomIdText = transform.Find("Roominfo").GetComponent<Text>();
            _roomNotice = transform.Find("Notice").GetComponent<Text>();
            _templateLine = transform.Find("Grid/Person (1)").gameObject;
            _templateLine.SetActive(false);

            _playerNameTexts.Clear();
            _playerSwatches.Clear();
            _playerLines.Clear();

            for (int i = 0; i < MaxPlayers; i++)
            {
                GameObject line = i == 0 ? _templateLine : Instantiate(_templateLine, transform.Find("Grid"));

                line.name = $"Person ({i + 1})";
                line.SetActive(false);

                _playerLines.Add(line);
                _playerNameTexts.Add(line.transform.Find("Name").GetComponent<Text>());
                _playerSwatches.Add(line.transform.Find("Swatch").GetComponent<Image>());
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

            RefreshApp();
        }

        public void RefreshApp()
        {
            _roomIdText.text = PhotonNetwork.CurrentRoom.GetRoomInfo();

            if (!PhotonNetwork.InRoom || _playerLines.Count == 0)
            {
                for (int i = 0; i < _playerLines.Count; i++)
                    _playerLines[i].SetActive(false);

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
                    continue;
                }

                NetPlayer player = players[i];
                if (player == null || player.IsNull || (!player.IsLocal && !player.InRoom))
                {
                    _playerLines[i].SetActive(false);
                    continue;
                }

                _playerLines[i].SetActive(true);

                _roomNotice.gameObject.SetActive(false);

                GorillaParent.instance.vrrigDict.TryGetValue(player, out VRRig rig);

                _playerNameTexts[i].text = player.GetName();
                _playerNameTexts[i].color = rig != null ? rig.playerText1.color : Color.white;

                SetSwatchColour(_playerSwatches[i], rig);
            }
        }

        private void SetSwatchColour(Image swatch, VRRig rig)
        {
            if (swatch == null)
                return;

            if (rig == null)
            {
                swatch.material = null;
                swatch.color = Color.white;
                return;
            }

            Material material;

            if (rig.setMatIndex == 0)
            {
                material = rig.scoreboardMaterial;
                swatch.color = rig.playerColor;
            }
            else
            {
                material = rig.materialsToChangeTo[rig.setMatIndex];
                swatch.color = material.color;
            }

            swatch.material = material;
        }

        private void JoinedRoomEvent() => RefreshApp();

        private void LeftRoomEvent() => RefreshApp();

        private void PlayerJoinedEvent(NetPlayer player) => RefreshApp();

        private void PlayerLeftEvent(NetPlayer player) => RefreshApp();
    }
}
