using UnityEngine;
using UnityEngine.Assertions;
using DWORD = System.UInt32;
/// <summary>
/// ствольное оружие
/// </summary>
public class BaseWeaponSlotBarrel : BaseWeaponSlot
{
    // от iBaseInterface
    new public const uint ID = 0x751B5737;
    public override object GetInterface(DWORD id)
    {
        return id == ID ? this : base.GetInterface(id);
    }

    // работа с оружием
    public Vector3 InWorld;
    protected Vector3 GunDelta;
    protected FPO pBarrel;
    protected float BarrelPos;
    protected float BarrelRotateSpeed;
    I3DSoundEvent sound;
    //AudioClip sound;
    PARTICLE_SYSTEM pFlash;
    public Vector3 GetAim(ref float dx, ref float dy, float ood)
    {
        WPN_DATA_BARREL wpnData = (WPN_DATA_BARREL)GetWpnData();
        dy = Mathf.Clamp(dy, -wpnData.AutoAimLimit, wpnData.AutoAimLimit) + (RandomGenerator.Rand01() - .5f) * wpnData.DispersionY;
        dx = Mathf.Clamp(dx, -wpnData.AutoAimLimit, wpnData.AutoAimLimit) + (RandomGenerator.Rand01() - .5f) * wpnData.DispersionX;

        //TODO! Возможно стоит инвертировать условия ошибки наведения
        //if (Owner.GetDetailStage() < DETAIL_STAGE_NONE)
        //{
        dy -= GunDelta.x * ood;
        dx -= GunDelta.y * ood;
        //}

        float s1, c1, s2, c2;
        Storm.Math.SinCos(dy, out s1, out c1);
        Storm.Math.SinCos(dx, out s2, out c2);

        MATRIX Pos = Owner.GetPosition();
        return Pos.Right * (s1 * c2) + Pos.Up * s2 + Pos.Dir * (c1 * c2);
    }
    protected void Update(float scale)
    {
        if (pFPO == null) return;
        WPN_DATA_BARREL wpnData = (WPN_DATA_BARREL)GetWpnData();
        MATRIX Pos = new MATRIX(Owner.GetPosition());
        //InWorld = Pos.Org;
        InWorld.Set(Pos.Org.x,Pos.Org.y,Pos.Org.z);
        //if (Owner.GetDetailStage() < DETAIL_STAGE_NONE)
        InWorld += Pos.Right * GunDelta.x + Pos.Up * GunDelta.y + Pos.Dir * GunDelta.z;
        
        // разборки со вспышкой
        if (pFlash != null)
        {
            pFlash.Update(scale, Vector3.zero);
            pFlash.Die();

            //Не рисовать вспышку, если уровень детализации объекта ниже полного (т.е. дальше, чем область детально отрисовки)
            //Другими словами - так как закомментировано - рисуем всегда.
            //if (Owner.GetDetailStage() > DETAIL_STAGE_FULL)
            //{
            //    pFlash.Release(); pFlash = null;
            //}
        }
        // возня со стволом
        if (pBarrel != null)
        {
            //if (Owner.GetDetailStage() <= DETAIL_STAGE_HALF)
            //{
            // откат
            if (pBarrel.Org.z < BarrelPos)
            {
                pBarrel.Org.z += (BarrelPos - pBarrel.Org.z) * wpnData.RecoilMod * scale;
            }
            // вращение
            if (BarrelRotateSpeed > 0)
            {
                pBarrel.BankRightPrec(BarrelRotateSpeed * scale);
                BarrelRotateSpeed -= wpnData.RotationMod * scale;
            }
            //}
            //else
            //{
            //    pBarrel.Org.z = BarrelPos;
            //    BarrelRotateSpeed = 0;
            //}
        }
    }
    protected override void DoFire(iContact tgt, float ood, float dx, float dy)
    {
        if (sound != null)
        {
            sound.UpdateController(InWorld);
            sound.Start();
        }
        
        WPN_DATA_BARREL wpnData = (WPN_DATA_BARREL)GetWpnData();
        //if (Owner->GetDetailStage() <= DETAIL_STAGE_HALF && rScene.GetSceneVisualizer() != 0 && rScene.GetSceneVisualizer()->GetSceneConfig()->v_recoil != 0)
        if (rScene.GetSceneVisualizer() != null && rScene.GetSceneVisualizer().GetSceneConfig().v_recoil != 0)
        {
            //TODO корректно реализовать вспышку.
            //if (pBarrel != null)
            //{
            //    GameObject gl = new GameObject(pBarrel.Top().TextName + " flash");
            //    Light gunlight = gl.AddComponent<Light>();
            //    gunlight.color = Color.red;
            //    gl.transform.position = Engine.ToCameraReference(InWorld);
            //    GameObject.Destroy(gl, 2);
            //}

            // организуем вспышку
            if (pFlash == null)
            {
                if (wpnData.Flash != 0xFFFFFFFF)
                {
                    pFlash = rScene.GetSceneVisualizer().CreateParticle(wpnData.Flash, 0);
                    Assert.IsNotNull(pFlash);
                    //var ROFlash = (RO)pFlash;
                    if (pBarrel != null) pBarrel.AttachObject(pFlash, new Vector3(0, 0, pBarrel.MaxZ()), Vector3.forward, Vector3.up);
                    else pFPO.AttachObject(pFlash, new Vector3(0, 0, Max.z), Vector3.forward, Vector3.up);
                }
            }
            else
            {
                pFlash.Reset();
                pFlash.Activate();
            }
            // дергаем ствол (если надо)
            if (pBarrel != null)
            {
                pBarrel.Org.z = BarrelPos - wpnData.RecoilDelta;
                BarrelRotateSpeed = wpnData.RotationSpeed;
            }
        }
    }
    public override WPN_DATA GetWpnData() { return (WPN_DATA)SubobjData; }

