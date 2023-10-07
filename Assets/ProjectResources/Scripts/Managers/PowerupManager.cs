using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Reflection;

public class PowerupManager : MonoBehaviour
{
    private AudioManager audioScript;
    private List<GameObject> generatedPowerupButtons;

    [Header("Assignments")]
    [SerializeField]
    private DisplayMessageManager messageScript;
    [SerializeField]
    private PowerupFunctionsManager functionsScript;
    [SerializeField]
    private GameObject powerupButtonPrefab;
    [SerializeField]
    private Transform powerupButtonGrid;

    [Header("Cosmetics")]
    public int initialPowerupsMultiplier = 3;
    public bool consumePowerup = true;

    private void Start()
    {
        //cycles through each powerup
        generatedPowerupButtons = new List<GameObject>();
        PowerupConstructor powerupInfo;
        for (int powerupID = 0; powerupID < GameInfo.Powerups.Length; powerupID++)
        {
            powerupInfo = GameInfo.Powerups[powerupID];

            //creates a powerup button for it, then updates the UI and button functionality
            generatedPowerupButtons.Add(Instantiate(powerupButtonPrefab, powerupButtonGrid));

            Button powerupButton = generatedPowerupButtons[powerupID].GetComponentInChildren<Button>();
            int tempID = powerupID;
            powerupButton.onClick.AddListener(delegate { OnPowerupButton(tempID); });

            Image powerupIcon = generatedPowerupButtons[powerupID].GetComponentInChildren<Image>();
            powerupIcon.sprite = powerupInfo.icon;

            TextMeshProUGUI[] texts = generatedPowerupButtons[powerupID].GetComponentsInChildren<TextMeshProUGUI>();

            TextMeshProUGUI amount = texts[0];
            GameInfo.Powerups[powerupID].amountOwned = initialPowerupsMultiplier * GameInfo.Powerups[powerupID].rarity;
            amount.text = "x" + powerupInfo.amountOwned.ToString();

            TextMeshProUGUI name = texts[1];
            name.text = powerupInfo.name;
        }

        consumePowerup = true;

        audioScript = GameObject.FindGameObjectWithTag("GlobalObject").GetComponentInChildren<AudioManager>();
    }

    public void ChangePowerupsOwned(int powerupID, int amount = 0, bool addInsteadOfSet = true)
    {
        //gives option to add no. of powerups or set the value
        GameInfo.Powerups[powerupID].amountOwned = addInsteadOfSet ? (GameInfo.Powerups[powerupID].amountOwned + amount) : amount;

        if (generatedPowerupButtons[powerupID])
        {
            //updates the amount owned UI
            TextMeshProUGUI amountText = generatedPowerupButtons[powerupID].GetComponentsInChildren<TextMeshProUGUI>()[0];

            int amountToShow = Mathf.Clamp(GameInfo.Powerups[powerupID].amountOwned, 0, 999);

            amountText.text = "x" + amountToShow;

            amountText.color = amountToShow == 0 ? Constants.RedColor : Constants.WhiteColor;
        }
    }

    private void OnPowerupButton(int powerupID)
    {
        if (!GameFlowManager.enablePlayerLetterInput)
            return;

        //when a powerup button is pressed:
        if ((GameInfo.Powerups[powerupID].amountOwned > 0) || (!consumePowerup))
        {
            //gets the corresponding powerup function from the PowerupFunctionsScript
            MethodInfo correspondingFunction = typeof(PowerupFunctionsManager).GetMethod(GameInfo.Powerups[powerupID].functionName);

            //that function is executed and will return a boolean value
            //this boolean value is based on whether or not the powerup is used successfully
            if ((bool)correspondingFunction.Invoke(functionsScript, null))
                ChangePowerupsOwned(powerupID, consumePowerup ? -1 : 0);
            else
                //display message: unable to use powerup
                messageScript.CreatePopup("Unable to Use Powerup");
        }
        else
        {
            //display message: none owned
            messageScript.CreatePopup("No Powerups Left");
        }

        audioScript.CreateSfxInstance(audioScript.buttonClicked);
    }
}
