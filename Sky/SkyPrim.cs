using UnityEngine;
using DWORD = System.UInt32;
using crc32value = System.UInt32;
using crc32 = System.UInt32;
using System.Text;
using System.IO;
using static renderer_dll;
using UnityEngine.Assertions;
using static SkyCfg;
using System.Runtime.InteropServices;
using System;

public class SkyDefines
{
    public const int SF_LAYER0 = 0x00000001;
    public const int SF_LAYER1 = 0x00000002;
    public const int SF_SUN = 0x00000004;
    public const int SF_STARS = 0x00000008;
    public const int SF_PLANET = 0x00000010;
    public const int SF_FLARES = 0x00000020;
    public const int SF_GALAXY = 0x00000040;
}
public class Sky
{
    public const int SKY_FACES = 32;

    public const float SKY_LAYER_Z = 0.0f;
    public const float GALAXY_Z = 0.1f;
    public const float PLANET_Z = 0.5f;

    public const float SKY_Z = 1.00f;

    class FLAR_DESC
    {
        float Intensity;
        float Size;
        float RealSize;
        DWORD Color;
        float[] UV = new float[4];
        public void Set(SunFlarDesc sfd)
        {
            Intensity = sfd.Intensity;
            Size = sfd.Size;
            RealSize = sfd.Size;
            Color = sfd.Color;
            for (int i = 0; i != 4; ++i)
                UV[i] = sfd.UV[i];
        }
        void Resize(float f) { }

    };

    public class SUN
    {
        FLAR_DESC Disk;
        FLAR_DESC Cron1;
        FLAR_DESC Cron2;
        FLAR_DESC CronOverBright;

        float OverBrightKoeff;

        //float       TanX, TanZ;

        string Mask;

        //Texture* texture;
        Texture2D texture;
        //iRS* state;

        SunCfg cur_cfg;
        ObjId cur_cfgid;

        float FlarResizeCoeff;

        float m_Time;
        float FlaresInt;
        bool FlaresActive;
        public float Light;

        public SUN()
        {
            FlarResizeCoeff = d3d.Dx() / 640f;
            Disk = new FLAR_DESC();
            Cron1 = new FLAR_DESC();
            Cron2 = new FLAR_DESC();
            CronOverBright = new FLAR_DESC();
        }
        ~SUN() { }


        public void Initialize(ObjId config)
        {
            SunCfg cfg = dll_data.LoadFile<SunCfg>(config);
            if (cfg == null)
            {
                Log.Message("Error : Invalid Sun Config, applying default...\n");
                cfg = dll_data.LoadFile<SunCfg>("default#sun");
                Assert.IsNotNull(cfg);
            }

            Disk.Set(cfg.Disk);
            Cron1.Set(cfg.Cron1);
            Cron2.Set(cfg.Cron2);
            CronOverBright.Set(cfg.CronOverBright);

            OverBrightKoeff = cfg.OverBrightKoeff;

            //Mask = dll_data.LoadFile<char>(cfg.mask);
            texture = dll_data.LoadTexture(cfg.texture);
            //TanX    = Dir.x/Dir.y;
            //TanZ    = Dir.z/Dir.y;
            //state = dll_data.CreateRS(cfg->rs);

            cur_cfg = cfg;
            cur_cfgid = config;
        }
        //ObjId GetConfig();
        //void ApplyConfig(ObjId );

        //void Done();
        //bool Draw(bool last, const CAMERA&, const LAYER&);

        //void DrawCronePart(const FLAR_DESC&, float, float, float);
        //float Shin(const CAMERA&, const LAYER&);

        float Intencity() { return FlaresInt; }

        bool IsFlaresActive() { return FlaresActive; }

        //  bool Update(float );
        // void Move(bool);

        public bool Update(float scale)
        {
            m_Time += scale;
            return true;
        }

        internal bool Draw(bool v, CAMERA cam, LAYER layer1)
        {
            //throw new System.NotImplementedException();
            return true;
        }

        internal void Move(bool v)
        {
            //throw new System.NotImplementedException();
        }
    }

