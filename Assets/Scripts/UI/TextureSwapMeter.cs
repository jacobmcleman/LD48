using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureSwapMeter : Meter
{
    public Sprite[] textureStates;


    public override void UpdateValue()
    {
        int textureIndex = (int)(normalizedVal * textureStates.Length);
        if(textureIndex >= textureStates.Length) textureIndex = textureStates.Length - 1;

        GetComponent<SpriteRenderer>().sprite = textureStates[textureIndex];
    }
}
