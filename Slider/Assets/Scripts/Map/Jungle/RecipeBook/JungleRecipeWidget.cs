using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JungleRecipeWidget : MonoBehaviour
{
    public List<Image> recipeImages; // len 3
    public List<Image> plusSignImages; // len 2

    public void SetIngredientsOrNull(List<Shape> ingredients)
    {
        if (ingredients == null)
        {
            HideWidget();
            return;
        }
        
        if (ingredients.Count != 2 && ingredients.Count != 3)
        {
            Debug.LogError($"Tried displaying recipe with {ingredients.Count} shapes.");
            HideWidget();
            return;
        }
        
        for (int i = 0; i < plusSignImages.Count; i++)
        {
            plusSignImages[i].enabled = i + 1 < ingredients.Count;
        }

        for (int i = 0; i < recipeImages.Count; i++)
        {
            recipeImages[i].enabled = i < ingredients.Count;
            if (i < ingredients.Count)
            {
                recipeImages[i].sprite = ingredients[i].fullSprite;
            }
        }
    }

    private void HideWidget()
    {
        foreach (Image i in recipeImages)
        {
            i.enabled = false;
        }
        foreach (Image i in plusSignImages)
        {
            i.enabled = false;
        }
    }
}
