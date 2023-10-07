using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GenericButtonScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    protected Button button;
    protected Animator buttonAnimator;
    protected AudioManager audioScript;

    [Header("Animation Triggers")]
    public string HoverTrigger;
    public string ExitTrigger;

    [Header("Cosmetics")]
    public AudioClip overrideHoverSfx;
    public AudioClip overrideButtonClickSfx;

    protected virtual void Start()
    {
        audioScript = GameObject.FindGameObjectWithTag("GlobalObject").GetComponentInChildren<AudioManager>();

        button = GetComponent<Button>();
        buttonAnimator = GetComponent<Animator>();
        button.onClick.AddListener(ButtonClickSound);
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        buttonAnimator.SetTrigger(HoverTrigger);

        audioScript.CreateSfxInstance(overrideHoverSfx ? overrideHoverSfx : audioScript.buttonHover);
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        buttonAnimator.SetTrigger(ExitTrigger);
    }

    protected virtual void ButtonClickSound() => audioScript.CreateSfxInstance(overrideButtonClickSfx ? overrideButtonClickSfx : audioScript.buttonClicked);
}
