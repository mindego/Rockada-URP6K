using System;
using UnityEngine;
using static AnimationParams;
using static RoFlags;
using crc32 = System.UInt32;
using DWORD = System.UInt32;


//public partial class BaseSubobj : IBaseItem
//{
//    private DWORD OwnHandle;
//    private bool mIsRemote;
//    public DWORD GetHandle() { return OwnHandle; }
//    public virtual bool IsRemote() { return mIsRemote; }
//    public virtual bool IsLocal() { return (!mIsRemote); }
//    public BaseScene getScene() { return rScene; }

//    public void BaseItemInit(BaseScene s, DWORD h)
//    {
//        if (rScene == null) rScene = s;
//        if (h == Constants.THANDLE_INVALID)
//        {
//            OwnHandle = (uint)this.GetHashCode();//TODO ВРЕМЕННО! Тут в случае FFFFFFFF должен быть уникальный ключ.
//            rScene.ItemsArray.Add(OwnHandle,this); 
//            mIsRemote = false;
//        }
//        else
//        {
//            //rScene.ItemsArray.Set(OwnHandle = h, this);
//            OwnHandle = h;
//            if (rScene.ItemsArray.ContainsKey(OwnHandle)) rScene.ItemsArray.Remove(OwnHandle);
//            rScene.ItemsArray.Add(OwnHandle, this);
//            mIsRemote = true;
//        }
//    }

//    public  float Regenerate(float RegenerateSpeed)
//    {
//            float d = .0f;
//            if (mCurrentState < State2Life.sDamaged)
//            {
//                d = SubobjData.Armor - Life;
//                if (d > RegenerateSpeed) d = RegenerateSpeed;
//                if (rScene.IsHost())
//                {
//                    Life += d;
//                    if (GetState() != mCurrentState) Owner.RequestSubobjRecalc();
//                }
//            }
//            return d;
//    }

//    public int SetRemote(char[] pData)
//    {
//        throw new NotImplementedException();
//    }

//    public bool SetLocal(int DataLength, char[] pData)
//    {
//        throw new NotImplementedException();
//    }

//    public void Release()
//    {
//        throw new NotImplementedException();
//    }
//}

public partial class BaseSubobj : IAnimationServer //Animations
{
    public IAnimationServer getIAnimationServer() { return this; }

    public IAnimationSupport getSupport()
    {
        return rScene.getSV().getIAnimationSupport();
    }

    public virtual void enumerateSlots(string slot_id, IAnimationSlotEnum an, bool recurse)
    {
        AnimationEnumSubobj Helper = new AnimationEnumSubobj(slot_id, an);
        GetFpo().EnumerateSlots(Helper, recurse);
    }

    public virtual float getWeight() { return Owner.getWeight(); }
    public virtual void addWeight(float weight) { Owner.addWeight(weight); }

    public virtual Vector3 getVector(crc32 name)
    {
        if (name == iOSpeedName)
            return Owner.GetSpeed();
        return Vector3.zero;
    }

    public virtual ILog getLog() { return rScene.GetLog(); }
}
public partial class BaseSubobj : IBaseItem
{
    private IBaseItem myBaseItem;

    /// <summary>
    /// Проброс сцены из myBaseItem
    /// </summary>
    public BaseScene rScene
    {
        get => getScene();
    }
    public uint GetHandle()
    {
        return myBaseItem.GetHandle();
    }

    public BaseScene getScene()
    {
        return myBaseItem.getScene();
    }

    public virtual bool IsLocal()
    {
        return myBaseItem.IsLocal();
    }

    public virtual bool IsRemote()
    {
        return myBaseItem.IsRemote();
    }

    public void Release()
    {
        myBaseItem.Release();
    }

    public bool SetLocal(int DataLength, char[] pData)
    {
        return myBaseItem.SetLocal(DataLength, pData);
    }

    public int SetRemote(char[] pData)
    {
        return myBaseItem.SetRemote(pData);
    }

    private void BaseItemInit(BaseScene s, DWORD h)
    {
        myBaseItem = new BaseItem(s, h, this);
    }
}

