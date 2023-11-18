using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JungleRecipeBookStatNumber : MonoBehaviour
{
    public float center = 0;
    public float std = 1; // fake std
    public bool useDecimal;
    private float myNumber;

    [Header("References")]
    public List<Image> numberImages;
    public List<Sprite> numberSprites;
    public Sprite decimalSprite;
    public Sprite emptySprite;

    private void Start() 
    {
        RefreshNumber();
    }

    public void RefreshNumber()
    {
        SetNumber(GenerateNewNumber());
    }

    private float GenerateNewNumber()
    {
        return center + Random.Range(-std, std);
    }

    private void SetNumber(float number)
    {
        myNumber = number;

        if (useDecimal)
        {
            string s = myNumber.ToString();

            if (s.StartsWith("."))
            {
                s = "0" + s;
            }

            int offset = numberImages.Count - Mathf.Min(numberImages.Count, s.Length);
            char[] charArray = s.ToCharArray();

            for (int i = 0; i < numberImages.Count; i++)
            {
                if (i < offset)
                {
                    numberImages[i].sprite = emptySprite;
                    continue;
                }
                if (charArray[i] == '.')
                {
                    numberImages[i].sprite = decimalSprite;
                    continue;
                }
                else
                {
                    numberImages[i].sprite = numberSprites[charArray[i] - '0'];
                }
            }
        }
        else
        {
            bool leadingZeros = true;
            for (int i = 0; i < numberImages.Count; i++)
            {
                float power = Mathf.Pow(10, numberImages.Count - 1 - i);
                int index = (int)(number / power) % 10;
                leadingZeros = leadingZeros && index == 0 && i != numberImages.Count - 1;

                if (leadingZeros)
                {
                    numberImages[i].sprite = emptySprite;
                }
                else
                {
                    numberImages[i].sprite = numberSprites[index];
                }
            }
        }
    }
}