    // от BaseSubobj
    public override void Explode(bool CanStaySelf, bool CanKeepChildren)
    {
        //SafeRelease(sound);
        sound = null;
        //SafeRelease(pFlash);
        pFlash = null;
        base.Explode(CanStaySelf, CanKeepChildren);
    }
    public void BasePrepare()
    {
        WPN_DATA_BARREL WpnData = (WPN_DATA_BARREL)GetWpnData();
        // ищем ствол
        if (WpnData.Barrel != 0)
        {
            //pBarrel = (FPO)pFPO.GetSubObject((int)WpnData.Barrel);
            pBarrel = (FPO)pFPO.GetSubObject(WpnData.Barrel);
            Asserts.AssertEx(pBarrel != null);
            BarrelPos = pBarrel.Org.z;
            BarrelRotateSpeed = 0;
        }
        // считаем дельту места вылета пули 
        GunDelta.Set(0, 0, Max.z);
        GunDelta = pFPO.ToWorldPoint(GunDelta);
        GunDelta = pFPO.Top().ToLocalPoint(GunDelta);
        // грузим звук
        if (rScene.GetSceneVisualizer() != null)
        {
            //GSParam sound_params(false,true,InWorld,Owner.GetSpeed(),0,0);

            Vector3 myVel = Owner.GetSpeed();
            if (InWorld == null) InWorld = Owner.GetPosition().Org;
            I3DSoundEventController ctr = RefSoundCtrWrapper.CreateSoundCtrWrapper(InWorld, myVel, (DWORD)pFPO.Top());
            //I3DSoundEventController ctr = RefSoundCtrWrapper.CreateSoundCtrWrapper(Owner.GetPosition(), myVel, (DWORD)pFPO.Top().GetHashCode());
            //I3DSoundEventController ctr = RefSoundCtrWrapper.CreateSoundCtrWrapper(ref InWorld, ref Owner.GetSpeed(), (DWORD)pFPO.Top().GetHashCode());
            //I3DSoundEventController ctr = WeaponSoundController.CreateWeaponCtrWrapper(this, Owner.GetSpeed(), (DWORD)pFPO.Top().GetHashCode());

            sound = rScene.GetSceneVisualizer().Get3DSound().LoadEvent(
              "Weapon", GetWpnData().FullName, "Fire", false, true, ctr);
            sound.setName(Owner.UnitName + " " + GetWpnData().FullName);
            ctr.Release();
            //TODO реализовать загрузку звуков ствольного оружия
        }
    }
    public BaseWeaponSlotBarrel(BaseScene s, DWORD h, SUBOBJ_DATA d) : base(s, h, d)
    {
        GunDelta = Vector3.zero;
        sound = null;
        pBarrel = null;
        pFlash = null;
    }
    /// <summary>
    /// инициализация
    /// </summary>
    /// <param name="s"></param>
    /// <param name="o"></param>
    /// <param name="so"></param>
    /// <param name="fpo"></param>
    /// <param name="sld"></param>
    /// <param name="slot_id"></param>
    /// <param name="lay_id"></param>
    public override void HostPrepare(HostScene s, BaseObject o, BaseSubobj so, FPO fpo, SLOT_DATA sld, int slot_id, int lay_id)
    {
        base.HostPrepare(s, o, so, fpo, sld, slot_id, lay_id);
        BasePrepare();
    }
    //public override void RemotePrepare(RemoteScene s, BaseObject o, BaseSubobj so, FPO fpo,SLOT_DATA sld,SubobjCreatePacket p, DWORD Offset);// инициализация
    ~BaseWeaponSlotBarrel()
    {
        sound = null;
    }
    public override float GetWeight()
    {
        return .0f;
    }
}
