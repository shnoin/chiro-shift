using Unity.VisualScripting;
using UnityEngine;

public class FallIntoVoid : MonoBehaviour
{
	public GameManager gm;

	private void Start()
	{
		gm = FindFirstObjectByType<GameManager>();
	}

	private void OnTriggerEnter(Collider other)
	{
		gm.ResetTutLevel();
	}
}
