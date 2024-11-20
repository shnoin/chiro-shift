using UnityEngine;
using TMPro;    

public class HUDController : MonoBehaviour
{
    [SerializeField] TMP_Text interactionText;

    public static HUDController instance;

    private void Awake()
    {
        instance = this;
    }

    public void EnableInteractionText(string text)
    {
        interactionText.text = text + "(G)";
        interactionText.gameObject.SetActive(true);
    }

    public void DisableInteractionText()
    {
       
        interactionText.gameObject.SetActive(false);
    }


  
}
