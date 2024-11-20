using Unity.VisualScripting;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float playerReach;
    private Interactable currentInteractable;
    public Transform orientation;
    public GameManager gm;

    public bool player1;

    public bool gun;



	// Update is called once per frame
	void Update()
    {
        if (gm.getCamera() == player1)
        {
			CheckInteraction();
			if (Input.GetKeyDown(KeyCode.G) && currentInteractable != null)
			{
				currentInteractable.Interact();
			}
		}
    }

    void CheckInteraction()
    {
        RaycastHit hit;
        Ray ray = new Ray(orientation.transform.position, orientation.transform.forward);
        if (Physics.Raycast(ray, out hit, playerReach))
        {
            if (hit.collider.tag == "Interactable")
            {
				int targetLayer = LayerMask.NameToLayer("gun");

                if (hit.collider.gameObject.layer == targetLayer)
                    gun = true;
                else
                    gun = false;

				if (!player1 || (!gun && player1))
				{
					Interactable newInteractable = hit.collider.GetComponent<Interactable>();
					if (currentInteractable && newInteractable != currentInteractable)
					{
						currentInteractable.DisableOutLine();
					}

					if (newInteractable.enabled)
					{
						SetNewCurrentInteractable(newInteractable);
					}
					else // if interactable is not enabled
					{
						DisableCurrentInteractable();
					}
				}
            }
            else //if not an interactable
            {
                DisableCurrentInteractable();
            }
        }
        else // if nothing is in reach
        {
            DisableCurrentInteractable();
        }
    }

    void SetNewCurrentInteractable(Interactable newInteractable)
    {
        currentInteractable = newInteractable;
        currentInteractable.EnableOutLine();
        HUDController.instance.EnableInteractionText(currentInteractable.message);
    }

    void DisableCurrentInteractable()
    {
        HUDController.instance.DisableInteractionText();
        if (currentInteractable)
        {
            currentInteractable.DisableOutLine();
            currentInteractable = null;
        }
    }
}
