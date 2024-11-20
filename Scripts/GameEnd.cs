using System.Collections.Generic;
using UnityEngine;

public class GameEnd : MonoBehaviour
{
	public GameManager gm;

	private List<GameObject> objectsInside = new List<GameObject>();

	private void OnTriggerEnter(Collider other)
	{
		// Add the GameObject to the list if it's not already in it
		if (!objectsInside.Contains(other.gameObject) && other.transform.root.CompareTag("Player1"))
		{
			objectsInside.Add(other.gameObject);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		// Remove the GameObject from the list
		if (objectsInside.Contains(other.gameObject))
		{
			objectsInside.Remove(other.gameObject);
		}
	}

	public void Start()
	{
		gm = FindFirstObjectByType<GameManager>();
	}

	public void Update()
	{
		if (GetCount() == 2 && !gm.ended)
		{
			StartCoroutine(gm.LevelEnd());
		}
	}

	public int GetCount()
	{
		// Return the number of objects currently inside the trigger
		return objectsInside.Count;
	}
}
