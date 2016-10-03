using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SeekOpenSpace : Flooder
{
	public SeekOpenSpace(World parent, Int2 givenStart) : base(parent, givenStart)
	{
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

	public SpaceSeekingNode PeekNextSpaceSeeker()
	{
		return (SpaceSeekingNode)(PeekNextNode());
	}

	protected override void OnInitializeLoveMap()
	{
		SpaceSeekingNode start = PeekNextSpaceSeeker();

		if (start == null) return;

		SetAllDistancesFromLocation(start.location, Directions.ValidDirectionList());
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
		if (begin == null) return;

		Tile current = universe.GetTileAt(begin.location);
		Stack<SpaceCounter> toScore = new Stack<SpaceCounter>();

		while (current.IsBuildable())
		{
			SpaceCounter currCounter = GetCounterAt(current.Loc());
			if (currCounter == null)
			{
				currCounter = new SpaceCounter(this, current.Loc());
				loveMap[current.Loc().x, current.Loc().y] = currCounter;
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

	protected override int OnCompareNodes(FloodingNode a, FloodingNode b)
	{
		SpaceSeekingNode sa = (SpaceSeekingNode)(a);
		SpaceSeekingNode sb = (SpaceSeekingNode)(b);
		return sa.GetCounter().Value().CompareTo(sb.GetCounter().Value());
	}

	public void blockLocation(Int2 toBlock)
	{
		SpaceCounter blocked = GetCounterAt(toBlock);

		if (blocked == null) return;

		foreach (Direction dir in Directions.ValidDirections())
		{
			SpaceCounter curr = GetCounterAt(Directions.InDirection(blocked.location, dir));
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
		}
	}
}

public class SpaceSeekingNode : FloodingNode
{
	public SpaceSeekingNode( SeekOpenSpace parent, Int2 location ) : base(parent, location )
	{
	}

	public SeekOpenSpace Parent()
	{
		return (SeekOpenSpace)(parent);
	}

	public SpaceCounter GetCounter()
	{
		return Parent().GetCounterAt(location);
	}
}

public class SpaceCounter : CountingNode
{
	public SpaceCounter( SeekOpenSpace parent, Int2 location ) : base(parent, location )
	{
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

	protected override void OnUpdateTotals()
	{
		SetValue(CalculateValue());
	}

	protected override int OnCalculateValue()
	{
		int result = 0;
		foreach (Direction curr in Directions.ValidDirections())
		{
			if (ScoreIn(curr) <= 0) SetCostIn(curr, 0);
			result += TotalSpaceIn(curr);
		}
		return result;
	}
}
