using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class UrbanPlanningOffice : CityOffice
{
	public RoadBuilder roader;
	public StructureBuilder structurer;
	public bool roadSpaceAvailable, buildingSpaceAvailable;
	public Tile cityCenter;
	public List<RoadNode> expandableRoads;

	public UrbanPlanningOffice( City givenCity, RoadBuilder roadB = null, StructureBuilder structureB = null ) : base(givenCity )
	{
		roader = (roadB == null) ? new RoadBuilder(city) : roadB;
		structurer = (structureB == null) ? new StructureBuilder(city) : structureB;

		roadSpaceAvailable = city.startRoad != null && city.startRoad.HasRoad() && city.startRoad.Neighbors().FindIndex(elem => elem.IsBuildable()) >= 0;
		buildingSpaceAvailable = roadSpaceAvailable;

		expandableRoads = new List<RoadNode>();
		List<Direction> must = new List<Direction>();
		must.Add(Direction.East);
		must.Add(Direction.West);
		List<Direction> can = new List<Direction>();
		int[] depths = new int[4] { -1, -1, -1, -1 };
		RoadNode firstNode = new RoadNode((Road)city.startRoad.Surface, must, can, depths);
		if (firstNode.IsAvailable())
		{
			expandableRoads.Add(firstNode);
		}
	}

	private void HandleStructureRequest(StructureRequest request)
	{
		Debug.LogError("handling a structure request " + request.ToString());

		if (expandableRoads.Count <= 0) return;

		LocationPair bounds = null;

		Road success = null;
		List<RoadNode> toRemove = new List<RoadNode>();
		foreach (RoadNode toConsider in expandableRoads)
		{
			if (!toConsider.IsAvailable())
			{
				toRemove.Add(toConsider);
				continue;
			}

			bounds = toConsider.TryToBuild(request.size);
			if (bounds != null)
			{
				success = (Road)(toConsider.ground.Surface);
				break;
			}
		}

		foreach (RoadNode removed in toRemove)
		{
			expandableRoads.RemoveAll((elem) => (elem == removed));
		}

		if (bounds == null)
		{
			bool someoneChanged = false;
			foreach (RoadNode toGrow in expandableRoads)
			{
				RoadNode expanded = toGrow.Grow();
				if (expanded != null)
				{
					expandableRoads.Add(expanded);
					someoneChanged = true;
					break;
				}
			}

			if (!someoneChanged)
			{
				foreach (RoadNode toSplit in expandableRoads)
				{
					RoadNode split = toSplit.Split();
					if (split != null)
					{
						expandableRoads.Add(split);
						someoneChanged = true;
						break;
					}
				}
			}

			if (someoneChanged) request.Reject();
		}
		else
		{
//			buildStructure(Road connectedTo, Direction directionFromRoad, Structure.Type givenType, LocationPair locals)
			structurer.buildStructure(success, bounds.toHere, Structure.Type.House, bounds);
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

		if (roadSpaceAvailable && buildingSpaceAvailable && expandableRoads.Count > 0)
		{
			bills.Enqueue(new StructureRequest(this, CityBill.BillObject.House, size));
		}
	}

	public class RoadNode
	{
		public Tile ground;
		public List<Direction> mustGrowIn, canGrowIn;
		public int[] availableDepths;

		public RoadNode(Road given)
		{
			ground = (given != null) ? given.foundation : null;

			mustGrowIn = new List<Direction>();
			canGrowIn = new List<Direction>();
			availableDepths = new int[Directions.ValidDirections().Length];
			for (int i = 0; i < availableDepths.Length; i++)
			{
				availableDepths[i] = -1;
			}
		}

		public RoadNode(Road given, Direction[] givenMustGrowIn, Direction[] givenCanGrowIn, int[] givenDepths)
		{
			ground = (given != null) ? given.foundation : null;

			mustGrowIn = new List<Direction>(givenMustGrowIn);
			canGrowIn = new List<Direction>(givenCanGrowIn);
			availableDepths = givenDepths;
		}

		public RoadNode(Road given, List<Direction> givenMustGrowIn, List<Direction> givenCanGrowIn, int[] givenDepths)
		{
			ground = (given != null) ? given.foundation : null;

			mustGrowIn = givenMustGrowIn;
			canGrowIn = givenCanGrowIn;
			availableDepths = givenDepths;
		}

		public RoadNode Grow()
		{
			if (ground == null) return null;

			Tile best = null;
			Tile goodEnough = null;
			foreach (Direction dir in mustGrowIn)
			{
				Tile current = ground.getDirection(dir);

				if (current.IsBuildable())
				{
					Tile toConsider = ground.getDirection(Directions.oppositeOf(dir));
					if (toConsider.HasRoad()) best = current;
					else goodEnough = current;
				}
			}

			if (best == null)
			{
				foreach (Direction dir in canGrowIn)
				{
					Tile current = ground.getDirection(dir);

					if (current.IsBuildable())
					{
						Tile toConsider = ground.getDirection(Directions.oppositeOf(dir));
						if (toConsider.HasRoad()) best = current;
						else if (goodEnough == null) goodEnough = current;
					}
				}
			}

			Tile toUse = (best != null) ? best : goodEnough;

			if (toUse == null)
			{
				Debug.LogError("Can't grow at " + ground.ToS());
				return null;
			}

			Road created = new Road(toUse);
			return new RoadNode(created, mustGrowIn, canGrowIn, availableDepths);
		}

		public RoadNode Split()
		{
			if (ground == null) return null;

			Tile target = null;
			Direction targetDir = Direction.None;
			foreach (Direction dir in CanSplitIn())
			{
				Tile current = ground.getDirection(dir);
				if (current.IsBuildable())
				{
					target = current;
					targetDir = dir;
					break;
				}
			}

			if (target == null)
			{
				Debug.LogError("Can't split at " + ground.ToS());
				return null;
			}

			Road created = new Road(target);
			List<Direction> targetMustGrow = new List<Direction>();
			targetMustGrow.Add(targetDir);
			return new RoadNode(created, targetMustGrow, canGrowIn, availableDepths);
		}

		public LocationPair TryToBuild(Int2 buildingSize)
		{
			if (!IsAvailableToBuild() || !IsAvailableToGrow()) return null;

			List<Direction> availableDirs = CanSplitIn();

			if (availableDirs.Count <= 0) return null;

			LocationPair buildingBounds = null;
			foreach (Direction dir in availableDirs)
			{
				Tile toConsider = ground.getDirection(dir);
				buildingBounds = StructureBuilder.BoundsForStructure(toConsider, dir, buildingSize.x, buildingSize.y);

				if (buildingBounds != null)
				{
					availableDepths[(int)(dir)] = buildingBounds.SpaceInDirection(dir);
					break;
				}
			}

			return buildingBounds;
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
			if (ground == null) return false;

			Debug.LogError("checking build available at " + ground.ToS());
			foreach (Direction dir in Directions.ValidDirections())
			{
				Tile current = ground.getDirection(dir);

				if (mustGrowIn.FindIndex((elem) => (elem == dir)) < 0 && current.IsBuildable())
				{
					Debug.LogError(current.ToS() + " (" + dir.ToString() + ") is buildable.");
					Debug.LogError("Left (" + Directions.leftOf(dir).ToString() + ") is buildable? " + current.getDirection(Directions.leftOf(dir)).IsBuildable());
					Debug.LogError("Left (" + Directions.leftOf(dir).ToString() + ") is buildable? " + current.getDirection(Directions.leftOf(dir)).IsBuildable());
					if (current.getDirection(Directions.leftOf(dir)).IsBuildable() || current.getDirection(Directions.rightOf(dir)).IsBuildable()) return true;
				}
			}

			return false;
		}

		public bool IsAvailableToGrow()
		{
			if (ground == null) return false;

			foreach (Direction dir in mustGrowIn)
			{
				Tile current = ground.getDirection(dir);

				if (current.IsBuildable()) return true;
			}

			foreach (Direction dir in canGrowIn)
			{
				Tile current = ground.getDirection(dir);

				if (current.IsBuildable()) return true;
			}

			return false;
		}

		public void SetAvailableDepthIn(Direction dir, int givenDepth)
		{
			int index = (int)(dir);

			if (index < 0 || index >= availableDepths.Length) return;

			availableDepths[index] = givenDepth;
		}

		public int AvailableDepthIn(Direction dir)
		{
			int index = (int)(dir);

			if (index < 0 || index >= availableDepths.Length) return -1;

			return availableDepths[(int)(dir)];
		}

		public bool DepthFitsIn(Direction dir, int considerDepth)
		{
			int currentDepth = AvailableDepthIn(dir);

			return currentDepth < 0 || currentDepth >= considerDepth;
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

		public List<Direction> CanSplitIn()
		{
			List<Direction> results = new List<Direction>();

			if (ground == null) return results;

			foreach (Direction dir in Directions.ValidDirections())
			{
				if (mustGrowIn.FindIndex((elem ) => (elem == dir)) >= 0) continue;

				Tile current = ground.getDirection(dir);

				if (current.IsBuildable())
				{
					results.Add(dir);
				}
			}

			return results;
		}

		public List<Tile> CanSplitTo()
		{
			List<Tile> results = new List<Tile>();

			if (ground == null) return results;

			foreach (Direction dir in Directions.ValidDirections())
			{
				Tile current = ground.getDirection(dir);

				if (current.IsBuildable())
				{
					Tile toConsider = ground.getDirection(Directions.oppositeOf(dir));
					if (toConsider.HasNoRoad()) results.Add(current);
				}
			}

			return results;
		}

		public List<Tile> CanGrowTo()
		{
			List<Tile> results = new List<Tile>();

			if (ground == null) return results;

			foreach (Direction dir in mustGrowIn)
			{
				Tile current = ground.getDirection(dir);

				if (current.IsBuildable()) results.Add(current);
			}

			foreach (Direction dir in canGrowIn)
			{
				Tile current = ground.getDirection(dir);

				if (current.IsBuildable()) results.Add(current);
			}

			return results;
		}
	}
}
