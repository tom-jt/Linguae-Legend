using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CameraPanTypes
{
    Vertical,
    Horizontal,
    Diagonal,
    ZoomIn,
    ZoomOut,
    Circular
}

public class CameraPanScript : MonoBehaviour
{
    private bool isPanning = false;
    private bool reverseMotion = false;
    private RectTransform objectTransform;
    private CameraPanTypes cameraPanType;
    private float timeSinceBeginPan;

    private float newYPos;
    private float newXPos;
    private float newWidth;
    private float newHeight;

    [Header("Cosmetics")]
    public float panDuration = 60f;
    public bool doReverseMotion = true;

    public void BeginObjectPan(RectTransform objectToPan, CameraPanTypes panType)
    {
        objectTransform = objectToPan;
        cameraPanType = panType;
        ChangePanState(true);

        timeSinceBeginPan = Time.time;
    }

    public void ChangePanState(bool newState) => isPanning = newState;

    private void LateUpdate()
    {
        if (isPanning)
        {
            float timeRatio = (Time.time - timeSinceBeginPan) / panDuration;

            if (doReverseMotion)
            {
                if (reverseMotion)
                    timeRatio = 1 - timeRatio;

                if ((timeRatio <= 0) && (reverseMotion))
                {
                    reverseMotion = false;
                    timeSinceBeginPan = Time.time;
                }
                else if ((timeRatio >= 1) && (!reverseMotion))
                {
                    reverseMotion = true;
                    timeSinceBeginPan = Time.time;
                }
            }

            switch (cameraPanType)
            {
                case CameraPanTypes.Vertical:
                    newYPos = Mathf.Lerp(270, -270, timeRatio);
                    objectTransform.Translate(0, newYPos - objectTransform.localPosition.y, 0);
                    break;
                case CameraPanTypes.Horizontal:
                    newXPos = Mathf.Lerp(480, -480, timeRatio);
                    objectTransform.Translate(newXPos - objectTransform.localPosition.x, 0, 0);
                    break;
                case CameraPanTypes.Diagonal:
                    newYPos = Mathf.Lerp(270, -270, timeRatio);
                    newXPos = Mathf.Lerp(480, -480, timeRatio);
                    objectTransform.Translate(newXPos - objectTransform.localPosition.x, newYPos - objectTransform.localPosition.y, 0);
                    break;
                case CameraPanTypes.ZoomIn:
                    newWidth = Mathf.Lerp(2880, 4320, timeRatio);
                    newHeight = Mathf.Lerp(1620, 2430, timeRatio);
                    objectTransform.sizeDelta = new Vector2(newWidth, newHeight);
                    break;
                case CameraPanTypes.ZoomOut:
                    newWidth = Mathf.Lerp(2880, 1920, timeRatio);
                    newHeight = Mathf.Lerp(1620, 1080, timeRatio);
                    objectTransform.sizeDelta = new Vector2(newWidth, newHeight);
                    break;
                case CameraPanTypes.Circular:
                    newXPos = 200 * Mathf.Cos(2 * Mathf.PI * timeRatio);
                    newYPos = 150 * Mathf.Sin(2 * Mathf.PI * timeRatio);
                    objectTransform.Translate(newXPos - objectTransform.localPosition.x, newYPos - objectTransform.localPosition.y, 0);
                    break;
            }
        }
    }
}
