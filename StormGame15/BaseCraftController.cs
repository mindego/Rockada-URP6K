using DWORD = System.UInt32;
using static UnitDataDefines;
using static HangarStatus;
using static iSensorsDefines;
using static CameraDefines;
using UnityEngine;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
// BaseCraftController - базовый класс для интерфейса между игрой и игроком на крафте
class BaseCraftController : CommLink, IBaseCraftController
{
    // команды, триггеры и переменные
    const string sIDevice = "i_device";
    const DWORD iIDevice = 0x38091575;
    const string sIDeviceSens = "i_device_sens";
    const DWORD iIDeviceSens = 0xF11C776A;
    const string sIDeviceDeadZone = "i_device_dzone";
    const DWORD iIDeviceDeadZone = 0xB45A0B96;
    const string sIDeviceInvY = "i_device_inv_y";
    const DWORD iIDeviceInvY = 0x521ACCED;
    const string sIDeviceXasYaw = "i_device_yaw_x";
    const DWORD iIDeviceXasYaw = 0x1C5C1198;
    const string sIRudder = "i_rudder";
    const DWORD iIRudder = 0xAA274D77;
    const string sIRudderSens = "i_rudder_sens";
    const DWORD iIRudderSens = 0xF4C18035;
    const string sIRudderDeadZone = "i_rudder_dzone";
    const DWORD iIRudderDeadZone = 0x4F8B9A04;
    const string sIEMaxTrun = "i_device_maxturn";
    const DWORD iIEMaxTrun = 0x0A01FFF7;

# if _DEBUG
    const string sIDeviceSensC1 = "i_device_sens1";
    const DWORD iIDeviceSensC1 = 0xFC48940B;
    const string sIDeviceSensC2 = "i_device_sens2";
    const DWORD iIDeviceSensC2 = 0x6541C5B1;
    const string sIDeviceSensC3 = "i_device_sens3";
    const DWORD iIDeviceSensC3 = 0x1246F527;
#endif

    const string sClTurnLeft = "cl_turn_left";
    const DWORD iClTurnLeft = 0x28A19C39;
    const string sClTurnRight = "cl_turn_right";
    const DWORD iClTurnRight = 0x7AF63D80;
    const string sClTurnDown = "cl_turn_down";
    const DWORD iClTurnDown = 0x4E39EB6A;
    const string sClTurnUp = "cl_turn_up";
    const DWORD iClTurnUp = 0x6E7A75F7;
    const string sClBankLeft = "cl_bank_left";
    const DWORD iClBankLeft = 0xFF39FF1A;
    const string sClBankRight = "cl_bank_right";
    const DWORD iClBankRight = 0xD846D491;
    const string sClMoveLeft = "cl_move_left";
    const DWORD iClMoveLeft = 0x5B319120;
    const string sClMoveRight = "cl_move_right";
    const DWORD iClMoveRight = 0x1EEE054D;
    const string sClMoveDown = "cl_move_down";
    const DWORD iClMoveDown = 0x3DA9E673;
    const string sClMoveUp = "cl_move_up";
    const DWORD iClMoveUp = 0x1CF0ACD6;
    const string sClMoveBackward = "cl_move_backward";
    const DWORD iClMoveBackward = 0xDD94D2F1;
    const string sClMoveForward = "cl_move_forward";
    const DWORD iClMoveForward = 0x9D34007C;
    const string sClJoystickThrottle = "cl_joystick_throttle";
    const DWORD iClJoystickThrottle = 0x8543B44E;

    const string sClAttack = "cl_attack";
    const DWORD iClAttack = 0xD8C3F602;
    const string sClWeapon = "cl_weapon";
    const DWORD iClWeapon = 0xF6307CDF;
    const string sClWeaponNext = "cl_weapon_next";
    const DWORD iClWeaponNext = 0xD3E9C33D;
    const string sClWeaponPrev = "cl_weapon_prev";
    const DWORD iClWeaponPrev = 0x6B245B54;

    const string sClAutopilot = "cl_autopilot";
    const DWORD iClAutopilot = 0xCC26B5B9;
    const string sClAutoThrottle = "cl_auto_throttle";
    const DWORD iClAutoThrottle = 0x57178C22;

    const string sClTgtPrev = "cl_target_prev";
    const DWORD iClTgtPrev = 0x1E23A51A;
    const string sClTgtNext = "cl_target_next";
    const DWORD iClTgtNext = 0xA6EE3D73;
    const string sClTargetThreat = "cl_target_threat";
    const DWORD iClTargetThreat = 0x5F70E366;
    const string sClTargetNearest = "cl_target_nearest";
    const DWORD iClTargetNearest = 0x2E106D9A;
    const string sClTargetAtRecticle = "cl_target_at_recticle";
    const DWORD iClTargetAtRecticle = 0x42783D6A;
    const string sClTargetAccept = "cl_target_accept";
    const DWORD iClTargetAccept = 0xC0C752B5;

    const string sClTargetKill = "cl_target_kill";
    const DWORD iClTargetKill = 0xA5E87B26;

    static float DeviceMode1SensC = 0.004f;
    static float DeviceMode2SensC = 0.005f;
    static float DeviceMode3SensC = 0.006f;

    static float LandCraftMaxRoll = Storm.Math.GRD2RD(20f);
    static float LandCraftMaxYaw = Storm.Math.GRD2RD(21f);
    static float LandCraftMaxSpeed = Storm.Math.KPH2MPS(30f);
    static float LandCraftMaxRange = 100f;
    static float LandIntMinRange = 101f;
    static float LandIntMaxRange = 500f;
    static float LandIntMaxGlissadeAngle = Storm.Math.GRD2RD(22f);
    static float LandIntMaxRoll = Storm.Math.GRD2RD(23f);
    static float LandMaxCourseAngle = Storm.Math.GRD2RD(10f);
    static float LandIntMinSpeed = Storm.Math.KPH2MPS(140f);
    static float LandIntMaxSpeed = Storm.Math.KPH2MPS(400f);
    // для iBaseInterface
    public const uint ID = 0x0AB7E70C;

