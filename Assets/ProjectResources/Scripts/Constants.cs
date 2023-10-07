using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants : MonoBehaviour
{
    //keyboard characters in the order of keyboard from left to right and top to bottom
    public static string keyboardChrs = "qwertyuiopasdfghjklzxcvbnm";

    public static Color OrangeColor = new Color32(195, 168, 45, 255);
    public static Color WarmOrangeColor = new Color32(195, 122, 45, 255);
    public static Color GreenColor = new Color32(63, 152, 55, 255);
    public static Color DarkGreenColor = new Color32(28, 111, 28, 255);
    public static Color GreyColor = new Color32(102, 102, 106, 255);
    public static Color LightGreyColor = new Color32(100, 100, 100, 255);
    public static Color WhiteColor = new Color32(229, 229, 229, 255);
    public static Color BlackColor = new Color32(18, 18, 18, 255);
    public static Color RedColor = new Color32(242, 28, 30, 255);
    public static Color DarkRedColor = new Color32(116, 23, 13, 255);
    public static Color BlueColor = new Color32(0, 30, 191, 255);
    public static Color LightBlueColor = new Color32(25, 186, 212, 255);
    public static Color YellowColor = new Color32(243, 204, 20, 255);
    public static Color PurpleColor = new Color32(144, 30, 210, 255);
    public static Color PinkColor = new Color32(255, 110, 220, 255);
    public static Color TransparentColor = new Color32(0, 0, 0, 0);

    public static Color GetTransparentColor(Color color, float newAlpha) => new Color(color.r, color.g, color.b, newAlpha);

    public static string StartMenuScene = "StartMenu";
    public static string MainGameScene = "MainGame";
    public static string EndMenuScene = "EndMenu";
    
    public static int WeightedRandomiser(int[] rarity)
    {
        //finds the total rarity within the array
        int totalRarity = 0;

        foreach (int item in rarity)
            totalRarity += item;

        //generates a random number within the totalRarity
        int randomRarity = Random.Range(0, totalRarity) + 1;
        int accumulativeRarity = 0;

        //accumulates the rarities
        //then outputs the index of the first rarity value that crosses over the randomly generated number
        for (int item = 0; item < rarity.Length; item++)
        {
            accumulativeRarity += rarity[item];

            if (accumulativeRarity >= randomRarity)
                return item;
        }
        return -1;
    }

    public static int BinarySearch(string wordToSearch, string[] searchInList)
    {
        //standard binary search algorithm, only works with a sorted list
        int lowerBound = 0;
        int upperBound = searchInList.Length - 1;
        int comparePosition;
        string compareTo;

        while (lowerBound <= upperBound)
        {
            //divides the array in half each time and checks the middle element
            //then look at the lower or top half of the list depending on the relative position
            //of the middle element to the search term
            comparePosition = (upperBound + lowerBound) / 2;

            compareTo = searchInList[comparePosition];

            if (string.Compare(wordToSearch, compareTo) == 0)
                return comparePosition;
            else if (string.Compare(wordToSearch, compareTo) > 0)
                lowerBound = comparePosition + 1;
            else
                upperBound = comparePosition - 1;
        }

        return -1;
    }

    public static int LinearSearch(string wordToSearch, string[] searchInList, int[] removeLetters = null)
    {
        if (removeLetters != null)
        {
            //removes letters at those positions
            wordToSearch = RemoveLettersInString(wordToSearch, removeLetters);

            //if the resultant length is 1 or less, then automatically return a 'true' value
            if (wordToSearch.Length <= 1)
                return 0;
        }

        //otherwise, loop through the word list
        //compare each word by removing letters from the same positions
        //E.g.
        //if the guessed word is HE!LO
        //then it would become HELO
        //then it would be a valid word since HELLO with the third letter removed is also HELO 
        string compareTo;
        for (int wordIndex = 0; wordIndex < searchInList.Length; wordIndex++)
        {
            compareTo = searchInList[wordIndex];

            if (removeLetters != null)
                compareTo = RemoveLettersInString(compareTo, removeLetters);

            if (wordToSearch == compareTo)
                return wordIndex;
        }

        return -1;
    }

    public static string RemoveLettersInString(string oldStr, int[] removeLettersAtIndex)
    {
        //loops through the string and removes the letter at the specified positions
        for (int index = 0; index < removeLettersAtIndex.Length; index++)
            oldStr = oldStr.Remove(removeLettersAtIndex[index] - index, 1);
        return oldStr;
    }

    public static string AddSpacesToString(string oldStr)
    {
        //creates a new string
        //and inserts a space before every capital letter
        //except at the first letter
        string newString = "";

        for (int letter = 0; letter < oldStr.Length; letter++)
        {
            if ((char.IsUpper(oldStr[letter])) && (letter != 0))
            {
                newString += " ";
            }
            newString += oldStr[letter];
        }

        return newString;
    }
}
