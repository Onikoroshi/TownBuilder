using UnityEngine;
using System;
using System.Collections;
using Random = UnityEngine.Random;
using System.Collections.Generic;

public class Int2
{
	public int x, y;

	public Int2(int gx, int gy)
	{
		x = gx;
		y = gy;
	}

	public override string ToString()
	{
		return x + "," + y;
	}

	public int CompareTo(Int2 given)
	{
		return x.CompareTo(given.x) + y.CompareTo(given.y);
	}
}

public class LocationPair
{
	public Int2 lowerLeft, upperRight;
	public Direction toHere;

	public LocationPair(Int2 givenLL, Int2 givenUR, Direction givenDir)
	{
		lowerLeft = givenLL;
		upperRight = givenUR;
		toHere = givenDir;
	}

	public int DistanceInDirection()
	{
		return DistanceInDirection(toHere);
	}

	public int DistanceInDirection(Direction dir)
	{
		if (lowerLeft == null || upperRight == null) return -1;

		switch (dir)
		{
			case Direction.East:
				return Math.Abs(upperRight.x - lowerLeft.x);
			case Direction.North:
				return Math.Abs(upperRight.y - lowerLeft.y);
			case Direction.South:
				return Math.Abs(lowerLeft.y - upperRight.y);
			case Direction.West:
				return Math.Abs(lowerLeft.x - upperRight.x);
		}

		return -1;
	}

	public int SpaceInDirection(Direction dir)
	{
		int dist = DistanceInDirection(dir);
		return (dist < 0) ? dist : dist + 1;
	}

	public override string ToString()
	{
		string left = "";
		string right = "";
		string middle = "";

		if (lowerLeft != null) left = lowerLeft.ToString();

		if (upperRight != null) right = upperRight.ToString();

		if (lowerLeft != null && upperRight != null) middle = " - ";

		return left + middle + right;
	}
}

public enum Direction { North, South, East, West, None }
public class Directions
{
	public const int NUM_VALID_DIRECTIONS = 4;

	public static Direction[] ValidDirections()
	{
		return new Direction[NUM_VALID_DIRECTIONS] {
			Direction.North,
			Direction.East,
			Direction.South,
			Direction.West
		};
	}

	public static List<Direction> ValidDirectionList()
	{
		List<Direction> results = new List<Direction>();
		foreach (Direction dir in ValidDirections())
		{
			results.Add(dir);
		}
		return results;
	}

	public static Direction RandomDirection()
	{
		return ValidDirections()[Random.Range(0, NUM_VALID_DIRECTIONS)];
	}

	public static Direction RandomDirection(List<Direction> toChoose)
	{
		return toChoose[Random.Range(0, toChoose.Count)];
	}

	public static Direction oppositeOf(Direction given)
	{
		switch (given)
		{
			case Direction.East:
				return Direction.West;
			case Direction.North:
				return Direction.South;
			case Direction.South:
				return Direction.North;
			case Direction.West:
				return Direction.East;
		}

		Debug.LogError("invalid direction given.");
		return Direction.None;
	}

	public static Direction rightOf(Direction given)
	{
		switch (given)
		{
			case Direction.East:
				return Direction.South;
			case Direction.North:
				return Direction.East;
			case Direction.South:
				return Direction.West;
			case Direction.West:
				return Direction.North;
		}

		Debug.LogError("invalid direction given.");
		return Direction.None;
	}

	public static Direction leftOf(Direction given)
	{
		switch (given)
		{
			case Direction.East:
				return Direction.North;
			case Direction.North:
				return Direction.West;
			case Direction.South:
				return Direction.East;
			case Direction.West:
				return Direction.South;
		}

		Debug.LogError("invalid direction given.");
		return Direction.None;
	}

	public static Int2 InDirectionForDistance(Int2 start, Direction dir, int dist)
	{
		switch (dir)
		{
			case Direction.East:
				return new Int2(start.x + dist, start.y);
			case Direction.North:
				return new Int2(start.x, start.y + dist);
			case Direction.South:
				return new Int2(start.x, start.y - dist);
			case Direction.West:
				return new Int2(start.x - dist, start.y);
		}

		Debug.LogError("invalid direction given.");
		return start;
	}

	public static Int2 InDirection(Int2 start, Direction dir)
	{
		return InDirectionForDistance(start, dir, 1);
	}
}