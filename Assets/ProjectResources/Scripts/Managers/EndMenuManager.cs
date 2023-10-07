using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class EndMenuManager : MonoBehaviour
{
    public static bool wonGame = false;
    private Animator titleAnimator;

    private AudioManager audioScript;

    [Header("Assignments")]
    [SerializeField]
    private CutsceneManager cutsceneScript;
    [SerializeField]
    private Animator transitionAnimator;
    [SerializeField]
    private GameObject title;
    [SerializeField]
    private Button replayButton;
    [SerializeField]
    private Button toStartButton;
    [SerializeField]
    private Button exitButton;

    [Header("Cosmetics")]
    public float sceneTransitionLength;

    private void Start()
    {
        titleAnimator = title.GetComponent<Animator>();

        replayButton.onClick.AddListener(OnReplayButton);
        toStartButton.onClick.AddListener(OnToStartButton);
        exitButton.onClick.AddListener(OnExitButton);

        //change the title to reflect if the game is won or not
        TextMeshProUGUI titleText = title.GetComponentInChildren<TextMeshProUGUI>();
        titleText.text = wonGame ? "The Legend Prevails!" : "The Legend Falls...";

        //find the corresponding cutscene for the end menu
        int endDiaglogueIndex;
        if (!wonGame)
            endDiaglogueIndex = -1;
        else if (GameFlowManager.gameMode == GameModeTypes.Adventure)
            endDiaglogueIndex = 1;
        else
            endDiaglogueIndex = 0;

        //if the cutscene is found, broadcast event to start playing the cutscene
        if (!cutsceneScript.TryFindCutscene(endDiaglogueIndex))
            EventManager.Broadcast(EventManager.StartRound, false);

        //if the mode was adventure mode, try unlocking the next difficulty
        if (GameFlowManager.gameMode == GameModeTypes.Adventure)
            NewAdventureDiffUnlock();

        transitionAnimator.SetTrigger("OpenScene");

        audioScript = GameObject.FindGameObjectWithTag("GlobalObject").GetComponentInChildren<AudioManager>();
        audioScript.ChangeBackgroundMusic(wonGame ? audioScript.winGame : audioScript.loseGame);
    }

    private void NewAdventureDiffUnlock()
    {
        GameInfo gameInfoScript = GameObject.FindGameObjectWithTag("GlobalObject").GetComponentInChildren<GameInfo>();

        //if the completed difficulty is the highest the player had unlocked, unlock the next one
        if ((int)GameFlowManager.difficulty >= gameInfoScript.playerSave.advDiffModes)
            gameInfoScript.playerSave.advDiffModes++;
    }

    private void OnEnable()
    {
        EventManager.StartRound += ShowMenu;
    }

    private void OnDisable()
    {
        EventManager.StartRound -= ShowMenu;
    }

    //once the cutscene ends, play the title animation
    private void ShowMenu(bool ignore) => titleAnimator.SetTrigger("RevealTitle");

    //reload game scene
    private void OnReplayButton() => StartCoroutine(LoadNewScene(Constants.MainGameScene));

    //load start menu
    private void OnToStartButton() => StartCoroutine(LoadNewScene(Constants.StartMenuScene));

    //quit the application
    private void OnExitButton() => StartCoroutine(LoadNewScene(null));

    //very similar to the one in startMenuManager
    public IEnumerator LoadNewScene(string newScene)
    {
        transitionAnimator.SetTrigger("CloseScene");

        audioScript.CreateSfxInstance(audioScript.sceneTransition);

        yield return new WaitForSeconds(sceneTransitionLength);

        if (newScene != null)
            SceneManager.LoadScene(newScene);
        else
            Application.Quit();
    }
}
