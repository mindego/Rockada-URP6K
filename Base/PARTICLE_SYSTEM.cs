using geombase;
using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.LightTransport;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UIElements;
using static ParticlesDefines;
using static renderer_dll;
using static RoFlags;
using static UnityEngine.ParticleSystem;
using crc32 = System.UInt32;
using DWORD = System.UInt32;

public class PARTICLE_SYSTEM : RO
{
    const crc32 PartileName = 0x50AD0248;
    public override string ToString()
    {
        string res = GetType().ToString();
        res += "\n" + GetFlags().ToString("X8");
        return res;
    }

    public delegate void BirthFuncDelegate(MATRIX Basis);
    public BirthFuncDelegate BirthFunc;
    public PARTICLE_SYSTEM(IParticleData pd, float GlobalSpeed) : base(ROF_TANSP | ROFID_PARTICLE, PartileName)
    {
        mData = IRefMem.addRef(pd);
        Data = pd.GetPARTICLE_DATA();
        mBirthPeriodFactor = 1;
        mBirthFrequenceFactor = 1;
        mDecaySpeedFactor = 1;

        Next = Prev = SubObjects = Parent = null;
        Link = 0;
        Name = 0x50AD0248;


        switch (Data.BirthType)
        {
            case BirthType.PB_NORMAL: BirthFunc = Norm_Rand_Birth; break;
            case BirthType.PB_TOROIDAL: BirthFunc = Toroidal_Birth; break;
            case BirthType.PB_SPHERICAL: BirthFunc = Spherical_Birth; break;
            case BirthType.PB_TORUS: BirthFunc = Torus_Birth; break;
            default:
                Asserts.Assert(false);
                BirthFunc = Spherical_Birth;
                break;
        }

        BirthMoment = 0;
        BirthReminder = 0;
        Reset();
        Activate();

        SelfRadius = 0; //Data.SelfRadius;
        HashRadius = MaxRadius = SelfRadius;

        m_MaxParticleRadius = 0;
        for (int i = 0; i != 256; ++i)
        {
            float r = Data.Size[i];
            m_MaxParticleRadius = r > m_MaxParticleRadius ? r : m_MaxParticleRadius;
        }
    }

    public static void SetLod(float minr) { minr_draw2 = Mathf.Pow(minr_draw = Mathf.Abs(minr), 2); }
    public static float Lod() { return minr_draw; }
    public static void SetMax(float size) { maxsize = size; }
    public static float Max() { return maxsize; }

    public A_PARTICLE[] Par = Alloca.ANewN<A_PARTICLE>(256);
    void Norm_Rand_Birth(MATRIX Basis)
    {
        Younger++;

        if (Younger == Data.MaxParts) Younger = 0;

        if (Data.IsLocal != 0)
        {
            Par[Younger].Speed.Set(
                Distr.Gauss() * Data.SpeedRadius.x + Data.SpeedBase.x,
                Distr.Gauss() * Data.SpeedRadius.y + Data.SpeedBase.y,
                Distr.Gauss() * Data.SpeedRadius.z + Data.SpeedBase.z);
            Par[Younger].Org.Set(
                Distr.Gauss() * Data.BirthRadius.x + Data.BirthBase.x,
                Distr.Gauss() * Data.BirthRadius.y + Data.BirthBase.y,
                Distr.Gauss() * Data.BirthRadius.z + Data.BirthBase.z);
        }
        else
        {
            Par[Younger].Speed =
                Basis.Right * (Distr.Gauss() * Data.SpeedRadius.x + Data.SpeedBase.x) +
                Basis.Up * (Distr.Gauss() * Data.SpeedRadius.y + Data.SpeedBase.y) +
                Basis.Dir * (Distr.Gauss() * Data.SpeedRadius.z + Data.SpeedBase.z);
            Par[Younger].Org =
                Basis.Right * (Distr.Gauss() * Data.BirthRadius.x + Data.BirthBase.x) +
                Basis.Up * (Distr.Gauss() * Data.BirthRadius.y + Data.BirthBase.y) +
                Basis.Dir * (Distr.Gauss() * Data.BirthRadius.z + Data.BirthBase.z) + PosOffset;
            if (Data.LocalBorn != 0) Par[Younger].Speed += EmSpeed;
        }

        Par[Younger].Org += Par[Younger].Speed * BirthMoment;
        Par[Younger].LivedFor = BirthMoment * GetDecaySpeed();
    }

