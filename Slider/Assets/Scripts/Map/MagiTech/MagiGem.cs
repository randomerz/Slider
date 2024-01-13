using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagiGem : MonoBehaviour, ISavable
{
    public GemManager gemMachine;
    public Item gemItem;
    private bool isTransporting;
    public GameObject particles;

    public void Load(SaveProfile profile) {}

    //fix edge cases for leaving scene
    public void Save()
    {
        if(isTransporting)
        {
            gemMachine.TransportGem(gemItem);
            gemMachine.Save();
        }
    }

    private void Start()
    {
        gemItem = GetComponent<Item>();
    }

    public void EnableGem()
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

    public void TransportGem()
    {
        if(!gemMachine.HasGemTransporter) return;
        isTransporting = true;
        StartCoroutine(TransportGemVFXCoroutine());
    }

    private IEnumerator TransportGemVFXCoroutine()
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
        if(item == gemItem)
            PlayerInventory.RemoveAndDestroyItem();
        else
            Destroy(gameObject);

        //update alchemy machine sprites and poof
        gemMachine.TransportGem(gemItem);
        AudioManager.Play("Puzzle Complete");

        CameraShake.Shake(1f, 0.5f);
        isTransporting = false;
        
    }
}