    public class LAYER
    {
        //int[] Index = new int[SKY_FACES * 9];
        float HC, H1, H2;                 // heights of hemisphere points
        float R1, R2;                     // radius of hemisphere points
        Vector2 Speed;                      // movement speed
        Vector2 Density;                    // mapping density
        Storm.Gradient Light;

        //MPOINT3D Points[SKY_FACES * 2 + 1];
        //FVec2 MapBase[SKY_FACES * 2 + 1];
        //FVec2 MapBias;

        Vector3[] Points = new Vector3[SKY_FACES * 2 + 1];

        Texture2D texture;
        //iRS* state;

        LayerCfg cur_cfg;
        ObjId cur_cfgid;

        public void Initialize(ObjId config)
        {
            //Stream st = GameDataHolder.GetRDataDB().GetStreamById(config);
            Stream st = dll_data.files.GetBlock(config).myStream;
            cur_cfg = StormFileUtils.ReadStruct<LayerCfg>(st);
            cur_cfgid = config;
            st.Close();

            //string textureName = GameDataHolder.GetTexturesDB().GetNameById(cur_cfg.texture);
            //texture = GameDataHolder.GetResource<Texture2D>(PackType.TexturesDB, textureName);
            texture = dll_data.LoadTexture(cur_cfg.texture);
        }
        public void ApplyConfig(ObjId config)
        {
            Initialize(config);
            GameObject dome = new GameObject(texture.name);

            MeshFilter mf = dome.AddComponent<MeshFilter>();
            MeshRenderer mr = dome.AddComponent<MeshRenderer>();

            float squishness = cur_cfg.r1 / cur_cfg.r2;
            //Debug.Log("Squishness: " + config + " = " + squishness);
            mf.mesh = ShapeGenerator.MakeHemisphere(Camera.main.farClipPlane, SKY_FACES, SKY_FACES);
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            Material tmpMaterialTemplate = new Material(shader);
            tmpMaterialTemplate.mainTexture = texture;

            //material.EnableKeyword("_SPECULAR_SETUP");
            //material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            //material.EnableKeyword("_ALPHABLEND_ON");

            //material.SetFloat("_WorkflowMode", 0f);
            //material.SetFloat("_Surface", 1f);
            //material.SetFloat("_DstBlend", 10f); // This makes trasnparent happen
            //material.SetFloat("_Mode", 2f); // This says make it transparent
            //material.SetFloat("_SrcBlend", 5f);
            //material.SetFloat("_ZWrite", 0f);
            //material.SetFloat("_Smoothness", 1f);
            //material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;


            tmpMaterialTemplate.EnableKeyword("_SPECULAR_SETUP");
            tmpMaterialTemplate.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            tmpMaterialTemplate.EnableKeyword("_ALPHABLEND_ON");

            tmpMaterialTemplate.SetFloat("_WorkflowMode", 0f);
            tmpMaterialTemplate.SetFloat("_Surface", 1f);
            tmpMaterialTemplate.SetFloat("_DstBlend", 10f); // This makes trasnparent happen
            tmpMaterialTemplate.SetFloat("_Mode", 2f); // This says make it transparent
            tmpMaterialTemplate.SetFloat("_SrcBlend", 5f);
            tmpMaterialTemplate.SetFloat("_ZWrite", 0f);
            tmpMaterialTemplate.SetFloat("_Smoothness", 1f);
            tmpMaterialTemplate.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            mr.material = tmpMaterialTemplate;
            dome.transform.localScale = new Vector3(1, squishness, 1);
        }

        public void Move(float dt)
        {
            //MapBias += Speed * dt;
        }
    }

    class FLARES
    {
        //Texture* texture;
        //iRS* state;

        FlaresCfg cur_cfg;
        ObjId cur_cfgid;

        float m_Time;

        //void InvalidateConfig();

        public FLARES() { }
        ~FLARES() { }

        public void Initialize(ObjId objid)
        {
            //TODO загружать вспышки возможнно и не нужно.
        }
        //ObjId GetConfig();
        //void ApplyConfig(ObjId );

        //void Draw(const CAMERA&,float);
    };

    struct Galaxy
    {
        //TRef<Texture> texture;
        //TRef<iRS> state;

