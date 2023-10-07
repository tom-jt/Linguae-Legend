using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecretMenuScript : MonoBehaviour
{
    public static bool forceCheatMenu;

    private AudioManager audioScript;

    private void Start()
    {
        audioScript = GameObject.FindGameObjectWithTag("GlobalObject").GetComponentInChildren<AudioManager>();
        forceCheatMenu = false;
    }

    private void Update()
    {
        if ((Input.GetKey(KeyCode.Tab)) && (Input.GetKeyUp(KeyCode.DownArrow)))
        {
            forceCheatMenu = !forceCheatMenu;
            audioScript.CreateSfxInstance(audioScript.boxSelect);
        }
    }
}
