using ExitGames.Client.Photon;
using GorillaNetworking;
using MonkePhone.Behaviours;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using System.Linq;
using UnityEngine;

namespace MonkePhone.Extensions
{
	public static class PlayerEx
	{
		public static string GetName(this NetPlayer player, VRRig vrRig = null, bool includePhone = true)
		{
			if (player.GetPlayerRef() != null)
			{
				return GetName(player.GetPlayerRef(), vrRig, includePhone);
			}
			int actor = player.ActorNumber;
			if (player.InRoom && PhotonNetwork.InRoom)
			{
				var punPlayer = PhotonNetwork.CurrentRoom.GetPlayer(actor);
				if (punPlayer != null)
				{
					return GetName(punPlayer, vrRig, includePhone);
				}
			}
			return "n/a";
		}

		public static string GetName(this Player player, VRRig vrRig = null, bool includePhone = true)
		{
			string safetyCheckName = PlayFabAuthenticator.instance.GetSafety() ? player.DefaultName : player.NickName;
			safetyCheckName = safetyCheckName.ToUpper();

			if (safetyCheckName.Length > 12)
			{
				safetyCheckName.Substring(0, 12);
			}

			if (!vrRig) return safetyCheckName;

			Hashtable customProperties = player.CustomProperties;
			CosmeticsController.CosmeticSet cosmeticSet = vrRig.cosmeticSet;
			
			string summaryAppendage = "";

			if (includePhone && customProperties.ContainsKey(Constants.CustomProperty))
			{
				summaryAppendage += PhoneManager.Instance.Data.phoneEmoji;
			}

			
			return summaryAppendage == "" ? safetyCheckName : $"{safetyCheckName}{summaryAppendage}";
		}
		public static Color GetColor(this VRRig rig)
		{
			return rig.playerColor;
		}
		public static bool IsTalking(NetPlayer player, VRRig rig)
		{
			if (player == null || rig == null) return false;
			if (GorillaComputer.instance.voiceChatOn == "FALSE") return false;

			if (rig.remoteUseReplacementVoice || rig.localUseReplacementVoice)
				return rig.SpeakingLoudness > rig.replacementVoiceLoudnessThreshold;

			if (player.IsLocal)
			{
				Recorder rec = NetworkSystem.Instance.LocalRecorder;
				return rec != null && rec.IsCurrentlyTransmitting;
			}

			Speaker speaker = rig.GetComponentInChildren<Speaker>();
			return speaker != null && speaker.IsPlaying;
		}
	}
}