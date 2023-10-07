using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndividualSfxScript : MonoBehaviour
{
    private AudioSource sfxSource;
    private void OnEnable()
    {
        sfxSource = GetComponent<AudioSource>();
    }
    private void Update()
    {
        if (!sfxSource.isPlaying)
            Destroy(gameObject);
    }
}
