using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour
{
    public int index;
    private UpgradeManager.Upgrade upgrade;

    private UpgradeScreenUI theBoss;

    public Text upgradeTitleText;
    public Text upgradeDescriptionText;
    public Text copperPriceText;
    public Text ironPriceText;
    public Text goldPriceText;

    public Image upgradeIcon;
    

    public void Populate(int newIndex, UpgradeManager.Upgrade newUpgrade, UpgradeScreenUI newBoss)
    {
        upgrade = newUpgrade;
        index = newIndex;
        theBoss = newBoss;

        upgradeTitleText.text = upgrade.name;
        upgradeDescriptionText.text = upgrade.description;
        copperPriceText.text = upgrade.copperCost.ToString();
        ironPriceText.text = upgrade.ironCost.ToString();
        goldPriceText.text = upgrade.goldCost.ToString();
        upgradeIcon.sprite = upgrade.image;
    }   

    public void TryPurchase()
    {
        if(FindObjectOfType<Inventory>().TryPurchase(upgrade.goldCost, upgrade.ironCost, upgrade.copperCost))
        {
            UpgradeManager.Instance.PurchaseUpgrade(index);
            theBoss.RefreshUpgrades();

            Debug.Log("BOUGHT THING");
        }
        else
        {
            // TODO: feedback for cant afford
            Debug.Log("CANT AFFORD");
        }
    }
}
