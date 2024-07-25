using System;

[System.Serializable]
public class Resolution
{
    public int width;
    public int height;

    public Resolution(int width, int height)
    {
        this.width = width;
        this.height = height;
    }

    public static Resolution FromUnityStruct(UnityEngine.Resolution unityStruct)
    {
        return new Resolution(unityStruct.width, unityStruct.height);
    }

    public override bool Equals(object obj)
    {
        return obj is Resolution resolution &&
               width == resolution.width &&
               height == resolution.height;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(width, height);
    }
}