using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LingeringEffectsManager : MonoBehaviour
{
    public static int extraAttempts = 0;
    public static int extraObstacles = 0;
    public static int extraBenefits = 0;
    public static int presetNumberOfLetters = 0;

    public static void CheckLingeringEffects()
    {
        //add extra attempts or presets the number of letters in the next secret word

        WordGameClass.numberOfAttempts = WordGameClass.numberOfAttempts + extraAttempts;

        if (presetNumberOfLetters > 0)
            WordGameClass.numberOfLettersInWord = presetNumberOfLetters;
    }

    public static void ResetLingeringEffects()
    {
        //resets all variables
        extraAttempts = 0;
        extraObstacles = 0;
        extraBenefits = 0;
        presetNumberOfLetters = 0;
    }
}
