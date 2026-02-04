using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WORD = System.Int32;//На самом деле должно быть UInt16
using Face = System.ValueTuple<System.Int32, System.Int32, System.Int32>;
using System;
using UnityEngine.Pool;

public class UIDrawer
{
    private GameObject UICanvasHolder;
    private Canvas UICanvas;
    private Dictionary<string, Sprite> SpriteStorage = new Dictionary<string, Sprite>();
    private Dictionary<int, GameObject> GameObjectStorage = new Dictionary<int, GameObject>();

    public List<IBObject> screenObjects = new List<IBObject>();
    public UIDrawer()
    {
        UICanvasHolder = new GameObject("UICanvasHolder");
        UICanvas = UICanvasHolder.AddComponent<Canvas>();
        //UICanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        UICanvas.renderMode = RenderMode.ScreenSpaceCamera;
        UICanvas.worldCamera = Engine.UnityCamera;
        UICanvas.vertexColorAlwaysGammaSpace = true;
        UICanvasHolder.SetActive(true);
    }

    public void Add(IBObject obj)
    {
        screenObjects.Add(obj);
    }

    public void Clear()
    {
        screenObjects.Clear();
    }

    internal void ClearScreen()
    {
        for (var i = UICanvasHolder.transform.childCount - 1; i >= 0; i--)
        {
            //GameObject.Destroy(UICanvasHolder.transform.GetChild(i).gameObject);
            //UICanvasHolder.transform.GetChild(i).gameObject.SetActive(false);
            HudDevicesPool.Release(UICanvasHolder.transform.GetChild(i).gameObject);
        }
    }

    public void DrawIBO(IBObject v, Stack<Matrix2D> m_transformstack)
    {
        DrawIBO(v);
    }

    private Rect[] GetRects(BObject myBObject)
    {
        //RECT должен получаться из двух граней (faces), имеющих как минимум 2 общих вершины.
        List<Rect> rects = new List<Rect>();
        Rect tmpRect;
        //m_Indices[f * 3] = v0;
        //m_Indices[f * 3 + 1] = v1;
        //m_Indices[f * 3 + 2] = v2;
        List<Face> faces = new List<Face>();

        for (int i = 0; i < myBObject.GetNumActiveIndices(); i += 3)
        {
            var rFace = (myBObject.m_Indices[i], myBObject.m_Indices[i + 1], myBObject.m_Indices[i + 2]);
            foreach (Face lFace in faces)
            {
                if (IsPair(lFace, rFace, out WORD[] indices))
                {
                    tmpRect = GetRect(lFace, rFace, indices);

                }
            }
            faces.Add(rFace);
        }
        return rects.ToArray();
    }

    private Rect GetRect(Face lFace, Face rFace, WORD[] indices)
    {
        Rect tmpRect = new Rect();
        return tmpRect;
    }


    private bool IsPair(Face lFace, Face rFace, out WORD[] indices)
    {
        indices = new WORD[4];

        HashSet<WORD> myIndices = new HashSet<WORD> {
            lFace.Item1,
            lFace.Item2,
            lFace.Item3,
            rFace.Item1,
            rFace.Item2,
            rFace.Item3
        };

        if (myIndices.Count != 4) return false;

        int i = 0;
        foreach (WORD v in myIndices)
        {
            indices[i++] = v;
        }
        return true;
    }

    private Vector2[] GetTransformedVertices(BSubObject mySubobject, Bill bill)
    {
        Vector2[] vertices = new Vector2[4];
        for (int i = 0; i < mySubobject.m_NumVertices; i++)
        {
            vertices[i] = FullStackTransformPoint(mySubobject.m_Vertices[i].position, bill);
        }
        return vertices;
    }
    private Vector2[] GetTransformedVertices(BObject myBobject, Bill bill = null)
    {
        //Bill bill = (Bill)myBobject.Bill;
        if (bill == null) bill = (Bill)myBobject.Bill;
        if (bill == null) Debug.LogError("Bill is empty for " + myBobject);

        Vector2[] vertices = new Vector2[myBobject.m_NumVertices];
        for (int i = 0; i < myBobject.m_NumVertices; i++)
        {
            vertices[i] = FullStackTransformPoint(myBobject.m_Vertices[i].position, bill);
        }
        return vertices;
    }
    private Vector2 GetSizeInCanvas(Vector2[] vertices)
    {
        return new Vector2(
                Mathf.Abs(vertices[0].x - vertices[2].x) * UICanvas.pixelRect.width, 
                Mathf.Abs(vertices[0].y - vertices[2].y) * UICanvas.pixelRect.height
            );
    }