    void Toroidal_Birth(MATRIX Basis)
    {
        Younger++;

        if (Younger == Data.MaxParts) Younger = 0;

        Vector3 c = Distr.Sphere();
        if (Data.IsLocal != 0)
        {
            Par[Younger].Speed.Set(
                c.x * Data.SpeedRadius.x + Data.SpeedBase.x + Distr.Gauss() * Data.SpeedRadius.z,
                c.y * Data.SpeedRadius.y + Data.SpeedBase.y + Distr.Gauss() * Data.SpeedRadius.z,
                                       Data.SpeedBase.z + Distr.Gauss() * Data.SpeedRadius.z);
            Par[Younger].Org.Set(
                c.x * Data.BirthRadius.x + Data.BirthBase.x + Distr.Gauss() * Data.BirthRadius.z,
                c.y * Data.BirthRadius.y + Data.BirthBase.y + Distr.Gauss() * Data.BirthRadius.z,
                                       Data.BirthBase.z + Distr.Gauss() * Data.BirthRadius.z);
        }
        else
        {
            Par[Younger].Speed =
                Basis.Right * (c.x * Data.SpeedRadius.x + Data.SpeedBase.x + Distr.Gauss() * Data.SpeedRadius.z) +
                Basis.Up * (c.y * Data.SpeedRadius.y + Data.SpeedBase.y + Distr.Gauss() * Data.SpeedRadius.z) +
                Basis.Dir * (Data.SpeedBase.z + Distr.Gauss() * Data.SpeedRadius.z);
            if (Data.LocalBorn != 0) Par[Younger].Speed += EmSpeed;
            Par[Younger].Org =
                Basis.Right * (c.x * Data.BirthRadius.x + Data.BirthBase.x + Distr.Gauss() * Data.BirthRadius.z) +
                Basis.Up * (c.y * Data.BirthRadius.y + Data.BirthBase.y + Distr.Gauss() * Data.BirthRadius.z) +
                Basis.Dir * (Data.BirthBase.z + Distr.Gauss() * Data.BirthRadius.z);
            Par[Younger].Org += PosOffset;
        }
        Par[Younger].Org += Par[Younger].Speed * BirthMoment;
        Par[Younger].LivedFor = BirthMoment * GetDecaySpeed();
    }

    private Vector3 VectorAnd(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
    }
    void Spherical_Birth(MATRIX Basis)
    {
        Younger++;

        if (Younger == Data.MaxParts) Younger = 0;

        Vector3 c = Distr.Sphere();
        //Vector3 pos = (c & Data.BirthRadius) + Data.BirthBase;
        //Vector3 spd = (c & Data.SpeedRadius) + Data.SpeedBase;
        Vector3 pos = VectorAnd(c, Data.BirthRadius) + Data.BirthBase;
        Vector3 spd = VectorAnd(c, Data.SpeedRadius) + Data.SpeedBase;

        if (Data.IsLocal != 0)
        {
            Par[Younger].Org = pos;
            Par[Younger].Speed = spd;
        }
        else
        {
            Par[Younger].Org = Basis.ProjectVector(pos) + PosOffset;
            Par[Younger].Speed = Basis.ProjectVector(spd);
            if (Data.LocalBorn != 0)
                Par[Younger].Speed += EmSpeed;
        }
        Par[Younger].Org += Par[Younger].Speed * BirthMoment;
        Par[Younger].LivedFor = BirthMoment * GetDecaySpeed();
    }

