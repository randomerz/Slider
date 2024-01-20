using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using UnityEngine;


public class Path : MonoBehaviour
{
    public bool active = false;
    [SerializeField] private bool creatingBlobs = true;
    public Path pair;
    private Shape currentShape = null;
    bool defaultAnim = true; //used to see if the sprite needs to be flipped
    public bool horizontal = false;


    [Header("Animation Blobs")]
    public GameObject blob;
    private Direction direction;
    private float timeBetweenCreation = 3.8f;
    private float travelDistance = 0;

    private float timeCount = 4f;

    void Update()
    {
        if (active && creatingBlobs)
        {
            if (timeCount >= timeBetweenCreation)
            {
                CreateBlob();
                timeCount = 0;
            }
            timeCount += Time.deltaTime;
        }
    }

    public void CreateBlob()
    {
        GameObject go = Instantiate(blob);
        Blob new_blob = go.GetComponent<Blob>();
        new_blob.transform.parent = this.transform;

        BoxCollider2D collider = this.GetComponent<BoxCollider2D>();
        travelDistance = (int)this.transform.localScale.x; // + 0.5f

        if (pair != null)
        {
            travelDistance += (int)pair.transform.localScale.x;
        }

        new_blob.UpdateBlobOnPath(defaultAnim, direction, travelDistance, pair, currentShape);

        // set blob to be the correct starting position
        if (direction == Direction.LEFT)
        {
            new_blob.transform.localPosition = new Vector3(collider.offset.x + (collider.size.x / 2), 0, 0);
        } else if (direction == Direction.DOWN)
        {
            new_blob.transform.localPosition = new Vector3(collider.offset.x + (collider.size.x / 2) - 0.1f, 0, 0);
        }
        else
        {
            new_blob.transform.localPosition = new Vector3(collider.offset.x - (collider.size.x / 2), 0, 0);
        }
    }

    public void Activate(bool right, Shape shape, bool creating = true)
    {
        creatingBlobs = creating;
        active = true;

        //delete blobs if wrong shape or wrong direction
        //this is so gross I should see if i can fix the if statement
        bool deleted = false;
        if (defaultAnim != right || (currentShape != null && !shape.shapeName.Equals(currentShape.shapeName)))
        {
            foreach (Blob blob in this.gameObject.GetComponentsInChildren<Blob>())
            {
                Destroy(blob.gameObject);
                deleted = true;
            }
        }

        currentShape = shape;

        if (right)
        {
            defaultAnim = true;
        } else
        {
            defaultAnim = false;
        }

        if (horizontal)
        {
            if (defaultAnim)
            {
                direction = Direction.RIGHT;
            }
            else
            {
                direction = Direction.LEFT;
            }
        }
        else
        {
            if (defaultAnim)
            {
                direction = Direction.DOWN;
            }
            else
            {
                direction = Direction.UP;
            }
        }

        if (pair != null && !pair.isActive())
        {
            pair.Activate(right, shape, false);
        }

        //prepopulate some blobs if there are no blob
        bool pathHasNoBlobs = this.gameObject.transform.childCount == 0 && (pair == null || pair.gameObject.transform.childCount == 0);
        if ((pathHasNoBlobs || deleted) && creatingBlobs)
        {
            BoxCollider2D collider = this.GetComponent<BoxCollider2D>();
            float length = (int)this.transform.localScale.x;

            if (pair != null)
            {
                length += (int)pair.transform.localScale.x;
            }

            float distancebetween = 2.5f;
            int blobCount = (int)(length / distancebetween);

            for (int i = 0; i < blobCount; i++)
            {
                GameObject go = Instantiate(blob);
                Blob new_blob = go.GetComponent<Blob>();
                new_blob.transform.parent = this.transform;

                travelDistance = length;
                float traveledDistance = 0.15f * (i * distancebetween);

                // set blob to be the correct starting position
                if (direction == Direction.LEFT)
                {
                    new_blob.transform.localPosition = new Vector3(collider.offset.x + (collider.size.x / 2) - traveledDistance, 0, 0);
                }
                else if (direction == Direction.DOWN)
                {
                    new_blob.transform.localPosition = new Vector3(collider.offset.x + (collider.size.x / 2) - 0.1f - traveledDistance, 0, 0);
                    travelDistance -= 0.1f;
                }
                else
                {
                    new_blob.transform.localPosition = new Vector3(collider.offset.x - (collider.size.x / 2) + traveledDistance, 0, 0);
                }

                travelDistance -= (i * distancebetween);

                new_blob.UpdateBlobOnPath(defaultAnim, direction, travelDistance, pair, currentShape);
                new_blob.setSpeed(0);
                new_blob.setAlpha(0);
                new_blob.setTraveledDistance(traveledDistance);
            }
        }


        //fade in blobs
        foreach (Blob blob in this.gameObject.GetComponentsInChildren<Blob>())
        {
            blob.fadeIn();
        }
    }

