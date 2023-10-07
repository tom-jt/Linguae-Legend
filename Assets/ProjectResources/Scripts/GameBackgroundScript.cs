using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameBackgroundScript : BackgroundClass
{
    private int currentSpriteIndex = -1;

    [Header("Assignments")]
    [SerializeField]
    private Sprite[] backgroundSpriteSet;
    [SerializeField]
    private int[,] adventureSpriteIndex = new int[3, 15] 
    {
        //easy mode
        {0, 1, 2, 3, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        //normal mode
        {5, 6, 7, 8, 9, 10, 11, 12, 13, 14, -1, -1, -1, -1, -1},
        //hard mdoe
        {15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29},
    };

    public void SetupBackground(int round) =>
        StartCoroutine(ImageTransition(
            GameFlowManager.gameMode == GameModeTypes.Adventure ? 
            GetAdventureBackground(round) : 
            GetRandomBackground()));
    
    private Sprite GetAdventureBackground(int round)
    {
        int spriteIndex = adventureSpriteIndex[(int)GameFlowManager.difficulty, round - 1];
        return (spriteIndex != -1) ? backgroundSpriteSet[spriteIndex] : null;
    }

    private Sprite GetRandomBackground()
    {
        int spriteIndex;
        int repeatCount = 0;
        do
        {
            spriteIndex = Random.Range(0, backgroundSpriteSet.Length);
            repeatCount++;
        } while ((spriteIndex == currentSpriteIndex) && (repeatCount < 100));

        currentSpriteIndex = spriteIndex;

        return backgroundSpriteSet[spriteIndex];
    }
}
