using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//NOTE the manual integer assignment of the box effects
public enum EffectTypes
{
    None = 0,
    Mist = -1,
    Lava = -2,
    ForceLetter = -3,
    TeleportIn = -4,
    TeleportOut = -5,
    Ice = -6,
    HealthBox = 1,
    PowerupBox = 2,
    Chaos = 3,
    Veil = 4
}

public class BoxEffectManager : WordGameClass
{
    private List<int> effectsInRound;

    private int amountOfObstacles;
    private int amountOfBenefits;

    [Header("Assignments")]
    [SerializeField]
    private LetterBoxManager letterBoxScript;
    [SerializeField]
    private AdventureBoxEffects adventureEffectScript;
    [SerializeField]
    private GameFlowManager gameFlowScript;
    [SerializeField]
    private HealthManager healthScript;
    [SerializeField]
    private PowerupManager powerupScript;

    [Header("Box Effect Variables")]
    public float lavaDamage = 10f;
    public float healBoxHeal = 10f;
    public float veilCost = 10f;
    public int iceMeltRowDelay = 1;

    public void InitiateEffectGeneration(int round)
    {
        //adventure mode uses a specially curated set of box effects instead of randomly generated ones
        if (GameFlowManager.gameMode == GameModeTypes.Adventure)
        {
            effectsInRound = adventureEffectScript.GetAdventureEffects(round);
        }
        else
        {
            //other modes randomly generate the amount of box effects first
            RandomiseEffectAmounts(round);
            //then randomly generates a position for them
            effectsInRound = RandomiseEffectTypes(amountOfObstacles, amountOfBenefits);
        }

        RandomiseEffectPositions();
    }

    public void RandomiseEffectAmounts(int round)
    {
        //amount of obstacles and benefits generated scale with the current round
        int obstacleScale;
        int benefitScale;
        obstacleScale = (int)Mathf.Round(Mathf.Pow(8 * round, 0.5f));
        benefitScale = (int)Mathf.Round(Mathf.Pow(3 * round, 0.5f));
        amountOfObstacles = Random.Range(obstacleScale - 1, obstacleScale + 2) + LingeringEffectsManager.extraObstacles;
        amountOfBenefits = Random.Range(benefitScale - 1, benefitScale + 2) + LingeringEffectsManager.extraBenefits;
    }

    private List<int> RandomiseEffectTypes(int obstacles, int benefits)
    {
        //randomised effects are appended to list
        List<int> randomisedEffects = new List<int>();

        for (int effect = 0; effect < obstacles; effect++)
        {
            int randomIndex = Constants.WeightedRandomiser(GameInfo.ObstacleRarities) + 1;
            if (randomIndex != -1)
                randomisedEffects.Add((-randomIndex));
        }
        for (int effect = 0; effect < benefits; effect++)
        {
            int randomIndex = Constants.WeightedRandomiser(GameInfo.BenefitRarities) + 1;
            if (randomIndex != -1)
                randomisedEffects.Add(randomIndex);
        }

        return randomisedEffects;
    }

    private void RandomiseEffectPositions()
    {
        //randomised effects are assigned positions and specific letter boxes
        if (effectsInRound == null)
            return;

        IndividualBoxScript effectBoxScript;
        BoxEffectConstructor effect;

        for (int effectCounter = 0; effectCounter < effectsInRound.Count; effectCounter++)
        {
            effect = GameInfo.BoxEffects[effectsInRound[effectCounter]];

            if (numberOfAttempts >= effect.bottomRowRestriction)
            {
                int repeatCount = 0;
                int randomRow;
                int randomBox;

                //box effects have restrictions to where they can be placed
                //so randomises a 2D position until it is not in the restriction area
                do
                {
                    randomRow = Random.Range(0, numberOfAttempts - effect.bottomRowRestriction);
                    randomBox = Random.Range(0, numberOfLettersInWord);
                    effectBoxScript = setOfBoxes[randomRow, randomBox];
                    repeatCount++;
                } while ((effectBoxScript.currentBoxEffect != 0) && (repeatCount < 100));

                //script.SendMessage(string) calls the function with name identical to the parameter 
                //within the designated script
                if (repeatCount < 100)
                    effectBoxScript.SendMessage(effect.functionName);
            }
        }
    }

    
    //box effects, written in a format that allows potential changes
    public void LavaEffect()
    {
        //lava = takes damage
        healthScript.ChangeHealth(-lavaDamage);
    }

    public void HealthBoxEffect()
    {
        //health box = heal
        healthScript.ChangeHealth(healBoxHeal);
    }

    public void PowerupBoxEffect(IndividualBoxScript boxToEdit)
    {
        //powerup box = gives powerups of a certain kind equal to its rarity value
        powerupScript.ChangePowerupsOwned((int)boxToEdit.variable, GameInfo.PowerupRarities[(int)boxToEdit.variable]);
    }

    public void TeleportEffect(IndividualBoxScript boxToEdit)
    {
        //copies the inputted letter to the TeleportOut box
        IndividualBoxScript teleportOutScript = setOfBoxes[((int[])boxToEdit.variable)[0], ((int[])boxToEdit.variable)[1]];
        StartCoroutine(teleportOutScript.TransitionToTeleportOut(boxToEdit.GetText()));
    }

    public void VeilRemove(IndividualBoxScript boxToEdit)
    {
        boxToEdit.RemoveVeilButton();
    }

    public void OnVeilButton(IndividualBoxScript boxScript)
    {
        if (boxScript.VeilEffect())
            healthScript.ChangeHealth(-veilCost);
    }
}
