using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsOverlayScript : OverlayClass
{
    private Animator settingsAnimator;

    [Header("Assignments")]
    [SerializeField]
    private GameFlowManager gameFlowScript;
    [SerializeField]
    private HealthManager healthScript;

    [Space]
    [SerializeField]
    private TMP_Dropdown colorblindDropDown;
    [SerializeField]
    private Toggle toggleSimulation;
    [SerializeField]
    private TextMeshProUGUI colorblindText;

    [Space]
    [SerializeField]
    private TMP_Dropdown boxStyleDropdown;

    [Space]
    [SerializeField]
    private Slider bgmSlider;
    [SerializeField]
    private Image bgmIcon;
    [SerializeField]
    private Slider sfxSlider;
    [SerializeField]
    private Image sfxIcon;
    [SerializeField]
    private Sprite volumeIcon;
    [SerializeField]
    private Sprite mutedIcon;

    [Space]
    [SerializeField]
    private Toggle profanityToggle;

    [Space]
    [SerializeField]
    private Button resetButton;

    [Space]
    [SerializeField]
    private Button quitButton;

    [Header("Cosmetics")]
    public int resetRoundHealthCost = 10;

    //initialisation of variables and option status
    protected override void Start()
    {
        //base refers to the parent class
        //base.Start() here refers to OverlayClass.Start()
        base.Start();

        //menu is deactivated by default
        menu.SetActive(false);

        settingsAnimator = menu.GetComponent<Animator>();

        //options in settings are initialised
        toggleSimulation.isOn = ColorBlindFilter.simulateColorBlindness;
        colorblindText.text = ColorBlindFilter.simulateColorBlindness ? "Simulate" : "Colorblind";
        toggleSimulation.onValueChanged.AddListener(delegate { OnFilterModeToggled(toggleSimulation.isOn); });
        colorblindDropDown.onValueChanged.AddListener(delegate { OnColorblindOptionChanged(colorblindDropDown.value); });
        colorblindDropDown.value = (int)ColorBlindFilter.mode;

        boxStyleDropdown.onValueChanged.AddListener(delegate { OnBoxStyleOptionChanged(boxStyleDropdown.value); });
        boxStyleDropdown.value = (int)GameFlowManager.boxAnimStyle;

        bgmSlider.value = audioScript.GetBackgroundMusicVolume();
        bgmIcon.sprite = bgmSlider.value == 0 ? mutedIcon : volumeIcon;
        bgmSlider.onValueChanged.AddListener(delegate { OnBGMValueChanged(bgmSlider.value); });

        sfxSlider.value = audioScript.GetMasterVolume();
        sfxIcon.sprite = sfxSlider.value == 0 ? mutedIcon : volumeIcon;
        sfxSlider.onValueChanged.AddListener(delegate { OnSFXValueChanged(sfxSlider.value); });

        profanityToggle.isOn = GameFlowManager.useProfanityFilter;
        profanityToggle.onValueChanged.AddListener(delegate { OnProfanityToggleChange(profanityToggle.isOn); });

        //these buttons only appear in the game scene and not the start menu, thus the feature flag
        if (resetButton)
            resetButton.onClick.AddListener(OnResetButton);

        if (quitButton)
            quitButton.onClick.AddListener(OnQuitButton);
    }

    //intermediate from public to private function
    public void PublicToggleOverlay(bool toggleOn) => ToggleOverlay(toggleOn);

    protected override void ToggleOverlay(bool toggleOn)
    {
        if (menuExiting)
            return;

        base.ToggleOverlay(toggleOn);

        //animation the menu
        if (settingsAnimator)
        {
            string triggerName = toggleOn ? "DropDown" : "ReturnUp";
            settingsAnimator.SetTrigger(triggerName);
        }

        audioScript.CreateSfxInstance(toggleOn ? audioScript.overlaySlideDown : audioScript.overlaySlideUp);
    }

    private void OnFilterModeToggled(bool toSimulation)
    {
        EventManager.Broadcast(EventManager.ColorblindToggle, toSimulation);
        colorblindText.text = toSimulation ? "Simulate" : "Colorblind";
    }

    private void OnColorblindOptionChanged(int newOption) => ColorBlindFilter.mode = (ColorBlindMode)newOption;

    private void OnBoxStyleOptionChanged(int newOption) => GameFlowManager.boxAnimStyle = (BoxFlipStyles)newOption;

    private void OnBGMValueChanged(float newValue)
    {
        //change background music volume
        //switch to mute icon if the new value is 0
        audioScript.ChangeBackgroundMusicVolume(newValue);
        bgmIcon.sprite = newValue == 0 ? mutedIcon : volumeIcon;
    }

    private void OnSFXValueChanged(float newValue)
    {
        //change sound effects volume
        //switch to mute icon if the new value is 0
        audioScript.ChangeMasterVolume(newValue);
        sfxIcon.sprite = newValue == 0 ? mutedIcon : volumeIcon;
    }

    private void OnProfanityToggleChange(bool newValue) => GameFlowManager.useProfanityFilter = newValue;

    private void OnResetButton()
    {
        //cannot reset round if input state is false
        //such as during animations and game loading
        if (!currentInputBool)
            return;

        currentInputBool = false;

        //deduct some health to prevent exploits
        //and reset the round without changing the length of the secret word
        StartCoroutine(gameFlowScript.ResetRoundProcess(true));
        healthScript.ChangeHealth(-resetRoundHealthCost);
        ToggleOverlay(false);
    }

    private void OnQuitButton()
    {
        //toggles out of the settings menu
        //leaves the game and returns to start menu
        StartCoroutine(gameFlowScript.LoadNewScene(Constants.StartMenuScene, false));
        ToggleOverlay(false);
    }
}
