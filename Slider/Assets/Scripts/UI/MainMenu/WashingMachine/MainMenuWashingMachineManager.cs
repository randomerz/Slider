using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuWashingMachineManager : MonoBehaviour
{
    private const float DELAY_MUSIC_ONE = 32 - 6.4f;
    private const float DELAY_MUSIC_TWO = 58 - 6.4f;
    private const float TIME_ACROSS_SCREEN = 1.6f * 16;

    public MainMenuWashingMachine playerWashingMachine;
    public float playerRotationSpeed;

    public GameObject washingMachinePrefab;
    public List<Sprite> sprites;
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    private bool activated;
    private bool isFirstProp = true;

    public void Activate()
    {
        if (activated)
        {
            return;
        }

        activated = true;

        StartCoroutine(HandlePlayer());
        StartCoroutine(HandleProps());
    }

    private IEnumerator HandlePlayer()
    {
        yield return new WaitForSeconds(DELAY_MUSIC_ONE);

        float t = 0;
        while (t < Mathf.Abs(playerRotationSpeed))
        {
            playerWashingMachine.rotationSpeed = (t / Mathf.Abs(playerRotationSpeed)) * playerRotationSpeed;

            yield return null;
            t += Time.deltaTime;
        }
        playerWashingMachine.rotationSpeed = playerRotationSpeed;
    }

    private IEnumerator HandleProps()
    {
        yield return new WaitForSeconds(DELAY_MUSIC_TWO);

        for (int i = 0; i < 95000; i++)
        {
            SpawnProp();

            yield return new WaitForSeconds(TIME_ACROSS_SCREEN);
        }
    }

    private void SpawnProp()
    {
        Vector3 startPosition = new Vector3(minX, Random.Range(minY, maxY));
        Vector3 endPosition = new Vector3(maxX, Random.Range(minY, maxY));
        
        GameObject go = Instantiate(washingMachinePrefab, startPosition, Quaternion.Euler(0, 0, Random.Range(0, 360)));
        MainMenuWashingMachine wm = go.GetComponent<MainMenuWashingMachine>();
        wm.shouldMove = true;
        wm.rotationSpeed = Random.Range(-2 * playerRotationSpeed, 2 * playerRotationSpeed);
        wm.startPos = startPosition;
        wm.endPos = endPosition;
        wm.travelDuration = TIME_ACROSS_SCREEN;

        if (isFirstProp)
        {
            wm.spriteRenderer.sprite = sprites[0];
            isFirstProp = false;
        }
        else
        {
            wm.spriteRenderer.sprite = sprites[Random.Range(0, sprites.Count)];
        }
    }
}