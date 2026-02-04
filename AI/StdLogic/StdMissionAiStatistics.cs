using DWORD = System.UInt32;

public abstract partial class StdMissionAi : IMissionAi
{
    public bool RegisterDamageUser(IBaseUnitAi ai)
    {
        ContactInfo info = FindContact(ai);
        if (info!=null)
        {
            info.NotifyAboutDamages(true);
            return true;
        }
        return false;
    }

    public bool UnRegisterDamageUser(IBaseUnitAi ai)
    {
        ContactInfo info = FindContact(ai);
        if (info!=null)
        {
            info.NotifyAboutDamages(false);
            return true;
        }
        return false;
    }

    public float QueryDamage(DWORD VictimHandle, DWORD GadHandle, DWORD WeaponCode, float Damage)
    {
        Asserts.AssertBp(VictimHandle != Constants.THANDLE_INVALID);
        mpJustDamaged = FindContact(VictimHandle);
        if (mpJustDamaged!=null)
        {
            if (mpJustDamaged.IsGod())
                Damage = 0;
            else if (true == IsNoDamageFromFriendly() && Constants.THANDLE_INVALID != GadHandle)
            {
                ContactInfo engager = FindContact(GadHandle);
                if (engager !=null && engager.mpGroupCont.mpGroupData.Side == mpJustDamaged.mpGroupCont.mpGroupData.Side)
                    Damage = 0;
            }
        }
        return Damage;
    }

}