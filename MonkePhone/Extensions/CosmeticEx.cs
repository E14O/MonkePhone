using GorillaNetworking;
using System.Collections.Generic;

namespace MonkePhone.Extensions
{
	public static class CosmeticEx
	{
		public class CosmeticInfo
		{
			public string CosmeticId { get; set; }
			public string DisplayName { get; set; }
		}
		// ima be real idk if this works 
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
	}
}