    // от CommLink
    public virtual void OnTrigger(uint code, bool on)
    {
        Player.Validate();
        if (Player.Ptr() == null) return;
        switch (code)
        {
            case iClTurnLeft: if (on) { OldKbdTurn.y = KbdTurn.y; KbdTurn.y = -1; } else { if (KbdTurn.y == -1) { KbdTurn.y = OldKbdTurn.y; OldKbdTurn.y = 0; } else if (OldKbdTurn.y == -1) { OldKbdTurn.y = 0; } } return;
            case iClTurnRight: if (on) { OldKbdTurn.y = KbdTurn.y; KbdTurn.y = 1; } else { if (KbdTurn.y == 1) { KbdTurn.y = OldKbdTurn.y; OldKbdTurn.y = 0; } else if (OldKbdTurn.y == 1) { OldKbdTurn.y = 0; } } return;
            case iClTurnDown: if (on) { OldKbdTurn.x = KbdTurn.x; KbdTurn.x = -1; } else { if (KbdTurn.x == -1) { KbdTurn.x = OldKbdTurn.x; OldKbdTurn.x = 0; } else if (OldKbdTurn.x == -1) { OldKbdTurn.x = 0; } } return;
            case iClTurnUp: if (on) { OldKbdTurn.x = KbdTurn.x; KbdTurn.x = 1; } else { if (KbdTurn.x == 1) { KbdTurn.x = OldKbdTurn.x; OldKbdTurn.x = 0; } else if (OldKbdTurn.x == 1) { OldKbdTurn.x = 0; } } return;
            case iClBankLeft: if (on) { OldKbdTurn.z = KbdTurn.z; KbdTurn.z = -1; } else { if (KbdTurn.z == -1) { KbdTurn.z = OldKbdTurn.z; OldKbdTurn.z = 0; } else if (OldKbdTurn.z == -1) { OldKbdTurn.z = 0; } } return;
            case iClBankRight: if (on) { OldKbdTurn.z = KbdTurn.z; KbdTurn.z = 1; } else { if (KbdTurn.z == 1) { KbdTurn.z = OldKbdTurn.z; OldKbdTurn.z = 0; } else if (OldKbdTurn.z == 1) { OldKbdTurn.z = 0; } } return;
            case iClMoveLeft: if (on) { OldKbdMove.x = KbdMove.x; KbdMove.x = -1; } else { if (KbdMove.x == -1) { KbdMove.x = OldKbdMove.x; OldKbdMove.x = 0; } else if (OldKbdMove.x == -1) { OldKbdMove.x = 0; } } return;
            case iClMoveRight: if (on) { OldKbdMove.x = KbdMove.x; KbdMove.x = 1; } else { if (KbdMove.x == 1) { KbdMove.x = OldKbdMove.x; OldKbdMove.x = 0; } else if (OldKbdMove.x == 1) { OldKbdMove.x = 0; } } return;
            case iClMoveDown: if (on) { OldKbdMove.y = KbdMove.y; KbdMove.y = -1; } else { if (KbdMove.y == -1) { KbdMove.y = OldKbdMove.y; OldKbdMove.y = 0; } else if (OldKbdMove.y == -1) { OldKbdMove.y = 0; } } return;
            case iClMoveUp: if (on) { OldKbdMove.y = KbdMove.y; KbdMove.y = 1; } else { if (KbdMove.y == 1) { KbdMove.y = OldKbdMove.y; OldKbdMove.y = 0; } else if (OldKbdMove.y == 1) { OldKbdMove.y = 0; } } return;
            case iClMoveBackward: if (on) { OldKbdMove.z = KbdMove.z; KbdMove.z = -1; } else { if (KbdMove.z == -1) { KbdMove.z = OldKbdMove.z; OldKbdMove.z = 0; } else if (OldKbdMove.z == -1) { OldKbdMove.z = 0; } } return;
            case iClMoveForward: if (on) { OldKbdMove.z = KbdMove.z; KbdMove.z = 1; } else { if (KbdMove.z == 1) { KbdMove.z = OldKbdMove.z; OldKbdMove.z = 0; } else if (OldKbdMove.z == 1) { OldKbdMove.z = 0; } } return;
            case iClAttack: pWeapons.SetTrigger(on); return;
            case iIEMaxTrun: myEMouseTurn = on; return;
        }
    }


    public virtual void OnCommand(uint code, string arg1, string arg2)
    {
        Player.Validate();
        if (Player.Ptr() == null) return;
        switch (code)
        {
            case iClTgtNext:
                NextTarget();
                return;
            case iClTgtPrev:
                PrevTarget();
                return;
            case iClTargetNearest:
                NearestTarget();
                return;
            case iClTargetAtRecticle:
                TargetAtRecticle();
                return;
            case iClTargetThreat:
                TargetToThreat();
                return;
            case iClAutoThrottle:
                {
                    int i = (int)(AutoThrottle * 100f);
                    if (arg1[0] == '+' || arg1[0] == '-')
                    {
                        //i += atol(arg1);
                        i += int.Parse(arg1);
                    }
                    else
                    {
                        i = int.Parse(arg1);
                    }
                    i = Mathf.Clamp(i, -100, 100);
                    AutoThrottle = .01f * i;
                }
                return;
            case iClJoystickThrottle:
                myUseThrottleOnJoy = (int.Parse(arg1) != 0);
                return;
            case iClWeaponNext:
                Debug.Log("Current weapon group selected: " + pWeapons.GetWeapon());
                SwitchWeapon(+1);
                Debug.Log("New weapon group selected: " + pWeapons.GetWeapon());
                return;
            case iClWeaponPrev:
                SwitchWeapon(-1);
                return;
            case iClTargetAccept:
                TargetAccept();
                return;
            //# if RELEASE
            //            case iClTargetKill:
            //                if (pWeapons.GetTarget() != null)
            //                {
            //                    iBaseVictim* vict = pWeapons->GetTarget()->GetInterface<iBaseVictim>();
            //                    if (vict) vict->AddDamage(THANDLE_INVALID, WeaponCodeUltimateDeath, 0);
            //                }
            //                return;
            //#endif
            default:
                return;
        }
    }

    public virtual object OnVariable(uint code, object data)
    {
        Player.Validate();
        if (Player.Ptr() == null) return null;
        int r_int = 0;
        switch (code)
        {
            // device
            case iIDevice:
                if (data != null) SetDevice((int)data);
                return myDevice;
            case iIDeviceSens:
                if (data != null) SetDeviceSens((float)data);
                return myDeviceSens;
            case iIDeviceDeadZone:
                if (data != null) SetDeviceZero((float)data);
                return myDeviceDeadZone;
            case iIDeviceInvY:
                if (data != null) SetDeviceInvY((int)data);
                return myDeviceInvY;
            case iIDeviceXasYaw:
                if (data != null) SetDeviceXasYaw((int)data);
                return myDeviceXasYaw;
            case iIRudder:
                if (data != null) SetRudder((int)data);
                return myRudder;
            case iIRudderSens:
                if (data != null) SetRudderSens((float)data);
                return myRudderSens;
            case iIRudderDeadZone:
                if (data != null) SetRudderZero((float)data);
                return myRudderDeadZone;
            // всякая погребень
            case iClAutopilot:
                if (data != null) SetAutopilotMode((int)data != 0);
                return AutopilotOn;
# if _DEBUG
            case iIDeviceSensC1:
                if (data != 0) DeviceMode1SensC = *(float*)data;
                return &DeviceMode1SensC;
            case iIDeviceSensC2:
                if (data != 0) DeviceMode2SensC = *(float*)data;
                return &DeviceMode2SensC;
            case iIDeviceSensC3:
                if (data != 0) DeviceMode3SensC = *(float*)data;
                return &DeviceMode3SensC;
            case 0xB031C26E:
                if (data != 0) LandCraftMaxRoll = GRD2RD(*(float*)data);
                return 0;
            case 0xF9E94795:
                if (data != 0) LandCraftMaxYaw = GRD2RD(*(float*)data);
                return 0;
            case 0x0B6C3663:
                if (data != 0) LandCraftMaxSpeed = KPH2MPS(*(float*)data);
                return 0;
            case 0x97CD92DC:
                if (data != 0) LandCraftMaxRange = *(float*)data;
                return 0;
            case 0xACF97247:
                if (data != 0) LandIntMinRange = *(float*)data;
                return 0;
            case 0x4A99DDA6:
                if (data != 0) LandIntMaxRange = *(float*)data;
                return 0;
            case 0x5DDC5B0A:
                if (data != 0) LandIntMaxGlissadeAngle = GRD2RD(*(float*)data);
                return 0;
            case 0xE9027101:
                if (data != 0) LandIntMaxRoll = GRD2RD(*(float*)data);
                return 0;
            case 0xA5EE7469:
                if (data != 0) LandMaxCourseAngle = GRD2RD(*(float*)data);
                return 0;
            case 0x3058D6F8:
                if (data != 0) LandIntMinSpeed = KPH2MPS(*(float*)data);
                return 0;
            case 0xD6387919:
                if (data != 0) LandIntMaxSpeed = KPH2MPS(*(float*)data);
                return 0;
#endif
            case iClWeapon:
                if (data != null)
                {
                    int w = (int)data - 1;
                    if (w < 0 || w > 2)
                        rScene.Message("cl_weapon codes: 1-Primary, 2-Secondary, 3-Rocket");
                    else
                    {
                        rScene.Message("Switching to weapon group: " + w);
                        SetWeapon(w);
                    }
                }
                else
                    r_int = pWeapons.GetWeapon() + 1;
                return r_int;
            default:
                return 0;
        }
    }

