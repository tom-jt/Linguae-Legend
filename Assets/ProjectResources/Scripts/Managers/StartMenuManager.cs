using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//3 different menu screens
public enum MenuTypes
{
    Play,
    Difficulty,
    ModeSelect,
}

public class StartMenuManager : MonoBehaviour
{
    private bool menuTransitioning;

    private AudioManager audioScript;

    [Header("Assignments")]
    [SerializeField]
    private Animator transitionAnimator;
    public MenuTypes defaultMenu = MenuTypes.Play;
    public GameObject playMenu;
    public GameObject difficultyMenu;
    public GameObject modeSelectMenu;
    public Button exitButton;
    public Button playButton;
    public Button adventureButton;
    public Button endlessButton;
    public Button customButton;
    public Button modeBackButton;
    public Button easyButton;
    public Button normalButton;
    public Button hardButton;
    public Button difficultyBackButton;

    [Header("Cosmetics")]
    public float menuTransitionDelay;
    public float sceneTransitionLength;

    private void Awake()
    {
        menuTransitioning = false;
        SwitchMenu(defaultMenu);
    }

    private void Start()
    {
        //assign button functions
        exitButton.onClick.AddListener(OnExitButton);

        playButton.onClick.AddListener(OnPlayButtonClicked);

        adventureButton.onClick.AddListener(delegate { OnModeSelectButton(GameModeTypes.Adventure); });
        endlessButton.onClick.AddListener(delegate { OnModeSelectButton(GameModeTypes.Endless); });
        customButton.onClick.AddListener(delegate { OnModeSelectButton(GameModeTypes.Custom); });

        easyButton.onClick.AddListener(delegate { OnDifficultyButton(DifficultyTypes.Rookie); });
        normalButton.onClick.AddListener(delegate { OnDifficultyButton(DifficultyTypes.Normal); });
        hardButton.onClick.AddListener(delegate { OnDifficultyButton(DifficultyTypes.Insane); });

        modeBackButton.onClick.AddListener(OnModeBackButton);
        difficultyBackButton.onClick.AddListener(OnDifficultyBackButton);

        transitionAnimator.SetTrigger("OpenScene");
        audioScript = GameObject.FindGameObjectWithTag("GlobalObject").GetComponentInChildren<AudioManager>();
        PlayRandomMenuMusic();
    }

    private void PlayRandomMenuMusic()
    {
        //chooses a random music track to play
        if (audioScript)
        {
            int randomMusicIndex = Random.Range(0, audioScript.startMenuPlaylist.Length);
            audioScript.ChangeBackgroundMusic(audioScript.startMenuPlaylist[randomMusicIndex]);
        }
    }

    public void SwitchMenu(MenuTypes newMenu)
    {
        //activates the requested menu and deactivates all others
        playMenu.SetActive(newMenu == MenuTypes.Play);
        difficultyMenu.SetActive(newMenu == MenuTypes.Difficulty);
        modeSelectMenu.SetActive(newMenu == MenuTypes.ModeSelect);
    }

    private void OnPlayButtonClicked()
    {
        //move to mode select menu after transition
        if (menuTransitioning)
            return;

        StartCoroutine(SwitchMenuDelay(MenuTypes.ModeSelect, "PlayToMode"));
    }

    private void OnModeSelectButton(GameModeTypes mode)
    {
        if (!menuTransitioning)
        {
            //assigns the selected mode to gameMode
            GameFlowManager.gameMode = mode;

            if (mode == GameModeTypes.Adventure)
            {
                //checks and activates the adventure mode difficulties that is player has unlocked
                GameInfo gameInfoScript = GameObject.FindGameObjectWithTag("GlobalObject").GetComponentInChildren<GameInfo>();

                normalButton.gameObject.SetActive(gameInfoScript.playerSave.advDiffModes >= (int)DifficultyTypes.Normal);
                hardButton.gameObject.SetActive(gameInfoScript.playerSave.advDiffModes >= (int)DifficultyTypes.Insane);
            }
            else
            {
                //otherwise activate all difficulty buttons
                normalButton.gameObject.SetActive(true);
                hardButton.gameObject.SetActive(true);
            }

            //transition to difficulty menu after transition
            StartCoroutine(SwitchMenuDelay(MenuTypes.Difficulty, "ModeToDiff"));
        }
    }

    private void OnModeBackButton()
    {
        //return to main menu
        if (!menuTransitioning)
            StartCoroutine(SwitchMenuDelay(MenuTypes.Play, "ModeToDiff"));
    }

    private void OnDifficultyBackButton()
    {
        //return to mode select menu
        if (!menuTransitioning)
            StartCoroutine(SwitchMenuDelay(MenuTypes.ModeSelect, "ModeToDiff"));
    }

    private IEnumerator SwitchMenuDelay(MenuTypes newMenu, string triggerName)
    {
        menuTransitioning = true;

        transitionAnimator.SetTrigger(triggerName);
        yield return new WaitForSeconds(menuTransitionDelay);
        SwitchMenu(newMenu);

        menuTransitioning = false;
    }

    private void OnDifficultyButton(DifficultyTypes difficulty)
    {
        if (!menuTransitioning)
        {
            //assigns the difficulty chosen to the GameFlowManager
            //loads the main game scene
            GameFlowManager.difficulty = difficulty;
            transitionAnimator.SetTrigger("CloseScene");
            StartCoroutine(LoadSceneDelay(Constants.MainGameScene));
        }
    }

    private void OnExitButton()
    {
        //when exiting, plays transition first before quitting
        if (!menuTransitioning)
        {
            transitionAnimator.SetTrigger("CloseScene");
            StartCoroutine(LoadSceneDelay(null));
        }
    }

    private IEnumerator LoadSceneDelay(string newScene)
    {
        audioScript.CreateSfxInstance(audioScript.sceneTransition);

        yield return new WaitForSeconds(sceneTransitionLength);

        //waits for animation before loading the new scene
        //if newScene is null, quits the application instead
        if (newScene != null)
            SceneManager.LoadScene(newScene);
        else
            Application.Quit();
    }
}
