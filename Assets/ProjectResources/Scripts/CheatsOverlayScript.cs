using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CheatsOverlayScript : OverlayClass
{
    private Animator cheatsAnimator;

    [Header("Assignments")]
    [SerializeField]
    private GameFlowManager gameFlowScript;
    [SerializeField]
    private HealthManager healthScript;
    [SerializeField]
    private SettingsOverlayScript settingsScript;
    [SerializeField]
    private PowerupManager powerupScript;
    [SerializeField]
    private Button blockerButton;
    [SerializeField]
    private Button displaySecretWordButton;
    [SerializeField]
    private Animator displaySecretWordAnimator;
    [SerializeField]
    private TMP_InputField secretWordInputField;
    [SerializeField]
    private Button secretWordChangeButton;
    [SerializeField]
    private TMP_InputField healthInputField;
    [SerializeField]
    private Button healthAddButton;
    [SerializeField]
    private Button healthMinusButton;
    [SerializeField]
    private Toggle invincibleToggle;
    [SerializeField]
    private Toggle infPowerupsToggle;

    protected override void Start()
    {
        //toggles the cheat menu button on if in custom mode
        openOverlayButton.transform.parent.gameObject.SetActive(
            (GameFlowManager.gameMode == GameModeTypes.Custom) || (SecretMenuScript.forceCheatMenu));

        //deactivates menu by default
        menu.SetActive(false);

        cheatsAnimator = menu.GetComponentInChildren<Animator>(true);

        //assigns button functions
        openOverlayButton.onClick.AddListener(OnCheatMenuButton);
        closeOverlayButton.onClick.AddListener(OnCloseCheatMenuButton);
        blockerButton.onClick.AddListener(OnCloseCheatMenuButton);

        displaySecretWordButton.onClick.AddListener(OnDisplaySecretWord);

        secretWordChangeButton.onClick.AddListener(OnChangeSecretWordButton);

        healthAddButton.onClick.AddListener(delegate { OnHealthButton(true); });
        healthMinusButton.onClick.AddListener(delegate { OnHealthButton(false); });

        if (healthScript)
        {
            invincibleToggle.isOn = healthScript.isInvincible;
            invincibleToggle.onValueChanged.AddListener(delegate { OnInvincibleButton(invincibleToggle.isOn); });
        }

        if (powerupScript)
        { 
            infPowerupsToggle.isOn = !powerupScript.consumePowerup;
            infPowerupsToggle.onValueChanged.AddListener(delegate { OnInfPowerupButton(infPowerupsToggle.isOn); });
        }

        audioScript = GameObject.FindGameObjectWithTag("GlobalObject").GetComponentInChildren<AudioManager>();
        AddButtonSfx();
    }

    private void OnCheatMenuButton()
    {
        //activates menu
        menuExiting = false;
        menu.SetActive(true);

        audioScript.CreateSfxInstance(audioScript.overlaySlideDown);
    }

    private void OnCloseCheatMenuButton()
    {
        //if not closing menu, then reset animtations and close it after delay
        if (menuExiting == false)
        {
            displaySecretWordAnimator.SetTrigger("ReturnIdle");
            cheatsAnimator.SetTrigger("CheatsSlideRight");
            menuExiting = true;

            audioScript.CreateSfxInstance(audioScript.overlaySlideUp);
        }

        StartCoroutine(DisableCheatMenu());
    }

    private IEnumerator DisableCheatMenu()
    {
        //this is used to stall instead of WaitForSeconds because the time scale is 0
        //so WaitForSeconds would be paused
        yield return new WaitForSecondsRealtime(animationDuration);

        menuExiting = false;
        menu.SetActive(false);
    }

    //displays the secret word
    private void OnDisplaySecretWord()
    {
        TextMeshProUGUI secretWordBox = displaySecretWordAnimator.GetComponentInChildren<TextMeshProUGUI>(true);
        secretWordBox.text = WordGameClass.secretWord;
        displaySecretWordAnimator.SetTrigger("SlideRight");
    }

    private void OnChangeSecretWordButton()
    {
        if (string.IsNullOrEmpty(secretWordInputField.text))
            return;

        //check if an integer is entered
        //out keyword assigns the result into a declared variable
        //in this case, the int.TryParse() tries to convert the inputted text into an integer
        //then returns boolean of whether it is successful
        //also assigns the converted integer to newWordLength
        if (int.TryParse(secretWordInputField.text, out int newWordLength))
        {
            if ((newWordLength <= 15) && (newWordLength >= 2))
            {
                //manually reset round, then close the cheat and settings overlays to resume time scale
                WordGameClass.numberOfLettersInWord = newWordLength;
                WordDatabaseManager.GetWordListForValidCheck(newWordLength);
                secretWordInputField.text = null;
                StartCoroutine(gameFlowScript.ResetRoundProcess(true));

                OnCloseCheatMenuButton();
                settingsScript.PublicToggleOverlay(false);
            }
        }
        else
        {
            //if the input is not an integer
            //then take the input as a newSecretWord
            //checks the database to see if this entered word is valid
            string newSecretWord = secretWordInputField.text.Trim().ToLower();
            WordDatabaseManager.GetWordListForValidCheck(newSecretWord.Length);
            if (WordDatabaseManager.CheckIfValidWord(newSecretWord))
            {
                //if it is valid, assign secret word
                //then manually reset the round with the predetermined word
                WordGameClass.secretWord = newSecretWord;
                WordGameClass.numberOfLettersInWord = newSecretWord.Length;

                secretWordInputField.text = null;
                StartCoroutine(gameFlowScript.ResetRoundProcess(true, false));

                //close both menus to resume time scale
                OnCloseCheatMenuButton();
                settingsScript.PublicToggleOverlay(false);
            }
            else
            {
                //if it is not valid, then restore the word list to its previous state
                WordDatabaseManager.GetWordListForValidCheck(WordGameClass.numberOfLettersInWord);
            }
        }
    }

    private void OnHealthButton(bool addNotMinus)
    {
        //inputted text must be an integer
        if (int.TryParse(healthInputField.text, out int changeValue)) 
        {
            //resets the input field and changes health accordingly
            healthInputField.text = null;
            healthScript.ChangeHealth(changeValue * (addNotMinus ? 1 : -1));
        }
    }

    private void OnInvincibleButton(bool toInvincible) => healthScript.isInvincible = toInvincible;

    private void OnInfPowerupButton(bool doNotConsume) => powerupScript.consumePowerup = !doNotConsume;
}
