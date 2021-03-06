using System;
using System.Linq;
using Verse;

namespace RimWorld.Planet
{
	public static class WorldGenerator
	{
		public const float DefaultPlanetCoverage = 0.3f;

		public const OverallRainfall DefaultOverallRainfall = OverallRainfall.Normal;

		public const OverallTemperature DefaultOverallTemperature = OverallTemperature.Normal;

		public static World GenerateWorld(float planetCoverage, string seedString, OverallRainfall overallRainfall, OverallTemperature overallTemperature)
		{
			DeepProfiler.Start("GenerateWorld");
			Rand.Seed = (GenText.StableStringHash(seedString) ^ 4323276);
			Current.CreatingWorld = new World();
			Current.CreatingWorld.info.planetCoverage = planetCoverage;
			Current.CreatingWorld.info.seedString = seedString;
			Current.CreatingWorld.info.overallRainfall = overallRainfall;
			Current.CreatingWorld.info.overallTemperature = overallTemperature;
			Current.CreatingWorld.info.name = NameGenerator.GenerateName(RulePackDefOf.NamerWorld, null, false, null);
			foreach (WorldGenStepDef current in from gs in DefDatabase<WorldGenStepDef>.AllDefs
			orderby gs.order
			select gs)
			{
				DeepProfiler.Start("WorldGenStep - " + current);
				try
				{
					current.worldGenStep.GenerateFresh(seedString);
				}
				catch (Exception arg)
				{
					Log.Error("Error in WorldGenStep: " + arg);
				}
				finally
				{
					DeepProfiler.End();
				}
			}
			Current.CreatingWorld.grid.StandardizeTileData();
			Current.CreatingWorld.FinalizeInit();
			Find.Scenario.PostWorldGenerate();
			World creatingWorld = Current.CreatingWorld;
			Current.CreatingWorld = null;
			DeepProfiler.End();
			return creatingWorld;
		}
	}
}
