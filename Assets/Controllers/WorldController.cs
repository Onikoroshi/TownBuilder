using UnityEngine;
using System.Collections;

public class WorldController : MonoBehaviour
{
	public Sprite flatSprite, blockedSprite, structureSprite;

	public World World { get; protected set; }

	void Start()
	{
		World = new World(10, 10);

		int tilesCreated = 0;
		for ( int x = 0; x < World.Width; x++ )
		{
			for ( int y = 0; y < World.Height; y++ )
			{
				Tile tile_data = World.initalizeTileAt(x, y);
				if (tile_data != null)
				{
					GameObject tile_go = new GameObject(); 
					tile_go.name = "Tile_" + x + "_" + y;
					tile_go.transform.position = new Vector3(tile_data.X, tile_data.Y, 0);
					tile_go.transform.SetParent(this.transform, true);
					tile_go.layer = LayerMask.NameToLayer("ground");
					SpriteRenderer tile_sr = tile_go.AddComponent<SpriteRenderer>();

					if (tile_data.Type == Tile.TileType.Blocked)
					{
						tile_sr.sprite = blockedSprite;
					}
					else
					{
						tile_sr.sprite = flatSprite;	
					}

					tile_data.RegisterTileTypechangedCallback((tile) =>
					{
						OnTileTypeChanged(tile_data, tile_go);
					});

					tile_data.RegisterSurfaceChangedCallback((tile) =>
					{
						OnSurfaceChanged(tile_data, tile_go);
					});

					tile_data.RegisterStructureChangedCallback((tile) =>
					{
						OnStructureChanged(tile_data, tile_go);
					});

					tilesCreated++;
				}
			}
		}

		Debug.LogError("World initialized with " + tilesCreated + " tiles.");

		World.initializeObstacles();

		World.filled = true;
	}

	float randomizeTileTimer = 2f;

	void Update()
	{
//		randomizeTileTimer -= Time.deltaTime;
//
//		if (randomizeTileTimer < 0) {
//			Debug.LogError("Randomizing tiles");
//			randomizeTiles();
//			randomizeTileTimer = 2f;
//		}
	}

	void OnTileTypeChanged( Tile tile_data, GameObject tile_go )
	{
		if (tile_data.Type == Tile.TileType.Flat)
		{
			tile_go.GetComponent<SpriteRenderer>().sprite = flatSprite;
		}
		else if (tile_data.Type == Tile.TileType.Blocked)
		{
			tile_go.GetComponent<SpriteRenderer>().sprite = blockedSprite;
		}
		else
		{
			Debug.LogError("OnTileTypeChanged - Unrecognized tile type.");
		}
	}

	void OnSurfaceChanged( Tile tile_data, GameObject tile_go )
	{
		if (tile_data.Surface.type == SurfaceMod.Type.Road)
		{
			tile_go.GetComponent<SpriteRenderer>().color = Color.grey;
		}
		else
		{
			Debug.LogError("OnTileTypeChanged - Unrecognized tile type.");
		}
	}

	void OnStructureChanged( Tile tile_data, GameObject tile_go )
	{
//		Debug.LogError("adding structure to " + tile_data.ToS() + " with building type " + tile_data.Building.type.ToString());
		if (tile_data.Building.type == Structure.Type.House || tile_data.Building.type == Structure.Type.CityCenter)
		{
			if (!tile_data.atLocation(tile_data.Building.lowerLeft)) return;

			GameObject structure_go = new GameObject(); 
			structure_go.name = tile_data.Building.type.ToString() + "_" + tile_data.Building.entrance.foundation.Loc().x + "_" + tile_data.Building.entrance.foundation.Loc().y;
			structure_go.transform.position = new Vector3(((float)tile_data.X) - 0.5f, ((float)tile_data.Y) - 0.5f, 0);
			structure_go.transform.SetParent(tile_go.transform, true);
			structure_go.layer = LayerMask.NameToLayer("structure");
			SpriteRenderer structure_sr = structure_go.AddComponent<SpriteRenderer>();
			structure_go.GetComponent<SpriteRenderer>().sprite = structureSprite;
			structure_go.transform.localScale = new Vector3((tile_data.Building.upperRight.x + 1) - tile_data.Building.lowerLeft.x, (tile_data.Building.upperRight.y + 1) - tile_data.Building.lowerLeft.y, 1);

			if (tile_data.Building.type == Structure.Type.House)
				structure_go.GetComponent<SpriteRenderer>().color = Color.cyan;
			else if (tile_data.Building.type == Structure.Type.CityCenter)
				structure_go.GetComponent<SpriteRenderer>().color = Color.yellow;
		}
		else
		{
			Debug.LogError("OnTileTypeChanged - Unrecognized tile type.");
		}
	}
}