    // переключение целей
    private readonly TContact SuggestedTarget = new TContact();
    void CheckRecticleContact(ref iContact cand, ref float candd, iContact curr)
    {
        Vector3 delta = curr.GetOrg() - Player.Ptr().GetOrg();
        float d = delta.magnitude;
        if (d > rScene.GetSceneVisualizer().GetCameraDataCockpit().GetRadarRange()) return;
        if (d != .0f) d = Vector3.Dot(delta, Player.Ptr().GetDir()) / d;
        if (cand == null || candd < d) { cand = curr; candd = d; }
    }

    void PrevTarget()
    {
        iContact pContact = pWeapons.GetTarget();
        if (pContact != null && pContact.GetTopContact() != pContact)
        {
            pWeapons.SetTarget(pContact.GetTopContact());
            return;
        }
        iContact cand = null;
        iContact curr = null;
        iSensors pSensors = (iSensors)Player.Ptr().GetInterface(iSensors.ID);
        while ((curr = pSensors.GetEnemyInZone(Player.Ptr().GetOrg(), rScene.GetSceneVisualizer().GetCameraDataCockpit().GetRadarRange(), curr)) != null)
        {
            if (curr == pContact)
            {
                if (cand != null) break;
            }
            else
            {
                cand = curr;
            }
        }
        pWeapons.SetTarget(cand);
    }
    void NextTarget()
    {
        iContact pContact = pWeapons.GetTarget();
        if (pContact != null && pContact.GetTopContact() != pContact)
        {
            pWeapons.SetTarget(pContact.GetTopContact());
            return;
        }
        iContact curr = null;
        iSensors pSensors = (iSensors)Player.Ptr().GetInterface(iSensors.ID);
        while ((curr = pSensors.GetEnemyInZone(Player.Ptr().GetOrg(), rScene.GetSceneVisualizer().GetCameraDataCockpit().GetRadarRange(), curr)) != null)
        {
            if (curr == pContact) { pContact = null; continue; }
            if (pContact != null) continue;
            break;
        }
        pWeapons.SetTarget(curr);
    }
    void NearestTarget()
    {
        float candd = float.MaxValue;
        iContact cand = null;
        iContact curr = null;
        iSensors pSensors = (iSensors)Player.Ptr().GetInterface(iSensors.ID);
        while ((curr = pSensors.GetEnemyInZone(Player.Ptr().GetOrg(), rScene.GetSceneVisualizer().GetCameraDataCockpit().GetRadarRange(), curr)) != null)
        {
            Vector3 delta = curr.GetOrg() - Player.Ptr().GetOrg();
            float d = delta.magnitude;
            if (cand == null || candd > d) { cand = curr; candd = d; }
        }
        pWeapons.SetTarget(cand);
    }
    void TargetAtRecticle()
    {
        float candd = float.MaxValue;
        iContact cand = null;
        iContact curr = null;
        iSensors pSensors = (iSensors)Player.Ptr().GetInterface(iSensors.ID);
        while ((curr = pSensors.GetEnemyInZone(Player.Ptr().GetOrg(), rScene.GetSceneVisualizer().GetCameraDataCockpit().GetRadarRange(), curr)) != null)
        {
            CheckRecticleContact(ref cand, ref candd, curr);
            if (curr.HasSubContacts() == true)
            {
                iContact sub = curr;
                while ((sub = curr.GetNextSubContact(sub)) != curr) CheckRecticleContact(ref cand, ref candd, sub);
            }
        }
        while ((curr = pSensors.GetFriend(curr)) != null)
        {
            if (curr.GetHandle() == Player.Ptr().GetHandle()) continue;
            CheckRecticleContact(ref cand, ref candd, curr);
        }
        Debug.Log(("Locked on recticle:", cand, candd, pWeapons));
        pWeapons.SetTarget(cand);
    }
    void TargetToThreat()
    {
        pWeapons.SetTarget(Player.Ptr().GetThreat());
    }
    void TargetAccept()
    {
        pWeapons.SetTarget(SuggestedTarget.Ptr());
    }

    public void SelectSuggestedTarget(int nTargets, iContact[] Targets, float[] TargetWeights)
    {
        Player.Validate();
        if (Player.Ptr() == null) return;
        iContact pNewTgt;
        pNewTgt = pWeapons.SelectTarget(nTargets, Targets, TargetWeights);
        if (pNewTgt != SuggestedTarget.Ptr())
        {
            if (mpSuggestedTargetSound != null) mpSuggestedTargetSound.Play(false, rScene.GetSceneVisualizer().GetSceneConfig().s_vhudspeach);
            //SuggestedTarget = new TContact(pNewTgt);
            SuggestedTarget.setPtr(pNewTgt);
        }
        if (Player.Ptr() != null) pTurrets.SetTargets(nTargets, Targets, TargetWeights);
    }
    protected void SetSuggestedTarget(iContact pNewTgt)
    {
        if (pNewTgt != SuggestedTarget.Ptr())
        {
            if (mpSuggestedTargetSound != null) mpSuggestedTargetSound.Play(false, rScene.GetSceneVisualizer().GetSceneConfig().s_vhudspeach);
            //SuggestedTarget = new TContact(pNewTgt);
            SuggestedTarget.setPtr(pNewTgt);
        }
    }
    protected void UpdateTargets(float scale)
    {
        // проверяем рекомендуемую цель
        SuggestedTarget.Validate();
        if (SuggestedTarget.Ptr() != null && (SuggestedTarget.Ptr().GetAge() > 0 || SuggestedTarget.Ptr().IsOnlyVisual()))
        {
            //SuggestedTarget = null;
            SuggestedTarget.setPtr(null);
        }
        // проверяем свою цель
        iContact pTarget = pWeapons.GetTarget();
        if (pTarget != null && (pTarget.GetAge() > 0 || pTarget.IsOnlyVisual()))
        {
            pWeapons.SetTarget(null);
            pTarget = null;
        }
    }
    public iContact GetSuggestedTarget()
    {
        return SuggestedTarget.Ptr();
    }

    // переключение оружия
    private void SwitchWeapon(int sign)
    {
        int i, j;
        for (i = 0, j = pWeapons.GetWeapon() + sign; i < 3; i++, j += sign)
        {
            if (j < 0) j += 3; else if (j > 2) j -= 3;
            pWeapons.SetWeapon(j);
            Debug.Log(string.Format("Weapon WS {2} {0} cond {1}", pWeapons.GetWeapon(), pWeapons.GetCondition(), pWeapons));
            if (pWeapons.GetCondition() > .0f) return;
        }
    }
    private void SetWeapon(int wpn)
    {
        int curr = pWeapons.GetWeapon();
        pWeapons.SetWeapon(wpn);
        Debug.Log(string.Format("Weapon {0} cond {1}", pWeapons.GetWeapon(), pWeapons.GetCondition()));
        if (pWeapons.GetCondition() == .0f)
            pWeapons.SetWeapon(curr);
    }

