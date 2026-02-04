using DWORD = System.UInt32;

public class StdStaticAi : StandartUnitAi
{
    new public const uint ID = 0x3364EDCE;
    iWeaponSystemTurrets turrets;
    float check_firing_timer;

    bool Idle(StateFlag flag, float scale)
    {
        if (turrets != null)
        {
            check_firing_timer -= scale;
            if (check_firing_timer < 0)
            {
                check_firing_timer = 2f;
                iWeaponSystemDedicated turr = null;
                aiming = 0;
                mTarget.setPtr(null);
                while ((turr = turrets.GetNextTurret(turr)) != null)
                {
                    iContact cnt = turr.GetTarget();
                    //if (cnt!=null) mTarget = new TContact(cnt);
                    if (cnt != null) mTarget.setPtr(cnt);
                    float t = turr.GetAim();
                    if (t > aiming)
                        aiming = t;
                }
            }
        }
        return true;

    }

    // api
    //IBaseUnitAi
    public override void SelectTarget(int nTargets, iContact[] Targets, float[] TargetWeights)
    {
        if (isWeaponsEnabled() == false) return;
        if (turrets != null)
            turrets.SetTargets(nTargets, Targets, TargetWeights);

    }

    public override void SetSkill(DWORD skill, bool already_setted)
    {
        base.SetSkill(skill, already_setted);
        if (turrets!=null)
            turrets.SetAimError(mpSkill.valTurretFireAimError);

    }


    // IBaseAi
    public override void SetInterface(IGame igame, iContact contact, UNIT_DATA unit_data, IGroupAi grp_ai)
    {
        base.SetInterface(igame, contact, unit_data, grp_ai);
        check_firing_timer = -1f;
        SetState(Idle);
    }

    public override void SideChanged(iContact new_cnt)
    {
        base.SideChanged(new_cnt);
        turrets = (iWeaponSystemTurrets) self.Ptr().GetInterface(iWeaponSystemTurrets.ID);

    }
    

    public override void setFireMode(bool enable)
    {
        base.setFireMode(enable);
        if (enable == false)
        {
            setTarget(null);
            turrets.SetTargets(0, null, null);
        }

    }
};

