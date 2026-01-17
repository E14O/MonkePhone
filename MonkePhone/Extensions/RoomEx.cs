using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;

namespace MonkePhone.Extensions
{
	public static class RoomEx
	{
		public static string GetCurrentRoomId(this Room room)
		{
			if (room == null || !PhotonNetwork.InRoom)
				return "Not In Room";

			return room.Name;
		}

		public static string GetCurrentGamemode(this Room room)
		{
			if (room == null || !PhotonNetwork.InRoom)
				return "None";

			if (room.CustomProperties.TryGetValue("gameMode", out object modeObj) && modeObj != null)
			{
				string modeStr = modeObj.ToString();

				if (modeStr.Contains("MODDED_Casual"))
					return "(M) Casual";
				else if (modeStr.Contains("MODDED_Infection"))
					return "(M) Infection";
				else if (modeStr.Contains("Casual"))
					return "Casual";
				else if (modeStr.Contains("Infection"))
					return "Infection";
				else
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

		public static string GetCurrentRoomInfo(this Room room)
		{
			if (room == null || !PhotonNetwork.InRoom)
				return "Room Id: Not In Room Game Mode: None";

			return $"Room Id: {room.GetCurrentRoomId()} | GameMode: {room.GetCurrentGamemode()}";
		}
	}
}