    // автопилот
    float myRequestedSpeed;
    bool myFinalAutopilot = false;
    bool AutopilotOn;
    Vector3 AutopilotOrg;
    bool PausedOn;
    float AutopilotTime;
    readonly TContact AutopilotLeader = new TContact(); //Todo удалить это 
    readonly TContact AutopilotHangar = new TContact();//Todo и это, если автопилот игрока работает неправильно.

    bool canTurnAutopilotOn(float scale)
    {
        Asserts.Assert(AutopilotHangar.Ptr() != null);
        BaseHangar h = (BaseHangar)AutopilotHangar.Ptr().GetInterface(BaseHangar.ID);
        if (h != null)
        {
            Vector3 touch_pos = h.GetPointFor(Player.Ptr(), true);
            Vector3 diff = touch_pos - Player.Ptr().GetOrg();
            float norma2 = diff.sqrMagnitude;

# if _DEBUG
            if (scale)
            {
                switch (Player->GetProtoType())
                {
                    case UT_FLYING_FAST:
                        {
                            rScene.Message("Roll angle=%f", RD2GRD(Player->GetRollAngle()));
                            rScene.Message("Speed angle=%f", RD2GRD(Acos(getCos(diff, Player->GetSpeed()))));

                            const HangarSlotData* data = h->getDataFor(Player);
                            VECTOR gliss = h->GetPointForStart() - touch_pos;
                            gliss.Normalize();
                            calcGlissadeVector(gliss, gliss, data->myInfo->myLandAngle);
                            //SetVector(gliss,atan2(gliss.x,gliss.z),Asin(gliss.y)-data->myInfo->myLandAngle);
                            rScene.Message("Glissade angle=%f", RD2GRD(Acos(getCos(gliss, diff))));
                            rScene.Message("myLandAngle=%f", RD2GRD(data->myInfo->myLandAngle));
                            break;
                        }
                    case UT_FLYING:
                    case UT_FLYING_AGILE:
                        {
                            rScene.Message("Diff angle=%f", RD2GRD(Acos(getCos(h->GetPointForStart() - touch_pos, Player->GetDir()))));
                            rScene.Message("Roll angle=%f", RD2GRD(Player->GetRollAngle()));
                        }
                }
            }
#endif //_DEBUG

            if (norma2 < Mathf.Pow(300f, 2))
            {
                switch ((uint)Player.Ptr().GetProtoType())
                {
                    case UT_FLYING_FAST:
                        return matchParamsForInt(Player.Ptr(), h, norma2, touch_pos, diff);
                    case UT_FLYING:
                    case UT_FLYING_AGILE:
                        return matchParamsForBF(Player.Ptr(), h, norma2, touch_pos);
                    default:
                        Asserts.Assert(false);
                        break;
                }
            }
        }
        return false;
    }

    float getCos(Vector3 vec1, Vector3 vec2)
    {
        return Vector3.Dot(vec1, vec2) / (vec1.magnitude * vec2.magnitude);
    }
    bool matchParamsForInt(iContact cnt, BaseHangar h, float norma2, Vector3 tp, Vector3 diff)
    {
        if (Storm.Math.isInRange(norma2, Mathf.Pow(LandIntMinRange, 2), Mathf.Pow(LandIntMaxRange, 2)))
            if (Storm.Math.isInRange(cnt.GetSpeed().sqrMagnitude, Mathf.Pow(LandIntMinSpeed, 2), Mathf.Pow(LandIntMaxSpeed, 2)))
                if (Storm.Math.isInRange(cnt.GetRollAngle(), -LandIntMaxRoll, LandIntMaxRoll))
                    if (getCos(diff, cnt.GetSpeed()) > Mathf.Cos(LandMaxCourseAngle))
                    {
                        HangarSlotData data = h.getDataFor(cnt);
                        Vector3 gliss = h.GetPointForStart() - tp;
                        gliss.Normalize();
                        BaseCraftAutopilotLand.calcGlissadeVector(out gliss, gliss, data.myInfo.myLandAngle);
                        //SetVector(gliss,atan2(gliss.x,gliss.z),Asin(gliss.y)-data->myInfo->myLandAngle);
                        return getCos(gliss, diff) > Mathf.Cos(LandIntMaxGlissadeAngle);
                    }
        return false;
    }

    bool matchParamsForBF(iContact cnt, BaseHangar h, float norma2, Vector3 tp)
    {
        if (norma2 < Mathf.Pow(LandCraftMaxRange, 2))
            if (cnt.GetSpeed().sqrMagnitude < Mathf.Pow(LandCraftMaxSpeed, 2))
                if (Storm.Math.isInRange(cnt.GetRollAngle(), -LandCraftMaxRoll, LandCraftMaxRoll))
                    return getCos(h.GetPointForStart() - tp, cnt.GetDir()) > Mathf.Cos(LandCraftMaxYaw);
        return false;
    }
    public bool isShowedWaypoint()
    {
        return AutopilotHangar.Ptr() != null;
    }
    public void SetAutopilotMode(bool on)
    {
        if (on == AutopilotOn) return;
        AutopilotOn = on;
        if (Player.Ptr() != null && on)
        {
            BaseCraftAutopilotTurnTo turnto = (BaseCraftAutopilotTurnTo)pControls.GetAutopilot(BaseCraftAutopilotTurnTo.ID);
            if (turnto != null)
                turnto.Set(Player.Ptr().GetDir(), 0);
        }
    }
    public void SetAutopilotData(iContact leader, Vector3 org, float time)
    {
        //Debug.Log("SetAutopilotData " + leader !=null? leader:"NO LEADER");
        if (AutopilotHangar.Ptr() == null)
        {
            AutopilotLeader.setPtr(leader);
            AutopilotOrg = org;
            if (leader == null && Storm.Math.NormaFAbs(AutopilotOrg) > .0f)
                AutopilotOrg.y += rScene.SurfaceLevel(AutopilotOrg.x, AutopilotOrg.z);
            AutopilotTime = time;
            if (mpNewWaypointSound != null) mpNewWaypointSound.Play(false, rScene.GetSceneVisualizer().GetSceneConfig().s_vhudspeach);
        }
    }

    BaseHangar setHangarStatus(iContact hg, HangarStatus st)
    {
        BaseHangar hng = hg != null ? (BaseHangar)hg.GetInterface(BaseHangar.ID) : null;
        if (hng != null)
            hng.setStatus(st);
        return hng;
    }
    public void MakeLanding(iContact hg)
    {
        BaseHangar hangar;
        if (hg != AutopilotHangar.Ptr()) //TODO Странное сравнение - дожно ошибку выдавать
            setHangarStatus(AutopilotHangar.Ptr(), hsClosed);
        if (hg != null && Player.Ptr() != null)
        {
            if ((hangar = setHangarStatus(hg, (Player.Ptr().GetProtoType() == UT_FLYING_FAST) ? hsIntLanding : hsBFLanding)) != null)
            {
                AutopilotOrg = hangar.GetPointFor(Player.Ptr(), true);
                AutopilotOrg.y += 3f;
            }
        }
        myFinalAutopilot = false;
        AutopilotHangar.setPtr(hg);
    }
    public void SetPause(bool p)
    {
        PausedOn = p;
    }
    public Vector3 GetAutopilotOrg() { return AutopilotOrg; }
    public iContact GetAutopiloLeader() { return AutopilotLeader.Ptr(); }
    public bool IsAutopilotOn() { return AutopilotOn; }
    public bool IsPaused() { return PausedOn; }
    public void setSpeed(float spd) { myRequestedSpeed = spd; }

