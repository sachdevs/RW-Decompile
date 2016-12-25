using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_PlantsCut : WorkGiver_Scanner
	{
		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}

		[DebuggerHidden]
		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			List<Designation> desList = Find.DesignationManager.allDesignations;
			for (int i = 0; i < desList.Count; i++)
			{
				Designation des = desList[i];
				if (des.def == DesignationDefOf.CutPlant || des.def == DesignationDefOf.HarvestPlant)
				{
					yield return des.target.Thing;
				}
			}
		}

		public override Job JobOnThing(Pawn pawn, Thing t)
		{
			if (t.def.category != ThingCategory.Plant)
			{
				return null;
			}
			if (!pawn.CanReserve(t, 1))
			{
				return null;
			}
			if (t.IsForbidden(pawn))
			{
				return null;
			}
			foreach (Designation current in Find.DesignationManager.AllDesignationsOn(t))
			{
				if (current.def == DesignationDefOf.HarvestPlant)
				{
					Job result;
					if (current.def == DesignationDefOf.HarvestPlant && !((Plant)t).HarvestableNow)
					{
						result = null;
						return result;
					}
					result = new Job(JobDefOf.Harvest, t);
					return result;
				}
				else if (current.def == DesignationDefOf.CutPlant)
				{
					Job result = new Job(JobDefOf.CutPlant, t);
					return result;
				}
			}
			return null;
		}
	}
}