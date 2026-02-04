public abstract partial class StdMissionAi
{
    public void UpdateMenu(IGroupAi grp, bool HasNewItem)
    {

        foreach (MissionClient mcl in mlClients)
        {
            bool upd = false;
            if (grp == null)
                upd = true;
            else if (mcl.Group() != null && mcl.Group() == grp)
            {
                IBaseUnitAi un = mcl.Client().GetAI();
                if (un != null)
                {
                    iContact cnt = un.GetContact();
                    if (cnt != null)
                    {
                        iContact ld = mcl.Group().GetLeaderContact();
                        if (ld != null && ld.GetHandle() == cnt.GetHandle())
                            upd = true;
                    }
                }

            }
            if (upd)
                mcl.Client().UpdateMenu(HasNewItem);
        }
    }

    public int SelectItem(iClient client, uint id)
    {
        uint ret = Constants.THANDLE_INVALID;
        MissionClient mcl = FindClient(client);
        if (mcl!=null && mcl.Group()!=null)
        {
            IBaseUnitAi ai = client.GetAI();
            if (ai!=null)
                ret = (uint) mcl.Group().SelectItem(ai, id);
        }

        if (ret == Constants.THANDLE_INVALID)
        {
            myMenuService.notifySelected(id);

            IMenuItemHolder hld = mpMenuHolder.FindItem(id); // находим выбранный пункт
            if (hld!=null)
            {
                if (hld.GetChildCount()!=0)
                    ret = hld.GetName();
            }
        }
        return (int) ret;
    }
};