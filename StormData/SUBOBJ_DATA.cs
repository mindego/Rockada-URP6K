using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using DWORD = System.UInt32;
using static stormdata_dll;

public class SUBOBJ_DATA : STORM_DATA, iSTORM_DATA<SUBOBJ_DATA>
{
    public static StormDataHolder<SUBOBJ_DATA> Datas = new StormDataHolder<SUBOBJ_DATA>("subobj");
    // маски
    public const uint SF_CLASS_MASK = 0x000000FF;
    public const uint SF_FLAGS_MASK = 0x0000FF00;
    // классы подобъектов
    public const uint SC_SUBOBJ = 0x00000000;
    public const uint SC_TURRET = 0x00000001;
    public const uint SC_CRAFT_PART = 0x00000002;
    public const uint SC_WEAPON_SLOT = 0x00000003;
    public const uint SC_DEBRIS_PART = 0x00000004;
    public const uint SC_RADAR = 0x00000005;
    public const uint SC_FOOT = 0x00000006;
    public const uint SC_DETACHED = 0x00000007;
    public const uint SC_HANGAR = 0x00000008;
    public const uint SC_SFG = 0x00000009;
    // флаги подобъекта
    public const uint SF_DETACHED = 0x00000100;
    public const uint SF_CRITICAL = 0x00000200;

    public string DescribeFlags()
    {
        string res = FullName;
        res += "\nSF_CLASS_MASK " + (GetFlag(SF_CLASS_MASK) != 0);
        res += "\nSF_FLAGS_MASK " + (GetFlag(SF_FLAGS_MASK) != 0);
        res += "\nSC_SUBOBJ " + (GetFlag(SC_SUBOBJ) != 0);
        res += "\nSC_TURRET " + (GetFlag(SC_TURRET) != 0);
        res += "\nSC_CRAFT_PART " + (GetFlag(SC_CRAFT_PART) != 0);
        res += "\nSC_WEAPON_SLOT " + (GetFlag(SC_WEAPON_SLOT) != 0);
        res += "\nSC_DEBRIS_PART " + (GetFlag(SC_DEBRIS_PART) != 0);
        res += "\nSC_RADAR " + (GetFlag(SC_RADAR) != 0);
        res += "\nSC_FOOT " + (GetFlag(SC_FOOT) != 0);
        res += "\nSC_DETACHED " + (GetFlag(SC_DETACHED) != 0);
        res += "\nSC_HANGAR " + (GetFlag(SC_HANGAR) != 0);
        res += "\nSC_SFG " + (GetFlag(SC_SFG) != 0);
        res += "\nSF_DETACHED " + (GetFlag(SF_DETACHED) != 0);
        res += "\nSF_CRITICAL " + (GetFlag(SF_CRITICAL) != 0);
        return res;
    }
    /*    SUBOBJ_DATA(const char*);
        virtual void ProcessToken(READ_TEXT_FILE&,const char*);
        virtual void Reference(const SUBOBJ_DATA* ref);
        virtual void MakeLinks();
        // data section
        const char* Description;
        const char* DescriptionShort;
        unsigned int Flags;
        const char* FileName;
        unsigned int CodedFileName;
        float Armor;
        TLIST<SUBOBJ_DATA> SubobjDatas;
        Tab<AnimationPackage*> myAnimations;
        DEBRIS_DATA* Debris;
        DEBRIS_DATA* DetachedDebris;
        DEBRIS_DATA* SubobjDebris;
        float DeltaY;
        int UnitDataIndex;
        // data access
        void SetFlag(unsigned int Flag) { Flags |= Flag; }
        void ClearFlag(unsigned int Flag) { Flags &= ~Flag; }
        unsigned int GetFlag(unsigned int Flag) const { return (Flags&Flag); }
    unsigned int GetClass()                  const { return (Flags&SF_CLASS_MASK); }
        static STORM_DATA_API SUBOBJ_DATA*  __cdecl GetByName(const char *Name,bool MustExist=true);
    static STORM_DATA_API SUBOBJ_DATA*  __cdecl GetByCode(unsigned int Code,bool MustExist=true);
    static STORM_DATA_API SUBOBJ_DATA*  __cdecl GetFirstItem();
    static STORM_DATA_API int __cdecl nItems();
    */

