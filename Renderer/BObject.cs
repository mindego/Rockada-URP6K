using UnityEngine;
using UnityEngine.Assertions;
using WORD = System.Int32;

public class BObject : IBObject
{
    /// <summary>
    ///         <br>BObject.SetVertO(4 * n + 0, new Vector2(x1, y1), new Vector2(tx1 / 256f, ty1 / 256f));</br>
    ///         <br>BObject.SetVertO(4 * n + 1, new Vector2(x1, y2), new Vector2(tx1 / 256f, ty2 / 256f));</br>
    ///         <br>BObject.SetVertO(4 * n + 2, new Vector2(x2, y2), new Vector2(tx2 / 256f, ty2 / 256f));</br>
    ///         <br>BObject.SetVertO(4 * n + 3, new Vector2(x2, y1), new Vector2(tx2 / 256f, ty1 / 256f));</br>
    /// </summary>
    /// 
    public override string ToString()
    {
        string res = this.GetType().ToString() + " " + name + "\n";
        for (int i = 0; i < m_Vertices.Length; i++)
        {
            res += "Vectex[" + i + "] " + m_Vertices[i] + "\n";
        }
        return res;
    }
    public BillVertex[] m_Vertices;
    public int m_NumVertices;
    public WORD[] m_Indices;
    public int m_NumIndices;

    public float def_z;

    int m_ActiveVertices;
    int m_ActiveFaces;

    bool locked;
    //void CalculateBound();
    //BBox m_BoxBound;
    //BSphere m_SphereBound;
    bool m_BoundValid;
    public IBill Bill { get; set; }
    public string name { get; set; }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public bool Lock()
    {
        locked = true;
        return locked;
    }

    public int RefCount()
    {
        return 0;
    }

    public int Release()
    {
        return 0;
    }

    public void SetActiveMesh(int n_vertices, int n_faces)
    {
        m_ActiveVertices = n_vertices;
        m_ActiveFaces = n_faces;
        m_BoundValid = false;
    }

    public void SetBill(IBill bill)
    {
        Bill = bill;
    }

    public void SetFace(int f, int v0, int v1, int v2)
    {
        Assert.IsTrue(locked);

        Assert.IsTrue(v0 < m_NumVertices && v0 >= 0);
        Assert.IsTrue(v1 < m_NumVertices && v1 >= 0);
        Assert.IsTrue(v2 < m_NumVertices && v2 >= 0);
        Assert.IsTrue(f < m_NumIndices / 3 && f >= 0);

        m_Indices[f * 3] = v0;
        m_Indices[f * 3 + 1] = v1;
        m_Indices[f * 3 + 2] = v2;
    }

    public (int,int,int) GetFace(int f)
    {
        return (m_Indices[f * 3], m_Indices[f * 3 + 1], m_Indices[f * 3 + 2]);

    }

    public void SetVertC(int i, Color32 col, Color32 spec)
    {
        Assert.IsTrue(locked);
        Assert.IsTrue(i < m_NumVertices && i >= 0);

        m_Vertices[i].color = col;
        m_Vertices[i].specular = spec;
    }

    public void SetVertC(int start, int num, Color32 col, Color32 spec)
    {
        Asserts.AssertBp(locked);
        num += start;

        //DWORD c = col.PackARGBMax();
        //DWORD s = spec.PackARGBMax();

        for (int i = start; i != num; ++i)
        {
            Asserts.AssertBp(i < m_NumVertices && i >= 0);
            m_Vertices[i].color = col;
            m_Vertices[i].specular = spec;
        }
    }

    public void SetVertO(int i, Vector2 pos, Vector2 tex)
    {
        Asserts.AssertBp(locked);

        Asserts.AssertBp(i < m_NumVertices && i >= 0);

        //m_Vertices[i].position = new Vector2(pos.x, 1-pos.y);
        m_Vertices[i].position = new Vector2(pos.x, pos.y);
        m_Vertices[i].depth = def_z;
        m_Vertices[i].texcoords = new Vector2(tex.x, 1 - tex.y);
    }

    public void SetVertO(int i, Vector2 pos)
    {
        Assert.IsTrue(locked);

        Assert.IsTrue(i < m_NumVertices && i >= 0);

        m_Vertices[i].position = pos; //TODO Возможно, тут надо копировать, а не присваивать
        m_Vertices[i].depth = def_z;
    }

    public void UnLock()
    {
        locked = false;
        //m_BoundValid = false;
    }

    internal bool Initialize(int numv, int numq, float _def_z)
    {
        def_z = _def_z;
        m_NumVertices = numv;
        m_Vertices = new BillVertex[m_NumVertices];
        m_ActiveVertices = numv;
        m_NumIndices = numq * 3;
        m_Indices = new WORD[m_NumIndices];
        m_ActiveFaces = numq;

        return true;
    }

    internal Rect GetTexRect(int index)
    {
        Rect texRect = new Rect();

        texRect.position = m_Vertices[4 * index + 1].texcoords; //из-за "перевёрнутой" системы координат в текстуре нужен нижний, а не верхний пиксель
        //texRect.width = Mathf.Abs(m_Vertices[4 * index + 0].texcoords.x - m_Vertices[4 * index + 2].texcoords.x); 
        //texRect.height = Mathf.Abs(m_Vertices[4 * index + 0].texcoords.y - m_Vertices[4 * index + 2].texcoords.y);
        texRect.width = m_Vertices[4 * index + 0].texcoords.x - m_Vertices[4 * index + 2].texcoords.x;
        texRect.height = m_Vertices[4 * index + 0].texcoords.y - m_Vertices[4 * index + 2].texcoords.y;

        return texRect;
    }

    internal Rect GetTexRectRaw(int index)
    {
        Rect texRect = new Rect();
        
        texRect.xMin = m_Vertices[4 * index + 0].texcoords.x;
        texRect.xMax = m_Vertices[4 * index + 2].texcoords.x;

        texRect.yMin = m_Vertices[4 * index + 2].texcoords.y;
        texRect.yMax = m_Vertices[4 * index + 0].texcoords.y;

        return texRect;
    }


    internal Color GetColor(int index)
    {
        //TODO возвращать корректный цвет худдевайса
        //return m_Vertices[4*index + 0].color;
        return Color.green;
    }

    internal Color GetColor()
    {
        return Color.green;
    }

    public BillVertex[] GetVertices() { return m_Vertices; }
    public int GetNumActiveVertices() { return m_ActiveVertices; }
    public WORD[] GetIndices() { return m_Indices; }
    public int GetNumActiveIndices() { return m_ActiveFaces * 3; }
}