public partial class BaseSubobj : IDisposable
{
    public override string ToString()
    {
        return String.Format("{0} {1} hash {2}", GetType().ToString(), SubobjData.FullName, GetHashCode().ToString("X8"));
    }

    public virtual void Dispose()
    {
        myBaseItem.Release();
        if (Owner != null)
        {
            if (pFPO != null)
            {
                Owner.OnSubSubobj(this, true);
                if (Parent == null) pFPO.Release();
            }
            Owner.SubobjsList.Sub(this);
        }
    }
}

public partial class BaseSubobj : iBaseVictim
{
    public float GetCondition()
    {
        return Mathf.Clamp(Life / SubobjData.Armor, .0f, 1f);
    }

    public float GetLife()
    {
        return Life;
    }

    public float GetDamage()
    {
        return SubobjData.Armor - Life;
    }

    public float GetTotalLife()
    {
        return SubobjData.Armor;
    }

    public void AddDamage(uint GadHandle, uint WeaponCode, float d)
    {
        if (pFPO == null) return;
        // тряска камеры
        // TODO Вернуть тряску камеры
        //if (rScene.GetSceneVisualizer() != null) rScene.GetSceneVisualizer().AddCameraDamageShake(Owner.GetHandle(), d);
        // если не на хосте
        //if (rScene.IsClient())
        //{
        //    // посылаем уведомление на сервер
        //    SubobjDamagePacket sdp(GetHandle(), GadHandle, WeaponCode, d);
        //    rScene.SendItemData(&sdp);
        //    // уведомляем сцену
        //    rScene.OnAddDamage(Owner->GetHandle(), GadHandle, WeaponCode, d, false);
        //}
        //else
        //{
        // уменьшаем жизнь и уведомляем сцену
        AddRealDamage(GadHandle, WeaponCode, d);
        //}
    }

    void AddRealDamage(DWORD GadHandle, DWORD WeaponCode, float d)
    {
        float d1 = rScene.GetDamage(Owner.GetHandle(), GadHandle, WeaponCode, d);
        Debug.LogFormat("d {0}/{1} weaponcode {2}", d, d1, iBaseVictim.DescribeWeaponCode(WeaponCode));
        d = WeaponCode == iBaseVictim.WeaponCodeUltimateDeath ? GetLife() * 2f : d1;
        if (d == 0) return;

        Debug.LogFormat("Transferring {0}/{1} damage from {2} to owner {3} weaponcode {4}",d, GetLife(), GetFpo().TextName,Owner.GetFpo().Top().TextName, iBaseVictim.DescribeWeaponCode(WeaponCode));
        Life -= d;
        float oldc = Owner.GetCondition();
        float newc = Owner.SetCondition(Life / SubobjData.Armor, isRoot() || (SubobjData.GetFlag(SUBOBJ_DATA.SF_CRITICAL) != 0));
        bool IsFinal = (WeaponCode == iBaseVictim.WeaponCodeUltimateDeath) ? true : (newc <= 0 && oldc > 0);
        int newState = GetState();
        if (newState != mCurrentState)
        {
            if (Parent == null && mCurrentState < State2Life.sDamaged &&
                newState >= State2Life.sDamaged && newState < State2Life.sKilled)
                Owner.onHullDamaged();
            Owner.RequestSubobjRecalc();
        }
        rScene.OnAddDamage(Owner.GetHandle(), GadHandle, WeaponCode, d, IsFinal);
    }
}
public partial class BaseSubobj : IQueryInterface, iBaseInterface, TLIST_ELEM<BaseSubobj>, IUsesFPO
{
    // data
    protected SUBOBJ_DATA SubobjData; // указатель на данные
    public BaseObject Owner;      // хозяин
    protected int SlotID;     // ID слота
    protected int LayoutID;   // ID в рамках layout
    public BaseSubobj Parent;     // подобъект уровнем выше (м.б. null - для Root-a)
    public FPO pFPO;
    protected Vector3 Min, Max;
    protected int mCurrentState;  // State type actually
    // от BaseVictim
    protected float Life;

    // от iBaseInterface
    public const uint ID = 0xA55307D2;

    public Vector3 GetMin() { return Min; }
    public Vector3 GetMax() { return Max; }

