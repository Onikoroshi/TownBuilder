using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Flooder
{
	protected World universe;
	protected CountingNode[,] loveMap;
	protected List<FloodingNode> openNodes, targetNodes, closedNodes;
	private bool openChanged, targetsChanged;
	protected FloodingNode startingNode;

	public Flooder(World parent, Int2 givenStart)
	{
		InitializeEverything(parent, givenStart);
	}

	public void InitializeEverything(World parent, Int2 givenStart)
	{
		OnInitializeEverything(parent, givenStart);
	}

	protected virtual void OnInitializeEverything(World parent, Int2 givenStart)
	{
		universe = parent;
		loveMap = new CountingNode[parent.Width, parent.Height];
		openNodes = new List<FloodingNode>();
		closedNodes = new List<FloodingNode>();
		targetNodes = new List<FloodingNode>();

		openChanged = false;
		targetsChanged = false;

		FloodingNode toStart = new FloodingNode(this, givenStart);
		CountingNode toCount = new CountingNode(this, toStart.location);
		loveMap[givenStart.x, givenStart.y] = toCount;
		AddOpenNode(toStart);
		startingNode = toStart;

		InitializeLoveMap();
	}

	public void AddOpenNode(FloodingNode given)
	{
		openNodes.Add(given);
		closedNodes.RemoveAll((elem ) => (elem.location.CompareTo(given.location) == 0));
		openChanged = true;
		given.open = true;
	}

	public void AddCloseNode(FloodingNode given)
	{
		closedNodes.Add(given);
		openNodes.RemoveAll((elem ) => (elem.location.CompareTo(given.location) == 0));
		openChanged = true;
		given.open = false;
	}

	public void AddTargetNode(FloodingNode given)
	{
		targetNodes.Add(given);
		given.target = true;
	}

	public FloodingNode PeekNextNode()
	{
		return OnGetNextNode();
	}

	protected virtual FloodingNode OnGetNextNode()
	{
		if (openNodes == null) return null;

		if (openNodes.Count <= 0) return null;

		if (openChanged)
		{
			openNodes.Sort((x, y ) => CompareNodes(x, y));
			openChanged = false;
		}

		return openNodes[0];
	}

	public FloodingNode PopNextNode()
	{
		FloodingNode result = OnGetNextNode();

		if (result != null) OnPopNextNode(result);

		return result;
	}

	protected virtual void OnPopNextNode(FloodingNode toPop)
	{
		openNodes.RemoveAll((elem ) => (elem.location.CompareTo(toPop.location) == 0));
	}

	public void InitializeLoveMap()
	{
		OnInitializeLoveMap();
	}

	protected virtual void OnInitializeLoveMap()
	{
		// Override this method in your child class.
	}

	public int CompareNodes(FloodingNode a, FloodingNode b)
	{
		return OnCompareNodes(a, b);
	}

	protected virtual int OnCompareNodes(FloodingNode a, FloodingNode b)
	{
		return a.location.CompareTo(b.location);
	}
}

public class FloodingNode
{
	protected Flooder parent;
	public Int2 location;
	public bool target, open;

	public FloodingNode(Flooder givenParent, Int2 givenLocation, bool isOpen)
	{
		parent = givenParent;
		location = givenLocation;
		open = isOpen;
	}

	public FloodingNode(Flooder givenParent, Int2 givenLocation) : this(givenParent, givenLocation, true)
	{
	}

	public bool IsOpen()
	{
		return open;
	}

	public bool isClosed()
	{
		return !open;
	}

	public bool IsTarget()
	{
		return target;
	}

	public void Open()
	{
		parent.AddOpenNode(this);
	}

	public void Close()
	{
		parent.AddCloseNode(this);
	}

	public void FoundTarget()
	{
		parent.AddTargetNode(this);
	}
}

public class CountingNode
{
	protected Flooder parent;
	public Int2 location;
	private int[] dirCosts, dirScores;
	private int score, cost, value;
	private bool totalStale;

	public CountingNode(Flooder givenParent, Int2 givenLoc)
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

	protected virtual void OnUpdateTotals()
	{
		CalculateScore();
		CalculateCost();
		SetValue(CalculateValue());
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

	protected virtual int OnCalculateValue()
	{
		return score - cost;
	}
}
