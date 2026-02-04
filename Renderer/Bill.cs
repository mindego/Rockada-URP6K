using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static renderer_dll;
using DWORD = System.UInt32;
using WORD = System.UInt16;


public class Bill : IBill
{
    //TODO реализовать класс Bill??
    BillStats m_Stats = new BillStats();
    float BillNearZ = 0.0f, BillFarZ = CAMERA.SceneNearZ;

    public IBObject Create(int nv, int nf)
    {
        //#pragma message ("z-status of bobjects?")
        BObject bobject = new BObject();

        if (!bobject.Initialize(nv, nf, 1.0f))
        {
            bobject.Release();
            return null;
        }
        bobject.SetBill(this);
        m_Stats.NumCreatedBObjects++;
        //m_Stats.NumCreatedFacets += nf;
        //m_Stats.NumCreatedVertices += nv;
        return bobject;

    }
    public ITexturesDB CreateTexturesDB(string path)
    {
        return (ITexturesDB)dll_data.CreateTexturesDB(path);
    }

    public void ApplyTransform()
    {
        //Matrix2D m2d = m_TransformStack.Peek();

        transform_applied = true;
    }

    public IFont CreateFont(string fontname, float width, float height)
    {
        string font_name;
        font_name = fontname + "#font";
        FontData data = dll_data.LoadFile<FontData>(font_name);
        if (data != null)
        {
            StormFontImpl font = new StormFontImpl();
            if (!font.Initialize(this, data))
            {
                font.Release();
                return null;
            }
            font.SetWH(width, height, 0);
            //using (StreamWriter outputFile = new StreamWriter(font.data.bitmap+"#" + fontname + ".txt"))
            //{
            //    outputFile.Write(font.DumpFontInfo());
            //}
            return font;
        }
        return null;
    }
    //public void Draw(IBObject obj)
    //{
    //    //Renderer.StormUI.Add(obj);
    //    //Debug.Log("Drawing " + obj.name + " using texture " + BillTexture==null? "NoTexture":BillTexture.name);
    //    //if (obj == null)
    //    //{
    //    //    Debug.Log("IBObject is empty");
    //    //    return;
    //    //}
    //    if (!transform_applied)
    //        ApplyTransform();


    //    Renderer.StormUI.DrawIBO(obj);
    //}

    public void Draw(IBObject myObject)
    {
        BObject bobject = (BObject)myObject;

        //Assert.IsTrue(drawing);

        //if (!Engine.DrawBill) return;

        if (!transform_applied)
            ApplyTransform();

        Renderer.StormUI.DrawIBO(myObject, m_TransformStack);
    }

    private Texture2D BillTexture;
    public void SetTexture(Texture2D texture)
    {
        //Debug.Log(string.Format("Switching texture from {0} to {1}",BillTexture == null ? "Empty":BillTexture.name,texture.name));
        BillTexture = texture;
    }
    public Texture2D GetTexture()
    {
        return BillTexture;
    }

    public ICompositeMap CreateMap(MapData d, ITexturesDB db)
    {
        return new Largemap(this, d, db);
    }

    public IBClipper CreateClipper(int v, Vector2[] clip)
    {
        //throw new System.NotImplementedException();
        return null; //TODO реализовать клиппер (предположительно - обрезалку изображения
    }

    public void SetStyle(BillStyle billStyle)
    {
        //TODO РЕализовать установку стиля доски
        //throw new System.NotImplementedException();
    }

    //TransformStack2D m_TransformStack;

    public Stack<Matrix2D> m_TransformStack = new Stack<Matrix2D>();
    bool drawing;
    bool transform_applied;
    public void PopTrasform()
    {
        m_TransformStack.Pop();
        transform_applied = false;
    }
    /// <summary>
    /// Поместить трансформ в стек трансформов.
    /// в исходниках имя методе - с опечаткой
    /// </summary>
    /// <param name="transform"></param>
    public void PushTrasform(Matrix2D transform)
    {
        m_TransformStack.Push(transform);
        transform_applied = false;
    }

    BClipper m_CurrentClipper;
    public void SetClipping(IBClipper clipper)
    {
        if (clipper != null) clipper.AddRef();
        m_CurrentClipper = (BClipper)clipper;
        if (m_CurrentClipper != null) m_CurrentClipper.Apply(m_TransformStack.Peek());
    }

    public string DumpTransformStack()
    {
        var ClonedStack = new Stack<Matrix2D>(m_TransformStack);
        string res = "";
        Matrix2D ma;
        int cnt = ClonedStack.Count - 1;
        while (ClonedStack.Count > 0)
        {
            ma = ClonedStack.Pop();
            res += string.Format("Transform {0} {1}\n", cnt--, ma);
        }
        return res;
    }

    public void Begin()
    {
        Renderer.StormUI.ClearScreen();
        //Engine.SetViewPort(ViewPort(0, 0, d3d.Dx(), d3d.Dy(), BillNearZ, BillFarZ));

        //bill_start_state.Apply();

        {
            Matrix34f m = new Matrix34f();
            m.Identity();
            m.pos[0] = -.5f;
            m.pos[1] = .75f / 2f;

            {
                Vector3 tmpVector = m.tm[1];
                tmpVector[1] = -1;
                m.tm[1] = tmpVector;
            }

            //Engine.SetViewTransform(m);
        }

        {
            Engine.SetProjectionPersp(2, 2 / .75f, .5f, 2.0f);
        }

        transform_applied = false;
        drawing = true;

        Engine.InitStack();
        {
            Matrix2D m = new Matrix2D(); m.Identity();
            PushTrasform(m);
        }
    }

    public void End()
    {
        PopTrasform();
        Engine.DoneStack();
        drawing = false;
    }
}


public class BClipper : IBClipper
{
    public BClipper() { }
    ~BClipper() { }
    //bool Initialize(Bill _bill, int n, const CPlane* planes );
    bool Initialize(Bill _bill, int n, object planes)
    {
        m_NumPlanes = n;

        //m_Planes = new CPlane[m_NumPlanes];
        //m_PlanesTransformed = new CPlane[m_NumPlanes];
        //for (int i = 0; i < m_NumPlanes; ++i)
        //    m_Planes[i] = planes[i];
        return true;
    }
    //ClipStatus SphereVisible( BSphere );
    public void Clip(Matrix2D object_locus, DWORD clip_status,
               BillVertex in_v, int num_in_v, WORD in_i, int num_in_i,
               BillVertex out_v, int num_out_v, WORD out_i, int num_out_i)
    {
        return;
    }

    public void Apply(Matrix2D tm) { }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }

    public int Release()
    {
        throw new System.NotImplementedException();
    }

    //private CPlane* m_Planes;
    private int m_NumPlanes;

    //CPlane* m_PlanesTransformed;
}

public class BillStats
{
    public int NumCreatedBObjects;
    int NumCreatedFacets;
    int NumCreatedVertices;
    public BillStats()
    {
        NumCreatedBObjects = 0;
        NumCreatedFacets = 0;
        NumCreatedVertices = 0;
    }
};

