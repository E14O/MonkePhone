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
				if (modeStr.Contains("MODDED_SuperCasual"))
					return "(M)(S) Casual";
				else if (modeStr.Contains("MODDED_SuperInfect"))
					return "(M)(S) Infection";
				else if (modeStr.Contains("MODDED_Casual"))
					return "(M) Casual";
				else if (modeStr.Contains("MODDED_Infection"))
					return "(M) Infection";
				else if (modeStr.Contains("MODDED_Paintbrawl"))
					return "(M) Paintbrawl";
				else if (modeStr.Contains("MODDED_Guardian"))
					return "(M) Guardian";
				else if (modeStr.Contains("Casual"))
					return "Casual";
				else if (modeStr.Contains("Infection"))
					return "Infection";
				else if (modeStr.Contains("Paintbrawl"))
					return "Paintbrawl";
				else if (modeStr.Contains("Guardian"))
					return "Guardian";
				else if (modeStr.Contains("SuperInfect"))
					return "(S) Infection";
				else if (modeStr.Contains("SuperCasual"))
					return "(S) Casual";
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