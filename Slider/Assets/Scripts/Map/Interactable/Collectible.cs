using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Collectible : MonoBehaviour
{
    [System.Serializable]
    public class CollectibleData 
    {
        public string name;
        public Area area;

        public CollectibleData(string name, Area area)
        {
            this.name = name;
            this.area = area;
        }

        public override bool Equals(object obj)
        {
            if (obj is not CollectibleData)
                return false;

            return (
                this.name == (obj as CollectibleData).name && 
                this.area == (obj as CollectibleData).area
            );
        }

        public override int GetHashCode()
        {
            return name.GetHashCode() + area.GetHashCode();
        }
    }

    public UnityEvent onCollect;

    [SerializeField] private CollectibleData cData;
    public bool shouldDisableAtStart = false;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ParticleType particle = ParticleType.SmokePoof;
    
    private void Start() 
    {
        if (cData.area == Area.None)
            cData.area = SGrid.Current.GetArea();
        if (shouldDisableAtStart)
            gameObject.SetActive(false);
    }

    public void AttachToTileUnderneath()
    {
        TileUtil.AttachToTileUnderneath(gameObject);
    }

    public CollectibleData GetCollectibleData() 
    {
        return cData;
    }

    public string GetName() 
    {
        return cData.name;
    }

    public string GetTranslatedName()
    {
        return LocalizationLoader.LoadCollectibleTranslation(cData.name, cData.area);
    }

    public void SetArea(Area a) 
    {
        cData.area = a;
    }

    public Area GetArea() 
    {
        return cData.area;
    }


    public void DoPickUp(bool skipCutscene = false)
    {
        if(!skipCutscene)
            ItemPickupEffect.StartCutscene(spriteRenderer.sprite, cData.name, DoOnCollect);
        else
            DoOnCollect();
        DespwanCollectable(gameObject);
    }

    public void DoOnCollect() 
    {
        PlayerInventory.AddCollectible(this);
        onCollect.Invoke();
        SaveSystem.SaveGame($"New Collectible: {GetName()}");
    }

    public void SpawnCollectable(bool withAudio = true)
    {
        if(!gameObject.activeSelf)
        {
            if (withAudio)
                AudioManager.Play("Puzzle Complete");
            ParticleManager.SpawnParticle(particle, transform.position, transform);
        }
        gameObject.SetActive(true);
    }

    public void DespwanCollectable(GameObject item)
    {
        item.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            DoPickUp();
        }
    }

    public void AttachToSTileUnderneath()
    {
        TileUtil.AttachToTileUnderneath(gameObject);
    }


    // common methods for onCollect
    public void ActivateSTile(int stileId) 
    {
        SGrid.Current.CollectSTile(stileId);
        AchievementManager.IncrementAchievementStat("slidersCollected", false);
    }


    public SpriteRenderer getSpriteRenderer()
    {
        return spriteRenderer;
    }

}
