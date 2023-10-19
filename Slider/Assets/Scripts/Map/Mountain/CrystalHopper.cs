using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalHopper : MonoBehaviour
{
    public Animator animator;
    public Minecart minecart;
    
    public void DispenseCrystal()
    {
        if(minecart.TryAddCrystals())
        {
            animator.Play("Dispense");
        }
    }
}