    void Torus_Birth(MATRIX Basis)
    {
        Younger++;

        if (Younger == Data.MaxParts) Younger = 0;

        Vector2 sc1 = Distr.Circle();
        Vector2 sc2 = Distr.Circle();

        double y = Data.BirthRadius.x + Data.BirthRadius.y * sc2.x;

        Vector3 pos = new Vector3((float)(sc1.x * y), (float)(sc1.y * y), Data.BirthRadius.z * sc2.y);

        float minR = Data.BirthRadius.x - Data.BirthRadius.y;
        float maxR = Data.BirthRadius.x + Data.BirthRadius.y;

        float coeff = interpolateFloat(minR, Data.SpeedRadius.x, maxR, Data.SpeedRadius.y, Mathf.Sqrt(pos.x * pos.x + pos.y * pos.y));

        Vector3 spd = new Vector3(pos.x, pos.y, pos.z * Data.SpeedRadius.z);

        spd *= coeff / spd.magnitude;

        if (Data.IsLocal != 0)
        {
            Par[Younger].Org = pos + Data.BirthBase;
            Par[Younger].Speed = spd + Data.SpeedBase;
        }
        else
        {
            Par[Younger].Org = Basis.ProjectVector(pos + Data.BirthBase) + PosOffset;
            Par[Younger].Speed = Basis.ProjectVector(spd + Data.SpeedBase);
            if (Data.LocalBorn != 0)
                Par[Younger].Speed += EmSpeed;
        }


        Par[Younger].Org += Par[Younger].Speed * BirthMoment;
        Par[Younger].LivedFor = BirthMoment * GetDecaySpeed();
    }

    private float interpolateFloat(float x0, float y0, float x1, float y1, float x)
    {
        return ((x0 - x) * y1 + (x - x1) * y0) / (x0 - x1);
    }

    float GetBirthFrequence() { return Data.BirthFrequence * mBirthFrequenceFactor; }
    float GetBirthPeriod() { return Data.BirthPeriod * mBirthPeriodFactor; }
    float GetDecaySpeed() { return Data.DecaySpeed * mDecaySpeedFactor; }

