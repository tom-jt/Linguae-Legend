using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootOverlayScript : OverlayClass
{
    private Animator lootAnimator;

    private IndividualLootScript[] generatedLootScripts;

    private List<int> generatedPowerupID;
    private List<int> generatedEphemeralIndex;

    [Header("Assignments")]
    [SerializeField]
    private GameFlowManager gameFlowScript;
    [SerializeField]
    private PowerupManager powerupScript;
    [SerializeField]
    private EphemeralManager ephemeralScript;
    [SerializeField]
    private GameObject lootOptionPrefab;
    [SerializeField]
    private RectTransform optionsRoot;

    [Header("Cosmetics")]
    public float lootOptionsInterval = 0.25f;
    public int numberOfOptions = 3;

    protected override void Start()
    {
        base.Start();
        
        //deactivated by default
        menu.SetActive(false);

        lootAnimator = menu.GetComponent<Animator>();
    }

    protected override void ToggleOverlay(bool toggleOn)
    {
        //similar to settings menu
        if (menuExiting)
            return;

        base.ToggleOverlay(toggleOn);

        if (lootAnimator)
        {
            string triggerName = toggleOn ? "FadeIn" : "FadeOut";
            lootAnimator.SetTrigger(triggerName);
        }
    }

    public void OpenOptionMenu()
    {
        //the loot menu does not have an open menu button
        //instead, it opens when a round is won and GameFlowManager calls it
        ToggleOverlay(true);

        generatedLootScripts = new IndividualLootScript[numberOfOptions];

        generatedPowerupID = new List<int>();
        generatedEphemeralIndex = new List<int>();

        //generate a number of options to choose from, and randomises their loot contents
        for (int lootOption = 0; lootOption < numberOfOptions; lootOption++)
        {
            GameObject generatedLootOption = Instantiate(lootOptionPrefab, optionsRoot);
            generatedLootScripts[lootOption] = generatedLootOption.GetComponent<IndividualLootScript>();
            RandomizeLootOption(generatedLootScripts[lootOption]);
        }

        StartCoroutine(OptionEnterAnimations());
    }

    private IEnumerator OptionEnterAnimations()
    {
        //each option then slides down with an animation
        for (int lootOption = 0; lootOption < generatedLootScripts.Length; lootOption++)
        {
            generatedLootScripts[lootOption].animator.SetTrigger("DropDown");

            audioScript.CreateSfxInstance(audioScript.lootSlide);

            yield return new WaitForSecondsRealtime(lootOptionsInterval);
        }
    }

    private void RandomizeLootOption(IndividualLootScript lootScript)
    {
        //picks between powerup or ephemeral
        int lootType = Random.Range(0, 2);

        //used switch statement in case new options are added later
        switch (lootType)
        {
            case 0:
                PowerupLoot(lootScript);
                break;

            case 1:
                EphemeralLoot(lootScript);
                break;
        }
    }

    private void PowerupLoot(IndividualLootScript lootScript)
    {
        //generates a random powerupID based on rarity
        //cannot generate the same powerup for the same loot screen
        int repeatCount = 0;
        int powerupID;
        do
        {
            powerupID = Constants.WeightedRandomiser(GameInfo.PowerupRarities);
            repeatCount++;
        } while ((generatedPowerupID.Contains(powerupID)) && (repeatCount < 100));

        PowerupConstructor powerupInfo = GameInfo.Powerups[powerupID];

        //small randomisation of the amount of powerups obtained
        int randomAmount = Random.Range(GameInfo.PowerupRarities[powerupID] - 1, GameInfo.PowerupRarities[powerupID] + 2);

        //updates the loot section to match the information
        lootScript.UpdateLootInfo(
            "Powerup",
            powerupInfo.name,
            powerupInfo.description,
            Mathf.Clamp(randomAmount, 1, 999),
            powerupInfo.pixelImage);

        //give the powerup when button is clicked
        lootScript.lootButton.onClick.AddListener(delegate { AcceptPowerup(powerupID, randomAmount); });

        //add the id to list to prevent it from generating again
        generatedPowerupID.Add(powerupID);
    }

    private void EphemeralLoot(IndividualLootScript lootScript)
    {
        //generates a random ephemeral that is not duplicated
        int repeatCount = 0;
        int ephemeralID;
        do
        {
            ephemeralID = Constants.WeightedRandomiser(GameInfo.EphemeralRarities);
            repeatCount++;
        } while ((generatedEphemeralIndex.Contains(ephemeralID)) && (repeatCount < 100));

        //updates the loot section to match the information
        lootScript.UpdateLootInfo("Ephemeral",
            GameInfo.Ephemerals[ephemeralID].name,
            GameInfo.Ephemerals[ephemeralID].description,
            0,
            GameInfo.Ephemerals[ephemeralID].pixelImage);

        lootScript.lootButton.onClick.AddListener(delegate { AcceptEphemeral(ephemeralID); });

        generatedEphemeralIndex.Add(ephemeralID);
    }

    private void AcceptPowerup(int powerupID, int amount)
    {
        //remove all options, then give powerups
        RemoveOptions();
        powerupScript.ChangePowerupsOwned(powerupID, amount);
        CloseOptionMenu();
    }

    private void AcceptEphemeral(int ephemeralID)
    {
        //remove all options, then calls the Ephemeral Manager to handle the effect
        RemoveOptions();
        ephemeralScript.TriggerEphemeral(ephemeralID);
        CloseOptionMenu();
    }

    private void RemoveOptions()
    {
        //cycles through each button and remove its function
        //preventing the player from clicking multiple
        for (int lootOption = 0; lootOption < generatedLootScripts.Length; lootOption++)
            generatedLootScripts[lootOption].lootButton.onClick.RemoveAllListeners();

        StartCoroutine(DestroyOptions());
    }

    private IEnumerator DestroyOptions()
    {
        //cycles through each option with a delay and deactivates them after animation
        for (int lootOption = 0; lootOption < generatedLootScripts.Length; lootOption++)
        {
            generatedLootScripts[lootOption].animator.SetTrigger("ReturnUp");

            Destroy(generatedLootScripts[lootOption].gameObject, animationDuration);

            audioScript.CreateSfxInstance(audioScript.overlaySlideUp);

            yield return new WaitForSeconds(lootOptionsInterval);
        }
    }

    private void CloseOptionMenu()
    {
        //begin the next round after a reward has been chosen
        ToggleOverlay(false);
        gameFlowScript.InitiateRound();
    }
}
