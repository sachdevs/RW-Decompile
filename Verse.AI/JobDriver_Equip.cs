using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse.Sound;

namespace Verse.AI
{
	public class JobDriver_Equip : JobDriver
	{
		public override bool TryMakePreToilReservations()
		{
			return this.pawn.Reserve(this.job.targetA, this.job, 1, -1, null);
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
			yield return new Toil
			{
				initAction = delegate
				{
					ThingWithComps thingWithComps = (ThingWithComps)this.$this.job.targetA.Thing;
					ThingWithComps thingWithComps2;
					if (thingWithComps.def.stackLimit > 1 && thingWithComps.stackCount > 1)
					{
						thingWithComps2 = (ThingWithComps)thingWithComps.SplitOff(1);
					}
					else
					{
						thingWithComps2 = thingWithComps;
						thingWithComps2.DeSpawn();
					}
					this.$this.pawn.equipment.MakeRoomFor(thingWithComps2);
					this.$this.pawn.equipment.AddEquipment(thingWithComps2);
					if (thingWithComps.def.soundInteract != null)
					{
						thingWithComps.def.soundInteract.PlayOneShot(new TargetInfo(this.$this.pawn.Position, this.$this.pawn.Map, false));
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}
