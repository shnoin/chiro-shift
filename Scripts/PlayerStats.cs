using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public healthBar healthbar;

    private GameManager gameManager;

    void Start()
    {
		gameManager = FindFirstObjectByType<GameManager>();
	}

    void Update()
    {
        healthbar.fillAmount = gameManager.health/gameManager.maxHealth;
    }
}
