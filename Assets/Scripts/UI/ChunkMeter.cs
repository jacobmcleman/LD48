using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChunkMeter : Meter
{
    private Image[] chunks;

    public Color fullColor;
    public Color lowColor;
    public Color warningColor;

    private void Start()
    {
        chunks = transform.GetComponentsInChildren<Image>();
    }

    public override void UpdateValue()
    {
        int barsFilled = (int)(normalizedVal * chunks.Length - 1) + 1;

        Color setToColor = fullColor;
        if(barsFilled < 3) setToColor = warningColor;
        else if(barsFilled < chunks.Length - 1) setToColor = lowColor;

        for(int i = 0; i < chunks.Length; ++i)
        {
            chunks[i].color = setToColor;
            chunks[i].enabled = i < barsFilled;
        }
    }
}