        GalaxyCfg cfg;
        //MPOINT3D* mesh;
        //short* meshi;

        //Galaxy() : mesh(0), meshi(0) { }
        //~Galaxy() { freeMesh(); }

        //void ApplyConfig(ObjId );
        //void Draw(const CAMERA&, float);
        private int getNumPoints()
        {
            return cfg.nSegments * 4 + 2;
        }

        int getNumTriangles()
        {
            return cfg.nSegments * 4;
        }

        internal void ApplyConfig(uint galaxy)
        {
            //TODO Реализовать загрузку данных отрисовки галактики
        }

        //void freeMesh();
        //void genMesh();
    }
    public class PLANET_8
    {
        static PlanetCfg nullConfig = new PlanetCfg()
        {
            color = new Vector3[4],
            a = 0,
            h = 0,
            beta = 0,
            size = 0,
            texture = CRC32.CRC_NULL,
            rs = CRC32.CRC_NULL
        };



        //    MPOINT3D Points[9];
        //    SVec4 Color[9];
        //    FVec2 Map[9];
        //    static short Index[8 * 3];

        public Texture2D texture;
        //    TRef<iRS> state;
        public PlanetCfg cur_cfg = new PlanetCfg();

        //    void init();

        //    public:
        //PLANET_8();

        //    static void InitIndex();

        //    void Draw(const CAMERA&, float z);
        internal void ApplyConfig(PlanetCfg cfg)
        {
            if (cfg != null)
            {
                if (cur_cfg.texture != cfg.texture)
                    texture = dll_data.LoadTexture(cfg.texture);
                //if (cur_cfg.rs != cfg.rs)
                //    state = dll_data.CreateRS(cfg.rs);
                cur_cfg = cfg;
            }
            else
            {
                texture = null;
                //state = 0;
                cur_cfg = nullConfig;
            }

            init();
        }

        public void init()
        {
            //const PlanetCfg&d = cur_cfg;
            //float tan225 = 0.414214; // tan(Pi()/8);

            //VECTOR Org = Globus(d.a, d.h);
            //VECTOR Left = Org ^ VUp; Left.Normalize(d.size * OO256);
            //VECTOR Up = Left ^ Org; Up.Normalize(d.size * OO256);

            //Color[0] = GetColor((d.color[1] + d.color[2]) * .5f);
            //Color[2] = Color[1] = GetColor(d.color[0]);
            //Color[8] = Color[3] = GetColor(d.color[1]);
            //Color[7] = Color[4] = GetColor(d.color[2]);
            //Color[6] = Color[5] = GetColor(d.color[3]);

            //Map[0] = FVec2(.5, .5) * TEXTURE_SIZE;
            //RadialMappig(Stride(Map + 1, sizeof(Map[0])), 8, GRD2RD(d.beta), Map[0], FVec2(.5, .5) * TEXTURE_SIZE);
            //MPOINT3D* Pd = Points; Pd++->Org = Org;
            //Pd++->Org = Org + Up + Left * tan225; Pd++->Org = Org + Up - Left * tan225;
            //Pd++->Org = Org - Left + Up * tan225; Pd++->Org = Org - Left - Up * tan225;
            //Pd++->Org = Org - Up - Left * tan225; Pd++->Org = Org - Up + Left * tan225;
            //Pd++->Org = Org + Left - Up * tan225; Pd++->Org = Org + Left + Up * tan225;
            //for (int i = 0; i < 9; i++)
            //{
            //    Points[i].Point = CData2D + i;
            //    Points[i].Org *= 1000;
            //}

            PlanetCfg d = cur_cfg;
            Vector3 Org = StormTransform.Globus(d.a, d.h);
            //VECTOR Left = Org ^ VUp; Left.Normalize(d.size * OO256);
            Vector3 Left = Vector3.Cross(Org, Vector3.up); Left = Left.normalized * (d.size * Storm.Math.OO256);
            Vector3 Up = Vector3.Cross(Left, Org); Up = Up.normalized * (d.size * Storm.Math.OO256);
        }

