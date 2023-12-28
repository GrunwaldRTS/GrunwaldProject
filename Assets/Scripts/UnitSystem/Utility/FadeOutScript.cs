using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOutScript : MonoBehaviour
{
    public float fadeDuration = 2f;

    private Renderer objectRenderer;
    private Color initialColor;
    private float fadeTimer;
    private bool isFading = false;

    void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        initialColor = objectRenderer.material.color;
        isFading = true; // Start fading when the object is instantiated
    }

    void Update()
    {
        if (isFading)
        {
            FadeOut();
            Debug.Log("fading");
        }
    }

    void FadeOut()
    {
        fadeTimer += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeDuration);
        Debug.Log(fadeTimer);
        Color newColor = initialColor;
        newColor.a = alpha;

        objectRenderer.material.color = newColor;

        if (fadeTimer >= fadeDuration)
        {
            // Object has completely faded away, destroy it
            Destroy(gameObject);
        }
    }
}