    private Vector2 GetSizeInCanvas(Vector2[] vertices, int index)
    {
        return new Vector2(
            Mathf.Abs(
                vertices[4 * index + 0].x - vertices[4 * index + 2].x) * UICanvas.pixelRect.width,
            Mathf.Abs(
                vertices[4 * index + 0].y - vertices[4 * index + 2].y) * UICanvas.pixelRect.height
            );
    }
    private Vector2 GetPositionInCanvas(Vector2[] vertices)
    {
        float pos_x = ((vertices[0].x + vertices[2].x) / 2);
        float pos_y = ((vertices[0].y + vertices[2].y) / 2);
        return new Vector2(pos_x * UICanvas.pixelRect.width, -pos_y * UICanvas.pixelRect.height);
    }
    private Vector2 GetPositionInCanvas(Vector2[] vertices, int index)
    {
        float pos_x = ((vertices[4 * index + 0].x + vertices[4 * index + 2].x) / 2);
        float pos_y = ((vertices[4 * index + 0].y + vertices[4 * index + 2].y) / 2);
        return new Vector2(pos_x * UICanvas.pixelRect.width, -pos_y * UICanvas.pixelRect.height);
    }
    private Vector2 FullStackTransformPoint(Vector2 point, Bill bill)
    {
        var ClonedStack = new Stack<Matrix2D>(bill.m_TransformStack);

        return FullStackTransformPoint(point, ClonedStack);
    }

    private Vector2 FullStackTransformPoint(Vector2 point, Stack<Matrix2D> TransformStack)
    {
        Vector2 res = point;
        Matrix2D ma;
        while (TransformStack.Count > 0)
        {
            ma = TransformStack.Pop();
            res = ma.TransformPoint(res);
        }
        return res;
    }

    //private GameObject CreateHUDdevice(string name, BObject myBobject, BSubObject myBSubObject)
    //{
    //    GameObject igObj;
    //    Sprite sprite;
    //    Image IR;

    //    Bill bill = (Bill)myBobject.Bill;

    //    igObj = new GameObject(name);
    //    igObj.transform.parent = UICanvasHolder.transform;

    //    sprite = GetSprite(bill, myBSubObject.GetTexRect());
    //    sprite.name = bill.GetTexture().name + "#" + myBobject.name;

    //    IR = igObj.AddComponent<Image>();
    //    IR.sprite = sprite;

    //    return igObj;
    //}
    //private GameObject CreateHUDdevice(string name, BObject myBobject, int i)
    //{
    //    GameObject igObj;
    //    Sprite sprite;
    //    Image IR;


    //    Bill bill = (Bill)myBobject.Bill;

    //    igObj = new GameObject(name);
    //    igObj.transform.parent = UICanvasHolder.transform;

    //    sprite = GetSprite(bill, myBobject.GetTexRectRaw(i));
    //    sprite.name = myBobject.Bill.GetTexture().name + "#" + myBobject.name;

    //    IR = igObj.AddComponent<Image>();
    //    IR.sprite = sprite;

    //    return igObj;
    //}


