using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyBoardManager : MonoBehaviour
{
    [Header("Assignments")]
    [SerializeField]
    private LetterBoxManager letterBoxScript;
    [SerializeField]
    private WordManager wordScript;

    //Lists are dynamic arrays that can change size without reinitialising it
    [SerializeField]
    private List<GameObject> keyboardButtons = new List<GameObject>();

    [SerializeField]
    private Button deleteButton;
    [SerializeField]
    private Button enterButton;

    private string inputtedString;

    public void SetupKeyboard()
    {
        //loops through each key button assigned in the inspector
        //then attaches a corresponding letter to it
        //also makes it a listener to the InputToLetterBox function by passing in their respective letter
        //NOTE that onClick.AddListener(function) basically calls the function when the button is clicked
        GameObject keyObject;
        for (int key = 0; key < keyboardButtons.Count; key++)
        {
            string assignChr = Constants.keyboardChrs[key].ToString().ToUpper();

            keyObject = keyboardButtons[key];
            keyObject.GetComponentInChildren<TextMeshProUGUI>().text = assignChr;
            keyObject.GetComponentInChildren<Button>().onClick.AddListener(delegate { InputToLetterBox(assignChr); });
        }

        deleteButton.onClick.AddListener(DeleteInLetterBox);
        enterButton.onClick.AddListener(OnWordConfirm);
    }

    public void ResetAllKeyColors()
    {
        //cycles through each key and remove their colours
        for (int key = 0; key < keyboardButtons.Count; key++)
        {
            IndividualKeyScript keyScript = keyboardButtons[key].GetComponent<IndividualKeyScript>();
            keyScript.UpdateBackImage(keyScript.originalBackColor);
            keyScript.currentBoxBack = BackColorTypes.None;
        }
    }

    //the following functions emulate a peripheral keyboard, and only trigger if enablePlayerLetterInput is set to true
    private void InputToLetterBox(string input) 
    {
        if (GameFlowManager.enablePlayerLetterInput) 
            letterBoxScript.InputTextIntoLetterBox(input); 
    }

    private void DeleteInLetterBox()
    {
        if (GameFlowManager.enablePlayerLetterInput)
            letterBoxScript.DeleteTextInLetterBox();
    }

    private void OnWordConfirm()
    {
        if (GameFlowManager.enablePlayerLetterInput)
            wordScript.InitiateWordEntered();
    }

    //the following detects peripheral keyboard inputs and calls the same functions outlined above
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
            DeleteInLetterBox();
        else if (Input.GetKeyDown(KeyCode.Return))
            OnWordConfirm();
        else if (System.String.IsNullOrEmpty(inputtedString))
            inputtedString = Input.inputString;
        else
        {
            char inputtedChar = inputtedString[0];
            if (System.Char.IsLetter(inputtedChar))
                InputToLetterBox(inputtedChar.ToString());

            inputtedString = "";
        }
    }

    public void ChangeKeyboardColor(char letterToChange, BackColorTypes newColorType, Color[] priorityColors = null, float delay = 0f)
    {
        //finds the key button with the letter first
        IndividualKeyScript keyScript = GetKeyObject(letterToChange)?.GetComponent<IndividualKeyScript>();

        //failsafe in case nothing was found (cannot happen under normal circumstances)
        if (keyScript)
        {
            //return if it is trying to assign the same color
            if (newColorType == keyScript.currentBoxBack)
                return;

            //check if it is already the priority color(s), if not, replace it with the new color
            if (priorityColors != null)
            {
                foreach (Color color in priorityColors)
                {
                    if (IndividualClass.ComparisonColors[keyScript.currentBoxBack] == color)
                        return;
                }
            }

            //updates the key colour
            StartCoroutine(keyScript.UpdateBackTypeAndImage(newColorType, delay));
        }
    }

    public GameObject GetKeyObject(char letterToGet)
    {
        //find the corresponding key through linear searching
        for (int letter = 0; letter < Constants.keyboardChrs.Length; letter++)
        {
            if (Constants.keyboardChrs[letter] == letterToGet)
                return keyboardButtons[letter];
        }
        return null;
    }

    public Color GetKeyColor(char letterToCheck)
    {
        //returns the key's colour corresponding to the letter parameter
        IndividualKeyScript keyScript = GetKeyObject(letterToCheck).GetComponent<IndividualKeyScript>();
        return IndividualClass.ComparisonColors[keyScript.currentBoxBack];
    }
}
