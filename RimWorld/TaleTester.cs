using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public static class TaleTester
	{
		public static void LogGeneratedTales(int count)
		{
			List<Tale> list = new List<Tale>();
			for (int i = 0; i < count; i++)
			{
				list.Add(TaleFactory.MakeRandomTestTale(null));
			}
			TaleTester.LogTales(list);
		}

		public static void LogSpecificTale(TaleDef def, int count)
		{
			List<Tale> list = new List<Tale>();
			for (int i = 0; i < count; i++)
			{
				list.Add(TaleFactory.MakeRandomTestTale(def));
			}
			TaleTester.LogTales(list);
		}

		public static void LogTalesInDatabase()
		{
			TaleTester.LogTales(from t in Find.TaleManager.AllTalesListForReading
			where t.def.usableForArt
			select t);
		}

		public static void LogDescriptionsTaleless()
		{
			List<Tale> list = new List<Tale>();
			for (int i = 0; i < 20; i++)
			{
				list.Add(null);
			}
			TaleTester.LogTales(list);
		}

		private static void LogTales(IEnumerable<Tale> tales)
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			foreach (Tale current in tales)
			{
				TaleReference tr = new TaleReference(current);
				stringBuilder.AppendLine(TaleTester.RandomArtworkName(tr));
				stringBuilder.AppendLine(TaleTester.RandomArtworkDescription(tr));
				stringBuilder.AppendLine();
				num++;
				if (num % 20 == 0)
				{
					Log.Message(stringBuilder.ToString());
					stringBuilder = new StringBuilder();
				}
			}
			if (!stringBuilder.ToString().NullOrEmpty())
			{
				Log.Message(stringBuilder.ToString());
			}
		}

		private static string RandomArtworkName(TaleReference tr)
		{
			List<Rule> list = new List<Rule>();
			switch (Rand.RangeInclusive(0, 4))
			{
			case 0:
				list.AddRange(RulePackDefOf.ArtName_Sculpture.RulesPlusIncludes);
				break;
			case 1:
				list.AddRange(RulePackDefOf.ArtName_WeaponMelee.RulesPlusIncludes);
				break;
			case 2:
				list.AddRange(RulePackDefOf.ArtName_WeaponGun.RulesPlusIncludes);
				break;
			case 3:
				list.AddRange(RulePackDefOf.ArtName_Furniture.RulesPlusIncludes);
				break;
			case 4:
				list.AddRange(RulePackDefOf.ArtName_SarcophagusPlate.RulesPlusIncludes);
				break;
			}
			return tr.GenerateText(TextGenerationPurpose.ArtName, list);
		}

		private static string RandomArtworkDescription(TaleReference tr)
		{
			List<Rule> list = new List<Rule>();
			switch (Rand.RangeInclusive(0, 4))
			{
			case 0:
				list.AddRange(RulePackDefOf.ArtDescription_Sculpture.RulesPlusIncludes);
				break;
			case 1:
				list.AddRange(RulePackDefOf.ArtDescription_WeaponMelee.RulesPlusIncludes);
				break;
			case 2:
				list.AddRange(RulePackDefOf.ArtDescription_WeaponGun.RulesPlusIncludes);
				break;
			case 3:
				list.AddRange(RulePackDefOf.ArtDescription_Furniture.RulesPlusIncludes);
				break;
			case 4:
				list.AddRange(RulePackDefOf.ArtDescription_SarcophagusPlate.RulesPlusIncludes);
				break;
			}
			return tr.GenerateText(TextGenerationPurpose.ArtDescription, list);
		}
	}
}
