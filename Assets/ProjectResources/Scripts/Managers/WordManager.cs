using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum BackColorTypes
{
    None = 0,
    Correct = 1,
    WrongPos = 2,
    WrongLetter = 3
}

public class WordManager : WordGameClass
{
    [Header("Assignments")]
    [SerializeField]
    private LetterBoxManager letterBoxScript;
    [SerializeField]
    private DisplayMessageManager messageScript;

    private string guessedWord;

    private void OnEnable()
    {
        EventManager.WordGuessEnd += TryBeginNextGuess;
        EventManager.RoundEnd += TriggerBannerMessage;
        EventManager.GameEnd += TriggerGameLoseBannerMessage;
    }

    private void OnDisable()
    {
        EventManager.WordGuessEnd -= TryBeginNextGuess;
        EventManager.RoundEnd -= TriggerBannerMessage;
        EventManager.GameEnd -= TriggerGameLoseBannerMessage;
    }

    //randomly gets a word from the word database
    //NOTE that secretWord is initialised in the WordGameClass and is static
    public void GenerateSecretWord(bool tryUseCommonList) => secretWord = WordDatabaseManager.GetSecretWord(tryUseCommonList);

    //when a guess is submitted
    public void InitiateWordEntered()
    {
        //display message: not enough letters entered
        if (currentInputBox != numberOfLettersInWord)
        {
            messageScript.CreatePopup("Not Enough Letters");
            letterBoxScript.AnimateRowShake();
            return;
        }

        guessedWord = "";
        string currentLetter;
        List<int> removeLetters = new List<int>();

        //loops through each letter in the row and appends it to the guessedWord
        //however, empty boxes or a box effect inputs "!" instead
        //the removeLetters list stores the positions of all letters that have been replaced with "!"
        //this is used later
        for (int letter = 0; letter < numberOfLettersInWord; letter++)
        {
            currentLetter = setOfBoxes[currentRow, letter].GetText();
            if ((setOfBoxes[currentRow, letter].currentBoxEffect == EffectTypes.Chaos) || (string.IsNullOrEmpty(currentLetter)))
            {
                removeLetters.Add(letter);
                if (string.IsNullOrEmpty(currentLetter))
                    currentLetter = "!";
            }
            guessedWord += currentLetter.ToLower();
        }

        //if some letters are removed, then convert the list into an array
        int[] removeLettersArray = removeLetters.Count == 0 ? null : removeLetters.ToArray();

        //CheckIfValidWord() returns a boolean that indicates if the word is valid or not,
        //then run the appropriate function
        if (WordDatabaseManager.CheckIfValidWord(guessedWord, removeLettersArray))
            ValidWordEntered();
        else
            InvalidWordEntered();
    }

    private void InvalidWordEntered()
    {
        //display message: invalid word entered
        messageScript.CreatePopup("Invalid Word");
        letterBoxScript.AnimateRowShake();
    }

    private void ValidWordEntered()
    {
        //stops player input and start revealing the box colors
        //the colors are supplied through the returned array in CompareWord()
        GameFlowManager.enablePlayerLetterInput = false;
        StartCoroutine(letterBoxScript.RevealBoxColors(guessedWord, CompareWord()));
    }
    
    private BackColorTypes[] CompareWord()
    {
        BackColorTypes[] letterCorrectness = new BackColorTypes[numberOfLettersInWord];
        //this array determines if the letter at that position is guessed correctly
        //WrongLetter - letter is not in the secret word
        //WrongPos - letter is in the secret word but in a different position
        //Correct - letter is in the correct position in the secret word

        //if a letter in the secret word has been compared already, it cannot be compared again
        //E.g. if the secret word is HELLO
        //and the guess is BEVEL
        //then the first 'E' would turn green
        //and the second would turn grey because there are no other 'E's to compare
        bool[] alreadyMatched = new bool[secretWord.Length];

        //first loop checks for Correct letters
        for (int letter = 0; letter < secretWord.Length; letter++)
        {
            if (guessedWord[letter] == secretWord[letter])
            {
                alreadyMatched[letter] = true;
                letterCorrectness[letter] = BackColorTypes.Correct;
            }
        }

        //second loop checks for WrongLetter and WrongPos letters
        for (int letter = 0; letter < secretWord.Length; letter++)
        {
            //skips the ones that are already matching the secret word in the correct position
            if (guessedWord[letter] != secretWord[letter])
            {
                int letterInSecret = 0;
                bool foundMatch = false;
                do
                {
                    //loops through each letter in the secret word
                    //each letter in secret word is compared to the letter in guessed word
                    //if it is not already used previously, and matches the letter in guessed word
                    //then change the guessed letter to WrongPosition
                    if ((!alreadyMatched[letterInSecret]) &&
                        (guessedWord[letter] == secretWord[letterInSecret]))
                    {
                        letterCorrectness[letter] = BackColorTypes.WrongPos;
                        foundMatch = true;
                    }
                    letterInSecret++;
                } while ((letterInSecret < secretWord.Length) && (!foundMatch));

                //if no letter in secret was matched with the guessed letter
                //otherwise, mark the letter in secret word as already matched
                if (!foundMatch)
                    letterCorrectness[letter] = BackColorTypes.WrongLetter;
                else
                    alreadyMatched[letterInSecret - 1] = true;
            }
        }

        return letterCorrectness;
    }

    //possible outcomes:
    //guessed right - move onto next round
    //guessed wrong - try to begin next guess
    public void CheckGameProgression() => OnWordGuessEnd(guessedWord == secretWord);

    //standard practice to broadcast events in protected virtual functions (explained in WordGameClass)
    protected virtual void OnWordGuessEnd(bool success) => EventManager.Broadcast(EventManager.WordGuessEnd, success);
    protected virtual void OnRoundEnd(bool success) => EventManager.Broadcast(EventManager.RoundEnd, success);

    //subscribed to WordGuessEnd
    private void TryBeginNextGuess(bool endRound)
    {
        //no attempts left - move onto next round
        if ((endRound) || (currentRow >= numberOfAttempts - 1))
        {
            OnRoundEnd(endRound);
            return;
        }

        //otherwise, move onto a new row and allow the user to continue the game
        currentRow++;
        letterBoxScript.FindNextVacantBox(0, false);

        GameFlowManager.enablePlayerLetterInput = true;
    }

    //subscribed to RoundEnd
    private void TriggerBannerMessage(bool wonRound)
    {
        string message;

        //if the player wins the round, generate a message based on how many attempts it took them
        if (wonRound)
        {
            float attemptsRatio = (float)numberOfAttempts / 4;
            message = (currentRow + 1) switch
            {
                int n when (n <= attemptsRatio) => "Legendary!",
                int n when (n <= attemptsRatio * 2) => "Fantastic!",
                int n when (n <= attemptsRatio * 3) => "Great!",
                int n when (n <= attemptsRatio * 4) => "Good.",
                _ => "Unintended"
            };
        }
        else
        {
            //if the player fails the round, the secret word is revealed
            message = "You Failed. The Word was " + (char.ToUpper(secretWord[0]) + secretWord.Substring(1)) + ".";
        }

        messageScript.CreateBanner(message);
    }

    //subscribed to GameEnd
    private void TriggerGameLoseBannerMessage(bool wonGame) 
    {
        if (!wonGame) 
        {
            string message = "You Failed. The Word was " + (char.ToUpper(secretWord[0]) + secretWord.Substring(1)) + ".";
            messageScript.CreateBanner(message);
        }
    }
}