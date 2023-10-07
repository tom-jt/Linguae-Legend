using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//these enums are essentially integers (indexed 0, 1, 2... unless specified)
//they attribute a name to those integers for coding clarity
//E.g. instead of gamemode = 0, 
//gamemode = GameModeTypes.Adventure is much more clear even though they are the same thing
//note that any data type in brackets, E.g. (int), explicitly converts the variable into that type
public enum GameModeTypes
{
    Adventure,
    Endless,
    Custom
}

//NOTE that rookie, normal, insane and easy, normal, hard respectively are used interchangeably
public enum DifficultyTypes
{
    Rookie,
    Normal,
    Insane
}

public enum BoxFlipStyles
{
    None,
    Flip,
    Swirl,
    Squish,
}

//main scripts are in MonoBehaviour class
public class GameFlowManager : MonoBehaviour
{
    public static GameModeTypes gameMode = GameModeTypes.Endless;
    public static DifficultyTypes difficulty = DifficultyTypes.Normal;
    public static BoxFlipStyles boxAnimStyle = BoxFlipStyles.Flip;
    public static bool enablePlayerLetterInput = true;
    public static bool useProfanityFilter = true;
    public static bool isBossStage = false;


    private int currentRound;
    private int roundsWon;
    private RectTransform adventureRoundHandle;
    private Image adventureRoundFill;

    private bool sliderMoving;
    private float startTime;
    private float currentHandlePosition;
    private float newHandlePosition;

    //array of the likelihood to generate secret words of varying lengths (cannot be 0 or 1)
    //see Constants script for the weighted randomisation
    private readonly int[] wordLengthRarities = new int[]
    {
        0, //0
        0, //1
        1, //2
        2, //3
        5, //4
        12, //5
        3, //6
        2, //7
        1, //8
        1, //9
        1, //10
        1, //11
        1, //12
        1, //13
        1, //14
        1, //15
    };

    private AudioManager audioScript;

    //these variables are to be assigned within the Unity Inspector
    //the inspector usually only allows you to assign public variables, but [SerializeField] allows private variables as well
    [Header("Assignments")]
    [SerializeField]
    private WordManager wordScript;
    [SerializeField]
    private LetterBoxManager letterBoxScript;
    [SerializeField]
    private KeyBoardManager keyboardScript;
    [SerializeField]
    private DisplayMessageManager messageScript;
    [SerializeField]
    private BoxEffectManager effectScript;
    [SerializeField]
    private CutsceneManager cutsceneScript;
    [SerializeField]
    private LootOverlayScript giveLootScript;
    [SerializeField]
    private EphemeralManager ephemeralScript;
    [SerializeField]
    private GameBackgroundScript gameBackgroundScript;

    //TextMeshProUGUI is basically a more versatile text box/label
    [SerializeField]
    private TextMeshProUGUI gameInfoText;

    [SerializeField]
    private GameObject adventureRoundSlider;
    [SerializeField]
    private Animator sceneTransitionAnimator;

    [Header("Cosmetics")]
    public float sceneTransitionLength;
    public float beforeSceneTransitionDelay;
    public float bossBannerDelay;

    [Header("Game Variables")]
    public float adventureSliderMoveDuration = 0.5f;
    public int standardWordLength = 5;
    public int standardAttempts = 6;
    public int adventureRoundSets = 5;
    public int bossStagePerXRounds = 5;

    //Unity runs functions in the order:
    //Awake() -> OnEnable() -> Start()
    private void Awake()
    {
        //at the start of the game, assigns these important variables with default values entered in the inspector
        WordGameClass.numberOfLettersInWord = standardWordLength;
        WordGameClass.numberOfAttempts = standardAttempts;
    }

    //one line functions use => notation instead of {}
    //enablePlayerLetterInput controls whether or not the player can input/delete letters
    private void ChangeInputState(bool newValue) => enablePlayerLetterInput = newValue;

