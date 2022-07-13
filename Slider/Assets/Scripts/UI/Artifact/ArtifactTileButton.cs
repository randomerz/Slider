using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ArtifactTileButton : MonoBehaviour
{
    private const int UI_OFFSET = 37;

    public ArtifactTileButtonAnimator buttonAnimator;
    [SerializeField] private UIArtifact buttonManager;

    [FormerlySerializedAs("emptySprite")]
    [SerializeField] private Sprite emptySpriteDefault;
    [SerializeField] private Sprite completedSprite;
    [SerializeField] private Sprite hoverSprite;
    [SerializeField] private Sprite blankSprite;
    [SerializeField] private List<ArtifactTBPlugin> plugins;

    // public static bool canComplete = false;
    public bool isComplete = false;
    // public bool isInMove = false;
    public int islandId = -1;
    public int x;
    public int y;

    public bool shouldFlicker = false;

    protected Sprite islandSprite;
    protected Sprite emptySprite;

    private bool hasInit;

    public STile MyStile { get; private set; }
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
        islandSprite = buttonAnimator.sliderImage.sprite;
        UseDefaultEmptySprite();
    }

    protected void OnDisable()
    {
        if (MyStile != null && MyStile.isTileActive)
        {
            SetSpriteToIslandOrEmpty();
        }
    }

    protected virtual void Start()
    {
        Init();
        UpdateTileActive();
    }

    public void UpdateTileActive()
    {
        if (!hasInit)
        {
            Init();
        }
        SetSpriteToIslandOrEmpty();
        SetPosition(MyStile.x, MyStile.y);
    }

    public virtual void SetPosition(int x, int y)
    {
        this.x = x;
        this.y = y;

        SetAnchoredPos(x, y);

        plugins.ForEach(plugin =>
        {
            plugin.OnPosChanged();
        });
    }

    public void SetSpriteToIslandOrEmpty()
    {
        if (!hasInit)
        {
            Init();
        }

        if (TileIsActive)
        {
            buttonAnimator.sliderImage.sprite = isComplete ? completedSprite : islandSprite;
        }
        else
        {
            SetSpriteToEmpty();
        }
    }

    public void SetSpriteToEmpty()
    {
        buttonAnimator.sliderImage.sprite = emptySprite;
    }

    public void SetSpriteToHover()
    {
        buttonAnimator.sliderImage.sprite = hoverSprite;
    }

    public void UseDefaultEmptySprite()
    {
        emptySprite = emptySpriteDefault;
    }

    public void SetEmptySprite(Sprite s)
    {
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

    //public void UpdatePushedDown()
    //{
    //    buttonAnimator.UpdatePushedDown();
    //}

    public void SetHighlighted(bool v)
    {
        buttonAnimator.SetHighlighted(v);
    }

    public void SetPushedDown(bool v)
    {
        buttonAnimator.SetPushedDown(v);
    }

    public void SetLightning(bool v)
    {
        buttonAnimator.SetLightning(v);
    }

    public void SetSelected(bool v)
    {
        buttonAnimator.SetSelected(v);
    }

    public void SetIsInMove(bool v)
    {
        buttonAnimator.SetIsForcedDown(v && TileIsActive);
    }

    public void SetComplete(bool value)
    {
        if (!TileIsActive)
            return;

        isComplete = value;
        SetSpriteToIslandOrEmpty();
    }

    public void SetShouldFlicker(bool shouldFlicker)
    {
        this.shouldFlicker = shouldFlicker;
    }

    public void Flicker(int numFlickers) 
    {
        if (!hasInit)
        {
            Init();
        }
        shouldFlicker = false;
        StartCoroutine(NewButtonFlicker(numFlickers));
    }

    public void FlickerImmediate(int numFlickers)
    {
        if (!hasInit)
        {
            Init();
        }
        shouldFlicker = false;
        StartCoroutine(NewButtonFlicker(numFlickers, true));
    }

    private IEnumerator NewButtonFlicker(int numFlickers, bool startOnBlank=false) {
        if (!startOnBlank)
        {
            SetSpriteToIslandOrEmpty();
            yield return new WaitForSeconds(.25f);
        }
        
        for (int i = 0; i < numFlickers; i++) 
        {
            buttonAnimator.sliderImage.sprite = blankSprite;
            yield return new WaitForSeconds(.25f);
            SetSpriteToIslandOrEmpty();
            yield return new WaitForSeconds(.25f);
        }
    }

    private void Init()
    {
        hasInit = true;
        MyStile = SGrid.current.GetStile(islandId);
        islandSprite = buttonAnimator.sliderImage.sprite;
        InitLinkButton();
    }

    private void InitLinkButton()
    {
        LinkButton = null;
        foreach (ArtifactTileButton b in buttonManager.buttons)
        {
            if (MyStile.linkTile != null && MyStile.linkTile == b.MyStile)
            {
                LinkButton = b;
                b.LinkButton = this;
            }
        }
    }
    private void SetAnchoredPos(int x, int y)
    {
        Vector3 pos = new Vector3((x % SGrid.current.height) - 1, y - 1) * UI_OFFSET; //C: i refuse to make everything into MT buttons
        GetComponent<RectTransform>().anchoredPosition = pos;
    }
}