    // sounds
    protected IWave mpMissileWarningSound;
    protected IWave mpMissileStartSound;
    protected IWave mpNewWaypointSound;
    protected IWave mpSuggestedTargetSound;
    protected float mOldCondition;
    protected float mOldEnergy;
    protected float[] mOldWeaponCond = new float[3];
    private void UpdateSounds(float scale)
    {
        if (mpMissileStartSound != null)
        {
            int mc = Player.Ptr().GetMissileCount();
            if (mc > myPrevMissileCount && !mpMissileStartSound.IsPlaying())
                mpMissileStartSound.Play(false, rScene.GetSceneVisualizer().GetSceneConfig().s_vhudspeach);
            myPrevMissileCount = mc;
        }

        // предупреждение о ракете
        if (mpMissileWarningSound != null)
        {
            iContact thr = Player.Ptr().GetThreat();
            if (thr != null && (uint)thr.GetProtoType() == UT_MISSILE && Player.Ptr().GetThreatF() > 2) //TODO - проверить! Скорее всего, GetProtiType должен возвразать DWORD, а не int
                mpMissileWarningSound.Play(true, rScene.GetSceneVisualizer().GetSceneConfig().s_vhudspeach);
            else
                mpMissileWarningSound.Stop();
        }
        // предупреждения о повреждениях
        if (rScene.GetRadioEnvironment().IsRadioFree() == true)
        {
            float c = Player.Ptr().GetCondition();
            if (c < .66)
            {
                if (c < .33)
                {
                    if (mOldCondition >= .33) rScene.GetRadioEnvironment().PlayWave(0, "CriticalDamages", rScene.GetSceneVisualizer().GetSceneConfig().s_vhudspeach);
                }
                else
                {
                    if (mOldCondition >= .66) rScene.GetRadioEnvironment().PlayWave(0, "HighDamages", rScene.GetSceneVisualizer().GetSceneConfig().s_vhudspeach);
                }
            }
            mOldCondition = c;
        }
        // предупреждения о недостатке энергии
        if (rScene.GetRadioEnvironment().IsRadioFree() == true)
        {
            float c = pControls.GetBatteryLoad();
            if (c < .25 && mOldEnergy >= .25) rScene.GetRadioEnvironment().PlayWave(0, "LowEnergy", rScene.GetSceneVisualizer().GetSceneConfig().s_vhudspeach);
            mOldEnergy = c;
        }
        // предупреждение об окончании боезапаса
        if (rScene.GetRadioEnvironment().IsRadioFree() == true)
        {
            int OldWpn = pWeapons.GetWeapon();
            bool NeedToSay = false;
            for (int i = 0; i < 3; i++)
            {
                pWeapons.SetWeapon(i);
                float c = pWeapons.GetCondition();
                if (c > 0 && c < .1 && mOldWeaponCond[i] >= .1) NeedToSay = true;
                mOldWeaponCond[i] = c;
            }
            if (NeedToSay == true) rScene.GetRadioEnvironment().PlayWave(0, "LowAmmo", rScene.GetSceneVisualizer().GetSceneConfig().s_vhudspeach);
            pWeapons.SetWeapon(OldWpn);
        }
    }
    int myPrevMissileCount;

    // controls
    private EInput myDI;


    // клавиатура
    Vector3 KbdTurn;
    Vector3 OldKbdTurn;
    Vector3 KbdMove;
    Vector3 OldKbdMove;
    float AutoThrottle;
    // devices
    int myDevice;
    float myDeviceSens;
    float myDeviceDeadZone;
    int myDeviceInvY;
    int myDeviceXasYaw;
    int myRudder;
    float myRudderSens;
    float myRudderDeadZone;
    float myMouseX, myMouseY, myMouseTimer;
    bool myUseThrottleOnJoy;
    bool myEMouseTurn = false;

    /// <summary>
    /// Альтернативный режим E-Mouse
    /// Мышь двигает камеру, автопилот поворачивает в сторону направления взгляда.
    /// </summary>
    private void MouseLookTurnTo()
    {
        if (AutopilotOn == false && Player.Ptr().GetState() == CS_IN_GAME && pControls.SetAutopilot(BaseCraftAutopilotTurnTo.ID) != null)
        { // TurnTo
            float dy = MouseAlaQuakeValue(myDI.getMouseDX(), myDeviceSens * DeviceMode1SensC, 1);
            float dx = MouseAlaQuakeValue(myDI.getMouseDY(), myDeviceSens * DeviceMode1SensC, 1);

            if (myDeviceInvY != 0)
                dx = -dx;
            //MATRIX tmp = new MATRIX(Player.Ptr().GetOrg(), Player.Ptr().GetDir(), Player.Ptr().GetUp(), Player.Ptr().GetRight());
            //MATRIX tmp = new MATRIX(rScene.GetSceneVisualizer().GetCameraData().myCamera);
            MATRIX tmp = new MATRIX(Engine.EngineCamera);
            EngineDebug.DebugConsole("Before: " + tmp.Dir);
            Vector3 tmpDir = tmp.Dir;
            tmp.TurnRightPrec(dy);
            tmp.TurnUpPrec(dx);
            EngineDebug.DebugConsole("After: " + (tmp.Dir - tmpDir));
            //rScene.GetSceneVisualizer().GetCameraData().myCamera.Set(tmp);
            Engine.EngineCamera.Set(tmp);
            return;
            //MATRIX tmp = new MATRIX(rScene.GetSceneVisualizer().GetCameraData().myCamera);
            Vector3 dir = ((BaseCraftAutopilotTurnTo)pControls.GetAutopilot(BaseCraftAutopilotTurnTo.ID)).GetDir();
            tmp.TurnRightPrec(Mathf.Atan2(Vector3.Dot(dir, Player.Ptr().GetRight()), Vector3.Dot(dir, Player.Ptr().GetDir())));
            tmp.TurnUpPrec(Mathf.Asin(Vector3.Dot(dir, Player.Ptr().GetUp())));

            float rot = myEMouseTurn ?
                -Storm.Math.fov(rScene.GetSceneVisualizer().GetCameraData().CameraAspect * 1.333f) * .5f * myDeviceDeadZone : Storm.Math.Pi();
            ((BaseCraftAutopilotTurnTo)pControls.GetAutopilot(BaseCraftAutopilotTurnTo.ID)).Set(tmp.Dir, rot);
        }
    }
    /// <summary>
    /// Оригинальный режим E-Mouse
    /// </summary>
    private void MouseTurnTo()
    {
        //EngineDebug.DebugConsole("MouseTurnTo()");
        if (AutopilotOn == false && Player.Ptr().GetState() == CS_IN_GAME && pControls.SetAutopilot(BaseCraftAutopilotTurnTo.ID) != null)
        { // TurnTo
            float dy = MouseAlaQuakeValue(myDI.getMouseDX(), myDeviceSens * DeviceMode1SensC, 1);
            float dx = MouseAlaQuakeValue(myDI.getMouseDY(), myDeviceSens * DeviceMode1SensC, 1);

            //EngineDebug.DebugConsoleFormat("MouseTurnTo vals: {0}:{1} sens {2}/{3}", dx, dy, myDeviceSens, DeviceMode1SensC);
            if (myDeviceInvY != 0)
                dx = -dx;
            MATRIX tmp = new MATRIX(Player.Ptr().GetOrg(), Player.Ptr().GetDir(), Player.Ptr().GetUp(), Player.Ptr().GetRight());
            Vector3 dir = ((BaseCraftAutopilotTurnTo)pControls.GetAutopilot(BaseCraftAutopilotTurnTo.ID)).GetDir();
            tmp.TurnRightPrec(Mathf.Atan2(Vector3.Dot(dir, Player.Ptr().GetRight()), Vector3.Dot(dir, Player.Ptr().GetDir())) + dy);
            tmp.TurnUpPrec(Mathf.Asin(Vector3.Dot(dir, Player.Ptr().GetUp())) + dx);

            float rot = myEMouseTurn ?
                -Storm.Math.fov(rScene.GetSceneVisualizer().GetCameraData().CameraAspect * 1.333f) * .5f * myDeviceDeadZone : Storm.Math.Pi();
            ((BaseCraftAutopilotTurnTo)pControls.GetAutopilot(BaseCraftAutopilotTurnTo.ID)).Set(tmp.Dir, rot);
        }
    }

