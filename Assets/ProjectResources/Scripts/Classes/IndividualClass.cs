using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IndividualClass : WordGameClass
{
    [HideInInspector]
    public BackColorTypes currentBoxBack = BackColorTypes.None;
    [HideInInspector]
    public Color originalBorderColor;
    [HideInInspector]
    public Color originalBackColor;
    [HideInInspector]
    public Color originalForeColor;

    [Header("Assignments")]
    [SerializeField]
    protected Sprite defaultSprite;

    protected virtual void OnEnable()
    {
        UpdateBorder(originalBorderColor = defaultBorderColor);
        UpdateBackImage(originalBackColor = ComparisonColors[BackColorTypes.None]);
    }

    public virtual void UpdateBorder(Color newColor, Sprite newSprite = null)
    {
        Image currentBorderImage = GetComponentsInChildren<Image>()[0];
        UpdateGraphics(currentBorderImage, newColor, newSprite);
    }

    public virtual void UpdateBackImage(Color newColor, Sprite newSprite = null)
    {
        Image currentBackImage = GetComponentsInChildren<Image>()[1];
        UpdateGraphics(currentBackImage, newColor, newSprite);
    }

    public virtual void UpdateForeImage(Color newColor, Sprite newSprite = null)
    { 
        Image currentForeImage = GetComponentsInChildren<Image>()[2];
        UpdateGraphics(currentForeImage, newColor, newSprite);
    }

    private void UpdateGraphics(Image imageToUpdate, Color newColor, Sprite newSprite = null)
    {
        if (imageToUpdate.color != newColor)
            imageToUpdate.color = newColor;

        if (newSprite)
            imageToUpdate.sprite = newSprite;
        else
            imageToUpdate.sprite = defaultSprite;
    }
}
