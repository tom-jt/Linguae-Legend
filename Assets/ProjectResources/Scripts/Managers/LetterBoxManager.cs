using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//subclass of the WordGameClass, and can access all of the variables and functions within that class
public class LetterBoxManager : WordGameClass
{
    private Transform[] emptyRowTransform;

    [Header("Assignments")]
    [SerializeField]
    private KeyBoardManager keyboardScript;
    [SerializeField]
    private WordManager wordScript;
    [SerializeField]
    private HealthManager healthScript;
    [SerializeField]
    private BoxEffectManager effectScript;
    [SerializeField]
    private GameObject letterBoxPrefab;
    [SerializeField]
    private GameObject emptyRowPrefab;

    [Header("Cosmetics")]
    public float boxWidth = 125;
    public float boxHeight = 125;
    public float gridSpacing = 10;
    public float boxColorRevealInterval = 0.2f;
    public float boxRemoveInterval = 0.1f;
    public float boxRemoveAnimLength = 0.5f;

    public void SetupBoxes()
    {
        //initialise new empty arrays with length and width equal to the grid size
        setOfBoxes = new IndividualBoxScript[numberOfAttempts, numberOfLettersInWord];
        emptyRowTransform = new Transform[numberOfAttempts];

        //the letter box grid is set up row by row
        //where each row a parent is created first, then filled in with boxes
        //then, the main controlling script attached to each box is stored in the 2D array

        IndividualBoxScript instantiatedBoxScript;
        
        //for loop functions as a while loop:
        //for (initialisation; condition; increment) {}
        for (int eachRow = 0; eachRow < numberOfAttempts; eachRow++)
        {
            //Instantiate creates an emptyRowPrefab object in the scene at position 'transform'
            emptyRowTransform[eachRow] = Instantiate(emptyRowPrefab, transform).GetComponentsInChildren<Transform>()[1];

            for (int eachBox = 0; eachBox < numberOfLettersInWord; eachBox++)
            {
                instantiatedBoxScript = Instantiate(letterBoxPrefab, emptyRowTransform[eachRow]).GetComponent<IndividualBoxScript>();
                
                instantiatedBoxScript.boxPosition = new int[] { eachRow, eachRow };

                setOfBoxes[eachRow, eachBox] = instantiatedBoxScript;
            }
        }

        //begins on the first row
        currentRow = 0;

        ResizeRoot();
    }

