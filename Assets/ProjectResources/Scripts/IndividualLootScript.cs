using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IndividualLootScript : MonoBehaviour
{
    public Animator animator;
    public TextMeshProUGUI lootType;
    public TextMeshProUGUI lootName;
    public Image icon;
    public TextMeshProUGUI lootAmount;
    public TextMeshProUGUI description;
    public Button lootButton;

    public void UpdateLootInfo(string type, string name, string desc, int amount, Sprite image)
    {
        lootType.text = type;
        lootName.text = Constants.AddSpacesToString(name);
        description.text = desc;
        icon.sprite = image;

        lootAmount.text = amount != 0 ? ("x" + amount.ToString()) : "";
    }
}
