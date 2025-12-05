using UnityEngine;

public class ActivityLight : MonoBehaviour
{
    [Header("Optional: find these automatically if null")]
    [SerializeField] Renderer targetRenderer;
    [SerializeField] Light targetLight;

    [Header("Colors")]
    [SerializeField] Color invalidColor = Color.red;

    void Awake()
    {
        if (!targetRenderer) targetRenderer = GetComponent<Renderer>();
        if (!targetLight)    targetLight    = GetComponent<Light>();
        ShowInvalid(false);
    }

    public void ShowInvalid(bool on)
    {
        gameObject.SetActive(on);

        if (targetLight) targetLight.color = invalidColor;

        if (targetRenderer && targetRenderer.material)
        {
            if (on)
            {
                targetRenderer.material.EnableKeyword("_EMISSION");
                targetRenderer.material.SetColor("_EmissionColor", invalidColor);
            }
            else
            {
                
                targetRenderer.material.SetColor("_EmissionColor", Color.black);
            }
        }
    }
}