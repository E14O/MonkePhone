using GorillaGameModes;
using GorillaNetworking;
using Photon.Pun;
using UnityEngine.UI;

namespace MonkePhone.Behaviours.Apps
{
    public class ScoreboardApp : PhoneApp
    {
        // Plan: display if the user is using monke phone on the scoreboard with a image.
        // Plan: Everything below is subject to be edited, just a placeholder for now.

        public override string AppId => "Scoreboard";

        private string _roomName => NetworkSystem.Instance.RoomName;

        private Text _roomInfo;

        public override void Initialize()
        {
            _roomInfo = transform.Find("Header (1)").GetComponent<Text>();
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

        private void RefreshApp()
        {
            if (_roomInfo == null) return;
            if (!NetworkSystem.Instance.InRoom)
            {
                _roomInfo.text = $"Room ID: Not Connected Game Mode: Not Connected";
            }
            else
            {
                _roomInfo.text = $"Room ID: {_roomName} GameMode: {NetworkSystem.Instance.GameModeString.};";
            }
        }

        private void JoinedRoomEvent() => RefreshApp();

        private void LeftRoomEvent() => RefreshApp();

        private void PlayerJoinedEvent(NetPlayer player) => RefreshApp();

        private void PlayerLeftEvent(NetPlayer player) => RefreshApp();
    }
}
