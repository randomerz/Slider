using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheatsControlPanel : MonoBehaviour
{
    private List<Collectible> collectibles = new();
    private List<int> collectibleIndexesToGive = new();
    private int currentCollectibleIndex = 0;

    private readonly List<Area> ALL_AREAS = new() {
        Area.Village,
        Area.Caves,
        Area.Ocean,
        Area.Jungle,
        Area.Desert,
        Area.Factory,
        Area.Military,
        Area.Mountain,
        Area.MagiTech,
    };
    private Area areaToTeleportTo = Area.None;
    private int currentAreaIndex = 0;
    private AsyncOperation sceneLoad;

    private bool isNoClipOn;

    private const string GIVE_STRING = "Give";
    private const string TELEPORT_STRING = "Teleport";
    private const string UNDO_STRING = "Undo";
    private const string NO_CLIP_OFF = "NoClip-Off";
    private const string NO_CLIP_ON = "NoClip-On";

    [SerializeField] private TextMeshProUGUI collectibleLabelText;
    [SerializeField] private TextMeshProUGUI collectibleButtonText;
    [SerializeField] private TextMeshProUGUI areaLabelText;
    [SerializeField] private TextMeshProUGUI areaButtonText;
    [SerializeField] private TextMeshProUGUI noClipText;
    [SerializeField] private GameObject anchorPrefab;
    [SerializeField] private SceneChanger sceneChanger; // needs to stay active for coroutine

    private void OnEnable()
    {
        bool isInMenu = GameUI.instance.isMenuScene;
        if (isInMenu)
        {
            return;
        }

        if (SGrid.Current == null)
        {
            Debug.LogError($"SGrid.Current is null!");
            return;
        }

        collectibles = SGrid.Current.GetCollectibles();
        areaToTeleportTo = Area.None;
        currentCollectibleIndex = 0;
        currentAreaIndex = 0;
        SetCollectibleIndex(0);
        SetAreaIndex(0);

        noClipText.text = isNoClipOn ? NO_CLIP_ON : NO_CLIP_OFF;
    }

    public void CheckCheatsOnPauseClosed()
    {
        if (areaToTeleportTo != Area.None)
        {
            sceneChanger.ShowOverlayIfNotBusy();
            DoSetScene(areaToTeleportTo.ToString());
            areaToTeleportTo = Area.None;
            return;
        }

        if (collectibleIndexesToGive.Count != 0)
        {
            SetCheated();
            foreach (int i in collectibleIndexesToGive)
            {
                DoGive(collectibles[i].GetName());
            }
            collectibleIndexesToGive.Clear();
        }
    }

    public void DecrementCollectiblesList()
    {
        currentCollectibleIndex -= 1;
        if (currentCollectibleIndex < 0)
        {
            currentCollectibleIndex = collectibles.Count - 1;
        }
        SetCollectibleIndex(currentCollectibleIndex);
    }

    public void IncrementCollectiblesList()
    {
        currentCollectibleIndex += 1;
        if (currentCollectibleIndex > collectibles.Count - 1)
        {
            currentCollectibleIndex = 0;
        }
        SetCollectibleIndex(currentCollectibleIndex);
    }

    public void ToggleCurrentCollectible()
    {
        if (collectibleIndexesToGive.Contains(currentCollectibleIndex))
        {
            collectibleIndexesToGive.Remove(currentCollectibleIndex);
        }
        else
        {
            collectibleIndexesToGive.Add(currentCollectibleIndex);
        }

        SetCollectibleIndex(currentCollectibleIndex);
    }

    public void DecrementAreaList()
    {
        currentAreaIndex -= 1;
        if (currentAreaIndex < 0)
        {
            currentAreaIndex = ALL_AREAS.Count - 1;
        }
        SetAreaIndex(currentAreaIndex);
    }

    public void IncrementAreaList()
    {
        currentAreaIndex += 1;
        if (currentAreaIndex > ALL_AREAS.Count - 1)
        {
            currentAreaIndex = 0;
        }
        SetAreaIndex(currentAreaIndex);
    }

    public void ToggleCurrentArea()
    {
        if (areaToTeleportTo == ALL_AREAS[currentAreaIndex])
        {
            areaToTeleportTo = Area.None;
        }
        else
        {
            areaToTeleportTo = ALL_AREAS[currentAreaIndex];
        }
        
        SetAreaIndex(currentAreaIndex);
    }

    public void DoSpawnAnchor()
    {
        Debug.Log($"[Cheats] Spawned Anchor");
        SetCheated();

        Instantiate(anchorPrefab, Player.GetPosition(), Quaternion.identity);
    }

    public void DoGiveScrollAndBoots()
    {
        Debug.Log($"[Cheats] Gave Scroll and Boots");
        SetCheated();

        PlayerInventory.AddCollectibleFromData(new Collectible.CollectibleData("Scroll of Realigning", Area.Desert));

        PlayerInventory.AddCollectibleFromData(new Collectible.CollectibleData("Boots", Area.Jungle));
        Player.GetInstance().UpdatePlayerSpeed();
    }

    public void DoNoClip()
    {
        Debug.Log($"[Cheats] Called NoClip");
        SetCheated();

        Player p = GameObject.Find("Player").GetComponent<Player>();
        p.toggleCollision();

        isNoClipOn = !isNoClipOn;
        noClipText.text = isNoClipOn ? NO_CLIP_ON : NO_CLIP_OFF;
    }

    public void DoRespawnPlayer()
    {
        Debug.Log($"[Cheats] Called Respawn player");

        SetCheated();

        if (SceneSpawns.lastSpawn != SceneSpawns.SpawnLocation.Default)
        {
            // Try spawning player at that spawn
            SceneSpawns[] spawns = FindObjectsOfType<SceneSpawns>(includeInactive: true);
            foreach (SceneSpawns spawn in spawns)
            {
                if (spawn.spawnName == SceneSpawns.lastSpawn)
                {
                    spawn.TeleportPlayerToSpawn();
                    return;
                }
            }
        }

        if (SGrid.Current.DefaultSpawn != null)
        {
            if (SGrid.Current is FactoryGrid factoryGrid && FactoryGrid.PlayerInPast)
            {
                factoryGrid.DefaultSpawnFactoryPast.TeleportPlayerToSpawn();
                return;
            }

            SGrid.Current.DefaultSpawn.TeleportPlayerToSpawn();
            return;
        }
        else
        {
            Debug.LogError($"SGrid did not have a DefaultSpawn!");
        }
    }

    private void SetCheated()
    {
        if (SaveSystem.Current != null)
        {
            SaveSystem.Current.SetBool("UsedCheats", true);
        }
    }

    private void SetTeleported()
    {
        if (SaveSystem.Current != null)
        {
            SaveSystem.Current.SetBool("UsedTeleport", true);
        }
    }

    private void SetCollectibleIndex(int index)
    {
        currentCollectibleIndex = index;

        if (collectibles.Count == 0)
        {
            collectibleLabelText.text = "No Collectibles!";
            collectibleLabelText.fontStyle = FontStyles.Normal;
            collectibleButtonText.text = GIVE_STRING;
            return;
        }

        bool isOn = collectibleIndexesToGive.Contains(index);
        collectibleLabelText.text = collectibles[index].GetName();
        collectibleLabelText.fontStyle = isOn ? FontStyles.Underline : FontStyles.Normal;
        collectibleButtonText.text = isOn ? UNDO_STRING : GIVE_STRING;
    }

    private void DoGive(string collectibleName) 
    {
        Debug.Log($"[Cheats] Gave collecible: {collectibleName}");
        SGrid.Current.GivePlayerTheCollectible(collectibleName);
    }


    private void SetAreaIndex(int index)
    {
        currentAreaIndex = index;

        bool isOn = areaToTeleportTo == ALL_AREAS[index];
        areaLabelText.text = ALL_AREAS[index].ToString();
        areaLabelText.fontStyle = isOn ? FontStyles.Underline : FontStyles.Normal;
        areaButtonText.text = isOn ? UNDO_STRING : TELEPORT_STRING;
    }

    private void DoSetScene(string sceneName)
    {
        Debug.Log($"[Cheats] Set Scene to {sceneName}");
        SetTeleported();
        DebugUIManager.justDidSetScene = true;
        sceneChanger.sceneName = sceneName.Trim();

        sceneChanger.ChangeScenes();
    }
}