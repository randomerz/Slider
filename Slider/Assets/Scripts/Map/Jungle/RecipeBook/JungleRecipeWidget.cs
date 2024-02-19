using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JungleRecipeWidget : MonoBehaviour
{
    public List<Image> recipeImages; // len 3
    public List<Image> plusSignImages; // len 2
    public Shape questionMarkShape; // constant

    public void SetIngredientsOrNull(Recipe.Shapes shapes, Recipe recipe)
    {
        if (shapes == null)
        {
            HideWidget();
            return;
        }

        Shape result = recipe.result;
        Recipe.Difficulty difficulty = recipe.difficulty;

        List<Shape> ingredients = shapes.ingredients;
        
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

        bool resultSpriteAtLeastOutlined = JungleRecipeBookSave.GetCompletionStatus(result) != JungleRecipeBookSave.ShapeStatus.None;
        bool recipeCraftedBefore = shapes.numberOfTimesCreated > 0;
        JungleRecipeBookSave.ShapeStatus displayStatus = GetDisplayStatus(resultSpriteAtLeastOutlined, recipeCraftedBefore);

        for (int i = 0; i < recipeImages.Count; i++)
        {
            recipeImages[i].enabled = i < ingredients.Count;
            if (i < ingredients.Count)
            {
                recipeImages[i].sprite = GetDisplaySprite(displayStatus, ingredients[i], difficulty, i == 0);
            }
        }
    }

    private JungleRecipeBookSave.ShapeStatus GetDisplayStatus(bool resultOutlined, bool recipeCraftedBefore)
    {
        if (recipeCraftedBefore)
        {
            return JungleRecipeBookSave.ShapeStatus.Full;
        }
        if (resultOutlined)
        {
            return JungleRecipeBookSave.ShapeStatus.Outline;
        }
        return JungleRecipeBookSave.ShapeStatus.None;
    }

    private Sprite GetDisplaySprite(JungleRecipeBookSave.ShapeStatus displayStatus, Shape shape, Recipe.Difficulty difficulty, bool notQuestionMark)
    {
        if (displayStatus == JungleRecipeBookSave.ShapeStatus.None)
        {
            return questionMarkShape.GetDisplaySprite(displayStatus);
        }
        if (displayStatus == JungleRecipeBookSave.ShapeStatus.Full)
        {
            return shape.GetDisplaySprite(displayStatus);
        }
        if (notQuestionMark)
        {
            return shape.GetDisplaySprite(displayStatus);
        }
        if (difficulty == Recipe.Difficulty.hard)
        {
            return questionMarkShape.GetDisplaySprite(displayStatus);
        }
        return shape.GetDisplaySprite(displayStatus);
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
