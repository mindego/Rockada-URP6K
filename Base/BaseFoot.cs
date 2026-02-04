using System;
using UnityEngine;
using DWORD = System.UInt32;
// базовый класс
class BaseFoot : BaseSubobj
{

    // от iBaseInterface
    new public const uint ID = 0x48396CB4;
    public override object GetInterface(DWORD id)
    {
        return (id == ID ? this : base.GetInterface(id));
    }

    // от BaseSubobj
    public BaseFoot(BaseScene s, DWORD h, SUBOBJ_DATA d) : base(s, h, d)
    {
        mpKnee = null;
        mpThigh = null;
    }

    public override void HostPrepare(HostScene s, BaseObject o, BaseSubobj so, FPO fpo, SLOT_DATA sld, int slot_id, int lay_id)
    {
        base.HostPrepare(s, o, so, fpo, sld, slot_id, lay_id);
        BasePrepare();
    }
    //virtual void RemotePrepare(RemoteScene*, BaseObject*, BaseSubobj*, FPO*,const SLOT_DATA*,const SubobjCreatePacket*, DWORD);// инициализация

    // own API
    public float GetStepDist(float MaxHipAngle)
    {
        return GetKneePos(MaxHipAngle) * Mathf.Sin(MaxHipAngle) * 2;
    }
    public void ProcessStep(float f, float tf, float MaxHipAngle, float MaxThighHeight)
    {
        if (pFPO == null) return;
        bool stay;
        // определяем свой коэфф.
        if (f < tf) { stay = false; f = f / tf; } else { stay = true; f = (1 - f) / (1 - tf); }
        // определяем угол поворота бедра
        float a = GetHipAngle(f, MaxHipAngle);
        // поворачиваем бедро
        pFPO.Dir = pFPO.ObjectData.Dir;
        pFPO.Right = Vector3.Cross(pFPO.Up, pFPO.Dir);
        pFPO.TurnRightPrec(a);
        // двигаем колено
        mpKnee.Org.z = GetKneePos(a);
        // поворачиваем и двигаем ляжку
        mpThigh.Up = mpThigh.ObjectData.Up;
        mpThigh.Right = Vector3.Cross(mpThigh.Up, mpThigh.Dir);
        mpThigh.BankLeftPrec(a);
        mpThigh.Org.z = GetThighPos(f, stay, MaxThighHeight);
    }
    //inline const FOOT_DATA* Dt() const { return (const FOOT_DATA*)SubobjData; }
    private FOOT_DATA Dt<T>() where T : FOOT_DATA { return (FOOT_DATA)SubobjData; }


    // own
    private FPO mpKnee;
    private FPO mpThigh;
    private void BasePrepare()
    {
        string spErrFormat = "Foot \"{0}\" of object \"{1}\": cannot find {2}";
        if (pFPO == null) return;
        // ищем колено
        mpKnee = (FPO)pFPO.GetSubObject("knee");
        if (mpKnee == null)
            throw new Exception(string.Format(spErrFormat, GetData().FullName, Owner.GetObjectData().FullName, "knee"));
        // ищем ляжку
        mpThigh = (FPO)mpKnee.GetSubObject("thigh");
        if (mpThigh == null)
            throw new Exception(string.Format(spErrFormat, GetData().FullName, Owner.GetObjectData().FullName, "leg"));
    }
    float GetHipAngle(float f, float amax)
    {
        return (Dt<FOOT_DATA>().IsLeft ? -amax * (1 - 2 * f) : amax * (1 - 2 * f));
    }
    float GetKneePos(float a) { return mpKnee.ObjectData.Org.z / Mathf.Cos(a); }
    float GetThighPos(float f, bool stay, float hmax)
    {
        return (stay ? mpThigh.ObjectData.Org.z : mpThigh.ObjectData.Org.z + hmax * (1 - Mathf.Pow(1 - f * 2, 2)));
    }
};