    public virtual bool ResetState()
    {
        int NewState = GetState();
        if (NewState != mCurrentState)
        {
            if (pFPO != null)
            {
                // если на хосте
                //if (rScene.IsHost())
                //{ // отправляем пакет
                //    SubobjStatePacket ssp(GetHandle(), (int) NewState);
                //    rScene.SendItemData(&ssp);
                //}
                // выполняем команду
                switch (NewState)
                {
                    case State2Life.sDeleted: Delete(true); Explode(false, true); break;
                    case State2Life.sKilled: Explode(false, true); break;
                    case State2Life.sExploded: Explode(false, false); break;
                    default: ResetImage(); break;
                }
            }
            // ставим новый State
            mCurrentState = NewState;
        }
        // возвращяем. живы ли?
        return (mCurrentState < State2Life.sKilled);
    }
    public virtual void Explode(bool CanStaySelf, bool CanKeepChildren)
    {
        
        if (pFPO == null) return;
        Debug.LogFormat("Exploding {0} CanStaySelf:{1} CanKeepChildren:{2}",this.GetFpo().TextName, CanStaySelf, CanKeepChildren);
        // проставляем свою смерть
        if (Life > 0) Owner.SetCondition(Life = 0, (SubobjData.GetFlag(SUBOBJ_DATA.SF_CRITICAL) != 0));
        // отваливаем детей
        BaseSubobj s = null;
        while ((s = NextChild(s))!=null) s.Explode(CanKeepChildren, CanKeepChildren);
        // уведомляем хозяина
        Owner.OnSubSubobj(this);
        // если можем оторваться - отрываемся
        if (Parent == null || CanStaySelf == false || SubobjData.DetachedDebris != null || (RandomGenerator.Rand() >> 14) == 0)
        {
            ResetImage();
            CreateDebrisInfo Info=new CreateDebrisInfo(rScene, pFPO, SubobjData);
            Info.mCenter = pFPO.Top().Org;
            Info.SetSpeed(Owner.GetSpeed(), Owner.GetSpeed().magnitude);
            Info.DoIt();
        }
        else
        {
            ProcessTree(false, true, 0);
        }
        // обнуляем pFPO
        pFPO = null;
    }
    public BaseSubobj(BaseScene s, DWORD h, SUBOBJ_DATA sd)
    {
        BaseItemInit(s, h);
        //rScene = s;
        SubobjData = sd;
        SlotID = -1;
        LayoutID = -1;
    }

    ~BaseSubobj()
    {
        Dispose();
    }

    public FPO GetFpo() { return pFPO; }
    public virtual object GetInterface(DWORD id)
    {
        switch (id)
        {
            case iBaseVictim.ID: return (iBaseVictim)this;
            case BaseSubobj.ID: return (pFPO != null ? this : null);
            //case BaseItem.ID:      return (BaseItem*) this;
            case iContact.ID: return pFPO != null ? getContact() : null;
            default: return Owner.GetInterface(id);
        }
    }

    public virtual void BasePrepare(BaseObject o, FPO r, SLOT_DATA sld)
    {
        // создаем физический объект
        if (SlotID != -1)
        { // на слоте
          // создаем FPO
            pFPO = rScene.CreateFPO(SubobjData.FileName);
            if (pFPO == null) throw new Exception(string.Format("Object \"{0}\", subobj \"{1}\": cannot load FPO \"{2}\"!", o.GetObjectData().FullName, SubobjData.FullName, SubobjData.FileName));
            // аттачим его к родительскому FPO
            //RO tmpRO = (RO)pFPO;
            //r.AttachObject(ref tmpRO, sld.Org, sld.Dir, sld.Up);
            //r.AttachObject(ref pFPO, sld.Org, sld.Dir, sld.Up);
            r.AttachObject(pFPO, sld.Org, sld.Dir, sld.Up);
            pFPO.Org += pFPO.Up * SubobjData.DeltaY;
        }
        else
        {  // по FPO
           // ищем или создаем FPO
            pFPO = (FPO)(Parent != null ? r.GetSubObject(SubobjData.FileName) : r);
            if (pFPO == null) throw new Exception(string.Format("Object \"{0}\", subobj \"{1}\": cannot get subFPO \"{2}\"!", o.GetObjectData().FullName, SubobjData.FullName, SubobjData.FileName));
        }
        // назначаем линки
        //ProcessTree(true, true, (iBaseInterface*)(BaseItem*)this);
        ProcessTree(true, true, this);
        if (Parent != null) Parent.ProcessTree(true, false);
        // добавляемся к хозяину
        Owner = o;
        Owner.SubobjsList.AddToTail(this);
        Owner.OnAddSubobj(this);
        //Debug.Log("Added " + this.SubobjData.FullName + " to " + o.GetFpo().TextName + " "+  o.GetFpo().GetHashCode().ToString("X8"));
    }

