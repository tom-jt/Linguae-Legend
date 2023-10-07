using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HelpOverlayScript : OverlayClass
{
    private bool infoStyle = false;
    private bool goToDefaultPanel = true;
    private Animator helpAnimator;

    [Header("Assignments")]
    public GameObject[] panelButtons;
    public GameObject[] infoPanels;
    public RectTransform scrollViewContent;
    public GameObject infoPrefabS1;
    public GameObject infoPrefabS2;
    public Sprite powerupOverviewImage;
    public Sprite ephemeralOverviewImage;
    public Sprite obstaclesOverviewImage;
    public Sprite benefitsOverviewImage;

    [Header("Cosmetics")]
    public GameObject defaultButton;
    [TextArea(5, 10)]
    public string powerupOverview;
    [TextArea(5, 10)]
    public string ephemeralOverview;
    [TextArea(5, 10)]
    public string obstaclesOverview;
    [TextArea(5, 10)]
    public string benefitsOverview;

    protected override void Start()
    {
        base.Start();
        menu.SetActive(false);

        helpAnimator = menu.GetComponent<Animator>();

        //cycles through each panel button and assigns them to toggle their respective panels
        for (int buttonIndex = 0; buttonIndex < panelButtons.Length; buttonIndex++)
        {
            GameObject button = panelButtons[buttonIndex];
            int functionIndex = buttonIndex;

            button.GetComponent<Button>().onClick.AddListener( 
                delegate
                {
                    TogglePanels(infoPanels[functionIndex]); 
                    ButtonVisuals(button); 
                });
        }

        //automatically generate the powerups/ephemerals and box effects panels
        BuildDynamicPanels();

        goToDefaultPanel = true;
    }

    protected override void ToggleOverlay(bool toggleOn)
    {
        if (menuExiting)
            return;

        base.ToggleOverlay(toggleOn);

        //the first time the help menu is activated, switch to default panel
        if ((toggleOn) && (goToDefaultPanel))
        {
            if (defaultButton)
                defaultButton.GetComponent<Button>().onClick.Invoke();
            goToDefaultPanel = false;
        }

        if (helpAnimator)
        {
            string triggerName = toggleOn ? "DropDown" : "ReturnUp";
            helpAnimator.SetTrigger(triggerName);
        }

        audioScript.CreateSfxInstance(toggleOn ? audioScript.overlaySlideDown : audioScript.overlaySlideUp);
    }

    private void BuildDynamicPanels()
    {
        //this boolean toggles between info block styles 1 and 2
        infoStyle = false;

        BuildInfoBlock(infoPanels[2].transform, 
            "Powerups - Overview",
            powerupOverview,
            powerupOverviewImage
            );

        //loops through each possible powerup, creates an infoblock
        //and attaches the name, description, and icon of the powerup to it
        foreach (PowerupConstructor powerup in GameInfo.Powerups)
            BuildInfoBlock(infoPanels[2].transform, powerup.name, powerup.description, powerup.icon);

        BuildInfoBlock(infoPanels[2].transform,
            "Ephemerals - Overview",
            ephemeralOverview,
            ephemeralOverviewImage
            );

        //loops through each possible ephemeral, creates an infoblock
        //and attaches the name, description, and icon of the ephemeral to it
        foreach (EphemeralConstructor ephemeral in GameInfo.Ephemerals)
            BuildInfoBlock(infoPanels[2].transform, ephemeral.name, ephemeral.description, ephemeral.pixelImage);

        BuildInfoBlock(infoPanels[3].transform,
            "Obstacles - Overview",
            obstaclesOverview,
            obstaclesOverviewImage
            );

        //loops through each possible box effect, creates an infoblock
        //and attaches the name, description, and image of the box effect to it
        foreach (BoxEffectConstructor boxEffect in GameInfo.BoxEffects.Values)
            if ((boxEffect.rarity != 0) && (boxEffect.index < 0))
                BuildInfoBlock(infoPanels[3].transform, boxEffect.name, boxEffect.description, boxEffect.helpImage);

        BuildInfoBlock(infoPanels[3].transform,
            "Benefits - Overview",
            benefitsOverview,
            benefitsOverviewImage
        );

        foreach (BoxEffectConstructor boxEffect in GameInfo.BoxEffects.Values)
            if ((boxEffect.rarity != 0) && (boxEffect.index > 0))
                BuildInfoBlock(infoPanels[3].transform, boxEffect.name, boxEffect.description, boxEffect.helpImage);
    }

    //creates an information block based on the parameters
    private void BuildInfoBlock(Transform parent, string header, string body, Sprite image)
    {
        GameObject infoBlock;
        TextMeshProUGUI[] textBoxes;
        Image infoImage;

        infoBlock = Instantiate(infoStyle ? infoPrefabS1 : infoPrefabS2, parent);
        infoStyle = !infoStyle;

        textBoxes = infoBlock.GetComponentsInChildren<TextMeshProUGUI>();
        textBoxes[0].text = header;
        textBoxes[1].text = body;

        infoImage = infoBlock.GetComponentInChildren<Image>();
        infoImage.sprite = image;
    }

    //make a certain panel active and all others inactive
    private void TogglePanels(GameObject newPanel)
    {
        for (int panel = 0; panel < infoPanels.Length; panel++)
            infoPanels[panel].SetActive(infoPanels[panel] == newPanel);

        //change the size of the content window so that the scroll wheel works
        StartCoroutine(ResizeContent(newPanel));
    }

    //WaitForEndOfFrame() is used to bypass Unity's technical limitation of layout groups
    private IEnumerator ResizeContent(GameObject referencePanel)
    {
        yield return new WaitForEndOfFrame();
        float resizeHeight = referencePanel.GetComponent<VerticalLayoutGroup>().minHeight;
        scrollViewContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, resizeHeight);
    }

    private void ButtonVisuals(GameObject buttonObject)
    {
        //extend the black background of the button into the contents layout
        //remove any extended black backgrounds of other buttons
        for (int button = 0; button < panelButtons.Length; button++)
        {
            RectTransform backImage = panelButtons[button].GetComponentsInChildren<Image>()[1].rectTransform;
            if (backImage)
                backImage.anchorMin = new Vector2(0, panelButtons[button] == buttonObject ? -0.11f : 0);
        }
    }
}
