using UnityEngine;
using DWORD = System.UInt32;

public class BaseStatic : BaseObject
{
    new public const uint ID = 0x00DDAC72;
    BaseCollidingFPO<BaseStatic> mpColliding;
    public BaseStatic(BaseScene s, uint h, OBJECT_DATA od) : base(s, h, od)
    {
        mpColliding = null;

    }

    ~BaseStatic()
    {
        Dispose();
    }

    public override void Dispose()
    {
        //if (mpColliding != null) mpColliding.Dispose(); //TODO - возможно правильнее освобождать коллайдер, а не оставлять это на сборщик мусора.
        mpColliding = null;
        base.Dispose();
    }

    public override void HostPrepare(HostScene s, UnitSpawnData sd, Vector3 o, float angle, iContact hangar)
    {
        Vector3 org = o;
        // если стоим не в Position, добавляем высоту террейна
        //org.y += rScene.SurfaceLevel(org.x, org.z) + Dt().Delta.y;
        org.y = rScene.SurfaceLevel(org.x, org.z) + Dt().Delta.y;
        base.HostPrepare(s, sd, org, angle, hangar);
        // готовим слоты
        if ((uint)sd.Layout1Name != 0xFFFFFFFF)
        {
            LAYOUT_DATA l = ObjectData.GetLayout(LAYOUT_DATA.SLOTS_LAYOUT, (uint)sd.Layout1Name);
            if (l == null)
                throw new System.Exception(string.Format("Static \"{0}\": cannot find slots layout {1}", ObjectData.FullName, sd.Layout1Name.ToString("X8")));
            BaseObjectEnumSubobj Helper = new BaseObjectEnumSubobj(rScene, this, l);
            pFPO.EnumerateSlots(Helper);
        }

        //Тень реализуется движком
        //CreateShadow(false);
        //UpdateShadow();
        if (Dt().Avoidable)
            pFPO.SetFlag(HashFlags.OF_GROUP_COLLIDABLE2);
    }


    //public override bool Move(float scale)
    //{
    //    return base.Move(scale);
    //}

    //public override bool Update(float scale)
    //{
    //    return base.Update(scale);
    //}

    protected STATIC_DATA Dt()
    {
        return (STATIC_DATA)ObjectData;
    }

    public override Vector3 GetSpeed()
    {
        return Vector3.zero;
    }
    public override bool IsSurfaced()
    {
        return true;
    }
    public override float GetMaxSpeed()
    {
        return .0f;
    }
    public override float GetMaxCornerSpeed()
    {
        return .0f;
    }

    public override int getUnitType()
    {
        return (int)iSensorsDefines.UT_GROUND;
    }

    // от iBaseInterface
    public override object GetInterface(DWORD id)
    {
        switch (id)
        {
            case iBaseColliding.ID:
                {
                    BaseStatic s = (BaseStatic)this;
                    if (s.mpColliding == null) s.mpColliding = new BaseCollidingFPO<BaseStatic>(s);
                }
                return mpColliding;
            case BaseStatic.ID: return this;
            default: return base.GetInterface(id);
        }
    }
}
/// <summary>
/// Класс для совместимости с ОШ. В СН ангар - не отдельное здание, а Subobject
/// </summary>
//public class BaseStaticHangar : BaseStatic
//{
//    private string HANGAR=null;
//    //private const string HANGAR = "Hull";
//    //private const string HANGAR = "Main";
//    BaseSubHangar myBaseHangar;
//    public BaseStaticHangar(BaseScene s, uint h, OBJECT_DATA od) : base(s, h, od)
//    {
//        HANGAR_DATA myHangarData = (HANGAR_DATA) od;

//        //if (myHangarData.HangarSubobj == null) return;
//        //SUB_HANGAR_DATA reconstructed = new SUB_HANGAR_DATA(myHangarData.HangarSubobj.FullName);//TODO! исправить на корректное получение субобъекта ангара
//        HANGAR = (myHangarData.HangarSubobj != null) ? myHangarData.HangarSubobj.FullName : "Root";
//        SUB_HANGAR_DATA reconstructed = new SUB_HANGAR_DATA(HANGAR);//TODO! исправить на корректное получение субобъекта ангара
//        reconstructed.myHangarData = myHangarData.myHangarData;
//        reconstructed.myDoorName = myHangarData.myDoorName;
//        reconstructed.mySoundName = myHangarData.mySoundName;
//        Debug.Log(reconstructed.myHangarData);
//        myBaseHangar = new BaseSubHangar(s, h, reconstructed);
//        //od.RootData.Flags &= SUBOBJ_DATA.SC_HANGAR;
//    }


//    public override void HostPrepare(HostScene s, UnitSpawnData sd, Vector3 o, float angle, iContact hangar)
//    {
//        base.HostPrepare(s, sd, o, angle, hangar);
//        if (this.GetRoot().pFPO.GetSubObject(HANGAR) == null) return;//TODO! исправить на корректное получение субобъекта ангара
//        if (myBaseHangar == null) return;
//        myBaseHangar.HostPrepare(s, this, this.GetRoot(), this.GetRoot().pFPO, null, -1, -1);
//    }

//    public override object GetInterface(uint id)
//    {
//        if (id == BaseHangar.ID) return myBaseHangar.GetInterface(id);
//        return base.GetInterface(id);
//    }
//}
