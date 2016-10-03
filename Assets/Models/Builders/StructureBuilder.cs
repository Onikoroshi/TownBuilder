using UnityEngine;
using System.Collections;

public class StructureBuilder : Builder
{
	public StructureBuilder(City givenCity) : base(givenCity)
	{
		
	}

	public Structure buildStructure(Road connectedTo, Direction directionFromRoad, Structure.Type givenType, LocationPair locals)
	{
		if (locals == null) return null;

		Int2 localLowerLeft = locals.lowerLeft;
		Int2 localUpperRight = locals.upperRight;

		int depth = locals.DistanceInDirection(directionFromRoad);

		//		Debug.LogError("local lower left: " + localLowerLeft.ToString());
		//		Debug.LogError("local upper right: " + localUpperRight.ToString());

		Int2 globalLowerLeft = localLowerLeft;
		Int2 globalUpperRight = localUpperRight;

		switch (directionFromRoad)
		{
			case Direction.East:
				globalLowerLeft = Directions.InDirectionForDistance(localUpperRight, Directions.oppositeOf(directionFromRoad), depth);
				globalUpperRight = Directions.InDirectionForDistance(localLowerLeft, directionFromRoad, depth);
				break;
			case Direction.South:
				globalLowerLeft = localUpperRight;
				globalUpperRight = localLowerLeft;
				break;
			case Direction.West:
				globalUpperRight = Directions.InDirectionForDistance(localUpperRight, Directions.oppositeOf(directionFromRoad), depth);
				globalLowerLeft = Directions.InDirectionForDistance(localLowerLeft, directionFromRoad, depth);
				break;
		}

		//		Debug.LogError("global lower left: " + globalLowerLeft.ToString());
		//		Debug.LogError("global upper right: " + globalUpperRight.ToString());

		Structure built = new Structure(connectedTo, directionFromRoad, givenType, globalLowerLeft, globalUpperRight);
		return built;
	}

	public Structure buildStructure(Road connectedTo, Direction directionFromRoad, Structure.Type givenType, int width, int depth)
	{
//		Debug.LogError("building " + givenType.ToString() + " to the " + directionFromRoad + " of " + connectedTo.foundation.ToS() + " with width " + width + " and depth " + depth);
		Tile targetDriveway = connectedTo.foundation.getDirection(directionFromRoad);
//		Debug.LogError("target driveway: " + targetDriveway.ToS() + " - " + targetDriveway.Loc().ToString());

		LocationPair locals = BoundsForStructure(targetDriveway, directionFromRoad, width, depth);

		return buildStructure(connectedTo, directionFromRoad, givenType, locals);
	}

	public static LocationPair BoundsForStructure(Tile start, Direction directionFromRoad, int width, int depth)
	{
		if (start.IsUnBuildable()) return null;

		Tile current = start;
		int freeLeft = 0;
		for ( freeLeft = 0; freeLeft < width; freeLeft++ )
		{
			if (current.IsUnBuildable()) break;

			int freeDepth = 0;
			Tile back = current;
			for (freeDepth = 0; freeDepth < depth; freeDepth++)
			{
				//				Debug.LogError("is " + back.ToS() + " unbuildable? " + back.IsUnBuildable());
				if (back.IsUnBuildable()) break;
				back = back.getDirection(directionFromRoad);
			}
			if (freeDepth < depth) break;

			current = current.getDirection(Directions.leftOf(directionFromRoad));
		}
		//		Debug.LogError("free " + freeLeft + " spaces to the left (" + Directions.leftOf(directionFromRoad) + ")");

		current = start;
		int freeRight = 0;
		for ( freeRight = 0; freeRight < width; freeRight++ )
		{
			if (current.IsUnBuildable()) break;

			int freeDepth = 0;
			Tile back = current;
			for (freeDepth = 0; freeDepth < depth; freeDepth++)
			{
				if (back.IsUnBuildable()) break;
				back = back.getDirection(directionFromRoad);
			}
			if (freeDepth < depth) break;

			current = current.getDirection(Directions.rightOf(directionFromRoad));
		}
		//		Debug.LogError("free " + freeRight + " spaces to the right (" + Directions.rightOf(directionFromRoad) + ")");

		int freeWidth = freeLeft + freeRight;
		//		Debug.LogError("free width " + freeWidth + " compared to needed width: " + width);
		if ((freeLeft + freeRight) <= width) return null;

		//		Debug.LogError(freeLeft + " free spaces to the left and " + freeRight + " free spaces to the right");

		int targetLeft = 0;
		int targetRight = 0;
		if (freeLeft < freeRight)
		{
			targetLeft = freeLeft - 1;
			targetRight = width - freeLeft;
		}
		else if (freeLeft > freeRight)
		{
			targetRight = freeRight - 1;
			targetLeft = width - freeRight;
		}
		else
		{
			targetLeft = freeLeft - 1;
			targetRight = width - freeLeft;
		}
		//		int targetLeft = (freeLeft < freeRight) ? (freeLeft-1) : (width - freeRight);
		//		int targetRight = (freeRight < freeLeft) ? (freeRight-1) : (width - freeLeft);

		//		Debug.LogError("looking " + targetLeft + " to the left and " + targetRight + " to the right");

		Int2 localLowerLeft = Directions.InDirectionForDistance(start.Loc(), Directions.leftOf(directionFromRoad), targetLeft);
		Int2 localUpperRight = Directions.InDirectionForDistance(start.Loc(), Directions.rightOf(directionFromRoad), targetRight);
		localUpperRight = Directions.InDirectionForDistance(localUpperRight, directionFromRoad, depth-1);

		return new LocationPair(localLowerLeft, localUpperRight, directionFromRoad);
	}
}
