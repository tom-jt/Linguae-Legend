using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndMenuBackgroundScript : MonoBehaviour
{
    [Header("Assignments")]
    [SerializeField]
    private CameraPanScript cameraPanScript;
    [SerializeField]
    private Image backgroundImage;
    [SerializeField]
    private Image darkenImage;
    [SerializeField]
    private Sprite winSprite;
    [SerializeField]
    private Sprite loseSprite;

    [Header("Cosmetics")]
    [Range(0f, 1f)]
    public float backgroundDarkenPercent = 0.6f;

    private void Start()
    {
        darkenImage.canvasRenderer.SetAlpha(backgroundDarkenPercent);
        backgroundImage.sprite = EndMenuManager.wonGame ? winSprite : loseSprite;
        cameraPanScript.BeginObjectPan(backgroundImage.rectTransform, CameraPanTypes.ZoomOut);
    }
}
