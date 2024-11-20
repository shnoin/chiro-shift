using UnityEngine;
using UnityEngine.UI;

public class healthBar : MonoBehaviour
{
    public Image fill;
    public float changeSpeed;
    public float fillAmount {get; set;} = 1f;
    
    void Update()
    {
        fill.fillAmount = Mathf.Lerp(fill.fillAmount, fillAmount, changeSpeed * Time.deltaTime);
    }
}
