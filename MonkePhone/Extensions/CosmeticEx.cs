using GorillaNetworking;
using System.Collections.Generic;
using System.Linq;

namespace MonkePhone.Extensions
{
	public static class CosmeticEx
	{
		public class CosmeticInfo
		{
			public string CosmeticId { get; set; }
			public string DisplayName { get; set; }
		}
		// yea i still dont know if this gonna help me or not
		public static readonly List<CosmeticInfo> KnownCosmetics = new List<CosmeticInfo>
		{
			new CosmeticInfo { CosmeticId = "LMAPY.", DisplayName = "FIRE STICK" },
			new CosmeticInfo { CosmeticId = "LBADE.", DisplayName = "FINGER PAINTER" },
			new CosmeticInfo { CosmeticId = "LBAGS.", DisplayName = "ILLUSTRATOR" },

		};
		public static bool HasCosmetic(this VRRig rig, string cosmeticId)
		{
			if (rig == null || string.IsNullOrEmpty(cosmeticId))
				return false;

			return rig.rawCosmeticString.Contains(cosmeticId);
		}

		public static bool HasFireStick(this VRRig rig) => rig.HasCosmetic("LMAPY.");

		public static bool HasFingerPainter(this VRRig rig) => rig.HasCosmetic("LBADE.");

		public static bool HasIllustrator(this VRRig rig) => rig.HasCosmetic("LBAGS.");

		

		public static List<CosmeticInfo> GetPlayerCosmetics(this VRRig rig)
		{
			List<CosmeticInfo> playerCosmetics = new List<CosmeticInfo>();

			if (rig == null)
				return playerCosmetics;

			foreach (var cosmetic in KnownCosmetics)
			{
				if (rig.HasCosmetic(cosmetic.CosmeticId))
				{
					playerCosmetics.Add(cosmetic);
				}
			}

			return playerCosmetics;
		}

		public static bool HasAnyCosmetic(this VRRig rig)
		{
			return rig.GetPlayerCosmetics().Count > 0;
		}

		public static bool HasAllCosmetics(this VRRig rig)
		{
			return rig.GetPlayerCosmetics().Count == KnownCosmetics.Count;
		}

		public static string GetCosmeticString(this VRRig rig)
		{
			if (rig == null)
				return string.Empty;

			return rig.rawCosmeticString;
		}
	}
}