    //private void DrawIBO_RawImage(IBObject v)
    //{
    //    //GameObject gObj = new GameObject("GUI Object " + v.name + " " + (i + 1) + "/" + RectCount);
    //    //gObj.transform.parent = UICanvasHolder.transform;
    //    //RawImage image = gObj.AddComponent<RawImage>();
    //    //{
    //    //    image.rectTransform.anchorMin = new Vector2(0f, 1f);
    //    //    image.rectTransform.anchorMax = new Vector2(0f, 1f);
    //    //} //Top-Left
    //    //image.texture = myBobject.Bill.GetTexture();
    //    //image.rectTransform.sizeDelta = GetSizeInCanvas(TransformedVertices, i);
    //    //image.rectTransform.anchoredPosition = GetPositionInCanvas(TransformedVertices, i);
    //    //image.color = myBobject.GetColor(i);
    //    //image.uvRect = myBobject.GetTexRect(i);
    //    //Debug.Log("Drawing bobj " + i + " " + v.name + " " + TransformedVertices[4 * i + 0].y + ":" + TransformedVertices[4 * i + 2].y + " mage.uvRect " + image.uvRect);
    //}

    public class BSubObject
    {
        public BillVertex[] m_Vertices;
        public int m_NumVertices;

        public override string ToString()
        {
            string res = "";
            for (int i = 0; i < m_NumVertices; i++)
            {
                res += string.Format("Vectex {0} {1}\n", i, m_Vertices[i]);
            }
            return res;
        }
        public BSubObject()
        {
            m_Vertices = new BillVertex[4];
            m_NumVertices = 0;
        }

        public void AddVertex(BillVertex v)
        {
            if (m_NumVertices >= 4) return;
            m_Vertices[m_NumVertices++] = v;
        }

        /// <summary>
        /// Размещение вершин для создания RECT в корректном положении (CCW)
        /// [NW,SW,SE,NE]
        /// </summary>
        public void RectangularCorrection()
        {
            //Debug.Log(("RAW:" ,m_Vertices[0], m_Vertices[1], m_Vertices[2], m_Vertices[3]));
            Array.Sort(m_Vertices, new BillVertexComparator());
            Swap(2, 3);
            if (m_Vertices[0].texcoords.y == m_Vertices[2].texcoords.y) Swap(2, 3);
            //Debug.Log(("Transformed:", m_Vertices[0], m_Vertices[1], m_Vertices[2], m_Vertices[3]));
        }
        private class BillVertexComparator : IComparer<BillVertex>
        {
            public int Compare(BillVertex v1, BillVertex v2)
            {
                if (v1.position.x != v2.position.x) return (v1.position.x > v2.position.x) ? 1 : -1;
                if (v1.position.y != v2.position.y) return (v1.position.y > v2.position.y) ? 1 : -1;
                return 0;
            }
        }

        private void Swap(int i1, int i2)
        {
            BillVertex tmp;
            tmp = m_Vertices[i1];
            m_Vertices[i1] = m_Vertices[i2];
            m_Vertices[i2] = tmp;

        }

        internal Rect GetTexRect()
        {
            //Debug.Log(("RAW:" ,m_Vertices[0], m_Vertices[1], m_Vertices[2], m_Vertices[3]));
            Rect texRect = new Rect();

            texRect.xMin = m_Vertices[0].texcoords.x;
            texRect.xMax = m_Vertices[2].texcoords.x;

            texRect.yMin = m_Vertices[2].texcoords.y;
            texRect.yMax = m_Vertices[0].texcoords.y;

            return texRect;
        }

        internal Color GetColor()
        {
            return m_Vertices[0].color;
        }
    }

    private BSubObject[] GetBSubObjects(BObject myBObject)
    {
        int BSubObjectNum = myBObject.GetNumActiveVertices() / 4;
        List<BSubObject> BSubObjects = new List<BSubObject>();
        BSubObject tmpSubObject;

        List<Face> faces = new List<Face>();
        Face rFace;
        for (int i = 0; i < myBObject.GetNumActiveIndices(); i += 3)
        {
            rFace = (myBObject.m_Indices[i], myBObject.m_Indices[i + 1], myBObject.m_Indices[i + 2]);
            foreach (Face lFace in faces)
            {
                if (IsPair(lFace, rFace, out WORD[] indices))
                {
                    tmpSubObject = new BSubObject();
                    foreach (WORD index in indices)
                    {
                        tmpSubObject.AddVertex(myBObject.GetVertices()[index]);
                    }

                    tmpSubObject.RectangularCorrection();
                    BSubObjects.Add(tmpSubObject);
                }
            }
            faces.Add(rFace);
        }
        return BSubObjects.ToArray();
    }