    private void Start()
    {
        //finds the AudioManager in the scene
        audioScript = GameObject.FindGameObjectWithTag("GlobalObject").GetComponentInChildren<AudioManager>();

        sceneTransitionAnimator.SetTrigger("OpenScene");

        //prevents the player from inputting
        ChangeInputState(false);

        //these variables are initialised in Start() because they reset everytime the game is played
        sliderMoving = false;
        adventureRoundSets = 5 * ((int)difficulty + 1);
        currentRound = 1;
        roundsWon = 0;

        //runs once when the game is first setting up
        InitialSetup();
    }

    private void InitialSetup()
    {
        //activates adventure round progress slider if in adventure mode
        bool isAdventureMode = gameMode == GameModeTypes.Adventure;
        adventureRoundSlider.SetActive(isAdventureMode);
        if (isAdventureMode)
        {
            adventureRoundFill = adventureRoundSlider.GetComponentsInChildren<Image>()[2];
            adventureRoundHandle = adventureRoundSlider.GetComponentsInChildren<RectTransform>()[4];
        }

        //setup the keyboard in KeyboardManager
        keyboardScript.SetupKeyboard();

        //get a list of profanity words from a txt file
        WordDatabaseManager.GetProfanityList();

        //change the background music and cycle through a playlist
        audioScript.ChangeBackgroundMusic(audioScript.gamePlaylist[AudioManager.currentGamePlaylistIndex]);
        IncrementGameMusicIndex();

        //begin the first round
        InitiateRound();
    }

    public void InitiateRound()
    {
        //check whether the round is a boss stage, switch to boss music if so
        CheckBossStageAndMusic();

        //lingering effects are carried over from previous rounds (mostly used by ephemerals)
        //they include variables that can change how the round is initialised
        LingeringEffectsManager.CheckLingeringEffects();

        //generates a random secret word
        RandomiseSecretWord();

        //generates the grid of boxes and box effects
        InitiateBoxSetups();

        //lingering effects are reset after everything is setup
        LingeringEffectsManager.ResetLingeringEffects();

        //changes the game's background
        gameBackgroundScript.SetupBackground(currentRound);

        //if the game is in adventure mode, then it would try and see if there is a cutscene for that round
        //if there is one, the round does not start yet (because the cutscene will play first)
        //otherwise, a message is broadcasted to tell other scripts that the round is starting
        bool doStartRound = true;
        if ((gameMode == GameModeTypes.Adventure) && (cutsceneScript.TryFindCutscene(currentRound)))
            doStartRound = false;
        EventManager.Broadcast(EventManager.StartRound, doStartRound);

        //updates the UI onscreen
        UpdateGameInfoUI();
    }

    private void RandomiseSecretWord()
    {
        if (LingeringEffectsManager.presetNumberOfLetters == 0)
        {
            //if there is no preset
            //generates a random word length for the secret word
            //the word length rarities array is extracted to only contain the lengths that can be generated
            //easy mode: 4-5 length
            //normal mode: 4-6 length
            //hard mode: 4-7 length
            int minLength = standardWordLength - 1;
            int maxLength = standardWordLength + (int)difficulty;
            int[] wordLengthRange = new int[maxLength - minLength + 1];
            System.Array.Copy(wordLengthRarities, minLength, wordLengthRange, 0, maxLength - minLength + 1);

            //the word length is then determined through a weighted randomiser
            WordGameClass.numberOfLettersInWord = minLength + Constants.WeightedRandomiser(wordLengthRange);
        }

        //retrieves the word list with words of that length
        WordDatabaseManager.GetWordListForValidCheck(WordGameClass.numberOfLettersInWord);
        //generates a secret word with that length in the WordManager
        wordScript.GenerateSecretWord((int)difficulty <= 1);
    }

    private void InitiateBoxSetups()
    {
        //LetterBoxManager generates rows of boxes for the 'Wordle' component of the game
        letterBoxScript.SetupBoxes();

        //then, box effects are generated and placed into those tiles
        effectScript.InitiateEffectGeneration(currentRound);

        //when the grid is complete, the next vacant box is found so that the user can begin entering letters
        //NOTE that is first vacant box is NOT ALWAYS the first box of the grid, because of certain box effects
        letterBoxScript.FindNextVacantBox(0, false);
    }

