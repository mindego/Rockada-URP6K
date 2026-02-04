using System.Linq;

public class DUST_DATA : STORM_DATA,iSTORM_DATA<DUST_DATA>
{
    public static StormDataHolder<DUST_DATA> Datas = new StormDataHolder<DUST_DATA>("dust");
    public const int MAX_DUSTS_COUNT = 9;
    // internal part
    public DUST_DATA(string n) : base(n)
    {
        mLoadSound = false;
        //MemSet(Dust, MAX_DUSTS_COUNT * 4, 0);
    }
    public DUST_DATA()
    {
        //Конструктор-заглушка для временных файлов
    }
    public override void ProcessToken(READ_TEXT_STREAM st, string value)
    {
        // читаем данные до "}"
        do
        {
            if (st.LoadBool(ref mLoadSound, "LoadSound")) continue;
            if (st.LdHS(ref Dust[0], "Dust[0]")) continue;
            if (st.LdHS(ref Dust[1], "Dust[1]")) continue;
            if (st.LdHS(ref Dust[2], "Dust[2]")) continue;
            if (st.LdHS(ref Dust[3], "Dust[3]")) continue;
            if (st.LdHS(ref Dust[4], "Dust[4]")) continue;
            if (st.LdHS(ref Dust[5], "Dust[5]")) continue;
            if (st.LdHS(ref Dust[6], "Dust[6]")) continue;
            if (st.LdHS(ref Dust[7], "Dust[7]")) continue;
            if (st.LdHS(ref Dust[8], "DustOnObject")) continue;
            if (st.Recognize("Reference"))
            {
                Reference(GetByName(st.GetNextItem()));
                return;
            }
            base.ProcessToken(st, value);
        } while (false);

    }
    public virtual void Reference(DUST_DATA dataref)
    {
        mLoadSound = dataref.mLoadSound;
        for (int i = 0; i < MAX_DUSTS_COUNT; i++)
            Dust[i] = dataref.Dust[i];

    }
    // data section
    public bool mLoadSound;
    public uint[] Dust = new uint[MAX_DUSTS_COUNT];
    int GetDustDim() { return MAX_DUSTS_COUNT; }
    // data access
    public static DUST_DATA GetByName(string Name, bool MustExist = true)
    {
        //foreach (DUST_DATA d in Datas)
        //{
        //    if (d.FullName == Name) return d;
        //}
        //return null;
        //return Datas.GetItem(Name, 0, MustExist);
        //uint Code = Constants.THANDLE_INVALID;
        //if (Name != "") Code = Hasher.HshString(Name);
        uint Code = Constants.THANDLE_INVALID;
        if (Name != "") Code = Hasher.HshString(Name);
        //if (Name != "") Code = Hasher.CodeString(Name);
        return GetByCode(Code, MustExist);
    }
    public static DUST_DATA GetByCode(uint Code, bool MustExist = true)
    {
        //foreach (DUST_DATA d in ObjDatasHolder.DatasDust)
        //{
        //    if (d.GetName() == Code) return d;
        //}
        //return null;
        //return Datas.GetItem(Name, 0, MustExist);
        //uint Code = Constants.THANDLE_INVALID;
        //if (Name != "") Code = Hasher.HshString(Name);
        foreach  (DUST_DATA data in Datas)
        {
            if (data.Name == Code) return data;
        }
        if (MustExist) throw new System.Exception($"Could not get  DUST_DATA {Code:X8}");
        return null;
    }
    public static DUST_DATA GetFirstItem()
    {
        return Datas.First();
    }
    public static int nItems()
    {
        return Datas.Count;
    }

    public static void insertDustData(READ_TEXT_STREAM st)
    {
        DUST_DATA vData = new DUST_DATA(st.GetNextItem());
        StormLog.LogMessage("Loading Dust " + vData.FullName + " [OK]", StormLog.logPriority.DEBUG);
        Datas.InsertItem(vData, st.LineNumber()).Load(st);
    }

    //public static void loadDustData(int code, PackType db)
    //{
    //    switch (code)
    //    {
    //        case 0:
    //            LoadUtils.parseData(db, Datas.GetItemName(), "Dusts", "Dusts.txt", "[STORM DUST DATA FILE V1.0]", "Dust", insertDustData);
    //            return;
    //    }

    //}
    public static void loadDustData(int code, IMappedDb db)
    {
        switch (code)
        {
            case 0:
                LoadUtils.parseData(db, Datas.GetItemName(), "Dusts", "Dusts.txt", "[STORM DUST DATA FILE V1.0]", "Dust", insertDustData);
                return;
        }

    }

    public DUST_DATA GetByCodeLocal(uint Code, bool MustExist = true)
    {
        return DUST_DATA.GetByCode(Code, MustExist);
    }
}