    internal void DrawIBO(IBObject v)
    {
        //DrawIBONaive(v);
        //DrawIBOSubobj(v);
        DrawIBOMesh(v);
    }

    private ObjectPool<GameObject> HudDevicesPool = new ObjectPool<GameObject>(onCreatePoolObject, onTakePoolObject, onReleasePoolObject, onDestroyPoolObject);

    private static void onDestroyPoolObject(GameObject obj)
    {
        return;
    }

    private static void onReleasePoolObject(GameObject obj)
    {
        obj.transform.SetParent(null);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        obj.SetActive(false);
    }

    private static void onTakePoolObject(GameObject obj)
    {
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        obj.transform.localScale = Vector3.one;
        obj.SetActive(true);
    }

    private static GameObject onCreatePoolObject()
    {
        //GameObject igObj = new GameObject();
        //igObj.AddComponent<Image>();
        //igObj.SetActive(false);
        //return igObj;

        GameObject igobj = new GameObject();
        var mr = igobj.AddComponent<MeshRenderer>();
        var mf = igobj.AddComponent<MeshFilter>();
        mr.material= new Material(MaterialStorage.DefaultUI);
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        igobj.SetActive(false);
        return igobj;
    }

    private Vector3[] GetTransformedVertices3D(BObject myBObject, Bill bill)
    {
        Vector2[] TransformedVertices2D = GetTransformedVertices(myBObject, bill);
        Vector3[] TransformedVertices3D = new Vector3[TransformedVertices2D.Length];
        for (int i=0;i< TransformedVertices2D.Length;i++)
        {
            TransformedVertices3D[i] = new Vector3(TransformedVertices2D[i].x, TransformedVertices2D[i].y, 0);
        }
        return TransformedVertices3D;
    }
    private void DrawIBOMesh(IBObject v)
    {
        BObject myBObject = (BObject)v;
        Bill bill = (Bill)myBObject.Bill;
        var origVertices = myBObject.GetVertices();
        var origIndices = myBObject.GetIndices();
        //int[] tris = myBObject.GetIndices();
        int[] tris = new int[myBObject.GetNumActiveIndices()];
        Vector2[] uv = new Vector2[myBObject.GetNumActiveVertices()];
        Vector3[] transVertices = new Vector3[myBObject.GetNumActiveVertices()];
        Color[] colors = new Color[myBObject.GetNumActiveVertices()];
        for (int i=0;i<myBObject.GetNumActiveVertices(); i++)
        {
            uv[i] = origVertices[i].texcoords;
            transVertices[i]= FullStackTransformPoint(myBObject.m_Vertices[i].position, bill);
            transVertices[i].x -= 0.5f;
            transVertices[i].x *= UICanvas.pixelRect.width;

            transVertices[i].y = 1 - transVertices[i].y;
            transVertices[i].y -= 0.5f;
            transVertices[i].y *= UICanvas.pixelRect.height;

            colors[i] = origVertices[i].color;
        }

        for (int i =0;i< myBObject.GetNumActiveIndices(); i+=3)
        {
            tris[i] = origIndices[i];
            tris[i+1] = origIndices[i+2];
            tris[i + 2] = origIndices[i + 1];
        }

        Mesh billMesh = new Mesh();
        billMesh.vertices = transVertices;
        billMesh.triangles = tris;
        billMesh.uv = uv;
        billMesh.name = "mesh " + v.name;
        billMesh.colors = colors;
        billMesh.RecalculateNormals();
        billMesh.RecalculateBounds();
        billMesh.RecalculateTangents();
        GameObject gobj = HudDevicesPool.Get();
        //GameObject gobj = new GameObject();
        gobj.name=v.name;
        gobj.transform.SetParent(UICanvasHolder.transform, false);
        gobj.transform.localPosition = Vector3.zero;
        gobj.transform.localRotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        var mf = gobj.GetComponent<MeshFilter>();
        var mr = gobj.GetComponent<MeshRenderer>();
        //var mr = gobj.AddComponent<MeshRenderer>();
        //var mf = gobj.AddComponent<MeshFilter>();
        //mr.material = new Material(MaterialStorage.DefaultTransparent);

        mf.mesh = billMesh;
        mr.material.mainTexture = bill.GetTexture();
        //mr.material.color= myBObject.GetColor();
    }
    internal void DrawIBOSubobj(IBObject v)
    {
        BObject myBObject = (BObject)v;
        Bill bill = (Bill)myBObject.Bill;
        //Vector2[] TransformedVertices = GetTransformedVertices(myBObject, bill);

        BSubObject[] bSubObjects = GetBSubObjects(myBObject);
        int subobjNum = bSubObjects.Length;
        for (int i = 0; i < bSubObjects.Length; i++)
        {
            BSubObject bSubObj = bSubObjects[i];
            Sprite tmpSprite= GetSprite(bill, bSubObj.GetTexRect());
            if (tmpSprite == null) continue;
            Vector2[] TransformedVertices = GetTransformedVertices(bSubObj, bill);
            string name = "GUI Object image " + myBObject.name + " " + (i + 1) + " / " + subobjNum + " " + bSubObj.ToString();
            //var hash = (int)Hasher.HshString(name + bill.m_TransformStack.Peek());

            GameObject igObj;
            Image IR;
            Vector2 size= GetSizeInCanvas(TransformedVertices);
            Vector2 position = GetPositionInCanvas(TransformedVertices);
            if (size.x == 0 || size.y == 0) continue; //объект нулевого размера нет смысла рисовать.
            //Debug.Log(string.Format("Drawing subobj {0} @ {1} {2}", myBObject.name,position,size));
            if (position.x > UICanvas.pixelRect.width || position.x < 0) continue; //Находящиеся за пределами экрана - тоже
            if (-position.y > UICanvas.pixelRect.height || -position.y < 0) continue; //Находящиеся за пределами экрана - тоже
            igObj = HudDevicesPool.Get();
            //igObj.transform.parent = UICanvasHolder.transform;
            igObj.transform.SetParent(UICanvasHolder.transform, false);
            igObj.name = name;

           
            IR = igObj.GetComponent<Image>();
            IR.sprite = tmpSprite;
            IR.sprite.name = bill.GetTexture().name + "#" + myBObject.name;

            IR.rectTransform.anchorMin = new Vector2(0f, 1f);
            IR.rectTransform.anchorMax = new Vector2(0f, 1f);
            IR.rectTransform.sizeDelta = size;
            IR.rectTransform.localPosition=Vector3.zero;
            IR.rectTransform.localRotation = Quaternion.LookRotation(Vector3.forward,Vector3.up);
            IR.rectTransform.localScale = Vector3.one;
            IR.rectTransform.anchoredPosition = position;
            
            //IR.color = myBObject.GetColor(i);
            IR.color = bSubObj.GetColor();
            if (size.x==0 || size.y==0)
            {
                Debug.LogError("Zero size object "+ bSubObj);
            }

        }

    }
    //internal void DrawIBONaive(IBObject v)
    //{
    //    BObject myBobject = (BObject)v;
    //    Bill bill = (Bill)myBobject.Bill;

