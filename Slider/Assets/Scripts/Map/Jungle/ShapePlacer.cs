using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapePlacer : MonoBehaviour
{
    public GameObject shapeItem;
    private List<GameObject> shapes;

    void Start()
    {
        shapes = new List<GameObject>();
    }

    public void place(Shape shape)
    {
        GameObject created = Instantiate(shapeItem);
        created.transform.parent = this.transform;
        created.GetComponentInChildren<SpriteRenderer>().sprite = shape.sprite;
        created.GetComponentInChildren<Item>().itemName = shape.name;
        created.transform.localPosition = new Vector3(0.5f, 0.5f, 0); // make this random for when 2 different shapes are created
        shapes.Add(created);
        created.SetActive(true);
    }
    public void remove(Item item)
    {
        Shape s = item.GetComponent<ShapeItem>().shape;
        //if it is picked up from the the blanket then remove it
        for (int i = 0; i < shapes.Count; i++)
        {
            if (s.name.Equals(shapes[i].GetComponent<ShapeItem>().name))
            {
                shapes.RemoveAt(i);
                place(s);
                break;
            }
        }
    }

    public void removeAll()
    {
        shapes = new List<GameObject>();
        foreach (Item item in this.gameObject.GetComponentsInChildren<Item>())
        {
            Destroy(item.gameObject);
        }
    }
}
