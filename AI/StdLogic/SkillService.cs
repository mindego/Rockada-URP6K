using DWORD = System.UInt32;
public class SkillService<T> : ISkillService where T : ISkillable
{
    public virtual void setSkill(DWORD name)
    {
        myMsn.SetSkill(name, myBool);
    }
    public SkillService(T imp, bool bl)
    {
        myMsn = imp;
        myBool = bl;
    }

    T myMsn;
    bool myBool;
};

public class SkillServiceNoBool<T> : ISkillService where T : StdGroupAi
{
    public virtual void setSkill(DWORD name)
    {
        myMsn.SetSkill(name);
    }
    public SkillServiceNoBool(T imp)
    {
        myMsn = imp;
    }


    T myMsn;
};

public interface ISkillable
{
    public void SetSkill(DWORD name, bool already_setted);
}
