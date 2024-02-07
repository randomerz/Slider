using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class ArtifactTileButton : MonoBehaviour
{
    private const int UI_OFFSET = 37;
    private const float SECONDS_BETWEEN_REPEAT_NEW_FLICKER = 8.5f;

    public ArtifactTileButtonAnimator buttonAnimator;
    public RectTransform imageRectTransform;
    public UIArtifact buttonManager;

    [FormerlySerializedAs("emptySprite")]
    [SerializeField] private Sprite emptySpriteDefault;
    public Sprite islandSpriteDefault; // public for editor tools
    public Sprite completedSprite; // public for editor tools
    private Sprite completedSpriteDefault;
    [SerializeField] private Sprite hoverSprite;
    [SerializeField] private Sprite blankSprite;
    [SerializeField] public List<ArtifactTBPlugin> plugins;


    public bool isComplete = false;
    public int islandId = -1;
    public int x;
    public int y;

    public Sprite islandSprite;
    protected Sprite emptySprite;
    private FlashWhiteImage[] buttonIcons; // power lines, minecarft junctions, etc
    private bool dontUpdateDefaultSpriteOnAwake;

    public STile MyStile {
        get
        {
            if (SGrid.Current == null)
            {
                return null;
            }
            return SGrid.Current.GetStile(islandId);
        }
    }
    public bool TileIsActive { 
        get
        {
            if (MyStile != null)
            {
                return MyStile.isTileActive;
            }
            return false;
        } 
    }
    public ArtifactTileButton LinkButton { get; private set; }

    protected void Awake() 
    {
        Init();
        if (!dontUpdateDefaultSpriteOnAwake)
        {
            RestoreDefaultIslandSprite();
        }
        RestoreDefaultEmptySprite();
    }

    protected void OnDisable()
    {
        if (MyStile != null && MyStile.isTileActive)
        {
            SetSpriteToIslandOrEmpty();
        }
    }

    public void Init()
    {
        LinkButton = null;
        completedSpriteDefault = completedSprite;
        foreach (ArtifactTileButton b in buttonManager.buttons)
        {
            if (MyStile != null && MyStile.linkTile != null && MyStile.linkTile == b.MyStile)
            {
                LinkButton = b;
                b.LinkButton = this;
            }
        }
    }

    protected virtual void Start()
    {
        UpdateTileActive();
        buttonIcons = GetComponentsInChildren<FlashWhiteImage>(includeInactive: true);
    }

    public void OnSelect()
    {
        if (Player.GetInstance().GetCurrentControlScheme() == "Controller")
        {
            SetControllerHoverHighlighted(true);
        }
    }

    public void OnDeselect()
    {
        SetControllerHoverHighlighted(false);
    }

    public void UpdateTileActive()
    {
        SetSpriteToIslandOrEmpty();

        //Fix for Conveyor Bug in Factory
        if (MyStile.GetMovingDirection() == Vector2.zero && UIArtifact.GetInstance().MoveQueueEmpty())
        {
            SetPosition(MyStile.x, MyStile.y, false);
        }
    }

    public virtual void SetPosition(int x, int y, bool animateChange=false)
    {
        //Debug.Log($"Changed button pos from ({this.x}, {this.y}) to ({x}, {y})");
        if (animateChange && TileIsActive)
        {
            // The "Travel" direction
            Vector2 dif = CalculatePositionAnimationVector(x,y);
            buttonAnimator.AnimatePositionFrom(-dif * 2);
        }

        this.x = x;
        this.y = y;

        SetAnchoredPos(x, y);

        plugins.ForEach(plugin =>
        {
            plugin.OnPosChanged();
        });

        SetSpriteToIslandOrEmpty();
    }

    protected virtual Vector2 CalculatePositionAnimationVector(int x, int y)
    {
        return new Vector2(x - this.x, y - this.y).normalized;
    }

    public void SetSpriteToIslandOrEmpty()
    {
        if (TileIsActive)
        {
            buttonAnimator.sliderImage.sprite = isComplete ? completedSprite : islandSprite;
        }
        else
        {
            SetSpriteToEmpty();
        }
    }

    public void SetSpriteToIsland()
    {
        buttonAnimator.sliderImage.sprite = isComplete ? completedSprite : islandSprite;
    }

    public void SetSpriteToEmpty()
    {
        buttonAnimator.sliderImage.sprite = emptySprite;
    }

    public void SetSpriteToHover()
    {
        buttonAnimator.sliderImage.sprite = hoverSprite;
    }

    public void RestoreDefaultIslandSprite()
    {
        islandSprite = islandSpriteDefault;
    }

    public void RestoreDefaultCompletedSprite()
    {
        completedSprite = completedSpriteDefault;
    }

    public void RestoreDefaultEmptySprite()
    {
        emptySprite = emptySpriteDefault;
    }

    public void RestoreDefaultEmptySpriteIfNotDefault(bool andUpdate=true)
    {
        if (emptySprite != emptySpriteDefault)
        {
            emptySprite = emptySpriteDefault;
            SetSpriteToIslandOrEmpty();
        }
    }

    public void SetIslandSprite(Sprite s)
    {
        dontUpdateDefaultSpriteOnAwake = true;
        islandSprite = s;
    }

    public void SetCompletedSprite(Sprite s)
    {
        // In case this is called before Init()
        if (completedSpriteDefault == null)
        {
            completedSpriteDefault = completedSprite;
        }
        dontUpdateDefaultSpriteOnAwake = true;
        completedSprite = s;
    }

    public void SetEmptySprite(Sprite s)
    {
        // dontUpdateDefaultSpriteOnAwake = true;
        emptySprite = s;
    }

    public void AfterStileMoveDragged(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (e.stile.islandId == islandId)
        {
            SetPushedDown(false);
        }
    }

    public void SelectButton()
    {
        buttonManager.SelectButton(this);
    }
    
    public void SetControllerHoverHighlighted(bool v)
    {
        buttonAnimator.SetControllerHoverHighlight(v);
    }

    public void SetHighlighted(bool v)
    {
        buttonAnimator.SetHighlighted(v);
    }

    public void SetPushedDown(bool v)
    {
        buttonAnimator.SetPushedDown(v);
        if (this.LinkButton != null)
        {
            this.LinkButton.buttonAnimator.SetPushedDown(v);
        }
    }

    public void SetLinkPushedDown()
    {
        this.LinkButton.SetPushedDown(!this.LinkButton.buttonAnimator.IsPushedDown);
    }

    public void SetLightning(bool v)
    {
        buttonAnimator.SetLightning(v);
    }

    public void SetScrollHighlight(bool v)
    {
        buttonAnimator.SetScrollHighlight(v);
    }

    public void SetSelected(bool v)
    {
        buttonAnimator.SetSelected(v);
        if (this.LinkButton != null)
        {
            this.LinkButton.buttonAnimator.SetSelected(v);
        }
    }

    public void SetIsInMove(bool v)
    {
        buttonAnimator.SetIsForcedDown(v && TileIsActive);
        if (this.LinkButton != null)
        {
            this.LinkButton.buttonAnimator.SetIsForcedDown(v && TileIsActive);
        }
    }

    public void SetComplete(bool value)
    {
        if (!TileIsActive)
            return;

        isComplete = value;
        SetSpriteToIslandOrEmpty();
    }

    public void Flicker(int numFlickers, bool repeat=true)
    {
        StartCoroutine(NewButtonFlicker(numFlickers, repeat: repeat));
    }

    public void FlickerImmediate(int numFlickers)
    {
        StartCoroutine(NewButtonFlicker(numFlickers, startOnBlank: true, repeat: false));
    }

    protected virtual void SetAnchoredPos(int x, int y)
    {
        Vector3 pos = new Vector3((x % SGrid.Current.Height) - 1, y - 1) * UI_OFFSET; 
        GetComponent<RectTransform>().anchoredPosition = pos;
    }

    private IEnumerator NewButtonFlicker(int numFlickers, bool startOnBlank=false, bool repeat=true) 
    {
        SetSpriteToIslandOrEmpty();
            
        if (startOnBlank)
        {
            // We wait until end of frame because sliderImage.sprite is written to often (usually by queue things)
            yield return new WaitForEndOfFrame();
        }
        else
        {
            yield return new WaitForSeconds(.25f);
        }

        foreach (FlashWhiteImage e in buttonIcons)
        {
            if (e.gameObject.activeSelf)
            {
                e.Flash(numFlickers);
            }
        }

        for (int i = 0; i < numFlickers; i++) 
        {
            buttonAnimator.sliderImage.sprite = blankSprite;
            yield return new WaitForSeconds(.25f);
            SetSpriteToIslandOrEmpty();
            yield return new WaitForSeconds(.25f);
        }

        if (!repeat)
        {
            yield break;
        }

        if (IsTileExplored())
        {
            yield break;
        }
        
        yield return new WaitForSeconds(SECONDS_BETWEEN_REPEAT_NEW_FLICKER);
        
        // if it's still unexplored then repeat
        if (IsTileExplored())
        {
            yield break;
        }

        StartCoroutine(NewButtonFlicker(numFlickers, startOnBlank));
    }

    private bool IsTileExplored()
    {
        return SGrid.Current.gridTilesExplored != null && SGrid.Current.gridTilesExplored.IsTileExplored(islandId);
    }
}