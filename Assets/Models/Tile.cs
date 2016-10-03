using UnityEngine;
using System.Collections;
using System;
using Random = UnityEngine.Random;
using System.Collections.Generic;

public class Tile
{
	public World world;

	public int X { get; protected set; }

	public int Y { get; protected set; }

	public enum TileType { Flat, Blocked, None }

	private TileType _type = TileType.Flat;
	public TileType Type
	{
		get { return _type; }
		set
		{
			if (_type != value)
			{
				_type = value; 
				if (cbTileTypeChanged != null)
					cbTileTypeChanged(this);
			}
		}
	}
	Action<Tile> cbTileTypeChanged;

	public enum StateType { Empty, Occupied }

	private StateType _state = StateType.Empty;
	public StateType State { get { return _state; } set { _state = value; } }

	private SurfaceMod _surface;
	public SurfaceMod Surface
	{
		get { return _surface; }
		set
		{
			if (_surface != value)
			{
				_surface = value;
				if (cbSurfaceChanged != null) cbSurfaceChanged(this);
			}
		}
	}
	Action<Tile> cbSurfaceChanged;

	private Structure _structure;
	public Structure Building
	{
		get { return _structure; }
		set
		{
			if (_structure != value)
			{
				_structure = value;
				if (atLocation(Building.lowerLeft) && cbStructureChanged != null) cbStructureChanged(this);
			}
		}
	}
	Action<Tile> cbStructureChanged;

	public Tile()
	{
		this.Type = TileType.None;
	}

	public Tile( World world, int x, int y, TileType type )
	{
		this.world = world;
		this.X = x;
		this.Y = y;
		this._type = type;
	}

	public bool atLocation(Int2 other)
	{
		return !IsNone() && (this.X == other.x) && (this.Y == other.y);
	}

	public Int2 Loc()
	{
		return new Int2(X, Y);
	}

	public bool isFlat()
	{
		return !IsNone() && Type == TileType.Flat;
	}

	public bool IsOccupied()
	{
		return !IsNone() && State == StateType.Occupied;
	}

	public bool IsUnoccupied()
	{
		return !IsOccupied();
	}

	public bool HasRoad()
	{
		return !IsNone() && Surface != null && Surface.type == SurfaceMod.Type.Road;
	}

	public bool HasNoRoad()
	{
		return IsNone() || !HasRoad();
	}

	public bool IsBuildable()
	{
		if (IsNone())
		{
			Debug.LogError(ToS() + " is not a valid tile");
		}
		else if (!isFlat())
		{
			Debug.LogError(ToS() + " is not flat");
		}
		else if (IsOccupied())
		{
			Debug.LogError(ToS() + " is occupied");
		}
		else if (HasRoad())
		{
			Debug.LogError(ToS() + " has a road");
		}
		return !IsNone() && isFlat() && IsUnoccupied() && HasNoRoad();
	}

	public bool IsUnBuildable()
	{
		return !IsBuildable();
	}

	public bool IsNone()
	{
		return Type == TileType.None;
	}

	public List<Direction> NeighborDirections()
	{
		List<Direction> adjacent = new List<Direction>();

		foreach (Direction dir in Directions.ValidDirections())
		{
			Tile attempt = getDirection(dir);
			if (!attempt.IsNone())
				adjacent.Add(dir);
		}

		return adjacent;
	}

	public List<Direction> BuildableNeighborDirections()
	{
		List<Direction> adjacent = new List<Direction>();

		foreach (Direction dir in Directions.ValidDirections())
		{
			Tile attempt = getDirection(dir);
			if (attempt.IsBuildable())
				adjacent.Add(dir);
		}

		return adjacent;
	}

	public List<Tile> Neighbors()
	{
		List<Tile> adjacent = new List<Tile>();

		foreach (Direction dir in Directions.ValidDirections())
		{
			Tile attempt = getDirection(dir);
			if (!attempt.IsNone())
				adjacent.Add(attempt);
		}

		return adjacent;
	}

	public List<Tile> BuildableNeighbors()
	{
		List<Tile> adjacent = new List<Tile>();

		foreach (Direction dir in Directions.ValidDirections())
		{
			Tile attempt = getDirection(dir);
			if (attempt.IsBuildable())
				adjacent.Add(attempt);
		}

		return adjacent;
	}

	public Tile getDirection(Direction dir)
	{
		switch (dir)
		{
			case Direction.East:
				return world.GetTileAt(X + 1, Y);
			case Direction.North:
				return world.GetTileAt(X, Y + 1);
			case Direction.South:
				return world.GetTileAt(X, Y - 1);
			case Direction.West:
				return world.GetTileAt(X - 1, Y);
		}

		Debug.LogError("invalid direction given.");
		return new Tile();
	}

	public Direction GoDirectionTo(Tile other)
	{
		if (other.X < X) return Direction.West;
		else if (other.X > X) return Direction.East;
		else if (other.Y < Y) return Direction.South;
		else if (other.Y > Y) return Direction.North;
		else return Direction.None;
	}

	public void RegisterTileTypechangedCallback( Action<Tile> callback )
	{
		cbTileTypeChanged += callback;
	}

	public void UnregisterTileTypechangedCallback( Action<Tile> callback )
	{
		cbTileTypeChanged -= callback;
	}

	public void RegisterSurfaceChangedCallback( Action<Tile> callback )
	{
		cbSurfaceChanged += callback;
	}

	public void UnregisterSurfaceChangedCallback( Action<Tile> callback )
	{
		cbSurfaceChanged -= callback;
	}

	public void RegisterStructureChangedCallback( Action<Tile> callback )
	{
		cbStructureChanged += callback;
	}

	public void UnregisterStructureChangedCallback( Action<Tile> callback )
	{
		cbStructureChanged -= callback;
	}

	public string ToS()
	{
		return (IsNone()) ? "Invalid" : X + "," + Y;
	}

	public static TileType randomType()
	{
		TileType targetType = TileType.Flat;
		if (Random.Range(0, 2) % 2 != 0)
		{
			targetType = TileType.Blocked;
		}
		return targetType;
	}

	public static TileType defaultType()
	{
		return TileType.Flat;
	}

	public static Predicate<Tile> equalTo(Tile other)
	{
		return delegate(Tile tile )
		{
			return tile.X == other.X && tile.Y == other.Y;
		};
	}
}
