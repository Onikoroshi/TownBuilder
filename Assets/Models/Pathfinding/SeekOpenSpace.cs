using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SeekOpenSpace
{
	public World universe;
	public SpaceCounter[,] loveMap;
	public Int2 startingLocation;

	public SeekOpenSpace(World parent, Int2 givenStart)
	{
		universe = parent;
		loveMap = new SpaceCounter[parent.Width, parent.Height];

		SpaceCounter toCount = new SpaceCounter(this, givenStart);
		SetCounterAt(givenStart, toCount);
		startingLocation = givenStart;
//		Debug.LogError("initializing start counter at " + toCount.location.ToString());
//
//		Debug.LogError("counter at " + toCount.location.ToString() + " is null? " + (loveMap[toCount.location.x, toCount.location.y] == null));
//		Debug.LogError("with getter " + (GetCounterAt(toCount.location) == null));

		InitializeLoveMap();
	}

	public SpaceCounter[,] GetLoveMap()
	{
		return (SpaceCounter[,])(loveMap);
	}

	public SpaceCounter GetCounterAt(Int2 location)
	{
		if (location.x >= 0 && location.x < universe.Width && location.y >= 0 && location.y < universe.Height)
			return (SpaceCounter)(loveMap[location.x, location.y]);
		else
			return null;
	}

	public void SetCounterAt(Int2 location, SpaceCounter toSet)
	{
		if (location.x >= 0 && location.x < universe.Width && location.y >= 0 && location.y < universe.Height) loveMap[location.x, location.y] = (SpaceCounter)(toSet);
	}

	public void InitializeLoveMap()
	{
		Debug.LogError("is start null? " + (startingLocation == null));
		if (startingLocation == null) return;

		SetAllDistancesFromLocation(startingLocation, Directions.ValidDirectionList());
	}

	public void SetAllDistancesFromLocation(Int2 location, List<Direction> requestedDirs)
	{
		SpaceCounter begin = GetCounterAt(location);

		foreach (Direction dir in requestedDirs)
		{
			SetAllDistancesInDirection(begin, dir);
		}
	}

	private void SetAllDistancesInDirection(SpaceCounter begin, Direction dir)
	{
		Debug.LogError("is begining for setting distances in " + dir.ToString() + " null? " + (begin == null));
		if (begin == null) return;

		Tile current = universe.GetTileAt(Directions.InDirection(begin.location, dir));
		Stack<SpaceCounter> toScore = new Stack<SpaceCounter>();
		toScore.Push(begin);

		while (current.IsBuildable())
		{
			SpaceCounter currCounter = GetCounterAt(current.Loc());
			if (currCounter == null)
			{
				currCounter = new SpaceCounter(this, current.Loc());
				SetCounterAt(current.Loc(), currCounter);
				Debug.LogError("initializing counter at " + currCounter.location);
			}

			SetDirectDistancesInDirection(currCounter, Directions.leftOf(dir));
			SetDirectDistancesInDirection(currCounter, Directions.rightOf(dir));

			toScore.Push(currCounter);
			current = current.getDirection(dir);
		}

		int scoreSoFar = 0;
		while (toScore.Count > 0)
		{
			SpaceCounter needsScore = toScore.Pop();
			needsScore.SetScoreIn(dir, scoreSoFar);
			needsScore.SetTotalSpaceIn(dir);
			scoreSoFar++;
		}
	}

	private void SetDirectDistancesInDirection(SpaceCounter seeker, Direction dir)
	{
		if (seeker == null) return;

		int score = 0;

		Tile current = universe.GetTileAt(seeker.location).getDirection(dir);
		while (current.IsBuildable())
		{
			score++;
			current = current.getDirection(dir);
		}

		seeker.SetScoreIn(dir, score);
	}

	public void blockLocation(Int2 toBlock)
	{
		SpaceCounter blocked = GetCounterAt(toBlock);

		if (blocked == null) return;
		{
			blocked = new SpaceCounter(this, toBlock);
			SetCounterAt(toBlock, blocked);
			Debug.LogError("initializing counter at " + blocked.location);
		}

		foreach (Direction dir in Directions.ValidDirections())
		{
			Int2 loc = Directions.InDirection(blocked.location, dir);
			SpaceCounter curr = GetCounterAt(loc);
			if (curr == null)
			{
				curr = new SpaceCounter(this, loc);
				SetCounterAt(loc, curr);
			}

			Direction opp = Directions.oppositeOf(dir);
			int saveScore = curr.ScoreIn(opp);
			int saveCost = curr.CostIn(opp);
			curr.SetScoreIn(opp, 0);
			curr.SetCostIn(opp, 0);

			while (curr != null && curr.TotalSpaceIn(opp) > 0)
			{
				curr.SetScoreIn(opp, curr.ScoreIn(opp) - saveScore);
				curr.SetCostIn(opp, curr.CostIn(opp) - saveCost);
				curr = GetCounterAt(Directions.InDirection(curr.location, dir));
			}

			if (blocked.CostIn(dir) <= 0 && universe.GetTileAt(blocked.location).HasRoad() && universe.GetTileAt(Directions.InDirection(blocked.location, opp)).HasRoad()) SetAllDistancesInDirection(curr, dir);
		}
	}
}

public class SpaceCounter
{
	protected SeekOpenSpace parent;
	public Int2 location;
	private int[] dirCosts, dirScores;
	private int score, cost, value;
	private bool totalStale;

