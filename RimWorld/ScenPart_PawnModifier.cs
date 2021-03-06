using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class ScenPart_PawnModifier : ScenPart
	{
		protected float chance = 1f;

		protected PawnGenerationContext context;

		private bool hideOffMap;

		private string chanceBuf;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<float>(ref this.chance, "chance", 0f, false);
			Scribe_Values.Look<PawnGenerationContext>(ref this.context, "context", PawnGenerationContext.All, false);
			Scribe_Values.Look<bool>(ref this.hideOffMap, "hideOffMap", false, false);
		}

		protected void DoPawnModifierEditInterface(Rect rect)
		{
			Rect rect2 = rect.TopHalf();
			Rect rect3 = rect2.LeftPart(0.333f).Rounded();
			Rect rect4 = rect2.RightPart(0.666f).Rounded();
			Text.Anchor = TextAnchor.MiddleRight;
			Widgets.Label(rect3, "chance".Translate());
			Text.Anchor = TextAnchor.UpperLeft;
			Widgets.TextFieldPercent(rect4, ref this.chance, ref this.chanceBuf, 0f, 1f);
			Rect rect5 = rect.BottomHalf();
			Rect rect6 = rect5.LeftPart(0.333f).Rounded();
			Rect rect7 = rect5.RightPart(0.666f).Rounded();
			Text.Anchor = TextAnchor.MiddleRight;
			Widgets.Label(rect6, "context".Translate());
			Text.Anchor = TextAnchor.UpperLeft;
			if (Widgets.ButtonText(rect7, this.context.ToStringHuman(), true, false, true))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (PawnGenerationContext localCont2 in Enum.GetValues(typeof(PawnGenerationContext)))
				{
					PawnGenerationContext localCont = localCont2;
					list.Add(new FloatMenuOption(localCont.ToStringHuman(), delegate
					{
						this.context = localCont;
					}, MenuOptionPriority.Default, null, null, 0f, null, null));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
		}

		public override void Randomize()
		{
			this.chance = GenMath.RoundedHundredth(Rand.Range(0.05f, 1f));
			this.context = PawnGenerationContextUtility.GetRandom();
			this.hideOffMap = false;
		}

		public override void Notify_PawnGenerated(Pawn pawn, PawnGenerationContext context)
		{
			if (this.hideOffMap && PawnGenerationContext.PlayerStarter.Includes(context))
			{
				return;
			}
			if (pawn.RaceProps.Humanlike && this.context.Includes(context))
			{
				this.ModifyPawn(pawn);
			}
		}

		public override void PostMapGenerate(Map map)
		{
			if (Find.GameInitData == null)
			{
				return;
			}
			if (this.hideOffMap && (this.context == PawnGenerationContext.PlayerStarter || this.context == PawnGenerationContext.All))
			{
				foreach (Pawn current in Find.GameInitData.startingPawns)
				{
					if (current.RaceProps.Humanlike)
					{
						this.ModifyPawn(current);
					}
				}
			}
		}

		protected abstract void ModifyPawn(Pawn p);
	}
}
