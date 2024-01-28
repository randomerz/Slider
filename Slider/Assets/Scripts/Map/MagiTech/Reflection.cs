using UnityEngine;

public class Reflection : MonoBehaviour
{
    public bool isAnimated;
    private bool isMoving = true;
    
    [Header("References")]
    [SerializeField] private SpriteRenderer mySpriteRenderer;
    [SerializeField] private SpriteRenderer targetSpriteRenderer;
    [Tooltip("Pivot will default to Player's feet unless specified.")]
    public Transform reflectionPivotTransform;

    private void Start()
    {
        if (SGrid.Current.GetArea() != Area.MagiTech)
        {
            gameObject.SetActive(false);
        }

        if (reflectionPivotTransform == null)
        {
            SetReflectionPivot(Player.GetInstance().GetPlayerFeetTransform());
        }

        UpdateSprite();
    }

    private void LateUpdate()
    {
        if (isMoving)
        {
            UpdatePosition();
        }
        if (isAnimated)
        {
            UpdateSprite();
        }
    }

    public void SetReflectionPivot(Transform reflectionPivot)
    {
        reflectionPivotTransform = reflectionPivot;
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        float offsetY = targetSpriteRenderer.transform.position.y - reflectionPivotTransform.position.y;
        Vector3 newPosition = new Vector3(reflectionPivotTransform.position.x, reflectionPivotTransform.position.y - offsetY);
        transform.position = newPosition;
    }

    private void UpdateSprite()
    {
        mySpriteRenderer.sprite = targetSpriteRenderer.sprite;
        mySpriteRenderer.flipX = targetSpriteRenderer.flipX;
        mySpriteRenderer.enabled = targetSpriteRenderer.enabled;
        // Color? Other attributes?
    }
}