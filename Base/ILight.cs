using UnityEngine;

public interface ILight : IObject
{
    public void SetRadius(float r);
    public float GetRadius();

    public void SetIntensity(float f);
    public void SetColor(Color v);
    public void SetPosition(Vector3 v);

    public IHashObject GetHashObject();
};
