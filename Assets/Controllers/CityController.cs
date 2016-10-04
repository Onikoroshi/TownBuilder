using UnityEngine;
using System.Collections;

public class CityController : MonoBehaviour
{
	private float stepSpeed = 0.1f;

	public World world;
	public City city;

	private float nextActionTime = 0.0f;

	// Use this for initialization
	void Start ()
	{
		setWorld();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (world == null)
		{
			setWorld();
		}

		if (world == null || city == null) return;

		if (Time.time > nextActionTime ) {
//			Debug.LogError("using step speed " + stepSpeed);
			nextActionTime = Time.time + stepSpeed;

			city.handleNextBill();
		}
	}

	private void setWorld()
	{
		if (world != null)
		{
			return;
		}

		GameObject controllerGameObject = GameObject.Find("WorldController");
		if (controllerGameObject != null)
		{
			WorldController controllerObject = controllerGameObject.GetComponent<WorldController>();
			if (controllerObject != null)
			{
				World consider = controllerObject.World;
				if (consider != null && consider.filled)
				{
					world = consider;
					city = new City(world);
					stepSpeed = 30f / (float)(world.Width * world.Height);
//					Debug.LogError("set step speed to " + stepSpeed);
				}
			} 
		}
	}
}
