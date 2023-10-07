using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ColorBlindMode
{
    Normal = 0,
    Protanopia = 1,
    Protanomaly = 2,
    Deuteranopia = 3,
    Deuteranomaly = 4,
    Tritanopia = 5,
    Tritanomaly = 6,
    Achromatopsia = 7,
    Achromatomaly = 8,
}

[ExecuteInEditMode]
public class ColorBlindFilter : MonoBehaviour
{
    public static ColorBlindMode mode = ColorBlindMode.Normal;
    public static ColorBlindMode previousMode = ColorBlindMode.Normal;

    public bool showDifference = false;
    public static bool simulateColorBlindness = false;

    private Material material;

    private static readonly Color[,] RGB =
    {
        { new Color(1f,0f,0f),   new Color(0f,1f,0f), new Color(0f,0f,1f) },    // Normal
        { new Color(.56667f, .43333f, 0f), new Color(.55833f, .44167f, 0f), new Color(0f, .24167f, .75833f) },    // Protanopia
        { new Color(.81667f, .18333f, 0f), new Color(.33333f, .66667f, 0f), new Color(0f, .125f, .875f)    }, // Protanomaly
        { new Color(.625f, .375f, 0f), new Color(.70f, .30f, 0f), new Color(0f, .30f, .70f)    },   // Deuteranopia
        { new Color(.80f, .20f, 0f), new Color(.25833f, .74167f, 0f), new Color(0f, .14167f, .85833f)    },    // Deuteranomaly
        { new Color(.95f, .05f, 0f), new Color(0f, .43333f, .56667f), new Color(0f, .475f, .525f) }, // Tritanopia
        { new Color(.96667f, .03333f, 0), new Color(0f, .73333f, .26667f), new Color(0f, .18333f, .81667f) }, // Tritanomaly
        { new Color(.299f, .587f, .114f), new Color(.299f, .587f, .114f), new Color(.299f, .587f, .114f)  },   // Achromatopsia
        { new Color(.618f, .32f, .062f), new Color(.163f, .775f, .062f), new Color(.163f, .320f, .516f)  }    // Achromatomaly
    };

    private static readonly Color[,] RGB2 =
{
        { new Color(1f,0f,0f),   new Color(0f,1f,0f), new Color(0f,0f,1f) },    // Normal
        { new Color(1f, 0f, 0f), new Color(0f, 0f, 1f), new Color(0f, 0f, 1f) },    // Protanopia
        { new Color(.7f, 0f, .7f), new Color(0f, 1f, .4f), new Color(.4f, 0f, 1f)    }, // Protanomaly
        { new Color(1f, 0f, 0f), new Color(0f, 0f, 1f), new Color(.4f, .4f, .2f)    },   // Deuteranopia
        { new Color(.72531f, 0f, .27469f), new Color(.35116f, .64884f, 0f), new Color(0f, 0f, 1f)    },    // Deuteranomaly
        { new Color(1f, 0f, 0f), new Color(0f, 1f, 0f), new Color(.5f, 0f, .5f) }, // Tritanopia
        { new Color(1f, .2f, 0f), new Color(0f, 1f, .2f), new Color(.2f, 0f, 1f) }, // Tritanomaly
        { new Color(.5f, 0f, 0f), new Color(0f, 1f, 0f), new Color(1f, 0f, 1f)  },   // Achromatopsia
        { new Color(1f,0f,0f),   new Color(.15f,.7f,.15f), new Color(0f,.3f,.7f)  }    // Achromatomaly
    };

    private void Awake()
    {
        material = new Material(Shader.Find("Hidden/ChannelMixer"));
        RemapColors();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // No effect
        if (mode == ColorBlindMode.Normal)
        {
            Graphics.Blit(source, destination);
            return;
        }

        // Change effect
        if (mode != previousMode)
        {
            RemapColors();
            previousMode = mode;
        }

        // Apply effect
        Graphics.Blit(source, destination, material, showDifference ? 1 : 0);
    }

    private void OnEnable()
    {
        EventManager.ColorblindToggle += ChangeFilterMode;
    }

    private void OnDisable()
    {
        EventManager.ColorblindToggle -= ChangeFilterMode;
    }

    private void ChangeFilterMode(bool changeToSimulate)
    {
        simulateColorBlindness = changeToSimulate;
        RemapColors();
    }

    private void RemapColors()
    {
        Color[,] filterToUse = simulateColorBlindness ? RGB : RGB2;
        material.SetColor("_R", filterToUse[(int)mode, 0]);
        material.SetColor("_G", filterToUse[(int)mode, 1]);
        material.SetColor("_B", filterToUse[(int)mode, 2]);
    }
}