using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// оружие с отделяемыми частями
/// </summary>
public class BaseWeaponSlotDetaching : BaseWeaponSlot
{

    // от iBaseInterface
    new public const uint ID = 0x20F5E3EA;
    public override object GetInterface(DWORD id)
    {
        return id == ID ? this : base.GetInterface(id);
    }

    // от BaseItem
    //public virtual DWORD GetCreatePktLength(DWORD Code, DWORD Offset)const;
    //virtual DWORD GetCreatePkt(ItemDataPacket*, DWORD Offset)const;
    //virtual bool OnDataPacket(float PacketDelay,const ItemDataPacket*);

    // работа с оружием
    private int DetachingObjectsIdx;
    private FPO[] DetachingObjects = new FPO[4];
    private bool DetachObject(int idx)
    {
        if (DetachingObjects[idx] == null) return false;
        //Debug.Log("Trying to detach object, res: " + DetachingObjects[idx].Release());
        DetachingObjects[idx].Release();
        DetachingObjects[idx] = null;
        Owner.SubWeight(GetWpnData<WPN_DATA_DETACHING>().Mass);
        return true;
    }
    protected override void DoFire(iContact tgt, float ood, float dx, float dy)
    {
        Vector3 prevOrg = DetachingObjects[DetachingObjectsIdx].Org;

        Asserts.AssertBp(DetachingObjects[DetachingObjectsIdx] != null);
        // готовим матрицу
        DetachingObjects[DetachingObjectsIdx].ToWorld();
        // стреляем

        //Debug.LogFormat("Object {0} recalced pos {1}->{2}", DetachingObjects[DetachingObjectsIdx].TextName,prevOrg, DetachingObjects[DetachingObjectsIdx].Org);
        LocalFire(tgt, (MATRIX)DetachingObjects[DetachingObjectsIdx]);
    }
    public virtual void LocalFire(iContact tgt, MATRIX pos)
    {
        // отрываем объект
        DetachObject(DetachingObjectsIdx++);
        // сообщаем всем клиентам
        //TODO Реализовать обмен данными?
        //WeaponSlotDetachingNObjects s = new WeaponSlotDetachingNObjects(GetHandle(), DetachingObjectsIdx);
        //rScene.SendItemData(s);
    }
    public int GetDetachingObjectsIdx() { return DetachingObjectsIdx; }
    //public const WPN_DATA_DETACHING* GetWpnData() const { return (const WPN_DATA_DETACHING*)SubobjData; }
    private WPN_DATA_DETACHING GetWpnData<T>() where T : WPN_DATA_DETACHING
    {
        return (WPN_DATA_DETACHING)GetWpnData();
    }
    public override int GetStatus()
    {
        if (pFPO == null || DetachingObjectsIdx >= GetWpnData<WPN_DATA_DETACHING>().nDetachingNames)
            return BaseWeaponSlot.WPN_DISABLED;
        return BaseWeaponSlot.WPN_READY;
    }
    public override bool Fire(iContact tgt, float ood, float dx, float dy)
    {
        if (GetStatus() == WPN_DISABLED) return false;
        // удаленно управляемый объект не может стрелять сам
        //if (Owner.IsRemote()) return true;
        //// на клиенте - посылаем запрос на выстрел
        //if (rScene.IsClient())
        //{
        //    WeaponSlotDetachingRemoteFirePacket p(GetHandle(), (tgt! = 0 ? tgt->GetHandle() : THANDLE_INVALID));
        //    rScene.SendItemData(&p);
        //}
        //else
        //{ // на хосте - стреляем сами
        DoFire(tgt, 0, 0, 0);
        //}
        return true;
    }


    // от BaseSubobj

    public void BasePrepare()
    {
        //TODO возможно, тут нужны исправления отделяемых боеприпасов
        for (int i = 0; i < GetWpnData<WPN_DATA_DETACHING>().nDetachingNames; i++)
        {
            UnityEngine.Debug.Log("Loading WPN_DATA_DETACHING [" + GetWpnData<WPN_DATA_DETACHING>().DetachingNames[i] + "]");
            DetachingObjects[i] = (FPO)pFPO.GetSubObject(GetWpnData<WPN_DATA_DETACHING>().DetachingNames[i]);
            if (DetachingObjects[i] == null)
                throw new System.Exception(string.Format("WeaponSlot \"%s\": cannot find detaching object!", SubobjData.FullName));
        }
    }
    public BaseWeaponSlotDetaching(BaseScene s, DWORD h, SUBOBJ_DATA d) : base(s, h, d)
    {
        DetachingObjectsIdx = 0;
    }
    public override void HostPrepare(HostScene s, BaseObject o, BaseSubobj so, FPO fpo, SLOT_DATA sld, int slot_id, int lay_id)
    {
        base.HostPrepare(s, o, so, fpo, sld, slot_id, lay_id);
        BasePrepare();
    }
    //virtual void RemotePrepare(RemoteScene*, BaseObject*, BaseSubobj*, FPO*,const SLOT_DATA*,const SubobjCreatePacket*, DWORD);// инициализация
    public override float GetWeight()
    {
        float w = base.GetWeight();
        return (w > 0f ? w - GetWpnData<WPN_DATA_DETACHING>().Mass * DetachingObjectsIdx : w);
    }
};