    private void CheckBossStageAndMusic()
    {
        //if currentRound is divisible by bossStagePerXRounds, then it is a boss stage
        //(set to every 5 rounds)
        bool wasBossStage = isBossStage;
        isBossStage = currentRound % bossStagePerXRounds == 0;

        if (isBossStage)
        {
            //preset the number of letters
            LingeringEffectsManager.presetNumberOfLetters = standardWordLength + (int)difficulty;

            //change to boss music
            audioScript.ChangeBackgroundMusic(audioScript.bossPlaylist[AudioManager.currentBossPlaylistIndex]);
            AudioManager.currentBossPlaylistIndex = AudioManager.currentBossPlaylistIndex < audioScript.bossPlaylist.Length - 1 ?
                AudioManager.currentBossPlaylistIndex + 1 : 0;
        } 
        else if (wasBossStage)
        {
            //if exiting a boss stage, change music back
            audioScript.ChangeBackgroundMusic(audioScript.gamePlaylist[AudioManager.currentGamePlaylistIndex]);
            IncrementGameMusicIndex();
        }
    }

    //one line function but divided for clarity
    //the ? expression is a ternary operator, a shorthand for "if" statenents
    //E.g:
    //Variable = (Condition) ? (OutcomeA) : (OutcomeB)

    //is identical to

    //if (Condition) 
    //{
    //  Variable = OutcomeA;
    //}
    //else
    //{
    //  Variable = OutcomeB;    
    //}

    private void IncrementGameMusicIndex() =>
        AudioManager.currentGamePlaylistIndex = 
            AudioManager.currentGamePlaylistIndex < audioScript.gamePlaylist.Length - 1 ?
                AudioManager.currentGamePlaylistIndex + 1 : 0;

    private void OnEnable()
    {
        //these are delegate events, all stored and controlled by the EventManager
        //basically they are functions that are subscribed to a delegate (delegate += function) and can be collectively called by broadcasting a message
        //In InitiateRound() seen previously: EventManager.Broadcast(EventManager.StartRound, doStartRound);
        //It broadcasts EventManager.StartRound, which calls every function that subscribed to the delegate EventManager.StartRound
        //events can also pass variables into the subscribed functions
        //in this project, all events pass a boolean, which must be the parameter for all subscribing functions
        EventManager.GameEnd += InitiateGameEnd;
        EventManager.RoundEnd += RoundFinished;
        EventManager.StartRound += BeforeStartRound;
    }

    private void OnDisable()
    {
        //it is standard practice to subscribe and unsubscribe functions in OnEnable() and OnDisable()
        EventManager.GameEnd -= InitiateGameEnd;
        EventManager.RoundEnd -= RoundFinished;
        EventManager.StartRound -= BeforeStartRound;
    }

    private void BeforeStartRound(bool moveToGame)
    {
        //moveToGame is true when there is no cutscene to play or it ended already
        //note that one line condition/expression statements do not require {} 
        //if there is a boss stage, then create a boss banner
        if ((moveToGame) && (isBossStage))
            StartCoroutine(TryCreateBossBanner());

        //if moveToGame is true, the player is allowed to input letters and begin the actual gameplay
        ChangeInputState(moveToGame);
    }

    private IEnumerator TryCreateBossBanner()
    {
        //coroutines such as this IEnumerator are subprograms that allow for a delay in the code
        //denoted by WaitForSeconds(amount of time) or similar.
        //They run alongside the main line of code (like asynchronous functions)
        yield return new WaitForSeconds(bossBannerDelay);

        messageScript.CreateBanner("Boss Stage!", Constants.DarkRedColor);
        audioScript.CreateSfxInstance(audioScript.bossStage);
    }

