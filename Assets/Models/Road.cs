using UnityEngine;
using System.Collections;

public class Road : SurfaceMod
{
	public Road( Tile ground ) : base(ground, SurfaceMod.Type.Road)
	{
		Debug.LogError("Adding Road to " + ground.ToS());
	}
}
