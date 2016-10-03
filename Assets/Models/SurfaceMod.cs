using UnityEngine;
using System.Collections;

public class SurfaceMod
{
	public enum Type { Road }

	public Tile foundation;
	public Type type;

	public SurfaceMod(Tile givenContainer, Type givenType)
	{
		foundation = givenContainer;
		type = givenType;

		if (foundation != null)
		{
			foundation.Surface = this;
		}
	}
}
