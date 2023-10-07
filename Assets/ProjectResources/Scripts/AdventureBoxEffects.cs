using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdventureBoxEffects : WordGameClass
{
    private List<int> effects = new List<int>();
    private int[,] mistBlocks;
    private int[,] lavaBlocks;
    private int[,] healthBlocks;
    private int[,] forceLetterBlocks;
    private int[,] iceBlocks;
    private int[,] powerupBlocks;
    private int[,] teleportBlocks;
    private int[,] chaosBlocks;
    private int[,] veilBlocks;

    private Dictionary<int, int[,]> IDToVar;

    private void OnEnable()
    {
        //initialises arrays indicating the amount of each obstacle in each round in adventure mode
        mistBlocks = new int[3, 15]
        {
            //easy mode
            {0, 0, 2, 2, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            //normal mode
            {2, 1, 3, 1, 2, 4, 0, 4, 6, 2, -1, -1, -1, -1, -1},
            //hard mdoe
            {2, 2, 4, 4, 12, 0, 2, 0, 0, 0, 1, 2, 4, 8, 8},
        };

        lavaBlocks = new int[3, 15]
        {
            //easy mode
            {0, 0, 0, 0, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            //normal mode
            {0, 0, 0, 3, 4, 0, 3, 0, 0, 3, -1, -1, -1, -1, -1},
            //hard mdoe
            {0, 3, 2, 3, 0, 4, 3, 2, 1, 8, 2, 4, 8, 16, 8},
        };

        forceLetterBlocks = new int[3, 15]
        {
            //easy mode
            {0, 2, 1, 0, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            //normal mode
            {0, 0, 1, 0, 2, 3, 2, 0, 0, 2, -1, -1, -1, -1, -1},
            //hard mdoe
            {2, 0, 1, 2, 0, 1, 0, 2, 2, 0, 1, 1, 1, 1, 1},
        };

        healthBlocks = new int[3, 15]
        {
            //easy mode
            {0, 1, 1, 1, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            //normal mode
            {1, 2, 1, 2, 3, 4, 1, 3, 3, 3, -1, -1, -1, -1, -1},
            //hard mdoe
            {1, 0, 1, 2, 1, 1, 2, 1, 3, 0, 0, 0, 1, 2, 2},
        };

        iceBlocks = new int[3, 15]
        {
            //easy mode
            {0, 0, 0, 2, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            //normal mode
            {1, 2, 0, 2, 2, 0, 2, 4, 2, 2, -1, -1, -1, -1, -1},
            //hard mdoe
            {0, 2, 3, 0, 0, 4, 3, 2, 1, 8, 2, 2, 1, 1, 0},
        };

        powerupBlocks = new int[3, 15]
        {
            //easy mode
            {1, 1, 1, 2, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            //normal mode
            {2, 1, 1, 0, 2, 4, 1, 1, 2, 3, -1, -1, -1, -1, -1},
            //hard mdoe
            {2, 1, 1, 2, 1, 2, 1, 0, 3, 0, 5, 4, 3, 2, 1},
        };

        teleportBlocks = new int[3, 15]
        {
            //easy mode
            {0, 0, 0, 0, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            //normal mode
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 1, -1, -1, -1, -1, -1},
            //hard mdoe
            {1, 0, 1, 2, 1, 1, 2, 1, 3, 0, 0, 0, 1, 2, 2},
        };

        chaosBlocks = new int[3, 15]
        {
            //easy mode
            {0, 0, 0, 0, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            //normal mode
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, -1, -1, -1, -1},
            //hard mdoe
            {3, 3, 2, 2, 8, 1, 3, 1, 0, 5, 3, 3, 3, 3, 3},
        };

        veilBlocks = new int[3, 15]
        {
            //easy mode
            {0, 0, 0, 0, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
            //normal mode
            {1, 2, 3, 2, 1, 0, 1, 3, 3, 2, -1, -1, -1, -1, -1},
            //hard mdoe
            {0, 0, 1, 0, 0, 1, 0, 1, 2, 0, 0, 1, 1, 1, 2},
        };

        IDToVar = new Dictionary<int, int[,]>
        {
            { (int)EffectTypes.Mist, mistBlocks },
            { (int)EffectTypes.Lava, lavaBlocks },
            { (int)EffectTypes.ForceLetter, forceLetterBlocks },
            { (int)EffectTypes.TeleportIn, teleportBlocks },
            { (int)EffectTypes.Ice, iceBlocks },
            { (int)EffectTypes.HealthBox, healthBlocks },
            { (int)EffectTypes.PowerupBox, powerupBlocks },
            { (int)EffectTypes.Chaos, chaosBlocks },
            { (int)EffectTypes.Veil, veilBlocks }
        };
    }

    public List<int> GetAdventureEffects(int round)
    {
        //make round start from 0 (starts from 1 in GameFlowManager)
        round--;
        effects = new List<int>();

        //adds each box effect with amounts initialised above into a list
        foreach (int effectID in IDToVar.Keys)
            AddEffectsToList(IDToVar[effectID][(int)GameFlowManager.difficulty, round], effectID);

        //returns list to generate effect positions
        return effects;
    }

    private void AddEffectsToList(int amount, int effectID)
    {
        //adds an instance of the effect equal to the amount
        for (int addEffect = 0; addEffect < amount; addEffect++)
            effects.Add(effectID);
    }
}
