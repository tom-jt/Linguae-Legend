using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CutsceneManager : MonoBehaviour
{
    private IndividualCutsceneScript[] adventureDialogues;
    private IndividualCutsceneScript currentDialogueScript;
    private int currentDialogueLine;
    private Image currentCutsceneImage;
    private bool exitingCutscene;

    [Header("Assignments")]
    [SerializeField]
    private CameraPanScript cameraPanScript;
    [SerializeField]
    private GameObject cutsceneOverlay;
    [SerializeField]
    private Button nextDialogueButton;
    [SerializeField]
    private RectTransform imageRoot;
    [SerializeField]
    private TextMeshProUGUI dialogueTextBox;
    [SerializeField]
    private Animator dialogueAnimator;
    [SerializeField]
    private GameObject cutsceneImagePrefab;

    [Header("Cosmetics")]
    public float imageFadeOutDuration;

    private void OnEnable()
    {
        //initialise variables and button functions
        //NOTE that the button covers the entire screen
        adventureDialogues = GetComponentsInChildren<IndividualCutsceneScript>();
        nextDialogueButton.onClick.AddListener(OnNextButton);
        cutsceneOverlay.SetActive(false);

        exitingCutscene = false;
    }

    public bool TryFindCutscene(int progressIndex = 0)
    {
        //loops through each dialogue object and see if any of them matches the current difficulty and round
        //foreach loop functions similarly to Python's for loops
        //var is a flexible data type, similar to obj
        foreach (var dialogue in adventureDialogues)
        {
            if ((dialogue.csDifficulty == GameFlowManager.difficulty) &&
                (dialogue.csRound == progressIndex))
            {
                //dialogue is found, begins dialogue at index 0
                currentDialogueScript = dialogue;
                currentDialogueLine = 0;

                cutsceneOverlay.SetActive(true);
                dialogueAnimator.SetTrigger("FadeIn");

                //display the current dialogue, also does this when the button is clicked
                OnNextButton();
                return true;
            }
        }

        return false;
    }

    private IEnumerator FadeCurrentScreen(Image currentImage)
    {
        currentImage.GetComponent<Animator>().SetTrigger("FadeOut");

        //if the cutscene is ending, then fade out dialogue
        if (exitingCutscene)
            dialogueAnimator.SetTrigger("FadeOut");

        yield return new WaitForSeconds(imageFadeOutDuration);

        //destroy the current image
        Destroy(currentImage.gameObject);

        if (exitingCutscene)
        {
            //after cutscene ends, start the round and deactivate the cutscene overlay
            EventManager.Broadcast(EventManager.StartRound, true);
            cameraPanScript.ChangePanState(false);
            cutsceneOverlay.SetActive(false);
            exitingCutscene = false;
        }
    }

    private void OnNextButton()
    {
        if (exitingCutscene)
            return;

        //if the user clicks while the text is still appearing, make it immediately appear
        if (!dialogueAnimator.GetCurrentAnimatorStateInfo(0).IsName("CutsceneIdle"))
        {
            dialogueAnimator.SetTrigger("ReturnIdle");
        }
        else
        {
            //if the dialogues are exhausted, then end the cutscene
            if (currentDialogueLine >= currentDialogueScript.dialogue.Length)
            {
                exitingCutscene = true;
                ExitCutscene();
            }
            else
            {
                //otherwise, try changing the cutscene image and dialogue
                //then shift the dialogue counter up by 1
                dialogueTextBox.text = currentDialogueScript.dialogue[currentDialogueLine];
                dialogueAnimator.SetTrigger("FadeIn");

                TryChangeCutsceneImage();
                currentDialogueLine++;
            }

        }
    }

    private void TryChangeCutsceneImage()
    {
        //check if the dialogue counter is in the dialogueIndex array
        int imageIndex = System.Array.IndexOf(currentDialogueScript.dialogueIndex, currentDialogueLine);

        //if not, then -1 is returned, otherwise returns the position found
        if (imageIndex >= 0)
        {
            //if so, that means a new image would be generated
            GameObject generatedImage = Instantiate(cutsceneImagePrefab, imageRoot);

            //fades out current image
            if (currentCutsceneImage)
            {
                generatedImage.transform.SetAsFirstSibling();
                StartCoroutine(FadeCurrentScreen(currentCutsceneImage));
            }
            else
            {
                //if it's the first image of the cutscene, then fade it in instead
                generatedImage.GetComponent<Animator>().SetTrigger("FadeIn");
            }

            //pan the image to give it some animation
            currentCutsceneImage = generatedImage.GetComponent<Image>();
            currentCutsceneImage.sprite = currentDialogueScript.dialogueSprites[imageIndex];
            cameraPanScript.BeginObjectPan(currentCutsceneImage.rectTransform, currentDialogueScript.dialogueCameraPans[imageIndex]);
        }
    }

    private void ExitCutscene() => StartCoroutine(FadeCurrentScreen(currentCutsceneImage));
}
