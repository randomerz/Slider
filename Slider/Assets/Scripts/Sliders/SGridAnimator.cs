using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SGridAnimator : MonoBehaviour
{
    public bool isMoving = false;

    // set in inspector
    public AnimationCurve movementCurve;
    public float movementDuration = 1;

    public class OnTileMoveArgs : System.EventArgs
    {
        public STile stile;
    }
    public static event System.EventHandler<OnTileMoveArgs> OnSTileMove;


    private List<STile> collidersInactive = new List<STile>();
    


    public void Move(SMove move)
    {
        STile[,] grid = SGrid.GetGrid();
        foreach (Vector4Int m in move.moves)
        {
            if (grid[m.x, m.y].isTileActive)
            {
                StartCoroutine(StartMovingAnimation(grid[m.x, m.y], m));
            }
            else
            {
                grid[m.x, m.y].SetGridPosition(m.z, m.w);

                // if the player is on a slider, disable hitboxes temporarily
                if (Player.GetStileUnderneath() != -1)
                {
                    grid[m.x, m.y].SetSliderCollider(false);
                    collidersInactive.Add(grid[m.x, m.y]);
                }
            }
        }
    }

    private IEnumerator StartMovingAnimation(STile stile, Vector4Int moveCoords)
    {
        float t = 0;
        isMoving = true;

        Vector2 start = new Vector2(moveCoords.x, moveCoords.y);
        Vector2 end = new Vector2(moveCoords.z, moveCoords.w);
        stile.SetMovingDirection(GetMovingDirection(start, end));

        STile playerStile = null;
        if (Player.GetStileUnderneath() != -1)
        {
            playerStile = SGrid.GetStile(Player.GetStileUnderneath());
            playerStile.SetBorderCollider(true);
        }

        StartCoroutine(StartCameraShakeEffect());

        while (t < movementDuration)
        {
            float s = movementCurve.Evaluate(t / movementDuration);
            Vector2 pos = Vector2.Lerp(start, end, s);
            //Vector3 pos = (1 - s) * orig + s * target;
            
            stile.SetMovingPosition(pos);

            yield return null;
            t += Time.deltaTime;
        }

        isMoving = false;

        if (playerStile)
        {
            playerStile.SetBorderCollider(false);
        }
        foreach (STile inactiveStile in collidersInactive) {
            inactiveStile.SetSliderCollider(true);
        }
        collidersInactive.Clear();

        stile.SetMovingDirection(Vector2.zero);
        stile.SetGridPosition(moveCoords.z, moveCoords.w);

        InvokeOnSTileMove(stile);
    }

    private Vector2 GetMovingDirection(Vector2 start, Vector2 end)
    {
        Vector2 dif = start - end;
        if (dif.x > 0)
        {
            return Vector2.right;
        }
        else if (dif.x < 0)
        {
            return Vector2.left;
        }
        else if (dif.y > 0)
        {
            return Vector2.up;
        }
        else if (dif.y < 0)
        {
            return Vector2.down;
        }
        else
        {
            Debug.LogError("Moving Tile to the same spot!");
            return Vector2.zero;
        }
    }

    private IEnumerator StartCameraShakeEffect()
    {
        CameraShake.ShakeConstant(movementDuration + 0.1f, 0.15f);
        AudioManager.Play("Slide Rumble");

        yield return new WaitForSeconds(movementDuration);

        CameraShake.Shake(0.5f, 1f);
        AudioManager.Play("Slide Explosion");

    }


    private void InvokeOnSTileMove(STile stile)
    {
        OnSTileMove?.Invoke(this, new OnTileMoveArgs { stile = stile });
    }
}
