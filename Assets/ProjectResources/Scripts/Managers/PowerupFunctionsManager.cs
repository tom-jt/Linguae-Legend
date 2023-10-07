using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerupFunctionsManager : WordGameClass
{ 
    private List<IndividualBoxScript> selectableBoxes;

    [Header("Assignments")]
    [SerializeField]
    private KeyBoardManager keyboardScript;
    [SerializeField]
    private LetterBoxManager letterBoxScript;
    [SerializeField]
    private DisplayMessageManager messageScript;
    [SerializeField]
    private HealthManager healthScript;
    [SerializeField]
    private GameObject swordVfx;
    [SerializeField]
    private GameObject lanternVfx;
    [SerializeField]
    private GameObject mapVfx;
    [SerializeField]
    private GameObject chaosRuneVfx;
    [SerializeField]
    private GameObject magicStaffVfx;
    [SerializeField]
    private GameObject feastVfx;

    [Header("Cosmetics")]
    public Transform vfxRoot;
    public float swordDelay = 0.5f;
    public float swordAnimLength;
    public float lanternAnimLength;
    public float mapAnimLenth;
    public float magicStaffAnimLenth;
    public float feastAnimLength;
    public float runeAnimLength;

    [Header("Powerup Variables")]
    public float feastMaxHealthDeduction = 5f;
    public float feastHealAmount = 15f;

    //some powerups require the user to select 1 letter box amongst several highlighted ones
    //when that box is selected, this subprogram is ran to reset highlighted boxes
    private void OnBoxSelectedResetButtons(bool playBoxSelectedSfx = true)
    {
        for (int obstacle = 0; obstacle < selectableBoxes.Count; obstacle++)
        {
            //goes to each highlighted box
            IndividualBoxScript eachBoxScript = selectableBoxes[obstacle];

            //Manually return the border to its previous colour before being highlighted
            if (eachBoxScript.currentBoxEffect == EffectTypes.ForceLetter)
                eachBoxScript.UpdateBorder(GameInfo.BoxEffects[(int)EffectTypes.ForceLetter].boxColor);

            else if ((currentInputBox >= numberOfLettersInWord) || (selectableBoxes[obstacle] != setOfBoxes[currentRow, currentInputBox]))
                eachBoxScript.UpdateBorder(eachBoxScript.originalBorderColor);

            else
                eachBoxScript.UpdateBorder(selectedBorderColor);

            //deactivate button
            eachBoxScript.clickableButton.onClick.RemoveAllListeners();
            eachBoxScript.clickableButton.gameObject.SetActive(false);
        }

        if ((audioScript) && (playBoxSelectedSfx))
            audioScript.CreateSfxInstance(audioScript.boxSelect);
    }

    //POWERUP FUNCTIONS HERE
    //all powerup functions return a bool value, indicating if they are used successfully
    public bool Sword()
    {
        char randomKey;
        int repeatCount = 0;
        do
        {
            repeatCount++;
            randomKey = Constants.keyboardChrs[Random.Range(0, 26)];

            //check if the random letter is NOT in the word, is NOT already greyed out
            if ((!secretWord.Contains(randomKey.ToString())) &&
                (keyboardScript.GetKeyColor(randomKey) != ComparisonColors[BackColorTypes.WrongLetter]))
            {
                //creates a sword visual effect at that position
                GameObject generatedVfx = Instantiate(swordVfx, vfxRoot);
                generatedVfx.transform.position = keyboardScript.GetKeyObject(randomKey).transform.position;

                Destroy(generatedVfx, swordAnimLength);

                keyboardScript.ChangeKeyboardColor(randomKey, BackColorTypes.WrongLetter, null, swordDelay);

                audioScript.CreateSfxInstance(audioScript.sword);
                return true;
            }
        } while (repeatCount < 100);
        //if no valid letter is found, then nothing occurs
        return false;
    }

    //generic function that prevents the user from entering letters and clicking on more powerups while animation is playing
    private IEnumerator WaitForAnimation(float duration)
    {
        GameFlowManager.enablePlayerLetterInput = false;
        yield return new WaitForSeconds(duration);
        GameFlowManager.enablePlayerLetterInput = true;
    }
    
    public bool Lantern()
    {
        selectableBoxes = new List<IndividualBoxScript>();
        bool foundObstacle = false;
        for (int row = currentRow; row < numberOfAttempts; row++)
        {
            for (int box = 0; box < numberOfLettersInWord; box++)
            {
                //loops through every box to find obstacles
                IndividualBoxScript boxScript = setOfBoxes[row, box];
                if (boxScript.currentBoxEffect < 0)
                {
                    //makes the box selectable, via a button
                    selectableBoxes.Add(boxScript);

                    boxScript.UpdateBorder(clickableBorderColor);

                    boxScript.clickableButton.gameObject.SetActive(true);
                    boxScript.clickableButton.onClick.AddListener(delegate { LanternOnBoxSelected(boxScript); });

                    foundObstacle = true;
                }
            }
        }

        //if at least 1 obstacle is found, then it is successful
        if (foundObstacle)
            GameFlowManager.enablePlayerLetterInput = false;

        return foundObstacle;
    }

    private void LanternOnBoxSelected(IndividualBoxScript boxScript)
    {
        //after choosing an obstacle, reset that box
        boxScript.TryResetBox();
        OnBoxSelectedResetButtons();

        //generate lantern animation here
        GameObject generatedVfx = Instantiate(lanternVfx, vfxRoot);
        Destroy(generatedVfx, lanternAnimLength);
        StartCoroutine(WaitForAnimation(lanternAnimLength));

        audioScript.CreateSfxInstance(audioScript.lantern);
    }

    public bool Map()
    {
        if (currentRow == 0)
            return false;

        //loops through the previous submitted guess
        selectableBoxes = new List<IndividualBoxScript>();
        bool foundWrongPosBox = false;
        for (int box = 0; box < numberOfLettersInWord; box++)
        {
            IndividualBoxScript boxScript = setOfBoxes[currentRow - 1, box];
            if (boxScript.currentBoxBack == BackColorTypes.WrongPos)
            {
                //makes all letters in the wrong position selectable
                selectableBoxes.Add(boxScript);

                boxScript.UpdateBorder(clickableBorderColor);

                boxScript.clickableButton.gameObject.SetActive(true);
                boxScript.clickableButton.onClick.AddListener(delegate { MapOnBoxSelected(boxScript); });

                foundWrongPosBox = true;
            }
        }

        //successful if at least 1 letter is in the wrong position
        if (foundWrongPosBox)
            GameFlowManager.enablePlayerLetterInput = false;

        return foundWrongPosBox;
    }

    private void MapOnBoxSelected(IndividualBoxScript boxScript)
    {
        string letter = boxScript.GetText();
        //find the correct position for that letter (that is not already occupied)
        for (int letterInSecret = 0; letterInSecret < secretWord.Length; letterInSecret++)
        {
            //finds a position where:
            //letter matches the character in the secret word
            //the player did not already place the correct letter here in the previous guess
            //there are no letters already compared at this position (E.g. from previous uses of Map)
            //and there are no obstacles at this position
            if ((letter == secretWord[letterInSecret].ToString()) &&
                (setOfBoxes[currentRow - 1, letterInSecret].currentBoxBack != BackColorTypes.Correct) &&
                (setOfBoxes[currentRow, letterInSecret].currentBoxBack != BackColorTypes.Correct) &&
                (setOfBoxes[currentRow, letterInSecret].currentBoxEffect >= 0))
            {
                //correct position found, place letter in the correct box in the current row
                IndividualBoxScript correctBoxScript = setOfBoxes[currentRow, letterInSecret];

                //resets the box before entering the letter and flipping it to reveal the compared colour
                correctBoxScript.TryResetBox();
                correctBoxScript.UpdateText(letter);
                correctBoxScript.UpdateComparedBackColor(BackColorTypes.Correct);
                correctBoxScript.deletable = false;

                //also updates keyboard to reflect this information
                keyboardScript.ChangeKeyboardColor(letter[0], BackColorTypes.Correct);

                //if the current input indicator was on this box, then move it
                if (currentInputBox == letterInSecret)
                    letterBoxScript.FindNextVacantBox(currentInputBox + 1);

                OnBoxSelectedResetButtons();

                //play map animation here
                GameObject generatedVfx = Instantiate(mapVfx, vfxRoot);
                Destroy(generatedVfx, mapAnimLenth);
                StartCoroutine(WaitForAnimation(mapAnimLenth));

                audioScript.CreateSfxInstance(audioScript.map);

                return;
            }
        }

        //if no vacant position found, then display this message
        messageScript.CreatePopup("Location Already Found or Position Unnavigable", 2f);
        OnBoxSelectedResetButtons();

        GameFlowManager.enablePlayerLetterInput = true;
    }

    public bool MagicStaff()
    {
        //loops through each box and make all box effects selectable
        selectableBoxes = new List<IndividualBoxScript>();
        bool foundVacantBox = false;
        bool foundBoxEffect = false;
        for (int row = currentRow; row < numberOfAttempts; row++)
        {
            for (int box = 0; box < numberOfLettersInWord; box++)
            {
                IndividualBoxScript boxScript = setOfBoxes[row, box];
                if (boxScript.currentBoxEffect != EffectTypes.None)
                {
                    //EXCLUDES the teleporter box effect
                    if ((boxScript.currentBoxEffect != EffectTypes.TeleportIn) && (boxScript.currentBoxEffect != EffectTypes.TeleportOut))
                    {
                        selectableBoxes.Add(boxScript);

                        boxScript.UpdateBorder(clickableBorderColor);
                        boxScript.clickableButton.gameObject.SetActive(true);
                        boxScript.clickableButton.onClick.AddListener(delegate { MagicStaffOnBoxEffectSelected(boxScript); });

                        foundBoxEffect = true;
                    }
                }
                else
                {
                    foundVacantBox = true;
                }
            }
        }

        //makes sure that there is at least 1 box effect AND 1 vacant box to move the box effect to
        //otherwise, returns false
        if (foundBoxEffect)
            if (foundVacantBox)
                GameFlowManager.enablePlayerLetterInput = false;
            else
                OnBoxSelectedResetButtons();

        return (foundBoxEffect) && (foundVacantBox);
    }

    private void MagicStaffOnBoxEffectSelected(IndividualBoxScript effectBox)
    {
        OnBoxSelectedResetButtons();

        //loops through to make all vacant boxes selectable
        selectableBoxes = new List<IndividualBoxScript>();

        for (int row = currentRow; row < numberOfAttempts; row++)
        {
            for (int box = 0; box < numberOfLettersInWord; box++)
            {
                IndividualBoxScript boxScript = setOfBoxes[row, box];
                if (boxScript.currentBoxEffect == EffectTypes.None)
                {
                    selectableBoxes.Add(boxScript);

                    boxScript.UpdateBorder(clickableBorderColor);

                    boxScript.clickableButton.gameObject.SetActive(true);
                    boxScript.clickableButton.onClick.AddListener(delegate { MagicStaffOnSecondBoxSelected(effectBox, boxScript); });
                }
            }
        }
    }

    private void MagicStaffOnSecondBoxSelected(IndividualBoxScript effectBox, IndividualBoxScript boxScript)
    {
        OnBoxSelectedResetButtons();

        //resets the first box and moves it into the second box
        //for box effects that randomly generate elements, the previous element is carried over instead
        if (effectBox.currentBoxEffect == EffectTypes.ForceLetter)
            boxScript.ForceLetter(effectBox.GetText());
        else if (effectBox.currentBoxEffect == EffectTypes.PowerupBox)
            boxScript.PowerupBox((int)effectBox.variable);
        else
            boxScript.SendMessage(effectBox.currentBoxEffect.ToString());

        effectBox.TryResetBox();
        letterBoxScript.FindNextVacantBox(currentInputBox, false);

        //generate magic staff animation here
        GameObject generatedVfx = Instantiate(magicStaffVfx, vfxRoot);
        Destroy(generatedVfx, magicStaffAnimLenth);
        StartCoroutine(WaitForAnimation(magicStaffAnimLenth));

        audioScript.CreateSfxInstance(audioScript.wand);
    }

    public bool ChaosRune()
    {
        //finds a random position that does not have a box effect or is already compared
        int repeatCount = 0;
        int randomRow;
        int randomBox;
        IndividualBoxScript chosenBox;
        do
        {
            randomRow = Random.Range(currentRow, numberOfAttempts - 1);
            randomBox = Random.Range(0, numberOfLettersInWord);
            chosenBox = setOfBoxes[randomRow, randomBox];
            repeatCount++;
        } while (((chosenBox.currentBoxEffect != EffectTypes.None) || (chosenBox.currentBoxBack != BackColorTypes.None)) && (repeatCount < 100));

        if (repeatCount < 100)
        {
            chosenBox.SendMessage(EffectTypes.Chaos.ToString());

            //generate chaos rune animation here
            GameObject generatedVfx = Instantiate(chaosRuneVfx, vfxRoot);
            generatedVfx.transform.position = chosenBox.transform.position;

            Destroy(generatedVfx, runeAnimLength);

            audioScript.CreateSfxInstance(audioScript.rune);
        }

        //if no valid position is found after 100 repetitions, then return false
        return repeatCount < 100;
    }

    public bool Feast()
    {
        //makes sure that the player's health is not already over the max health if it would be reduced
        if (healthScript.GetRatio() < 1 - (feastMaxHealthDeduction / healthScript.maxHealth))
        {
            //reduces max health then increases health
            healthScript.ChangeMaxHealth(-feastMaxHealthDeduction);
            healthScript.ChangeHealth(feastHealAmount);

            //generate feast animation here
            GameObject generatedVfx = Instantiate(feastVfx, vfxRoot);
            Destroy(generatedVfx, feastAnimLength);
            StartCoroutine(WaitForAnimation(feastAnimLength));

            audioScript.CreateSfxInstance(audioScript.feast);

            return true;
        }

        return false;
    }
}
