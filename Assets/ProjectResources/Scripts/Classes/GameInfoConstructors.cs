using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//these are constructors
//really similar to records
//I.e. arrays where each cell is comprised of several data types and variables
//especially useful for efficiently creating new powerups, ephemerals, or box effects
//this is for the basic save data
[System.Serializable]
public class SaveDataConstructor
{
    public int advDiffModes;
    public float sfxVolume;
    public float bgVolume;
    public int colorblindFilter;
    public int boxAnimationStyle;
    public bool profanityToggle;
    public int gamePlaylistIndex;
    public int bossPlaylistIndex;

    public SaveDataConstructor(int adv, float sfx, float bg, int colorFilter, int boxStyle, bool profanity, int gamePlaylist, int bossPlaylist)
    {
        advDiffModes = adv;
        sfxVolume = sfx;
        bgVolume = bg;
        colorblindFilter = colorFilter;
        boxAnimationStyle = boxStyle;
        profanityToggle = profanity;
        gamePlaylistIndex = gamePlaylist;
        bossPlaylistIndex = bossPlaylist;
    }
}

[System.Serializable]
public class PowerupConstructor 
{
    public string name;
    public string functionName;
    [TextArea(5, 10)]
    public string description;
    public Sprite icon;
    public Sprite pixelImage;
    public int rarity;
    public int amountOwned;
}

[System.Serializable]
public class EphemeralConstructor
{
    public string name;
    [TextArea(5, 10)]
    public string description;
    public Sprite pixelImage;
    public int rarity;
}

[System.Serializable]
public class BoxEffectConstructor
{
    //obstacles, index -1 and below
    //helpful effects, index 1 and above
    public int index;

    public string name;
    public string functionName;
    [TextArea(5, 10)]
    public string description;
    public Color boxColor;
    public int bottomRowRestriction;
    public Sprite helpImage;
    public int rarity;
}