    /// <summary>
    /// Альтернативный режим управления мышью по стандарному курсору Unity
        /// </summary>
    private void MouseCursorTurnTo()
    {
        if (AutopilotOn == false && Player.Ptr().GetState() == CS_IN_GAME && pControls.SetAutopilot(BaseCraftAutopilotTurnTo.ID) != null)
        {

            if (Cursor.visible==false) Cursor.visible = true;
            Vector3 mousePos = Input.mousePositionDelta;
            mousePos.x /= Screen.width;
            mousePos.y /=Screen.height;

            //mousePos.x -= 0.5f;
            //mousePos.y -= 0.5f;


            int i = 1;
            if (myDeviceInvY != 0)
                i = -i;

            float dy = MouseAlaQuakeValue(myDI.getMouseDX(), myDeviceSens * DeviceMode1SensC, 1);
            float dx = MouseAlaQuakeValue(myDI.getMouseDY(), myDeviceSens * DeviceMode1SensC, 1);

            Debug.LogFormat("Wtorm: {0}/{1} vs Unity {2}/{3}",dx,dy,mousePos.x,mousePos.y);

            MATRIX tmp = new MATRIX(Player.Ptr().GetOrg(), Player.Ptr().GetDir(), Player.Ptr().GetUp(), Player.Ptr().GetRight());
            Vector3 dir = ((BaseCraftAutopilotTurnTo)pControls.GetAutopilot(BaseCraftAutopilotTurnTo.ID)).GetDir();
            tmp.TurnRightPrec(Mathf.Atan2(Vector3.Dot(dir, Player.Ptr().GetRight()), Vector3.Dot(dir, Player.Ptr().GetDir())) + mousePos.x*i);
            tmp.TurnUpPrec(Mathf.Asin(Vector3.Dot(dir, Player.Ptr().GetUp())) + mousePos.y);

            float rot = myEMouseTurn ?
    -Storm.Math.fov(rScene.GetSceneVisualizer().GetCameraData().CameraAspect * 1.333f) * .5f * myDeviceDeadZone : Storm.Math.Pi();
            ((BaseCraftAutopilotTurnTo)pControls.GetAutopilot(BaseCraftAutopilotTurnTo.ID)).Set(tmp.Dir, rot);
        }
    }

    private static float MouseAlaQuakeValue(float val, float sens, float time) //TODO Возможно, это правильнее перенести в Input
    {
        return Mathf.Clamp(val * sens / time, -1f, 1f);
    }

    private static float MouseAlaJoystickValue1(float val, float shift, float sens, float zero)
    {
        val += shift * sens;
        return Mathf.Clamp(val, -1f - zero, 1f + zero);
    }

    private static float MouseAlaJoystickValue2(float val, float zero)
    {
        if (val > 0)
            return val > zero ? val - zero : 0;
        else
            return val < -zero ? val + zero : 0;
    }
    private static float applySensitivity(float value, float sens)
    {
        sens = sens < .5f ? sens + .5f : (sens * 4 - 1);

        return value < 0 ? -Mathf.Pow(-value, sens) : Mathf.Pow(value, sens);
    }
    private void UpdateControls(float scale)
    {
        rScene.GetSceneVisualizer().GetCameraData().SetRef(Player.Ptr().GetHandle());
        // управление
        if (myDI != null)
        {
            Vector3 c = Vector3.zero;
            Vector3 t = Vector3.zero;
            if (rScene.GetGameData().mUseControls == true)
            {
                // клавиатура
                c = KbdTurn;
                // спец. устройства
                float[] signs = { -1, 1 };

                switch (myDevice)
                {
                    case 0: // keys only
                        break;
                    case 1: // eMouse
                        MouseTurnTo();
                        //MouseCursorTurnTo();
                        //MouseLookTurnTo(); //TODO! Вынести в отдельный тип управления
                        break;
                    case 2: // Mousa a-la Quake
                        myMouseTimer += scale;
                        if (myMouseTimer >= .05)
                        {
                            float dx = myDI.getMouseDX();
                            float dy = myDI.getMouseDY();
                            myMouseX = MouseAlaQuakeValue(dx, myDeviceSens * DeviceMode2SensC, myMouseTimer);
                            myMouseY = MouseAlaQuakeValue(dy, myDeviceSens * DeviceMode2SensC, myMouseTimer);
                            myMouseTimer = 0;
                        }
                        if (myDeviceXasYaw != 0)
                            c.y += myMouseX;
                        else
                            c.z += myMouseX;

                        //select <float> (c.y, c.z, myDeviceXasYaw) += myMouseX;
                        c.x += myMouseY * signs[myDeviceInvY];
                        break;
                    case 3: // Mousa a-la Joystick
                        myMouseTimer += scale;
                        if (myMouseTimer >= .05)
                        {
                            float dx = myDI.getMouseDX();
                            float dy = myDI.getMouseDY();
                            myMouseX = MouseAlaJoystickValue1(myMouseX, dx, myDeviceSens * DeviceMode3SensC, myDeviceDeadZone);
                            myMouseY = MouseAlaJoystickValue1(myMouseY, dy, myDeviceSens * DeviceMode3SensC, myDeviceDeadZone);
                            myMouseTimer = 0;
                        }

                        if (myDeviceXasYaw != 0)

                            c.y += MouseAlaJoystickValue2(myMouseX, myDeviceDeadZone);
                        else
                            c.z += MouseAlaJoystickValue2(myMouseX, myDeviceDeadZone);
                        //select < float &> (c.y, c.z, myDeviceXasYaw) +=
                        //    MouseAlaJoystickValue2(myMouseX, myDeviceDeadZone);
                        c.x += signs[myDeviceInvY] * MouseAlaJoystickValue2(myMouseY, myDeviceDeadZone);
                        break;

                    default:
                        // джойстик
                        c.x +=
                            applySensitivity(myDI.getJoystickY(), Mathf.Clamp(myDeviceSens, 0, 1));
                        if (myDeviceXasYaw != 0)
                            c.y += applySensitivity(myDI.getJoystickX(), Mathf.Clamp(myDeviceSens, 0, 1));
                        else
                            c.z += applySensitivity(myDI.getJoystickX(), Mathf.Clamp(myDeviceSens, 0, 1));

                        //select < float &> (c.y, c.z, myDeviceXasYaw) +=
                        //    applySensitivity(myDI->getJoystickX(), Clamp<float>(myDeviceSens, 0, 1));

                        // throttle
                        if (myUseThrottleOnJoy)
                            AutoThrottle = myDI.getThrottle();
                        break;
                }
                // педали
                if (myRudder > 0)
                    if (myDeviceXasYaw != 0)
                        c.z += applySensitivity(myDI.getRudder(), myRudderSens);
                    else
                        c.y += applySensitivity(myDI.getRudder(), myRudderSens);
                //select < float &> (c.z, c.y, myDeviceXasYaw) +=
                //    applySensitivity(myDI->getRudder(), myRudderSens);

                // тяга
                t = KbdMove;
                if (t.z == 0) t.z = AutoThrottle * Mathf.Abs(AutoThrottle);
            }
            pControls.SetControls(c);
            pControls.SetThrust(t);
        }
        // автопилот
        AutopilotLeader.Validate();
        AutopilotHangar.Validate();
        if (AutopilotOn)
        {
            if (AutopilotHangar.Ptr() != null)
            { // land
                pControls.Land(AutopilotHangar.Ptr(), myFinalAutopilot);
            }
            else
            {
                if (AutopilotTime >= .0f)
                {
                    if (PausedOn)
                    {  // pause
                        pControls.Pause();
                    }
                    else
                    {  // fly to
                        Vector3 Delta = AutopilotOrg - Player.Ptr().GetOrg();
                        float d = Delta.magnitude;
                        float dt = AutopilotTime - rScene.GetTime();
                        if (d > .0f)
                        {
                            Delta /= d;
                            float speed = (myRequestedSpeed > 0f) ? Storm.Math.KPH2MPS(myRequestedSpeed) : (dt > 0 ? d / dt : 1000f);
                            pControls.FlyTo(Delta, new Vector3(0, 0, speed));
                        }
                    }
                }
                else   // follow
                    pControls.FollowUnit(AutopilotLeader.Ptr(), AutopilotOrg);
            }
        }
        else
        {
            //myFinalAutopilot = AutopilotHangar() != null ? canTurnAutopilotOn(scale) : false;
            myFinalAutopilot = AutopilotHangar.Ptr() != null ? canTurnAutopilotOn(scale) : false;
            if (myFinalAutopilot)  // land
                SetAutopilotMode(true);
            if (myDevice != 1) // ручной режим управления
                pControls.Manual();
        }
    }

