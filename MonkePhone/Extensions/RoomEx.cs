using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;

namespace MonkePhone.Extensions
{
    public static class RoomEx
    {
        public static string GetRoomId(this Room room)
        {
            if (room == null || !PhotonNetwork.InRoom)
                return "Not In Room";

            return room.Name;
        }

        public static string GetGameMode(this Room room)
        {
            if (room == null || !PhotonNetwork.InRoom)
                return "None";

            if (room.CustomProperties.TryGetValue("gameMode", out object modeObj) && modeObj != null)
            {
                string modeStr = modeObj.ToString();

                // TODO: someone find out the names of the gamemode values

                // if (modeStr.Contains("SUPER_Casual"))
                // return "(S)Casual";
                // else if (modeStr.Contains("SUPER_Infection"))
                //return "(S)Infection";
                if (modeStr.Contains("Casual"))
                    return "Casual";
                else if (modeStr.Contains("Infection"))
                    return "Infection";

                // if (modeStr.Contains("MODDED_SUPER_Casual"))
                // return "(S)(M)Casual";
                // else if (modeStr.Contains("MODDED_SUPER_Infection"))
                // return "(S)(M)Infection";
                if (modeStr.Contains("MODDED_Casual"))
                    return "(M)Casual";
                else if (modeStr.Contains("MODDED_Infection"))
                    return "(M)Infection";

                return modeStr;
            }
            else if (GorillaComputer.instance != null)
            {
                return GorillaComputer.instance.currentGameMode.Value;
            }

            return "Unknown";
        }

        public static int GetPlayerCount(this Room room)
        {
            if (room == null || !PhotonNetwork.InRoom)
                return 0;

            return room.PlayerCount;
        }

        public static int GetMaxPlayers(this Room room)
        {
            if (room == null || !PhotonNetwork.InRoom)
                return 0;

            return room.MaxPlayers;
        }

        public static string GetRoomInfo(this Room room)
        {
            if (room == null || !PhotonNetwork.InRoom)
                return "Room Id: Not In Room Game Mode: None";

            return $"Room Id: {room.GetRoomId()} Game Mode: {room.GetGameMode()}";
        }
    }
}