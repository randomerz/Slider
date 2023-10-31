using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using System.Text.RegularExpressions;


[CreateAssetMenu(fileName = "Shape", menuName = "Scriptable Objects/Shape")]
public class Shape : ScriptableObject
{
    [FormerlySerializedAs("name")]
    public string shapeName;

    [FormerlySerializedAs("sprite")]
    public Sprite fullSprite;
    public Sprite outlineSprite;
    public Sprite emptySprite;

    public Sprite GetDisplaySprite()
    {
        return GetDisplaySprite(JungleRecipeBookSave.GetCompletionStatus(this));
    }

    public Sprite GetDisplaySprite(JungleRecipeBookSave.ShapeStatus status)
    {
        switch (status)
        {
            case JungleRecipeBookSave.ShapeStatus.Full:
            case JungleRecipeBookSave.ShapeStatus.PendingFull:
                return fullSprite;
            case JungleRecipeBookSave.ShapeStatus.Outline:
            case JungleRecipeBookSave.ShapeStatus.PendingOutline:
                return outlineSprite;
            case JungleRecipeBookSave.ShapeStatus.None:
            default:
                return emptySprite;
        }
    }

    public string GetDisplayName()
    {
        JungleRecipeBookSave.ShapeStatus status = JungleRecipeBookSave.GetCompletionStatus(this);
        
        switch (status)
        {
            case JungleRecipeBookSave.ShapeStatus.Full:
            case JungleRecipeBookSave.ShapeStatus.PendingFull:
                return shapeName;
            case JungleRecipeBookSave.ShapeStatus.Outline:
            case JungleRecipeBookSave.ShapeStatus.PendingOutline:
                return Regex.Replace(shapeName, "[a-zA-Z]", "_");
            case JungleRecipeBookSave.ShapeStatus.None:
            default:
                return "???";
        }
    }
}
