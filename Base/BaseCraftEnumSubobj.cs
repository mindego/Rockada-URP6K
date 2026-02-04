
/// <summary>
/// BaseCraftEnumSubobj (Спасибо, Кэп!)
/// </summary>
class BaseCraftEnumSubobj : ISlotEnum
{

    private const string CantLoadWeapon = "Error : craft can't load {0} weapon slot";

    private BaseScene rScene;
    private BaseCraft rCraft;
    WPN_DATA PrimaryWeapon;
    WPN_DATA SecondaryWeapon;
    WPN_DATA RocketWeapon;
    public virtual bool ProcessSlot(SLOT_DATA sld, int slot_id, FPO r)
    {
        uint n = Hasher.HshString(new string(sld.Name));
        switch (n)
        {
            case 0x0C90CFDB: // DuzaFront
            case 0x6B09D242: // DuzaBack
            case 0x46A1D839: // DuzaRight
            case 0x7CA0F41D: // DuzaLeft
            case 0x601505A9: // DuzaTop
            case 0x1E70AAF1: // DuzaBottom
                return true;
        }

        if (rScene.IsClient()) return true;

        // проверка на вшивость
        Asserts.AssertEx(r.Link != null);
        BaseSubobj so = (BaseSubobj)((iBaseInterface)r.Link).GetInterface(BaseSubobj.ID);
        Asserts.AssertEx(so != null);

        // создаем оружие
        LAYOUT_DATA dt = rCraft.Dt().GetLayout(LAYOUT_DATA.WEAPON_LAYOUT);
        if (dt == null) return true;
        // перебираем все итем в текущем раскладе
        int i = 0;
        foreach (var li in dt.Items)
        {
            // проверяем имя итема
            if (li.Name != n) { i++; continue; }
            // если не HostScene, просто обозначаем, что обработали
            try
            {
                WPN_DATA sd = null;
                if (li.Value == "PRIMARY") sd = PrimaryWeapon;
                else
                  if (li.Value == "SECONDARY") sd = SecondaryWeapon;
                else sd = RocketWeapon;
                if (sd != null)
                {
                    ((HostScene)rScene).CreateSubobj(rCraft, so, sd, r, sld, slot_id, i);
                    //UnityEngine.Debug.Log(string.Format("li: {0} {1} {2}", li, i,dt)); //Выбор номера слота работает корректно.
                    return true;
                }
            }
            catch 
            {
                rScene.Message("Craft \"{0}\": failed to process slot \"{1}\"!", rCraft.GetObjectData().FullName, li.Value);
                throw;
            }
            i++;
            return true;
        }
        return true;
    }
    public BaseCraftEnumSubobj(BaseScene s, BaseCraft o)
    {
        rScene = s;
        rCraft = o;
        PrimaryWeapon = null;
        SecondaryWeapon = null;
        RocketWeapon = null;
    }
    public void Init(uint n1, uint n2, uint n3)
    {
        //if (n1 != -1)
        if (n1 != Constants.THANDLE_INVALID)
        {
            try
            {
                PrimaryWeapon = (WPN_DATA)WPN_DATA.GetByCode(n1);
            }
            catch (System.Exception e)
            {
                rScene.Message(CantLoadWeapon, "primary");
                throw;
            }
            Asserts.AssertEx(PrimaryWeapon.GetClass() == SUBOBJ_DATA.SC_WEAPON_SLOT);
        }
        if (n2 != Constants.THANDLE_INVALID)
        {
            try
            {
                SecondaryWeapon = (WPN_DATA)WPN_DATA.GetByCode(n2);
            }
            catch (System.Exception e)
            {
                rScene.Message(CantLoadWeapon, "secondary");
                throw;
            }
            Asserts.AssertEx(SecondaryWeapon.GetClass() == SUBOBJ_DATA.SC_WEAPON_SLOT);
        }
        if (n3 != Constants.THANDLE_INVALID)
        {
            try
            {
                RocketWeapon = (WPN_DATA)WPN_DATA.GetByCode(n3);
            }
            catch (System.Exception e)
            {
                rScene.Message(CantLoadWeapon, "rocket");
                throw;
            }
            Asserts.AssertEx(RocketWeapon.GetClass() == SUBOBJ_DATA.SC_WEAPON_SLOT);
        }
    }
};
