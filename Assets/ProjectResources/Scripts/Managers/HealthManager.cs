using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthManager : MonoBehaviour
{
    private AudioManager audioScript;

    private float previousHealth;
    private float previousRatio;
    private float health;

    private bool sliderMoving;
    private float startTime;

    [Header("Assignments")]
    [SerializeField]
    private Animator healthScreenVfxAnimator;
    [SerializeField]
    private TextMeshProUGUI healthText;
    [SerializeField]
    private Image healthBarFill;
    [SerializeField]
    private GameObject healthChangeVfxPrefab;

    [Header("Cosmetics")]
    public float maxHealth = 100f;
    public bool isInvincible = false;
    public float healthChangeVfxLifeTime = 0.5f;
    public float healthSliderMoveDuration = 0.5f;

    public float GetRatio() => health / maxHealth;

    private void Start()
    {
        audioScript = GameObject.FindGameObjectWithTag("GlobalObject").GetComponentInChildren<AudioManager>();

        //sets health to equal max health
        health = maxHealth;
        isInvincible = false;
    }

    public void ChangeHealth(float amount)
    {
        //ignores health reduction if the player is invincible
        if ((amount < 0) && (isInvincible))
            return;

        //records previous health and ratio of health (to be used for animating the health bar slider)
        previousHealth = health;
        previousRatio = GetRatio();

        //Mathf.Clamp ensures that the value is between the second and third parameters
        //in this case, identical to:
        //if (health + amount > maxHealth)
        //     health = maxHealth;
        // else if (health + amount < 0)
        //     health = 0;
        // else 
        //     health += amount;

        health = Mathf.Clamp(health + amount, 0, maxHealth);

        //create some health visual effects and update the UI
        HealthChangeEffx(amount);
        UpdateHealthUI();

        //if health drops below 0, broadcast to end the game
        if (health <= 0)
            EventManager.Broadcast(EventManager.GameEnd, false);
    }

    public void ChangeMaxHealth(float amount)
    {
        previousRatio = GetRatio();

        //clamp the max health so that it's between 1 and 10000
        maxHealth = Mathf.Clamp(maxHealth + amount, 1, 10000);

        //then clamp the health so that it falls within maxHealth range
        health = Mathf.Clamp(health, 0, maxHealth);

        UpdateHealthUI();
    }

    private void OnEnable()
    {
        EventManager.WordGuessEnd += WordGuessHealthChange;
        EventManager.RoundEnd += BossStageLoseGame;
    }

    private void OnDisable()
    {
        EventManager.WordGuessEnd -= WordGuessHealthChange;
        EventManager.RoundEnd -= BossStageLoseGame;
    }

    private void WordGuessHealthChange(bool guessCorrect)
    {
        //if guessed correctly
        //rewards health equal to 5 times the number of attempts left
        //if guessed wrongly
        //deduct 5 + 5 times the difficulty (0, 1, or 2)
        int amount = 5 * (guessCorrect ? 
            WordGameClass.numberOfAttempts - WordGameClass.GetCurrentAttempt() :
            -(1 + (int)GameFlowManager.difficulty));

        ChangeHealth(amount);
    }

    private void BossStageLoseGame(bool wonRound)
    {
        //boss stage makes the player lose the game
        //sets health to 0
        if (GameFlowManager.isBossStage && !wonRound)
            ChangeHealth(-health);
    }

    private void HealthChangeEffx(float changeAmount)
    {
        //creates a temporary visual and audio effect
        GameObject changeVfx = Instantiate(healthChangeVfxPrefab, healthText.transform);
        TextMeshProUGUI changeVfxText = changeVfx.GetComponent<TextMeshProUGUI>();
        changeVfxText.text = (changeAmount >= 0 ? "+" : "-") + Mathf.Round(Mathf.Abs(changeAmount));
        changeVfxText.color = changeAmount >= 0 ? Constants.GreenColor : Constants.RedColor;

        Destroy(changeVfx, healthChangeVfxLifeTime);

        string vfxTrigger = changeAmount >= 0 ? "OnHeal" : "OnDamage";
        healthScreenVfxAnimator.SetTrigger(vfxTrigger);

        AudioClip healthChangeAudio = changeAmount >= 0 ? audioScript.heal : audioScript.damage;
        audioScript.CreateSfxInstance(healthChangeAudio);
    }

    private void UpdateHealthUI()
    {
        //begin animating the health bar to move to a designated percentage
        startTime = Time.time;
        sliderMoving = true;
    }

    //movement of the health bar every frame when health or max health is changed
    private void Update()
    {
        if (sliderMoving)
        {
            float timeRatio = (Time.time - startTime) / healthSliderMoveDuration;

            if (timeRatio >= 1)
            {
                sliderMoving = false;
            }
            else
            {
                float healthFillRatio = Mathf.Lerp(previousRatio, GetRatio(), timeRatio);
                healthBarFill.fillAmount = healthFillRatio;

                float textValue = Mathf.Lerp(previousHealth, health, timeRatio);
                healthText.text = Mathf.Round(textValue) + " / " + maxHealth;
            }
        }
    }
}
