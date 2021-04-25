using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeScreenUI : MonoBehaviour
{
    public GameObject[] buttons;

    void OnEnable()
    {
        RefreshUpgrades();
    }

    public void RefreshUpgrades()
    {
        if(UpgradeManager.Instance == null) return;
        
        List<UpgradeManager.Upgrade> toShow = UpgradeManager.Instance.GetShopUpgrades();

        for(int i = 0; i < 4; ++i)
        {
            if(i < toShow.Count)
            {
                buttons[i].SetActive(true);
                buttons[i].GetComponent<UpgradeButton>().Populate(i, toShow[i], this);
            }
            else
            {
                buttons[i].SetActive(false);
            }
        }
    }
}