    private void RoundFinished(bool wonRound)
    {
        //when a round is finished, the EventManager.RoundEnd is broadcasted, which this function is subscribed to
        if ((gameMode == GameModeTypes.Adventure) && (currentRound >= adventureRoundSets))
        {
            //trigger end game scene if the finished round is greater or equal to the number of rounds required in adventure
            EventManager.Broadcast(EventManager.GameEnd, true);
            //return; returns a null value (void functions have return type of null)
            //it ends the function prematurely
            return;
        }

        currentRound++;

        if (wonRound)
            roundsWon++;

        //a coroutine begins to reset the round so that a new round can be initiated
        StartCoroutine(ResetRoundProcess(false, true, wonRound));
    }

    //any initialised values of function parameters are default values if it's not specified when called
    public IEnumerator ResetRoundProcess(bool resetSameRound, bool generateSecretWord = true, bool wonRound = false)
    {
        //calls the function letterBoxScript.RemoveBoxes(), which returns a float that is used as the await time
        yield return new WaitForSeconds(letterBoxScript.RemoveBoxes());

        //reset the keyboard colours
        keyboardScript.ResetAllKeyColors();

        //give the player powerups and ephemerals here if they won the round
        if (wonRound)
        {
            giveLootScript.OpenOptionMenu();
        }
        else
        {
            //if the round is restarted, only the boxes get reset
            if (resetSameRound)
                ReInitiateCurrentRound(generateSecretWord);
            else //otherwise, the entire InitiateRound() process is called
                InitiateRound();
        }
    }

    public void ReInitiateCurrentRound(bool generateSecretWord)
    {
        if (generateSecretWord)
            wordScript.GenerateSecretWord((int)difficulty <= 1);

        InitiateBoxSetups();
        ChangeInputState(true);
    }

    private void InitiateGameEnd(bool won)
    {
        //when the game ends, determine what to do based on whether the player won or not
        EndMenuManager.wonGame = won;
        StartCoroutine(LoadNewScene(Constants.EndMenuScene, true));
    }

    public IEnumerator LoadNewScene(string newScene, bool incluldeDelay)
    {
        //loads the EndMenu scene after a delay if needed
        if (incluldeDelay)
            yield return new WaitForSeconds(beforeSceneTransitionDelay);

        sceneTransitionAnimator.SetTrigger("CloseScene");
        audioScript.CreateSfxInstance(audioScript.sceneTransition);

        yield return new WaitForSeconds(sceneTransitionLength);
        SceneManager.LoadScene(newScene);
    }

    private void UpdateGameInfoUI()
    {
        //onscreen information are updated when a new round is initiated
        string roundText = currentRound.ToString();
        if (gameMode == GameModeTypes.Adventure)
            roundText += " of " + adventureRoundSets;

        string difficultyText = difficulty.ToString();

        //$ before a string allows convenient variable replacement (similar to Python)
        //a shorthand for string.Format()
        gameInfoText.text = $"| {difficultyText} {gameMode} | Round {roundText} | Wins: {roundsWon} |";

        //animates the position of the round slider
        if (adventureRoundSlider.activeSelf)
        {
            startTime = Time.time;
            currentHandlePosition = adventureRoundHandle.localPosition.x;
            //Mathf.Lerp interpolates a value
            newHandlePosition = Mathf.Lerp(-369f, 369f, (float)currentRound / adventureRoundSets);

            sliderMoving = true;
        }
    }

    //Update() is ran EVERY SINGLE FRAME AFTER the Start() function
    //this section simply moves the adventure round slider every frame
    private void Update()
    {
        if (sliderMoving)
        {
            float timeRatio = (Time.time - startTime) / adventureSliderMoveDuration;

            if (timeRatio >= 1)
            {
                sliderMoving = false;
            }
            else
            {
                float roundFillRatio = Mathf.Lerp((float)(currentRound - 1) / adventureRoundSets, (float)currentRound / adventureRoundSets, timeRatio);
                adventureRoundFill.fillAmount = roundFillRatio;

                float handlePosition = Mathf.Lerp(currentHandlePosition, newHandlePosition, timeRatio);
                adventureRoundHandle.Translate(handlePosition - adventureRoundHandle.localPosition.x, 0, 0);
            }
        }
    }
}