        internal void Draw(CAMERA cam, float PLANET_Z)
        {
            StormUnityRenderer.DrawPlanet(this,cam, PLANET_Z);
        }
    }
    public struct STAR_DESC
    {
        public Vector3 Org;
        public float Size;
        public Color Color;
    };

    class STARS
    {
        STAR_DESC[] pStars;

        Texture2D texture;
        //iRS* state;

        StarsCfg cur_cfg;
        ObjId cur_cfgid;
        //void Init( const StarsCfg &cfg);

        float[] StarUV;
        private float S64(int n)
        {
            return 0.25f * n - 0.25f;
        }
        private float E64(int n)
        {
            return 0.25f * n;
        }
        public STARS()
        {
            StarUV = new float[4] { S64(3), S64(4), E64(3), E64(4) };
        }
        ~STARS() { }

        public void Initialize(ObjId config)
        {
            Stream st = dll_data.files.GetBlock(config).myStream;
            cur_cfg = StormFileUtils.ReadStruct<StarsCfg>(st);
            cur_cfgid = config;
            st.Close();

            texture = dll_data.LoadTexture(cur_cfg.texture);
        }
        // ObjId GetConfig();
        public void ApplyConfig(ObjId config)
        {
            Initialize(config);

            Init(cur_cfg);
        }
        private void Init(StarsCfg cfg)
        {
            pStars = new STAR_DESC[cfg.num];
            //for (int n = cfg.num; n!=0; s++, --n)
            //{
            //    float A = (n & 3 == 0) ? 1 : 0;
            //    s->Org = VECTOR(norm_rand() + A, fabs(norm_rand()), norm_rand() + A);
            //    s->Org.Normalize();
            //    VECTOR VColor = VECTOR(150, 150, 150) + VECTOR(norm_rand(), norm_rand(), norm_rand()) * 12.5;
            //    VColor.Normalize(s->Org.y * 255);
            //    s->Color = SVec4(255, int(VColor.x), int(VColor.y), int(VColor.z));
            //    s->Size = fabs(1 + 0.5 * norm_rand()) * d3d.Dx() / 640;
            //}
            Debug.Log("Generating stars: " + cfg.num);
            STAR_DESC s;
            for (int n = 0; n < cfg.num; n++)
            {
                float A = ((n & 3) == 0) ? 1 : 0;
                s = new STAR_DESC();
                s.Org = new Vector3(Storm.Math.norm_rand() + A, Mathf.Abs(Storm.Math.norm_rand()), Storm.Math.norm_rand() + A);
                s.Org.Normalize();
                Color VColor = new Color(150 / 255, 150 / 255, 150 / 255) + new Color(Storm.Math.norm_rand(), Storm.Math.norm_rand(), Storm.Math.norm_rand()) * 12.5f;
                s.Color = VColor;
                s.Size = Mathf.Abs(1 + 0.5f * Storm.Math.norm_rand()) * 1; // 1= d3d.Dx() / 640, где Dx() - ширина экрана
                //Debug.Log("Creating star " + n + " " + s.Org + " " + RandomGenerator.norm_rand().ToString());
                Debug.Log("Creating star " + n + " " + s.Org + " " + RandomGenerator.Rand().ToString());
                pStars[n] = s;

                //GameObject Star = GameObject.CreatePrimitive(PrimitiveType.Cube);
                GameObject Star = new GameObject();
                Star.transform.position = s.Org;

                SpriteRenderer sr = Star.AddComponent<SpriteRenderer>();
                //Rect spriteRect = new Rect(StarUV[0] * texture.width, StarUV[1] * texture.height, StarUV[2] * texture.width, StarUV[3] * texture.height);
                //Rect spriteRect = new Rect(StarUV[0], StarUV[1], StarUV[2], StarUV[3]);
                Rect spriteRect = new Rect(StarUV[0] * texture.width, StarUV[1] * texture.height, (StarUV[2] - StarUV[0]) * texture.width, (StarUV[3] - StarUV[1]) * texture.height);
                Debug.Log("Using Rect: " + spriteRect);
                Sprite starSprite = Sprite.Create(texture, spriteRect, new Vector2(0.5f, 0.5f));
                sr.sprite = starSprite;
                //BillboardRenderer br = Star.AddComponent<BillboardRenderer>();
                //BillboardAsset ba = new BillboardAsset();
                //ba.name="Star " + n;
                //ba.width = s.Size;
                //ba.height = s.Size;
                //Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
                //Material baMaterial = new Material(shader);
                //baMaterial.mainTexture = texture;
                //ba.material = baMaterial;
                //br.billboard = ba;


                Star.name = "Star " + n + " size " + s.Size;

            }
        }

