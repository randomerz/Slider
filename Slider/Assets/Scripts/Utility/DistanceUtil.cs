using UnityEngine;

public class DistanceUtil
{
    public static float DistanceToLine(Vector3 point, Vector3 A, Vector3 B)
    {
        Vector3 AB = B - A;
        Vector3 AP = point - A;

        // float dot = Vector3.Dot(AB, AP);

        Vector3 proj = Vector3.Project(AP, AB);

        if (Vector3.Dot(AB, proj) < 0)
        {
            // behind A
            return AP.magnitude;
        }
        else if (proj.magnitude > AB.magnitude)
        {
            // after B
            return (point - B).magnitude;
        }
        else
        {
            return (proj - AP).magnitude;
        }
    }
}