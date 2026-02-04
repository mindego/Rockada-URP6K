using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DWORD = System.UInt32;

public class EXPLOSION_DATA : STORM_DATA, TLIST_ELEM<EXPLOSION_DATA>, IDisposable
{
    public static StormDataHolder<EXPLOSION_DATA> Datas = new StormDataHolder<EXPLOSION_DATA>("explosion");

    private TLIST_ELEM_IMP<EXPLOSION_DATA> TLIST_IMP = new TLIST_ELEM_IMP<EXPLOSION_DATA>();


    //  // internal part
    //  EXPLOSION_DATA(const char*,float);
    //virtual void ProcessToken(READ_TEXT_FILE&,const char*);
    //  virtual void Reference(const EXPLOSION_DATA* ref);
    //virtual void MakeLinks();
    //  // data section
    //  float LifeTime, Timer;
    //  int Vertical;
    //  VECTOR Delta;
    //  float SpeedCoeff;
    //  float Probability;
    //  bool LoopedSound;
    //  float VDist2;
    //  unsigned int Particle;
    //  VisLightData LightData1;
    //  float LightD;
    //  VisDecalData DecalData1;
    //  float myFlareProbability;
    //  Bool<true> myHashed;

    //  TLIST<DEBRIS_SET> DebrisSetsList;
    //  TLIST<EXPLOSION_DATA> ExplChain;
    //  static const int FlareLen = 256;
    //  char Flare[FlareLen];
    //  // data access
    //  static STORM_DATA_API EXPLOSION_DATA* __cdecl GetByName(const char* Name,bool MustExist = true);
    //  static STORM_DATA_API EXPLOSION_DATA* __cdecl GetByCode(unsigned int Code, bool MustExist = true);
    //  static STORM_DATA_API EXPLOSION_DATA* __cdecl GetFirstItem();
    //  static STORM_DATA_API int __cdecl nItems();

    // internal part

    public override string ToString()
    {
        string res = base.ToString();
        res += "\nVDist2 " + VDist2;
        res += "\nFlare " + Flare;
        res += "\nParticle " + Particle.ToString("X8");
        res += "\nLifeTime: " + LifeTime;
        res += "\nTimer: " + Timer;
        return res;
    }
    // data section
    public float LifeTime, Timer;
    public bool Vertical;
    public Vector3 Delta;
    public float SpeedCoeff;
    public float Probability;
    public bool LoopedSound;
    public float VDist2;
    public uint Particle;
    public VisLightData LightData1;
    public float LightD;
    public VisDecalData DecalData1;
    public float myFlareProbability;
    public bool myHashed = true;

    public List<DEBRIS_SET> DebrisSetsList;
    //public List<EXPLOSION_DATA> ExplChain;
    public TLIST<EXPLOSION_DATA> ExplChain;
    const int FlareLen = 256;
    //char Flare[FlareLen];
    public string Flare;

    //public EXPLOSION_DATA() : this("undefined",0) { }
    public EXPLOSION_DATA(string name, float t) : base(name)
    {
        LifeTime = 5;
        Vertical = false;
        Delta.Set(0, 0, 0);
        Particle = 0;
        LightData1.mRadius = 0;
        LightD = 0;
        Timer = t;
        Probability = 1f;
        VDist2 = 500;
        SpeedCoeff = 0f;
        LoopedSound = false;
        DecalData1.Size.Set(10, 10, 10);
        DecalData1.tc_model = 0;
        DecalData1.draw_script = 0xFFFFFFFF;
        DecalData1.myColor = new Vector3(1, 1, 1);
        DecalData1.myTransparencyCoeffs = new Vector3(0, 0, 1);
        //Flare = "";
        Flare = null;
        myFlareProbability = 1f;

        DebrisSetsList = new List<DEBRIS_SET>();
        //ExplChain = new List<EXPLOSION_DATA>();
        ExplChain = new TLIST<EXPLOSION_DATA>();
    }

