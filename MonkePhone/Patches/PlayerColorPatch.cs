using GorillaNetworking;
using UnityEngine;

namespace MonkePhone.Patches
{
	public static class PlayerColorPatch
	{
		public static bool GetPlayerColorAndMaterial(VRRig rig, out Color color, out Material material)
		{
			color = Color.white;
			material = null;

			if (rig == null)
				return false;

			int setMatIndex = rig.setMatIndex;

			if (setMatIndex == 0)
			{
				material = rig.scoreboardMaterial;
				color = rig.playerColor;
			}
			else if (setMatIndex > 0 && rig.materialsToChangeTo != null && setMatIndex < rig.materialsToChangeTo.Length)
			{
				material = rig.materialsToChangeTo[setMatIndex];
				if (material != null)
					color = material.color;
			}

			if (color.a < 0.1f)
				color.a = 1f;

			return true;
		}

		public static Color GetPlayerColor(VRRig rig)
		{
			GetPlayerColorAndMaterial(rig, out Color color, out Material _);
			return color;
		}

		public static Material GetPlayerMaterial(VRRig rig)
		{
			GetPlayerColorAndMaterial(rig, out Color _, out Material material);
			return material;
		}
	}
}