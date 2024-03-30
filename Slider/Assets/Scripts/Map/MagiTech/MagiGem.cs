using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagiGem : MonoBehaviour, ISavable
{
    public GemManager gemMachine;
    public Item gemItem;
    protected bool isTransporting;
    public GameObject particles;

    public void Load(SaveProfile profile) {
        if (gemItem == null)
        {
            Debug.LogError("gem null on " + gameObject.name);
            return;
        }
        if (!gemItem.shouldDisableAtStart)
        {
            EnableGem(); //if gem active by default, disable if already collected
        }
    }

    //fix edge cases for leaving scene
    public virtual void Save()
    {
        if (isTransporting)
        {
            gemMachine.TransportGem(gemItem);
            gemMachine.Save();
        }
    }

    public virtual void EnableGem()
    {
        if (Enum.TryParse(gemItem.itemName, out Area itemNameAsEnum))
        {
            gemItem.gameObject.SetActive(!gemMachine.HasAreaGem(itemNameAsEnum));
        }
        else if (gemItem.itemName == "Mountory")
        {
            gemItem.gameObject.SetActive(!gemMachine.HasAreaGem(Area.Mountain));
        }   
    }

    public virtual void TransportGem()
    {
        if (!gemMachine.HasGemTransporter) return;
        isTransporting = true;
        StartCoroutine(TransportGemVFXCoroutine());
    }

    protected virtual IEnumerator TransportGemVFXCoroutine()
    {
        CameraShake.ShakeConstant(1f, 0.2f);
        ParticleManager.SpawnParticle(ParticleType.MiniSparkle, transform.position, transform);

        yield return new WaitForSeconds(1);
        
        //start teleport animation
        particles.SetActive(true);

        yield return new WaitForSeconds(0.2f);

        UIEffects.FlashWhite(speed: 2f);
        particles.transform.parent = transform.parent;

        ParticleManager.SpawnParticle(ParticleType.SmokePoof, transform.position);
        Item item = PlayerInventory.GetCurrentItem();
        if (item == gemItem)
            PlayerInventory.RemoveAndDestroyItem();
        else
            Destroy(gameObject);

        //update alchemy machine sprites and poof
        AudioManager.Play("Puzzle Complete");
        CameraShake.Shake(1f, 0.5f);
        
        FinishTransportGem();
    }

    protected virtual void FinishTransportGem()
    {
        gemMachine.TransportGem(gemItem);
        isTransporting = false;
    }
}
