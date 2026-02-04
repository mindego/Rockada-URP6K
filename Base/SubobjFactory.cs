using DWORD = System.UInt32;

public static class SubobjFactory
{
    public static BaseSubobj CreateSubobj(BaseScene s, SUBOBJ_DATA sd, DWORD h)
    {
        switch (sd.GetClass())
        {
            case SUBOBJ_DATA.SC_WEAPON_SLOT:
                switch (((WPN_DATA)sd).Type)
                {
                    case WpnDataDefines.WT_PLASMA: return new BaseWeaponSlotPlasma(s, h, sd);
                    case WpnDataDefines.WT_GUN: return new BaseWeaponSlotGun(s, h, sd);
                    case WpnDataDefines.WT_ROCKET: return new BaseWeaponSlotRocket(s, h, sd);
                    case WpnDataDefines.WT_MISSILE: return new BaseWeaponSlotMissile(s, h, sd);
                    default: Asserts.AssertEx(false); break;
                }
                break;
            case SUBOBJ_DATA.SC_TURRET:
                return new BaseTurret(s, h, sd);
            case SUBOBJ_DATA.SC_RADAR:
                return new BaseRadar(s, h, sd);
            case SUBOBJ_DATA.SC_FOOT:
                return new BaseFoot(s, h, sd);
            case SUBOBJ_DATA.SC_DETACHED:
                return new BaseDetached(s, h, sd);
            case SUBOBJ_DATA.SC_HANGAR:
                return new BaseSubHangar(s, h, sd);
            case SUBOBJ_DATA.SC_SFG:
                return new BaseSubSfg(s, h, sd);
        }

        return new BaseSubobj(s, h, sd);
    }
}
