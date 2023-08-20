using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class MagiLaserFlash : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    [SerializeField] private float angleDebug;

    public void SetAngle(float degrees)
    {
        angleDebug = degrees;

        if ((degrees - 45) % 90 == 0)
        {
            // 45-135-225-315 bounce
            animator.SetBool("is45Degrees", true);
            transform.rotation = Quaternion.Euler(0, 0, degrees - 45);
        }
        else if (degrees % 90 == 0)
        {
            // 0-90-180-270 bounce
            animator.SetBool("is45Degrees", false);
            transform.rotation = Quaternion.Euler(0, 0, degrees);
        }
        else
        {
            Debug.LogWarning("Set Laser Flash effect angle to an illegal angle: " + degrees);
        }
    }
}
