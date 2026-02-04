using crc32 = System.UInt32;

public class MenuService <TeamImp>: HandlerService<IMenuService, IMenuHandler>, IMenuService where TeamImp: IMenuHolder
{
    public void addMenuItem(string name, string caption, string parentitem)
    {
        myMsn.AddMenuItem(name, Hasher.HshString(name), caption, parentitem);
    }

    public void deleteMenuItem(crc32 nm)
    {
        myMsn.DeleteMenuItem(nm);
    }

    public void notifySelected(crc32 select)
    {
        for (int i = 0; i < myHandlers.Count;)
        {
            bool ret = myHandlers[i].notifySelect(select);
            i = processRemove(ret, i);
        }
    }

    public MenuService()
    {
        myMsn = default(TeamImp); //TODO! Какая-то херня получается
    }

    public void initialize(TeamImp msn)
    {
        myMsn = msn;
    }

    TeamImp myMsn;
};