    //    //int RectCount = myBobject.m_NumVertices / 4;
    //    int RectCount = myBobject.GetNumActiveVertices() / 4;
    //    Vector2[] TransformedVertices = GetTransformedVertices(myBobject, bill);
    //    int bobjhash = myBobject.GetHashCode();

    //    //{
    //    //    string res = "";
    //    //    for (int i = 0; i < 4; i++)
    //    //    {
    //    //        res += string.Format("Vectex {0} {1}\n", i, myBobject.m_Vertices[i]);
    //    //    }
    //    //    Debug.Log(res);
    //    //}
    //    for (int i = 0; i < RectCount; i++)
    //    {
    //        string name = "GUI Object image " + myBobject.name + " " + (i + 1) + " / " + RectCount + " " + bobjhash.ToString("X8");
    //        var hash = (int)Hasher.HshString(name + bill.m_TransformStack.Peek());

    //        GameObject igObj;
    //        Image IR;

    //        if (!GameObjectStorage.ContainsKey(hash)) GameObjectStorage.Add(hash, CreateHUDdevice(name, myBobject, i));
    //        igObj = GameObjectStorage[hash];
    //        //igObj = CreateHUDdevice(name, myBobject, i);
    //        IR = igObj.GetComponent<Image>();
    //        {
    //            IR.rectTransform.anchorMin = new Vector2(0f, 1f);
    //            IR.rectTransform.anchorMax = new Vector2(0f, 1f);
    //            IR.rectTransform.sizeDelta = GetSizeInCanvas(TransformedVertices, i);
    //            IR.rectTransform.anchoredPosition = GetPositionInCanvas(TransformedVertices, i);
    //            IR.color = myBobject.GetColor(i);
    //        }
    //        igObj.SetActive(true);
    //    }
    //}