    /// <summary>
    /// FPO
    /// </summary>
    /// <param name="tgt"></param>
    /// <param name="Top"></param>
    /// <param name="o"></param>
    /// <param name="src"></param>
    static void ConvertTo(out Vector3 tgt, RO Top, RO o, Vector3 src)
    {
        tgt = src;
        while (o != Top)
        {
            tgt = o.Right * tgt.x + o.Up * tgt.y + o.Dir * tgt.z + o.Org;
            o = o.Parent;
        }
    }
    /// <summary>
    /// назначить линки у FPO
    /// </summary>
    /// <param name="recalc"></param>
    /// <param name="set_links"></param>
    /// <param name="new_link"></param>
    /// <param name="o"></param>
    void ProcessTree(bool recalc, bool set_links, object new_link = null, RO o = null)
    {
        if (o == null)
        { // если начать с нуля
            if (pFPO == null) return;
            o = pFPO;
        }
        if (recalc && o.GetFlag(RoFlags.ROFID_ALLOBJECTS) == RoFlags.ROFID_FPO)
        { // считаем minmax
            if (o == pFPO)
            {
                Min.Set(((FPO)o).MinX(), ((FPO)o).MinY(), ((FPO)o).MinZ());
                Max.Set(((FPO)o).MaxX(), ((FPO)o).MaxY(), ((FPO)o).MaxZ());
            }
            else
            {
                Vector3 r;
                ConvertTo(out r, pFPO, o, new Vector3(((FPO)o).MinX(), ((FPO)o).MinY(), ((FPO)o).MinZ()));
                if (Min.x > r.x) Min.x = r.x; if (Max.x < r.x) Max.x = r.x;
                if (Min.y > r.y) Min.y = r.y; if (Max.y < r.y) Max.y = r.y;
                if (Min.z > r.z) Min.z = r.z; if (Max.z < r.z) Max.z = r.z;
                ConvertTo(out r, pFPO, o, new Vector3(((FPO)o).MaxX(), ((FPO)o).MaxY(), ((FPO)o).MaxZ()));
                if (Min.x > r.x) Min.x = r.x; if (Max.x < r.x) Max.x = r.x;
                if (Min.y > r.y) Min.y = r.y; if (Max.y < r.y) Max.y = r.y;
                if (Min.z > r.z) Min.z = r.z; if (Max.z < r.z) Max.z = r.z;
            }
        }
        // обрабатываем все подобъекты
        for (RO f = o.SubObjects; f != null; f = f.Next)
        {
            if (f.Link != o.Link) continue;
            ProcessTree(recalc, set_links, new_link, f);
        }
        // если надо, назначаем линк
        if (set_links) o.Link = new_link;
    }

    /// <summary>
    /// инициализация
    /// </summary>
    /// <param name="hs">Hostscene</param>
    /// <param name="o">Owner BaseObject</param>
    /// <param name="s">Parent BaseSubobj</param>
    /// <param name="r"></param>
    /// <param name="sld"></param>
    /// <param name="slot_id"></param>
    /// <param name="layout_id"></param>
    public virtual void HostPrepare(HostScene hs, BaseObject o, BaseSubobj s, FPO r, SLOT_DATA sld, int slot_id, int layout_id)
    { // инициализация
      // инитим данные
        Parent = s;
        Life = SubobjData.Armor;
        SlotID = slot_id;
        LayoutID = layout_id;
        // общая часть инициализации
        BasePrepare(o, r, sld);
        // создаем подобъекты этого объекта

        foreach (SUBOBJ_DATA sd in SubobjData.SubobjDatas)
        {
            hs.CreateSubobj(o, this, sd, pFPO);
        }
    }

