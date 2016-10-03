using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class World {
	Tile[,] tiles;

	public int Width { get; private set; }
	public int Height { get; private set; }

	public World(int width = 100, int height = 100)
	{
		this.Width = width;
		this.Height = height;

		tiles = new Tile[width, height];

		Debug.LogError("World created empty.");
	}

	public Tile initalizeTileAt(int x, int y)
	{
		if (invalidPos(x, y))
		{
			Debug.LogError("Can't create tile (" + x + "," + y + ") out of this world.");
			return new Tile();
		}

		Tile tileToAdd = GetTileAt(x, y);
		if (tileToAdd.IsNone())
		{
			Tile.TileType targetType = Tile.defaultType();

			tileToAdd = new Tile(this, x, y, targetType);
			tiles[x, y] = tileToAdd;
		}
		return tileToAdd;
	}

	public bool invalidPos(int x, int y)
	{
		return x >= Width || x < 0 || y >= Height || y < 0;
	}

	public bool invalidPos(Vector2 loc)
	{
		return invalidPos((int)loc.x, (int)loc.y);
	}

	public bool validPos(int x, int y)
	{
		return !invalidPos(x, y);
	}

	public bool validPos(Vector2 loc)
	{
		return validPos((int)loc.x, (int)loc.y);
	}

	public Tile GetTileAt(Int2 location)
	{
		return GetTileAt(location.x, location.y);
	}

	public Tile GetTileAt(int x, int y)
	{
		Tile result = null;
		if (!invalidPos(x, y)) result = tiles[x, y];

		return (result == null) ? new Tile() : result;
	}

	public void initializeObstacles(int requestedNumber = -1)
	{
		randomizeObstacles(requestedNumber);
	}

	private void randomizeObstacles(int requestedNumber = -1)
	{
		if (requestedNumber < 0)
			requestedNumber = Random.Range(1, (int)(Height*Width*0.001f));

		Debug.LogError("randomizing " + requestedNumber + " obstacles.");

		List<Vector2> blocked = new List<Vector2>();
		for ( int i = 0; i < requestedNumber; i++ )
		{
			Vector2 attempt = new Vector2(Random.Range(0, Width), Random.Range(0, Height));
			while (blocked.FindIndex(elem => (elem.x == attempt.x && elem.y == attempt.y)) >= 0)
			{
				attempt.x = Random.Range(0, Width);
				attempt.y = Random.Range(0, Height);
			}
			blocked.Add(attempt);

			int centerX = (int)attempt.x;
			int centerY = (int)attempt.y;
			int targetWidth = Random.Range(1, (int)(Width * 0.5));
			int targetHeight = Random.Range(1, (int)(Height * 0.5));
			buildObstacleAt(centerX, centerY, targetWidth, targetHeight);
			Debug.LogError("Added at " + centerX + "," + centerY + " with width " + targetWidth + " and height " + targetHeight);
		}
//		buildObstacleAt(0, 8, 3, 3);
	}

	private void buildObstacleAt(int x, int y, int width, int height)
	{
		int halfHeight = height / 2;
		int maxHeight = Height - 2;
		int startY = Math.Max(1, Math.Min(maxHeight, y - halfHeight));
		int stopY = Math.Max(1, Math.Min(maxHeight, y + halfHeight));

		int lastScale = 1;
		for (int currY = startY; currY <= stopY; currY++)
		{
			int scale = lastScale;
			if (currY < y && (lastScale * 2) < width)
			{
				scale += Random.Range(1, 3);
			}
			else if ((lastScale * 2) > 1)
			{
				scale -= Random.Range(1, 3);
			}
			lastScale = scale;

			int startX = Math.Max(1, Math.Min((Width - 2), x - scale));
			int stopX = Math.Max(1, Math.Min((Width - 2), x + scale));

			if ((stopX - startX) <= 1)
				break;

			for ( int currX = startX; currX <= stopX; currX++ )
			{
				Tile currTile = GetTileAt(currX, currY);
				if (currTile.IsNone())
				{
					break;
				}
				else
				{
					currTile.Type = Tile.TileType.Blocked;
				}
			}
		}
	}

//	stupid way to do it. :(
//	private void buildObstacleAt(int x, int y, int width, int height)
//	{
//		Tile curr = GetTileAt(x, y);
//		List<Tile> open = new List<Tile>();
//		List<Tile> closed = new List<Tile>();
//
//		open.Add(curr);
//		while (open.Count > 0)
//		{
//			curr = open[0];
//			curr.Type = Tile.TileType.Blocked;
//			bool disp = curr.X == 2 && curr.Y == 8;
//
//			open.RemoveAt(0);
//			closed.Add(curr);
//
//			int i = 0;
//			List<Tile> neighbors = curr.Neighbors();
//			foreach (var neighbor in neighbors)
//			{
//
//				if (neighbor != null
//					&& neighbor.isFlat()
//					&& open.FindIndex(Tile.equalTo(neighbor)) < 0
//					&& closed.FindIndex(Tile.equalTo(neighbor)) < 0
//					&& insideObstacle(neighbor, x, y, width, height))
//				{
//					open.Add(neighbor);
//				}
//			}
//		}
//	}
//
//	private bool insideObstacle(Tile consider, int obsX, int obsY, int obsWidth, int obsHeight)
//	{
//		return consider.X >= obsX
//			&& consider.Y >= obsY
//			&& consider.X <= obsX + obsWidth
//			&& consider.Y <= obsY + obsHeight
//			&& consider.X < Width - 1
//			&& consider.Y < Height - 1
//			&& consider.X > 0
//			&& consider.Y > 0;
//	}
}
