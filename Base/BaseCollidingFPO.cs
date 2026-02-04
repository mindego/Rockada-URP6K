using System;
using UnityEngine;
using DWORD = System.UInt32;

//public class BaseCollidingFPO<T> : iBaseColliding where T : BaseObject
public class BaseCollidingFPO<T> : iBaseColliding where T : IUsesFPO
{
    public object GetInterface(DWORD id) { return null; }
    public bool IsReady() { return (myOwner.GetFpo() != null); }
    public FPO GetFpo() { return myOwner.GetFpo(); }
    public Vector3 GetOrg() { return (myOwner.GetFpo() != null ? myOwner.GetFpo().Org : Vector3.zero); }
    public Vector3 GetDir() { return (myOwner.GetFpo() != null ? myOwner.GetFpo().Dir : Vector3.forward); }
    public Vector3 GetUp() { return (myOwner.GetFpo() != null ? myOwner.GetFpo().Up : Vector3.up); }
    public Vector3 GetRight() { return myOwner.GetFpo().Right; }
    public float GetRadius() { return (myOwner.GetFpo() != null ? myOwner.GetFpo().HashRadius : .0f); }
    public float GetWeight() { return .0f; }
    public float GetMaxSpeed() { return .0f; }
    public Vector3 GetSpeed() { return Vector3.zero; }
    public Vector3 GetSpeedFor(Vector3 Org) { return Vector3.zero; }
    public void Rewind(float scale) { }
    public void MakeStep(float scale) { }
    public void ApplyForce(Vector3 Force, Vector3 Org) { }

    public T1 GetInterface<T1>() where T1 : iBaseInterface
    {
        throw new NotImplementedException();
    }

    private T myOwner;
    public BaseCollidingFPO(T c) {
        myOwner = c;
    }
}

public interface IUsesFPO
{
    public FPO GetFpo();
}