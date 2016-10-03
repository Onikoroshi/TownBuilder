using UnityEngine;
using System.Collections;

public class RoadBuilder : Builder
{
	public RoadBuilder(City givenCity) : base(givenCity)
	{
		
	}

	public bool BuildRoad(Tile onTile)
	{
		if (onTile.IsBuildable() && IsNextToRoad(onTile))
		{
			Road built = new Road(onTile);
			return true;
		}
		else return false;
	}

	private bool IsNextToRoad(Tile target)
	{
		return target.Neighbors().FindIndex(elem => (elem.Surface != null && elem.Surface.type == SurfaceMod.Type.Road)) >= 0;
	}
}
