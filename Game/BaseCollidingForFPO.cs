using System;
using UnityEngine;
using DWORD = System.UInt32;

public class BaseCollidingForFPO : iBaseColliding,IDisposable
{

    // от iBaseInterface
    public virtual object GetInterface(DWORD id) { return null; }

    // от iBaseColliding
    public virtual bool IsReady()
    {
        return (m_pFPO != null);
    }
    public virtual FPO GetFpo()
    {
        return m_pFPO;
    }
    public virtual Vector3 GetOrg()
    {
        return (m_pFPO != null ? m_pFPO.Org : Vector3.zero);
    }
    public virtual Vector3 GetDir()
    {
        return (m_pFPO != null ? m_pFPO.Dir : Vector3.forward);
    }
    public virtual Vector3 GetUp()
    {
        return (m_pFPO != null ? m_pFPO.Up : Vector3.up);
    }
    public virtual Vector3 GetRight()
    {
        return (m_pFPO != null ? m_pFPO.Right : Vector3.right);
    }
    public virtual float GetRadius()
    {
        return (m_pFPO != null ? m_pFPO.HashRadius : .0f);
    }
    public virtual float GetWeight()
    {
        return 0f;
    }
    public virtual float GetMaxSpeed()
    {
        return 0f;
    }
    public virtual Vector3 GetSpeed() {
        return Vector3.zero;
    }
    public virtual Vector3 GetSpeedFor(Vector3 Org)
    {
        return Vector3.zero;
    }
    public virtual void Rewind(float scale) { }
    public virtual void MakeStep(float scale) { }
    public virtual void ApplyForce(Vector3 Force, Vector3 Org) { }

    public T GetInterface<T>() where T : iBaseInterface
    {
        throw new NotImplementedException();
    }

    // own
    private FPO m_pFPO;
    public BaseCollidingForFPO(FPO fpo)
    {
        m_pFPO = fpo;
    }
    public void Dispose() { }
}