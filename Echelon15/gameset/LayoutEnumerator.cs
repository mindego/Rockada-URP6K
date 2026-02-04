using System;
using UnityEngine.Assertions;
using static WpnDataDefines;
using static SUBOBJ_DATA;
using static OBJECT_DATA;

internal class LayoutEnumerator
{
    internal static void enumLayouts<T>(T data, IEnumer<EnumItemInfo> enumer) where T : OBJECT_DATA
    {
        throw new NotImplementedException();
    }

    internal static bool enumWeapons(CRAFT_DATA data, StormDataHolder<SUBOBJ_DATA> Data, int layout_idx, IEnumer<EnumItemInfo> enumer)
    {
        Assert.IsTrue(layout_idx >= 0 && data != null && layout_idx <= 2);

        LAYOUT_DATA ld = findFirstWeaponLayout(data);
        if (ld == null) return false;
        int[] weights = { WW_SMALL - 1, WW_SMALL - 1, WW_SMALL - 1 };
        foreach (LAYOUT_ITEM li in ld.Items)
        {
            int idx = getWeaponIndex(li);
            if (idx == layout_idx)
                weights[idx] = getLayoutItemWeight(li);
        }

        foreach (SUBOBJ_DATA s in Data)
        {
            if (s.GetClass() == SUBOBJ_DATA.SC_WEAPON_SLOT)
            {
                WPN_DATA w = (WPN_DATA)s;
                EnumItemInfo info = new EnumItemInfo();

                if (w.IsHuman == (data.Side == 0))
                {
                    if (w.Type == WT_ROCKET || w.Type == WT_MISSILE)
                    {
                        if (w.Weight <= weights[2])
                        {
                            info.myFullName = w.FullName;
                            info.myName = w.Name;
                            info.myDescription = w.DescriptionShort;
                            enumer.process(info);
                        }
                    }
                    else
                        for (int i = 0; i != 2; ++i)
                            if (w.Weight <= weights[i])
                            {
                                info.myFullName = w.FullName;
                                info.myName = w.Name;
                                info.myDescription = w.DescriptionShort;
                                enumer.process(info);
                            }
                }
            }
        }
        return true;
    }
    internal static bool enumWeapons<T>(CRAFT_DATA data, StormDataHolder<SUBOBJ_DATA> Data,  int layout_idx, IEnumer<EnumItemInfo> enumer)
    {
        Assert.IsTrue(layout_idx >= 0 && data!=null && layout_idx <= 2);


        throw new NotImplementedException();
    }

    private static LAYOUT_DATA findFirstWeaponLayout(CRAFT_DATA d) //TODO переделать в Generic класс вместо явного CRAFT_DATA
    {
        foreach (LAYOUT_DATA r in d.Layouts)
            if (r.Type == LAYOUT_DATA.WEAPON_LAYOUT) return r;
        return null;
        
    }
    private static int getWeaponIndex(LAYOUT_ITEM li)
    {
        switch (li.Name)
        {
            case 0xD7FFEB63: return 0; // PRIMARY
            case 0xA06E9505: return 1; // SECONDARY
            case 0xC1BE8367: return 2; // ROCKET
        }
        return -1;
    }
    private static int getLayoutItemWeight(LAYOUT_ITEM li)
    {
        string name = li.Value;
        
        switch (Hasher.HshString(name.ToUpper()))
        {
            case 0x7DEED75C: return WW_SMALL;    // SMALL
            case 0xC9747DCE: return WW_MEDIUM;   // MEDIUM
            case 0x729A4710: return WW_HEAVY;    // HEAVY
        }
        return WW_STATIC;
    }
}