        // void Draw(const CAMERA&);
    };

    public Sky()
    {
        Sun = new SUN();
        Flares = new FLARES();
        layer1 = new LAYER();
        layer2 = new LAYER();
        Planets = new PLANET_8[10];
        Stars = new STARS();
        galaxy = new Galaxy();

        //rs_evnstart = dll_data.CreateRS("evnstart");

    }

    ~Sky() { }

    public void Initialize(ObjId config)
    {
        //Stream st = GameDataHolder.GetRDataDB().GetStreamByName(config);
        //Stream st = dll_data.files.GetBlock(config).myStream;
        //cur_cfgid = config;
        //cur_cfg = StormFileUtils.ReadStruct<SkyCfg>(st);
        //st.Close();

        SkyCfg cfg = dll_data.LoadFile<SkyCfg>(config);
        if (cfg == null)
        {
            Log.Message("Error : Invalid Sky Config, applying default...\n");
            cfg = dll_data.LoadFile<SkyCfg>("default#sky");
            Assert.IsNotNull(cfg);
        }

        Mode = cfg.flags;

        if ((Mode & SF_SUN) != 0)
            Sun.Initialize(cfg.sun);

        if ((Mode & SF_FLARES) != 0)
            Flares.Initialize(cfg.flares);

        if ((Mode & SF_GALAXY) != 0)
            galaxy.ApplyConfig(cfg.galaxy);

        if ((Mode & SF_LAYER0) != 0)
            layer1.Initialize(cfg.layer0);

        if ((Mode & SF_LAYER1) != 0)
            layer2.Initialize(cfg.layer1);

        if ((Mode & SF_PLANET) != 0)
            loadPlanets(cfg.planets);

        if ((Mode & SF_STARS) != 0)
            Stars.Initialize(cfg.stars);

        cur_cfg = cfg;
        cur_cfgid = config;
    }

    public void loadPlanets(crc32 cfg_name)
    {
        MemBlock bl = dll_data.files.GetBlock(cfg_name);
        int nPl = 0;
        if (bl != null)
        {

            //int numPlanets = Clamp<int>(bl.Size() / sizeof(PlanetCfg), 0, 10);
            int numPlanets = Mathf.Clamp(bl.Size() / PlanetCfg.GetSize(), 0, 10);

            Debug.Log("Planets to draw: " + numPlanets);

            for (; nPl != numPlanets; ++nPl)
            {
                var cfg = bl.Convert<PlanetCfg>(nPl, PlanetCfg.GetSize());
                Planets[nPl] = new PLANET_8();
                Planets[nPl].ApplyConfig(cfg);
            }
        }

        for (; nPl != 10; ++nPl)
        {
            Planets[nPl] = new PLANET_8();
            Planets[nPl].ApplyConfig(null);
        }
    }
    //ObjId GetConfig();
    public void ApplyConfig(ObjId config)
    {
        Initialize(config);

        layer1.ApplyConfig(cur_cfg.layer0);
        layer2.ApplyConfig(cur_cfg.layer1);
        //Stars.ApplyConfig(cur_cfg.stars);
    }

    //void Draw(bool last, const CAMERA&, bool sunvisible);
    //void Move(float);

    protected int Mode;
    protected int numPlanets;

    //void loadPlanets(crc32 cfg_name);

    //void DrawSky(const CAMERA&, bool sunvisible);
    //void DrawFlars(const CAMERA&);

    SUN Sun;
    LAYER layer1, layer2;
    FLARES Flares;
    PLANET_8[] Planets;
    STARS Stars;
    Galaxy galaxy;

    //iRS* rs_evnstart;

    SkyCfg cur_cfg;
    ObjId cur_cfgid;

