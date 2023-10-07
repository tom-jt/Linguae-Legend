using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class WordDatabaseManager : MonoBehaviour
{
    private static string[] totalWordList;
    private static string[] profanityWordList;
    private static string[] currentWordList;
    private static string[] currentWordListCommon;

    private void Awake()
    {
        SeparateWordLists();
    }

    private void SeparateWordLists()
    {
        //get the word list with every word of length 2-15
        totalWordList = ReadFromWordList(Application.streamingAssetsPath + "/WordLists/TotalWordList.txt");

        for (int wordLength = 2; wordLength <= 15; wordLength++)
        {
            //loops through each word length, then create a separate wordlist if it does not exist already
            string wordListPath = Application.streamingAssetsPath + "/WordLists/" + wordLength + "WordList.txt";
            if (!File.Exists(wordListPath))
                WriteIntoWordList(wordListPath, wordLength);
        }
    }

    private static string[] ReadFromWordList(string path)
    {
        //read all text in a file
        string stringOfWords = File.ReadAllText(path).ToLower();
        //split the text into an array of string, separated by new lines (\n and \r\n are new line indicators)
        //also removes amy empty cells
        return stringOfWords.Split(new string[] { "\n", "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
    }

    private void WriteIntoWordList(string path, int lengthOfWord)
    {
        //writes into a text file at a specified path
        StreamWriter streamWriter = new StreamWriter(path, true);

        //loops through the total word list, then write into the file any words that have a specified word length
        for (int currentWord = 0; currentWord < totalWordList.Length; currentWord++)
            if (totalWordList[currentWord].Length == lengthOfWord)
                streamWriter.WriteLine(totalWordList[currentWord]);

        streamWriter.Close();
    }

    public static void GetProfanityList()
    {
        //try to find and store the profanity word list
        string profanityWordListPath = Application.streamingAssetsPath + "/WordLists/ProfanityWordList.txt";

        if (File.Exists(profanityWordListPath))
            profanityWordList = ReadFromWordList(profanityWordListPath);
    }

    public static void GetWordListForValidCheck(int wordLength)
    {
        currentWordList = null;
        currentWordListCommon = null;

        //retrieves the word list with a specific length
        string currentWordListPath = Application.streamingAssetsPath + "/WordLists/" + wordLength + "WordList.txt";
        if (File.Exists(currentWordListPath))
            currentWordList = ReadFromWordList(currentWordListPath);

        //if there is a list of common words, then retrieve it also
        //the common word list are words frequently used
        //it is used to generate secret words for easy and normal modes
        string commonWordListPath = Application.streamingAssetsPath + "/CommonWordLists/" + wordLength + "WordListCommon.txt";
        if (File.Exists(commonWordListPath))
            currentWordListCommon = ReadFromWordList(commonWordListPath);
    }

    public static string GetSecretWord(bool tryUseCommonList)
    {
        //use either the common or full word list
        string[] listToUse = (tryUseCommonList && currentWordListCommon != null) ? currentWordListCommon : currentWordList;

        //randomly picks an index
        int randomWordIndex = Random.Range(0, listToUse.Length);

        //if a profanity filter is used
        //check if the randomly selected word is vulgar through a binary search
        //if so, then reselect the word until it is accepted
        if (GameFlowManager.useProfanityFilter)
            while (Constants.BinarySearch(listToUse[randomWordIndex], profanityWordList) != -1)
                randomWordIndex = Random.Range(0, listToUse.Length);

        //returns the randomly generated word
        return listToUse[randomWordIndex];
    }

    public static bool CheckIfValidWord(string wordToCheck, int[] removeLetters = null)
    {
        //return values:
        //false - not a valid word
        //true - is a valid word

        //if the profanity filter is on, then check if the inputted guess is vulgar through a binary search
        if (GameFlowManager.useProfanityFilter)
            if (Constants.BinarySearch(wordToCheck, profanityWordList) != -1)
                return false;
        
        //if no letters were removed, then do a binary search through the full word list of the same length
        //if words are removed, do a modified linear search through the list (explained in the Constants script)
        //both searches return the found position or -1 if not found
        if (removeLetters != null)
            return Constants.LinearSearch(wordToCheck, currentWordList, removeLetters) != -1;
        else
            return Constants.BinarySearch(wordToCheck, currentWordList) != -1;
    }
}
