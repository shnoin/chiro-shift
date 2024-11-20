using UnityEngine;

public class DisappearPlatform : MonoBehaviour
{
    public float disappearDelay = 5f;
    public float reappearDelay = 3f;
    public float warningDuration = 2.5f;

    Renderer platformRenderer;
    Collider platformCollider;
    Material platformMat;
    private Color originalColor;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        platformRenderer = GetComponent<Renderer>();
        platformCollider = GetComponent<Collider>();
        platformMat = GetComponent<Material>();

        originalColor = platformMat.color;
       
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Invoke("WarningColor", disappearDelay - warningDuration);
            Invoke("Disappear", disappearDelay);
        }
    }

    void WarningColor()
    {
        platformMat.color = Color.black;
    }

    void Disappear()
    {
        platformRenderer.enabled = false;
        platformCollider.enabled = false;
        Invoke("Reappear", reappearDelay);
    }

    void Reappear()
    {
        platformRenderer.enabled = false;
        platformCollider.enabled = false;
    }
}
