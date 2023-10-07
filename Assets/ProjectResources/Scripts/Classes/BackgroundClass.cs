using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundClass : MonoBehaviour
{
    [Header("General Assignments")]
    [SerializeField]
    protected CameraPanScript cameraPanScript;
    [SerializeField]
    protected Image backgroundImage;
    [SerializeField]
    protected Image darkenImage;

    [Header("General Cosmetics")]
    public float backgroundDarkenDelay = 0.5f;
    [Range(0f, 1f)]
    public float backgroundDarkenPercent = 0.6f;
    public float defaultWidth = 2880;
    public float defaultHeight = 1620;

    protected virtual IEnumerator ImageTransition(Sprite newImage)
    {
        darkenImage.CrossFadeAlpha(1f, backgroundDarkenDelay, false);
        yield return new WaitForSeconds(backgroundDarkenDelay);

        int randomPanType = Random.Range(0, System.Enum.GetValues(typeof(CameraPanTypes)).Length);
        backgroundImage.sprite = newImage;
        ResetImage();
        cameraPanScript.BeginObjectPan(backgroundImage.rectTransform, (CameraPanTypes)randomPanType);

        darkenImage.CrossFadeAlpha(backgroundDarkenPercent, backgroundDarkenDelay, false);
    }

    protected virtual void ResetImage()
    {
        backgroundImage.rectTransform.localPosition = new Vector3(0, 0, 0);
        backgroundImage.rectTransform.sizeDelta = new Vector2(defaultWidth, defaultHeight);
    }
}
