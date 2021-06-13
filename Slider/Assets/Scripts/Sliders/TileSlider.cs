using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileSlider : MonoBehaviour
{
    public bool isEmpty;

    public int islandId = -1;
    public int xPos;
    public int yPos;

    public AnimationCurve movementCurve;
    public float movementDuration = 1;
    public bool isMoving = false;

    private const int SLIDER_WIDTH = 17;

    [Header("References")]
    public Collider2D sliderCollider;
    public GameObject floorTileGrid;
    public GameObject wallTileGrid;
    public GameObject decorationsTileGrid;

    void Awake()
    {
        SetEmpty(isEmpty);
        Vector3 defaultPos = SLIDER_WIDTH * new Vector3(xPos, yPos);
        transform.position = defaultPos;
        SetTileMapPositions(defaultPos);
    }

    void Update()
    {
        
    }

    public void SetPosition(int x, int y)
    {
        xPos = x;
        yPos = y;
        Vector3 newPos = SLIDER_WIDTH * new Vector3(x, y);
        //Debug.Log("new position of tile " + islandId + ": " + newPos);

        StartCoroutine(StartCameraShakeEffect());

        if (!isEmpty)
        {
            // animations and style
            //if (Player.GetSliderUnderneath() == islandId)
            //{
            //    // set relative pos;
            //    Player.SetPosition(Player.GetPosition() - transform.position + newPos);
            //}
            //transform.position = newPos;
            //SetTileMapPositions(newPos);
            StartCoroutine(StartMovingAnimation(transform.position, newPos, Player.GetSliderUnderneath() == islandId, Player.GetPosition() - transform.position));
        }
        else
        {
            transform.position = newPos;
            SetTileMapPositions(newPos);
        }

        transform.position = newPos;
    }

    public void SetPositionRaw(int x, int y)
    {
        xPos = x;
        yPos = y;
        Vector3 newPos = SLIDER_WIDTH * new Vector3(x, y);
        transform.position = newPos;
        SetTileMapPositions(newPos);
        transform.position = newPos;
    }

    private IEnumerator StartMovingAnimation(Vector3 orig, Vector3 target, bool shouldMovePlayer, Vector3 playerOffset)
    {
        float t = 0;
        isMoving = true;

        while (t < movementDuration)
        {
            float x = movementCurve.Evaluate(t / movementDuration);
            Vector3 pos = (1 - x) * orig + x * target;

            if (shouldMovePlayer)
            {
                Player.SetPosition(playerOffset + pos);
            }
            transform.position = pos;
            SetTileMapPositions(pos);

            yield return null;
            t += Time.deltaTime;
        }

        isMoving = false;

        if (shouldMovePlayer)
        {
            Player.SetPosition(playerOffset + target);
        }
        transform.position = target;
        SetTileMapPositions(target);
    }

    private IEnumerator StartCameraShakeEffect()
    {
        CameraShake.ShakeConstant(movementDuration + 0.1f, 0.15f);

        yield return new WaitForSeconds(movementDuration);

        CameraShake.Shake(0.5f, 1f);
    }

    public void SetEmpty(bool isEmpty)
    {
        this.isEmpty = isEmpty;
        floorTileGrid.SetActive(!isEmpty);
        wallTileGrid.SetActive(!isEmpty);
        decorationsTileGrid.SetActive(!isEmpty);
        sliderCollider.isTrigger = !isEmpty;
    }

    private void SetTileMapPositions(Vector3 pos)
    {
        pos = pos + new Vector3(-0.5f, -0.5f);
        floorTileGrid.transform.position = pos;
        wallTileGrid.transform.position = pos;
        decorationsTileGrid.transform.position = pos;
    }
}