    public void Deactivate()
    {
        //currentShape = null;
        active = false;

        if (pair != null && pair.isActive())
        {
            pair.Deactivate();
        }

        //fade out all the blobs
        foreach (Blob blob in this.gameObject.GetComponentsInChildren<Blob>()) {
            if (blob.isActiveAndEnabled)
            {
                blob.fadeOut();
            }
        }
    }

    public bool isActive()
    {
        return active;
    }

    public bool getAnimType()
    {
        return defaultAnim;
    }

    public void DeletePair()
    {
        pair = null;
    }

    public void ChangePair()
    {
        //pair = null;
        Vector2 one = new Vector2(1, 0);
        Vector2 two = new Vector2(-1, 0);


        Vector3 offset = new Vector3(0, 0.5f, 0);

        if (!horizontal)
        {
            offset = new Vector3(-0.5f, 0, 0);
        }

        Physics2D.queriesStartInColliders = false;

        RaycastHit2D checkOne = Physics2D.Raycast(transform.position + offset, one.normalized, (1 + this.transform.localScale.x / 2), LayerMask.GetMask("JunglePaths"));
        RaycastHit2D checkTwo = Physics2D.Raycast(transform.position + offset, two.normalized, (1 + this.transform.localScale.x / 2), LayerMask.GetMask("JunglePaths"));

        //want to find the closest bin or box and stile
        if (checkOne.collider != null)
        {
            Path other = checkOne.collider.gameObject.GetComponent<Path>();
            if (!other.transform.parent.Equals(this.transform.parent))
            {
                other.pair = this;
                pair = other;
            }
        }
        if (checkTwo.collider != null && pair == null)
        {
            Path other = checkTwo.collider.gameObject.GetComponent<Path>();
            if (!other.transform.parent.Equals(this.transform.parent))
            {
                if (this.gameObject.name == "house right")
                {
                    print(other.gameObject.name);
                }
                other.pair = this;
                pair = other;
            }
        }

        Physics2D.queriesStartInColliders = true;
    }

    void OnDrawGizmosSelected()
    {
        // Draws a 5 unit long red line in front of the object
        if (horizontal)
        {
            Gizmos.color = Color.red;
        }
        else
        {
            Gizmos.color = Color.blue;
        }

        Vector2 one = new Vector2(1, 0);
        Vector2 two = new Vector2(-1, 0);

        Vector3 offset = new Vector3(0, 0.5f, 0);

        if (!horizontal)
        {
            offset = new Vector3(-0.5f, 0, 0);
        }

        Vector3 directionone = transform.TransformDirection(one * (1 + this.transform.localScale.x / 2));
        Gizmos.DrawRay(transform.position + offset, directionone);
        Vector3 directiontwo = transform.TransformDirection(two * (1 + this.transform.localScale.x / 2));
        Gizmos.DrawRay(transform.position + offset, directiontwo);

    }
}