    private void ResizeRoot()
    {
        //changes the size of the parent object to make scrolling work (if the grid is too large)
        RectTransform rootTransform = GetComponent<RectTransform>();
        rootTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, numberOfLettersInWord * (boxWidth + gridSpacing));
        rootTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, numberOfAttempts * (boxHeight + gridSpacing));
    }

    public float RemoveBoxes()
    {
        //called a coroutine that removes the boxes, returns the length of time it would take
        StartCoroutine(AnimateRowRemoval());
        return (boxRemoveInterval * emptyRowTransform.Length) + boxRemoveAnimLength + 0.25f;
    }

    private IEnumerator AnimateRowRemoval()
    {
        //loops through each row and plays a removal animation
        for (int row = 0; row < emptyRowTransform.Length; row++)
        {
            yield return new WaitForSeconds(boxRemoveInterval);
            emptyRowTransform[row].GetComponent<Animator>().SetTrigger("SlideRight");

            //calls each IndividualBoxScript to alert them that the boxes are getting destroyed
            for (int box = 0; box < setOfBoxes.GetLength(1); box++)
                setOfBoxes[row, box].OnBoxDestroy();
        }

        yield return new WaitForSeconds(boxRemoveAnimLength);

        //destroys the boxes
        for (int row = 0; row < emptyRowTransform.Length; row++)
            Destroy(emptyRowTransform[row].parent.gameObject);
    }

    //row shakes when invalid input
    public void AnimateRowShake() => emptyRowTransform[currentRow].GetComponent<Animator>().SetTrigger("Shake");

    public void FindNextVacantBox(int checkInputBox, bool toggleCurrentBorder = true)
    {
        //loops through the row of boxes to find the next vacant box, starting from checkInputBox
        bool foundInputtableOrLastBox = false;
        while (!foundInputtableOrLastBox)
        {
            if ((checkInputBox >= numberOfLettersInWord) || (setOfBoxes[currentRow, checkInputBox].isVacant))
            {
                foundInputtableOrLastBox = true;

                UpdateSelection(checkInputBox, toggleCurrentBorder);
            }
            checkInputBox++;
        }
    }

    public void UpdateSelection(int newCurrentInputBox, bool toggleCurrentBorder)
    {
        //updates the boarder and currentInputBox to indicate where is new input location is
        if (toggleCurrentBorder)
            ToggleBorderSelected(false);

        currentInputBox = newCurrentInputBox;
        ToggleBorderSelected(true);
    }

    private void ToggleBorderSelected(bool toSelect)
    {
        //failsafe is the currentInputBox extends beyond the range of the array
        if (currentInputBox >= numberOfLettersInWord)
            return;

        IndividualBoxScript boxToEdit = setOfBoxes[currentRow, currentInputBox];

        //changes the border color of the selected box to grey and the previously selected box back to white
        Color newBorderColor = toSelect ? selectedBorderColor : boxToEdit.originalBorderColor;
        boxToEdit.UpdateBorder(newBorderColor);
    }

    public void InputTextIntoLetterBox(string input)
    {
        //failsafe is the currentInputBox extends beyond the range of the array
        if (currentInputBox >= numberOfLettersInWord)
            return;

        IndividualBoxScript boxToInput = setOfBoxes[currentRow, currentInputBox];
        boxToInput.UpdateText(input);

        //after inputting a letter, finds the next available box
        FindNextVacantBox(currentInputBox + 1);
    }

    public void DeleteTextInLetterBox()
    {
        //similar but the reverse of FindNextVacantBox
        //finds the most recent box that is deletable, then removes the letter (if any)
        bool foundDeletableOrFirstBox = false;
        int checkInputBox = currentInputBox;
        while (!foundDeletableOrFirstBox)
        {
            checkInputBox--;
            if (checkInputBox < 0)
            {
                foundDeletableOrFirstBox = true;
            }
            else
            {
                IndividualBoxScript boxToInput = setOfBoxes[currentRow, checkInputBox];
                if (boxToInput.deletable)
                {
                    boxToInput.UpdateText(null);

                    foundDeletableOrFirstBox = true;
                    UpdateSelection(checkInputBox, true);

                    audioScript.CreateSfxInstance(audioScript.boxDelete);
                }
            }
        }
    }

    //after a guess is submitted, it is compared to the secret word in WordManager and the results are displayed
    public IEnumerator RevealBoxColors(string inputtedWord, BackColorTypes[] colorTypes)
    {
        //loops through each box in the current row
        //then change its back colour to the corresponding colour in the colorTypes array
        for (int box = 0; box < colorTypes.Length; box++)
        {
            IndividualBoxScript boxToEdit = setOfBoxes[currentRow, box];
            
            //does not change the colour if the box already has one
            if (boxToEdit.currentBoxBack == BackColorTypes.None)
            {
                //certain box effects prevent the colour from being revealed
                if ((boxToEdit.currentBoxEffect != EffectTypes.Ice) &&
                    (boxToEdit.currentBoxEffect != EffectTypes.Mist))
                {
                    //changes the background colour
                    boxToEdit.UpdateComparedBackColor(colorTypes[box]);

                    //also changes the letter colour on the keyboard to match
                    RevealKeyboardColor(colorTypes[box], inputtedWord[box]);

                    //then checks if there are any effects that trigger on this box
                    CheckForBoxEffects(boxToEdit, colorTypes[box]);

                    audioScript.CreateSfxInstance(audioScript.boxReveal);
                }
                else
                {
                    //if the colour was not revealed, stores it (if needed later)
                    boxToEdit.variable = colorTypes[box];

                    audioScript.CreateSfxInstance(audioScript.boxNotReveal);
                }

                CheckForIceMelt(box);
            }

            yield return new WaitForSeconds(boxColorRevealInterval);
        }

        yield return new WaitForSeconds(boxColorRevealInterval);
        EndColorDisplay();
    }

    private void RevealKeyboardColor(BackColorTypes newColor, char letter)
    {
        //change keyboard button to match the revealed color
        //some colours are more important than others
        //E.g. if the secret word is: HELLO
        //and the guess is BEVEL
        //then the 'E' key would turn green at the second letter
        //but the 'E' at the 4th position would force the key to turn grey
        //thus by having green as a priority colour, the keyboard does not change to grey at the second 'E'
        List<Color> priorityColor = new List<Color> { ComparisonColors[BackColorTypes.Correct] };
        if (newColor == BackColorTypes.WrongLetter)
            priorityColor.Add(ComparisonColors[BackColorTypes.WrongPos]);

        keyboardScript.ChangeKeyboardColor(letter, newColor, priorityColor.ToArray());
    }

    private void CheckForIceMelt(int currentColumn)
    {
        if (currentRow >= effectScript.iceMeltRowDelay)
        {
            //for each box in a row, if it has the Ice box effect, then reveal its colour when the NEXT guess is submitted
            IndividualBoxScript boxScript = setOfBoxes[currentRow - effectScript.iceMeltRowDelay, currentColumn];
            if (boxScript.currentBoxEffect == EffectTypes.Ice)
            {
                boxScript.IceEffect();
                RevealKeyboardColor((BackColorTypes)boxScript.variable, boxScript.GetText()[0]);
            }
        }
    }
    
    private void CheckForBoxEffects(IndividualBoxScript boxToEdit, BackColorTypes boxColor)
    {
        //if the box contains an effect, perform the associated effect
        switch (boxToEdit.currentBoxEffect)
        {
            case EffectTypes.Lava when boxColor != BackColorTypes.Correct:
                effectScript.LavaEffect();
                break;

            case EffectTypes.HealthBox when boxColor == BackColorTypes.Correct:
                effectScript.HealthBoxEffect();
                break;

            case EffectTypes.PowerupBox when boxColor == BackColorTypes.Correct:
                effectScript.PowerupBoxEffect(boxToEdit);
                break;

            case EffectTypes.TeleportIn:
                effectScript.TeleportEffect(boxToEdit);
                break;

            case EffectTypes.Veil:
                effectScript.VeilRemove(boxToEdit);
                break;
        }
    }

    //calls WordManager to process the outcome of the guess
    private void EndColorDisplay() => wordScript.CheckGameProgression();
}
