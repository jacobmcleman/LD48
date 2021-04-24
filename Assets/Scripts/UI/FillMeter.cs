using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FillMeter : Meter
{
    public Image bar;

    public override void UpdateValue()
    {
        bar.fillAmount = normalizedVal;
    }
}
