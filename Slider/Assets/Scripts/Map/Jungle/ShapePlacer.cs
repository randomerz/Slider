using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapePlacer : MonoBehaviour
{
    public GameObject shapeItem;
    public Shape makeShape;
    private bool making = false;

    void Update()
    {
        if (this.transform.childCount == 0 && making)
        {
            place(makeShape);
        }
    }

    public void place(Shape shape)
    {
        making = true;
        GameObject created = Instantiate(shapeItem);
        created.transform.parent = this.transform;
        // created.transform.GetChild(0).transform.localPosition = new Vector3(0f, -0.5f, 0);
        created.GetComponentInChildren<SpriteRenderer>().sprite = shape.fullSprite;
        created.GetComponentInChildren<Item>().itemName = shape.shapeName;
        created.transform.localPosition = new Vector3(0f, 0f, 0f);
        makeShape = shape;
    }

    public void stop()
    {
        makeShape = null;
        making = false;

        foreach (Item item in this.gameObject.GetComponentsInChildren<Item>())
        {
            Destroy(item.gameObject);
        }

        this.gameObject.SetActive(false);
    }
}
