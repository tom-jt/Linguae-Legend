using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordGameClass : MonoBehaviour
{
    protected static AudioManager audioScript;
    protected static IndividualBoxScript[,] setOfBoxes;
    protected static int currentRow;
    protected static int currentInputBox;

    protected static Color defaultBorderColor = Constants.WhiteColor;
    protected static Color selectedBorderColor = Constants.LightGreyColor;
    protected static Color clickableBorderColor = Constants.YellowColor;

    public static int numberOfLettersInWord;
    public static int numberOfAttempts;
    public static string secretWord;

    public static int GetCurrentAttempt() => currentRow;

    //a protected function can only be accessed by itself and its subclasses
    //a virtual function can be overridden by its subclasses using the keyword 'override'
    protected virtual void Start()
    {
        //a global object is made to not deload between scenes
        audioScript = GameObject.FindGameObjectWithTag("GlobalObject").GetComponentInChildren<AudioManager>();
    }

    public static Dictionary<BackColorTypes, Color> ComparisonColors = new Dictionary<BackColorTypes, Color>()
    {
        { BackColorTypes.None, Constants.BlackColor },
        { BackColorTypes.Correct, Constants.GreenColor },
        { BackColorTypes.WrongPos, Constants.OrangeColor },
        { BackColorTypes.WrongLetter, Constants.GreyColor }
    };
}