    //virtual void ProcessToken(READ_TEXT_FILE&,const char*);
    //virtual void Reference(const SUBOBJ_DATA* ref);
    //public virtual void MakeLinks();
    // data section
    public string Description;
    public string DescriptionShort;
    public uint Flags;
    public string FileName;
    public uint CodedFileName;
    public float Armor;
    //TLIST<SUBOBJ_DATA> SubobjDatas;
    public List<SUBOBJ_DATA> SubobjDatas = new List<SUBOBJ_DATA>();
    public List<AnimationPackage> myAnimations = new List<AnimationPackage>();
    public DEBRIS_DATA Debris;
    public DEBRIS_DATA DetachedDebris;
    public DEBRIS_DATA SubobjDebris;
    public float DeltaY;
    public int UnitDataIndex;
    //public string UnitData;
    // data access
    public void SetFlag(uint Flag) { Flags |= Flag; }
    public void ClearFlag(uint Flag) { Flags &= ~Flag; }
    public uint GetFlag(uint Flag) { return (Flags & Flag); }
    public uint GetClass() { return (Flags & SF_CLASS_MASK); }

    public SUBOBJ_DATA() : this("undefined") { }

    public SUBOBJ_DATA(string name) : base(name)
    {
        Description = AddString(gpDescriptionCtr, Name);
        DescriptionShort = AddString(gpDescriptionShortCtr, Name);
        //Description = "No Description";
        //DescriptionShort = "No Description found";
        FileName = FullName;
        Flags = 0;
        Armor = 100;
        Debris = null;
        DetachedDebris = null;
        SubobjDebris = null;
        DeltaY = 0;
        UnitDataIndex = 0;
    }

    public override void ProcessToken(READ_TEXT_STREAM st, string value)
    {
        do
        {
            if (st.LdAS(ref FileName, "FileName")) continue;
            if (st.LdAS(ref FileName, "ObjectName")) continue;
            if (st.LoadFloat(ref Armor, "Armor")) continue;
            if (st.LoadFloat(ref DeltaY, "DeltaY")) continue;
            if (st.LdHST<DEBRIS_DATA>(ref Debris, "Debris")) continue;

            /*
            DWORD myCode=0;
            if (st.LdHS(ref myCode,"Debris"))
            {
                Debris = DEBRIS_DATA.GetByCode(myCode);
                continue;
            }
            */




            if (st.LdHST<DEBRIS_DATA>(ref DetachedDebris, "DetachedDebris")) continue;
            if (st.LdHST<DEBRIS_DATA>(ref SubobjDebris, "SubobjDebris")) continue;
            if (st.Recognize("Critical")) { SetFlag(SF_CRITICAL); continue; }
            if (st.Recognize("Detached")) { SetFlag(SF_DETACHED); continue; }
            if (st.Recognize("Turret")) { ADDCLASS<TURRET_DATA>(st); continue; }
            if (st.Recognize("Subobj")) { ADDCLASS<SUBOBJ_DATA>(st); continue; }
            if (st.Recognize("Part")) { ADDCLASS<SUBOBJ_DATA>(st); continue; }
            if (st.Recognize("Radar")) { ADDCLASS<RADAR_DATA>(st); continue; }
            if (st.Recognize("Foot")) { ADDCLASS<FOOT_DATA>(st); continue; }
            if (st.Recognize("DetachedPart")) { ADDCLASS<DETACHED_DATA>(st); continue; }
            if (st.Recognize("Hangar")) { ADDCLASS<SUB_HANGAR_DATA>(st); continue; }
            if (st.Recognize("Sfg")) { ADDCLASS<SUB_SFG_DATA>(st); continue; }

            if (st.Recognize("Animation"))
            {
                string name = st.GetNextItem();
                AnimationPackage data = AnimationPackage.getAnimationPackage(Hasher.HashString(name));
                if (data != null)
                {
                    myAnimations.Add(data);
                    continue;
                }
                //PARSE_ERROR;
                throw new System.Exception(string.Format("{0} {1} {2}", st.LineNumber(), "Animations", st.DebugStream()));
            }

            if (st.Recognize("Reference"))
            {
                Reference(GetByName(st.GetNextItem()));
                return;
            }
            if (st.Recognize("UnitData"))
            {

                UnitDataIndex = UnitDataTable.GetIdxByName(st.GetNextItem());
                if (UnitDataIndex < 0) throw new Exception("Unit not fount in UnitDataTable");
                return;
            }

            base.ProcessToken(st, value);
        } while (false);
    }