    public void Move(float scale)
    {
        layer1.Move(scale);
        layer2.Move(scale);
        Sun.Update(scale);

    }

    public void Draw(bool last, CAMERA OldCam, bool sun_visible)
    {
        /*if (last)  DrawFlars(OldCam ) else*/
        DrawSky(OldCam, sun_visible);
    }

    public void DrawSky(CAMERA OldCam, bool sun_visible)
    {
        //TODO Рисовать на небе ВСЁ!
        CAMERA Cam = OldCam; //Cam.ZeroPlanes();

        //if (Mode!=0)
        //    rs_evnstart->Apply();


        //if ((Mode & SF_LAYER0)!=0)
        //    layer1.Draw(Cam, SKY_LAYER_Z);

        //if ((Mode & SF_GALAXY) != 0)
        //    galaxy.Draw(Cam, GALAXY_Z);

        if ((Mode & SF_PLANET) != 0)
            for (int i = 0; i != 10; ++i)
                Planets[i].Draw(Cam, PLANET_Z);

        if ((Mode & SF_SUN) != 0)
        {
            bool visible = Sun.Draw(false, Cam, layer1);
            Sun.Move(sun_visible && visible);
        }

        //if ((Mode & SF_STARS) != 0)
        //    Stars.Draw(Cam);


        //if ((Mode & SF_LAYER1) != 0)
        //    layer2.Draw(Cam, SKY_Z);
    }
};

namespace Storm
{
    public struct Gradient
    {
        // colors [0..255]
        Vector3Int c_max;
        Vector3Int c_min;

        Vector3 v_max;
        Vector3 v_min;
        Vector3 v_div; // = 1/(max-min)

        //void SetVDiv();

        //Vector4s GetColor(const Vector3f & x, const Vector3f & dir, int a);
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\tc_max " + c_max);
            sb.AppendLine("\tc_min " + c_min);

            sb.AppendLine("\tv_max " + v_max);
            sb.AppendLine("\tv_min " + v_min);
            sb.AppendLine("\tv_div " + v_div);

            return sb.ToString();
        }

        internal Vector4 GetColor(Vector3 x, Vector3 dir, int a)
        {
            
            float v = Vector3.Dot(dir, x) * (1 / x.magnitude);
            return new Vector4(
             (v > v_max[0]) ? c_max[0] : (v < v_min[0]) ? c_min[0] : (int)((c_max[0] * (v - v_min[0]) + c_min[0] * (v_max[0] - v)) * v_div[0]),
             (v > v_max[1]) ? c_max[1] : (v < v_min[1]) ? c_min[1] : (int)((c_max[1] * (v - v_min[1]) + c_min[1] * (v_max[1] - v)) * v_div[1]),
             (v > v_max[2]) ? c_max[2] : (v < v_min[2]) ? c_min[2] : (int)((c_max[2] * (v - v_min[2]) + c_min[2] * (v_max[2] - v)) * v_div[2]), a);
        }
    }

}
public struct LayerCfg
{
    public float hc, h1, h2;                 // heights of hemisphere points
    public float r1, r2;                     // radius of hemisphere points
    public Vector2 speed;                      // movement speed
    public Vector2 density;                    // mapping density
    public Storm.Gradient color;
    public crc32value texture;
    crc32value rs;

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("Radii: ");
        sb.AppendLine("\t" + r1 + " " + r2);
        sb.Append("Heights: ");
        sb.AppendLine("\t" + hc + " " + h1 + " " + h2);
        sb.Append(color);
        //sb.AppendLine("Texture " + GameDataHolder.GetTexturesDB().GetNameById(texture));
        //sb.AppendLine("Rs " + GameDataHolder.GetRenderStatesDB().GetNameById(rs));
        return sb.ToString();
    }
}
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class PlanetCfg : IStormImportable<PlanetCfg>
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public Vector3[] color = new Vector3[4];
    public float a, h;
    public float beta;
    public float size;
    public crc32value texture;
    public crc32value rs;

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(this.GetType().ToString());
        for (int i=0;i<color.Length;i++)
        {
            sb.AppendLine("color " + i + ": " + color[i]);
        }
        sb.AppendLine("a: " + a);
        sb.AppendLine("h: " + h);
        sb.AppendLine("beta: " + beta);
        sb.AppendLine("size: " + size);
        sb.AppendLine("texture: " + texture.ToString("X8"));
        sb.AppendLine("rs: " + rs.ToString("X8"));
        return sb.ToString();
    }
    internal static int GetSize()
    {
        return 3 * 4 + (4 + 4) + 4 + 4 + 4 + 4;
    }

    public PlanetCfg Import(Stream st)
    {
        return StormFileUtils.ReadStruct<PlanetCfg>(st);
    }
}
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class SunFlarDesc
{
    public float Intensity;
    public float Size;
    public DWORD Color;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public float[] UV = new float[4];
    public void Resize(float f) { }
};

