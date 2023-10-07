using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalObjectScript : MonoBehaviour
{
    private void Awake()
    {
        if (GameObject.FindGameObjectsWithTag("GlobalObject").Length <= 1)
            DontDestroyOnLoad(gameObject);
        else
            Destroy(gameObject);
    }
}
