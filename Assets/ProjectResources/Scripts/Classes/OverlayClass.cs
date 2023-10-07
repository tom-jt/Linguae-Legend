using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//base class for all of the overlay scripts
public class OverlayClass : MonoBehaviour
{
    //protected variables mean that each subclass will have their own copy of this variable
    //only protected static variables are unique and shared amongst all subclasses
    protected bool currentInputBool;
    protected bool menuExiting;

    protected AudioManager audioScript;

    [Header("General Assignments")]
    [SerializeField]
    protected GameObject menu;
    [SerializeField]
    protected Button openOverlayButton;
    [SerializeField]
    protected Button closeOverlayButton;

    [Header("General Cosmetics")]
    public float animationDuration;

    protected virtual void Start()
    {
        //assign the open and close buttons with functions
        if (openOverlayButton)
            openOverlayButton.onClick.AddListener(delegate { ToggleOverlay(true); });

        if (closeOverlayButton)
            closeOverlayButton.onClick.AddListener(delegate { ToggleOverlay(false); });

        audioScript = GameObject.FindGameObjectWithTag("GlobalObject").GetComponentInChildren<AudioManager>();
        AddButtonSfx();
    }

    protected virtual void AddButtonSfx()
    {
        //loops through each button in the overlay and assigns them a sound effect when clicked
        Button[] allButtons = menu.GetComponentsInChildren<Button>(true);
        for (int button = 0; button < allButtons.Length; button++)
            allButtons[button].onClick.AddListener(delegate { audioScript.CreateSfxInstance(audioScript.buttonClicked); });
    }

    protected virtual void ToggleOverlay(bool toggleOn)
    {
        //broadcast that a menu has been toggled on/off
        //this triggers another script with darkens or undarkens the background
        EventManager.Broadcast(EventManager.OverlayMenuToggle, toggleOn);

        if (toggleOn)
        {
            //activates the overlay
            menu.SetActive(true);

            //time is frozen
            //meaning that in game animations and scripts are paused and do not update
            //NOTE that coroutines are also paused by this
            Time.timeScale = 0f;

            //saves the current input state then sets it to false regardless
            currentInputBool = GameFlowManager.enablePlayerLetterInput;
            GameFlowManager.enablePlayerLetterInput = false;
        }
        else
        {
            //this is used so that the open and close menu buttons cannot be clicked again while closing menu
            menuExiting = true;

            //restore the previous input state before the overlay was opened
            GameFlowManager.enablePlayerLetterInput = currentInputBool;
            StartCoroutine(DisableMenu());

            //resume script updates and animations
            Time.timeScale = 1f;
        }
    }

    private IEnumerator DisableMenu()
    {
        //allow close menu animation to play, then deactivate the menu
        yield return new WaitForSeconds(animationDuration);
        menu.SetActive(false);

        menuExiting = false;
    }
}
