using UnityEngine;
using DWORD = System.UInt32;
using static iSensorsDefines;
/// <summary>
/// BaseCollidingForBaseCraft - переходних между iBaseColliding и BaseCraft
/// </summary>
public class BaseCollidingForBaseCraft : iBaseColliding
{

    // от iBaseInterface
    public virtual object GetInterface(DWORD id)
    {
        return id == iBaseColliding.ID ? this : mpCraft.GetInterface(id);
    }
    //template <class C> C * GetInterface() const { return (C*)GetInterface(C::ID); }

    // от iBaseColliding
    public virtual bool IsReady()
    {
        return (mpCraft.pFPO != null && mpCraft.GetState() == CS_IN_GAME);
    }
    public virtual FPO GetFpo()
    {
        return mpCraft.pFPO;
    }
    public virtual Vector3 GetOrg()
    {
        return (mpCraft.pFPO != null ? mpCraft.pFPO.Org : Vector3.zero);
    }
    public virtual Vector3 GetDir()
    {
        return (mpCraft.pFPO != null ? mpCraft.pFPO.Dir : Vector3.forward);
    }
    public virtual Vector3 GetUp()
    {
        return (mpCraft.pFPO != null ? mpCraft.pFPO.Up : Vector3.up);
    }
    public virtual Vector3 GetRight()
    {
        return (mpCraft.pFPO != null ? mpCraft.pFPO.Right : Vector3.right);
    }
    public virtual float GetRadius()
    {
        return (mpCraft.pFPO != null ? mpCraft.pFPO.HashRadius : .0f);
    }
    public virtual float GetWeight()
    {
        return mpCraft.W;
    }
    public virtual float GetMaxSpeed()
    {
        return mpCraft.SpeedF + mpCraft.CornerSpeed.magnitude * mpCraft.pFPO.HashRadius;
    }
    public virtual Vector3 GetSpeed()
    {
        return mpCraft.Speed;
    }
    public virtual Vector3 GetSpeedFor(Vector3 Org)
    {
        if (mpCraft.pFPO == null) return Vector3.zero;
        // переводим в локальную систему
        Vector3 dif = mpCraft.pFPO.ExpressPoint(Org);

        // определяем линейные скорости из угловых
        Vector3 tangent_speed = Vector3.Cross(mpCraft.CornerSpeed, dif);

        // переводим в мировую систему
        // добавляем линейную скорость
        return mpCraft.Speed + mpCraft.pFPO.ProjectVector(tangent_speed);
    }
    public virtual void Rewind(float scale)
    {
        if (mpCraft.pFPO == null) return;
        mpCraft.pFPO.Org -= mpCraft.Speed * scale;

        float theta = mpCraft.CornerSpeed.magnitude;
        if (theta > 0)
            mpCraft.pFPO.Rotate(mpCraft.CornerSpeed / theta, -scale * theta);
    }
    public virtual void MakeStep(float scale)
    {
        if (mpCraft.pFPO == null) return;

        mpCraft.pFPO.Org += mpCraft.Speed * scale;

        float theta = mpCraft.CornerSpeed.magnitude;
        if (theta > 0)
            mpCraft.pFPO.Rotate(mpCraft.CornerSpeed / theta, scale * theta);

        m_Force = Vector3.zero;
        m_ForceMomentum = Vector3.zero;
    }
    public virtual void ApplyForce(Vector3 Force, Vector3 Org)
    {
        if (mpCraft.pFPO == null) return;

        Vector3 Diff = mpCraft.pFPO.ExpressPoint(Org);
        Vector3 Frce = mpCraft.pFPO.ExpressVector(Force);

        Vector3 ForceMomentum, ForceRadial, ForceTangent;
        Momentum(Diff, Frce, mpCraft.GetRadius(), out ForceMomentum, out ForceRadial, out ForceTangent);
        m_ForceMomentum += ForceMomentum;
        m_Force += ForceRadial;

        mpCraft.Speed += mpCraft.GetFpo().ProjectVector(m_Force);

        mpCraft.CornerSpeed += mpCraft.GetFpo().ProjectVector(m_ForceMomentum);
    }

    // own
    private BaseCraft mpCraft;
    private Vector3 m_ForceMomentum;
    private Vector3 m_Force;

    public BaseCollidingForBaseCraft(BaseCraft c)
    {
        mpCraft = c;
        m_Force = Vector3.zero;
        m_ForceMomentum = Vector3.zero;
    }

    public void Dispose() { }
    void Momentum(Vector3 pos, Vector3 force, float Radius, out Vector3 momentum, out Vector3 radial, out Vector3 tangent)
    {
        //float r2=pos.Norma2();
        //float f2=force.Norma2();
        momentum = (Vector3.Cross(pos, force)) / (Mathf.Pow(Radius, 2));

        tangent = Vector3.Cross(momentum, pos);
        radial = force - tangent;//pos*((force*pos)/pos.Norma2());
    }
};