using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakableTree : MonoBehaviour
{

    private bool isShaken;
    [SerializeField] public Collider2D myCollider;
    [SerializeField] private float pickUpDuration;
    [SerializeField] public AnimationCurve xPickUpMotion;
    [SerializeField] public AnimationCurve yPickUpMotion;
    public GameObject StuckPaper;

    // Start is called before the first frame update
    void Start()
    {
        isShaken = false;
    }

    private void Awake()
    {
        isShaken = false;
    }

    public void shakeTree()
    {
        if (isShaken)
        {
            Debug.Log("already shaken");
        } else
        {
            Debug.Log("you shake tree");
            GameObject instance = Instantiate(StuckPaper, myCollider.transform.position + new Vector3(1.1f, 1.1f), myCollider.transform.rotation, null) as GameObject;
            StartCoroutine(animateFallingPaper(instance, null));

            isShaken = true;
        }
        
    }


    protected IEnumerator animateFallingPaper(GameObject instance, System.Action callback = null)
    {
        float t = 0;
        Vector3 target = instance.transform.position + new Vector3(0.5f, -1.2f);
        SpriteRenderer sr = instance.GetComponent<Collectible>().getSpriteRenderer();

        Vector3 start = new Vector3(instance.transform.position.x, instance.transform.position.y);
        while (t < 1)
        {
            float x = xPickUpMotion.Evaluate(t / pickUpDuration);
            float y = yPickUpMotion.Evaluate(t / pickUpDuration);
            Vector3 pos = new Vector3(Mathf.Lerp(start.x, target.x, x),
                                      Mathf.Lerp(start.y, target.y, y));

            sr.transform.position = pos;

            yield return null;
            t += Time.deltaTime;
        }

        sr.transform.position = target;
        instance.GetComponent<BoxCollider2D>().transform.position += new Vector3(0.5f, -1.2f);//idk why this doesnt change on its own
        callback();
        //idk what callback does
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
