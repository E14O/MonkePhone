using GorillaNetworking;
using Photon.Pun;
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
            // use this text and edit others
          //  _roomInfo.text = $"Room ID: {NetworkSystem.Instance.RoomName} Game Mode:";

            // god awful way to do this and will be edited later

            if (_roomInfo == null) return;
            if (!PhotonNetwork.InRoom)
            {
                _roomInfo.text = $"Room ID: Not Connected Game Mode: Not Connected";
            }
            else
            {
                string gameMode = GorillaComputer.instance.currentGameMode.Value;
                string roomName = PhotonNetwork.CurrentRoom.Name;
                object modeObj;
                if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gameMode", out modeObj) && modeObj != null)
                {
                    string modeStr = modeObj.ToString();
                    if (modeStr.Contains("MODDED_Casual"))
                    {
                        _roomInfo.text = $"{NetworkSystem.Instance.RoomPlayerCount}/10 Room Id: {roomName} Game: Modded Casual";
                    }
                    else if (modeStr.Contains("MODDED_Infection"))
                    {
                        _roomInfo.text = $"{NetworkSystem.Instance.RoomPlayerCount}/10 Room Id: {roomName} Game: Modded Infection";
                    }
                    else if (modeStr.Contains("MODDED_Guardian"))
                    {
                        _roomInfo.text = $"{NetworkSystem.Instance.RoomPlayerCount}/10 Room Id: {roomName} Game: Modded Guardian";
                    }
                    else if (modeStr.Contains("MODDED_FreezeTag"))
                    {
                        _roomInfo.text = $"{NetworkSystem.Instance.RoomPlayerCount}/10 Room Id: {roomName} Game: Modded FreezeTag";
                    }
                    else
                    {
                        _roomInfo.text = $"{NetworkSystem.Instance.RoomPlayerCount}/10  Room Id:  {roomName}  Game: ";
                    }
                }
                else
                {
                    _roomInfo.text = $"{NetworkSystem.Instance.RoomPlayerCount}/10  Room Id:  {roomName}  Game: ";
                }
            }
        }

        private void JoinedRoomEvent() => RefreshApp();

        private void LeftRoomEvent() => RefreshApp();

        private void PlayerJoinedEvent(NetPlayer player) => RefreshApp();

        private void PlayerLeftEvent(NetPlayer player) => RefreshApp();
    }
}
