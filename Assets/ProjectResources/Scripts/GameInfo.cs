using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class GameInfo : MonoBehaviour
{
    //player's save, the parameters in the inspector are default values when save file not found
    public SaveDataConstructor playerSave;

    public static PowerupConstructor[] Powerups;
    public static int[] PowerupRarities;

    public static EphemeralConstructor[] Ephemerals;
    public static int[] EphemeralRarities;

    public static Dictionary<int, BoxEffectConstructor> BoxEffects;
    public static int[] ObstacleRarities;
    public static int[] BenefitRarities;

    [Header("Assignments")]
    [SerializeField]
    private AudioManager audioScript;

    //every powerup, ephemeral event, and box effect are created and assigned in the inspector
    public PowerupConstructor[] assignPowerups;
    public EphemeralConstructor[] assignEphemerals;
    public BoxEffectConstructor[] assignBoxEffects;

    private void Start()
    {
        AssignGameInfo();
        LoadSave();
    }

    private void AssignGameInfo()
    {
        //transfer the assigned variables to static arrays/Dictionary that any script can access
        //also transfers the rarities of each into their respective arrays for weighted randomisation

        Powerups = assignPowerups;
        PowerupRarities = new int[Powerups.Length];
        for (int powerupID = 0; powerupID < Powerups.Length; powerupID++)
            PowerupRarities[powerupID] = Powerups[powerupID].rarity;

        Ephemerals = assignEphemerals;
        EphemeralRarities = new int[Ephemerals.Length];
        for (int ephemeralID = 0; ephemeralID < Ephemerals.Length; ephemeralID++)
            EphemeralRarities[ephemeralID] = Ephemerals[ephemeralID].rarity;

        BoxEffects = new Dictionary<int, BoxEffectConstructor>();
        List<int> tempObstacleRarity = new List<int>();
        List<int> tempBenefitRarity = new List<int>();
        for (int boxEffectID = 0; boxEffectID < assignBoxEffects.Length; boxEffectID++)
        {
            BoxEffectConstructor indvBoxEffect = assignBoxEffects[boxEffectID];
            BoxEffects.Add(indvBoxEffect.index, indvBoxEffect);

            if (indvBoxEffect.index > 0)
                tempBenefitRarity.Add(indvBoxEffect.rarity);
            else if (indvBoxEffect.index < 0)
                tempObstacleRarity.Add(indvBoxEffect.rarity);
        }
        ObstacleRarities = tempObstacleRarity.ToArray();
        BenefitRarities = tempBenefitRarity.ToArray();
    }

    private void LoadSave()
    {
        //read the binary file and deserialises it into a readable contructor format
        //then apply the variables within the game
        string path = Application.streamingAssetsPath + "/save.txt";

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            playerSave = (SaveDataConstructor)formatter.Deserialize(stream);

            stream.Close();
        }

        audioScript.ChangeMasterVolume(playerSave.sfxVolume);
        audioScript.ChangeBackgroundMusicVolume(playerSave.bgVolume);
        ColorBlindFilter.mode = (ColorBlindMode)playerSave.colorblindFilter;
        GameFlowManager.boxAnimStyle = (BoxFlipStyles)playerSave.boxAnimationStyle;
        GameFlowManager.useProfanityFilter = playerSave.profanityToggle;
        AudioManager.currentBossPlaylistIndex = playerSave.bossPlaylistIndex;
        AudioManager.currentGamePlaylistIndex = playerSave.gamePlaylistIndex;
    }

    private void OnApplicationQuit()
    {
        playerSave = new SaveDataConstructor(playerSave.advDiffModes, audioScript.newMasterVolume, 
            audioScript.GetBackgroundMusicVolume(), (int)ColorBlindFilter.mode, 
            (int)GameFlowManager.boxAnimStyle, GameFlowManager.useProfanityFilter,
            AudioManager.currentGamePlaylistIndex, AudioManager.currentBossPlaylistIndex);

        SaveGame();
    }

    private void SaveGame()
    {
        //saves the current variables such as volume to a save file, and serialises it to binary
        string path = Application.streamingAssetsPath + "/save.txt";

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, playerSave);

        stream.Close();
    }
}