    public virtual object queryObject(uint id, int num = 0)
    {
        return null;
    }

    bool isRoot() { return Parent == null; }

    public virtual void AddRadiusDamage(uint GadHandle, uint WeaponCode, Vector3 Org, float Xr, float Xd)
    {
        if (pFPO == null) return;
        // предварительная проверка
        Vector3 d = Org - pFPO.Org;
        if (d.sqrMagnitude > Mathf.Pow(pFPO.HashRadius + Xr,2)) return;
        // определяем, координаты в моей системе
        Vector3 InMyOrg = new Vector3(Vector3.Dot(d,pFPO.Right), Vector3.Dot(d,pFPO.Up), Vector3.Dot(d,pFPO.Dir));
        // определяем, досталось ли лично мне
        d.Set(0, 0, 0);
        if (InMyOrg.x > Max.x) d.x = InMyOrg.x - Max.x;
        if (InMyOrg.x < Min.x) d.x = Min.x - InMyOrg.x;
        if (InMyOrg.y > Max.y) d.y = InMyOrg.y - Max.y;
        if (InMyOrg.y < Min.y) d.y = Min.y - InMyOrg.y;
        if (InMyOrg.z > Max.z) d.z = InMyOrg.z - Max.z;
        if (InMyOrg.z < Min.z) d.z = Min.z - InMyOrg.z;
        float delta = d.magnitude;
        Debug.LogFormat("Damages: InMyOrg{0} max:{1} min:{2} d:{3} delta:{4} Xr{5} Xd:{6}", InMyOrg, Max, Min,d,delta,Xr,Xd);
        if (delta < Xr)
        {
            delta = Mathf.Pow(Xr - delta,2) * Xd;
            if (rScene.GetSceneVisualizer() != null) rScene.GetSceneVisualizer().AddCameraDamageShake(Owner.GetHandle(), delta);
            // если не на хосте
            if (rScene.IsClient())
            { // только посылаем уведомление
                //SubobjDamagePacket sdp(GetHandle(), GadHandle, WeaponCode, delta);
                //rScene.SendItemData(&sdp);

                // уведомляем сцену
                //rScene.OnAddDamage(Owner->GetHandle(), GadHandle, WeaponCode, delta, false);
            }
            else
            {
                // уменьшаем жизнь и уведомляем сцену
                AddRealDamage(GadHandle, WeaponCode, delta);
            }
        }
# if _DEBUG
        rScene.IncIdent();
#endif
        // уведомляем детей
        BaseSubobj s = null;
        while ((s = NextChild(s))!=null) s.AddRadiusDamage(GadHandle, WeaponCode, InMyOrg, Xr, Xd);
# if _DEBUG
        rScene.DecIdent();
#endif
    }

    public int GetState()
    {
        return State2Life.getStateFromLife(Life, SubobjData.Armor, pFPO != null);
    }

    public T GetInterface<T>() where T : iBaseInterface
    {
        throw new NotImplementedException();
    }

    public iContact getContact()
    {
        iContact parent = (iContact)Owner.GetInterface(iContact.ID);
        //Debug.Log("Parent: " + parent.GetFpo().TextName);
        //Debug.Log(GetData().DescribeFlags());
        if (GetData().GetFlag(SUBOBJ_DATA.SF_DETACHED) != 0)
        {
            //Debug.Log("Iterating subobjs");
            iContact ret = null;
            while ((ret = parent.GetNextSubContact(ret)) != parent)
            {
                //Debug.Log(string.Format("Iterating ret: {0} parent: {1}", ret, parent));
                if (ret.GetHandle() == GetHandle())
                    return ret;
            }
            //AssertEx(0);
        }
        return parent;
    }

