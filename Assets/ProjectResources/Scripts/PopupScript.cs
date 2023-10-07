using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupScript : MonoBehaviour
{
    private void OnEnable()
    {
        EventManager.Broadcast(EventManager.PopupNotification, true);
    }

    private void OnDisable()
    {
        EventManager.Broadcast(EventManager.PopupNotification, false);
    }
}
