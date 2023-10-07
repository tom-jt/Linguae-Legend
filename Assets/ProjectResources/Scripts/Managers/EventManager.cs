using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//delegate type with null return value and requires a bool parameter
public delegate void BoolAction(bool success);

public class EventManager : MonoBehaviour
{
    //all events that can be subscribed to
    public static BoolAction WordGuessEnd;
    public static BoolAction RoundEnd;
    public static BoolAction PopupNotification;
    public static BoolAction OverlayMenuToggle;
    public static BoolAction GameEnd;
    public static BoolAction StartRound;
    public static BoolAction ColorblindToggle;

    //broadcast function invokes an event if it exists
    //object?.Function() only runs the function attached to the object if the object is not null
    public static void Broadcast(BoolAction evt, bool success) => evt?.Invoke(success);
}
