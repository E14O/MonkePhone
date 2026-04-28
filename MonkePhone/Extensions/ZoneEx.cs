using System;

namespace MonkePhone.Extensions
{
    public static class ZoneEx
    {
        public static string ToTitleCase(this GTZone zone)
        {
            return zone switch
            {
                GTZone.cityNoBuildings => "City",
                GTZone.skyJungle => "Clouds",
                GTZone.cityWithSkyJungle => "City",
                GTZone.monkeBlocksShared => "Share My Blocks",
                GTZone.monkeBlocks => "Monke Blocks",
                GTZone.customMaps => "Virtual Stump",
                GTZone.ghostReactor => "Ghost Reactor",
                GTZone.hoverboard => "Hoverpark",
                GTZone.ranked => "Ranked",
                GTZone.tutorial => "Tutorial",
                GTZone.canyon => "Canyon",
                GTZone.cave => "Cave",
                GTZone.beach => "Beach",
                GTZone.basement => "Basement",
                GTZone.Metropolis => "Metropolis",
                GTZone.arcade => "Arcade",
                GTZone.critters => "Critters",
                GTZone.mountain => "Mountain",
				_ => Enum.GetName(typeof(GTZone), zone).ToTitleCase()
            };
        }
    }
}
