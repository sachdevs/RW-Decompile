using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Verse
{
	public static class HealthUtility
	{
		private static List<string> vitalPawnCapacityTags = new List<string>();

		public static readonly Color GoodConditionColor = new Color(0.6f, 0.8f, 0.65f);

		public static readonly Color DarkRedColor = new Color(0.73f, 0.02f, 0.02f);

		public static readonly Color ImpairedColor = new Color(0.9f, 0.7f, 0f);

		public static readonly Color SlightlyImpairedColor = new Color(0.9f, 0.9f, 0f);

		public static void Reset()
		{
			HealthUtility.vitalPawnCapacityTags.Clear();
			HealthUtility.vitalPawnCapacityTags.AddRange(BodyPartDefOf.Heart.tags);
			HealthUtility.vitalPawnCapacityTags.AddRange(BodyPartDefOf.Liver.tags);
			HealthUtility.vitalPawnCapacityTags.AddRange(BodyPartDefOf.LeftLung.tags);
			HealthUtility.vitalPawnCapacityTags.AddRange(BodyPartDefOf.LeftKidney.tags);
			HealthUtility.vitalPawnCapacityTags.AddRange(BodyPartDefOf.Stomach.tags);
			HealthUtility.vitalPawnCapacityTags.AddRange(BodyPartDefOf.Brain.tags);
		}

		public static void HealInjuryRandom(Pawn pawn, float amount)
		{
			BodyPartRecord part;
			if (!pawn.health.hediffSet.GetInjuredParts().TryRandomElement(out part))
			{
				return;
			}
			Hediff_Injury hediff_Injury = null;
			foreach (Hediff_Injury current in from x in pawn.health.hediffSet.GetHediffs<Hediff_Injury>()
			where x.Part == part
			select x)
			{
				if (!current.IsOld())
				{
					if (hediff_Injury == null || current.Severity > hediff_Injury.Severity)
					{
						hediff_Injury = current;
					}
				}
			}
			if (hediff_Injury != null)
			{
				hediff_Injury.Heal(amount);
			}
		}

		public static string GetGeneralConditionLabel(Pawn pawn, bool shortVersion = false)
		{
			if (pawn.health.Dead)
			{
				return "Dead".Translate();
			}
			if (!pawn.health.capacities.CanBeAwake)
			{
				return "Unconscious".Translate();
			}
			if (pawn.health.InPainShock)
			{
				return (!shortVersion || !"PainShockShort".CanTranslate()) ? "PainShock".Translate() : "PainShockShort".Translate();
			}
			if (pawn.Downed)
			{
				return "Incapacitated".Translate();
			}
			bool flag = false;
			for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; i++)
			{
				Hediff_Injury hediff_Injury = pawn.health.hediffSet.hediffs[i] as Hediff_Injury;
				if (hediff_Injury != null)
				{
					if (!hediff_Injury.IsOld())
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				return "Injured".Translate();
			}
			if (pawn.health.hediffSet.PainTotal > 0.3f)
			{
				return "InPain".Translate();
			}
			return "Healthy".Translate();
		}

		public static Pair<string, Color> GetPartConditionLabel(Pawn pawn, BodyPartRecord part)
		{
			float partHealth = pawn.health.hediffSet.GetPartHealth(part);
			float maxHealth = part.def.GetMaxHealth(pawn);
			float num = partHealth / maxHealth;
			string first = string.Empty;
			Color second = Color.white;
			if (partHealth <= 0f)
			{
				Hediff_MissingPart hediff_MissingPart = null;
				List<Hediff_MissingPart> missingPartsCommonAncestors = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
				for (int i = 0; i < missingPartsCommonAncestors.Count; i++)
				{
					if (missingPartsCommonAncestors[i].Part == part)
					{
						hediff_MissingPart = missingPartsCommonAncestors[i];
						break;
					}
				}
				if (hediff_MissingPart == null)
				{
					bool fresh = false;
					if (hediff_MissingPart != null && hediff_MissingPart.IsFreshNonSolidExtremity)
					{
						fresh = true;
					}
					bool solid = part.def.IsSolid(part, pawn.health.hediffSet.hediffs);
					first = HealthUtility.GetGeneralDestroyedPartLabel(part, fresh, solid);
					second = Color.gray;
				}
				else
				{
					first = hediff_MissingPart.LabelCap;
					second = hediff_MissingPart.LabelColor;
				}
			}
			else if (num < 0.4f)
			{
				first = "SeriouslyImpaired".Translate();
				second = HealthUtility.DarkRedColor;
			}
			else if (num < 0.7f)
			{
				first = "Impaired".Translate();
				second = HealthUtility.ImpairedColor;
			}
			else if (num < 0.999f)
			{
				first = "SlightlyImpaired".Translate();
				second = HealthUtility.SlightlyImpairedColor;
			}
			else
			{
				first = "GoodCondition".Translate();
				second = HealthUtility.GoodConditionColor;
			}
			return new Pair<string, Color>(first, second);
		}

		public static string GetGeneralDestroyedPartLabel(BodyPartRecord part, bool fresh, bool solid)
		{
			if (part.parent == null)
			{
				return "SeriouslyImpaired".Translate();
			}
			if (part.depth != BodyPartDepth.Inside && !fresh)
			{
				return "MissingBodyPart".Translate();
			}
			if (solid)
			{
				return "ShatteredBodyPart".Translate();
			}
			return "DestroyedBodyPart".Translate();
		}

		private static IEnumerable<BodyPartRecord> HittablePartsViolence(HediffSet bodyModel)
		{
			return from x in bodyModel.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined)
			where x.depth == BodyPartDepth.Outside || (x.depth == BodyPartDepth.Inside && x.def.IsSolid(x, bodyModel.hediffs))
			select x;
		}

		public static void GiveInjuriesOperationFailureMinor(Pawn p, BodyPartRecord part)
		{
			HealthUtility.GiveRandomSurgeryInjuries(p, 20, part);
		}

		public static void GiveInjuriesOperationFailureCatastrophic(Pawn p, BodyPartRecord part)
		{
			HealthUtility.GiveRandomSurgeryInjuries(p, 65, part);
		}

		public static void GiveInjuriesOperationFailureRidiculous(Pawn p)
		{
			HealthUtility.GiveRandomSurgeryInjuries(p, 65, null);
		}

		private static void GiveRandomSurgeryInjuries(Pawn p, int totalDamage, BodyPartRecord operatedPart)
		{
			IEnumerable<BodyPartRecord> source;
			if (operatedPart == null)
			{
				source = from x in p.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined)
				where !x.def.isConceptual
				select x;
			}
			else
			{
				source = from x in p.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined)
				where !x.def.isConceptual
				select x into pa
				where pa == operatedPart || pa.parent == operatedPart || (operatedPart != null && operatedPart.parent == pa)
				select pa;
			}
			source = from x in source
			where HealthUtility.GetMinHealthOfPartsWeWantToAvoidDestroying(x, p) >= 2f
			select x;
			BodyPartRecord brain = p.health.hediffSet.GetBrain();
			if (brain != null)
			{
				float maxBrainHealth = brain.def.GetMaxHealth(p);
				source = from x in source
				where x != brain || p.health.hediffSet.GetPartHealth(x) >= maxBrainHealth * 0.5f + 1f
				select x;
			}
			while (totalDamage > 0 && source.Any<BodyPartRecord>())
			{
				BodyPartRecord bodyPartRecord = source.RandomElementByWeight((BodyPartRecord x) => x.coverageAbs);
				float partHealth = p.health.hediffSet.GetPartHealth(bodyPartRecord);
				int num = Mathf.Max(3, GenMath.RoundRandom(partHealth * Rand.Range(0.5f, 1f)));
				float minHealthOfPartsWeWantToAvoidDestroying = HealthUtility.GetMinHealthOfPartsWeWantToAvoidDestroying(bodyPartRecord, p);
				if (minHealthOfPartsWeWantToAvoidDestroying - (float)num < 1f)
				{
					num = Mathf.RoundToInt(minHealthOfPartsWeWantToAvoidDestroying - 1f);
				}
				if (bodyPartRecord == brain && partHealth - (float)num < brain.def.GetMaxHealth(p) * 0.5f)
				{
					num = Mathf.Max(Mathf.RoundToInt(partHealth - brain.def.GetMaxHealth(p) * 0.5f), 1);
				}
				if (num <= 0)
				{
					break;
				}
				DamageDef damageDef = Rand.Element<DamageDef>(DamageDefOf.Cut, DamageDefOf.Scratch, DamageDefOf.Stab, DamageDefOf.Crush);
				Thing arg_256_0 = p;
				DamageDef def = damageDef;
				int amount = num;
				BodyPartRecord hitPart = bodyPartRecord;
				arg_256_0.TakeDamage(new DamageInfo(def, amount, -1f, null, hitPart, null, DamageInfo.SourceCategory.ThingOrUnknown));
				totalDamage -= num;
			}
		}

		private static float GetMinHealthOfPartsWeWantToAvoidDestroying(BodyPartRecord part, Pawn pawn)
		{
			float num = 999999f;
			while (part != null)
			{
				if (HealthUtility.ShouldRandomSurgeryInjuriesAvoidDestroying(part, pawn))
				{
					num = Mathf.Min(num, pawn.health.hediffSet.GetPartHealth(part));
				}
				part = part.parent;
			}
			return num;
		}

		private static bool ShouldRandomSurgeryInjuriesAvoidDestroying(BodyPartRecord part, Pawn pawn)
		{
			if (part == pawn.RaceProps.body.corePart)
			{
				return true;
			}
			if (part.def.tags.Any((string x) => HealthUtility.vitalPawnCapacityTags.Contains(x)))
			{
				return true;
			}
			for (int i = 0; i < part.parts.Count; i++)
			{
				if (HealthUtility.ShouldRandomSurgeryInjuriesAvoidDestroying(part.parts[i], pawn))
				{
					return true;
				}
			}
			return false;
		}

		public static void DamageUntilDowned(Pawn p)
		{
			if (p.health.Downed)
			{
				return;
			}
			HediffSet hediffSet = p.health.hediffSet;
			p.health.forceIncap = true;
			IEnumerable<BodyPartRecord> source = from x in HealthUtility.HittablePartsViolence(hediffSet)
			where !p.health.hediffSet.hediffs.Any((Hediff y) => y.Part == x && y.CurStage != null && y.CurStage.partEfficiencyOffset < 0f)
			select x;
			int num = 0;
			while (num < 300 && !p.Downed && source.Any<BodyPartRecord>())
			{
				num++;
				BodyPartRecord bodyPartRecord = source.RandomElementByWeight((BodyPartRecord x) => x.coverageAbs);
				int num2 = Mathf.RoundToInt(hediffSet.GetPartHealth(bodyPartRecord)) - 3;
				if (num2 >= 8)
				{
					DamageDef damageDef;
					if (bodyPartRecord.depth == BodyPartDepth.Outside)
					{
						damageDef = HealthUtility.RandomViolenceDamageType();
					}
					else
					{
						damageDef = DamageDefOf.Blunt;
					}
					int num3 = Rand.RangeInclusive(Mathf.RoundToInt((float)num2 * 0.65f), num2);
					HediffDef hediffDefFromDamage = HealthUtility.GetHediffDefFromDamage(damageDef, p, bodyPartRecord);
					if (!p.health.WouldDieAfterAddingHediff(hediffDefFromDamage, bodyPartRecord, (float)num3))
					{
						DamageDef def = damageDef;
						int amount = num3;
						BodyPartRecord hitPart = bodyPartRecord;
						DamageInfo dinfo = new DamageInfo(def, amount, -1f, null, hitPart, null, DamageInfo.SourceCategory.ThingOrUnknown);
						dinfo.SetAllowDamagePropagation(false);
						p.TakeDamage(dinfo);
					}
				}
			}
			if (p.Dead)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine(p + " died during GiveInjuriesToForceDowned");
				for (int i = 0; i < p.health.hediffSet.hediffs.Count; i++)
				{
					stringBuilder.AppendLine("   -" + p.health.hediffSet.hediffs[i].ToString());
				}
				Log.Error(stringBuilder.ToString());
			}
			p.health.forceIncap = false;
		}

		public static void DamageUntilDead(Pawn p)
		{
			HediffSet hediffSet = p.health.hediffSet;
			int num = 0;
			while (!p.Dead && num < 200 && HealthUtility.HittablePartsViolence(hediffSet).Any<BodyPartRecord>())
			{
				num++;
				BodyPartRecord bodyPartRecord = HealthUtility.HittablePartsViolence(hediffSet).RandomElementByWeight((BodyPartRecord x) => x.coverageAbs);
				int num2 = Rand.RangeInclusive(8, 25);
				DamageDef damageDef;
				if (bodyPartRecord.depth == BodyPartDepth.Outside)
				{
					damageDef = HealthUtility.RandomViolenceDamageType();
				}
				else
				{
					damageDef = DamageDefOf.Blunt;
				}
				DamageDef def = damageDef;
				int amount = num2;
				BodyPartRecord hitPart = bodyPartRecord;
				DamageInfo dinfo = new DamageInfo(def, amount, -1f, null, hitPart, null, DamageInfo.SourceCategory.ThingOrUnknown);
				p.TakeDamage(dinfo);
			}
			if (!p.Dead)
			{
				Log.Error(p + " not killed during GiveInjuriesToKill");
			}
		}

		public static void DamageLegsUntilIncapableOfMoving(Pawn p)
		{
			HediffDef def = Rand.Element<HediffDef>(HediffDefOf.Scratch, HediffDefOf.Bruise, HediffDefOf.Bite, HediffDefOf.Cut);
			int num = 0;
			while (p.health.capacities.CapableOf(PawnCapacityDefOf.Moving) && num < 300)
			{
				num++;
				IEnumerable<BodyPartRecord> source = from x in p.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined)
				where x.def.tags.Contains("MovingLimbCore") && p.health.hediffSet.GetPartHealth(x) >= 2f
				select x;
				if (!source.Any<BodyPartRecord>())
				{
					break;
				}
				BodyPartRecord bodyPartRecord = source.RandomElement<BodyPartRecord>();
				float maxHealth = bodyPartRecord.def.GetMaxHealth(p);
				float partHealth = p.health.hediffSet.GetPartHealth(bodyPartRecord);
				int min = Mathf.Clamp(Mathf.RoundToInt(maxHealth * 0.12f), 1, (int)partHealth - 1);
				int max = Mathf.Clamp(Mathf.RoundToInt(maxHealth * 0.27f), 1, (int)partHealth - 1);
				int num2 = Rand.RangeInclusive(min, max);
				if (p.health.WouldDieAfterAddingHediff(def, bodyPartRecord, (float)num2))
				{
					break;
				}
				Hediff_Injury hediff_Injury = (Hediff_Injury)HediffMaker.MakeHediff(def, p, bodyPartRecord);
				hediff_Injury.Severity = (float)num2;
				p.health.AddHediff(hediff_Injury, null, null);
			}
		}

		public static DamageDef RandomViolenceDamageType()
		{
			switch (Rand.RangeInclusive(0, 4))
			{
			case 0:
				return DamageDefOf.Bullet;
			case 1:
				return DamageDefOf.Blunt;
			case 2:
				return DamageDefOf.Stab;
			case 3:
				return DamageDefOf.Scratch;
			case 4:
				return DamageDefOf.Cut;
			default:
				return null;
			}
		}

		public static HediffDef GetHediffDefFromDamage(DamageDef dam, Pawn pawn, BodyPartRecord part)
		{
			HediffDef result = dam.hediff;
			if (part.def.IsSkinCovered(part, pawn.health.hediffSet) && dam.hediffSkin != null)
			{
				result = dam.hediffSkin;
			}
			if (part.def.IsSolid(part, pawn.health.hediffSet.hediffs) && dam.hediffSolid != null)
			{
				result = dam.hediffSolid;
			}
			return result;
		}

		public static bool TryAnesthetize(Pawn pawn)
		{
			if (!pawn.RaceProps.IsFlesh)
			{
				return false;
			}
			pawn.health.forceIncap = true;
			pawn.health.AddHediff(HediffDefOf.Anesthetic, null, null);
			pawn.health.forceIncap = false;
			return true;
		}

		public static void AdjustSeverity(Pawn pawn, HediffDef hdDef, float sevOffset)
		{
			if (sevOffset == 0f)
			{
				return;
			}
			Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hdDef, false);
			if (hediff != null)
			{
				hediff.Severity += sevOffset;
			}
			else if (sevOffset > 0f)
			{
				hediff = HediffMaker.MakeHediff(hdDef, pawn, null);
				hediff.Severity = sevOffset;
				pawn.health.AddHediff(hediff, null, null);
			}
		}

		public static BodyPartRemovalIntent PartRemovalIntent(Pawn pawn, BodyPartRecord part)
		{
			if (pawn.health.hediffSet.hediffs.Any((Hediff d) => d.Visible && d.Part == part && d.def.isBad))
			{
				return BodyPartRemovalIntent.Amputate;
			}
			return BodyPartRemovalIntent.Harvest;
		}

		public static int TicksUntilDeathDueToBloodLoss(Pawn pawn)
		{
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss, false);
			if (firstHediffOfDef == null)
			{
				return 2147483647;
			}
			return (int)((1f - firstHediffOfDef.Severity) / pawn.health.hediffSet.BleedRateTotal * 60000f);
		}
	}
}
