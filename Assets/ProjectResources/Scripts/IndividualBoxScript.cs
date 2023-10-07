using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IndividualBoxScript : IndividualClass
{
    private RectTransform teleportLine;

    [HideInInspector]
    public object variable;
    [HideInInspector]
    public int[] boxPosition;
    [HideInInspector]
    public Button clickableButton;

    [HideInInspector]
    public EffectTypes currentBoxEffect = EffectTypes.None;
    [HideInInspector]
    public bool isVacant = true;
    [HideInInspector]
    public bool deletable = true;

    [Header("Assignments")]
    public Animator boxAnimator;
    public TextMeshProUGUI boxText;
    public GameObject teleportLinePrefab;
    public GameObject veilButtonPrefab;

    [Header("Sprites")]
    [SerializeField]
    private Sprite sprTeleportIn;
    [SerializeField]
    private Sprite sprTeleportOut;
    [SerializeField]
    private Sprite sprVeil;
    [SerializeField]
    private Sprite sprMist;
    [SerializeField]
    private Sprite sprLava;
    [SerializeField]
    private Sprite sprIce;
    [SerializeField]
    private Sprite sprChaos;
    [SerializeField]
    private Sprite sprHealth;

    [Header("Cosmetics")]
    public float revealColorDelay = 0.2f;
    public float teleportOutAnimLength = 0.3f;

    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateForeImage(originalForeColor = Constants.TransparentColor);
        clickableButton = GetComponentInChildren<Button>(true);
        clickableButton.gameObject.SetActive(false);
    }

    public virtual void OnBoxDestroy()
    {
        RemoveTeleportLine();
        RemoveVeilButton();
    }

    public string GetText() => boxText.text;

    public void UpdateText(string newText)
    {
        boxText.text = newText;
        boxAnimator.SetTrigger("TextChange");
        isVacant = string.IsNullOrEmpty(newText);
    }

    public void UpdateComparedBackColor(BackColorTypes colorType)
    {
        currentBoxBack = colorType;
        StartCoroutine(FlipAnimation(ComparisonColors[colorType]));
    }

    private IEnumerator FlipAnimation(Color newColor)
    {
        //play box reveal flipping animation here
        if (GameFlowManager.boxAnimStyle != BoxFlipStyles.None)
            boxAnimator.SetTrigger("Box" + GameFlowManager.boxAnimStyle.ToString());
        yield return new WaitForSeconds(revealColorDelay);
        UpdateBackImage(newColor);
    }

    public void TryResetBox()
    {
        if ((currentBoxEffect == EffectTypes.TeleportIn) || (currentBoxEffect == EffectTypes.TeleportOut))
            setOfBoxes[((int[])variable)[0], ((int[])variable)[1]].ResetBox();

        ResetBox();
    }

    private void ResetBox()
    {
        deletable = true;

        currentBoxEffect = EffectTypes.None;
        currentBoxBack = BackColorTypes.None;
        UpdateBackImage(originalBackColor);
        UpdateForeImage(originalForeColor);
        UpdateBorder(originalBorderColor);
        UpdateText(null);
        RemoveTeleportLine();
        RemoveVeilButton();
    }




    //Box Effects here
    public void Lava()
    {
        UpdateBackImage(GameInfo.BoxEffects[(int)EffectTypes.Lava].boxColor, sprLava);
        currentBoxEffect = EffectTypes.Lava;
    }

    public void Mist()
    {
        UpdateForeImage(Constants.GetTransparentColor(GameInfo.BoxEffects[(int)EffectTypes.Mist].boxColor, 0.5f), sprMist);
        currentBoxEffect = EffectTypes.Mist;
    }

    public void ForceLetter() => ForceLetter(null);

    public void ForceLetter(string presetString)
    {
        if (!string.IsNullOrEmpty(presetString))
        {
            UpdateText(presetString);
        }
        else
        {
            int randomIndex = Random.Range(0, Constants.keyboardChrs.Length);
            UpdateText(Constants.keyboardChrs[randomIndex].ToString());
        }

        deletable = false;
        UpdateBorder(GameInfo.BoxEffects[(int)EffectTypes.ForceLetter].boxColor);
        currentBoxEffect = EffectTypes.ForceLetter;
    }

    public void HealthBox()
    {
        UpdateBackImage(GameInfo.BoxEffects[(int)EffectTypes.HealthBox].boxColor, sprHealth);
        currentBoxEffect = EffectTypes.HealthBox;
    }

    public void PowerupBox() => PowerupBox(-1);

    public void PowerupBox(int presetPowerup)
    {
        if (presetPowerup == -1)
        {
            int powerupID = Constants.WeightedRandomiser(GameInfo.PowerupRarities);
            variable = powerupID;
        }
        else
        {
            variable = presetPowerup;
        }

        UpdateForeImage(Constants.GetTransparentColor(GameInfo.BoxEffects[(int)EffectTypes.PowerupBox].boxColor, 0.5f), GameInfo.Powerups[(int)variable].pixelImage);
        currentBoxEffect = EffectTypes.PowerupBox;
    }

    private void TeleportIn()
    {
        int outTeleportY;
        int outTeleportX;
        int repeatCount = 0;
        IndividualBoxScript chosenBox;
        do
        {
            outTeleportY = Random.Range((boxPosition)[0] + 1, numberOfAttempts - 1);
            outTeleportX = Random.Range(0, numberOfLettersInWord);
            chosenBox = setOfBoxes[outTeleportY, outTeleportX];
            repeatCount++;
        } while ((chosenBox.currentBoxEffect != EffectTypes.None) && (repeatCount < 100));

        if (repeatCount < 100)
        {
            variable = new int[] { outTeleportY, outTeleportX };
            chosenBox.variable = new int[] { boxPosition[0], boxPosition[1]};

            StartCoroutine(GenerateTeleportLine(chosenBox));

            chosenBox.SendMessage(EffectTypes.TeleportOut.ToString());

            UpdateBackImage(GameInfo.BoxEffects[(int)EffectTypes.TeleportIn].boxColor, sprTeleportIn);
            currentBoxEffect = EffectTypes.TeleportIn;
        }
        else
            return;
    }

    private IEnumerator GenerateTeleportLine(IndividualBoxScript chosenBox)
    {
        yield return new WaitForSeconds(0.5f);
        chosenBox.teleportLine = Instantiate(teleportLinePrefab, transform.parent.parent.parent.parent).GetComponent<RectTransform>();
        yield return new WaitForEndOfFrame();

        chosenBox.teleportLine.sizeDelta = new Vector2(Vector3.Distance(transform.position, chosenBox.transform.position), chosenBox.teleportLine.rect.height);

        chosenBox.teleportLine.SetPositionAndRotation(new Vector3((transform.position.x + chosenBox.transform.position.x) / 2,
        (transform.position.y + chosenBox.transform.position.y) / 2, -1),
        Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(chosenBox.transform.position.y - transform.position.y,
        chosenBox.transform.position.x - transform.position.x)));

        while (true)
        {
            yield return new WaitForEndOfFrame();

            if (!chosenBox.teleportLine)
                break;

            chosenBox.teleportLine.SetPositionAndRotation(new Vector3((transform.position.x + chosenBox.transform.position.x) / 2,
            (transform.position.y + chosenBox.transform.position.y) / 2, -1), chosenBox.teleportLine.rotation);
        }
    }

    private void TeleportOut()
    {
        UpdateBackImage(GameInfo.BoxEffects[(int)EffectTypes.TeleportOut].boxColor, sprTeleportOut);
        currentBoxEffect = EffectTypes.TeleportOut;
    }

    public IEnumerator TransitionToTeleportOut(string teleportedLetter)
    {
        teleportLine.GetComponent<Animator>().SetTrigger("TeleportOut");

        audioScript.CreateSfxInstance(audioScript.teleport);

        yield return new WaitForSeconds(teleportOutAnimLength);

        Destroy(teleportLine.gameObject);
        ResetBox();
        ForceLetter(teleportedLetter);
    }

    private void RemoveTeleportLine()
    {
        if (teleportLine)
        {
            teleportLine.GetComponent<Animator>().SetTrigger("TeleportOut");
            Destroy(teleportLine.gameObject, teleportOutAnimLength);
        }
    }

    private void Chaos()
    {
        UpdateBackImage(GameInfo.BoxEffects[(int)EffectTypes.Chaos].boxColor, sprChaos);
        currentBoxEffect = EffectTypes.Chaos;
    }

    private void Veil()
    {
        variable = Instantiate(veilButtonPrefab, transform).GetComponent<Button>();
        ((Button)variable).transform.SetSiblingIndex(transform.childCount - 2);
        ((Button)variable).onClick.AddListener(delegate { FindObjectOfType<BoxEffectManager>().OnVeilButton(this); });

        UpdateBackImage(GameInfo.BoxEffects[(int)EffectTypes.Veil].boxColor, sprVeil);
        currentBoxEffect = EffectTypes.Veil;
    }

    public bool VeilEffect()
    {
        if (string.IsNullOrEmpty(GetText()) || !GameFlowManager.enablePlayerLetterInput)
            return false;

        if (GetText() == secretWord[boxPosition[1]].ToString())
        {
            UpdateComparedBackColor(BackColorTypes.Correct);
        }
        else
        {
            bool foundMatch = false;
            int letterInSecret = 0;
            do
            {
                if (GetText() == secretWord[letterInSecret].ToString())
                {
                    UpdateComparedBackColor(BackColorTypes.WrongPos);
                    foundMatch = true;
                }
                letterInSecret++;
            } while ((letterInSecret < secretWord.Length) && (!foundMatch));

            if (!foundMatch)
                UpdateComparedBackColor(BackColorTypes.WrongLetter);
        }

        deletable = false;
        currentBoxEffect = EffectTypes.None;
        RemoveVeilButton();

        return true;
    }

    public void RemoveVeilButton()
    {
        if ((variable is Button button) && (button != null))
        {
            button.onClick.RemoveAllListeners();
            Destroy(button.gameObject);
        }
    }

    private void Ice()
    {
        UpdateForeImage(Constants.GetTransparentColor(GameInfo.BoxEffects[(int)EffectTypes.Ice].boxColor, 0.5f), sprIce);
        currentBoxEffect = EffectTypes.Ice;
    }

    public void IceEffect()
    {
        UpdateComparedBackColor((BackColorTypes)variable);
        StartCoroutine(RemoveForeImage());
    }

    private IEnumerator RemoveForeImage()
    {
        yield return new WaitForSeconds(revealColorDelay);
        UpdateForeImage(Constants.TransparentColor);
    }
}
