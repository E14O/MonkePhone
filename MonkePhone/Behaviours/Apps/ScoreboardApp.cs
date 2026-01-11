using UnityEngine.UI;

namespace MonkePhone.Behaviours.Apps
{
    public class ScoreboardApp : PhoneApp
    {
        public override string AppId => "Scoreboard";

        // Plan: display if the user is using monke phone on the scoreboard with a image.
        // Plan: Everything below is subject to be edited, just a placeholder for now.

        private Text _roomInfo;

        public override void Initialize()
        {
            _roomInfo = transform.Find("RoomInfo").GetComponent<Text>();
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
            _roomInfo.text = $"Room ID: {NetworkSystem.Instance.RoomName} Game Mode:";
        }

        private void JoinedRoomEvent() => RefreshApp();

        private void LeftRoomEvent() => RefreshApp();

        private void PlayerJoinedEvent(NetPlayer player) => RefreshApp();

        private void PlayerLeftEvent(NetPlayer player) => RefreshApp();
    }
}