    private T select<T>(T a, T b, bool pr)
    {
        return pr ? a : b;
    }

    public void SetDevice(int v)
    {
        if (myDevice != v)
        {
            if (v < 4 || myDI.useJoysticks(Mathf.Max(v - 4, -1), myRudder - 1))
            {
                myDevice = v;
                myMouseX = 0;
                myMouseY = 0;
            }
            else
                rScene.Message("Failed to set Joy {0}", v.ToString());
        }
    }
    public void SetDeviceSens(float v)
    {
        myDeviceSens = v;
    }
    public void SetDeviceZero(float v)
    {
        myDeviceDeadZone = Mathf.Clamp(v, 0, 1);
        if (myDevice >= 4)
            myDI.setJoystickZone(myDeviceDeadZone);
    }
    public void SetDeviceInvY(int v)
    {
        myDeviceInvY = v;
    }
    public void SetDeviceXasYaw(int v)
    {
        myDeviceXasYaw = v;
    }
    public void SetRudder(int v)
    {
        if (myRudder != v)
        {
            if (v < 1 || myDI.useJoysticks(Mathf.Max(myDevice - 4, -1), v - 1))
                myRudder = v;
            else
                rScene.Message("Failed to set Rudder {0}", v.ToString());
        }
    }
    public void SetRudderSens(float v)
    {
        myRudderSens = Mathf.Clamp(v, 0, 1);
    }
    public void SetRudderZero(float v)
    {
        myRudderDeadZone = Mathf.Clamp(v, 0, 1);
        if (myRudder >= 1)
            myDI.setRudderZone(myRudderDeadZone);
    }

    // API for CameraLogicCockpit
    public int GetAutopilotState()
    {
        Asserts.AssertBp(Player.Ptr() != null);
        if (Player.Ptr().GetState() != CS_IN_GAME) return 2; //Это индекс в массиве { HUDCOLOR_DISABLED, HUDCOLOR_NORMAL, HUDCOLOR_DANGER };
        return (AutopilotOn == true ? 1 : 0);
    }
    public int GetSituationState()
    {
        Asserts.AssertBp(Player.Ptr() != null);
        if (SuggestedTarget.Ptr() == null) return 0;
        return (SuggestedTarget.Ptr() == pWeapons.GetTarget() ? 1 : 2); //Это индекс в массиве { HUDCOLOR_DISABLED, HUDCOLOR_NORMAL, HUDCOLOR_DANGER };
    }


