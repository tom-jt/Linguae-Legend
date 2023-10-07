using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundDarkeningScript : MonoBehaviour
{
    [Header("Assignments")]
    [SerializeField]
    private Image darkenImage;
    [SerializeField]
    private EventManager eventScript;

    [Header("Cosmetics")]
    public float darkenDuration;
    [Range(0f, 1f)]
    public float imageDarkenPercentage;

    private void OnEnable()
    {
        EventManager.OverlayMenuToggle += BackgroundDarkening;
    }

    private void OnDisable()
    {
        EventManager.OverlayMenuToggle -= BackgroundDarkening;
    }

    private void BackgroundDarkening(bool toggleOn)
    {
        if (toggleOn)
            darkenImage.gameObject.SetActive(true);
        else
            StartCoroutine(DisableImage());

        float newAlpha = toggleOn ? imageDarkenPercentage : 0f;
        float currentAlpha = toggleOn ? 0f : imageDarkenPercentage;

        darkenImage.canvasRenderer.SetAlpha(currentAlpha);
        darkenImage.CrossFadeAlpha(newAlpha, darkenDuration, true);
    }

    private IEnumerator DisableImage()
    {
        yield return new WaitForSeconds(darkenDuration);
        darkenImage.gameObject.SetActive(false);
    }
}