    private Sprite GetSprite(Texture2D myTexture, Rect TextureRect)
    {
        if (TextureRect.xMax > 1 || TextureRect.yMax > 1 || TextureRect.xMin < 0 || TextureRect.yMin < 0)
        {
            TextureRect.xMin = Mathf.Clamp01(TextureRect.xMin);
            TextureRect.xMax = Mathf.Clamp01(TextureRect.xMax);
            TextureRect.yMin = Mathf.Clamp01(TextureRect.yMin);
            TextureRect.yMax = Mathf.Clamp01(TextureRect.yMax);
        }
        TextureRect.xMin *= myTexture.width;
        TextureRect.yMin *= myTexture.height;
        TextureRect.xMax *= myTexture.width;
        TextureRect.yMax *= myTexture.height;
        //rect.xMin = Mathf.Clamp(rect.xMin, 0, myTexture.width);
        //rect.xMax = Mathf.Clamp(rect.xMax, 0, myTexture.width);
        //rect.yMin = Mathf.Clamp(rect.yMin, 0, myTexture.height);
        //rect.yMax = Mathf.Clamp(rect.yMax, 0, myTexture.height);

        if (TextureRect.width == 0 || TextureRect.height == 0)
        {
            //Debug.LogError("Incorrect Texture rect: " + TextureRect);
            return null;
        }
        Sprite mySprite;
        string key = myTexture.name + "#" + (TextureRect.xMin, TextureRect.xMax, TextureRect.yMin, TextureRect.yMax);
        if (!SpriteStorage.ContainsKey(key))
        {
            try
            {

                mySprite = Sprite.Create(myTexture, TextureRect, Vector2.zero);
                if (mySprite.rect.width < 1 || mySprite.rect.height < 1) mySprite = null;
                //Sprite.Create(myTexture,rect,Vector2.zero,128,)
                SpriteStorage.Add(key, mySprite);
                //Debug.Log("HUD Sprite created: " + (TextureRect.xMin, TextureRect.xMax, TextureRect.yMin, TextureRect.yMax));
                //Debug.Log(string.Format("Sprite created: {0} TextureRect {2} of {1} mySprite {3}", myTexture.name, (myTexture.width,myTexture.height),(TextureRect.xMin, TextureRect.xMax, TextureRect.yMin, TextureRect.yMax),(mySprite.rect,mySprite.textureRect)));
            }
            catch
            {
                Debug.LogError("HUD Sprite creation failed: " + (TextureRect.xMin, TextureRect.xMax, TextureRect.yMin, TextureRect.yMax));
                return null;
            }
        }
        
        //mySprite = Sprite.Instantiate(SpriteStorage[key]);
        mySprite = SpriteStorage[key];

        return mySprite;
    }
    private Sprite GetSprite(Bill bill, Rect TextureRect)
    {
        if (bill == null) return null;
        return GetSprite(bill.GetTexture(), TextureRect);
    }
}