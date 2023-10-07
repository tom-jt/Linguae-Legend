using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisplayMessageManager : MonoBehaviour
{
    private int popupCount = 0;

    private AudioManager audioScript;

    [Header("Assignments")]
    [SerializeField]
    private GameObject bannerPrefab;
    [SerializeField]
    private GameObject popupPrefab;
    [SerializeField]
    private Transform popupRoot;

    [Header("Cosmetics")]
    public int popupCap = 4;

    private void Start()
    {
        audioScript = GameObject.FindGameObjectWithTag("GlobalObject").GetComponentInChildren<AudioManager>();
    }

    public void CreateBanner(string message, Color? backColor = null, float lifeTime = 2.5f)
    {
        GameObject createdBanner = Instantiate(bannerPrefab, transform);
        createdBanner.GetComponentInChildren<TextMeshProUGUI>().text = message;

        if (backColor != null)
            createdBanner.GetComponentInChildren<Image>().color = (Color)backColor;

        Destroy(createdBanner, lifeTime);

        audioScript.CreateSfxInstance(audioScript.bannerSlide);
    }

    public void CreatePopup(string message, float lifeTime = 1.5f)
    {
        if (popupCount >= popupCap)
            return;

        GameObject createdpopup = Instantiate(popupPrefab, popupRoot);
        createdpopup.GetComponentInChildren<TextMeshProUGUI>().text = message;
        Destroy(createdpopup, lifeTime);

        audioScript.CreateSfxInstance(audioScript.popupMsg);
    }

    private void OnEnable()
    {
        EventManager.PopupNotification += PopupCounter;
    }

    private void OnDisable()
    {
        EventManager.PopupNotification -= PopupCounter;
    }

    private void PopupCounter(bool addCounter)
    {
        popupCount += addCounter ? 1 : -1;
    }
}
