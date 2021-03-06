using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public static class CharacterCardUtility
	{
		public const int MainRectsY = 100;

		private const float MainRectsHeight = 450f;

		private const int ConfigRectTitlesHeight = 40;

		public static Vector2 PawnCardSize = new Vector2(570f, 470f);

		private const int MaxNameLength = 12;

		private const int MaxNickLength = 9;

		private static Regex validNameRegex = new Regex("^[a-zA-Z0-9 '\\-]*$");

		public static void DrawCharacterCard(Rect rect, Pawn pawn, Action randomizeCallback = null, Rect creationRect = default(Rect))
		{
			bool flag = randomizeCallback != null;
			GUI.BeginGroup((!flag) ? rect : creationRect);
			Rect rect2 = new Rect(0f, 0f, 300f, 30f);
			NameTriple nameTriple = pawn.Name as NameTriple;
			if (flag && nameTriple != null)
			{
				Rect rect3 = new Rect(rect2);
				rect3.width *= 0.333f;
				Rect rect4 = new Rect(rect2);
				rect4.width *= 0.333f;
				rect4.x += rect4.width;
				Rect rect5 = new Rect(rect2);
				rect5.width *= 0.333f;
				rect5.x += rect4.width * 2f;
				string first = nameTriple.First;
				string nick = nameTriple.Nick;
				string last = nameTriple.Last;
				CharacterCardUtility.DoNameInputRect(rect3, ref first, 12);
				if (nameTriple.Nick == nameTriple.First || nameTriple.Nick == nameTriple.Last)
				{
					GUI.color = new Color(1f, 1f, 1f, 0.5f);
				}
				CharacterCardUtility.DoNameInputRect(rect4, ref nick, 9);
				GUI.color = Color.white;
				CharacterCardUtility.DoNameInputRect(rect5, ref last, 12);
				if (nameTriple.First != first || nameTriple.Nick != nick || nameTriple.Last != last)
				{
					pawn.Name = new NameTriple(first, nick, last);
				}
				TooltipHandler.TipRegion(rect3, "FirstNameDesc".Translate());
				TooltipHandler.TipRegion(rect4, "ShortIdentifierDesc".Translate());
				TooltipHandler.TipRegion(rect5, "LastNameDesc".Translate());
			}
			else
			{
				rect2.width = 999f;
				Text.Font = GameFont.Medium;
				Widgets.Label(rect2, pawn.Name.ToStringFull);
				Text.Font = GameFont.Small;
			}
			if (randomizeCallback != null)
			{
				Rect rect6 = new Rect(creationRect.width - 24f - 100f, 0f, 100f, rect2.height);
				if (Widgets.ButtonText(rect6, "Randomize".Translate(), true, false, true))
				{
					SoundDefOf.TickTiny.PlayOneShotOnCamera(null);
					randomizeCallback();
				}
				UIHighlighter.HighlightOpportunity(rect6, "RandomizePawn");
			}
			if (flag)
			{
				Widgets.InfoCardButton(creationRect.width - 24f, 0f, pawn);
			}
			else if (!pawn.health.Dead)
			{
				float num = CharacterCardUtility.PawnCardSize.x - 85f;
				if ((pawn.Faction == Faction.OfPlayer || pawn.IsPrisonerOfColony) && pawn.Spawned)
				{
					Rect rect7 = new Rect(num, 0f, 30f, 30f);
					TooltipHandler.TipRegion(rect7, PawnBanishUtility.GetBanishButtonTip(pawn));
					if (Widgets.ButtonImage(rect7, TexButton.Banish))
					{
						if (pawn.Downed)
						{
							Messages.Message("MessageCantBanishDownedPawn".Translate(new object[]
							{
								pawn.LabelShort
							}).AdjustedFor(pawn), pawn, MessageTypeDefOf.RejectInput);
						}
						else
						{
							PawnBanishUtility.ShowBanishPawnConfirmationDialog(pawn);
						}
					}
					num -= 40f;
				}
				if (pawn.IsColonist)
				{
					Rect rect8 = new Rect(num, 0f, 30f, 30f);
					TooltipHandler.TipRegion(rect8, "RenameColonist".Translate());
					if (Widgets.ButtonImage(rect8, TexButton.Rename))
					{
						Find.WindowStack.Add(new Dialog_ChangeNameTriple(pawn));
					}
					num -= 40f;
				}
			}
			string label = pawn.MainDesc(true);
			Rect rect9 = new Rect(0f, 45f, rect.width, 60f);
			Widgets.Label(rect9, label);
			TooltipHandler.TipRegion(rect9, () => pawn.ageTracker.AgeTooltipString, 6873641);
			Rect position = new Rect(0f, 100f, 250f, 450f);
			Rect position2 = new Rect(position.xMax, 100f, 258f, 450f);
			GUI.BeginGroup(position);
			float num2 = 0f;
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(0f, 0f, 200f, 30f), "Backstory".Translate());
			num2 += 30f;
			Text.Font = GameFont.Small;
			foreach (BackstorySlot backstorySlot in Enum.GetValues(typeof(BackstorySlot)))
			{
				Backstory backstory = pawn.story.GetBackstory(backstorySlot);
				if (backstory != null)
				{
					Rect rect10 = new Rect(0f, num2, position.width, 24f);
					if (Mouse.IsOver(rect10))
					{
						Widgets.DrawHighlight(rect10);
					}
					TooltipHandler.TipRegion(rect10, backstory.FullDescriptionFor(pawn));
					Text.Anchor = TextAnchor.MiddleLeft;
					string str = (backstorySlot != BackstorySlot.Adulthood) ? "Childhood".Translate() : "Adulthood".Translate();
					Widgets.Label(rect10, str + ":");
					Text.Anchor = TextAnchor.UpperLeft;
					Rect rect11 = new Rect(rect10);
					rect11.x += 90f;
					rect11.width -= 90f;
					string title = backstory.Title;
					Widgets.Label(rect11, title);
					num2 += rect10.height + 2f;
				}
			}
			num2 += 25f;
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(0f, num2, 200f, 30f), "IncapableOf".Translate());
			num2 += 30f;
			Text.Font = GameFont.Small;
			StringBuilder stringBuilder = new StringBuilder();
			WorkTags combinedDisabledWorkTags = pawn.story.CombinedDisabledWorkTags;
			if (combinedDisabledWorkTags == WorkTags.None)
			{
				stringBuilder.Append("(" + "NoneLower".Translate() + "), ");
			}
			else
			{
				List<WorkTags> list = CharacterCardUtility.WorkTagsFrom(combinedDisabledWorkTags).ToList<WorkTags>();
				bool flag2 = true;
				foreach (WorkTags current in list)
				{
					if (flag2)
					{
						stringBuilder.Append(current.LabelTranslated().CapitalizeFirst());
					}
					else
					{
						stringBuilder.Append(current.LabelTranslated());
					}
					stringBuilder.Append(", ");
					flag2 = false;
				}
			}
			string text = stringBuilder.ToString();
			text = text.Substring(0, text.Length - 2);
			Rect rect12 = new Rect(0f, num2, position.width, 999f);
			Widgets.Label(rect12, text);
			num2 += 100f;
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(0f, num2, 200f, 30f), "Traits".Translate());
			num2 += 30f;
			Text.Font = GameFont.Small;
			for (int i = 0; i < pawn.story.traits.allTraits.Count; i++)
			{
				Trait trait = pawn.story.traits.allTraits[i];
				Rect rect13 = new Rect(0f, num2, position.width, 24f);
				if (Mouse.IsOver(rect13))
				{
					Widgets.DrawHighlight(rect13);
				}
				Widgets.Label(rect13, trait.LabelCap);
				num2 += rect13.height + 2f;
				Trait trLocal = trait;
				TipSignal tip = new TipSignal(() => trLocal.TipString(pawn), (int)num2 * 37);
				TooltipHandler.TipRegion(rect13, tip);
			}
			GUI.EndGroup();
			GUI.BeginGroup(position2);
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(0f, 0f, 200f, 30f), "Skills".Translate());
			SkillUI.SkillDrawMode mode;
			if (Current.ProgramState == ProgramState.Playing)
			{
				mode = SkillUI.SkillDrawMode.Gameplay;
			}
			else
			{
				mode = SkillUI.SkillDrawMode.Menu;
			}
			SkillUI.DrawSkillsOf(pawn, new Vector2(0f, 35f), mode);
			GUI.EndGroup();
			GUI.EndGroup();
		}

		public static void DoNameInputRect(Rect rect, ref string name, int maxLength)
		{
			string text = Widgets.TextField(rect, name);
			if (text.Length <= maxLength && CharacterCardUtility.validNameRegex.IsMatch(text))
			{
				name = text;
			}
		}

		[DebuggerHidden]
		private static IEnumerable<WorkTags> WorkTagsFrom(WorkTags tags)
		{
			foreach (WorkTags workTag in tags.GetAllSelectedItems<WorkTags>())
			{
				if (workTag != WorkTags.None)
				{
					yield return workTag;
				}
			}
		}
	}
}
