using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class City
{
	public World world;
	public List<CityOffice> offices;
	public List<Builder> builders;

	public Tile startRoad;

	public City(World givenWorld)
	{
		world = givenWorld;

		startRoad = new Tile();
		List<Int2> tried = new List<Int2>();
		while (startRoad == null || tried.Count < (int)(world.Width * world.Height) || startRoad.IsUnBuildable())
		{
			Int2 attempt = new Int2(Random.Range(0, world.Width), Random.Range(0, world.Height));
			if (tried.FindIndex(elem => (elem.x == attempt.x && elem.y == attempt.y)) < 0)
			{
				startRoad = world.GetTileAt(attempt.x, attempt.y);
				tried.Add(attempt);
			}
		}

		Road built = new Road(startRoad);

		builders = new List<Builder>();
		RoadBuilder roadie = new RoadBuilder(this);
		StructureBuilder houser = new StructureBuilder(this);

		offices = new List <CityOffice>();
		offices.Add(new UrbanPlanningOffice(this, roadB: roadie, structureB: houser));
	}

	public void handleNextBill()
	{
		foreach (var office in offices)
		{
			office.HandleNextUnfinishedBill();
		}
	}
}