	public SpaceCounter(SeekOpenSpace givenParent, Int2 givenLoc)
	{
		parent = givenParent;
		location = givenLoc;

		dirCosts = new int[Directions.NUM_VALID_DIRECTIONS] { 0, 0, 0, 0 };
		dirScores = new int[Directions.NUM_VALID_DIRECTIONS] { 0, 0, 0, 0 };
		score = 0;
		cost = 0;
		value = 0;
		totalStale = false;
	}

	public SeekOpenSpace GetParent()
	{
		return (SeekOpenSpace)(parent);
	}

	public int TotalSpaceIn(Direction dir)
	{
		if (ScoreIn(dir) > 0)
		{
			return CostIn(dir) + ScoreIn(dir) + ScoreIn(Directions.leftOf(dir)) + ScoreIn(Directions.rightOf(dir));
		}
		else
		{
			return 0;
		}
	}

	public void SetTotalSpaceIn(Direction dir)
	{
		SpaceCounter next = GetParent().GetCounterAt(Directions.InDirection(location, dir));

		if (next == null || ScoreIn(dir) <= 0) SetCostIn(dir, 0);
		else SetCostIn(dir, next.CostIn(dir) + next.ScoreIn(Directions.leftOf(dir)) + next.ScoreIn(Directions.rightOf(dir)));
	}

	public int[] BestSplitLocForDir(Direction growingIn)
	{
		if (growingIn == Direction.None) return new int[0];

		Direction left = Directions.leftOf(growingIn);
		Direction right = Directions.rightOf(growingIn);

		List<int> bestLeft = new List<int>();
		int bestLeftVal = 0;
		List<int> bestRight = new List<int>();
		int bestRightVal = 0;

		Int2 currentLoc = Directions.InDirection(location, growingIn);
		SpaceCounter current = GetParent().GetCounterAt(currentLoc);

		int currWait = 1;
		while (current != null && current.ScoreIn(growingIn) > 0)
		{
			int scoreLeft = current.ScoreIn(left);
			if (scoreLeft > bestLeftVal)
			{
				bestLeftVal = scoreLeft;
				bestLeft.Clear();
				bestLeft.Add(currWait);
			}
			else if (scoreLeft == bestLeftVal) bestLeft.Add(currWait);

			int scoreRight = current.ScoreIn(left);
			if (scoreRight > bestRightVal)
			{
				bestRightVal = scoreRight;
				bestRight.Clear();
				bestRight.Add(currWait);
			}
			else if (scoreRight == bestRightVal) bestRight.Add(currWait);
			
			currentLoc = Directions.InDirection(location, growingIn);
			current = GetParent().GetCounterAt(currentLoc);
			currWait++;
		}

		int[] splitNums = new int[Directions.NUM_VALID_DIRECTIONS];
		for ( int i = 0; i < splitNums.Length; i++ )
		{
			if ((int)(left) == i && bestLeft.Count > 0)
			{
				splitNums[i] = bestLeft[Random.Range(0, bestLeft.Count)];
			}
			else if ((int)(right) == i && bestRight.Count > 0)
			{
				splitNums[i] = bestRight[Random.Range(0, bestRight.Count)];
			}
			else splitNums[i] = -1;
		}
		return splitNums;
	}

	protected void OnUpdateTotals()
	{
		SetValue(CalculateValue());
	}

	protected int OnCalculateValue()
	{
		int result = 0;
		foreach (Direction curr in Directions.ValidDirections())
		{
			if (ScoreIn(curr) <= 0) SetCostIn(curr, 0);
			result += TotalSpaceIn(curr);
		}
		return result;
	}

	public int CostIn(Direction givenDir)
	{
		if (totalStale) UpdateTotals();

		int index = (int)(givenDir);
		if (index >= 0 && index < Directions.NUM_VALID_DIRECTIONS) return dirCosts[index];
		else return 0;
	}

	public int TotalCost()
	{
		if (totalStale) UpdateTotals();

		return cost;
	}

	public int ScoreIn(Direction givenDir)
	{
		if (totalStale) UpdateTotals();

		int index = (int)(givenDir);
		if (index >= 0 && index < Directions.NUM_VALID_DIRECTIONS) return dirScores[index];
		else return 0;
	}

	public int TotalScore()
	{
		if (totalStale) UpdateTotals();

		return score;
	}

	public int Value()
	{
		if (totalStale) UpdateTotals();

		return value;
	}

	public void SetCostIn(Direction givenDir, int givenCost)
	{
		int index = (int)(givenDir);
		if (index >= 0 && index < Directions.NUM_VALID_DIRECTIONS) dirCosts[index] = givenCost;

		totalStale = true;
	}

	public void SetScoreIn(Direction givenDir, int givenScore)
	{
		int index = (int)(givenDir);
		if (index >= 0 && index < Directions.NUM_VALID_DIRECTIONS) dirScores[index] = givenScore;

		totalStale = true;
	}

	public void SetCosts(int[] givenCosts)
	{
		if (givenCosts.Length <= Directions.NUM_VALID_DIRECTIONS)
		{
			dirCosts = givenCosts;
		}

		totalStale = true;
	}

	public void SetScores(int[] givenScores)
	{
		if (givenScores.Length <= Directions.NUM_VALID_DIRECTIONS)
		{
			dirScores = givenScores;
		}

		totalStale = true;
	}

	public void UpdateTotals()
	{
		OnUpdateTotals();
		totalStale = false;
	}

	public void CalculateScore()
	{
		int sum = 0;

		foreach (int curr in dirScores) sum += curr;

		score = sum;
	}

	public void CalculateCost()
	{
		int sum = 0;

		foreach (int curr in dirCosts) sum += curr;

		cost = sum;
	}

	public void SetValue(int given)
	{
		value = given;
	}

	public int CalculateValue()
	{
		return OnCalculateValue();
	}
}
