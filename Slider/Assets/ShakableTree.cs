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
    [SerializeField] public PlayerConditionals myPlayerConditionals;
    public GameObject StuckPaper;

    // Start is called before the first frame update
    void Start()
    {
        isShaken = false;
        StuckPaper.SetActive(false);
    }

    public void shakeTree()
    {
        if (!isShaken)
        {
            StuckPaper.transform.position = myCollider.transform.position + new Vector3(1.1f, 1.1f);
            StartCoroutine(animateFallingPaper(StuckPaper, null));

            isShaken = true;
            myPlayerConditionals.disableConditionals();
            //myPlayerConditionals.addToOnAction = false;

            
        }
        
    }


    protected IEnumerator animateFallingPaper(GameObject instance, System.Action callback = null)
    {
        instance.SetActive(true);

        float t = 0;
        Vector3 target = instance.transform.position + new Vector3(0.5f, -1.2f);
        BoxCollider2D bc = instance.GetComponent<BoxCollider2D>();
        bc.enabled = false;

        Vector3 start = new Vector3(instance.transform.position.x, instance.transform.position.y);
        while (t < 1)
        {
            float x = xPickUpMotion.Evaluate(t / pickUpDuration);
            float y = yPickUpMotion.Evaluate(t / pickUpDuration);
            Vector3 pos = new Vector3(Mathf.Lerp(start.x, target.x, x),
                                      Mathf.Lerp(start.y, target.y, y));

            instance.transform.position = pos;

            yield return null;
            t += Time.deltaTime;
        }

        instance.GetComponent<BoxCollider2D>().enabled = true;
        bc.enabled = true;
        
        callback();
        //idk what callback does
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
