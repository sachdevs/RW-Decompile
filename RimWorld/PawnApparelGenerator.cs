using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public static class PawnApparelGenerator
	{
		private class PossibleApparelSet
		{
			private List<ThingStuffPair> aps = new List<ThingStuffPair>();

			private HashSet<ApparelUtility.LayerGroupPair> lgps = new HashSet<ApparelUtility.LayerGroupPair>();

			private BodyDef body;

			private ThingDef raceDef;

			private const float StartingMinTemperature = 12f;

			private const float TargetMinTemperature = -40f;

			private const float StartingMaxTemperature = 32f;

			private const float TargetMaxTemperature = 30f;

			public int Count
			{
				get
				{
					return this.aps.Count;
				}
			}

			public float TotalPrice
			{
				get
				{
					return this.aps.Sum((ThingStuffPair pa) => pa.Price);
				}
			}

			public float TotalInsulationCold
			{
				get
				{
					return this.aps.Sum((ThingStuffPair a) => a.InsulationCold);
				}
			}

			public void Reset(BodyDef body, ThingDef raceDef)
			{
				this.aps.Clear();
				this.lgps.Clear();
				this.body = body;
				this.raceDef = raceDef;
			}

			public void Add(ThingStuffPair pair)
			{
				this.aps.Add(pair);
				ApparelUtility.GenerateLayerGroupPairs(this.body, pair.thing, delegate(ApparelUtility.LayerGroupPair lgp)
				{
					this.lgps.Add(lgp);
				});
			}

			public bool PairOverlapsAnything(ThingStuffPair pair)
			{
				bool conflicts = false;
				ApparelUtility.GenerateLayerGroupPairs(this.body, pair.thing, delegate(ApparelUtility.LayerGroupPair lgp)
				{
					conflicts |= this.lgps.Contains(lgp);
				});
				return conflicts;
			}

			public bool CoatButNoShirt()
			{
				bool flag = false;
				bool flag2 = false;
				for (int i = 0; i < this.aps.Count; i++)
				{
					if (this.aps[i].thing.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))
					{
						for (int j = 0; j < this.aps[i].thing.apparel.layers.Count; j++)
						{
							ApparelLayer apparelLayer = this.aps[i].thing.apparel.layers[j];
							if (apparelLayer == ApparelLayer.OnSkin)
							{
								flag2 = true;
							}
							if (apparelLayer == ApparelLayer.Shell || apparelLayer == ApparelLayer.Middle)
							{
								flag = true;
							}
						}
					}
				}
				return flag && !flag2;
			}

			public bool Covers(BodyPartGroupDef bp)
			{
				for (int i = 0; i < this.aps.Count; i++)
				{
					if (this.aps[i].thing.apparel.bodyPartGroups.Contains(bp))
					{
						return true;
					}
				}
				return false;
			}

			public bool IsNaked(Gender gender)
			{
				if (gender == Gender.Male)
				{
					return !this.Covers(BodyPartGroupDefOf.Legs);
				}
				if (gender != Gender.Female)
				{
					return gender != Gender.None && false;
				}
				return !this.Covers(BodyPartGroupDefOf.Legs) || !this.Covers(BodyPartGroupDefOf.Torso);
			}

			public bool SatisfiesNeededWarmth(NeededWarmth warmth, bool mustBeSafe = false, float mapTemperature = 21f)
			{
				if (warmth == NeededWarmth.Any)
				{
					return true;
				}
				if (mustBeSafe && !GenTemperature.SafeTemperatureRange(this.raceDef, this.aps).Includes(mapTemperature))
				{
					return false;
				}
				if (warmth == NeededWarmth.Cool)
				{
					float num = this.aps.Sum((ThingStuffPair a) => a.InsulationHeat);
					return num >= -2f;
				}
				if (warmth == NeededWarmth.Warm)
				{
					float num2 = this.aps.Sum((ThingStuffPair a) => a.InsulationCold);
					return num2 <= -52f;
				}
				throw new NotImplementedException();
			}

			public void AddFreeWarmthAsNeeded(NeededWarmth warmth)
			{
				if (warmth == NeededWarmth.Any)
				{
					return;
				}
				if (warmth == NeededWarmth.Cool)
				{
					return;
				}
				if (DebugViewSettings.logApparelGeneration)
				{
					PawnApparelGenerator.debugSb.AppendLine();
					PawnApparelGenerator.debugSb.AppendLine("Trying to give free warm layer.");
				}
				if (!this.SatisfiesNeededWarmth(warmth, false, 21f))
				{
					if (DebugViewSettings.logApparelGeneration)
					{
						PawnApparelGenerator.debugSb.AppendLine("Checking to give free torso-cover at max price " + PawnApparelGenerator.freeWarmParkaMaxPrice);
					}
					Predicate<ThingStuffPair> parkaPairValidator = (ThingStuffPair pa) => pa.Price <= PawnApparelGenerator.freeWarmParkaMaxPrice && pa.InsulationCold <= -40f;
					ThingStuffPair parkaPair;
					if ((from pa in PawnApparelGenerator.allApparelPairs
					where parkaPairValidator(pa)
					select pa).TryRandomElementByWeight((ThingStuffPair pa) => pa.Commonality / (pa.Price * pa.Price), out parkaPair))
					{
						if (DebugViewSettings.logApparelGeneration)
						{
							PawnApparelGenerator.debugSb.AppendLine(string.Concat(new object[]
							{
								"Giving free torso-cover: ",
								parkaPair,
								" insulation=",
								parkaPair.InsulationCold
							}));
							foreach (ThingStuffPair current in from a in this.aps
							where !ApparelUtility.CanWearTogether(a.thing, parkaPair.thing, this.body)
							select a)
							{
								PawnApparelGenerator.debugSb.AppendLine(string.Concat(new object[]
								{
									"    -replaces ",
									current.ToString(),
									" InsulationCold=",
									current.InsulationCold
								}));
							}
						}
						this.aps.RemoveAll((ThingStuffPair pa) => !ApparelUtility.CanWearTogether(pa.thing, parkaPair.thing, this.body));
						this.aps.Add(parkaPair);
					}
				}
				if (!this.SatisfiesNeededWarmth(warmth, false, 21f))
				{
					if (DebugViewSettings.logApparelGeneration)
					{
						PawnApparelGenerator.debugSb.AppendLine("Checking to give free hat at max price " + PawnApparelGenerator.freeWarmHatMaxPrice);
					}
					Predicate<ThingStuffPair> hatPairValidator = (ThingStuffPair pa) => pa.Price <= PawnApparelGenerator.freeWarmHatMaxPrice && pa.InsulationCold <= -7f && (pa.thing.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead) || pa.thing.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead));
					ThingStuffPair hatPair;
					if ((from pa in PawnApparelGenerator.allApparelPairs
					where hatPairValidator(pa)
					select pa).TryRandomElementByWeight((ThingStuffPair pa) => pa.Commonality / (pa.Price * pa.Price), out hatPair))
					{
						if (DebugViewSettings.logApparelGeneration)
						{
							PawnApparelGenerator.debugSb.AppendLine(string.Concat(new object[]
							{
								"Giving free hat: ",
								hatPair,
								" insulation=",
								hatPair.InsulationCold
							}));
							foreach (ThingStuffPair current2 in from a in this.aps
							where !ApparelUtility.CanWearTogether(a.thing, hatPair.thing, this.body)
							select a)
							{
								PawnApparelGenerator.debugSb.AppendLine(string.Concat(new object[]
								{
									"    -replaces ",
									current2.ToString(),
									" InsulationCold=",
									current2.InsulationCold
								}));
							}
						}
						this.aps.RemoveAll((ThingStuffPair pa) => !ApparelUtility.CanWearTogether(pa.thing, hatPair.thing, this.body));
						this.aps.Add(hatPair);
					}
				}
				if (DebugViewSettings.logApparelGeneration)
				{
					PawnApparelGenerator.debugSb.AppendLine("New TotalInsulationCold: " + this.TotalInsulationCold);
				}
			}

			public void GiveToPawn(Pawn pawn)
			{
				for (int i = 0; i < this.aps.Count; i++)
				{
					Apparel apparel = (Apparel)ThingMaker.MakeThing(this.aps[i].thing, this.aps[i].stuff);
					PawnGenerator.PostProcessGeneratedGear(apparel, pawn);
					if (ApparelUtility.HasPartsToWear(pawn, apparel.def))
					{
						pawn.apparel.Wear(apparel, false);
					}
				}
				for (int j = 0; j < this.aps.Count; j++)
				{
					for (int k = 0; k < this.aps.Count; k++)
					{
						if (j != k && !ApparelUtility.CanWearTogether(this.aps[j].thing, this.aps[k].thing, pawn.RaceProps.body))
						{
							Log.Error(string.Concat(new object[]
							{
								pawn,
								" generated with apparel that cannot be worn together: ",
								this.aps[j],
								", ",
								this.aps[k]
							}));
							return;
						}
					}
				}
			}

			public override string ToString()
			{
				string str = "[";
				for (int i = 0; i < this.aps.Count; i++)
				{
					str = str + this.aps[i].ToString() + ", ";
				}
				return str + "]";
			}
		}

		private static List<ThingStuffPair> allApparelPairs;

		private static float freeWarmParkaMaxPrice;

		private static float freeWarmHatMaxPrice;

		private static PawnApparelGenerator.PossibleApparelSet workingSet;

		private static List<ThingStuffPair> usableApparel;

		private static StringBuilder debugSb;

		static PawnApparelGenerator()
		{
			PawnApparelGenerator.allApparelPairs = new List<ThingStuffPair>();
			PawnApparelGenerator.workingSet = new PawnApparelGenerator.PossibleApparelSet();
			PawnApparelGenerator.usableApparel = new List<ThingStuffPair>();
			PawnApparelGenerator.debugSb = null;
			PawnApparelGenerator.Reset();
		}

		public static void Reset()
		{
			PawnApparelGenerator.allApparelPairs = ThingStuffPair.AllWith((ThingDef td) => td.IsApparel);
			PawnApparelGenerator.freeWarmParkaMaxPrice = (float)((int)(StatDefOf.MarketValue.Worker.GetValueAbstract(ThingDefOf.Apparel_Parka, ThingDefOf.Cloth) * 1.3f));
			PawnApparelGenerator.freeWarmHatMaxPrice = (float)((int)(StatDefOf.MarketValue.Worker.GetValueAbstract(ThingDefOf.Apparel_Tuque, ThingDefOf.Cloth) * 1.3f));
		}

		public static void GenerateStartingApparelFor(Pawn pawn, PawnGenerationRequest request)
		{
			if (!pawn.RaceProps.ToolUser || !pawn.RaceProps.IsFlesh)
			{
				return;
			}
			if (pawn.Faction == null)
			{
				Log.Error("Cannot generate apparel for faction-less pawn " + pawn);
				return;
			}
			pawn.apparel.DestroyAll(DestroyMode.Vanish);
			float randomInRange = pawn.kindDef.apparelMoney.RandomInRange;
			float mapTemperature;
			NeededWarmth neededWarmth = PawnApparelGenerator.ApparelWarmthNeededNow(pawn, request, out mapTemperature);
			bool flag = Rand.Value < pawn.kindDef.apparelAllowHeadwearChance;
			PawnApparelGenerator.debugSb = null;
			if (DebugViewSettings.logApparelGeneration)
			{
				PawnApparelGenerator.debugSb = new StringBuilder();
				PawnApparelGenerator.debugSb.AppendLine("Generating apparel for " + pawn);
				PawnApparelGenerator.debugSb.AppendLine("Money: " + randomInRange.ToString("F0"));
				PawnApparelGenerator.debugSb.AppendLine("Needed warmth: " + neededWarmth);
				PawnApparelGenerator.debugSb.AppendLine("Headwear allowed: " + flag);
			}
			if (randomInRange >= 0.001f)
			{
				int num = 0;
				while (true)
				{
					PawnApparelGenerator.GenerateWorkingPossibleApparelSetFor(pawn, randomInRange, flag);
					if (DebugViewSettings.logApparelGeneration)
					{
						PawnApparelGenerator.debugSb.Append(num.ToString().PadRight(5) + "Trying: " + PawnApparelGenerator.workingSet.ToString());
					}
					if (num >= 10 || Rand.Value >= 0.85f)
					{
						goto IL_1FA;
					}
					float num2 = Rand.Range(0.45f, 0.8f);
					float totalPrice = PawnApparelGenerator.workingSet.TotalPrice;
					if (totalPrice >= randomInRange * num2)
					{
						goto IL_1FA;
					}
					if (DebugViewSettings.logApparelGeneration)
					{
						PawnApparelGenerator.debugSb.AppendLine(string.Concat(new string[]
						{
							" -- Failed: Spent $",
							totalPrice.ToString("F0"),
							", < ",
							(num2 * 100f).ToString("F0"),
							"% of money."
						}));
					}
					IL_36D:
					num++;
					continue;
					IL_1FA:
					if (num < 20 && Rand.Value < 0.97f && !PawnApparelGenerator.workingSet.Covers(BodyPartGroupDefOf.Torso))
					{
						if (DebugViewSettings.logApparelGeneration)
						{
							PawnApparelGenerator.debugSb.AppendLine(" -- Failed: Does not cover torso.");
						}
						goto IL_36D;
					}
					if (num < 30 && Rand.Value < 0.8f && PawnApparelGenerator.workingSet.CoatButNoShirt())
					{
						if (DebugViewSettings.logApparelGeneration)
						{
							PawnApparelGenerator.debugSb.AppendLine(" -- Failed: Coat but no shirt.");
						}
						goto IL_36D;
					}
					if (num < 50)
					{
						bool mustBeSafe = num < 17;
						if (!PawnApparelGenerator.workingSet.SatisfiesNeededWarmth(neededWarmth, mustBeSafe, mapTemperature))
						{
							if (DebugViewSettings.logApparelGeneration)
							{
								PawnApparelGenerator.debugSb.AppendLine(" -- Failed: Wrong warmth.");
							}
							goto IL_36D;
						}
					}
					if (num < 80 && PawnApparelGenerator.workingSet.IsNaked(pawn.gender))
					{
						if (DebugViewSettings.logApparelGeneration)
						{
							PawnApparelGenerator.debugSb.AppendLine(" -- Failed: Naked.");
						}
						goto IL_36D;
					}
					break;
				}
				if (DebugViewSettings.logApparelGeneration)
				{
					PawnApparelGenerator.debugSb.Append(string.Concat(new object[]
					{
						" -- Approved! Total price: $",
						PawnApparelGenerator.workingSet.TotalPrice.ToString("F0"),
						", TotalInsulationCold: ",
						PawnApparelGenerator.workingSet.TotalInsulationCold
					}));
				}
			}
			if ((!pawn.kindDef.apparelIgnoreSeasons || request.ForceAddFreeWarmLayerIfNeeded) && !PawnApparelGenerator.workingSet.SatisfiesNeededWarmth(neededWarmth, false, 21f))
			{
				PawnApparelGenerator.workingSet.AddFreeWarmthAsNeeded(neededWarmth);
			}
			if (DebugViewSettings.logApparelGeneration)
			{
				Log.Message(PawnApparelGenerator.debugSb.ToString());
			}
			PawnApparelGenerator.workingSet.GiveToPawn(pawn);
			PawnApparelGenerator.workingSet.Reset(null, null);
		}

		private static void GenerateWorkingPossibleApparelSetFor(Pawn pawn, float money, bool headwearAllowed)
		{
			PawnApparelGenerator.workingSet.Reset(pawn.RaceProps.body, pawn.def);
			List<ThingDef> reqApparel = pawn.kindDef.apparelRequired;
			if (reqApparel != null)
			{
				int i;
				for (i = 0; i < reqApparel.Count; i++)
				{
					ThingStuffPair pair = (from pa in PawnApparelGenerator.allApparelPairs
					where pa.thing == reqApparel[i]
					select pa).RandomElementByWeight((ThingStuffPair pa) => pa.Commonality);
					PawnApparelGenerator.workingSet.Add(pair);
					money -= pair.Price;
				}
			}
			int specialSeed = Rand.Int;
			while (Rand.Value >= 0.1f)
			{
				Predicate<ThingStuffPair> predicate = delegate(ThingStuffPair pa)
				{
					if (pa.Price > money)
					{
						return false;
					}
					if (!headwearAllowed && PawnApparelGenerator.IsHeadwear(pa.thing))
					{
						return false;
					}
					if (pa.stuff != null && !pawn.Faction.def.CanUseStuffForApparel(pa.stuff))
					{
						return false;
					}
					if (PawnApparelGenerator.workingSet.PairOverlapsAnything(pa))
					{
						return false;
					}
					if (!pawn.kindDef.apparelTags.NullOrEmpty<string>())
					{
						bool flag2 = false;
						for (int i = 0; i < pawn.kindDef.apparelTags.Count; i++)
						{
							for (int k = 0; k < pa.thing.apparel.tags.Count; k++)
							{
								if (pawn.kindDef.apparelTags[i] == pa.thing.apparel.tags[k])
								{
									flag2 = true;
									break;
								}
							}
							if (flag2)
							{
								break;
							}
						}
						if (!flag2)
						{
							return false;
						}
					}
					return pa.thing.generateAllowChance >= 1f || Rand.ValueSeeded(specialSeed ^ (int)pa.thing.index ^ 64128343) <= pa.thing.generateAllowChance;
				};
				for (int j = 0; j < PawnApparelGenerator.allApparelPairs.Count; j++)
				{
					if (predicate(PawnApparelGenerator.allApparelPairs[j]))
					{
						PawnApparelGenerator.usableApparel.Add(PawnApparelGenerator.allApparelPairs[j]);
					}
				}
				ThingStuffPair pair2;
				bool flag = PawnApparelGenerator.usableApparel.TryRandomElementByWeight((ThingStuffPair pa) => pa.Commonality, out pair2);
				PawnApparelGenerator.usableApparel.Clear();
				if (!flag)
				{
					return;
				}
				PawnApparelGenerator.workingSet.Add(pair2);
				money -= pair2.Price;
			}
		}

		private static bool IsHeadwear(ThingDef td)
		{
			return td.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead) || td.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead);
		}

		private static NeededWarmth ApparelWarmthNeededNow(Pawn pawn, PawnGenerationRequest request, out float mapTemperature)
		{
			int tile = request.Tile;
			if (tile == -1)
			{
				Map anyPlayerHomeMap = Find.AnyPlayerHomeMap;
				if (anyPlayerHomeMap != null)
				{
					tile = anyPlayerHomeMap.Tile;
				}
			}
			if (tile == -1)
			{
				mapTemperature = 21f;
				return NeededWarmth.Any;
			}
			NeededWarmth neededWarmth = NeededWarmth.Any;
			Twelfth twelfth = GenLocalDate.Twelfth(tile);
			mapTemperature = GenTemperature.AverageTemperatureAtTileForTwelfth(tile, twelfth);
			for (int i = 0; i < 2; i++)
			{
				NeededWarmth neededWarmth2 = PawnApparelGenerator.CalculateNeededWarmth(pawn, tile, twelfth);
				if (neededWarmth2 != NeededWarmth.Any)
				{
					neededWarmth = neededWarmth2;
					break;
				}
				twelfth = twelfth.NextTwelfth();
			}
			if (!pawn.kindDef.apparelIgnoreSeasons)
			{
				return neededWarmth;
			}
			if (request.ForceAddFreeWarmLayerIfNeeded && neededWarmth == NeededWarmth.Warm)
			{
				return neededWarmth;
			}
			return NeededWarmth.Any;
		}

		public static NeededWarmth CalculateNeededWarmth(Pawn pawn, int tile, Twelfth twelfth)
		{
			float num = GenTemperature.AverageTemperatureAtTileForTwelfth(tile, twelfth);
			if (num < pawn.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin, null) - 4f)
			{
				return NeededWarmth.Warm;
			}
			if (num > pawn.def.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin, null) + 4f)
			{
				return NeededWarmth.Cool;
			}
			return NeededWarmth.Any;
		}

		internal static void MakeTableApparelPairs()
		{
			IEnumerable<ThingStuffPair> arg_129_0 = from p in PawnApparelGenerator.allApparelPairs
			orderby p.thing.defName descending
			select p;
			TableDataGetter<ThingStuffPair>[] expr_2D = new TableDataGetter<ThingStuffPair>[6];
			expr_2D[0] = new TableDataGetter<ThingStuffPair>("thing", (ThingStuffPair p) => p.thing.defName);
			expr_2D[1] = new TableDataGetter<ThingStuffPair>("stuff", (ThingStuffPair p) => (p.stuff == null) ? string.Empty : p.stuff.defName);
			expr_2D[2] = new TableDataGetter<ThingStuffPair>("price", (ThingStuffPair p) => p.Price.ToString());
			expr_2D[3] = new TableDataGetter<ThingStuffPair>("commonality", (ThingStuffPair p) => (p.Commonality * 100f).ToString("F4"));
			expr_2D[4] = new TableDataGetter<ThingStuffPair>("def-commonality", (ThingStuffPair p) => p.thing.generateCommonality.ToString("F4"));
			expr_2D[5] = new TableDataGetter<ThingStuffPair>("insulationCold", (ThingStuffPair p) => (p.InsulationCold != 0f) ? p.InsulationCold.ToString() : string.Empty);
			DebugTables.MakeTablesDialog<ThingStuffPair>(arg_129_0, expr_2D);
		}

		public static void MakeTableApparelPairsByThing()
		{
			PawnApparelGenerator.MakeTablePairsByThing(PawnApparelGenerator.allApparelPairs);
		}

		internal static void LogHeadwearApparelPairs()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Listing all entries in allApparelPairs of headwear");
			foreach (ThingStuffPair current in from pa in PawnApparelGenerator.allApparelPairs
			where PawnApparelGenerator.IsHeadwear(pa.thing)
			orderby pa.thing.defName
			select pa)
			{
				stringBuilder.AppendLine(current + "  - " + current.commonalityMultiplier);
			}
			Log.Message(stringBuilder.ToString());
		}

		public static void MakeTablePairsByThing(List<ThingStuffPair> pairList)
		{
			DefMap<ThingDef, float> totalCommMult = new DefMap<ThingDef, float>();
			DefMap<ThingDef, float> totalComm = new DefMap<ThingDef, float>();
			DefMap<ThingDef, int> pairCount = new DefMap<ThingDef, int>();
			foreach (ThingStuffPair current in pairList)
			{
				DefMap<ThingDef, float> defMap;
				ThingDef thing;
				(defMap = totalCommMult)[thing = current.thing] = defMap[thing] + current.commonalityMultiplier;
				ThingDef thing2;
				(defMap = totalComm)[thing2 = current.thing] = defMap[thing2] + current.Commonality;
				DefMap<ThingDef, int> pairCount2;
				ThingDef thing3;
				(pairCount2 = pairCount)[thing3 = current.thing] = pairCount2[thing3] + 1;
			}
			IEnumerable<ThingDef> arg_192_0 = from d in DefDatabase<ThingDef>.AllDefs
			where pairList.Any((ThingStuffPair pa) => pa.thing == d)
			select d;
			TableDataGetter<ThingDef>[] expr_F3 = new TableDataGetter<ThingDef>[5];
			expr_F3[0] = new TableDataGetter<ThingDef>("thing", (ThingDef t) => t.defName);
			expr_F3[1] = new TableDataGetter<ThingDef>("pair count", (ThingDef t) => pairCount[t].ToString());
			expr_F3[2] = new TableDataGetter<ThingDef>("total commonality multiplier ", (ThingDef t) => totalCommMult[t].ToString("F4"));
			expr_F3[3] = new TableDataGetter<ThingDef>("total commonality", (ThingDef t) => totalComm[t].ToString("F4"));
			expr_F3[4] = new TableDataGetter<ThingDef>("def-commonality", (ThingDef t) => t.generateCommonality.ToString("F4"));
			DebugTables.MakeTablesDialog<ThingDef>(arg_192_0, expr_F3);
		}
	}
}
