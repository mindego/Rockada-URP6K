using crc32 = System.UInt32;
using static Hasher;
public class BaseObjectEnumSubobj : ISlotEnum
{
    protected BaseScene rScene;
    protected BaseObject Owner;
    protected LAYOUT_DATA SlotsLayout;
    public bool ProcessSlot(SLOT_DATA sld, int slot_id, FPO r)
    {
        // проверка на вшивость
        Asserts.AssertEx(r.Link != null);
        BaseSubobj so = (BaseSubobj)((iBaseInterface)r.Link).GetInterface(BaseSubobj.ID);
        Asserts.AssertEx(so != null);
        // если нет есть расклада слотов, прекращаем Enumerate
        if (SlotsLayout == null)
        {
            //Debug.Log(string.Format("No slots for {0}",Owner.GetObjectData().FullName));
            return false;
        }
        //Debug.Log(string.Format("Placing subobject {0} on {1}",r.TextName,new string(sld.Name)));
        // перебираем все итем в текущем раскладе
        uint n = Hasher.HshString(new string(sld.Name));
        int i = 0;
        //        for (LAYOUT_ITEM li = SlotsLayout.Items.Head(); li; li = li->Next(), i++)
        //        {
        //            // проверяем имя итема
        //            if (li.Name != n) continue;
        //            // если не HostScene, просто обозначаем, что обработали
        //            if (rScene.IsClient()) return true;
        //            try
        //            {
        //                // пытаемся создать ищем подобъект с таким именем
        //                ((HostScene )rScene).CreateSubobj(Owner, so, SUBOBJ_DATA.GetByName(li.Value), r, sld, slot_id, i);
        //        return true;
        //    }
        //                catch (System.Exception e) {
        //                        rScene.Message("Object \"%s\", layout \"%s\", slot \"%s\": failed to load subobj \"%s\" because of:",Owner.GetObjectData().FullName,SlotsLayout.FullName,sld.Name,li.Value);
        //                        rScene.GetLog().AddException(e);
        //                        throw;
        //                }
        //        }
        foreach (LAYOUT_ITEM li in SlotsLayout.Items)
        {
            // проверяем имя итема
            if (li.Name != n) continue;
            // если не HostScene, просто обозначаем, что обработали
            if (rScene.IsClient()) return true;
            //try
            //{
            // пытаемся создать ищем подобъект с таким именем
            ((HostScene)rScene).CreateSubobj(Owner, so, SUBOBJ_DATA.GetByName(li.Value), r, sld, slot_id, i++);
            //    return true;
            //}
            //catch (System.Exception e)
            //{
            //    rScene.Message("Object \"{0}\", layout \"{1}\", slot \"{2}\": failed to load subobj \"{3}\" because of:", Owner.GetObjectData().FullName, SlotsLayout.FullName, new string(sld.Name), li.Value);
            //    rScene.GetLog().AddException(e);
            //    throw;
            //}
        }
#if _DEBUG
        rScene.Message("Object \"%s\", layout \"%s\": unhandled slot \"%s\"", Owner.GetObjectData().FullName, SlotsLayout.FullName, sld.Name);
#endif
        return true;

    }
    public BaseObjectEnumSubobj(BaseScene s, BaseObject o, LAYOUT_DATA l)
    {
        rScene = s;
        Owner = o;
        SlotsLayout = l;
    }

};

public class AnimationEnumSubobj : ISlotEnum
{
    private IAnimationSlotEnum myObject;
    crc32 myName;
    public virtual bool ProcessSlot(SLOT_DATA sld, int slot_id, FPO r)
    {
        //UnityEngine.Debug.LogFormat("Processing animation slot: {0} {1} {2}" ,sld.Name,myName.ToString("X8"), HashString(sld.Name).ToString("X8"));
        if (myName == CRC32.CRC_NULL || HashString(sld.Name) == myName)
            myObject.processSlot((myName == CRC32.CRC_NULL) ? null : sld, r);
        return true;
    }
    public AnimationEnumSubobj(string name, IAnimationSlotEnum o)
    {
        myName = HashString(name);
        myObject = o;
    }
};

/// <summary>
/// WeaponSystemForBaseObject: переходник между iWeaponSystemTurrets и BaseObject
/// </summary>
//class WeaponSystemForBaseObject : iWeaponSystemTurrets
//{
//    public virtual float GetCondition()
//    {
//        return 1f;
//    }
//    public virtual void SetAimError(float err)
//    {
//        BaseObject o = (BaseObject)this;
//        BaseTurret t;
//        // проставляем всем башням
//        for (t = o.FirstTurret; t!=null; t = t.GetNextTurret())
//            if (t.GetWeaponSystem() != null)
//                t.GetWeaponSystem()->SetAimError(err);

//    }
//    public virtual void SetTargets(int nTargets, iContact[] Targets, float[] TargetWeights)
//    {
//        BaseObject o = (BaseObject) this;
//        BaseTurret t;
//        // если целей нет
//        if (nTargets == 0)
//        { // глушим все башни
//            for (t = o.FirstTurret; t!=null; t = t.GetNextTurret())
//                if (t.GetWeaponSystem() != null)
//                    t.GetWeaponSystem().SetTarget(null);
//            return;
//        }
//        // переводим все цели в локальную систему координат
//        const MATRIX&pos = o.GetPosition();
//        VECTOR* Orgs = (VECTOR*)_alloca(sizeof(VECTOR) * nTargets);
//        for (int i = 0; i < nTargets; i++)
//            Orgs[i] = pos.ExpressPoint(Targets[i].GetOrg());
//        // вызваем всем башням SelectTarget
//        for (t = o.FirstTurret; t!=null; t = t.GetNextTurret())
//            if (t.GetWeaponSystem() != 0)
//                t.GetWeaponSystem()->SetTarget(t.GetWeaponSystem().SelectTargetFromLocal(nTargets, Targets, Orgs, TargetWeights, .0f));

//    }
//    public iWeaponSystemDedicated GetNextTurret(iWeaponSystemDedicated prev)
//    {
//        BaseObject  o = (BaseObject*)this;
//        BaseTurret t = null;
//        if (prev == null)
//        {
//            t = o.FirstTurret;
//        }
//        else
//        {
//            for (t = o.FirstTurret; t != null; t = t->GetNextTurret())
//                if (t.GetWeaponSystem() == prev)
//                    break;
//            if (t == null) t = o.FirstTurret;
//            else t = t.GetNextTurret();
//        }
//        return (t != null ? t.GetWeaponSystem() : null);

//    }
//};



