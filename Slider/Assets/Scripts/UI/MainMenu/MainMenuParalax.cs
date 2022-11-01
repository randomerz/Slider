using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenuParalax : MonoBehaviour
{
    [System.Serializable]
    public struct ParalaxPlane
    {
        public GameObject gameObject;
        public float paralaxFactor;
    }

    public List<ParalaxPlane> paralaxPlanes;
    private const float CATCH_UP_RATE = .5f;


    void Update()
    {
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector2 mouseUV = new Vector2(mouseScreenPos.x / Screen.width, mouseScreenPos.y / Screen.height);
        mouseUV = Vector2.ClampMagnitude(mouseUV, 2);
        Vector2 paralaxDir = mouseUV * 2 - new Vector2(1, 1); // Range from [-1, 1]

        foreach (ParalaxPlane p in paralaxPlanes)
        {
            Vector3 target = paralaxDir * p.paralaxFactor;
            // Vector3 start = p.gameObject.transform.position;
            // p.gameObject.transform.localPosition = Vector3.Lerp(start, target, CATCH_UP_RATE);
            p.gameObject.transform.localPosition = target;
        }
    }
}
