using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndividualCutsceneScript : MonoBehaviour
{
    //all cutscenes scripts are separate game objects assigned in the inspector
    //they dictate the contents of each cutscene
    public DifficultyTypes csDifficulty;
    public int csRound;

    public int[] dialogueIndex;
    public CameraPanTypes[] dialogueCameraPans;
    public Sprite[] dialogueSprites;

    [TextArea(5, 10)]
    public string[] dialogue;
}
