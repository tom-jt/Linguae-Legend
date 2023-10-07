using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EphemeralManager : MonoBehaviour
{
    [Header("Assignments")]
    [SerializeField]
    private HealthManager healthScript;
    [SerializeField]
    private PowerupManager powerupScript;

    [Header("Ephemeral Variables")]
    [Range(0f, 1f)]
    public float fountainHealPercent = 0.2f;
    [Range(0f, 1f)]
    public float cursedScrollDamagePercent = 0.2f;
    public float armourMaxHealthIncrease = 10f;
    public int toolBagPowerupRepetitions = 3;
    public int toolBagPowerupAmount = 1;
    public int chestPowerupRepetitions = 5;
    public int chestExtraObstacles = 3;
    public int coinExtraBenefits = 3;


    //calls the function with the same name corresponding to the ephemeral ID
    public void TriggerEphemeral(int ephemeralID) => SendMessage(GameInfo.Ephemerals[ephemeralID].name.Replace(" ", null));

    private void HealingFountain()
    {
        healthScript.ChangeHealth(fountainHealPercent * healthScript.maxHealth);
    }

    private void BasicBook()
    {
        LingeringEffectsManager.extraAttempts++;
    }

    private void RoyalBook()
    {
        LingeringEffectsManager.extraAttempts += 2;
    }

    private void CursedScroll()
    {
        LingeringEffectsManager.extraAttempts += 2;
        healthScript.ChangeHealth(-cursedScrollDamagePercent * healthScript.maxHealth);
    }

    private void RustyArmour()
    {
        healthScript.ChangeMaxHealth(armourMaxHealthIncrease);
        healthScript.ChangeHealth(armourMaxHealthIncrease);
    }

    private void ToolBag()
    {
        //loops through powerups 3 times, then picks a random powerup to give to the player each time
        for (int powerupRepetition = 0; powerupRepetition < toolBagPowerupRepetitions; powerupRepetition++)
        {
            int powerupID = Constants.WeightedRandomiser(GameInfo.PowerupRarities);
            powerupScript.ChangePowerupsOwned(powerupID, toolBagPowerupAmount);
        }
    }

    private void AncientChest()
    {
        //loops through powerups 5 times, then gives the player an anount of random powerups equal to their rarities
        for (int powerupRepetition = 0; powerupRepetition < chestPowerupRepetitions; powerupRepetition++)
        {
            int powerupID = Constants.WeightedRandomiser(GameInfo.PowerupRarities);
            powerupScript.ChangePowerupsOwned(powerupID, GameInfo.PowerupRarities[powerupID]);
        }

        LingeringEffectsManager.extraObstacles += chestExtraObstacles;
    }

    private void LuckyCoin()
    {
        LingeringEffectsManager.extraBenefits += coinExtraBenefits;
    }
}