    public override void ProcessToken(READ_TEXT_STREAM st, string c)
    {
        //#define ADDEXPL(v) ExplChain.AddToTail(new EXPLOSION_DATA(c,v))->Load(f)
        do
        {
            if (st.LoadFloat(ref LifeTime, "LifeTime")) continue;
            if (st.LoadBool(ref myHashed, "Hashed")) continue;
            if (st.LoadFloat(ref Delta.x, "DeltaX")) continue;
            if (st.LoadFloat(ref Delta.y, "DeltaY")) continue;
            if (st.LoadFloat(ref Delta.z, "DeltaZ")) continue;
            if (st.LoadFloat(ref Probability, "Probability")) continue;
            if (st.LoadFloat(ref SpeedCoeff, "SpeedCoeff")) continue;
            if (st.LoadFloat(ref LightData1.mRadius, "LightRadius")) continue;
            if (st.LoadFloat(ref VDist2, "VDist")) continue;
            if (st.LoadFloat(ref myFlareProbability, "FlareProbability")) continue;
            if (st.Recognize("Vertical")) { Vertical = true; continue; }
            if (st.Recognize("LoopedSound")) { LoopedSound = true; continue; }
            if (st.LdHS(ref Particle, "Particle")) continue;
            if (st.Recognize("WreckSet")) { DebrisSetsList.Add(ReadDebrisSet(st)); continue; }
            // цепочки взрывов
            if (st.Recognize("ExplOnStart"))
            {
                c = st.GetNextItem();
                EXPLOSION_DATA ed = new EXPLOSION_DATA(c, 0);
                ed.Load(st);
                //ADDEXPL(0); 
                //ExplChain.Add(ed);
                ExplChain.AddToTail(ed);
                continue;
            }
            if (st.Recognize("ExplOnEnd"))
            {
                c = st.GetNextItem();
                EXPLOSION_DATA ed = new EXPLOSION_DATA(c, LifeTime);
                ed.Load(st);
                //ExplChain.Add(ed);
                ExplChain.AddToTail(ed);
                continue;
            }
            if (st.Recognize("ExplOnTime"))
            {
                c = st.GetNextItem();
                EXPLOSION_DATA ed = new EXPLOSION_DATA(c, st.AtoF(st.GetNextItem()));
                ed.Load(st);
                continue;
            }
            // свет
            if (st.LoadFloat(ref LightD, "LightD")) continue;
            if (st.Recognize("LightColor"))
            {
                LightData1.mColor = st.recognizeVector3f(st) * Storm.Math.OO256;
                LightData1.mIntensity = 1;
                continue;
            }
            // декаль
            if (st.LoadFloat(ref DecalData1.life_time, "DecalTime")) continue;
            if (st.LdHS(ref DecalData1.draw_script, "DecalScript")) continue;
            if (st.LoadInt(ref DecalData1.tc_model, "DecalModel")) continue;
            if (st.Recognize("DecalCoeffs"))
            {
                DecalData1.myTransparencyCoeffs = st.recognizeVector3f(st);
                DecalData1.myTransparencyCoeffs = calcTransparencyCoeffs(DecalData1.myTransparencyCoeffs);
                continue;
            }
            if (st.Recognize("DecalColor"))
            {
                DecalData1.myColor = st.recognizeVector3f(st) * Storm.Math.OO256;
                // by Tengiz order
                for (int i = 0; i < 3; i++)
                    DecalData1.myColor[i] = 1 - DecalData1.myColor[i];
                continue;
            }
            if (st.Recognize("DecalSize"))
            {
                float tmp = st.AtoF(st.GetNextItem());
                DecalData1.Size.Set(tmp, tmp, tmp);
                continue;
            }
            if (st.LoadFloat(ref DecalData1.Size.z, "DecalHelper")) continue;
            if (st.Recognize("Reference"))
            {
                Reference(GetByName(st.GetNextItem()));
                return;
            }
            if (st.Recognize("Flare"))
            {
                string str = st.GetNextItem();
                //Debug.Log("Flare name " + str + " for " + FullName);
                if (str.Length < FlareLen)
                    Flare = str;
                return;
            }

            base.ProcessToken(st, c);
        } while (false);
    }

    Vector3 calcTransparencyCoeffs(Vector3 v)
    {
        Vector3 res = Vector3.zero;
        res.x = v.x;
        res.z = -4f * (v.y - 0.5f * res.x - 0.5f * v.z);
        res.y = v.z - res.x - res.z;
        return res;
    }

    static DEBRIS_SET ReadDebrisSet(READ_TEXT_STREAM st)
    {
        if (st.GetNextItem() != "{") stormdata_dll.ParseError("ReadDebrisSet", "{");
        // создаем и инитим новый итем
        DEBRIS_SET d = new DEBRIS_SET();
        // читаем данные до "}"
        while (true)
        {
            string c = st.GetNextItem();
            if (c == "}") break;
            if (st.LoadFloat(ref d.MinSpeed, "MinSpeed")) continue;
            if (st.LoadFloat(ref d.MaxSpeed, "MaxSpeed")) continue;
            if (st.LoadFloat(ref d.Coeff.x, "CoeffX")) continue;
            if (st.LoadFloat(ref d.Coeff.y, "CoeffY")) continue;
            if (st.LoadFloat(ref d.Coeff.z, "CoeffZ")) continue;
            if (st.Recognize("MinSpeedX")) { st.GetNextItem(); continue; }
            if (st.Recognize("MinSpeedY")) { st.GetNextItem(); continue; }
            if (st.Recognize("MinSpeedZ")) { st.GetNextItem(); continue; }
            if (st.Recognize("MaxSpeedX")) { st.GetNextItem(); continue; }
            if (st.Recognize("MaxSpeedY")) { st.GetNextItem(); continue; }
            if (st.Recognize("MaxSpeedZ")) { st.GetNextItem(); continue; }
            if (st.Recognize("Coeff"))
            {
                d.Coeff.x = st.AtoF(st.GetNextItem());
                d.Coeff.y = st.AtoF(st.GetNextItem());
                d.Coeff.z = st.AtoF(st.GetNextItem());
                continue;
            }
            if (st.Recognize("Debris"))
            {
                d.Debrises.Add(Hasher.HshString(st.GetNextItem()));
                continue;
            }
            stormdata_dll.ParseError("Error parsing explosion set", "final");
        }
        if (Storm.Math.NormaFAbs(d.Coeff) == 0)
        {
            //delete d;
            stormdata_dll.ParseError("Error parsing explosion set", "Coeff");
        }
        return d;
    }

