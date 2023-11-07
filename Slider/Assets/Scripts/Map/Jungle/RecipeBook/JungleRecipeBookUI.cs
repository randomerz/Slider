using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JungleRecipeBookUI : MonoBehaviour
{
    public RecipeList recipeList;

    public Image displayImage;
    public TextMeshProUGUI displayNameText;
    public TextMeshProUGUI displayNumberText;
    public TextMeshProUGUI recipeCompletionText;
    public List<JungleRecipeWidget> recipeWidgets; // There are three of these
    public List<BumpChildren> bumps; // Unit can't serialize interfaces :(

    private int currentShapeIndex; // Horizontal
    private int currentRecipePageIndex; // Vertical
    private Shape currentShape;

    private const float BUMP_DELAY = 1f / 16f;

    public void SetCurrentShape(int index)
    {
        StartCoroutine(_SetCurrentShape(index));
    }

    private IEnumerator _SetCurrentShape(int index)
    {
        currentShapeIndex = index;
        currentShape = recipeList.list[index].result;
        
        displayImage.sprite = currentShape.GetDisplaySprite();
        displayNameText.text = currentShape.GetDisplayName();
        displayNumberText.text = (index + 1).ToString().PadLeft(2, '0');

        for (int i = 0; i < 3; i++)
        {
            bumps[i].DoBump(1);
            yield return new WaitForSeconds(BUMP_DELAY / 2);
        }

        bumps[3].DoBump(1);

        int totalRecipes = recipeList.list[index].combinations.Count;
        int completedRecipes = 0;
        foreach (Recipe.Shapes s in recipeList.list[index].combinations)
        {
            if (s.numberOfTimesCreated > 0)
            {
                completedRecipes += 1;
            }
        }
        recipeCompletionText.text = $"{completedRecipes}/{totalRecipes}";

        yield return new WaitForSeconds(BUMP_DELAY);

        SetCurrentRecipeDisplay(index, 0);
    }

    public void SetCurrentRecipeDisplay(int currentShapeIndex, int recipeIndex)
    {
        StartCoroutine(_SetCurrentRecipeDisplay(currentShapeIndex, recipeIndex));
    }

    private IEnumerator _SetCurrentRecipeDisplay(int currentShapeIndex, int recipeIndex)
    {
        currentRecipePageIndex = recipeIndex;

        for (int i = 0; i < recipeWidgets.Count; i++)
        {
            Recipe.Shapes shapes = null;
            if (recipeIndex + i < recipeList.list[currentShapeIndex].combinations.Count)
            {
                shapes = recipeList.list[currentShapeIndex].combinations[recipeIndex + i];
            }
            recipeWidgets[i].SetIngredientsOrNull(shapes, recipeList.list[currentShapeIndex].result);

            bumps[i + 4].DoBump(1);

            yield return new WaitForSeconds(BUMP_DELAY);
        }
    }

    public void IncrementCurrentShape()
    {
        int index = (currentShapeIndex + 1) % recipeList.list.Count;
        SetCurrentShape(index);
    }

    public void DecrementCurrentShape()
    {
        int index = (currentShapeIndex + recipeList.list.Count - 1) % recipeList.list.Count;
        SetCurrentShape(index);
    }

    public void IncrementRecipeDisplay()
    {
        int count = recipeList.list[currentShapeIndex].combinations.Count / 3;
        if (count <= 1)
            return;

        int index = (currentRecipePageIndex + 1) % count;
        SetCurrentRecipeDisplay(currentShapeIndex, index);
    }

    public void DecrementRecipeDisplay()
    {
        int count = recipeList.list[currentShapeIndex].combinations.Count / 3;
        if (count <= 1)
            return;
            
        int index = (currentRecipePageIndex + count - 1) % count;
        SetCurrentRecipeDisplay(currentShapeIndex, index);
    }
}
