using System;

namespace Verse
{
	public enum ThingRequestGroup : byte
	{
		Undefined,
		Nothing,
		Everything,
		HaulableEver,
		HaulableAlways,
		FoodSource,
		FoodSourceNotPlantOrTree,
		Corpse,
		Blueprint,
		BuildingArtificial,
		BuildingFrame,
		Pawn,
		PotentialBillGiver,
		Medicine,
		Filth,
		AttackTarget,
		Weapon,
		Refuelable,
		HaulableEverOrMinifiable,
		Drug,
		Shell,
		HarvestablePlant,
		Fire,
		Plant,
		Construction,
		HasGUIOverlay,
		Apparel,
		MinifiedThing,
		Grave,
		Art,
		ThingHolder,
		ActiveDropPod,
		Transporter,
		LongRangeMineralScanner,
		AffectsSky,
		PsychicDroneEmanator,
		WindSource,
		AlwaysFlee
	}
}
