using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class UrbanPlanningOffice : CityOffice
{
	public RoadBuilder roader;
	public StructureBuilder structurer;
	public List<RoadNode> buildableRoads, splittingRoads, growableRoads;
	public SeekOpenSpace seeker;

	public UrbanPlanningOffice( City givenCity, RoadBuilder roadB = null, StructureBuilder structureB = null ) : base(givenCity )
	{
		roader = (roadB == null) ? new RoadBuilder(city) : roadB;
		structurer = (structureB == null) ? new StructureBuilder(city) : structureB;

		Road origin = new Road(givenCity.startRoad);

		seeker = new SeekOpenSpace(givenCity.world, givenCity.startRoad.Loc());

		buildableRoads = new List<RoadNode>();
		splittingRoads = new List<RoadNode>();
		growableRoads = new List<RoadNode>();

		SpaceCounter startCounter = seeker.GetCounterAt(givenCity.startRoad.Loc());
		int[] splitNums = new int[4] { 0, 0, 0, 0 };
		Direction[] splitEverywhere = Directions.ValidDirections();
		RoadNode startNode = new RoadNode(origin, startCounter, Direction.None, splitEverywhere, splitNums);

		splittingRoads.Add(startNode);
	}

	private bool CanContinue()
	{
		return (buildableRoads.Count > 0) || (splittingRoads.Count > 0) || (growableRoads.Count > 0);
	}

	private void HandleStructureRequest(StructureRequest request)
	{
		Debug.LogError("handling a structure request " + request.ToString());

		if (!CanContinue()) return;

		List<RoadNode> toRemove = new List<RoadNode>();
		RoadNode toConsider = null;
		Direction buildableDir = Direction.None;
		for (int i = 0; i < buildableRoads.Count; i++)
		{
			toConsider = buildableRoads[i];

			if (!toConsider.IsAvailableToBuild())
			{
				toRemove.Add(toConsider);
			}
			else
			{
				buildableDir = toConsider.DirWithRoom(request.size);
				if (buildableDir != Direction.None)
				{
					break;
				}
			}

			toConsider = buildableRoads[0];
		}

		foreach (RoadNode removing in toRemove)
		{
			buildableRoads.RemoveAll((elem ) => (elem.ground.Loc() == removing.ground.Loc()));
		}

		if (toConsider != null)
		{
//			buildStructure(Road connectedTo, Direction directionFromRoad, Structure.Type givenType, int width, int depth)
			structurer.buildStructure((Road)(toConsider.ground.Surface), buildableDir, Structure.Type.House, request.size.x, request.size.y);
		}
		else
		{
			toRemove = new List<RoadNode>();
			RoadNode success = null;

			for ( int i = 0; i < splittingRoads.Count; i++ )
			{
				RoadNode considering = splittingRoads[i];

				success = considering.Split();
				if (success != null)
				{
					if (success.IsAvailableToBuild())
					{
						buildableRoads.Add(success);
					}

					if (success.IsAvailableToGrow())
					{
						growableRoads.Add(success);
					}

					if (success.IsAvailableToSplit())
					{
						splittingRoads.Add(success);
					}
				}

				if (considering.IsAvailableToBuild() && !(buildableRoads.FindIndex((elem) => (elem.ground.Loc() == considering.ground.Loc())) < 0))
				{
					buildableRoads.Add(considering);
				}

				if (considering.IsAvailableToGrow() && !(growableRoads.FindIndex((elem) => (elem.ground.Loc() == considering.ground.Loc())) < 0))
				{
					growableRoads.Add(considering);
				}

				if (!considering.IsAvailableToSplit())
				{
					toRemove.Add(considering);
				}

				if (success != null) break;
			}

			foreach (RoadNode removing in toRemove)
			{
				splittingRoads.RemoveAll((elem ) => (elem.ground.Loc() == removing.ground.Loc()));
			}

			if (success == null)
			{
				toRemove = new List<RoadNode>();

				for ( int i = 0; i < growableRoads.Count; i++ )
				{
					RoadNode considering = growableRoads[i];

					success = considering.Grow();
					if (success != null)
					{
						if (success.IsAvailableToBuild())
						{
							buildableRoads.Add(success);
						}

						if (success.IsAvailableToGrow())
						{
							growableRoads.Add(success);
						}

						if (success.IsAvailableToSplit())
						{
							splittingRoads.Add(success);
						}
					}

					if (considering.IsAvailableToBuild() && !(buildableRoads.FindIndex((elem ) => (elem.ground.Loc() == considering.ground.Loc())) < 0))
					{
						buildableRoads.Add(considering);
					}

					if (considering.IsAvailableToSplit() && !(splittingRoads.FindIndex((elem ) => (elem.ground.Loc() == considering.ground.Loc())) < 0))
					{
						splittingRoads.Add(considering);
					}

					if (!considering.IsAvailableToGrow())
					{
						toRemove.Add(considering);
					}

					if (success != null) break;
				}

				foreach (RoadNode removing in toRemove)
				{
					growableRoads.RemoveAll((elem ) => (elem.ground.Loc() == removing.ground.Loc()));
				}
			}

			if (success == null) request.Reject();
		}
	}

	protected override void onHandleNextUnfinishedBill()
	{
		CityBill toHandle = getNextUnfinishedBill();

		if (toHandle != null)
		{
			if (toHandle.GetType() == typeof(StructureRequest))
			{
				HandleStructureRequest((StructureRequest)toHandle);
				return;
			}

//			int targetX = Random.Range(0, ourWorld().Width);
//			int targetY = Random.Range(0, ourWorld().Height);
//
//			Tile target = ourWorld().GetTileAt(targetX, targetY);
//			roader.BuildRoad(target);
//			Road connection = (Road)target.Surface;
//
//			Direction buildDir = Directions.RandomDirection(target.NeighborDirections());
//
//			structurer.buildStructure(connection, buildDir, Structure.Type.House, 1, 1);
		}

		requestNextBill();
	}

	protected override void onRequestNextBill()
	{
		Int2 size = new Int2(1, 1);

		if (CanContinue())
		{
			bills.Enqueue(new StructureRequest(this, CityBill.BillObject.House, size));
		}
	}

	public class RoadNode
	{
		public Tile ground;
		public SpaceCounter counter;
		public List<Direction> splitIn, buildIn;
		public Direction growIn;
		public int[] spacesTilSplit;

		public RoadNode(Road givenRoad, SpaceCounter givenCounter, Direction toGrowIn, Direction[] toSplitIn, int[] givenSplitDists)
		{
			ground = (givenRoad != null) ? givenRoad.foundation : null;
			counter = givenCounter;
			growIn = toGrowIn;
			spacesTilSplit = givenSplitDists;

			splitIn = new List<Direction>();
			if (spacesTilSplit.Length > 0)
			{
				foreach (Direction dir in toSplitIn)
				{
					if (spacesTilSplit[(int)(dir)] == 0) splitIn.Add(dir);
				}
			}

			buildIn = new List<Direction>();
			foreach (Direction dir in Directions.ValidDirections())
			{
				if (dir != growIn && splitIn.FindIndex((elem) => (elem == dir)) < 0) buildIn.Add(dir);
			}
		}

		public RoadNode(Road givenRoad, SpaceCounter givenCounter, Direction toGrowIn) : this(givenRoad, givenCounter, toGrowIn, null, new int[0])
		{
			calculateSpacesTilSplit();
		}

		public void calculateSpacesTilSplit()
		{
			if (counter == null)
				spacesTilSplit = new int[0];
			else
				spacesTilSplit = counter.BestSplitLocForDir(growIn);
		}

		public RoadNode Grow()
		{
			if (ground == null || growIn == Direction.None) return null;

			Tile target = ground.getDirection(growIn);

			if (target.IsUnBuildable()) return null;

			Road created = new Road(target);
			int[] passSplits = new int[spacesTilSplit.Length];
			List<Direction> futureSplits = new List<Direction>();
			for ( int i = 0; i < spacesTilSplit.Length; i++ )
			{
				passSplits[i] = spacesTilSplit[i] - 1;
				if (passSplits[i] == 0) futureSplits.Add((Direction)(i));
			}
			if (counter != null) counter.GetParent().blockLocation(target.Loc());
			SpaceCounter nextCounter = counter.GetParent().GetCounterAt(target.Loc());
			return new RoadNode(created, nextCounter, growIn, futureSplits.ToArray(), passSplits);
		}

		public RoadNode Split()
		{
			if (ground == null || splitIn.Count <= 0) return null;

			Debug.LogError("trying to split at " + ground.ToS() + " with " + splitIn.Count + " possibilities" );

			Tile target = null;
			Direction targetDir = Direction.None;
			while (splitIn.Count > 0)
			{
				targetDir = splitIn[0];
				Debug.LogError("trying to split " + targetDir.ToString());
				Tile current = ground.getDirection(targetDir);
				Debug.LogError("looking at " + current.ToS());
				if (current.IsBuildable())
				{
					target = current;
					splitIn.RemoveAt(0);
					break;
				}
				else
				{
					splitIn.RemoveAt(0);
				}
			}

			if (target == null)
			{
				Debug.LogError("Can't split at " + ground.ToS() + " - " + splitIn.Count + " possibilities now");
				return null;
			}

			Road created = new Road(target);
			Debug.LogError("is my counter null? " + (counter == null));
			if (counter != null) counter.GetParent().blockLocation(target.Loc());
			SpaceCounter nextCounter = counter.GetParent().GetCounterAt(target.Loc());
			Debug.LogError("but the counter at " + target.Loc().ToString() + " is? " + (nextCounter == null));
			int[] splitNums = nextCounter.BestSplitLocForDir(targetDir);
			List<Direction> shouldSplit = new List<Direction>();
			for ( int i = 0; i < splitNums.Length; i++ )
			{
				if (splitNums[i] == 0) shouldSplit.Add((Direction)(i));
			}
			return new RoadNode(created, nextCounter, targetDir, shouldSplit.ToArray(), splitNums);
		}

		public Direction DirWithRoom(Int2 buildingSize)
		{
			if (!IsAvailableToBuild()) return Direction.None;

			Direction currDir = Direction.None;
			while (buildIn.Count > 0)
			{
				currDir = buildIn[0];
				int availableWidth = TillSplitInDir(currDir);

				if (availableWidth <= 0 || availableWidth < buildingSize.x) continue;

				Int2 loc = ground.Loc();
				int widthLeft = buildingSize.x;
				while (widthLeft > 0)
				{
					SpaceCounter currCounter = counter.GetParent().GetCounterAt(loc);
					if (currCounter == null || currCounter.ScoreIn(currDir) < buildingSize.y) break;

					widthLeft--;
					loc = Directions.InDirection(loc, growIn);
				}

				if (widthLeft == 0)
				{
					break;
				}
			}

			return currDir;
		}

		public bool IsAvailable()
		{
			if (ground == null) return false;

			foreach (Direction dir in Directions.ValidDirections())
			{
				Tile current = ground.getDirection(dir);

				if (current.IsBuildable()) return true;
			}

			return false;
		}

		public bool IsAvailableToBuild()
		{
			if (ground == null || counter == null || CanBuildOn().Count <= 0) return false;

			return true;
		}

		public bool IsAvailableToGrow()
		{
			if (ground == null || growIn == Direction.None || splitIn.Count > 0 || ground.getDirection(growIn).IsUnBuildable()) return false;

			return true;
		}

		public bool IsAvailableToSplit()
		{
			if (ground == null || CanSplitTo().Count <= 0) return false;

			return true;
		}

		public List<Tile> CanBuildOn()
		{
			List<Tile> results = new List<Tile>();

			if (ground == null || buildIn.Count <= 0) return results;

			foreach (Direction dir in buildIn)
			{
				Tile current = ground.getDirection(dir);

				if (current.IsBuildable())
				{
					results.Add(current);
				}
				else
				{
					buildIn.RemoveAll((elem ) => (elem == dir));
				}
			}

			return results;
		}

		public List<Tile> CanSplitTo()
		{
			List<Tile> results = new List<Tile>();

			if (ground == null) return results;

			foreach (Direction dir in splitIn)
			{
				Tile current = ground.getDirection(dir);

				if (current.IsBuildable())
				{
					results.Add(current);
				}
			}

			return results;
		}

		public List<Tile> ConnectedTo()
		{
			List<Tile> results = new List<Tile>();

			if (ground == null) return results;

			foreach (Direction dir in Directions.ValidDirections())
			{
				Tile current = ground.getDirection(dir);

				if (current.HasRoad()) results.Add(current);
			}

			return results;
		}

		public int TillSplitInDir(Direction given)
		{
			if (spacesTilSplit.Length <= 0) return -1;

			int index = (int)(given);
			if (index < 0 || index >= spacesTilSplit.Length) return -1;

			return spacesTilSplit[index];
		}
	}
}
