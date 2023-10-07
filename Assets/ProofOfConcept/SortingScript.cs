using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SortingScript : MonoBehaviour
{
    //objects in the graphical interface assigned to variables
    public Text inputFieldText;
    public RectTransform indicatorArrow;
    public GameObject sortingBoxesRoot;

    int currentBox = 0;

    //arrays
    Text[] sortingBoxes;

    void Start()
    {
        //find the text boxes so we can edit and update the text in the graphical interface
        sortingBoxes = sortingBoxesRoot.GetComponentsInChildren<Text>();
    }

    public void OnSortButtonClicked() //runs this function when the button is clicked
    {
        //standard bubble sort

        int loopNumber = 0;
        bool swapped = true;

        while (loopNumber < sortingBoxes.Length - 1 && swapped)
        {
            swapped = false;

            //parse converts a string into an integer, that's literally it
            int largestNumber = int.Parse(sortingBoxes[0].text); 

            //for loops: (initial value; condition; step)
            for (int box = 1; box < sortingBoxes.Length - loopNumber; box++)
            {
                int swapWith = int.Parse(sortingBoxes[box].text);

                if (largestNumber > swapWith)
                {
                    sortingBoxes[box].text = largestNumber.ToString();
                    sortingBoxes[box - 1].text = swapWith.ToString();

                    swapped = true;
                }
                else
                {
                    largestNumber = swapWith;
                }
            }

            loopNumber++; //i++ is effectively i = i + 1
        }
    }

    public void OnValueEntered() //function runs when a number is inputted by the user
    {
        //if nothing is inputted, then do nothing
        if (string.IsNullOrEmpty(inputFieldText.text))
            return;

        //sets the correct box with the number the user inputted
        sortingBoxes[currentBox].text = inputFieldText.text;

        //changes the box to input to
        if (currentBox < sortingBoxes.Length - 1)
            currentBox++;
        else
            currentBox = 0;

        //graphically updates the position of the arrow
        //so its underneath the box the user will input into
        Vector3 newPosition = indicatorArrow.position;
        newPosition.x = 260 * currentBox + 305;
        indicatorArrow.position = newPosition;
    }
}
