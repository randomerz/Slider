using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesertCasino : MonoBehaviour
{
    public Item diceItem;
    public DiceGizmo dice1;
    public DiceGizmo dice2;
    public SpriteRenderer[] casinoCeilingSprites;
    public List<Animator> casinoSigns;

    private void Start()
    {
        if (SaveSystem.Current.GetBool("desertDiscoBallFell"))
        {
            RemoveDiceItem();
        }
    }

    private void Update()
    {
        UpdateDistanceToCasino();
    }

    private void UpdateDistanceToCasino()
    {
        float distToCasino = GetDistanceToCasino();
        AudioManager.SetGlobalParameter("DesertDistToCasino", distToCasino);
        UpdateCasinoCeilingSpriteAlpha(distToCasino);
    }

    private float GetDistanceToCasino()
    {
        Vector3 pp = Player.GetPosition();
        STile s5 = SGrid.Current.GetStile(5);
        float s5x = s5.transform.position.x + Mathf.Clamp(pp.x - s5.transform.position.x, 0, 8.5f);
        float dist5 = s5.isTileActive ? (pp - new Vector3(s5x, s5.transform.position.y)).magnitude : 17; // center
        STile s6 = SGrid.Current.GetStile(6);
        float s6x = s6.transform.position.x + Mathf.Clamp(pp.x - s6.transform.position.x, -8.5f, 0);
        if (PlayerInventory.Contains("Slider 9", Area.Desert))
        {
            // Let's look into the casino for the final part!
            s6x = s6.transform.position.x + Mathf.Clamp(pp.x - s6.transform.position.x, -8.5f, 8.5f);
        }
        float dist6 = s6.isTileActive ? (pp - new Vector3(s6x, s6.transform.position.y)).magnitude : 17; // center
        return Mathf.Min(dist5, dist6);

    }

    private void UpdateCasinoCeilingSpriteAlpha(float distToCasino)
    {
        // map [6, 8] => [0, 1]
        float alpha = Mathf.Clamp(Mathf.InverseLerp(6, 8, distToCasino), 0, 1);
        Color c = new Color(1, 1, 1, alpha);
        foreach (SpriteRenderer s in casinoCeilingSprites)
        {
            s.color = c;
        }   
    }

    public void SyncSignAnimations()
    {
        foreach (Animator a in casinoSigns)
                a.Play("Idle", -1, 0);
    }

    public void RemoveDiceItem()
    {
        if (PlayerInventory.GetCurrentItem() == diceItem)
            PlayerInventory.RemoveItem();
        diceItem.gameObject.SetActive(false);
    }

    public void CheckRolledDice(Condition c)
    {
        c.SetSpec(dice1.isActiveAndEnabled && dice2.isActiveAndEnabled);
    }

    public void CheckDiceValues(Condition c)
    {
        if (CheckCasinoTogether() && dice1.value + dice2.value == 11) 
            c.SetSpec(true);
        else if (SaveSystem.Current.GetBool("desertDice")) 
            c.SetSpec(true);
        else c.SetSpec(false);
    }

    public bool CheckCasinoTogether()
    {
        return CheckGrid.contains(SGrid.GetGridString(), "56")
        && !SGrid.Current.GetStile(5).IsMoving()
        && !SGrid.Current.GetStile(6).IsMoving();
    }
}
