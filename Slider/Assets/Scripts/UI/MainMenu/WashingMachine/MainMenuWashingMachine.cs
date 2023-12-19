using UnityEngine;

public class MainMenuWashingMachine : MonoBehaviour
{
    public float rotationSpeed;
    
    public bool shouldMove;
    public Vector3 startPos;
    public Vector3 endPos;
    public float travelDuration;
    private float startTime;

    public SpriteRenderer spriteRenderer;

    private void Start()
    {
       startTime = Time.time;
    }

    private void Update()
    {
        transform.Rotate(new Vector3(0, 0, rotationSpeed * Time.deltaTime));

        if (shouldMove)
        {
            transform.position = Vector3.Lerp(startPos, endPos, (Time.time - startTime) / travelDuration);

            if (Time.time - startTime > travelDuration)
            {
                Destroy(gameObject);
            }
        }
    }
}