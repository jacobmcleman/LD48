using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    private Inventory inventory;

    private Text copperText;
    private Text ironText;
    private Text goldText;

    private void Start()
    {
        inventory = FindObjectOfType<Inventory>();

        inventory.inventoryChanged.AddListener(OnInventoryChange);

        copperText = transform.Find("Copper").GetComponentInChildren<Text>();
        ironText = transform.Find("Iron").GetComponentInChildren<Text>();
        goldText = transform.Find("Gold").GetComponentInChildren<Text>();
    }

    private void OnInventoryChange()
    {
        copperText.text = inventory.copper.ToString();
        ironText.text = inventory.iron.ToString();
        goldText.text = inventory.gold.ToString();
    }
}