    public virtual void Reference(SUBOBJ_DATA data)
    {
        Flags = data.Flags;
        FileName = data.FileName;
        CodedFileName = data.CodedFileName;
        Armor = data.Armor;
        Debris = data.Debris;
        DetachedDebris = data.DetachedDebris;
        SubobjDebris = data.SubobjDebris;
        DeltaY = data.DeltaY;
        UnitDataIndex = data.UnitDataIndex;
    }

    public override void MakeLinks()
    {
        CodedFileName = Hasher.HshString(FileName);
        //Вызов RSLV не требуется - переменные уже содержат сами объекты (LdHST)
        StormDataHDR.RSLV<DEBRIS_DATA>(ref Debris);
        StormDataHDR.RSLV<DEBRIS_DATA>(ref DetachedDebris);
        StormDataHDR.RSLV<DEBRIS_DATA>(ref SubobjDebris);
        //for (SUBOBJ_DATA d = SubobjDatas.Head(); d!=null; d = d.Next())
        //    d.MakeLinks();
        foreach (SUBOBJ_DATA d in SubobjDatas)
        {
            d.MakeLinks();
        }
        //if (UnitDataIndex < 0) stormdata_dll.StructError("object", FullName);

    }
    //public static SUBOBJ_DATA GetByName(string Name, bool MustExist = true)
    //{
    //    foreach (SUBOBJ_DATA data in ObjDatasHolder.DatasSubjObj)
    //    {
    //        if (data.FullName == Name) return data;
    //    }
    //    return null;
    //}

    //public SUBOBJ_DATA GetByCode(uint Code,bool MustExist=true)
    //{
    //    foreach (SUBOBJ_DATA data in ObjDatasHolder.DatasSubjObj)
    //    {
    //        if (data.CodedFileName == Code) return data;
    //    }
    //    return null;
    //}

    public int nItems()
    {
        return Datas.Count;
    }

    public SUBOBJ_DATA GetFirstItem()
    {
        //return (ObjDatasHolder.DatasSubjObj[0] != null) ? ObjDatasHolder.DatasSubjObj[0] : null;
        return Datas.First();
    }



    public void ADDTURRET(READ_TEXT_STREAM st)
    {
        TURRET_DATA obj = new TURRET_DATA(st.GetNextItem());
        obj.Load(st);
        SubobjDatas.Add(obj);
    }
    public void ADDRADAR(READ_TEXT_STREAM st)
    {
        RADAR_DATA obj = new RADAR_DATA(st.GetNextItem());
        obj.Load(st);
        SubobjDatas.Add(obj);
    }

    public void ADDSUBOBJ(READ_TEXT_STREAM st)
    {
        SUBOBJ_DATA obj = new SUBOBJ_DATA(st.GetNextItem());
        obj.Load(st);
        SubobjDatas.Add(obj);
        //Debug.Log(string.Format("Subobj added [{0}] {1} for {2}", obj.FullName,obj.Name.ToString("X8"),this.FullName));
    }

    public void ADDFOOT(READ_TEXT_STREAM st)
    {
        FOOT_DATA obj = new FOOT_DATA(st.GetNextItem());
        obj.Load(st);
        SubobjDatas.Add(obj);
    }

    public void ADDDETACHED(READ_TEXT_STREAM st)
    {
        //FOOT_DATA obj = new FOOT_DATA(st.GetNextItem());
        DETACHED_DATA obj = new DETACHED_DATA(st.GetNextItem());
        obj.Load(st);
        SubobjDatas.Add(obj);
    }