    public SUBOBJ_DATA GetData() { return SubobjData; }
    public BaseObject GetOwner() { return Owner; }
    public int GetLayoutID() { return LayoutID; }
    public BaseSubobj GetParent() { return Parent; }
    public virtual float GetWeight() { return 0; }

    public virtual void Delete(bool ShouldTryToLeft)
    {
        if (pFPO == null) return;
        // проставляем свою смерть
        if (Life > 0) Owner.SetCondition(Life = 0, (SubobjData.GetFlag(SUBOBJ_DATA.SF_CRITICAL) != 0));
        // отваливаем детей
        BaseSubobj s = null;
        while ((s = NextChild(s)) != null) s.Delete(false);
        // уведомляем хозяина
        Owner.OnSubSubobj(this);
        // пытаемся удалиться
        if (ShouldTryToLeft == false || SubobjData.DetachedDebris == null || pFPO.SetMainImage(2, (int)RoFlags.FSI_ROUND_UP, 0) != 2)
            pFPO.Release();
        // обнуляем pFPO
        pFPO = null;
    }

    //TODO! Похоже, необходима реализация класса как члена LinkedList
    private BaseSubobj next, prev;

    public virtual BaseSubobj Next()
    {
        return next;
    }
    public virtual BaseSubobj Prev()
    {
        return prev;
    }

    public virtual void SetNext(BaseSubobj n)
    {
        next = n;
    }
    public virtual void SetPrev(BaseSubobj p)
    {
        prev = p;
    }


    BaseSubobj NextChild(BaseSubobj s)
    { // следующий подобъект уровнем ниже меня
        s = (s != null ? s.Next() : this.Next());
        // ищем любой подобъект, чей родитель - я
        while (s != null)
        {
            if (s.Parent == this) break; s = s.Next();
        }
        return s;
    }

    public void ResetImage()
    {
        if (pFPO == null || !canChangeImage()) return;
        pFPO.SetImage(Life > SubobjData.Armor * .33f ? 0 : 1, (int)(RoFlags.FSI_FORCE | RoFlags.FSI_EQUAL_LINKS | RoFlags.FSI_ROUND_UP), pFPO.Link);
    }

    public virtual bool canChangeImage() { return true; }


    public virtual float getFloat(crc32 name)
    {
        switch (name)
        {
            case AnimationParams.iArmor: return (mCurrentState >= State2Life.sDamaged) ? 1 : 0;
            case AnimationParams.iCondition: return Condition();
            case AnimationParams.iDarkness: return rScene.getSV().getDarkness();
            default: return 0f;
        }
    }


    public float Regenerate(float RegenerateSpeed)
    {
        float d = .0f;
        if (mCurrentState < State2Life.sDamaged)
        {
            d = SubobjData.Armor - Life;
            if (d > RegenerateSpeed) d = RegenerateSpeed;
            if (rScene.IsHost())
            {
                Life += d;
                if (GetState() != mCurrentState) Owner.RequestSubobjRecalc();
            }
        }
        return d;
    }
    public float Condition()
    {
        return Life / SubobjData.Armor;
    }
}


public static class State2Life
{
    //typedef enum { sNormal = 0, sScratched = 6, sDamaged = 12, sKilled = 18, sExploded = 19, sDeleted = 20 }
    //State;
    public const int sNormal = 0, sScratched = 6, sDamaged = 12, sKilled = 18, sExploded = 19, sDeleted = 20;

    public static int i_RndUp(float x)
    {  // (n.n+1] => n+1
        return (x == (float)((int)(x)) ? (int)(x) : (int)(x + 0.5f));
    }

    public static int getStateFromLife(float life, float total_life, bool have_body)
    {
        if (have_body && life > 0)
            return sKilled - sNormal - i_RndUp(life * (float)(sKilled - sNormal) / total_life);    // [1..sKilled]
        if (life > -total_life * .25f) return sKilled;
        if (life > -total_life * 1E8) return sExploded;
        return sDeleted;
    }

    public static float getLifeFromState(int state, float total_life, out float cond)
    {
        switch (state)
        {
            case sDeleted: cond = -1E9f; return total_life * cond;
            case sExploded: cond = -.26f; return total_life * cond;
            case sKilled: cond = 0f; return 0;
            case sNormal: cond = 1f; return total_life;
            default: cond = ((float)(sKilled - state) - 0.5f) / (sKilled - sNormal); return total_life * cond;
        }
    }
}