    public override void Destroy()
    {
        mData.Release();
    }
    private bool isDisposed = false;
    public override void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;
        Destroy();
    }

    internal void Die()
    {
        Dying = true; Active = false;
    }


    internal bool Living()
    {
        return Alive;
    }
    public bool ToDie()
    {
        return Dying;
    }

    internal void Update(float scale, Vector3 emspeed)
    {
        if (!Alive) return;
        Scale = scale;
        Basis = new MATRIX(this);
        if (Data.IsLocal != 0)
            EmSpeed.Set(0, 0, 0);
        else
        {
            EmSpeed = emspeed;
            if (Parent != null)
                Parent.MatrixToWorld(ref Basis);
        }
        Vector3 DPO;
        JustDied = 0;
        JustBorn = 0;

        if (Active)
        {
            float r = (Scale + BirthReminder);
            if (r > GetBirthPeriod())
            {
                BirthMoment = r - GetBirthPeriod();
                PosOffset = EmSpeed * (-BirthMoment);
                DPO = (EmSpeed) * GetBirthPeriod();
                if (Data.IsLocal == 0) PosOffset += Basis.Org;

                JustBorn = Mathf.FloorToInt(r * GetBirthFrequence());
                if (JustBorn > Data.MaxParts)
                {
                    BirthMoment += (Data.MaxParts - JustBorn) * GetBirthPeriod();
                    PosOffset += (DPO * (Data.MaxParts - JustBorn));
                    JustBorn = Data.MaxParts;
                }
                LifeCicle_Birth();                             //
                for (int i = JustBorn; i >= 0; i--)
                {                     //
                    BirthFunc(Basis);                             //
                    PosOffset += DPO;                                //
                    BirthMoment -= GetBirthPeriod();
                }
                BirthReminder = GetBirthPeriod() + BirthMoment;
            }
            else BirthReminder = r;
        }
        RecalcFunc();
        Alive = (!Dying) || SomeAlive() != 0;
    }

    int SomeAlive() { return Num; }

    public void LifeCicle_Birth()
    {
        if (JustBorn + Num > Data.MaxParts)
        {
            JustDied = JustBorn + Num - Data.MaxParts;
            Num = Data.MaxParts;
        }
        else Num += JustBorn;
    }

    void RecalcFunc()
    {
        for (int i = 0, j = Younger - JustBorn; i < Num - JustBorn; i++, j--)
        {
            if (j < 0) j += Data.MaxParts;
            A_PARTICLE Pa = Par[j];
            Pa.LivedFor += Scale * GetDecaySpeed();

            if (Pa.LivedFor > 255)
            {
                Num = i;
                break;
            }

            Pa.Org += Pa.Speed * Scale + Data.Gravity * (Scale * Scale * .5f);
            Pa.Speed += Data.Gravity * Scale;

            if (Data.Friction != 0)
            {
                Pa.Speed *= 1.0f - Mathf.Min(1.0f, Pa.Speed.magnitude * Data.Friction * Scale);
            }
            Par[j] = Pa;
        }

        if (Num > 0)
        {
            int currentIndex = Younger;
            A_PARTICLE Pa = Par[Younger];
            int MaxP = Data.MaxParts - 1;

            bool is_local = Data.IsLocal != 0;
            float MaxR = 0;
            for (int j = Num; j != 0; --j)
            {
                float r = (is_local ? Pa.Org : Pa.Org - Basis.Org).magnitude + m_MaxParticleRadius;
                MaxR = r > MaxR ? r : MaxR;
                //if (Pa == Par) Pa = Par + MaxP; else Pa--;
                //if (Younger == 0) Pa = Par[MaxP]; else Pa = Par[Younger - 1];
                currentIndex = currentIndex == 0 ? MaxP : --currentIndex;
                Pa = Par[currentIndex];
            }

            SelfRadius = MaxRadius = MaxR;
            //    Assert( SelfRadius<10000 ); 
        }
    }
    public void Activate() { Active = true; Dying = false; }
    public void ReActivate() { Active = true; }
    public void DeActivate() { Active = false; }
    public void Reset() { Younger = 0; Num = 0; Dying = false; Active = false; Alive = true; BirthReminder = 0; }

    MATRIX Basis;
    float Scale;

    public IParticleData mData;
    public PARTICLE_DATA Data;
    float m_MaxParticleRadius;//max size of a single particle

    public int Num;
    public int Younger;
    public int JustBorn;
    public int JustDied;
    float BirthReminder;
    float BirthMoment;
    bool Active;
    bool Alive;
    bool Dying;

    float mBirthPeriodFactor;
    float mBirthFrequenceFactor;
    float mDecaySpeedFactor;

    public static float minr_draw, minr_draw2, maxsize;

    Vector3 PosOffset;
    Vector3 EmSpeed;

    public bool isLocal() { return Data.IsLocal != 0; }

    //void PARTICLE_SYSTEM::EDrawMainFull(LIGHT_DATA* DynamicLights, int nDynamicLights,const CAMERA &Cam)
    internal void EDrawMainFull(object v1, int nDynamicLights, CAMERA Cam)
    {
        Asserts.AssertBp(GetFlag(ROFID_PARTICLE | ROF_TANSP));

        if (Num != 0)
            switch (Data.DrawType)
            {
                case DrawType.PD_ADDA: Texture_Add_Draw(Data.IsLocal != 0 ? Cam : Engine.EngineCamera); break;
                case DrawType.PD_ATEXTURE: Alpha_Texture_Blend_Draw(Data.IsLocal != 0 ? Cam : Engine.EngineCamera); break;
                case DrawType.PD_ATEXTUREINV: Alpha_Texture_Inv_Draw(Data.IsLocal != 0 ? Cam : Engine.EngineCamera); break;
                case DrawType.PD_VECTOR: Vector_Draw(Data.IsLocal != 0 ? Cam : Engine.EngineCamera); break;
            }
    }

    void Texture_Add_Draw(CAMERA Lcam, bool clip = false)
    {
        A_PARTICLE Pa = Par[Younger];
        Debug.LogFormat("Drawing PS mode Texture_Add_Draw, local: {0} example part org: {1}, PS Org: {2}" ,(Data.IsLocal != 0),Pa.Org,Org);
        d3d.SetTexture(mData.GetTexture());
        int fog = 0;
        //DrawQuadPoints(Lcam, new GetColor_Add_Draw(fog), clip);
        DrawParticles(new GetColor_Add_Draw(fog), clip);
    }

    void DrawQuadPoints(CAMERA Lcam, GetColor get_color, bool clip)
    {

        DrawQuadPointsCustom(Lcam, null, get_color);
    }

    void DrawParticles(GetColor get_color, bool clip)
    {
        StormUnityRenderer.DrawParticleSystem(this, get_color, clip);
    }
    void DrawQuadPointsCustom(CAMERA Lcam, object draw_point, GetColor get_color)
    {
        int currentIndex = Younger;
        A_PARTICLE Pa = Par[Younger];
        int MaxP = Data.MaxParts - 1;

        //passes points  loded
        //ParticleLod p_lod(minr_draw, minr_draw2);

        //draws particles
        //AssertBp(sizeof(TLVertex) == sizeof(D3DTLVERTEX));

        int total_draw = 0;

        for (int j = Num; j != 0; --j)
        {
            int SuzFlag;
            //if (!(SuzFlag = Lcam.ProjectPoint(Pa->Org, Pt)))
            //{//!=BScreen
            int idx = (int)Mathf.Clamp(Pa.LivedFor, 0, 255);

            //float size = CAMERA::Aspect * Pt.ood * Data.Size[idx];
            float size = Data.Size[idx];
            if (size > maxsize) size = maxsize;

            float drx, dry;
            //loding point
            //if (p_lod.ProcessPoint(Pt.x, Pt.y, size, drx, dry, size))
            //{
            total_draw++;
            DWORD C = get_color.GetColor(Data.Color[idx]);

            //draw_point(size, Vector2f(drx, dry), Pt.zb, Pt.ood, C);
            //}
            //}
            //if (Pa == Par[0])
            currentIndex = currentIndex == 0 ? MaxP : --currentIndex;
            Pa = Par[currentIndex];
        }
    }
    struct GetColor_Add_Draw : GetColor
    {
        int m_Fog;

        public GetColor_Add_Draw(int fog)
        {
            m_Fog = fog;
        }

        public DWORD GetColor(DWORD color)
        {
            return (color & 0xFFFFFF) + (DWORD)m_Fog;
        }
    }
    public interface GetColor
    {
        public DWORD GetColor(DWORD color);
    }
    void Alpha_Texture_Blend_Draw(CAMERA Lcam, bool clip = false)
    {
        //Debug.Log("Drawing PS mode Alpha_Texture_Blend_Draw, local: " + (Data.IsLocal != 0));
    }
    void Alpha_Texture_Inv_Draw(CAMERA Lcam, bool clip = false)
    {
        Debug.Log("Drawing PS mode Alpha_Texture_Inv_Draw, local: " + (Data.IsLocal != 0));
    }
    void Vector_Draw(CAMERA Lcam, bool clip = false)
    {
        Debug.Log("Drawing PS mode Vector_Draw, local: " + (Data.IsLocal != 0));
    }
}


public class A_PARTICLE
{
    public Vector3 Org;
    public Vector3 Speed;
    public float LivedFor;

    public override string ToString()
    {
        return string.Format("A_PARTICLE {3} Org:{0} Speed:{1} LivedFor:{2}", Org, Speed, LivedFor, this.GetHashCode().ToString("X8"));
    }
};