    public void ADDSUB_HANGAR(READ_TEXT_STREAM st)
    {
        SUB_HANGAR_DATA obj = new SUB_HANGAR_DATA(st.GetNextItem());
        obj.Load(st);
        SubobjDatas.Add(obj);
    }
    public void ADDPART(READ_TEXT_STREAM st)
    {
        CRAFT_PART_DATA obj = new CRAFT_PART_DATA(st.GetNextItem());
        obj.Load(st);
        SubobjDatas.Add(obj);
    }
    public void ADDSFG(READ_TEXT_STREAM st)
    {
        SUB_SFG_DATA obj = new SUB_SFG_DATA(st.GetNextItem());
        obj.Load(st);
        SubobjDatas.Add(obj);
    }
    public void ADDCLASS<T>(READ_TEXT_STREAM st)
    {
        // #define ADDCLASS(T) SubobjDatas.AddToTail(new T(f.GetNextItem()))->Load(f)
        Type useObjectType = typeof(T);
        if (useObjectType == typeof(TURRET_DATA)) ADDTURRET(st);
        if (useObjectType == typeof(SUBOBJ_DATA)) ADDSUBOBJ(st);
        if (useObjectType == typeof(RADAR_DATA)) ADDRADAR(st);
        if (useObjectType == typeof(FOOT_DATA)) ADDFOOT(st);
        if (useObjectType == typeof(DETACHED_DATA)) ADDDETACHED(st);
        if (useObjectType == typeof(SUB_HANGAR_DATA)) ADDSUB_HANGAR(st);
        if (useObjectType == typeof(CRAFT_PART_DATA)) ADDPART(st);
        if (useObjectType == typeof(SUB_SFG_DATA)) ADDSFG(st);

    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder(base.ToString());
        //From SUBOBJ_DATA
        sb.AppendLine("Filename:  " + FileName);
        sb.AppendLine("Armor:  " + Armor);
        sb.AppendLine("DeltaY:  " + DeltaY);
        sb.AppendLine("Subobjs:  " + SubobjDatas.Count);
        sb.AppendLine("Flags: " + DescribeFlags());

        foreach (SUBOBJ_DATA _DATA in SubobjDatas)
        {
            sb.AppendLine("\tSubobjData:  " + _DATA);
        }
        sb.AppendLine("UnitDataIndex:  " + UnitDataIndex);

        //from STORM_DATA
        //sb.Append(base.ToString());


        return sb.ToString();
    }

    public static SUBOBJ_DATA GetByName(string Name, bool MustExist = true)
    {
        //return Datas.GetItem(Name, 0, MustExist);
        uint Code = Constants.THANDLE_INVALID;
        if (Name != "" && Name != null) Code = Hasher.HshString(Name);
        return GetByCode(Code, MustExist);
    }

    public static SUBOBJ_DATA GetByCode(DWORD Code, bool MustExist = true)
    {
        foreach (SUBOBJ_DATA data in Datas)
        {
            if (data.Name == Code) return data;
        }

        if (MustExist) throw new System.Exception($"Could not get SUBOBJ_DATA {Code:X8}");
        return null;
    }

    public static SUBOBJ_DATA InsertSubobjData(SUBOBJ_DATA d, int l)
    {
        return Datas.InsertItem(d, l);
    }

    public static void loadSubobjData(int code, IMappedDb db)
    {
        switch (code)
        {
            case 0:
                WPN_DATA.loadWeaponData(db);
                TURRET_DATA.loadTurretData(db);
                RADAR_DATA.loadRadarData(db);
                return;
            case 1:
                Datas.MakeLinks();
                return;
        }

    }

    public SUBOBJ_DATA GetByCodeLocal(uint Code, bool MustExist = true)
    {
        return GetByCode(Code, MustExist);
    }

    public string GetClassName()
    {
        Dictionary<DWORD, string> classes = new Dictionary<DWORD, string> {
            [SC_SUBOBJ] = "SC_SUBOBJ",
            [SC_TURRET] = "SC_TURRET",
            [SC_CRAFT_PART] = "SC_CRAFT_PART",
            [SC_WEAPON_SLOT] = "SC_WEAPON_SLOT",
            [SC_DEBRIS_PART] = "SC_DEBRIS_PART",
            [SC_RADAR] = "SC_RADAR",
            [SC_FOOT] = "SC_FOOT",
            [SC_DETACHED] = "SC_DETACHED",
            [SC_HANGAR] = "SC_HANGAR",
            [SC_SFG] = "SC_SFG"
        };
        
        foreach (var kvp in classes)
        {
            if (GetClass() == kvp.Key) return kvp.Value;
        }
        return "NOT FOUND";
    }
}



