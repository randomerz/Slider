using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JungleRecipeBook : MonoBehaviour
{
    public JungleRecipeBookUI jungleRecipeBookUI;

    private void Start() 
    {
        jungleRecipeBookUI.SetCurrentShape(0);
    }

    public void IncrementCurrentShape()
    {
        jungleRecipeBookUI.IncrementCurrentShape();
    }

}
