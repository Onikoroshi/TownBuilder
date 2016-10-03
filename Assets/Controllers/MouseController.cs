using UnityEngine;
using System.Collections;

public class MouseController : MonoBehaviour
{

	Vector3 lastFramePosition;

	// Use this for initialization
	void Start()
	{
	
	}
	
	// Update is called once per frame
	void Update()
	{
		Vector3 currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
		{
			if (lastFramePosition != null)
			{
				Vector3 diff = lastFramePosition - currentPos;
				Camera.main.transform.Translate(diff);
			}
		}

		lastFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
	}
}
