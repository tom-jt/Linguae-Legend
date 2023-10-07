using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartMenuBackgroundScript : BackgroundClass
{
    private int nextSpriteIndex = -1;

    [Header("Assignments")]
    [SerializeField]
    private Sprite[] startMenuSpriteSet;

    private void Start()
    {
        nextSpriteIndex = Random.Range(0, startMenuSpriteSet.Length);
        GetNextBackground();
    }

    private void GetNextBackground()
    {
        StartCoroutine(ImageTransition(startMenuSpriteSet[nextSpriteIndex]));
        nextSpriteIndex = (nextSpriteIndex >= startMenuSpriteSet.Length - 1) ? 0 : nextSpriteIndex + 1;
    }

    protected override IEnumerator ImageTransition(Sprite newImage)
    {
        StartCoroutine(base.ImageTransition(newImage));
        yield return null;
        StartCoroutine(BackgroundSlideshowDelay());
    }

    private IEnumerator BackgroundSlideshowDelay()
    {
        yield return new WaitForSeconds(cameraPanScript.panDuration);
        GetNextBackground();
    }
}
