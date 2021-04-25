using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    public static float DigSpeedIncrease
    {
        get {return Instance.digSpeedIncrease; }
    }

    public static float DigRangeIncrease
    {
        get { return Instance.digRangeIncrease; }
    }

    public static float AirRegenerationRateIncrease
    {
        get { return Instance.airRegenIncrease; }
    }

    public static float AirCapacityIncrease
    {
        get { return Instance.airCapIncrease; }
    }

    public static float SwimSpeedIncrease
    {
        get { return Instance.swimSpeedIncrease; }
    }

    public struct Upgrade
    {
        public string name;
        public string description;

        public int tier;

        public int copperCost;
        public int ironCost;
        public int goldCost;

        public Sprite image;

        public float digSpeedIncrease;
        public float digRangeIncrease;
         
        public float airRegenIncrease;
        public float airCapIncrease;

        public float swimSpeedIncrease;
    }

    private float digSpeedIncrease = 0;
    private float digRangeIncrease = 0;
        
    private float airRegenIncrease = 0;
    private float airCapIncrease = 0;

    private float swimSpeedIncrease = 0;

    private List<Upgrade> purchasedUpgrades;
    private List<Upgrade> currentShopUpgrades;
    private List<Upgrade> availableUpgrades;

    public Sprite flipperImage;
    public Sprite airJetImage;
    public Sprite airTankImage;
    public Sprite airIntakeImage;
    public Sprite longArmImage;

    public List<Upgrade> GetShopUpgrades()
    {
        while(currentShopUpgrades.Count < 4 && availableUpgrades.Count > 0)
        {
            int chosen = Random.Range(0, availableUpgrades.Count);
            currentShopUpgrades.Add(availableUpgrades[chosen]);
            availableUpgrades.RemoveAt(chosen);
        }
        return currentShopUpgrades;
    }

    public void PurchaseUpgrade(int shopIndex)
    {
        Upgrade purchased = currentShopUpgrades[shopIndex];
        currentShopUpgrades.RemoveAt(shopIndex);
        purchasedUpgrades.Add(purchased);

        digSpeedIncrease += purchased.digSpeedIncrease;
        digRangeIncrease += purchased.digRangeIncrease;
        airRegenIncrease += purchased.airRegenIncrease;
        airCapIncrease += purchased.airCapIncrease;
        swimSpeedIncrease += purchased.swimSpeedIncrease;

        int numToGenerate = Random.Range(0, 4) / 2; // 3 / 4 options become 1, 4th is 2
        if(numToGenerate == 0) numToGenerate = 1;
        while(numToGenerate > 0)
        {
            Upgrade nextTier = purchased;
            // If generate 2, make one of them probably cheaper
            nextTier.goldCost = (int)(nextTier.goldCost * Random.Range(1.5f, numToGenerate >= 2 ? 2.0f : 3.0f));
            nextTier.ironCost = (int)(nextTier.ironCost * Random.Range(1.5f, numToGenerate >= 2 ? 2.0f : 3.0f));
            nextTier.copperCost = (int)(nextTier.copperCost * Random.Range(1.5f, numToGenerate >= 2 ? 2.0f : 3.0f));
            nextTier.tier = purchased.tier + 1;
            availableUpgrades.Add(nextTier);
            numToGenerate--;
        }
    }

    private void Awake()
    {
        Instance = this;

        purchasedUpgrades = new List<Upgrade>();
        currentShopUpgrades = new List<Upgrade>();
        availableUpgrades = new List<Upgrade>();

        Upgrade flippers = new Upgrade();
        flippers.name = "Fancy Flippers";
        flippers.description = "Foot extensions seem to speed up swimming";
        flippers.tier = 0;
        flippers.copperCost = 5;
        flippers.ironCost = 10;
        flippers.goldCost = 0;
        flippers.image = flipperImage;
        flippers.digSpeedIncrease = 0;
        flippers.digRangeIncrease = 0;
        flippers.airRegenIncrease = 0;
        flippers.airCapIncrease = 0;
        flippers.swimSpeedIncrease = 0.1f;
        availableUpgrades.Add(flippers);
        
        Upgrade airJets = new Upgrade();
        airJets.name = "Air Propulsion";
        airJets.description = "Swim much faster, but uses some of your air";
        airJets.tier = 0;
        airJets.copperCost = 20;
        airJets.ironCost = 20;
        airJets.goldCost = 0;
        airJets.image = airJetImage;
        airJets.digSpeedIncrease = 0;
        airJets.digRangeIncrease = 0;
        airJets.airRegenIncrease = 0f;
        airJets.airCapIncrease = -5.0f;
        airJets.swimSpeedIncrease = 0.5f;
        availableUpgrades.Add(airJets);

        Upgrade airTank = new Upgrade();
        airTank.name = "Air Tank";
        airTank.description = "Turns out in this case bigger is better";
        airTank.tier = 0;
        airTank.copperCost = 10;
        airTank.ironCost = 0;
        airTank.goldCost = 5;
        airTank.image = airTankImage;
        airTank.digSpeedIncrease = 0;
        airTank.digRangeIncrease = 0;
        airTank.airRegenIncrease = 0f;
        airTank.airCapIncrease = 1.0f;
        airTank.swimSpeedIncrease = 0f;
        availableUpgrades.Add(airTank);

        Upgrade airIntake = new Upgrade();
        airIntake.name = "Air Intakes";
        airIntake.description = "Not sure this is the intended use, but air go brrr";
        airIntake.tier = 0;
        airIntake.copperCost = 8;
        airIntake.ironCost = 0;
        airIntake.goldCost = 4;
        airIntake.image = airIntakeImage;
        airIntake.digSpeedIncrease = 0;
        airIntake.digRangeIncrease = 0;
        airIntake.airRegenIncrease = 0.3f;
        airIntake.airCapIncrease = 0;
        airIntake.swimSpeedIncrease = 0;
        availableUpgrades.Add(airIntake);

        Upgrade longArms = new Upgrade();
        longArms.name = "Long Arms";
        longArms.description = "Allows you to reach things slightly further away";
        longArms.tier = 0;
        longArms.copperCost = 10;
        longArms.ironCost = 0;
        longArms.goldCost = 20;
        longArms.image = longArmImage;
        longArms.digSpeedIncrease = 0;
        longArms.digRangeIncrease = 1;
        longArms.airRegenIncrease = 0;
        longArms.airCapIncrease = 0;
        longArms.swimSpeedIncrease = 0;
        availableUpgrades.Add(longArms);

        Upgrade digSpeed = new Upgrade();
        digSpeed.name = "Drill Head Upgrade";
        digSpeed.description = "Chew through material much faster";
        digSpeed.tier = 0;
        digSpeed.copperCost = 10;
        digSpeed.ironCost = 0;
        digSpeed.goldCost = 20;
        digSpeed.image = longArmImage;
        digSpeed.digSpeedIncrease = 0.25f;
        digSpeed.digRangeIncrease = 0;
        digSpeed.airRegenIncrease = 0;
        digSpeed.airCapIncrease = 0;
        digSpeed.swimSpeedIncrease = 0;
        availableUpgrades.Add(digSpeed);
    }
}
