using System.Collections.Generic;
using Localization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheatsControlPanel : MonoBehaviour, IDialogueTableProvider
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
    private bool didSpawnAnchor;

    public enum CheatsControlPanelStrings
    {
        NoCollectibles,
        Give,
        Teleport,
        Undo,
        SpawnAnchor,
        RecallAnchor,
        NoClipOff,
        NoClipOn,
    }

    public Dictionary<string, LocalizationPair> TranslationTable { get; } = IDialogueTableProvider.InitializeTable(
        new Dictionary<CheatsControlPanelStrings, string>()
        {
            { CheatsControlPanelStrings.NoCollectibles, "No Collectibles!" },
            { CheatsControlPanelStrings.Give, "Give" },
            { CheatsControlPanelStrings.Teleport, "Teleport" },
            { CheatsControlPanelStrings.Undo, "Undo" },
            { CheatsControlPanelStrings.SpawnAnchor, "Spawn Anchor" },
            { CheatsControlPanelStrings.RecallAnchor, "Recall Anchor" },
            { CheatsControlPanelStrings.NoClipOff, "NoClip-Off" },
            { CheatsControlPanelStrings.NoClipOn , "NoCLip-On" }
        }
    );

    [SerializeField] private TextMeshProUGUI collectibleLabelText;
    [SerializeField] private TextMeshProUGUI collectibleButtonText;
    [SerializeField] private TextMeshProUGUI areaLabelText;
    [SerializeField] private TextMeshProUGUI areaButtonText;
    [SerializeField] private TextMeshProUGUI anchorText;
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

        noClipText.text = this.GetLocalizedSingle(isNoClipOn ? CheatsControlPanelStrings.NoClipOn : CheatsControlPanelStrings.NoClipOff);
        bool playerHasAnchor = PlayerInventory.Instance != null && PlayerInventory.Instance.GetHasCollectedAnchor();
        didSpawnAnchor = playerHasAnchor;
        anchorText.text = this.GetLocalizedSingle(playerHasAnchor
            ? CheatsControlPanelStrings.RecallAnchor
            : CheatsControlPanelStrings.SpawnAnchor);
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
        SetSpeedrunRestricted();

        bool playerHasAnchor = PlayerInventory.Instance != null && PlayerInventory.Instance.GetHasCollectedAnchor();
        if (!didSpawnAnchor && !playerHasAnchor)
        {
            Debug.Log($"[Cheats] Spawned Anchor");

            didSpawnAnchor = true;
            anchorText.text = this.GetLocalizedSingle(CheatsControlPanelStrings.RecallAnchor);
            Instantiate(anchorPrefab, Player.GetPosition(), Quaternion.identity);
            PlayerInventory.Instance.SetHasCollectedAnchor(true);
        }
        else
        {
            Debug.Log($"[Cheats] Recalled Anchor");

            PlayerInventory.ReturnAnchorFromMap();
        }
    }

    public void DoGiveScrollAndBoots()
    {
        Debug.Log($"[Cheats] Gave Scroll and Boots");
        SetSpeedrunRestricted();

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
        noClipText.text = this.GetLocalizedSingle(isNoClipOn ? CheatsControlPanelStrings.NoClipOn : CheatsControlPanelStrings.NoClipOff);
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

    private void SetSpeedrunRestricted()
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
            collectibleLabelText.text = this.GetLocalizedSingle(CheatsControlPanelStrings.NoCollectibles);
            collectibleLabelText.fontStyle = FontStyles.Normal;
            collectibleButtonText.text = this.GetLocalizedSingle(CheatsControlPanelStrings.Give);
            return;
        }

        bool isOn = collectibleIndexesToGive.Contains(index);
        collectibleLabelText.text = collectibles[index].GetTranslatedName();
        collectibleLabelText.fontStyle = isOn ? FontStyles.Underline : FontStyles.Normal;
        collectibleButtonText.text = this.GetLocalizedSingle(isOn ? CheatsControlPanelStrings.Undo : CheatsControlPanelStrings.Give);
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
        areaLabelText.text = ALL_AREAS[index].GetDisplayName();
        areaLabelText.fontStyle = isOn ? FontStyles.Underline : FontStyles.Normal;
        areaButtonText.text = (this as IDialogueTableProvider).GetLocalized(
                isOn ? CheatsControlPanelStrings.Undo : CheatsControlPanelStrings.Teleport
                ).TranslatedFallbackToOriginal;
    }

    private void DoSetScene(string sceneName)
    {
        Debug.Log($"[Cheats] Set Scene to {sceneName}");
        SetSpeedrunRestricted();
        DebugUIManager.justDidSetScene = true;
        sceneChanger.sceneName = sceneName.Trim();

        sceneChanger.ChangeScenes();
    }
}