public class SunCfg : IStormImportable<SunCfg>
{
    public SunFlarDesc Disk;//FLAR_DESC
    public SunFlarDesc Cron1;//FLAR_DESC
    public SunFlarDesc Cron2;//FLAR_DESC
    public SunFlarDesc CronOverBright;//FLAR_DESC
    public float OverBrightKoeff;//FLAR_DESC
    public crc32value mask;
    public crc32value texture;
    public crc32value rs;

    public SunCfg Import(Stream st)
    {
        st.Seek(0, SeekOrigin.Begin);
        Disk = StormFileUtils.ReadStruct<SunFlarDesc>(st);
        Cron1 = StormFileUtils.ReadStruct<SunFlarDesc>(st);
        Cron2 = StormFileUtils.ReadStruct<SunFlarDesc>(st);
        CronOverBright = StormFileUtils.ReadStruct<SunFlarDesc>(st);
        OverBrightKoeff = StormFileUtils.ReadStruct<float>(st);
        mask = StormFileUtils.ReadStruct<crc32value>(st);
        texture = StormFileUtils.ReadStruct<crc32value>(st);
        rs = mask = StormFileUtils.ReadStruct<crc32value>(st);
        return this;

    }
}
class FlarDesc
{
    float Position;
    float Intensity;
    float Size;
    DWORD Color;
    float[] UV = new float[4];
};

class FlaresCfg
{
    crc32value Texture;
    crc32value RState;
    float Size;
    int NumFlares;
    FlarDesc[] Flares = new FlarDesc[32];
};

public struct StarsCfg
{
    public crc32value texture;
    public crc32value rs;
    public int num;
};


class GalaxyCfg
{
    public crc32value texture;
    public crc32value rs;
    public float width;
    public float height;
    public float angle;
    public int nSegments;
    public DWORD[] Reserv = new DWORD[2];
};

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class SkyCfg : IStormImportable<SkyCfg>
{
    public const uint SF_LAYER0 = 0x00000001;
    public const uint SF_LAYER1 = 0x00000002;
    public const uint SF_SUN = 0x00000004;
    public const uint SF_STARS = 0x00000008;
    public const uint SF_PLANET = 0x00000010;
    public const uint SF_FLARES = 0x00000020;
    public const uint SF_GALAXY = 0x00000040;

    public int flags;

    public crc32value planets;
    public crc32value layer0;
    public crc32value layer1;
    public crc32value sun;
    public crc32value flares;
    public crc32value stars;
    public crc32 galaxy;

    public SkyCfg Import(Stream st)
    {
        //Stream st = dll_data.files.GetBlock(config).myStream;
        //cur_cfgid = config;
        return StormFileUtils.ReadStruct<SkyCfg>(st);
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        sb.AppendLine("Flags: " + flags.ToString("X8"));
        sb.AppendLine("Planets: " + planets.ToString("X8"));
        sb.AppendLine("Layer0: " + layer0.ToString("X8"));
        sb.AppendLine("Layer1: " + layer1.ToString("X8"));
        sb.AppendLine("Sun: " + sun.ToString("X8"));
        sb.AppendLine("Flares: " + flares.ToString("X8"));
        sb.AppendLine("Stars: " + stars.ToString("X8"));
        sb.AppendLine("Galaxy: " + galaxy.ToString("X8"));

        return sb.ToString();

    }
}