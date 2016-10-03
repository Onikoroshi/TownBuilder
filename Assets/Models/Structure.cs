using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Structure
{
	public enum Type { House, CityCenter }

	public List<Tile> foundation;
	public Tile driveway;
	public Road entrance;
	public Direction facing;
	public Type type;

	public Int2 lowerLeft, upperRight;

	public Structure(Road connectedTo, Direction directionFromRoad, Type givenType, Int2 givenLowerLeft, Int2 givenUpperRight)
	{
//		Debug.LogError("adding structure to the " + directionFromRoad.ToString() + " of type " + givenType.ToString() + " from lower left " + givenLowerLeft.ToString() + " to upper right " + givenUpperRight.ToString());
		entrance = connectedTo;
		driveway = connectedTo.foundation.getDirection(directionFromRoad);
		facing = Directions.oppositeOf(directionFromRoad);

		type = givenType;
		lowerLeft = givenLowerLeft;
		upperRight = givenUpperRight;

		foundation = new List<Tile>();
		for ( int x = givenLowerLeft.x; x <= givenUpperRight.x; x++ )
		{
			for ( int y = givenLowerLeft.y; y <= givenUpperRight.y; y++ )
			{
				Tile current = getWorld().GetTileAt(x, y);
				if (!current.IsNone())
				{
					current.Building = this;
					current.State = Tile.StateType.Occupied;
					foundation.Add(current);
				}
			}
		}
	}

	private World getWorld()
	{
		return (driveway != null && !driveway.IsNone()) ? driveway.world : null;
	}
}