    // own
    protected BaseScene rScene;
    protected readonly TContact Player = new TContact();
    protected iMovementSystemCraft pControls;
    public iWeaponSystemDedicated pWeapons;
    protected iWeaponSystemTurrets pTurrets;
    public BaseCraftController(BaseScene s)
    {
        rScene = s;
        AutopilotOn = false;
        pControls = null;
        pWeapons = null;
        pTurrets = null;
        mpMissileWarningSound = null;
        mpMissileStartSound = null;
        mpNewWaypointSound = null;
        mOldCondition = 0;
        mOldEnergy = 0;

        KbdTurn = Vector3.zero;
        OldKbdTurn = Vector3.zero;
        KbdMove = Vector3.zero;
        OldKbdMove = Vector3.zero;
        AutoThrottle = 0;

        myDevice = 0; myDeviceSens = 0; myDeviceDeadZone = 0; myDeviceInvY = 0; myDeviceXasYaw = 0;

        myRudder = 0; myRudderSens = 0; myRudderDeadZone = 0;
        myMouseX = 0; myMouseY = 0; myMouseTimer = 0;

        myPrevMissileCount = 0;
        myRequestedSpeed = 0f;

        myDI = s.GetInputApi();

        mOldWeaponCond[0] = 0;
        mOldWeaponCond[1] = 0;
        mOldWeaponCond[2] = 0;
        // устанвлиаем начальные значения
        iUnifiedVariableContainer cnt = rScene.GetGameData().mpControls.openContainer("Devices");
        Asserts.Assert(cnt != null);

        //TODO: восстановить настройку управления
        SetDevice(0);
        //SetDevice(cnt.getInt("Device"));
        //SetRudder(cnt.getInt("Rudder"));
        //SetDeviceSens(cnt.getFloat("DeviceSensitivity"));
        //SetDeviceZero(cnt.getFloat("DeviceDeadZone"));
        //SetDeviceInvY(cnt.getInt("DeviceInvertY"));
        //SetDeviceXasYaw(cnt.getInt("DeviceXasYaw"));
        //SetRudderSens(cnt.getFloat("RudderSensitivity"));
        //SetRudderZero(cnt.getFloat("RudderDeadZone"));
        //myUseThrottleOnJoy = cnt.getInt("DeviceThrottle") != 0;

        /*
        rScene.Message("Dev=%d, Dsens=%f, Ddz=%f, Rud=%d, Rsens=%f, Rdz=%f, inv=%d, XasY=%d",
            myDevice, myDeviceSens, myDeviceDeadZone, 
            myRudder, myRudderSens, myRudderDeadZone,
            myDeviceInvY, myDeviceXasYaw);
        */
        // создаем звуки
        mpMissileWarningSound = rScene.GetRadioEnvironment().CreateWave(0, "MissileWarning");
        mpMissileStartSound = rScene.GetRadioEnvironment().CreateWave(0, "MissileStart");
        mpNewWaypointSound = rScene.GetRadioEnvironment().CreateWave(0, "SuggestedWaypoint");
        mpSuggestedTargetSound = rScene.GetRadioEnvironment().CreateWave(0, "SuggestedTarget");
        // регистрируем клавиатуру
        rScene.GetCommandsApi().RegisterTrigger(sClTurnLeft, this);
        rScene.GetCommandsApi().RegisterTrigger(sClTurnRight, this);
        rScene.GetCommandsApi().RegisterTrigger(sClTurnDown, this);
        rScene.GetCommandsApi().RegisterTrigger(sClTurnUp, this);
        rScene.GetCommandsApi().RegisterTrigger(sClBankLeft, this);
        rScene.GetCommandsApi().RegisterTrigger(sClBankRight, this);
        rScene.GetCommandsApi().RegisterTrigger(sClMoveLeft, this);
        rScene.GetCommandsApi().RegisterTrigger(sClMoveRight, this);
        rScene.GetCommandsApi().RegisterTrigger(sClMoveDown, this);
        rScene.GetCommandsApi().RegisterTrigger(sClMoveUp, this);
        rScene.GetCommandsApi().RegisterTrigger(sClMoveBackward, this);
        rScene.GetCommandsApi().RegisterTrigger(sClMoveForward, this);
        rScene.GetCommandsApi().RegisterTrigger(sClAttack, this);
        rScene.GetCommandsApi().RegisterVariable(sClWeapon, this, VType.VAR_INT, "set weapon");
        rScene.GetCommandsApi().RegisterCommand(sClWeaponNext, this, 0, "switch to next weapon");
        rScene.GetCommandsApi().RegisterCommand(sClWeaponPrev, this, 0, "switch to previous weapon");
        // регистрируем device
        rScene.GetCommandsApi().RegisterVariable(sIDevice, this, VType.VAR_INT, "device mode");
        rScene.GetCommandsApi().RegisterVariable(sIDeviceSens, this, VType.VAR_FLOAT, "device sensitivity");
        rScene.GetCommandsApi().RegisterVariable(sIDeviceDeadZone, this, VType.VAR_FLOAT, "device dead zone");
        rScene.GetCommandsApi().RegisterVariable(sIDeviceInvY, this, VType.VAR_INT, "dvice Y invert");
        rScene.GetCommandsApi().RegisterVariable(sIDeviceXasYaw, this, VType.VAR_INT, "device X controls Yaw");
        rScene.GetCommandsApi().RegisterVariable(sIRudder, this, VType.VAR_INT, "rudder mode");
        rScene.GetCommandsApi().RegisterVariable(sIRudderSens, this, VType.VAR_FLOAT, "rudder sensitivity");
        rScene.GetCommandsApi().RegisterVariable(sIRudderDeadZone, this, VType.VAR_FLOAT, "rudder dead zone");
        rScene.GetCommandsApi().RegisterTrigger(sIEMaxTrun, this, "e-Mouse max turn mode");
        // регистрируем автопилот
        rScene.GetCommandsApi().RegisterVariable(sClAutopilot, this, VType.VAR_INT, "enable/disable autopilot");
        rScene.GetCommandsApi().RegisterCommand(sClAutoThrottle, this, 1, "set auto-throttle");
        rScene.GetCommandsApi().RegisterCommand(sClJoystickThrottle, this, 1, "use joystick throttle");
        // команды выбора целей
        rScene.GetCommandsApi().RegisterCommand(sClTgtNext, this, 0, "select next target");
        rScene.GetCommandsApi().RegisterCommand(sClTgtPrev, this, 0, "select prev target");
        rScene.GetCommandsApi().RegisterCommand(sClTargetThreat, this, 0, "select most threating unit as target");
        rScene.GetCommandsApi().RegisterCommand(sClTargetNearest, this, 0, "select nearset enemy unit as target");
        rScene.GetCommandsApi().RegisterCommand(sClTargetAtRecticle, this, 0, "select closest to recticle enemy unit as target");
        rScene.GetCommandsApi().RegisterCommand(sClTargetAccept, this, 0, "accept target");
        rScene.GetCommandsApi().RegisterCommand(sClTargetKill, this, 0, "kill target - mission debug purposes only");

#if _DEBUG
        rScene.GetCommandsApi().RegisterVariable(sIDeviceSensC1, this, VAR_FLOAT);
        rScene.GetCommandsApi().RegisterVariable(sIDeviceSensC2, this, VAR_FLOAT);
        rScene.GetCommandsApi().RegisterVariable(sIDeviceSensC3, this, VAR_FLOAT);
        rScene.GetCommandsApi().RegisterVariable("LandCraftMaxRoll", this, VAR_FLOAT);
        rScene.GetCommandsApi().RegisterVariable("LandCraftMaxYaw", this, VAR_FLOAT);
        rScene.GetCommandsApi().RegisterVariable("LandCraftMaxSpeed", this, VAR_FLOAT);
        rScene.GetCommandsApi().RegisterVariable("LandCraftMaxRange", this, VAR_FLOAT);
        rScene.GetCommandsApi().RegisterVariable("LandIntMinRange", this, VAR_FLOAT);
        rScene.GetCommandsApi().RegisterVariable("LandIntMaxRange", this, VAR_FLOAT);
        rScene.GetCommandsApi().RegisterVariable("LandIntMaxGlissadeAngle", this, VAR_FLOAT);
        rScene.GetCommandsApi().RegisterVariable("LandIntMaxRoll", this, VAR_FLOAT);
        rScene.GetCommandsApi().RegisterVariable("LandMaxCourseAngle", this, VAR_FLOAT);
        rScene.GetCommandsApi().RegisterVariable("LandIntMinSpeed", this, VAR_FLOAT);
        rScene.GetCommandsApi().RegisterVariable("LandIntMaxSpeed", this, VAR_FLOAT);
#endif

    }
    ~BaseCraftController()
    {
        Player.Validate();
        // убираем свои команды
        rScene.GetCommandsApi().UnRegister(this);
        // убиваем звуки
        mpMissileStartSound.Release();
        mpMissileWarningSound.Release();
        mpNewWaypointSound.Release();
        mpSuggestedTargetSound.Release();
        AutopilotHangar.Validate();
        if (AutopilotHangar.Ptr() != null)
            setHangarStatus(AutopilotHangar.Ptr(), hsClosed);
    }
    public void Process(float scale)
    {
        Player.Validate();
        if (Player.Ptr() == null)
            return;
        UpdateSounds(scale);
        UpdateControls(scale);
        UpdateTargets(scale);
    }
    public void SetOwnContact(iContact pNewContact)
    {
        //bool IsFirstTime = (Player == null);
        //Player = new TContact(pNewContact);
        bool IsFirstTime = (Player.Ptr() == null);
        Player.setPtr(pNewContact);
        pControls = (iMovementSystemCraft)Player.Ptr().GetInterface(iMovementSystemCraft.ID);
        pWeapons = (iWeaponSystemDedicated)Player.Ptr().GetInterface(iWeaponSystemDedicated.ID);
        pTurrets = (iWeaponSystemTurrets)Player.Ptr().GetInterface(iWeaponSystemTurrets.ID);
        // выполняем специфичные для craft player установки
        if (pControls != null) pControls.SetControlScale(1);
        if (pWeapons != null) pWeapons.SetAimError(0);
        rScene.GetSceneVisualizer().GetCameraData().SetRef(Player.Ptr().GetHandle());
        // если первый раз - иничиализация
        if (IsFirstTime == true)
        {
            // данные автопилота
            SetPause(true);
            AutopilotOn = false;
            AutopilotTime = .0f;
            AutopilotOrg = pNewContact.GetOrg();
            // устанавливаем режим Cockpit
            rScene.GetSceneVisualizer().SetCameraLogic(iCmNone);
            rScene.GetSceneVisualizer().SetCameraLogic(iCmCockpit);
        }
    }
};