public class CreateDebrisInfo : ICreateHidden
{

    const float DebrisSpeedFromCenterC = .035f;
    const float DebrisMinSpeed = 5.0f;
    const float DebrisOwnerSpeedC = 0.2f;
    private BaseScene mrScene;
    private FPO mpFPO;
    private MATRIX mInWorld;
    private Vector3 mSpeed;
    private float mSpeedDisp;
    private readonly SUBOBJ_DATA mpData;
    private void CreateDebris(FPO par, FPO fpo, DEBRIS_DATA d)
    {
        if (par != null)
        {
            for (RO o = par; o != mpFPO; o = o.Parent) fpo.ExpandSelf(o);
            fpo.ExpandSelf(mInWorld);
        }
        else
        {
            fpo.Org = mInWorld.Org;
            fpo.Dir = mInWorld.Dir;
            fpo.Up = mInWorld.Up;
            fpo.Right = mInWorld.Right;
        }
        fpo.Detach();
        Vector3 DebrSpd = new Vector3(mSpeed.x, mSpeed.y, mSpeed.z);
        //  DebrSpd+=(fpo.Org-Center)*DebrisSpeedFromCenterC;
        DebrSpd += (fpo.Org - mInWorld.Org) * DebrisSpeedFromCenterC;
        DebrSpd.x += Distr.Gauss() * mSpeedDisp;
        DebrSpd.y += Distr.Gauss() * mSpeedDisp;
        DebrSpd.z += Distr.Gauss() * mSpeedDisp;
        BaseDebris dbr = mrScene.CreateBaseDebris(fpo, d, 0xFFFFFFFF, fpo.HashRadius * 250.0f);
        if (dbr == null) return;
        dbr.SetSpeed(DebrSpd);
        dbr.SetCornerSpeed(mRotateAxis, mRotateSpeed);
    }
    private void ProcessVisble(FPO par, FPO fpo)
    {
        DEBRIS_DATA d = mpData.SubobjDebris;
        if (fpo.Link != null)
        {
            fpo.Link = null;
            RO r1;
            for (RO r = fpo.SubObjects; r != null; r = r1)
            {
                r1 = r.Next;
                if (r.GetFlag(ROFID_ALLOBJECTS) != ROFID_FPO) continue;
                ProcessVisble(fpo, (FPO)r);
            }
            d = mpData.DetachedDebris;
            if (d != null)
            {
                mrScene.GetFpoLoader().CreateHiddenObjects(fpo, this);
                // если нет image2 - исчезаем
                if (fpo.SetMainImage(2, (int)FSI_ROUND_UP, 0) != 2) { fpo.Release(); return; }
            }
        }
        if (par == null) d = mpData.Debris;
        if (d != null || fpo.Parent == null) CreateDebris(par, fpo, d);
    }
    public bool ProcessHidden(FPO par, FPO fpo)
    {
        fpo.SetMainImage(2, 0, null);
        CreateDebris(par, fpo, mpData.DetachedDebris);
        return true;
    }
    public Vector3 mCenter;
    public Vector3 mRotateAxis;
    float mRotateSpeed;
    public CreateDebrisInfo(BaseScene s, FPO f, SUBOBJ_DATA d)
    {
        mrScene = s;
        mpFPO = f;
        mSpeed = Vector3.zero;
        mSpeedDisp = 0;
        mpData = d;
        mRotateAxis = Vector3.zero;
        mRotateSpeed = 0;


        mInWorld = new MATRIX(mpFPO);
        for (RO o = mpFPO.Parent; o != null; o = o.Parent) mInWorld.ExpandSelf(o);
        mCenter = mInWorld.Org;
    }
    public void DoIt()
    {
        ProcessVisble(null, mpFPO);
    }
    public void SetSpeed(Vector3 Speed, float SpeedF)
    {
        mSpeed = Speed;
        mSpeedDisp = DebrisOwnerSpeedC * SpeedF + DebrisMinSpeed;
    }
};