    public void Reference(EXPLOSION_DATA refdata)
    {
        myHashed = refdata.myHashed;
        LifeTime = refdata.LifeTime;
        Timer = refdata.Timer;
        Vertical = refdata.Vertical;
        Delta = refdata.Delta;
        VDist2 = refdata.VDist2;
        Particle = refdata.Particle;
        SpeedCoeff = refdata.SpeedCoeff;
        LightData1 = refdata.LightData1;
        LightD = refdata.LightD;
        DecalData1 = refdata.DecalData1;
        myFlareProbability = refdata.myFlareProbability;
        // DebrisSetsList
        foreach (DEBRIS_SET ds in refdata.DebrisSetsList)
        {
            DebrisSetsList.Add(ds);
        }
        // ExplChain
        //foreach (EXPLOSION_DATA ed in refdata.ExplChain)
        //{
        //    EXPLOSION_DATA ned = new EXPLOSION_DATA(ed.FullName, ed.Timer);
        //    ned.Reference(ed);
        //    ExplChain.Add(ned);
        //}
        for (EXPLOSION_DATA ed = refdata.ExplChain.Head(); ed != null; ed = ed.Next())
        {
            EXPLOSION_DATA ned = new EXPLOSION_DATA(ed.FullName, ed.Timer);
            ned.Reference(ed);
            ExplChain.AddToTail(ned);
        }
        Flare = refdata.Flare;
    }

    public static EXPLOSION_DATA GetByName(string Name, bool MustExist = true)
    {
        //return Datas.GetItem(Name, 0, MustExist);
        uint Code = Constants.THANDLE_INVALID;
        if (Name != "") Code = Hasher.HshString(Name);
        foreach (EXPLOSION_DATA data in Datas)
        {
            if (data.Name == Code) return data;
        }
        if (MustExist) throw new System.Exception($"Could not get EXPLOSION_DATA {Name}");
        return null;
    }

    public static void insertExplData(READ_TEXT_STREAM st)
    {
        EXPLOSION_DATA eData = new EXPLOSION_DATA(st.GetNextItem(), 0);
        StormLog.LogMessage("Loading explosion " + eData.FullName + " [OK]", StormLog.logPriority.DEBUG);

        Datas.InsertItem(eData, st.LineNumber()).Load(st);
    }

    //public static void loadExplData(int code, PackType db)
    //{
    //    switch (code)
    //    {
    //        case 0:
    //            LoadUtils.parseData(db, Datas.GetItemName(), "Explosions", "Explosions.txt", "[STORM EXPLOSIONS DATA FILE 2.1]", "Expl", insertExplData);
    //            return;
    //        case 1:
    //            Datas.MakeLinks();
    //            return;
    //    }
    //}
    public static void loadExplData(int code, IMappedDb db)
    {
        switch (code)
        {
            case 0:
                LoadUtils.parseData(db, Datas.GetItemName(), "Explosions", "Explosions.txt", "[STORM EXPLOSIONS DATA FILE 2.1]", "Expl", insertExplData);
                return;
            case 1:
                Datas.MakeLinks();
                return;
        }
    }

    public override void MakeLinks()
    {
        VDist2 = Mathf.Pow(VDist2, 2);
        //TODO: Доделать линкование взрывов
        //for (DEBRIS_SET* s = DebrisSetsList.Head(); s; s = s->Next())
        //{
        //    for (LIST_ELEM* le = s->Debrises.Head(); le; le = le->Next())
        //        RSLVLE(le, DEBRIS_DATA);
        //}
        //for (EXPLOSION_DATA* e = ExplChain.Head(); e; e = e->Next())
        //    e->MakeLinks();
    }

    // data access
    public static EXPLOSION_DATA GetByCode(uint Code, bool MustExist=true)
    {
        return Datas.GetItem(null, Code, MustExist);
    }

    public EXPLOSION_DATA Next()
    {
        return ((TLIST_ELEM<EXPLOSION_DATA>)TLIST_IMP).Next();
    }

    public EXPLOSION_DATA Prev()
    {
        return ((TLIST_ELEM<EXPLOSION_DATA>)TLIST_IMP).Prev();
    }

    public void SetNext(EXPLOSION_DATA t)
    {
        ((TLIST_ELEM<EXPLOSION_DATA>)TLIST_IMP).SetNext(t);
    }

    public void SetPrev(EXPLOSION_DATA t)
    {
        ((TLIST_ELEM<EXPLOSION_DATA>)TLIST_IMP).SetPrev(t);
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

public struct VisDecalData
{
    public Vector3 Size;
    public float life_time;
    public int tc_model;
    public DWORD draw_script;
    public Vector3 myTransparencyCoeffs;
    public Vector3 myColor;
};

public struct VisLightData
{
    public float mRadius;
    public Vector3 mColor;
    public float mIntensity;
};


