using Codice.Client.BaseCommands.Merge;
using Codice.CM.Client.Differences;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Unity.Properties;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;
using UnityEngine;
using UnityEngine.Pool;
using static TerrainDefs;
using CPointsList = AList<CPOINT>;
using crc32 = System.UInt32;
using DWORD = System.UInt32;

public class DebugLog : IErrorLog, ILog
{
    public void AddException(System.Exception e)
    {
        throw new System.NotImplementedException();
    }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public void Append(string format, params object[] args)
    {
        throw new System.NotImplementedException();
    }

    public void AppendFailed()
    {
        throw new System.NotImplementedException();
    }

    public void AppendSucceeded()
    {
        throw new System.NotImplementedException();
    }

    public void CloseLogWindow()
    {
        throw new System.NotImplementedException();
    }

    public void CriticalMessage(string format, params object[] agrs)
    {
        throw new System.NotImplementedException();
    }

    public void DecIdent()
    {
        throw new System.NotImplementedException();
    }

    public bool Error(string at, int error, params string[] args)
    {
        Debug.Log(string.Format("{0} Error {1}", at, error));
        return false;
    }

    public void FlushLogFile()
    {
        throw new System.NotImplementedException();
    }

    public int GetIdent()
    {
        throw new System.NotImplementedException();
    }

    public void IncIdent()
    {
        throw new System.NotImplementedException();
    }

    public bool IsWindowOpen()
    {
        throw new System.NotImplementedException();
    }

    public void LogMessage(string message)
    {
        throw new System.NotImplementedException();
    }

    public void Message(string message)
    {
        throw new System.NotImplementedException();
    }

    public void Message(params string[] messages)
    {
        throw new System.NotImplementedException();
    }

    public void Message(string format, params string[] strings)
    {
        throw new System.NotImplementedException();
    }

    public void Message(string format, params float[] digits)
    {
        throw new System.NotImplementedException();
    }

    public void Message(string format, params object[] myparams)
    {
        Debug.Log(string.Format(format, myparams));
    }

    public void OpenLogFile(bool new_file = true, int max_size = 0)
    {
        throw new System.NotImplementedException();
    }

    public void OpenLogWindow(string title = "")
    {
        throw new System.NotImplementedException();
    }

    public void printMessage(int type, string fmt, params object[] va_list)
    {
        Debug.Log(type + string.Format(fmt, va_list));
    }

    public void printVMessage(int type, string fmt, object[] args)
    {
        throw new NotImplementedException();
    }

    public int RefCount()
    {
        throw new System.NotImplementedException();
    }

    public int Release()
    {
        throw new System.NotImplementedException();
    }

    public void ReOpenLogFile()
    {
        throw new System.NotImplementedException();
    }

    public void setAsStdOut()
    {
        throw new System.NotImplementedException();
    }

    public void setConsoleLineCount(int size)
    {
        throw new System.NotImplementedException();
    }

    public void setExecuteContext(string context)
    {
        throw new System.NotImplementedException();
    }

    public void setExtension(string ext)
    {
        throw new System.NotImplementedException();
    }

    public void SetIdent(int ident)
    {
        throw new System.NotImplementedException();
    }

    public void setIdentSize(int size)
    {
        throw new System.NotImplementedException();
    }

    public void SetLogFileMaxSize(int max_size)
    {
        throw new System.NotImplementedException();
    }

    public void setSource(string name, string text)
    {
        throw new System.NotImplementedException();
    }

    public void VAppend(string a, params object[] va_list)
    {
        throw new System.NotImplementedException();
    }

    public void VCriticalMessage(string a, params object[] va_list)
    {
        throw new System.NotImplementedException();
    }

    public void VMessage(string format, params string[] marker)
    {
        throw new System.NotImplementedException();
    }

    public void VMessage(string a, params object[] va_list)
    {
        throw new System.NotImplementedException();
    }
}
public class VmCommandDebug : IVmCommand
{
    private string name;
    public VmCommandDebug(string name)
    {
        Debug.Log("New command: " + name);
        this.name = name;
    }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public string describeParams(ref string myparams)
    {
        throw new System.NotImplementedException();
    }

    public string getName()
    {
        throw new System.NotImplementedException();
    }

    public IParamList getParamList()
    {
        throw new System.NotImplementedException();
    }

    public int getType()
    {
        throw new System.NotImplementedException();
    }

    public bool isParsingCorrect()
    {
        throw new System.NotImplementedException();
    }

    public void onCreate()
    {
        throw new System.NotImplementedException();
    }

    public void onOverride()
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

    public bool run()
    {
        throw new System.NotImplementedException();
    }
}
public class VmSeqDebug : IVmSequence
{
    public IVmCommand addCommand(string name, IVmFactory fct)
    {
        Debug.Log(string.Format("Command added {0}", name));
        return new VmCommandDebug(name);
    }

    public void AddRef()
    {
        throw new System.NotImplementedException();
    }

    public IVmSequence addSequence(string name, IVmFactory fct)
    {
        throw new System.NotImplementedException();
    }

    public string describeParams(ref string myparams)
    {
        throw new System.NotImplementedException();
    }

    public string getName()
    {
        throw new System.NotImplementedException();
    }

    public IParamList getParamList()
    {
        throw new System.NotImplementedException();
    }

    public int getType()
    {
        throw new System.NotImplementedException();
    }

    public bool isParsingCorrect()
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
}

public class STormTestVector3 : Editor
{
    [MenuItem("Tests/Vector3")]
    public static void OyWey()
    {
        //Final rotate axis: (0,6602784, -0,3209725, -0,6789764)->(Infinity, -Infinity, -Infinity) rotate_speed 1,969945E-41->1,969945E-41 speed (0,3478422, -0,2753445, -0,4549646) FC 0,1 RMS 3,141593 hr 0
        Vector3 rotate_axis = new Vector3(-0.3483355f, -0.7981761f, -0.4915051f);
        float rotate_speed = 1.839133E-39f;
        float FrictionCoeff = 0.1f;
        float RotateMaxSpeed = 3.141593f;
        Vector3 speed = new Vector3(0.5317292f, -0.4542829f, 0.2731082f);
        float HashRadius = 0;

        Vector3 axis_backup = rotate_axis;
        float speed_backup = rotate_speed;
        Vector3 Diff, Spd;
        Diff = new Vector3(0, HashRadius, 0);
        //Spd = -speed * DebrisData.FrictionCoeff;
        Spd = -speed * FrictionCoeff;
        Diff = Vector3.Cross(Diff, Spd);
        rotate_axis = Diff + rotate_axis * rotate_speed;
        Debug.LogFormat("1. rotate axis: {0}->{1} rotate_speed {2}->{3}", axis_backup, rotate_axis, speed_backup, rotate_speed);
        rotate_speed = rotate_axis.magnitude;
        if (rotate_speed != 0)
        //if (Mathf.Abs(rotate_speed) >= float.Epsilon)
            rotate_axis *= 1.0f / rotate_speed;
        Debug.LogFormat("2. rotate axis: {0}->{1} rotate_speed {2}->{3}", axis_backup, rotate_axis, speed_backup, rotate_speed);
        if (Mathf.Abs(RandomGenerator.Rand01()) < 0.25) rotate_speed *= 0.25f;
        if (rotate_speed > RotateMaxSpeed) rotate_speed = RotateMaxSpeed;
        else if (rotate_speed < -RotateMaxSpeed) rotate_speed = -RotateMaxSpeed;
        //rScene.Message("Speed %f Axis %f %f %f",rotate_speed,rotate_axis.x,rotate_axis.y,rotate_axis.z);
        Debug.LogFormat("final rotate axis: {0}->{1} rotate_speed {2}->{3}", axis_backup, rotate_axis, speed_backup, rotate_speed);
    }
}
public class StormTestClip : Editor
{
    public class PolarComparer : IComparer<Vector3>
    {
        public int Compare(Vector3 a, Vector3 b)
        {
            Vector2 adir = CartToPolar(a, Vector3.zero);
            Vector2 bdir = CartToPolar(b, Vector3.zero);
            if (adir.x == bdir.x) return 0;
            if (adir.x > bdir.x) return 1;
            return -1;

        }
    }
    public class VertexPolarComparer : IComparer<Vertex>
    {
        public int Compare(Vertex a, Vertex b)
        {
            return new PolarComparer().Compare(a.pos, b.pos);
        }
    }

    private static Vector2 CartToPolar(Vector3 point,Vector3 origin)
    {
        Vector3 local = point - origin;

        float r = local.magnitude;
        //local.Normalize() ;
        Vector2 dir = new Vector2(Mathf.Atan2(local.x,local.z)* Mathf.Rad2Deg, Mathf.Atan2(local.y,local.z)* Mathf.Rad2Deg);
        return dir;
    }

    [MenuItem("Clip/Clip plane")]
    public static void ClipPlane()
    {
        geombase.Plane[] planes = new geombase.Plane[]
        {
            new geombase.Plane(Vector3.left,10),
            new geombase.Plane(Vector3.right,10),
        };

        int v_count = 50+1;
        int spread = 25;
        Vertex[] vertices = new Vertex[v_count];
        ushort[] in_i = new ushort[v_count];
        for (int i = 0; i < v_count; i++)
        {
            vertices[i] = new Vertex();
            vertices[i].pos = new Vector3(UnityEngine.Random.Range(-spread, spread), UnityEngine.Random.Range(-spread, spread), UnityEngine.Random.Range(-spread, spread));

            in_i[i] = (ushort) i;
        }

        Array.Sort<Vertex>(vertices,new VertexPolarComparer());

        int v_count_out = 0;
        int num_out_i = 0;

        Vertex[] vertices_out = Alloca.ANewN<Vertex>(((v_count + 2) / 3) * (3 + 6));
        ushort[] out_i = Alloca.ANewN<ushort>(((v_count + 2) / 3) * 64);

        PolyDecal.ClipIndexed(planes,planes.Length,vertices,vertices.Length,in_i,in_i.Length,ref vertices_out, ref v_count_out, ref out_i, ref num_out_i);

        //Visualize,
        Shader shader = Shader.Find("HDRP/Unlit");
        Material red = new Material(shader);
        Material green = new Material(shader);
        Material blue = new Material(shader);
        red.color = Color.red;
        green.color = Color.green;
        blue.color = Color.blue;

        GameObject PlanesStorage = new GameObject("Planes storage");
        GameObject planeGobj;
        MeshRenderer mr;
        foreach (geombase.Plane plane in planes)
        {
            planeGobj = GameObject.CreatePrimitive(PrimitiveType.Plane);
            planeGobj.name = string.Format("Plane {0} {1}", plane.n, plane.d);

            planeGobj.transform.parent = PlanesStorage.transform;
            planeGobj.transform.rotation = Quaternion.LookRotation(Vector3.up, plane.n);
            planeGobj.transform.position = plane.n * (-plane.d);

            mr = planeGobj.GetComponent<MeshRenderer>();
            mr.material = blue;
        }


        GameObject storage = new GameObject("Vectices storage " + v_count_out);
        GameObject myVertex;
        Debug.Log("Drawing vertices: " + v_count_out);
        for (int i=0;i< v_count_out;i++)
        {
            Vertex v = vertices_out[i];
            myVertex = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            myVertex.transform.parent = storage.transform;
            myVertex.transform.position = v.pos;
            myVertex.name = "Clipped " + v.pos;
            mr = myVertex.GetComponent<MeshRenderer>();
            mr.material = green;
        }

        //foreach (Vertex v in vertices)
        //{
        //    myVertex = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //    myVertex.transform.parent = storage.transform;
        //    myVertex.transform.position = v.pos;
        //    myVertex.name = "Original";
        //    mr = myVertex.GetComponent<MeshRenderer>();
        //    mr.material = red;
        //}

        Debug.LogFormat("{0} {1}",new Vector3(-1,0,1), CartToPolar(new Vector3(-1,0,1),Vector3.zero));
    }
}

public class StormTestFileUtils : Editor
{
    [MenuItem("Tests/Files/File")]
    public static void IsFileThere()
    {
        string mypath = "E:/";
        foreach (string tmp in Directory.GetFiles(mypath))
        {
            Debug.Log("file: " + tmp);
        }
    }
}

public class StormTestDelegates : Editor
{
    delegate void meow();
    class Cat
    {
        string color;
        public Cat(string _name)
        {
            name = _name;
        }

        public void Meow()
        {
            Debug.LogFormat("{0} says \"MEOW!\"", name);
        }
        string name;
    }

    [MenuItem("Tests/Delegates")]
    public static void IsFileThere()
    {
        Cat[] cats =
        {
            new Cat("Vasya"),
            new Cat("Murzik"),
            new Cat("Chubais"),
            new Cat("Murka")
        };

        meow say = cats[0].Meow;

        foreach (Cat c in cats)
        {
            say();
        }

    }
}
public class StormTestVector2DtoLongLat : Editor
{
    [MenuItem("Tests/Texture/Texture2LongLat")]
    public static void Texture2LongLat()
    {
        string textureName = "sky1cld";

        TexturesDB tdb = new TexturesDB();
        tdb.Initialize("Graphics\\textures.dat");
        Texture2D tex = tdb.CreateTexture(textureName);
        int startTime = DateTime.Now.Millisecond;
        Texture2D LongLatTexture = Convert2LongLat(tex);

        int endTime = DateTime.Now.Millisecond;
        File.WriteAllBytes("e:/" + textureName + "_LongLat.png", LongLatTexture.EncodeToPNG());
        Debug.Log("Texture converted to LongLat: " + textureName + " time: " + (endTime - startTime) + "ms");
    }

    private static Texture2D Convert2LongLat(Texture2D textureIn)
    {

        Texture2D textureOut = new Texture2D(textureIn.width, textureIn.height);
        int projectedx;
        int w = textureIn.width;
        int h = textureIn.height;
        Color pixel;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                textureOut.SetPixel(x, y, Color.black);
            }
        }
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                pixel = textureIn.GetPixel(x, y);
                projectedx = (int)((w - 1) / 2 + (x - (w - 1) / 2) * Mathf.Cos(Mathf.PI * (y - (h - 1) / 2) / (h - 1)));

                textureOut.SetPixel(projectedx, y, pixel);
            }
        }
        textureOut.Apply();
        return textureOut;
    }
}

public class StormTestText : Editor
{
    [MenuItem("Tests/Text/Utf8Chars")]
    public static void TestUtf8Chars()
    {
        string myString = "Учебный центр";
        Encoding cp1251 = Encoding.GetEncoding("windows-1251");
        byte[] bytes = cp1251.GetBytes(myString);
        Debug.Log(bytes[0]);
        Debug.Log((char)bytes[0]);
    }

}
public class StormTestBObject : Editor
{
    [MenuItem("Tests/BObject/TestBObjectProjection")]
    public static void TestBObjectProjection()
    {
        //(46000.00, 500.00, 36456.00) Seaship
        RO_INFO ri = new RO_INFO();

    }
    [MenuItem("Tests/BObject/TestBObjectSquarization")]
    public static void TestSquarization()
    {
        // HULL 
        //Vectex 0(88.00, 65.00, 1.00)
        //Vectex 1(88.00, 159.00, 1.00)
        //Vectex 2(168.00, 159.00, 1.00)
        //Vectex 3(168.00, 65.00, 1.00)
        BillVertex nw = new BillVertex
        {
            position = new Vector2(88.00f, 65.00f)
        };
        BillVertex ne = new BillVertex
        {
            position = new Vector2(88.00f, 159.00f)
        };
        BillVertex se = new BillVertex
        {
            position = new Vector2(168.00f, 159.00f)
        };
        BillVertex sw = new BillVertex
        {
            position = new Vector2(168.00f, 65.00f)
        };

        UIDrawer.BSubObject subobj = new UIDrawer.BSubObject();
        subobj.AddVertex(se);
        subobj.AddVertex(sw);
        subobj.AddVertex(ne);
        subobj.AddVertex(nw);

        Debug.Log(subobj);

        subobj.RectangularCorrection();
        Debug.Log(subobj);

    }
}

public class StormTestVectors : Editor
{
    //Object in Gunsight! (44000.00, 1000.00, 36556.00)90 dot r/u(70,10824, 183,7013) diff(-489.20, -358.95, 1450.41) r/up((0.93, -0.03, 0.36), (-0.09, 0.94, 0.33))
    [MenuItem("Tests/Vector/TestDot")]
    public static void TestDot()
    {
        Vector3 diff = new Vector3(-489.20f, -358.95f, 1450.41f);
        Vector3 right = new Vector3(0.93f, -0.03f, 0.36f);
        Vector3 up = new Vector3(-0.09f, 0.94f, 0.33f);

        Debug.Log(Vector3.Dot(diff, right));
        Debug.Log(Vector3.Dot(diff, up));

        var CameraAspect = Storm.Math.aspect(Storm.Math.GRD2RD(90f));
    }

}
public class StormTestCRC32 : Editor
{
    [MenuItem("Tests/CRC32/compare")]
    static void CompareOutput()
    {
        CRC32 stormCRC32 = new CRC32();
        string text = "Texture";
        Debug.LogFormat("{0} {1} {2}", text, Hasher.HshString(text).ToString("X8"), stormCRC32.HashString(text).ToString("X8"));
    }
}
public class StormTestAppear : Editor
{
    [MenuItem("Tests/Utils/Sort using method")]
    static void TestSort()
    {
        GROUP_DATA g1 = new GROUP_DATA
        {
            Callsign = "Alpha"
        };
        GROUP_DATA g2 = new GROUP_DATA
        {
            Callsign = "Bravo"
        };
        GROUP_DATA g3 = new GROUP_DATA
        {
            Callsign = "Delta"
        };
        GROUP_DATA g4 = new GROUP_DATA
        {
            Callsign = "Echo"
        };
        GROUP_DATA g5 = new GROUP_DATA
        {
            Callsign = "XRay"
        };

        AppearInfo[] myArray =
        {
            new AppearInfo(g5),
            new AppearInfo(g1),
            new AppearInfo(g4),
            new AppearInfo(g3),
            new AppearInfo(g2),

        };

        Array.Sort(myArray, InstantActionAi.AppearCompare);
        //Array.Sort(myArray);

        foreach (var a in myArray)
        {
            Debug.Log(a.GetName());
        }
    }
}
public class StormTestDataLoad : Editor
{
    [MenuItem("Tests/Load Data/LoadAll")]
    static void LoadAll()
    {
        stormdata_dll.LoadAll();
    }

    [MenuItem("Tests/Load Data/ExportWeapon")]
    static void ExportXML()
    {
        stormdata_dll.LoadAll();
        //var outfile=XmlWriter.Create("Weapons.xml");
        //XmlSerializer xmlObjectSerializer = new XmlSerializer(typeof(List<weaponxml>));
        List<weaponxml> list = new List<weaponxml>();
        //using (StreamWriter outputFile = new StreamWriter("weapons.json"))
        //{
        foreach (var wd in SUBOBJ_DATA.Datas)
        {
            if (wd.GetClass() != SUBOBJ_DATA.SC_WEAPON_SLOT) continue;
            Debug.LogFormat("{0} {1} {2}", wd.FullName, wd.Description, wd.GetClassName());
            var wd_wpn = (WPN_DATA)wd;
            var lxml = new weaponxml
            {
                name = wd_wpn.FullName,
                filename = wd_wpn.FileName,
                EDamage = wd_wpn.Damage,
                IsHuman = wd_wpn.IsHuman,
                Load = wd_wpn.GetAmmoLoad(),
                ReloadTime = wd_wpn.GetReload(),
                Speed = wd_wpn.GetSpeed(),
                LifeTime = wd_wpn.GetLifeTime(),
                Xdamage = wd_wpn.XDamage,
                Xradius = wd_wpn.XRadius
            };
            //string json = JsonUtility.ToJson(lxml);
            //outputFile.Write(json);

            list.Add(lxml);
        }
        //}
        weaponwrapper weaponwrapper = new weaponwrapper(list.Count);
        foreach (var weapon in list)
        {
            weaponwrapper.Add(weapon);
        }

        string json = JsonUtility.ToJson(weaponwrapper);
        using (StreamWriter outputFile = new StreamWriter("weapons.json"))
        {
            outputFile.Write(json);
        }
        //outfile.Flush();
        stormdata_dll.ReleaseAll();
    }

    [Serializable]
    class weaponwrapper
    {
        public weaponxml[] weapons;
        int pos;

        public weaponwrapper(int size)
        {
            weapons = new weaponxml[size];
            pos = 0;
        }

        public bool Add(weaponxml weapon)
        {
            if (pos >= weapons.Length) return false;
            weapons[pos++] = weapon;
            return true;
        }
    }
    [Serializable]
    public struct weaponxml
    {
        public string name;
        public string filename;
        public float EDamage; //энергетический damage
        public bool IsHuman;//может нести игрок
        public float Load;// кол-во выстрелов
        public float ReloadTime;// промежуток между выстрелами
        public float Armor; // броня
        public float Speed;// скорость полета выстрела
        public float LifeTime; // время жизни выстрела
        public float Xdamage;// повреждения по области
        public float Xradius;//радиус повреждения по области
    }
}
public class StormTestMatrix2D : Editor
{
    private class uData
    {
        public float x, y;
        public float w, h;
        public float CurrSize;
        public bool changed;
        public float state;
        public float speed;
    }

    [MenuItem("Tests/Matrix2D/TestRotate")]
    public static void TestRotate()
    {
        BillVertex[] bv = new BillVertex[4];
        bv[0] = new BillVertex();
        bv[1] = new BillVertex();
        bv[2] = new BillVertex();
        bv[3] = new BillVertex();

        bv[0].position = new Vector2(1, 1);
        bv[1].position = new Vector2(1, 3);
        bv[2].position = new Vector2(3, 3);
        bv[3].position = new Vector2(3, 1);

        Matrix2D m = new Matrix2D();
        m.Identity();
        m.Rotate(Storm.Math.GRD2RD(20f));
        for (int i = 0; i < 4; i++)
        {
            bv[i].position = m.TransformPoint(bv[i].position);
        }

        var gobj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gobj.name = "Pivot";
        gobj.transform.position = new Vector3(0, 0, 0);
        for (int i = 0; i < 4; i++)
        {

            Debug.Log(string.Format("Pos[{0}] {1}", i, bv[i].position));
            gobj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gobj.transform.position = new Vector3(bv[i].position.x * 5, bv[i].position.y * 5, 0);
            gobj.name = "Pos " + i;
        }
    }

    [MenuItem("Tests/Matrix2D/TestDesktop")]
    public static void TestDesktop()
    {
        uData Data = new uData();
        Data.changed = true;
        Data.x = 0;
        Data.y = 0;
        Data.w = 1;
        Data.h = .75f;
        Data.state = 0;
        Data.speed = 1;
        //Data.CurrSize = 0;
        //Data.CurrSize = .5f;
        Data.CurrSize = 1f;
        int cnt = 0;

        Matrix2D MInit = new Matrix2D();
        float qx = 5f;
        MInit.Identity();
        Debug.Log(string.Format("{0} {1} {2} {3}", cnt++, "Identity", null, MInit));
        MInit.Move(Data.x, Data.y);
        Debug.Log(string.Format("{0} {1} {2} {3}", cnt++, "Move", (Data.x, Data.y), MInit));
        MInit.Scale(Data.w, Data.h / 0.75f);
        Debug.Log(string.Format("{0} {1} {2} {3}", cnt++, "Scale", (Data.w, Data.h / 0.75f), MInit));
        MInit.Move(0.5f, .375f);
        Debug.Log(string.Format("{0} {1} {2} {3}", cnt++, "Move", (0.5f, .375f), MInit));
        MInit.Scale(((qx - 1f) / qx + Data.CurrSize / qx), Data.CurrSize);
        Debug.Log(string.Format("{0} {1} {2} {3}", cnt++, "Scale", (((qx - 1f) / qx + Data.CurrSize / qx), Data.CurrSize), MInit));
        MInit.Move(-0.5f, -.375f);
        Debug.Log(string.Format("{0} {1} {2} {3}", cnt++, "Move", (-0.5f, -.375f), MInit));

        //GunSight Vertex 0(-0.05, -0.05)
        //GunSight Vertex 1(-0.05, -0.05)
        //GunSight Vertex 2(-0.05, -0.05)
        //GunSight Vertex 3(-0.05, -0.05)


        Debug.Log(MInit.TransformPoint(new Vector3(-0.05f, -0.05f)));
    }

    [MenuItem("Tests/Matrix2D/TestWeapon")]
    public static void TestWeapon()
    {
        Bill bill = new Bill();

        uData Data = new uData();
        Data.changed = true;
        Data.x = 0;
        Data.y = 0;
        Data.w = 1;
        Data.h = .75f;
        Data.state = 0;
        Data.speed = 1;
        //Data.CurrSize = 0;
        //Data.CurrSize = .5f;
        Data.CurrSize = 1f;
        int cnt = 0;

        Matrix2D MInit = new Matrix2D();
        float qx = 5f;
        MInit.Identity();
        Debug.Log(string.Format("{0} {1} {2} {3}", cnt++, "Identity", null, MInit));
        MInit.Move(Data.x, Data.y);
        Debug.Log(string.Format("{0} {1} {2} {3}", cnt++, "Move", (Data.x, Data.y), MInit));
        MInit.Scale(Data.w, Data.h / 0.75f);
        Debug.Log(string.Format("{0} {1} {2} {3}", cnt++, "Scale", (Data.w, Data.h / 0.75f), MInit));
        MInit.Move(0.5f, .375f);
        Debug.Log(string.Format("{0} {1} {2} {3}", cnt++, "Move", (0.5f, .375f), MInit));
        MInit.Scale(((qx - 1f) / qx + Data.CurrSize / qx), Data.CurrSize);
        Debug.Log(string.Format("{0} {1} {2} {3}", cnt++, "Scale", (((qx - 1f) / qx + Data.CurrSize / qx), Data.CurrSize), MInit));
        MInit.Move(-0.5f, -.375f);
        Debug.Log(string.Format("{0} {1} {2} {3}", cnt++, "Move", (-0.5f, -.375f), MInit));

        bill.PushTrasform(MInit);

        cnt = 0;
        uData WData = new uData();
        WData.x = 0.775f;
        WData.y = 0.001f;
        Matrix2D M = new Matrix2D();
        M.Identity();
        Debug.Log(string.Format("{0} {1} {2} {3}", cnt++, "Identity", null, M));
        M.Move(WData.x, WData.y);
        Debug.Log(string.Format("{0} {1} {2} {3}", cnt++, "Move", (WData.x, WData.y), M));
        bill.PushTrasform(M);

        Debug.Log("0,0" + M.TransformPoint(new Vector2(0.0f, 0.0f)));
        Debug.Log("center" + M.TransformPoint(new Vector2(0.5f, 0.75f / 2f)));
        Debug.Log(bill.DumpTransformStack());
        Debug.Log(FullStackTransformPoint(new Vector2(0.775f, 0.001f), bill));
        Debug.Log(FullStackTransformPoint(new Vector2(0.5f, 0.75f / 2f), bill));
        //Debug.Log(MInit.TransformPoint(new Vector3(-0.05f, -0.05f)));

    }

    private static Vector2 FullStackTransformPoint(Vector2 point, Bill bill)
    {
        var ClonedStack = new Stack<Matrix2D>(bill.m_TransformStack);
        Vector2 res = point;
        Matrix2D ma;
        while (ClonedStack.Count > 0)
        {
            ma = ClonedStack.Pop();
            res = ma.TransformPoint(res);
        }
        return res;
    }
}

public class StormTestCollisions : Editor
{
    [MenuItem("Tests/Collisions/LoadCollisions")]
    public static void TestCollisionLoader()
    {

    }

    [MenuItem("Tests/Collisions/HashINc")]
    public static void TestHash()
    {
        int[] test = new int[3];
        for (int i = 0; i < 3; i++)
        {
            test[i] = 0;
        }
        int a = ++test[1];
        Debug.LogFormat("One: {0}", a);
        //Debug.LogFormat("One: {0}", test[1]);
    }
}

public class StormTestCPOINTS : Editor
{
    [MenuItem("Tests/CPOINTS/arrayTest")]
    public static void arraytest()
    {
        CPOINT[] CPoints = new CPOINT[8 * 8];
        int[] CNext = new int[8 * 8];
        int[] CPrev = new int[8 * 8];

        CPointsList TopList = new CPointsList(CPoints, CNext, CPrev);
        CPointsList BottomList = new CPointsList(CPoints, CNext, CPrev, 8);
        CPointsList RightList = new CPointsList(CPoints, CNext, CPrev, 16);
        CPointsList LeftList = new CPointsList(CPoints, CNext, CPrev, 24);
        CPointsList l = new CPointsList(CPoints, CNext, CPrev, 32);

        AddVectorToList(LeftList, Vector3.left);
        AddVectorToList(LeftList, Vector3.left + Vector3.down);
        AddVectorToList(BottomList, Vector3.down);
        AddVectorToList(BottomList, Vector3.down + Vector3.right);
        AddVectorToList(RightList, Vector3.right);
        AddVectorToList(RightList, Vector3.right + Vector3.up);
        AddVectorToList(TopList, Vector3.up);
        AddVectorToList(TopList, Vector3.up + Vector3.left);

        for (int i = 0; i < CPoints.Length; i++)
        {
            Debug.LogFormat("{0} CPOINT {1}", i, CPoints[i] == null ? "Null" : CPoints[i]);
        }

    }
    private static void AddVectorToList(CPointsList l, Vector3 v)
    {
        l.AddTail(new CPOINT(v, 0));
    }
}
public class StormTest : Editor
{
    public static Renderer mpRenderer;
    public static IFpoLoader mpFpoLoader;
    private static bool IsLoaded = false;
    private static List<FPO> FPOS = new List<FPO>();

    //[MenuItem("Tests/GUI/Map")]
    //static void GenerateMapAsset()
    //{
    //    TexturesDB tdb = new TexturesDB();
    //    tdb.Initialize("Graphics\\textures.dat");
    //    MapData md = new MapData();
    //    md.MapSizeX = 11;
    //    md.MapSizeY = 12;
    //    md.SizeX = 11 * SQUARE_SIZE * BOXES_PAGE_SIZE * SQUARES_IN_BOX;
    //    md.SizeY = 12 * SQUARE_SIZE * BOXES_PAGE_SIZE * SQUARES_IN_BOX;
    //    md.Name = "ContMap";


    //    Largemap lm = new Largemap(new Bill(), md, tdb);

    //    lm.GenMap(40000, 40000, 7000, 7000, Color.green);
    //    //tdb.Destroy();

    //}
    [MenuItem("Tests/Texture/texture2bump")]
    static void GenerateMapAsset()
    {
        //string textureName = "CITYGRND";
        //string textureName = "terr_forest1";
        string textureName = "water0000";
        //string textureName = "stonem";
        //string textureName = "ha_bf2";
        //string textureName = "ha_bf1";

        TexturesDB tdb = new TexturesDB();
        tdb.Initialize("Graphics\\textures.dat");
        Texture2D tex = tdb.CreateTexture(textureName);
        Texture2D HeightMapTexture = HeightMap2NormalMap(tex);
        int startTime = DateTime.Now.Millisecond;
        Texture2D NormalMap = HeightMap2NormalMap(HeightMapTexture);
        int endTime = DateTime.Now.Millisecond;
        File.WriteAllBytes("e:/" + textureName + "_NormalMap.png", NormalMap.EncodeToPNG());
        Debug.Log("Texture converted to NormalMap: " + textureName + " time: " + (endTime - startTime) + "ms");
    }

    private static Texture2D HeightMap2NormalMap(Texture2D HeightMapTexture)
    {
        return HeightMap2NormalMapTexture(HeightMapTexture);
        //return HeightMap2NormalMapArray(HeightMapTexture);
    }

    private static Texture2D HeightMap2NormalMapArray(Texture2D HeightMapTexture)
    {
        int width = HeightMapTexture.width;
        int height = HeightMapTexture.height;
        //Texture2D res = new Texture2D(width, height, TextureFormat.RGB24, false);
        Texture2D res = new Texture2D(width, height);
        Color gu, gd, gl, gr;
        //Color rescolor = new Color();
        float dx, dy;
        //int MAGIC = -33;
        int MAGIC = -3;
        Color[] colors = HeightMapTexture.GetPixels();
        Color[] colorsout = new Color[height * width];
        Color rescolor;
        int offset, offsetu, offsetd, offsetl, offsetr;
        for (int y = 0; y < height; y++)
        {
            offsetu = y == 0 ? (height - 1) * width : (y - 1) * width;
            offsetd = y == height - 1 ? 0 : (y + 1) * width;
            offset = y * width;
            for (int x = 0; x < width; x++)
            {
                offsetl = x == 0 ? width - 1 : x - 1;
                offsetr = x == width - 1 ? 0 : x + 1;
                rescolor = new Color();
                gu = colors[x + offsetu];
                gd = colors[x + offsetd];
                gl = colors[offsetl + offset];
                gr = colors[offsetr + offset];
                //gu = HeightMapTexture.GetPixel(x, (int)Mathf.Repeat(y - 1, height - 1));
                //gd = HeightMapTexture.GetPixel(x, (int)Mathf.Repeat(y + 1, height - 1));
                //gl = HeightMapTexture.GetPixel((int)Mathf.Repeat(x - 1, width - 1), y);
                //gr = HeightMapTexture.GetPixel((int)Mathf.Repeat(x + 1, width - 1), y);


                //dx = (gl.r - gr.r) / 255.0f * 0.5f + 0.5f;
                //dy = (gu.r - gd.r) / 255.0f * 0.5f + 0.5f;

                //float dxdiff = (gl.r - gr.r);
                //float dydiff = (gu.r - gd.r);
                //dx = dxdiff < 0.05 ? 0.5f : dxdiff + 0.5f;
                //dy = dydiff < 0.05 ? 0.5f : dydiff + 0.5f;


                dx = (gl.r - gr.r) + 0.5f;
                dy = (gu.r - gd.r) + 0.5f;
                dx = 1.0f / (1.0f + Mathf.Exp(MAGIC * (dx - 0.5f)));
                dy = 1.0f / (1.0f + Mathf.Exp(MAGIC * (dy - 0.5f)));
                rescolor.r = dx;
                rescolor.g = 1 - dy;
                rescolor.b = 1;
                colorsout[x + offset] = rescolor;
                //Debug.Log(string.Format("{0}:{1} {2}",x,y,rescolor));
            }
        }
        res.SetPixels(colorsout);
        res.Apply();
        return res;
    }
    private static Texture2D HeightMap2NormalMapTexture(Texture2D HeightMapTexture)
    {
        int width = HeightMapTexture.width;
        int height = HeightMapTexture.height;
        //Texture2D res = new Texture2D(width, height, TextureFormat.RGB24, false);
        Texture2D res = new Texture2D(width, height);
        Color gu, gd, gl, gr;
        //Color rescolor = new Color();
        float dx, dy;
        //int MAGIC = -33;
        int MAGIC = -3;
        int offset, offsetu, offsetd, offsetl, offsetr;
        Color rescolor;
        for (int y = 0; y < height; y++)
        {
            offset = y;
            offsetu = y == 0 ? height - 1 : y - 1;
            offsetd = y == height - 1 ? 0 : y + 1;
            for (int x = 0; x < width; x++)
            {
                rescolor = new Color();
                offsetl = x == 0 ? width - 1 : x - 1;
                offsetr = x == width - 1 ? 0 : x + 1;
                //gu = HeightMapTexture.GetPixel(x, (int)Mathf.Repeat(y - 1, height - 1));
                //gd = HeightMapTexture.GetPixel(x, (int)Mathf.Repeat(y + 1, height - 1));
                //gl = HeightMapTexture.GetPixel((int)Mathf.Repeat(x - 1, width - 1), y);
                //gr = HeightMapTexture.GetPixel((int)Mathf.Repeat(x + 1, width - 1), y);
                gu = HeightMapTexture.GetPixel(x, offsetu);
                gd = HeightMapTexture.GetPixel(x, offsetd);
                gl = HeightMapTexture.GetPixel(offsetl, y);
                gr = HeightMapTexture.GetPixel(offsetr, y);


                //dx = (gl.r - gr.r) / 255.0f * 0.5f + 0.5f;
                //dy = (gu.r - gd.r) / 255.0f * 0.5f + 0.5f;

                //float dxdiff = (gl.r - gr.r);
                //float dydiff = (gu.r - gd.r);
                //dx = dxdiff < 0.05 ? 0.5f : dxdiff + 0.5f;
                //dy = dydiff < 0.05 ? 0.5f : dydiff + 0.5f;


                dx = (gl.r - gr.r) + 0.5f;
                dy = (gu.r - gd.r) + 0.5f;
                dx = 1.0f / (1.0f + Mathf.Exp(MAGIC * (dx - 0.5f)));
                dy = 1.0f / (1.0f + Mathf.Exp(MAGIC * (dy - 0.5f)));
                rescolor.r = dx;
                rescolor.g = 1 - dy;
                rescolor.b = 1;
                //Debug.Log(string.Format("{0}:{1} {2}",x,y,rescolor));
                res.SetPixel(x, y, rescolor);
            }
        }
        res.Apply();
        return res;
    }
    private static Texture2D Diffuse2HeightMap(Texture2D diffusetexture)
    {

        int width = diffusetexture.width;
        int height = diffusetexture.height;
        Texture2D res = new Texture2D(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var c = diffusetexture.GetPixel(x, y);
                float Gray = (c.r + c.g + c.b) / 3;
                res.SetPixel(x, y, new Color(Gray, Gray, Gray));
            }
        }
        res.Apply();
        return res;
    }
    private static Texture2D UncompressTexture(Texture2D compressedTexture)
    {
        Texture2D res = new Texture2D(compressedTexture.width, compressedTexture.height);
        for (int y = 0; y < compressedTexture.height; y++)
        {
            for (int x = 0; x < compressedTexture.width; x++)
            {
                res.SetPixel(x, y, compressedTexture.GetPixel(x, y));
            }
        }
        res.Apply();
        return res;

    }


    [MenuItem("Tests/Hangar generator")]
    static void TestHangarGeneration()
    {
        stormdata_dll.LoadAll();

        //string SpawnName = "Tank Hangar";
        string SpawnName = "Human Base 5 1";

        STATIC_DATA cd = (STATIC_DATA)OBJECT_DATA.GetByName(SpawnName);

        Debug.Log(cd);

        SUBOBJ_DATA root = cd.RootData;
        Debug.Log(root);

        stormdata_dll.ReleaseAll();
    }

    public class FakeCommand : IVmCommand
    {
        public void AddRef()
        {
            throw new System.NotImplementedException();
        }

        public string describeParams(ref string myparams)
        {
            throw new System.NotImplementedException();
        }

        public string getName()
        {
            throw new System.NotImplementedException();
        }

        public IParamList getParamList()
        {
            throw new System.NotImplementedException();
        }

        public int getType()
        {
            throw new System.NotImplementedException();
        }

        public bool isParsingCorrect()
        {
            throw new System.NotImplementedException();
        }

        public void onCreate()
        {
            throw new System.NotImplementedException();
        }

        public void onOverride()
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

        public bool run()
        {
            throw new System.NotImplementedException();
        }
    }

    public class FakeController : IVmController
    {
        public void AddRef()
        {
            throw new System.NotImplementedException();
        }

        public string describeParams(ref string myparams)
        {
            throw new System.NotImplementedException();
        }

        public uint getID()
        {
            throw new System.NotImplementedException();
        }

        public string getName()
        {
            throw new System.NotImplementedException();
        }

        public IParamList getParamList()
        {
            throw new System.NotImplementedException();
        }

        public bool getResume()
        {
            throw new System.NotImplementedException();
        }

        public int getType()
        {
            throw new System.NotImplementedException();
        }

        public bool isParsingCorrect()
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

        public void restart()
        {
            throw new System.NotImplementedException();
        }

        public void setID(uint id)
        {
            throw new System.NotImplementedException();
        }

        public void shutdown()
        {
            throw new System.NotImplementedException();
        }
    }
    public class FakeFactory : IVmFactory
    {
        public void AddRef()
        {
            throw new System.NotImplementedException();
        }

        public IVmCommand createCommand(uint name, IVmVariablePool pool = null, IVmController cont = null)
        {
            Debug.Log("Creating Command " + name.ToString("X8"));
            return new FakeCommand();
        }

        public IVmController createController(uint name)
        {
            Debug.Log("Creating Controller " + name.ToString("X8"));
            return new FakeController();
        }

        public int RefCount()
        {
            throw new System.NotImplementedException();
        }

        public int Release()
        {
            throw new System.NotImplementedException();
        }
    }

    //[MenuItem("Tests/AI/AiScript")]
    //static void TestAiScript()
    //{
    //    UNIT_DATA ud = new UNIT_DATA();
    //    //ud.AiScript = "OnAppear() {\r\n Patrol(Marker=Navy_Marker,Height=500,Dist=2000);\r\n}\r\n";
    //    //ud.AiScript = "//----------------------------\r\n//     Появление группы\r\n\r\nAppear(Takeoff=\"Fleet Oscar\");\r\n\r\nAddPriority(Group=Enemy_Vulcan,Coeff=-5);\r\nAddPriority(Group=Enemy_Vulcan_PVO,Coeff=-5);\r\nAddPriority(Group=Enemy_Vulcan,Coeff=-5);\r\nAddPriority(Group=\"Base Whiskey Detail A\",Coeff=-5);\r\nAddPriority(Group=\"Base Whiskey\",Coeff=-5);\r\nAddPriority(Group=\"Enemy_Trash_Roads\",Coeff=-5);\r\nAddPriority(Group=\"Enemy_Trash_Road_2\",Coeff=-5);\r\nAddPriority(Group=\"Enemy_Air_Patroll_1\",Coeff=-5);\r\nAddPriority(Group=\"Enemy_Brige_Sec\",Coeff=-5);\r\nAddPriority(Group=\"Enemy_Brige_Sec\",Coeff=-5);\r\nAddPriority(Group=\"Enemy_Air_Patroll_2\",Coeff=-5);\r\nAddPriority(Group=\"Enemy_Vulkan_Road_Musor\",Coeff=-5);\r\n\r\n// End Появление группы\r\n//----------------------------\r\n//     Путь\r\n\r\n// End Путь\r\n//----------------------------\r\n//     Атака блок поста\r\n\r\nOnMessage(Code=Ataka_na_Enemy_Block_Post_Potvergdenie) {\r\n AddPriority(Group=Enemy_Block_Post_PVO,Coeff=10000);\r\n AddPriority(Group=Enemy_Block_Post,Coeff=9000);\r\n StopEscort(); \r\n SetSpeed(Speed=450);\r\n RouteTo (Marker=EBP,Clear=1);\r\n SetSpeed(Speed=450);\r\n}\r\n\r\nOnReach(Marker=EBP) {\r\n Delay(Time=5);\r\n NotifyAll(Code=EBP_Doctignyt);\r\n} \r\n\r\nOnMessage(Code=EBP_Doctignyt_Potvergdenie,Count=0) {\r\n SetSpeed(Speed=450);\r\n RouteTo (Marker=EBP,Clear=1);\r\n SetSpeed(Speed=450);\r\n}\r\n\r\n// End Атака блок поста\r\n//----------------------------\r\n//     Захват превосходства в воздухе\r\n\r\nOnMessage(Code=Vector_Ataki_Na_Prikrytie) {\r\n Escort(Group=Enemy_Air_Patroll_3);\r\n AddPriority(Group=Enemy_Air_Patroll_3,Coeff=10000);\r\n PriorityGroup(Enemy_Air_Patroll_3);\r\n SetSpeed(Speed=450);\r\n}\r\n\r\n// End Захват превосходства в воздухе\r\n//----------------------------\r\n//     Операция по захвату транспорта\r\n\r\n\r\nOnMessage(Code=Operaci__Po_Zaxvaty_Nacat_) {\r\n StopEscort();\r\n SetSpeed(Speed=450);\r\n RouteTo(Marker=Vulcan_2,Clear=1);\r\n SetSpeed(Speed=450);\r\n AddPriority(Group=Enemy_Vulcan_Gate,Coeff=8000);\r\n AddPriority(Group=Enemy_Convoy_Tr,Coeff=9000);\r\n AddPriority(Group=Golf,Coeff=10000);\r\n AddPriority(Group=Enemy_Zasada,Coeff=8000);\r\n}\r\n\r\nOnReach(Marker=Vulcan_2) {\r\n NotifyAll(Code=Vulcan_2_Doctignyt);\r\n}\r\n\r\nOnMessage(Code=Vulcan_2_Doctignyt_Potvergdenie) {\r\n SetSpeed(Speed=450);\r\n RouteTo(Marker=Vulcan_3,Clear=1);\r\n SetSpeed(Speed=450); \r\n}\r\n\r\nOnReach(Marker=Vulcan_3) {\r\n Delay(Time=5);\r\n RouteTo(Marker=Vulcan_3,Clear=1);\r\n}\r\n\r\nOnMessage(Code=Go_Enemy_Transport_S_Antennoy_Alpha) {\r\n Escort(Group=Golf);\r\n}\r\n\r\nOnMessage(Code=Transport_S_Antennoy_Cocpit_Down) {\r\n  AddPriority(Group=Golf,Coeff=-5);\r\n SendMessage(Recipient=\"Zulu\",Code=Transport_S_Antennoy_Begin_Capt,Critical=1,ToAll=1);\r\n}\r\n\r\nOnMessage(Code=Transport_S_Antennoy_Nacat__Zaxvatili) {\r\n Escort(Group=Zulu,Dist=3000, Height=100, Delta=400);\r\n}\r\n\r\n// End Операция по захвату транспорта\r\n//----------------------------\r\n//     Возвращение на базу\r\n\r\n// End Возвращение на базу\r\n//----------------------------\r\n//----------------------------\r\n//     Провал мисси\r\n\r\nOnMessage(Code=Proval_Micci_Yes) {\r\n OnReach(Marker=Navy_Marker) {\r\n  Disappear(Base=\"Fleet Oscar\");\r\n }\r\n StopEscort(); \r\n RouteTo(Marker=Navy_Marker,Clear=1);\r\n}\r\n\r\nOnMessage(Code=Zulu_Vernylc__Na_Bazy) {\r\n  OnReach(Marker=Navy_Marker) {\r\n   Disappear(Base=\"Fleet Oscar\");\r\n  }\r\n  StopEscort(); \r\n  RouteTo(Marker=Navy_Marker,Clear=1);\r\n}\r\n\r\n// End Провал мисси\r\n//----------------------------\r\n\r\n\r\n";
    //    //ud.AiScript = "OnMessage(Code=OnGroupAppear, Caller=Yankee) {\n  Delay(Time=3);\n  Appear(Base=\"Training Center\");\n  AttackRadius(Radius=0);\n  RandomBounds(Value=1);\n}\nOnMessage(Code=PlayerAppeared) {\n\n  OnMessage(Code=PlayerReachedStart) {\n\n    OnMessage(Code=PlayerReached1) {\n\n      OnMessage(Code=PlayerReached3) {\n\n        OnMessage(Code=PlayerReachedEnd) {\n          NotifyAll(Code=FromTrainer6V1Notify);\n          Pause(Time=0);\n          SendMessage(Caller=#, ToAll=1,Code=FromTrainer6V1);\n        }\n        SendMessage(Caller=#, ToAll=1,Code=FromTrainer5V1);\n      }\n      SendMessage(Caller=#, ToAll=1,Code=FromTrainer4V1);\n      NotifyAll(Code=FromTrainer4V1Notify);\n    }\n    SendMessage(Caller=#, PostTime=2, ToAll=1, Critical=1, Code=FromTrainer2V1);\n    Escort(Group=Alpha,Delta=50);\n  }\n  SendMessage(Caller=#, ToAll=1,Code=FromTrainer1V1);\n  \n}\n\n\nOnMessage(Code=FromTC2V10Notify) { // Cover cadets\n  Delay(Time=5);\n  Resume();\n  SendMessage(Caller=#, WaitTime=20, Critical=1, Code=FromTrainer8V1); // Cadets return!\n  NotifyAll(Code=FromTrainer8V1Notify);\n  AttackRadius(Radius=100000);\n  AddPriority(Group=VHVY,Coeff=3);\n  AddPriority(Group=VEscort,Coeff=1);\n  AddPriority(Group=VAlpha,Coeff=10);\n  AddPriority(Group=VZulu,Coeff=10);\n}\n\nOnMessage(Code=FromTC3V10Notify) {\n  RouteTo (Marker=Base);\n}\n\nOnMessage(Code=FromTC7V10Notify) {\n  SendMessage(Caller=#, Critical=1, Code=FromTrainer11V1); // Land Cadets\n}\n\nOnMessage(Code=FromTC9V10Notify) {\n  SendMessage(Caller=#, Critical=1, Code=FromTrainer13V1); // Cadets dead\n}";
    //    ud.AiScript = "SetFormation(Name=Coloumn,Dist=4);\nAutoMessages(Group=1,Unit=1);";
    //    Debug.Log(ud.AiScript);
    //    //AiScriptParser.parceAiScript(ud.AiScript,new VmSeqDebug(),null,new DebugLog());
    //    //AiScriptParser.parceAiScript(ud.AiScript, new ThreadData(), StdGroupFactory.createStdGroupFactory(new StdCraftGroupAi()), new DebugLog());
    //    AiScriptParser.parceAiScript(ud.AiScript, new ThreadData(), new StdTankGroupFactory(null,null), new DebugLog());
    //}

    //[MenuItem("Tests/AI/putils")]
    //static void TestPutils()
    //{
    //    Debug.Log(string.Format("[{0}]", putils.next_char("aab")));
    //    Debug.Log(string.Format("[{0}]", putils.next_char(" aab")));
    //    Debug.Log(string.Format("[{0}]", putils.next_char("                        aab")));
    //    Debug.Log(string.Format("[{0}]", putils.next_char(" \naab")));
    //    Debug.Log(string.Format("[{0}]", putils.next_char("aab\n")));
    //    Debug.Log(string.Format("[{0}]", putils.next_char("= 1\n")));

    //    //string input = "-100.6)";
    //    //StrFloat myStr = new StrFloat(input);
    //    //Debug.Log(string.Format("input {0} output {1}", input, myStr.Value()));
    //}
    [MenuItem("Tests/AI/parsing")]
    static void TestParse()
    {
        //Debug.Log(string.Format("[{0}]", putils.next_char("aab")));
        //Debug.Log(string.Format("[{0}]", putils.next_char(" aab")));
        //Debug.Log(string.Format("[{0}]", putils.next_char("                        aab")));
        //Debug.Log(string.Format("[{0}]", putils.next_char(" \naab")));
        //Debug.Log(string.Format("[{0}]", putils.next_char("aab\n")));
        //Debug.Log(string.Format("[{0}]", putils.next_char("= 1\n")));
        //string data = "OnMessage(Code=FromRC1V6Notify) {\n  OnMessage(Code=FromRC3V6Notify) {\n    Delay(Time=2, Disp=2);\n    SendMessage(Caller=#, WaitTime=1, PostTime=0.1, Critical=1, Code=FromTC4V10);\n  }\n  OnMessage(Code=FromRC7V6Notify) {\n    OnMessage(Code=FromRC3V6Notify) {\n    }\n  }\n  Delay(Time=1, Disp=2);\n  SendMessage(Caller=#, WaitTime=5, PostTime=0.5, Critical=1, Code=FromTC1V10);\n  NotifyAll(Code=FromTC1V10Notify);\n  Delay(Time=1, Disp=2);\n  SendMessage(Caller=#, PostTime=1, Critical=1, Code=FromTC2V10);\n  NotifyAll(Code=FromTC2V10Notify);\n}\n\nOnMessage(Code=FromRC2V6Notify) {\n  Delay(Time=1, Disp=2);\n  SendMessage(Caller=#, WaitTime=5, PostTime=0.5, Critical=1, Code=FromTC3V10);\n  NotifyAll(Code=FromTC3V10Notify);\n  Delay(Time=10, Disp=10);                  // All RTB\n  NotifyAll(Code=FromTCRTB);\n}\n\nOnMessage(Code=AlphaTrainerOk) {\n  SendMessage(Recipient=\"Trainer\", PostTime=4, Critical=1, Code=FromTC5V10);\n  SendMessage(Caller=#, Critical=1, Code=FromTC6V10);\n  SendMessage(Caller=#, PostTime=4, Critical=1, Code=FromTC7V10);\n  NotifyAll(Code=FromTC7V10Notify);\n}\nOnMessage(Code=AlphaDeadTrainerOk) {\n  SendMessage(Recipient=\"Trainer\", PostTime=4, Critical=1, Code=FromTC8V10);\n  SendMessage(Caller=#, PostTime=4, Critical=1, Code=FromTC9V10);\n  NotifyAll(Code=FromTC9V10Notify);\n}}";
        //string data = "\n  OnMessage(Code=FromRC3V6Notify) {\n    Delay(Time=2, Disp=2);\n    SendMessage(Caller=#, WaitTime=1, PostTime=0.1, Critical=1, Code=FromTC4V10);\n  }\n  OnMessage(Code=FromRC7V6Notify) {\n    OnMessage(Code=FromRC3V6Notify) {\n    }\n  }\n  Delay(Time=1, Disp=2);\n  SendMessage(Caller=#, WaitTime=5, PostTime=0.5, Critical=1, Code=FromTC1V10);\n  NotifyAll(Code=FromTC1V10Notify);\n  Delay(Time=1, Disp=2);\n  SendMessage(Caller=#, PostTime=1, Critical=1, Code=FromTC2V10);\n  NotifyAll(Code=FromTC2V10Notify);\n}\n\nOnMessage(Code=FromRC2V6Notify) {\n  Delay(Time=1, Disp=2);\n  SendMessage(Caller=#, WaitTime=5, PostTime=0.5, Critical=1, Code=FromTC3V10);\n  NotifyAll(Code=FromTC3V10Notify);\n  Delay(Time=10, Disp=10);                  // All RTB\n  NotifyAll(Code=FromTCRTB);\n}\n\nOnMessage(Code=AlphaTrainerOk) {\n  SendMessage(Recipient=\"Trainer\", PostTime=4, Critical=1, Code=FromTC5V10);\n  SendMessage(Caller=#, Critical=1, Code=FromTC6V10);\n  SendMessage(Caller=#, PostTime=4, Critical=1, Code=FromTC7V10);\n  NotifyAll(Code=FromTC7V10Notify);\n}\nOnMessage(Code=AlphaDeadTrainerOk) {\n  SendMessage(Recipient=\"Trainer\", PostTime=4, Critical=1, Code=FromTC8V10);\n  SendMessage(Caller=#, PostTime=4, Critical=1, Code=FromTC9V10);\n  NotifyAll(Code=FromTC9V10Notify);\n}}";
        //string cb=putils.CloseBracket(data, '{', '}', 1);
        //Debug.Log("data\t: " + data);
        //Debug.Log("cb\t: " + cb);
        //Debug.Log("int\t: " + putils.CloseBracketPos(data, '{', '}', 1));

        //Debug.Log(string.Format("Res: {0} Exp: {1}", putils.CloseBracketPos("{ gsg }a", '{', '}'), 6));
        //Debug.Log(string.Format("Res: {0} Exp: {1}", putils.CloseBracketPos(" gsg }a", '{', '}', 1), 5));
        //Debug.Log(string.Format("Res: {0} Exp: {1}", putils.CloseBracketPos("{ gsg {}a", '{', '}'), -1));

        //string data = "                  // All RTB\n  NotifyAll(Code=FromTCRTB);\n";
        //string data = "NotifyAll(Code=FromTCRTB);\n";
        //string data = "NotifyAll(Code=AP25);\n\0";
        string data = "SetPlayerPosition(Position=1);\nSetCamera(Mode=\"none\",Vector=(46115.35,186.55,32935),Heading=17.22,Pitch=-36.38);\nAddObjective(Primary=1, Name=TrainObj);\n\nOnMessage(Code=FromTC2V10Notify) { // Cover cadets\n  OnMessage(Code=OnGroupLand,Caller=Alpha) {\n    SetObjective(Name=DefendObj, Success=1);\n  }\n  OnMessage(Code=OnGroupKill,Caller=Alpha) {\n    OnMessage(Code=FromTCRTB) {\n      NotifyAll(Code=AlphaDeadTrainerOk);\n    }\n    SetObjective(Name=DefendObj, Success=0);\n    SetTrigger(Num=1,Value=1);\n  }\n  Delay(Time=3);\n  DeleteObjective(Name=TrainObj);\n  AddObjective(Primary=1, Name=DefendObj);\n}\n\n\nOnMessage(Code=OnGroupKill,Caller=Alpha) {\n  OnMessage(Code=FromTCRTB) {\n    NotifyAll(Code=AlphaDeadTrainerOk);\n  }\n  SetTrigger(Num=1,Value=1);\n}\n\nOnMessage(Code=OnGroupKill,Caller=Zulu) {\n  SetTrigger(Num=2,Value=1);\n  NotifyAll(Code=TanksKilled);\n}\n\nOnMessage(Code=OnGroupKill,Caller=Trainer) {\n  OnMessage(Code=OnGroupKill,Caller=Alpha) {\n    SetTrigger(Num=1,Value=1);\n  }\n  OnMessage(Code=FromTCRTB) {\n  }\n  SetTrigger(Num=3,Value=1);\n}\n\nOnMessage(Code=OnGroupKill,Caller=Charlie) {\n  SetTrigger(Num=4,Value=1);\n}\n\nOnMessage(Code=OnGroupKill,Caller=Bravo) {\n  SetTrigger(Num=5,Value=1);\n}\n\nOnTriggers(T1=1,T2=1,T3=1,T4=1,T5=1) {\n  NotifyAll(Code=HumansKilled);\n}\n\n\nOnMessage(Code=OnGroupKill,Caller=VAlpha) {\n  SetTrigger(Num=6,Value=1);\n}\n\nOnMessage(Code=OnGroupKill,Caller=VZulu) {\n  SetTrigger(Num=7,Value=1);\n}\n\nOnMessage(Code=OnGroupKill,Caller=VHVY) {\n  SetTrigger(Num=8,Value=1);\n  NotifyAll(Code=VHVYKilled);\n}\n\nOnMessage(Code=OnGroupKill,Caller=VEscort) {\n  SetTrigger(Num=9,Value=1);\n}\n\nOnTriggers(T6=1,T7=1,T8=1,T9=1) {\n  NotifyAll(Code=VeliansKilled);\n}\n\nOnMessage(Code=WhichWay) {\n  RandomCode(Code=P1, Code=P2, Code=P3, Code=P1, Code=P2, Code=P3, Code=P1, Code=P2, Code=P3);\n}\n\n\nOnMessage(Code=PlayerReachedEnd) {\n  Delay(Time=50, Disp=8);\n  NotifyAll(Code=AmbushAttack);\n}\nOnMessage(Code=FromTrainer8V1Notify) {\n  Delay(Time=12, Disp=4);\n  NotifyAll(Code=AmbushAttack);\n}\n\nOnMessage(Code=FromTCRTB) {\n  NotifyAll(Code=AlphaTrainerOk);\n}\n\n\nOnMessage(Code=CheckSkill, Count=0) {\n  NotifyAll(Code=Skill1);\n}\nOnMessage(Code=OnUnitAppear, Caller=\"Trainer\", CallerIndex=2) {\n  OnMessage(Code=CheckSkill, Count=0) {\n    NotifyAll(Code=Skill2);\n  }\n}\nOnMessage(Code=OnUnitAppear, Caller=\"Trainer\", CallerIndex=3) {\n  OnMessage(Code=OnUnitAppear, Caller=\"Trainer\", CallerIndex=2) {\n  }\n  OnMessage(Code=CheckSkill, Count=0) {\n    NotifyAll(Code=Skill3);\n  }\n}\nOnMessage(Code=OnUnitAppear, Caller=\"Trainer\", CallerIndex=4) {\n  OnMessage(Code=OnUnitAppear, Caller=\"Trainer\", CallerIndex=2) {\n  }\n  OnMessage(Code=OnUnitAppear, Caller=\"Trainer\", CallerIndex=3) {\n  }\n  OnMessage(Code=CheckSkill, Count=0) {\n    NotifyAll(Code=Skill4);\n  }\n}\n\n\n\n// QUEST\nDeclareGlobalTrigger(Name=GlobalQuestStage1);\nDeclareGlobalTrigger(Name=GlobalQuestStage2);\nDeclareGlobalTrigger(Name=GlobalQuestStage3);\nDeclareGlobalTrigger(Name=GlobalQuestStage4);\nDeclareGlobalTrigger(Name=GlobalQuestStage5);\nDeclareGlobalTrigger(Name=GlobalQuestStage6);\nDeclareGlobalTrigger(Name=GlobalQuestStage7);\nDeclareGlobalTrigger(Name=GlobalQuestStage5Won);" + '\0';
        //Debug.Log("Trimmed\t: [" + putils.next_char(data)+"]");
        MockAI mai = new MockAI();
        AiScriptParser.parceAiScript(data, new ThreadData(), Factories.createBaseAiFactory(mai), new DebugLog());

        //AiScriptParser.parceAiScript(data, new ThreadData(), new BaseAiFactory(null,null), new DebugLog());
    }

    private class MockAI : IQuery, ITimeService, IRadioSender, IRadioService, IAi
    {
        public void AddRef()
        {
            throw new System.NotImplementedException();
        }

        public IAi getAi()
        {
            return new MockAI();
        }

        public float getDelta()
        {
            throw new System.NotImplementedException();
        }

        public CallerInfo getInfo()
        {
            throw new System.NotImplementedException();
        }

        public IErrorLog getLog()
        {
            throw new System.NotImplementedException();
        }

        public bool getOrg(out Vector3 v)
        {
            throw new System.NotImplementedException();
        }

        public int getSide()
        {
            throw new System.NotImplementedException();
        }

        public float getTime()
        {
            throw new System.NotImplementedException();
        }

        public int getType()
        {
            throw new System.NotImplementedException();
        }

        public int getVoice()
        {
            throw new System.NotImplementedException();
        }

        public bool isPlayable()
        {
            throw new System.NotImplementedException();
        }

        public bool isRadioFree()
        {
            throw new System.NotImplementedException();
        }

        public void notifyRadioMessage(RadioMessage Info)
        {
            throw new System.NotImplementedException();
        }

        public void ProcessRadioMessage(string msg_code, IAi caller, RadioMessage Info, bool to_all = false, bool say_flag = true)
        {
            throw new System.NotImplementedException();
        }

        public object Query(uint cls_id)
        {
            switch (cls_id)
            {
                case ITimeService.ID:
                    return (ITimeService)this;
                case IRadioSender.ID:
                    return (IRadioSender)this;
                case IRadioService.ID:
                    return (IRadioService)this;
            }
            return null;
        }

        public int RefCount()
        {
            throw new System.NotImplementedException();
        }

        public void registerHandler(IRadioHandler rh)
        {
            throw new System.NotImplementedException();
        }

        public int Release()
        {
            throw new System.NotImplementedException();
        }

        public void sendRadioMessage(string msg_code, IAi caller, RadioMessage Info, bool to_all, bool say_flag)
        {
            throw new System.NotImplementedException();
        }

        public void unregisterHandler(IRadioHandler rh)
        {
            throw new System.NotImplementedException();
        }

        public bool Update(float scale)
        {
            throw new System.NotImplementedException();
        }
    }

    [MenuItem("Tests/Matrix/Assign")]
    static void TestMatrix()
    {
        MATRIX a = new MATRIX();
        Debug.Log(a);
        a.Set(Vector3.zero, Vector3.forward, Vector3.up);
        Debug.Log(a);
        MATRIX b = new MATRIX();
        b.Set(a);
        Debug.Log(b);
        Debug.Log(Vec3cross(Vector3.up, Vector3.forward));

    }

    public static Vector3 Vec3cross(Vector3 a, Vector3 v)
    {
        return new Vector3(a.y * v.z - a.z * v.y, a.z * v.x - a.x * v.z, a.x * v.y - a.y * v.x);
    }

    [MenuItem("Tests/Matrix/ExpressPoint")]

    static void testExpressPoint()
    {
        //else GetAngles(ref TgtXangle, ref TgtYangle, ref empty, GetOwner().GetPosition().ExpressPoint(GetTargetOrgEx()));
        MATRIX shooter = new MATRIX();
        MATRIX tgt = new MATRIX(new Vector3(5, 5, 5));

    }
    [MenuItem("Tests/Matrix/RotateSimulation")]
    static void TestRotationValues()
    {
        MATRIX m = new MATRIX();
        m.Set(Vector3.zero, Vector3.forward, Vector3.up);
        float mYangle = 0.03618844f;
        //GetFpo().Dir = mOrgDir;
        //GetFpo().Right = mOrgRight;
        m.TurnRightPrec(mYangle);
        Debug.Log("new DIR " + m.Dir.normalized);
    }
    [MenuItem("Tests/Matrix/Rotate")]
    static void TestRotation()
    {
        MATRIX m = new MATRIX();
        m.Set(Vector3.zero, Vector3.forward, Vector3.up);
        int i;
        long myTime;
        myTime = GetMilliseconds();
        for (i = 0; i < 1000000; i++)
        {
            TurnRightPrecSin(m, 1);
        }
        Debug.Log("Sin time: " + (GetMilliseconds() - myTime) + " ms " + Vector3.Angle(Vector3.forward, m.Dir));

        m.Set(Vector3.zero, Vector3.forward, Vector3.up);

        myTime = GetMilliseconds();
        for (i = 0; i < 1000000; i++)
        {
            TurnRightPrecQuat(m, 1);
        }
        Debug.Log("Quat time: " + (GetMilliseconds() - myTime) + " ms " + Vector3.Angle(Vector3.forward, m.Dir));
    }

    private static long GetMilliseconds()
    {
        return System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
    }
    private static void TurnRightPrecSin(MATRIX m, float angle)
    {
        Vector2 cs = Storm.Math.getSinCos(angle);
        m.Dir = m.Dir * cs.y + m.Right * cs.x;
        m.Right = Vector3.Cross(m.Up, m.Dir);
    }

    private static void TurnRightPrecQuat(MATRIX m, float angle)
    {
        Quaternion q = Quaternion.AngleAxis(Storm.Math.RD2GRD(angle), m.Up);
        m.Dir = q * m.Dir;
        m.Right = Vector3.Cross(m.Up, m.Dir);
        //m.Right = q * m.Right;
    }
    [MenuItem("Tests/Collision/Sphere debug")]
    static void SphereDebug()
    {
        Vector3 camPos = new Vector3(45635.89f, 281.04f, 32195.16f);
        Vector3 camDir = new Vector3(0.50f, -0.37f, 1.14f).normalized;

        Vector3 tgtPos = new Vector3(45970.80f, 85.56f, 32849.61f);
        float tgtRadius = 511.142f;

        Vector3 localTgtPos = tgtPos - camPos;
        Vector3 localCamPos = camPos - camPos;

        GameObject cam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cam.name = "Camera impostor";
        cam.transform.position = localCamPos;

        GameObject tgt = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tgt.name = "Target impostor";
        tgt.transform.position = localTgtPos;
        tgt.transform.localScale = Vector3.one * tgtRadius;

        GameObject ray = new GameObject();
        ray.name = "LOS";
        LineRenderer rayRenderer = ray.AddComponent<LineRenderer>();
        rayRenderer.SetPositions(new Vector3[] { localCamPos, localTgtPos + camDir * 1000 });
        rayRenderer.startWidth = 5;
        rayRenderer.endWidth = 1;
        rayRenderer.startColor = Color.red;
        rayRenderer.endColor = Color.blue;
        rayRenderer.sharedMaterial = cam.GetComponent<MeshRenderer>().sharedMaterial;

        if (SphereIntersection(camPos, camDir, tgtPos, tgtRadius, out Vector3[] res))
        {
            GameObject i1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            i1.name = "intersection close";
            i1.transform.position = res[0] - camPos;

            GameObject i2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            i2.name = "intersection far";
            i1.transform.position = res[1] - camPos;
        }
    }

    private static bool SphereIntersection(Vector3 rayOrg, Vector3 rayDir, Vector3 tgtOrg, float tgtRadius, out Vector3[] res)
    {
        res = new Vector3[2];

        Vector3 L = tgtOrg - rayOrg;

        Debug.Log(string.Format("L: {1} rd: {1}", L, rayDir));
        float tc = Vector3.Dot(L, rayDir);
        if (tc < 0)
        {
            return false;
        }

        float d = Mathf.Sqrt(tc * tc - L.sqrMagnitude);
        if (d > tgtRadius)
        {
            return false;
        }
        Debug.Log(string.Format("d {0} > tgtRadius {1}", d, tgtRadius));
        Debug.Log(string.Format("L.mag {0} tc {1}", L.magnitude, tc));
        var t1c = Mathf.Sqrt(tgtRadius * tgtRadius - d * d);
        var t1 = tc - t1c;
        var t2 = tc + t1c;
        res[0] = rayOrg + rayDir * t1;
        res[1] = rayOrg + rayDir * t2;
        Debug.Log(t1);
        Debug.Log(t2);
        return true;
    }

    [MenuItem("Tests/Commands/Parser")]
    static void ParseCommand()
    {
        Commands cmd = new Commands(new DebugLog(), null, null);
        cmd.RegisterTrigger("cl_bank_right", new FakeCommlink());
        cmd.RegisterCommand("cl_auto_throttle", new FakeCommlink(), 1);
        //cmd.ProcessString("+cl_bank_right;",false);
        cmd.ProcessString("cl_auto_throttle 100;", false);
        cmd.ProcessString("cl_auto_throttle 0;  ".Trim(), false);
    }

    private class FakeCommlink : CommLink
    {
        public void OnTrigger(uint id, bool b)
        {
            Debug.Log("Triggering trigger " + id.ToString("X8") + " " + (b ? "on" : "off"));
        }

        public virtual void OnCommand(uint id, string str1, string str2)
        {
            Debug.Log(string.Format("Commanding command {0} arg1 {1} arr2 {2}", id.ToString("X8"), str1, str2));
        }
    }

    [MenuItem("Tests/Flags/DebugFlag")]
    static void ShowFlag()
    {
        Debug.Log(CampaignDefines.CF_CLASS_STATIC_GROUP + CampaignDefines.CF_APPEAR_MASK);
    }
    [MenuItem("Tests/UDB/Profile Generator")]
    static void TestProfileGenerator()
    {
        UnivarDB profile = new UnivarDB();
        profile.create();
        if (!profile.openRoot()) throw new System.Exception("Failed to create root for profile");

        var myDb = new UnivarDB();
        myDb.open(ProductDefs.getHddFile("GameData.dat"), true);
        Debug.Log("myDb: " + myDb.getDB());
        var mpLocalizationDb = myDb.getDB();
        profile.GetRoot().createString("Callsign", "Джейсон \"Wolf\" Скотт");

        iUnifiedVariableContainer default_options = (iUnifiedVariableContainer)mpLocalizationDb.GetRoot();
        Debug.Log("Default Options: " + (default_options != null ? default_options : "Failed:\n" + mpLocalizationDb));


        UniCopier un = new UniCopier(default_options, profile.GetRoot());
        un.copy("DefaultOptions", "Options");
        un.copy("DefaultControls", "Controls");
        Debug.Log("Generated profile: " + profile);

        iUnifiedVariableContainer mpControls = (UniVarContainer)profile.GetRoot().openContainer("Controls");
        iUnifiedVariableContainer mpOptions = (UniVarContainer)profile.GetRoot().openContainer("Options");
        iUnifiedVariableContainer keys = mpControls.openContainer("Keys");

        iUnifiedVariableContainer actions = keys.openContainer("ActionsList");
        var gdata = myDb.GetRoot().openContainer("ActionsList");

        uint handle = 4;
        iUnifiedVariableArray bk = actions.openArray(handle);
        ((UniVarArray)bk).Dump();
        string bk_name = bk.getNameShort();
        Debug.Log(bk.getNameShort() + " bk.GetSize() " + bk.GetSize());
        Debug.Log(bk);
        iUnifiedVariableContainer key = gdata.openContainer(bk_name);
        Debug.Log("KEy:" + key);
        if (key != null)
        {
            string action = key.getString("Command");
            if (action != null)
            {
                for (uint i = 0; i < bk.GetSize(); i++)
                {
                    Debug.Log(string.Format("Binding {0} to {1}", bk.getString(i), action));
                }
            }
        }
        return;
        //iUnifiedVariableContainer options = profile.GetRoot().createContainer("Options");
        //iUnifiedVariableContainer default_options = mpLocalizationDb.openContainer("\\Root\\DefaultOptions");
        //iUnifiedVariableContainer default_controls = mpLocalizationDb.openContainer("\\Root\\DefaultControls");

        //Debug.Log("Generated profile: " + profile);

        //iUnifiedVariableContainer keys = default_controls.openContainer("Keys");
        //iUnifiedVariableContainer actions = keys.openContainer("ActionsList");
        //uint handle = 4;
        //iUnifiedVariableArray bk = actions.openArray(handle);

        //string bk_name = bk.getNameShort();

        //iUnifiedVariableContainer gdata = myDb.GetRoot().openContainer("ActionsList");
        //Debug.Log(gdata);
        //iUnifiedVariableContainer key = gdata.openContainer(bk_name);
        //Debug.Log(key);

        //Debug.Log(bk);
        //Debug.Log(bk.GetSize());

        //string action = key.getString("Command");
        //for (uint i = 0; i < bk.GetSize(); i++)
        //{
        //    Debug.Log(string.Format("Binding {0} to {1}", bk.getString(i), action));
        //}

    }
    [MenuItem("Tests/Collision/OBB")]
    static void TestOBB()
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "target";
        cube.transform.position = new Vector3(50, 0, 500);
        cube.transform.localScale *= 50;
        cube.transform.rotation = UnityEngine.Random.rotation;

        Vector3[] bounds = new Vector3[]
        {
            new Vector3(-25,-25,-25),
            new Vector3(25,25,25)
        };

        for (int i = 0; i < 3; i++)
        {
            //Vector3 rayOrg = new Vector3(300, 0, 500);
            Vector3 rayOrg = new Vector3(UnityEngine.Random.value * 500, UnityEngine.Random.value * 500, UnityEngine.Random.value * 500);
            Vector3 offset = new Vector3(UnityEngine.Random.value * 50, UnityEngine.Random.value * 50, UnityEngine.Random.value * 50);
            Vector3 rayDir = ((cube.transform.position + offset) - rayOrg).normalized;


            GameObject rayVisualizer = new GameObject("ray " + i);
            rayVisualizer.transform.position = rayOrg;
            LineRenderer lr = rayVisualizer.AddComponent<LineRenderer>();
            lr.SetPositions(new Vector3[] { rayOrg, rayOrg + rayDir * 1000 });
            lr.startWidth = 5;
            lr.endWidth = 1;
            lr.startColor = Color.red;
            lr.endColor = Color.blue;

            if (CollisionUtils.OOBIntersect(rayOrg, rayDir, cube.transform.position, cube.transform.forward, cube.transform.up, bounds, out Vector3[] res))
            {
                GameObject gMin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                gMin.name = "hit close " + i;
                gMin.transform.position = res[0];
                GameObject gMax = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                gMax.name = "hit far " + i;
                gMax.transform.position = res[1];
            }
        }



    }

    [MenuItem("Tests/AVI player")]
    static void PlayVideo()
    {
        Menu.playAvi(new myAviPlayer(), Menu.sIntroAvi);

    }

    [MenuItem("Tests/Sound/GSound")]
    static void GSoundTest()
    {
        Sound snda = new Sound();
        SoundConfig sndcfg = new SoundConfig();
        SoundApi mpSound = new SoundApi();
        mpSound.Initialize(snda, sndcfg, null, null);
        I3DSound snd = mpSound.Create3D(1, true);

        var zvec = Vector3.zero;
        var zvec2 = Vector3.zero;
        I3DSoundEventController ctr = RefSoundCtrWrapper.CreateSoundCtrWrapper(zvec, zvec2, 0xFFFFFFFF);
        I3DSoundEvent DoorSound = snd.LoadEvent("Hangar", "Craft Hangar Big", "Move", true, false, ctr);
        I3DSoundEvent PPCSound = snd.LoadEvent("Weapon", "PPC", "Fire", false, false, ctr);
        PPCSound.Start();
        ((GSound)PPCSound).Play();
    }
    [MenuItem("Tests/Sound/SoundAPI")]
    static void SoundApiTest()
    {
        Sound snda = new Sound();
        SoundConfig sndcfg = new SoundConfig();
        SoundApi mpSound = new SoundApi();
        mpSound.Initialize(snda, sndcfg, null, null);
        I3DSound snd = mpSound.Create3D(1, true);
        var zvec = Vector3.zero;
        var zvec2 = Vector3.zero;
        I3DSoundEventController ctr = RefSoundCtrWrapper.CreateSoundCtrWrapper(zvec, zvec2, 0xFFFFFFFF);
        I3DSoundEvent DoorSound = snd.LoadEvent("Hangar", "Craft Hangar Big", "Move", true, false, ctr);
        //string text = "victor_+1+this_is_+training_center+alpha+zulu+6+8+9+base_+delta_+0";
        //string text = "victor_+1+this_is_+training_center+alpha+zulu+6+8+9";
        string text = "whiskey_+tango_+foxtrot";
        TokenParser mPars = new TokenParser(TokenParser.PhraseSpacial, text);
        string[] tokens = text.Split("+");
        uint[] crcs = new uint[tokens.Length];
        for (int i = 0; i < tokens.Length; i++)
        {
            crcs[i] = Hasher.HshString(tokens[i]);
            Debug.Log(string.Format(tokens[i], crcs[i]));
        }
        VoiceEng ve = new VoiceEng(mpSound);
        var mpVoiceDB = ve.OpenVoiceDB("Voice01.sfx");

        //AudioClip clip = ((SManager)snd).sfx_db.CreatePhrace(crcs, crcs.Length);
        AudioClip clip = ((VoicesDB)mpVoiceDB).CreatePhrace(crcs, crcs.Length);

        Debug.Log("Clip Length " + clip.length);
        GameObject sndsource = new GameObject("Sound Player");
        AudioSource src = sndsource.AddComponent<AudioSource>();
        src.clip = clip;
        src.Play();
    }
    [MenuItem("Tests/Enum datas")]
    static void EnumData()
    {
        stormdata_dll.LoadAll();

        foreach (var Data in OBJECT_DATA.Datas)
        {
            Debug.Log(Data.FullName + " " + Data.GetType());
        }
        //foreach (OBJECT_DATA data in OBJECT_DATA.Datas)
        //{
        //    if ((data.Flags & OBJECT_DATA.OC_CRAFT) != 0) Debug.Log(data.FullName);
        //}

        stormdata_dll.ReleaseAll();
    }
    [MenuItem("Tests/gData to GameObject")]
    static void gDataToGameObject()
    {
        stormdata_dll.LoadAll();

        //string SpawnName = "Human_HVY2";
        string SpawnName = "Human Avia Escort Vessel";
        string SpawnLayoutName = "Turrets Light";

        CARRIER_DATA cd = (CARRIER_DATA)OBJECT_DATA.GetByName(SpawnName);

        Debug.Log(cd.FullName);
        Debug.Log(cd.RootData.FileName);
        //foreach(LAYOUT_DATA layout in cd.Layouts)
        //{
        //    Debug.Log(layout.FullName + " type "+  layout.Type.ToString("X8"));
        //}

        LAYOUT_DATA layout = cd.GetLayout(LAYOUT_DATA.SLOTS_LAYOUT, Hasher.CodeString(SpawnLayoutName));

        Debug.Log(layout.FullName);
        foreach (LAYOUT_ITEM li in layout.Items)
        {
            Debug.Log(li.TextName + " " + li.Name.ToString("X8") + " " + li.Value);
        }

        stormdata_dll.ReleaseAll();
    }

    [MenuItem("Tests/Test UnitData")]
    static void UnitDataTest()
    {
        Init();

        string ObjName = "Human_BF1";
        CRAFT_DATA cd = (CRAFT_DATA)OBJECT_DATA.GetByName(ObjName);

        Debug.Log(string.Format("Unit {0} UnitDataIndex {1}", ObjName, cd.UnitDataIndex));
        Debug.Log(string.Format("Unit {0} UnitDataIndex {1}", ObjName, cd.TM));

        Discard();
    }
    [MenuItem("Tests/Test subobj search")]
    static void SearchSubobj()
    {

        Init();
        WPN_DATA_BARREL cd = (WPN_DATA_BARREL)SUBOBJ_DATA.GetByName("V_BG");
        FPO myFPO = mpFpoLoader.CreateFPO(cd.FileName);
        //CRAFT_DATA cd = (CRAFT_DATA)OBJECT_DATA.GetByName("Human_BF1");
        //string FileName = "Base A_Stern";
        //string FileName = "Base A Stern";
        //string FileName = "BASE A_STERN";
        //string FileName = "RE";

        //Debug.Log("FPO " + myFPO.GetHashCode().ToString("X8"));
        //Debug.Log("RO " + ((RO)myFPO).GetHashCode().ToString("X8"));

        Debug.Log("Pringing subobjs:");
        Debug.Log("Barrel FPO name: " + cd.Barrel.ToString("X8"));
        Debug.Log("Hashstring: " + Hasher.HshString("Gun").ToString("X8"));
        Debug.Log("Codestring: " + ((int)Hasher.CodeString("Gun")).ToString("X8"));
        Debug.Log("By id " + myFPO.GetSubObject(cd.Barrel));
        Debug.Log("By string " + myFPO.GetSubObject("Gun").Name.ToString("X8"));
        foreach (var z in cd.SubobjDatas)
        {
            Debug.Log("Subobjs: " + z.FullName);
            FPO tmpFPO = (FPO)myFPO.GetSubObject(z.FullName);
            if (tmpFPO == null) continue;
        }

        GameObject Gobj = CreateGameObjectFull(myFPO);
        Gobj.transform.rotation = Quaternion.LookRotation(myFPO.Dir, myFPO.Up);
        //StormTest.CreateGameObject(myFPO);
        Discard();
    }

    private class SlotTestEnum : ISlotEnum
    {
        private LAYOUT_DATA SlotsLayout;


        public SlotTestEnum(LAYOUT_DATA ld)
        {
            SlotsLayout = ld;
            Debug.Log("Enumerator created with count " + ld.Items.Count);
        }
        public bool ProcessSlot(SLOT_DATA sld, int slot_id, FPO myFPO)
        {
            //Debug.Log(string.Format("Processing slot using debug placer{0} {1} @ FPO [{2}] {3}", slot_id, sld, myFPO.TextName, myFPO.Name.ToString("X8")));
            if (SlotsLayout == null) return false;

            uint n = Hasher.HshString(new string(sld.Name));

            foreach (LAYOUT_ITEM li in SlotsLayout.Items)
            {
                if (li.Name != n) continue;

                SUBOBJ_DATA so = SUBOBJ_DATA.GetByName(li.Value);
                FPO SubObjFPO = mpFpoLoader.CreateFPO(so.FileName);
                SubObjFPO.TextName = li.Value;
                //SubObjFPO.Link = myFPO;
                //SubObjFPO.Org = sld.Org;
                //SubObjFPO.Dir = sld.Dir;
                //FPOS.Add(SubObjFPO);
                myFPO.AttachObject(SubObjFPO, SubObjFPO.Org, SubObjFPO.Dir, SubObjFPO.Up);
                //FPOS.Add(SubObjFPO);
                //GameObject Gobj = CreateGameObject(SubObjFPO);

                //Gobj.transform.position =sld.Org;
                //Gobj.name = li.Value;
            }
            return true;
        }
    }

    private static void Init()
    {
        if (IsLoaded) return;
        stormdata_dll.LoadAll();
        renderer_dll.Attach();
        FPOS = new List<FPO>();

        LOG output = new LOG();
        mpRenderer = Renderer.CreateRenderer(null, output, null, RendererApi.RENDERER_VERSION);
        mpFpoLoader = (IFpoLoader)FpoLoader.CreateFpoLoader(mpRenderer, null, "objects2.dat", "objects.dat");
        if (mpFpoLoader == null) Debug.Log("FPO loader failed to load itself");
    }
    private static void Discard()
    {
        if (!IsLoaded) return;
        stormdata_dll.ReleaseAll();
    }

    //[MenuItem("Tests/Load Craft")]
    //static void ImportCraft()
    //{
    //    ImportCraftByName("Human_Cargo4");
    //}
    //static void ImportCraftByName(string SpawnName)
    //{
    //    Init();
    //    //string SpawnName = "Human_BF2";
    //    CRAFT_DATA cd = (CRAFT_DATA)OBJECT_DATA.GetByName(SpawnName);

    //    FPO myFPO = mpFpoLoader.CreateFPO(cd.RootData.FileName);
    //    myFPO.TextName = SpawnName;

    //    //DrawFPOs();
    //    GameObject Gobj = DrawFpoParticle.DrawFPO(myFPO);
    //    Gobj.transform.rotation = Quaternion.LookRotation(myFPO.Dir, myFPO.Up);
    //    Discard();
    //}

    [MenuItem("Tests/Particles/TestPD")]
    static void ImportPD()
    {
        //ImportDLL.LoadDLLS();
        renderer_dll.Attach();
        renderer_dll.dll_data.Open(false);
        //string ParticleName = "AACExpl1";
        //string ParticleName = "PPCBlast";
        //string ParticleName = "UMGroundExpl4";
        //string ParticleName = "APCExpl1";
        string ParticleName = "GMGroundExpl3";
        //string ParticleName = "HPCExpl1";
        //string ParticleName = "HumanGPlaneDuza";
        ObjId id = new ObjId(ParticleName);
        id = renderer_dll.dll_data.particles.CompleteObjId(id);
        Debug.Log(id);
        PARTICLE_DATA pd = renderer_dll.dll_data.GetParticleData(id);

        Debug.Log(pd);
        stormdata_dll.ReleaseAll();

        GameObject tmp = new GameObject
        {
            name = ParticleName
        };
        tmp.transform.rotation = Quaternion.LookRotation(Vector3.up, Vector3.forward);
        ParticleSystem ps = tmp.AddComponent<ParticleSystem>();
        ps.name = ParticleName;

        var main = ps.main;
        //main.startSize = pd.SelfRadius;
        main.startSize = pd.Size[0]; //Начальный размер партикла.

        main.startSpeed = pd.SpeedRadius.z * pd.SpeedBase.z;

        main.startLifetime = 255 / pd.DecaySpeed;
        main.maxParticles = pd.MaxParts;

        var emission = ps.emission;
        emission.rateOverTime = pd.BirthFrequence;

        var shape = ps.shape;
        //if (resTexture != default) shape.texture = resTexture;
        switch (pd.BirthType)
        {
            case ParticlesDefines.BirthType.PB_NORMAL:
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.radius = pd.BirthBase.magnitude;
                shape.angle = Mathf.Sqrt(Mathf.Pow(pd.BirthRadius.x, 2) + Mathf.Pow(pd.BirthRadius.y, 2));
                break;
            case ParticlesDefines.BirthType.PB_SPHERICAL:
                shape.shapeType = ParticleSystemShapeType.Sphere;
                break;
            case ParticlesDefines.BirthType.PB_TOROIDAL:
                shape.shapeType = ParticleSystemShapeType.Donut;
                break;
            case ParticlesDefines.BirthType.PB_TORUS:
                shape.shapeType = ParticleSystemShapeType.Donut;
                break;
        }

        var limitVelocity = ps.limitVelocityOverLifetime;
        limitVelocity.enabled = true;
        limitVelocity.drag = pd.Friction;

        var col = ps.colorOverLifetime;
        //StormGradient gradient = new StormGradient();
        Gradient gradient = new StormGradient2();
        //Gradient gradient = new Gradient();
        int cnt = 8;
        GradientColorKey[] colorKeys = new GradientColorKey[cnt];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[cnt];


        var size = ps.sizeOverLifetime;
        AnimationCurve curve = new AnimationCurve();
        float sizeMin = pd.Size.Min();
        float sizeMax = pd.Size.Max();


        col.enabled = true;
        size.enabled = true;


        Color32 tmpColor;
        for (int i = 0; i < pd.Size.Length; i++)
        {
            float time = (float)i / (float)(pd.Size.Length - 1);
            curve.AddKey(time, (pd.Size[i] - sizeMin) / (sizeMax - sizeMin));

            //uint colorValue = pd.Color[i];
            //byte a = (byte)((colorValue >> 24) & 0xFF);
            //byte r = (byte)((colorValue >> 16) & 0xFF);
            //byte g = (byte)((colorValue >> 8) & 0xFF);
            //byte b = (byte)((colorValue) & 0xFF);
            //tmpColor = new Color32(r, g, b, a);
            ////colorKeys[i / 32] = new GradientColorKey(tmpColor, time);
            ////alphaKeys[i / 32] = new GradientAlphaKey(a, time);
            //colorKeys[i] = new GradientColorKey(tmpColor, time);
            //alphaKeys[i] = new GradientAlphaKey(a, time);
        }


        for (int i = 0; i < cnt; i++)
        {
            float time = (float)i / (float)cnt;
            //uint colorValue = pd.Color[i * 32];
            uint colorValue = pd.Color[i * (pd.Size.Length / 8)];
            byte a = (byte)((colorValue >> 24) & 0xFF);
            byte r = (byte)((colorValue >> 16) & 0xFF);
            byte g = (byte)((colorValue >> 8) & 0xFF);
            byte b = (byte)((colorValue) & 0xFF);
            tmpColor = new Color32(r, g, b, a);
            colorKeys[i] = new GradientColorKey(tmpColor, time);
            alphaKeys[i] = new GradientAlphaKey(a / 255f, time);
        }
        gradient.SetKeys(colorKeys, alphaKeys);
        col.color = gradient;

        size.size = new ParticleSystem.MinMaxCurve(1f, curve);

        //ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();

        var rend = ps.GetComponent<ParticleSystemRenderer>();
        string shadername = "Particles/Standard Unlit";
        Shader shader = Shader.Find(shadername);
        if (shader != null)
        {
            Material material = new Material(shader);

            Texture2D fulltexture = renderer_dll.dll_data.LoadTexture(pd.GetTextureName());
            Texture2D texture = CutTexture(fulltexture, pd.Mapping);

            material.mainTexture = texture;

            material.EnableKeyword("_ALPHABLEND_ON");
            material.SetOverrideTag("RenderType", "Transparent");

            material.SetFloat("_DstBlend", 1f);
            material.SetFloat("_SrcBlend", 5f);
            material.SetFloat("_ZWrite", 0f);
            material.SetFloat("_Mode", 4f);

            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;


            rend.sharedMaterial = material;
        }
        else
        {
            Debug.Log(string.Format("Shader {0} not found", shadername));
        }
    }


    private static Texture2D CutTexture(Texture2D input, float[] Mapping)
    {
        int textureWidth = (int)((Mapping[2] - Mapping[0]) * input.width);
        int textureHeight = (int)((Mapping[3] - Mapping[1]) * input.height);
        Debug.Log(string.Format("{0}x{1}", textureWidth, textureWidth));
        int textureX = (int)(Mapping[0] * input.width);
        int textureY = (int)(Mapping[1] * input.height);
        Texture2D texture = new Texture2D(textureWidth, textureHeight, input.format, false);

        Color pixel;
        Color correctedpixel;
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                pixel = input.GetPixel(textureX + x, input.height - textureY - y);
                correctedpixel.a = pixel.a;
                correctedpixel.r = correctedpixel.g = correctedpixel.b = pixel.a;
                //texture.SetPixel(x, y, correctedpixel);
                texture.SetPixel(x, y, pixel);
                //Debug.Log("Pixel " + texture.GetPixel(x, y));
            }
        }


        texture.name = input.name;
        texture.Apply();
        return texture;

    }

    //[MenuItem("Tests/Load Static")]
    //static void ImportStatic()
    //{
    //    //ImportStaticByName("Radar Complex 2", "Radars 10000m");
    //    //ImportStaticByName("V Craft Hangar");
    //    ImportStaticByName("WindMill");
    //    renderer_dll.dll_data.materials.ListRecords();
    //    stormdata_dll.ReleaseAll();

    //}


    //static void ImportStaticByName(string SpawnName, string SpawnLayoutName = "", LAYOUT_DATA customLayout = null)
    //{
    //    Init();

    //    if (customLayout != null)
    //    {

    //    }


    //    STATIC_DATA cd = (STATIC_DATA)OBJECT_DATA.GetByName(SpawnName);

    //    FPO myFPO;

    //    myFPO = mpFpoLoader.CreateFPO(cd.RootData.FileName);
    //    myFPO.TextName = SpawnName;
    //    if (SpawnLayoutName != "")
    //    {
    //        LAYOUT_DATA ld = cd.GetLayout(LAYOUT_DATA.SLOTS_LAYOUT, Hasher.HshString(SpawnLayoutName));

    //        Debug.Log("Loaded layout data [" + ld.FullName + "] size: " + ld.Items.Count);


    //        SlotTestEnum helper = new SlotTestEnum(ld);
    //        myFPO.EnumerateSlots(helper);
    //    }

    //    GameObject hull = DrawFpoParticle.DrawFPO(myFPO);
    //    //hull.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
    //    //hull.name = "HULL";
    //    //GameObject Carrier = new GameObject(SpawnName);
    //    //hull.transform.parent = Carrier.transform;
    //    Discard();
    //}

    //[MenuItem("Tests/Load Carrier")]
    //static void ImportCarrier()
    //{
    //    //ImportCarrierByName("Human Sea Carrier", "Turrets & Radar 30000m");
    //    //ImportCarrierByName("Human Sea Escort","Turrets");
    //    //ImportCarrierByName("Human Avia Carrier",            "Turrets Rockets & Radar 30000m");
    //    ImportCarrierByName("Human Avia Escort Vessel", "Turrets Rockets & Radar 30000m");
    //}
    //static void ImportCarrierByName(string SpawnName, string SpawnLayoutName, LAYOUT_DATA customLayout = null)
    //{
    //    Init();

    //    if (customLayout != null)
    //    {

    //    }

    //    //string SpawnName = "Human Sea Escort";
    //    //string SpawnLayoutName = "Turrets & Radar 20000m";
    //    //string SpawnName="Human Avia Escort Vessel";
    //    //string SpawnLayoutName = "Turrets Rockets &Radar 30000m";
    //    CARRIER_DATA cd = (CARRIER_DATA)OBJECT_DATA.GetByName(SpawnName);
    //    //       LAYOUT_DATA ld = cd.GetLayout(LAYOUT_DATA.SLOTS_LAYOUT, Hasher.CodeString(SpawnLayoutName));
    //    LAYOUT_DATA ld = cd.GetLayout(LAYOUT_DATA.SLOTS_LAYOUT, Hasher.HshString(SpawnLayoutName));

    //    Debug.Log("Loaded layout data [" + ld.FullName + "] size: " + ld.Items.Count);
    //    FPO myFPO;

    //    myFPO = mpFpoLoader.CreateFPO(cd.RootData.FileName);
    //    myFPO.TextName = SpawnName;
    //    //FPOS.Add(myFPO);
    //    SlotTestEnum helper = new SlotTestEnum(ld);
    //    myFPO.EnumerateSlots(helper);



    //    //GameObject Gobj = CreateGameObject(myFPO);
    //    //Gobj.name = SpawnName;

    //    //DrawFPOs();
    //    GameObject hull = DrawFpoParticle.DrawFPO(myFPO);
    //    //hull.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
    //    //hull.name = "HULL";
    //    //GameObject Carrier = new GameObject(SpawnName);
    //    //hull.transform.parent = Carrier.transform;
    //    Discard();
    //}

    //private static GameObject DrawFPO(FPO myFPO, GameObject parent = null)
    //{
    //    GameObject Gobj = CreateGameObject(myFPO,parent);
    //    if(myFPO.TextName !=null) Gobj.name = "DrawFPO [" + myFPO.TextName + "]";
    //    Gobj.transform.position = myFPO.Org;
    //    if (parent!=null)
    //    {
    //        Gobj.transform.parent = parent.transform;
    //        Gobj.transform.localPosition = myFPO.Org;
    //        //Gobj.transform.localRotation = Gobj.transform.localRotation * Quaternion.LookRotation(Vector3.forward, Vector3.up);
    //        //Gobj.transform.localRotation = Quaternion.Inverse(Gobj.transform.rotation);
    //    }
    //    //if (DrawFPO.Link != null)
    //    //{
    //    //    //Gobj.transform.position += ((FPO)DrawFPO.Link).Org;
    //    //    Debug.Log(string.Format("Attaching [{0}] to [{1}]", DrawFPO.TextName, ((FPO)DrawFPO.Link).Name.ToString("X8")));
    //    //    Debug.Log(string.Format("Attaching [{0}] to [{1}] {2}", DrawFPO.Org, ((FPO)DrawFPO.Link).Org, (((FPO)DrawFPO.Link).Org) + DrawFPO.Org));
    //    //    Gobj.name += " Link " + ((FPO)DrawFPO.Link).Name.ToString("X8") + " " + ((FPO)DrawFPO.Link).TextName;
    //    //    Gobj.transform.position = Quaternion.LookRotation(((RO)DrawFPO.Link).Dir) * ((RO) DrawFPO.Link).Org + Quaternion.LookRotation(((FPO)DrawFPO.Link).Dir) * DrawFPO.Org ;
    //    //}

    //    if (myFPO.SubObjects != null)
    //    {
    //        for (RO tmpFPO = myFPO.SubObjects; tmpFPO != null; tmpFPO = tmpFPO.Next)
    //        {
    //            GameObject corrector=DrawFPO((FPO)tmpFPO, Gobj);

    //            GameObject DebugDraw = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //            DebugDraw.name = "Debug pos for " + myFPO.TextName;
    //            DebugDraw.transform.position = tmpFPO.ToWorldPoint(tmpFPO.Org);
    //            DebugDraw.transform.parent = corrector.transform;
    //            //corrector.transform.localRotation = Gobj.transform.localRotation * Quaternion.LookRotation(Vector3.forward, Vector3.up);
    //            //corrector.transform.localRotation = Quaternion.Inverse(corrector.transform.rotation);
    //        }
    //    }

    //    return Gobj;

    //}

    //[MenuItem("Tests/Import as GameObject/Import all Static")]
    //static void ImportStatics()
    //{
    //    EditorImport im = new EditorImport();
    //    //im.ImportClass(OBJECT_DATA.OC_STATIC);
    //    im.ImportByObjectDataName("SHABOR");
    //    im.Dispose();

    //}

    //[MenuItem("Tests/Export as FBX/Export all Subobjs")]
    //static void ExportAllStatics()
    //{
    //    EditorImport im = new EditorImport();
    //    im.ExportAllSubobjClasses();
    //    //im.ExportClass(OBJECT_DATA.OC_CRAFT);
    //    //im.ExportClass(OBJECT_DATA.OC_SFG);
    //    im.Dispose();

    //}
    //[MenuItem("Tests/Export as FBX/Export all Static")]
    //static void ExportAllSubobjs()
    //{
    //    EditorImport im = new EditorImport();
    //    im.ExportAllSubobjClasses();
    //    //im.ExportClass(OBJECT_DATA.OC_CRAFT);
    //    //im.ExportClass(OBJECT_DATA.OC_SFG);
    //    im.Dispose();

    //}

    //[MenuItem("Tests/Import/Import Selected Static")]
    //static void ExportAllObject()
    //{
    //    EditorImport im = new EditorImport();
    //    im.ImportByObjectDataName("SHABOR");
    //    im.Dispose();

    //}
    //[MenuItem("Tests/FPO2GAMEOBJECT")]
    //static void ImportFPO()
    //{
    //    stormdata_dll.LoadAll();
    //    renderer_dll.Attach();
    //    LOG log = new LOG();
    //    //string FpoName = "ha_int1";
    //    string FpoName = "ha_bf6";

    //    Renderer mpRenderer = Renderer.CreateRenderer(null, log, null, RendererApi.RENDERER_VERSION);
    //    IFpoLoader mpFpoLoader = (IFpoLoader)FpoLoader.CreateFpoLoader(mpRenderer, null, "objects2.dat", "objects.dat");
    //    if (mpFpoLoader == null) Debug.Log("FPO loader failed to load itself");

    //    Vector3 pos = Vector3.zero;
    //    FPO myFPO;

    //    myFPO = mpFpoLoader.CreateFPO(FpoName);
    //    if (MaterialStorage.DefaultSolid == null) MaterialStorage.DefaultSolid = new Material(Shader.Find("HDRP/Lit"));
    //    if (MaterialStorage.DefaultTransparent == null) MaterialStorage.DefaultTransparent = new Material(Shader.Find("HDRP/Lit"));
    //    //myFPO = mpFpoLoader.CreateFPO("vs_block3_1");
    //    try
    //    {
    //        //GameObject Gobj = CreateGameObjectFull(myFPO);
    //        GameObject Gobj = DrawFpoParticle.DrawFPO(myFPO);
    //        Gobj.transform.position = pos;
    //        //ExportGameObject(Gobj);
    //    }
    //    catch (System.Exception e)
    //    {
    //        log.Message("Failed to gen FPO " + FpoName + e.ToString());
    //        throw;
    //    }


    //    stormdata_dll.ReleaseAll();
    //    renderer_dll.Detach();
    //}

    //[MenuItem("Tests/Import as GameObject/FPO2GAMEOBJECT")]
    //static void ImportFPOClass()
    //{
    //    EditorImport im = new EditorImport();

    //    //ImportFPO("ha_bf6");
    //    im.ImportByFPOName("ha_bf6");
    //    im.Dispose();
    //}



    [MenuItem("Tests/Import as GameObject/FPO2GAMEOBJECTS")]
    static void ImportFPOs()
    {
        stormdata_dll.LoadAll();

        Renderer mpRenderer = Renderer.CreateRenderer(null, new LOG(), null, RendererApi.RENDERER_VERSION);
        IFpoLoader mpFpoLoader = (IFpoLoader)FpoLoader.CreateFpoLoader(mpRenderer, null, "objects2.dat", "objects.dat");
        if (mpFpoLoader == null) Debug.Log("FPO loader failed to load itself");

        Vector3 pos = Vector3.zero;
        FPO myFPO;
        foreach (SUBOBJ_DATA wData in SUBOBJ_DATA.Datas)
        //foreach (OBJECT_DATA wData in OBJECT_DATA.Datas)
        {
            myFPO = mpFpoLoader.CreateFPO(wData.FileName);

            //if ((wData.Flags & OBJECT_DATA.OC_VEHICLE) == 0) continue;
            //myFPO = mpFpoLoader.CreateFPO(wData.RootData.FileName);

            GameObject Gobj = CreateGameObjectFull(myFPO);
            //pos += new Vector3(1, 0, 0) * myFPO.HashRadius * 2;
            pos += new Vector3(1, 0, 0) * myFPO.HashRadius;
            Gobj.transform.position = pos;
            pos += new Vector3(1, 0, 0) * myFPO.HashRadius;
            Gobj.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
            Gobj.name = wData.FullName;


        }

        //foreach (OBJECT_DATA wData in OBJECT_DATA.Datas)
        //{
        //    if ((wData.Flags & OBJECT_DATA.OC_VEHICLE) == 0) continue;
        //    myFPO = mpFpoLoader.CreateFPO(wData.RootData.FileName);
        //    GameObject Gobj = CreateGameObject(myFPO);

        //    pos += new Vector3(1, 0, 0) * myFPO.HashRadius;
        //    Gobj.transform.position = pos;
        //    pos += new Vector3(1, 0, 0) * myFPO.HashRadius;
        //    Gobj.name = wData.FullName;


        //}

        stormdata_dll.ReleaseAll();

    }

    public static GameObject CreateGameObject(FPO myFPO, GameObject parent = null)
    {
        //GameObject final = DrawGameObjectPool.objectPool.Get();

        //if (parent != null) final.transform.parent = parent.transform;
        // final.name = myFPO.TextName != "" ? "CO" + myFPO.TextName:myFPO.Name.ToString("X8");

        //GameObject corrector = new GameObject("Orientation corrector");
        //GameObject corrector = DrawGameObjectPool.objectPool.Get();
        //corrector.name = "Orientation corrector";
        //corrector.transform.parent = final.transform;

        //BuildGameObjectFromFdata(myFPO.fdata, myFPO.TextName, corrector);
        //GameObject hull = corrector.transform.GetChild(0).gameObject;

        //GameObject hull = DrawFpoParticle.fdata2gameObject(myFPO.fdata, myFPO.fdata.name.ToString("X8"), corrector);
        //GameObject hull = DrawFpoParticle.fdata2gameObject(myFPO.fdata, myFPO.fdata.name.ToString("X8"), final);
        GameObject hull = DrawFpoParticle.fdata2gameObject(myFPO.fdata, myFPO.fdata.name.ToString("X8"), parent);

        //GameObject hull = DrawFpoParticle.fpo2gameObject(myFPO,corrector);
        //if (hull == null) return null;
        //corrector.transform.localRotation = hull.transform.localRotation * Quaternion.LookRotation(Vector3.forward, Vector3.up);
        //corrector.transform.localRotation = Quaternion.Inverse(corrector.transform.rotation);
        hull.transform.localPosition = Vector3.zero;

        return hull;
    }
    public static GameObject CreateGameObjectFull(FPO myFPO, GameObject parent = null)
    {
        GameObject hull = StormUnityRenderer.fdata2gameObjectFull(myFPO.fdata, myFPO.fdata.name.ToString("X8"), parent);

        hull.transform.localPosition = Vector3.zero;

        return hull;
    }
}
//{
//    class PoolObject
//    {
//        private string name;
//        private static int count;
//        public PoolObject()
//        {
//            name = "Pool Object" + count++;
//            Debug.Log("Me new " + name);
//        }

//        public override string ToString()
//        {
//            return name;
//        }

//    }
//    [MenuItem("Tests/ObjectPool")]
//    static void TestObjectPool()
//    {
//        ObjectPool<PoolObject> op = new ObjectPool<PoolObject>(onCreate,onGetObj,onRelease,onDestroy,true,20,100);

//        static PoolObject onCreate()
//        {
//            return new PoolObject();
//        }
//        static void onGetObj(PoolObject obj) {
//            Debug.Log("Getting " + obj);
//        }

//        static void onRelease(PoolObject obj) { Debug.Log("releasing " + obj); }
//        static void onDestroy(PoolObject obj) { Debug.Log("destroying " + obj); }

//        Debug.Log(string.Format("Init {0} {1} {2}", op.CountActive, op.CountInactive, op.CountAll));
//        for (int i = 0; i < 30; i++)
//        {
//            //Debug.Log(string.Format("Init {0} {1} {2}",op.CountActive,op.CountInactive,op.CountAll));
//            PoolObject z = op.Get();
//            //Debug.Log(string.Format("Get {0} {1} {2}", op.CountActive, op.CountInactive, op.CountAll));
//            op.Release(z);

//            //Debug.Log(string.Format("Release {0} {1} {2}", op.CountActive, op.CountInactive, op.CountAll));
//        }
//        Debug.Log(string.Format("Fin {0} {1} {2}", op.CountActive, op.CountInactive, op.CountAll));
//        op.Clear();
//        Debug.Log(string.Format("CLear {0} {1} {2}", op.CountActive, op.CountInactive, op.CountAll));
//    }


//    [MenuItem("Tests/DATA/UnitDataTable file loader")]
//    static void TestUnitData()
//    {
//        ResourcePack rp = new ResourcePack();
//        rp.Open(_PI.getHddFile("gdata.dat"));
//        UnitDataDBHelper.LoadUnitDataTable(rp);
//        rp.Close();

//        StringBuilder sb = new StringBuilder();
//        sb.AppendLine("Example of UnitData");

//        //lEntry = 5;
//        //rEntry = 6;
//        //sb.AppendLine(string.Format("{0} vs {1}", UnitDataTable.pUnitDataTable.GetUD(lEntry).getName(), UnitDataTable.pUnitDataTable.GetUD(rEntry).getName()));

//        //UDTE = UnitDataTable.pUnitDataTable.GetUDTE(lEntry, rEntry);
//        //sb.AppendLine(string.Format("imp {0}", UDTE.importance));
//        //sb.AppendLine(string.Format("pow {0}", UDTE.power));


//        //lEntry = 6;
//        //rEntry = 5;
//        //sb.AppendLine(string.Format("{0} vs {1}", UnitDataTable.pUnitDataTable.GetUD(lEntry).getName(), UnitDataTable.pUnitDataTable.GetUD(rEntry).getName()));

//        //UDTE = UnitDataTable.pUnitDataTable.GetUDTE(lEntry, rEntry);
//        //sb.AppendLine(string.Format("imp {0}", UDTE.importance));
//        //sb.AppendLine(string.Format("pow {0}", UDTE.power));

//        sb.Append(UDTEtoText(5, 6));
//        sb.Append(UDTEtoText(6, 5));
//        sb.Append(UDTEtoText(0, 5));
//        sb.Append(UDTEtoText(5, 0));
//        sb.Append(UDTEtoText(0, 6));
//        sb.Append(UDTEtoText(6, 0));
//        Debug.Log(sb.ToString());
//    }

//    private static string UDTEtoText(int lEntry, int rEntry)
//    {
//        if (UnitDataTable.pUnitDataTable == null) return "UnitDataTable not loaded";
//        StringBuilder sb = new StringBuilder();
//        sb.AppendLine(string.Format("{0} vs {1}", UnitDataTable.pUnitDataTable.GetUD(lEntry).getName(), UnitDataTable.pUnitDataTable.GetUD(rEntry).getName()));
//        UnitDataTableEntry UDTE = UnitDataTable.pUnitDataTable.GetUDTE(lEntry, rEntry);
//        sb.AppendLine(string.Format("imp {0}", UDTE.importance));
//        sb.AppendLine(string.Format("pow {0}", UDTE.power));

//        return sb.ToString();
//    }
//    [MenuItem("Tests/DATA/Test static file loader")]
//    static void ExecuteStatic()
//    {
//        STATIC_DATA.loadStaticData(PackType.gData);
//        Debug.Log("Datas count: " + STATIC_DATA.Datas.Count);
//        string search = "Velian Main Base Part 1";
//        Debug.Log("Search test " + search + " -> " + OBJECT_DATA.GetByName(search));
//    }
//    [MenuItem("Tests/DATA/Test static hangar file loader")]
//    static void ExecuteStaticHangar()
//    {
//        HANGAR_DATA.loadHangarData(PackType.gData);
//        Debug.Log("Datas count: " + STATIC_DATA.Datas.Count);
//        string search = "Craft Hangar Big";
//        Debug.Log("Search test " + search + " -> " + OBJECT_DATA.GetByName(search));
//    }
//    [MenuItem("Tests/DATA/Test craft file loader")]
//    static void ExecuteCraft()
//    {
//        CRAFT_DATA.loadCraftData(PackType.gData);
//        Debug.Log("Datas count: " + CRAFT_DATA.Datas.Count);
//        string search = "Velian_INT3";
//        Debug.Log("Search test " + search + " -> " + CRAFT_DATA.GetByName(search));
//    }
//    [MenuItem("Tests/DATA/Test road file loader")]
//    static void ExecuteRoad()
//    {
//        ROADDATA.loadRoadData(0, PackType.gData);
//        Debug.Log("Datas count: " + ROADDATA.Datas.Count);
//        string search = "Dirty road";
//        Debug.Log("Search test " + search + " -> " + ROADDATA.GetByName(search));
//    }
//    //[MenuItem("Tests/DATA/Test weapon file loader")]
//    //static void ExecuteWeapon()
//    //{
//    //    //Stream st = GameDataHolder.GetResource<Stream>(PackType.gData, "Statics");

//    //    Stream st = GameDataHolder.GetResource<Stream>(PackType.gData, "Weapons");
//    //    Debug.Log("Data size: " + st.Length);

//    //   // LoadUtils lu = new LoadUtils();
//    //    //lu.parseData(st, "carrier", "Carriers", "Carriers.txt", "[STORM CARRIERS DATA FILE V1.0]", keys, callbacks);

//    //    //lu.parseData(st, "static object", "Statics", "Statics.txt", "[STORM STATICS DATA FILE V1.1]", "Static", STATIC_DATA.insertStaticData);

//    //    string[] keys = new string[] { "Plasma", "Gun", "Rocket", "Missile", "Projectile" };
//    //    LoadUtils.InsertCall[] calls = new LoadUtils.InsertCall[] { WPN_DATA.insertPlasmaData, WPN_DATA.insertGunData, WPN_DATA.insertRocketData, WPN_DATA.insertMissileData, WPN_DATA.insertProjectileData };
//    //    LoadUtils.parseMultiData(PackType.gData, "weapon", "Weapons", "Weapons.txt", "[STORM WEAPON DATA FILE V1.1]", keys, calls);
//    //    st.Close();

//    //    Debug.Log("Datas count: " + ObjDatasHolder.Datas.Count);
//    //    Debug.Log("DatasSubjObj count: " + ObjDatasHolder.DatasSubjObj.Count);
//    //}

//    [MenuItem("Tests/DATA/Test carrier file loader")]
//    static void ExecuteCarrier()
//    {
//        CARRIER_DATA.loadCarrierData(PackType.gData);
//        Debug.Log("Datas count: " + OBJECT_DATA.Datas.Count);
//        string search = "Human Sea Escort";
//        Debug.Log("Search test " + search + " -> " + OBJECT_DATA.GetByName(search));
//    }
//    [MenuItem("Tests/DATA/Test vehicle file loader")]
//    static void ExecuteVehicle()
//    {
//        VEHICLE_DATA.loadVehicleData(PackType.gData);
//        Debug.Log("Datas count: " + OBJECT_DATA.Datas.Count);
//        //string search = "Human Truck 1";
//        string search = "H_MEDIUM Cannon";
//        Debug.Log("Search test " + search + " -> " + VEHICLE_DATA.GetByName(search));
//    }

//    [MenuItem("Tests/DATA/Test explosion file loader")]
//    static void ExecuteExplosion()
//    {
//        EXPLOSION_DATA.loadExplData(0, PackType.gData);
//        Debug.Log("Datas count: " + EXPLOSION_DATA.Datas.Count);
//    }
//    [MenuItem("Tests/DATA/Test debris file loader")]
//    static void ExecuteDebris()
//    {
//        DEBRIS_DATA.loadDebrData(0, PackType.gData);
//        Debug.Log("Datas count: " + DEBRIS_DATA.Datas.Count);
//    }
//    [MenuItem("Tests/DATA/Test turret file loader")]
//    static void ExecuteTurret()
//    {
//        TURRET_DATA.loadTurretData(PackType.gData);
//        Debug.Log("Datas count: " + TURRET_DATA.Datas.Count);
//    }
//    [MenuItem("Tests/DATA/Test radar file loader")]
//    static void ExecuteRadar()
//    {
//        RADAR_DATA.loadRadarData(PackType.gData);
//        Debug.Log("Datas count: " + TURRET_DATA.Datas.Count);
//    }
//    [MenuItem("Tests/DATA/Test weapon file loader")]
//    static void ExecuteWeapon()
//    {
//        WPN_DATA.loadWeaponData(PackType.gData);
//        Debug.Log("Datas count: " + WPN_DATA.Datas.Count);
//    }
//    [MenuItem("Tests/DATA/Load All")]
//    static void LoadAll()
//    {
//        PackType DataDB = PackType.gData;
//        for (int i = 0; i < 2; i++)
//        {
//            DUST_DATA.loadDustData(i, DataDB);
//            ROADDATA.loadRoadData(i, DataDB);
//            EXPLOSION_DATA.loadExplData(i, DataDB);
//            DEBRIS_DATA.loadDebrData(i, DataDB);
//            SUBOBJ_DATA.loadSubobjData(i, DataDB);
//            OBJECT_DATA.loadObjectData(i, DataDB);
//        }

//        foreach (OBJECT_DATA objData in OBJECT_DATA.Datas)
//        {
//            Debug.Log(objData.FullName + " " + objData.Name.ToString("X8"));
//        }
//    }
//    [MenuItem("Tests/GameHolder")]
//    static void TestHolder()
//    {
//        GameHolder gh = new GameHolder(new LOG());
//        gh.Open("Battle of Rockade IV", "C2-1");
//        //gh.OpenData("Battle of Rockade IV", "C2-1");
//        Debug.Log(gh.PrintCampaign());
//        Debug.Log(gh.GetBriefingText().GetValue());
//    }

//    [MenuItem("Tests/Config.cfg loader")]
//    static void TestConfigLoader()
//    {
//        GameHolder gh = new GameHolder(new LOG());
//        gh.OpenOptions();
//        Debug.Log("Load error: [" + gh.GetError() + "]");

//        //gh.Open("Battle of Rockade IV", "C2-1");
//        //gh.OpenCampaign("Battle of Rockade IV", "C2-1");
//        Debug.Log(gh.PrintOptions());
//    }

//    [MenuItem("Tests/Basic loader")]
//    static void TestBasicLoader()
//    {
//        //string FileName = "Battle of Rockade IV.cmp";
//        //string VarName = "GameMapSizeZ";
//        //iUnifiedVariableDB pDb = UniVarUtils.CreateUnifiedVariableDB(Constants.UNIVARS_VERSION, FileName, true);
//        //iUnifiedVariableContainer mpCampaign = (UniVarContainer)pDb.GetRoot();
//        //iUnifiedVariableContainer mpDefaultMission = (UniVarContainer)mpCampaign.GetVariableTpl<iUnifiedVariableContainer>("Default Mission");
//        //UniVarInt pInt;
//        //pInt = (UniVarInt)mpCampaign.GetVariableTpl<iUnifiedVariableInt>(VarName);
//        //Debug.Log(pInt.GetValue());

//        ////string FileName = "Battle of Rockade IV.cmp";
//        //string FileName = "Configs.cfg";
//        //string CntName = "Options";
//        //string Cnt2Name = "Game";
//        //string VarName = "Skill";
//        //iUnifiedVariableDB pDb = UniVarUtils.CreateUnifiedVariableDB(Constants.UNIVARS_VERSION, FileName, true);
//        //iUnifiedVariableContainer root = (UniVarContainer)pDb.GetRoot();
//        //Debug.Log(root);
//        //iUnifiedVariableContainer cnt = root.GetVariableTpl<iUnifiedVariableContainer>(CntName);
//        //Debug.Log(cnt);
//        //iUnifiedVariableContainer cnt2 = cnt.GetVariableTpl<iUnifiedVariableContainer>(Cnt2Name);

//        //UniVarInt pInt;
//        //pInt = (UniVarInt)cnt2.GetVariableTpl<iUnifiedVariableInt>(VarName);
//        //Debug.Log(pInt.GetValue());

//        string FileName = "Configs.cfg";
//        string CntName = "Options";
//        string Cnt2Name = "Game";
//        string VarName = "Skill";
//        iUnifiedVariableDB pDb = UniVarUtils.CreateUnifiedVariableDB(Constants.UNIVARS_VERSION, FileName, true);

//        //iUnifiedVariableContainer cnt = pDb.GetVariableTpl<iUnifiedVariableContainer>(CntName);
//        iUnifiedVariableContainer cnt = pDb.GetVariableTpl<iUnifiedVariableContainer>("\\Root\\" + CntName);
//        //Debug.Log(cnt);
//        //iUnifiedVariableContainer cnt2 = cnt.GetVariableTpl<iUnifiedVariableContainer>(Cnt2Name);

//        UniVarInt pInt;
//        pInt = (UniVarInt)cnt.GetVariableTpl<iUnifiedVariableInt>(Cnt2Name + '\\' + VarName);
//        //pInt = (UniVarInt)cnt2.GetVariableTpl<iUnifiedVariableInt>(VarName);
//        Debug.Log(pInt.GetValue());
//    }

//    [MenuItem("Tests/FPO loader")]
//    static async void LoadFPOtest()
//    {
//        FpoLoader loader = new FpoLoader();
//        loader.Initialize(null, null, "objects2.dat", "objects.dat");

//        ExecuteStatic();
//        List<tmpFPOplacer> templates = new List<tmpFPOplacer>();
//        //loader.CreateFPO("ha_aviacarr");
//        //foreach(OBJECT_DATA objData in OBJECT_DATA.Datas)
//        //{
//        //    string objName = objData.FullName; 
//        //    string objFile = objData.RootData.FileName; 
//        //    uint objId = Hasher.HshString(objName); 
//        //    tmpFPOplacer replacer = new tmpFPOplacer(objId, objFile, objName); 
//        //    templates.Add(replacer);
//        //}
//        //GameObject tmpUnit = fpoBuilder.BuildFPO(replacer.objData);

//        //    public struct tmpFPOplacer
//        //{
//        //    public uint crc;
//        //    public string objData;
//        //    public string name;
//        //    public tmpFPOplacer(uint crc, string objData, string name = "")
//        //    { this.crc = crc; this.objData = objData; this.name = name == "" ? crc.ToString("X8") : name; }
//        //}

//        string search = "SHABOR";
//        OBJECT_DATA objData = OBJECT_DATA.GetByName(search);
//        string objName = objData.FullName;
//        string objFile = objData.RootData.FileName;
//        uint objId = Hasher.HshString(objName);
//        tmpFPOplacer replacer = new tmpFPOplacer(objId, objFile, objName);
//        //FpoBuilder fpoBuilder = new FpoBuilder();
//        //GameObject tmpUnit = fpoBuilder.BuildFPO(replacer.objData);
//        //Transform corrector = tmpUnit.transform.GetChild(0);
//        //Transform hull = corrector.GetChild(0);
//        //corrector.localRotation = hull.localRotation * Quaternion.LookRotation(Vector3.forward, Vector3.up);
//        //corrector.localRotation = Quaternion.Inverse(corrector.localRotation);

//        OBJECT_DATA.Datas.Clear();
//    }
//    [MenuItem("Tests/Hash/CRC test")]
//    static void TestCRC32()
//    {
//        string name = "Craft Hangar Big";
//        Debug.Log(name + " " + Hasher.HshString(name));
//    }
//    [MenuItem("Tests/Road loader")]
//    static void RoadTest()
//    {
//        GameHolder gh = new GameHolder(new LOG());
//        gh.Open("Battle of Rockade IV", "C2-1");
//        //gh.OpenData("Battle of Rockade IV", "C2-1");
//        Debug.Log(gh.PrintCampaign());
//        Debug.Log(gh.GetBriefingText().GetValue());

//        Stream st = gh.mpCampaign.openStream("RoadNet");
//        RoadNetDataHead hd = StormFileUtils.ReadStruct<RoadNetDataHead>(st);
//        //Debug.Log(hd);
//        RoadNetData rd = new RoadNetData(hd);
//        rd.head = hd;
//        for (int i = 0; i < hd.links_count; i++)
//        {
//            rd.LinksI[i] = StormFileUtils.ReadStruct<int>(st, st.Position);
//        }
//        for (int i = 0; i < hd.nodes_count; i++)
//        {
//            rd.NodesI[i] = StormFileUtils.ReadStruct<int>(st, st.Position);
//        }
//        for (int i = 0; i < hd.visuals_count; i++)
//        {
//            rd.VisualsI[i] = StormFileUtils.ReadStruct<int>(st, st.Position);
//        }


//        for (int i = 0; i < hd.links_count; i++)
//        {
//            rd.Links[i] = StormFileUtils.ReadStruct<LinkData>(st, st.Position);
//        }

//        for (int i = 0; i < hd.nodes_count; i++)
//        {
//            NodeDataHead nhd = StormFileUtils.ReadStruct<NodeDataHead>(st, st.Position);
//            Debug.Log(nhd);
//            NodeData nd = new NodeData();
//            nd.head = nhd;
//            nd.links = new int[nhd.link_count];
//            for (int j = 0; j < nhd.link_count; j++)
//            {
//                nd.links[j] = StormFileUtils.ReadStruct<int>(st, st.Position);
//            }
//            rd.Nodes[i] = nd;
//        }


//        for (int i = 0; i < hd.visuals_count; i++)
//        {
//            VisualDataHead vdh = StormFileUtils.ReadStruct<VisualDataHead>(st, st.Position);
//            VisualData vd = new VisualData();
//            vd.head = vdh;
//            vd.vectors = new Vector3[vdh.vector_count];
//            Debug.Log(vdh);
//            for (int j = 0; j < vdh.vector_count; j++)
//            {
//                vd.vectors[j] = StormFileUtils.ReadStruct<Vector3>(st, st.Position);
//            }
//            rd.Visuals[i] = vd;
//        }

//        LinkData node = rd.Links[4130];
//        Debug.Log(node);

//        ROADDATA.loadRoadData(0, PackType.gData);
//        ROADDATA data = ROADDATA.GetByCode(node.roaddata);
//        Debug.Log(data);

//        ROADDATA.Datas.Clear();
//        RoadNode node1 = new RoadNode(rd.Nodes[node.node1].head.org, rd.Nodes[node.node1].head.link_count);
//        RoadNode node2 = new RoadNode(rd.Nodes[node.node2].head.org, rd.Nodes[node.node2].head.link_count);

//        RoadLink roadLink = new RoadLink();
//        roadLink.Set(data, node1, node2);
//        Debug.Log(roadLink);

//        Debug.Log(rd);
//    }
//    [MenuItem("Tests/Terrain to texture")]
//    static void TerrainExportTest()
//    {
//        TERRAIN_DATA tdata = new TERRAIN_DATA("Scenes/Continent");
//        tdata.OpenHdr();
//        tdata.OpenSq(false, false);
//        tdata.OpenVb(false, false);
//        Debug.Log(tdata);

//        Color[] palette = {
//          new Color( 0, 0, 50 ), //0
//          new Color(0, 50, 0),         // 1
//          new Color(0, 70, 0) ,          // 2
//          new Color(0, 90, 0),           // 3
//          new Color(0, 110, 0),          // 4
//          new Color(50, 50, 0),         // 5
//          new Color(70, 70, 0),         // 6
//          new Color(90, 90, 0),         // 7
//          new Color(110, 110, 0),         // 8
//          new Color(130, 130, 0),         // 9
//          new Color(150, 150, 0),         //10
//          new Color(170, 170, 0),         // 1
//          new Color(190, 190, 0),         // 2
//          new Color(110, 0, 0),          // 3
//          new Color(130, 0, 0),           // 4
//          new Color(150, 0, 0),           // 5
//          new Color(50, 50, 50),         // 6
//          new Color(70, 70, 70),         // 7
//          new Color(90, 90, 90),         // 8
//          new Color(110, 110, 110),      // 9
//          new Color(130, 130, 130),      //20
//          new Color(150, 150, 150),       // 1
//          new Color(170, 170, 170),       // 2
//          new Color(30, 0, 30),         // 3
//          new Color(50, 0, 50),         // 4
//          new Color(70, 0, 70),         // 5
//          new Color(0, 250, 0),          // 6 Forest G250
//          new Color(110, 0, 110),       // 7
//          new Color(0, 200, 0),          // 8 Forest G200
//          new Color(150, 0, 150)        // 9
//        };



//        int sizeX = tdata.Header.SizeXBPages * TerrainDefs.VBOXES_PAGE_SIZE * TerrainDefs.BPAGE_IN_VBPAGES;
//        int sizeZ = tdata.Header.SizeZBPages * TerrainDefs.VBOXES_PAGE_SIZE * TerrainDefs.BPAGE_IN_VBPAGES;

//        Texture2D texture = new Texture2D(sizeX, sizeZ);
//        texture.name = "VB_import";
//        for (int z = 0; z < sizeZ; z++)
//        {
//            for (int x = 0; x < sizeX; x++)
//            {
//                T_VBOX vb = tdata.VBoxes.pager.Get(x, z);
//                //Debug.Log(((int)vb.material[0]).ToString());
//                texture.SetPixel(x, z, palette[vb.material[0]]);
//            }
//        }
//        AssetDatabase.CreateAsset(texture, "Assets/" + texture.name + ".texture2d");
//    }
//    [MenuItem("Tests/FPOloader/main")]
//    public static void TestFPOloader()
//    {
//        string fponame = "v_craftang_polosa";
//        FpoLoader mpFpoLoader = (FpoLoader)FpoLoader.CreateFpoLoader(new Renderer(), null, "objects2.dat", "objects.dat");
//        //MEOData.MEO_DATA_HDR meo = MEOData.MEO_DATA_HDR.LoadMEO("ha_bf");
//        //MEOData.MEO_DATA_HDR meo = MEOData.MEO_DATA_HDR.LoadMEO("ha_int1");
//        //MEOData.MEO_DATA_HDR meo = MEOData.MEO_DATA_HDR.LoadMEO("ha_aviacarr");
//        //MEOData.MEO_DATA_HDR meo = MEOData.MEO_DATA_HDR.LoadMEO("h-med2");
//        //MEOData.MEO_DATA_HDR meo = MEOData.MEO_DATA_HDR.LoadMEO("bf1ruins");
//        StormTestExamine.FPO myFPO = mpFpoLoader.CreateFPO(fponame);
//        if (myFPO == null)
//        {
//            Debug.Log("Failed to load FPO " + fponame);
//            return;
//        }

//        Debug.Log(myFPO.GetNumSubObjs(myFPO.fdata));
//    }
//    [MenuItem("Tests/FPOloader/Generate GameObject From FPO")]
//    public async static void TestGenerateFromFPO()
//    {
//        //string fponame = "ha_aviacarr";
//        //string fponame = "hnavycarr";
//        //string fponame = "civhomec1";
//        //string fponame = "v_lab";
//        string fponame = "v_craftang_polosa";
//        FpoLoader mpFpoLoader = (FpoLoader)FpoLoader.CreateFpoLoader(new Renderer(), null, "objects2.dat", "objects.dat");
//        StormTestExamine.FPO myFPO = mpFpoLoader.CreateFPO(fponame);

//        if (myFPO == null)
//        {
//            Debug.Log("Failed to load FPO " + fponame);
//            return;
//        }

//        //FPODesc(myFPO);
//        //FPOtoGOBJ(myFPO, fponame);

//        GameObject final = new GameObject(fponame);
//        GameObject corrector = new GameObject("Orientation corrector");
//        corrector.transform.parent = final.transform;

//        Debug.Log("Gameobject generation started");
//        GameObject res = await fdata2gameObject(myFPO.fdata, fponame, corrector);
//        corrector.transform.localRotation = res.transform.localRotation * Quaternion.LookRotation(Vector3.forward, Vector3.up);
//        corrector.transform.localRotation = Quaternion.Inverse(corrector.transform.rotation);
//        Debug.Log("Gameobject generation Done");
//        mpFpoLoader.Destroy();
//    }

//    private async static Task<GameObject> fdata2gameObject(FpoData fdata, string name = "unknown", GameObject parent = null)
//    {
//        GameObject res= DrawFpoParticle.fdata2gameObject(fdata, name, parent);
//        return res;
//        ////GameObject gobj = new GameObject(name);
//        ////GameObject gobj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//        ////StormMesh ms = GameDataHolder.GetResource<StormMesh>(PackType.MeshDB, fdata.name);

//        //GameObject gobj = new GameObject(name);

//        //FpoGraphData fpoGraphData = GameDataHolder.GetResource<FpoGraphData>(PackType.MeshDB, fdata.images[0].graph);

//        //if (fpoGraphData.GetLod(0) != 0x00000000)
//        //{
//        //    if (fpoGraphData != null)
//        //    {
//        //        StormMesh graph = GameDataHolder.GetResource<StormMesh>(PackType.MeshDB, fpoGraphData.GetLod(0));

//        //        Mesh tmpMesh = StormMeshImport.ExtractMesh(graph);
//        //        MeshRenderer mr = gobj.AddComponent<MeshRenderer>();
//        //        MeshFilter mf = gobj.AddComponent<MeshFilter>();
//        //        mf.mesh = tmpMesh;
//        //        mr.materials = StormMeshImport.GetMaterials(graph);
//        //    }
//        //}
//        //if (parent != null)
//        //{
//        //    gobj.transform.parent = parent.transform;
//        //}
//        //gobj.name = name;
//        ////gobj.transform.localScale *= 2 * fdata.images[1].radius;

//        //Vector3 fixedpos = fdata.pos.org;
//        //fixedpos.y *= -1;
//        //gobj.transform.localPosition = fixedpos;

//        //Vector3 FPOUp = fdata.pos.e2;
//        //Vector3 FPOLeft = fdata.pos.e1;

//        //FPOUp.y *= -1;
//        //FPOLeft.y *= -1;

//        //Vector3 FPODir = Vector3.Cross(FPOUp, FPOLeft);

//        ////if (parent != null) gobj.transform.localRotation = Quaternion.LookRotation(FPODir, FPOUp);
//        //gobj.transform.localRotation = Quaternion.LookRotation(FPODir, FPOUp * -1);

//        //FpoData f;
//        ////for (f=fdata.GetSubData();f!=null;f=f.GetSubData())
//        ////{
//        ////    await Task.Yield();
//        ////    await fdata2gameObject(f, "subobj", gobj);
//        ////}

//        //f = fdata.GetSubData();
//        //await Task.Delay(100);
//        //if (f != null)
//        //{
//        //    //await Task.Yield();
//        //    string subobjname = f.name.ToString("X8");
//        //    await fdata2gameObject(f, "subobj " + name, gobj);
//        //}

//        //f = fdata.GetNextData();
//        //if (f != null)
//        //{
//        //    //await Task.Yield();
//        //    await fdata2gameObject(f, "next", parent);
//        //}
//        ////for (f = fdata.GetNextData(); f != null; f = f.GetNextData())
//        ////{
//        ////    await Task.Yield();
//        ////    await fdata2gameObject(f, "next", parent);
//        ////}

//        //return gobj;
//    }


//    [MenuItem("Tests/Sky/Sky config reader")]
//    public static void DoReadSkyTest()
//    {
//        string skyName = "day1#env";
//        Stream st = GameDataHolder.GetRDataDB().GetStreamByName(skyName);

//        SkyCfg skyCfg = StormFileUtils.ReadStruct<SkyCfg>(st);
//        st.Close();
//        Debug.Log(skyCfg);

//        Debug.Log("Sun " + GameDataHolder.GetRDataDB().GetNameById(skyCfg.sun));
//        Debug.Log("Stars " + GameDataHolder.GetRDataDB().GetNameById(skyCfg.stars));
//        Debug.Log("Flares " + GameDataHolder.GetRDataDB().GetNameById(skyCfg.flares));
//        Debug.Log("Planets " + GameDataHolder.GetRDataDB().GetNameById(skyCfg.planets));
//        Debug.Log("Layer0 " + GameDataHolder.GetRDataDB().GetNameById(skyCfg.layer0));
//        Debug.Log("Layer1 " + GameDataHolder.GetRDataDB().GetNameById(skyCfg.layer1));

//        st = GameDataHolder.GetRDataDB().GetStreamById(skyCfg.layer0);
//        LayerCfg layer0Cfg = StormFileUtils.ReadStruct<LayerCfg>(st);
//        st.Close();

//        Debug.Log(layer0Cfg);

//        st = GameDataHolder.GetRDataDB().GetStreamById(skyCfg.layer1);
//        LayerCfg layer1Cfg = StormFileUtils.ReadStruct<LayerCfg>(st);
//        st.Close();

//        Debug.Log(layer1Cfg);

//        st = GameDataHolder.GetRDataDB().GetStreamById(skyCfg.stars);
//        StarsCfg stars = StormFileUtils.ReadStruct<StarsCfg>(st);
//        st.Close();
//        Debug.Log(stars);
//    }

//    [MenuItem("Tests/Hash/CodeString")]
//    public static void TestCodeString()
//    {
//        string input = "HULL";

//        Debug.Log(string.Format("{0} Hasher.codestring {1}", input, Hasher.CodeString(input).ToString("X8")));
//        //Debug.Log(Storm.Math.CodeString(input).ToString("X8"));
//    }

//    [MenuItem("Tests/MEO loader")]
//    static void LoadMEO()
//    {
//        MEOData.MEO_DATA_HDR MEO = MEOData.MEO_DATA_HDR.LoadMEO("ha_aviacarr");
//    }

//    [MenuItem("Tests/textures")]
//    static void TestTexture()
//    {
//        ResourcePack pk = GameDataHolder.GetTexturesDB();
//        pk.LoadRAT();
//        //pk.GetStreamById();
//    }
//    [MenuItem("Tests/Load Craft")]
//    static async void LoadCraftTest()
//    {
//        ////_PI.dataDir = _PI.mHddPath;
//        //_PI.dataDir = _PI.mHddPathWW;
//        //string objName = "Human_INT5";
//        //string objFile = "ha_int5";
//        //uint objId = Hasher.HshString(objName);
//        //tmpFPOplacer replacer = new tmpFPOplacer(objId, objFile, objName);

//        //FpoBuilder fpoBuilder = new FpoBuilder();

//        //GameObject tmpUnit = fpoBuilder.BuildFPO(replacer.objData);
//        //Transform corrector = tmpUnit.transform.GetChild(0);
//        //Transform hull = corrector.GetChild(0);
//        //corrector.localRotation = hull.localRotation * Quaternion.LookRotation(Vector3.forward, Vector3.up);
//        //corrector.localRotation = Quaternion.Inverse(corrector.localRotation);
//        //tmpUnit.name = objName;

//        //ResourcePack pk = GameDataHolder.GetFPODB();
//        //Debug.Log("Building " + replacer.objData);
//        //Debug.Log("ID: " + pk.GetIdByName(objFile).ToString("X8")); //B48C4137 orig 46B4899C ww. WTF???
//        //Debug.Log("Index: " + pk.GetIndexByName(objFile)); //B48C4137 orig 46B4899C ww. WTF???
//        //Debug.Log("Name from ID: " + pk.GetNameById(pk.GetIdByName(objFile)));
//        //Debug.Log("Hasher: " + Hasher.HshString(replacer.objData).ToString("X8"));


//        //StormFileUtils.SaveXML<string[]>("strings.xml", pk.names); 
//    }

//    private struct debugme
//    {
//        string name;
//        int index;
//        uint id;
//    }

//    [MenuItem("Tests/Generate Cube")]
//    static void GenerateCube()
//    {
//        Vector3[] vertices;
//        int xSize = 6;
//        int ySize = 6;
//        int zSize = 6;

//        int cornerVertices = 8;
//        int edgeVertices = (xSize + ySize + zSize - 3) * 4;
//        int faceVertices = (
//            (xSize - 1) * (ySize - 1) +
//            (xSize - 1) * (zSize - 1) +
//            (ySize - 1) * (zSize - 1)) * 2;
//        vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];

//        int v = 0;
//        for (int y = 0; y <= ySize; y++)
//        {
//            for (int x = 0; x <= xSize; x++)
//            {
//                vertices[v++] = new Vector3(x, y, 0);
//            }

//            for (int z = 1; z <= zSize; z++)
//            {
//                vertices[v++] = new Vector3(xSize, y, z);
//            }
//            for (int x = xSize - 1; x >= 0; x--)
//            {
//                vertices[v++] = new Vector3(x, y, zSize);
//            }
//            for (int z = zSize - 1; z > 0; z--)
//            {
//                vertices[v++] = new Vector3(0, y, z);
//            }
//        }

//        for (int z = 1; z < zSize; z++)
//        {
//            for (int x = 1; x < xSize; x++)
//            {
//                vertices[v++] = new Vector3(x, ySize, z);
//            }
//        }
//        for (int z = 1; z < zSize; z++)
//        {
//            for (int x = 1; x < xSize; x++)
//            {
//                vertices[v++] = new Vector3(x, 0, z);
//            }
//        }

//        GameObject gobj = new GameObject("Generated cube");
//        MeshFilter mf = gobj.AddComponent<MeshFilter>();
//        MeshRenderer mr = gobj.AddComponent<MeshRenderer>();
//        Mesh mesh = new Mesh();
//        mesh.name = "Generated cube mesh";
//        mf.mesh = mesh;

//        int quads = (xSize * ySize + xSize * zSize + ySize * zSize) * 2;
//        int[] triangles = new int[quads * 6];

//        int ring = (xSize + zSize) * 2;
//        int t = 0;
//        v = 0;

//        for (int y = 0; y < ySize; y++, v++)
//        {
//            for (int q = 0; q < ring - 1; q++, v++)
//            {
//                t = SetQuad(triangles, t, v, v + 1, v + ring, v + ring + 1);
//            }
//            t = SetQuad(triangles, t, v, v - ring + 1, v + ring, v + 1);
//        }
//        t = CreateTopFace(triangles, t, ring);
//        t = CreateBottomFace(triangles, t, ring);

//        Spherisize();
//        mesh.vertices = vertices;
//        mesh.triangles = triangles;
//        mesh.RecalculateNormals();

//        int CreateTopFace(int[] triangles, int t, int ring)
//        {
//            int v = ring * ySize;
//            for (int x = 0; x < xSize - 1; x++, v++)
//            {
//                t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
//            }
//            t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);

//            int vMin = ring * (ySize + 1) - 1;
//            int vMid = vMin + 1;
//            int vMax = v + 2;

//            for (int z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++)
//            {
//                t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + xSize - 1);
//                for (int x = 1; x < xSize - 1; x++, vMid++)
//                {
//                    t = SetQuad(
//                        triangles, t,
//                        vMid, vMid + 1, vMid + xSize - 1, vMid + xSize);
//                }
//                t = SetQuad(triangles, t, vMid, vMax, vMid + xSize - 1, vMax + 1);
//            }

//            int vTop = vMin - 2;
//            t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
//            for (int x = 1; x < xSize - 1; x++, vTop--, vMid++)
//            {
//                t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
//            }
//            t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);
//            return t;
//        }

//        int CreateBottomFace(int[] triangles, int t, int ring)
//        {
//            int v = 1;
//            int vMid = vertices.Length - (xSize - 1) * (zSize - 1);
//            t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);
//            for (int x = 1; x < xSize - 1; x++, v++, vMid++)
//            {
//                t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
//            }
//            t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);

//            int vMin = ring - 2;
//            vMid -= xSize - 2;
//            int vMax = v + 2;

//            for (int z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++)
//            {
//                t = SetQuad(triangles, t, vMin, vMid + xSize - 1, vMin + 1, vMid);
//                for (int x = 1; x < xSize - 1; x++, vMid++)
//                {
//                    t = SetQuad(
//                        triangles, t,
//                        vMid + xSize - 1, vMid + xSize, vMid, vMid + 1);
//                }
//                t = SetQuad(triangles, t, vMid + xSize - 1, vMax + 1, vMid, vMax);
//            }

//            int vTop = vMin - 1;
//            t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
//            for (int x = 1; x < xSize - 1; x++, vTop--, vMid++)
//            {
//                t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
//            }
//            t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);

//            return t;
//        }

//        void Spherisize()
//        {
//            Vector3 center = new Vector3(xSize / 2, ySize / 2, zSize / 2);
//            Vector3 vertex;
//            for (int i=0;i<vertices.Length;i++)
//            {
//                vertex=  vertices[i] - center;
//                vertex = vertex.normalized * 10;
//                vertices[i] = vertex;

//            }
//        }
//    }

//    private static int SetQuad(int[] triangles, int i, int v00, int v10, int v01, int v11)
//    {
//        triangles[i] = v00;
//        triangles[i + 1] = triangles[i + 4] = v01;
//        triangles[i + 2] = triangles[i + 3] = v10;
//        triangles[i + 5] = v11;
//        return i + 6;
//    }

//    //[MenuItem("Tests/FBX exporter")]
//    //static void ExportFPOtoFBX()
//    //{
//    //    _PI.dataDir = _PI.mHddPathWW;
//    //    PackType DataDB = PackType.gData;
//    //    //for (int i = 0; i < 2; i++)
//    //    //{
//    //    //    DUST_DATA.loadDustData(i, DataDB);
//    //    //    ROADDATA.loadRoadData(i, DataDB);
//    //    //    EXPLOSION_DATA.loadExplData(i, DataDB);
//    //    //    DEBRIS_DATA.loadDebrData(i, DataDB);
//    //    //    SUBOBJ_DATA.loadSubobjData(i, DataDB);
//    //    //    OBJECT_DATA.loadObjectData(i, DataDB);
//    //    //}

//    //    VEHICLE_DATA.loadVehicleData(DataDB);
//    //    //CRAFT_DATA.loadCraftData(DataDB);
//    //    //CARRIER_DATA.loadCarrierData(DataDB);

//    //    //string[] ExportNames = new string[] {
//    //    //    "Human Avia Carrier",
//    //    //    "Human Avia Escort Vessel"
//    //    //};
//    //    List<string> tmpNames = new List<string>();
//    //    foreach (OBJECT_DATA objdata in OBJECT_DATA.Datas)
//    //    {
//    //        tmpNames.Add(objdata.FullName);
//    //    }
//    //    string[] ExportNames = tmpNames.ToArray();
//    //    Vector3 pos = Vector3.zero;
//    //    foreach (string search in ExportNames)
//    //    {
//    //        string filePath = Path.Combine("E:/Unity/" + search + ".fbx");
//    //        OBJECT_DATA objData = OBJECT_DATA.GetByName(search);
//    //        string objName = objData.FullName;
//    //        string objFile = objData.RootData.FileName;
//    //        uint objId = Hasher.HshString(objName);
//    //        tmpFPOplacer replacer = new tmpFPOplacer(objId, objFile, objName);
//    //        FpoBuilder fpoBuilder = new FpoBuilder();

//    //        GameObject tmpUnit = fpoBuilder.BuildFPO(replacer.objData);
//    //        Transform corrector = tmpUnit.transform.GetChild(0);
//    //        Transform hull = corrector.GetChild(0);
//    //        corrector.localRotation = hull.localRotation * Quaternion.LookRotation(Vector3.forward, Vector3.up);
//    //        corrector.localRotation = Quaternion.Inverse(corrector.localRotation);
//    //        tmpUnit.name = search;
//    //        //UnityEditor.Formats.Fbx.Exporter.ModelExporter.ExportObject(filePath, tmpUnit);

//    //        MEOData.MEO_DATA_HDR MEO = MEOData.MEO_DATA_HDR.LoadMEO(objFile);

//    //        float MEOSize = Mathf.Abs(MEO.MsnBounds[0] - MEO.MsnBounds[2]);
//    //        pos += Vector3.right * MEOSize * 0.75f;
//    //        //Debug.Log(MEOSize);
//    //        tmpUnit.transform.position = pos;
//    //        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Quad);
//    //        box.transform.position = pos;
//    //        box.transform.rotation = Quaternion.LookRotation(Vector3.up);
//    //        box.transform.localScale = new Vector3(Mathf.Abs(MEO.MsnBounds[0] - MEO.MsnBounds[2]), Mathf.Abs(MEO.MsnBounds[1] - MEO.MsnBounds[3]), 1);
//    //        pos += Vector3.right * MEOSize * 0.75f;
//    //    }

//    //    DUST_DATA.Datas.Clear();
//    //    ROADDATA.Datas.Clear();
//    //    EXPLOSION_DATA.Datas.Clear();
//    //    DEBRIS_DATA.Datas.Clear();
//    //    SUBOBJ_DATA.Datas.Clear();
//    //    OBJECT_DATA.Datas.Clear();
//    //}
//}




namespace KR.Graphics
{
    [System.Serializable]
    public class Gradient : UnityEngine.Gradient
    {
        [System.Serializable]
        public class GradientKey
        {
            public GradientKey(Color color, float time)
            {
                this.color = color;
                this.time = time;
            }

            public Color color;
            [Range(0, 1)]
            public float time;
        }

        public List<GradientKey> keys = new List<GradientKey>() { new GradientKey(Color.white, 0) };

        new public Color Evaluate(float time)
        {
            SortKeys();

            GradientKey lastKey = keys[keys.Count - 1];

            //If the time is over the time of the last key we return the last key color.
            if (time > lastKey.time)
            {
                return lastKey.color;
            }

            for (int i = 0; i < keys.Count - 1; i++)
            {
                GradientKey actualKey = keys[i];
                GradientKey nextKey = keys[i + 1];

                if (time >= actualKey.time && time <= keys[i + 1].time)
                {
                    return Color.Lerp(actualKey.color, nextKey.color, (time - actualKey.time) / (nextKey.time - actualKey.time));
                }
            }

            return keys[0].color;
        }

        public void SortKeys()
        {
            keys.Sort((p1, p2) => (p1.time.CompareTo(p2.time)));
        }
    }
}
public class StormUDBTests
{
    private static bool GetLocDB()
    {

        iUnifiedVariableContainer gpDescriptionCtr = null;
        iUnifiedVariableContainer gpDescriptionShortCtr = null;
        // open localization DBs
        iUnifiedVariableDB pDb = UniVarUtils.CreateUnifiedVariableDB(Constants.UNIVARS_VERSION, ProductDefs.GetPI().getHddFile(stormdata_dll.gspLocalizationName), true);

        if (pDb == null)
        {
            LogFactory.GetLog().Message(stormdata_dll.gspDbError, ProductDefs.GetPI().getHddFile(stormdata_dll.gspLocalizationName));
            return false;
        }

        //        pDb.GetRoot();
        Debug.Log("localization DB loaded: " + pDb + " [ " + ProductDefs.GetPI().getHddFile(stormdata_dll.gspLocalizationName) + " ]");
        gpDescriptionCtr = pDb.GetVariableTpl<iUnifiedVariableContainer>("\\Root\\Descriptions Full");
        gpDescriptionShortCtr = pDb.GetVariableTpl<iUnifiedVariableContainer>("\\Root\\Descriptions Short");
        Debug.Log("localization long descr DB loaded: " + (gpDescriptionCtr == null ? "Failed" : gpDescriptionCtr));
        Debug.Log("localization short descr DB loaded: " + (gpDescriptionShortCtr == null ? "Failed" : gpDescriptionShortCtr));

        return true;
    }
    [MenuItem("Tests/UDB/Test LocalizationUDB load")]
    public static void TestLocalizationUDB()
    {
        Debug.Log("GetLocDB: " + GetLocDB());
    }

    [MenuItem("Tests/UDB/UnivarDB load")]
    static void Message()
    {
        //UnivarDB profile = new UnivarDB();
        //profile.create();
        //if (!profile.openRoot()) throw new System.Exception("Failed to create root for profile");
        ////Debug.Log(profile);
        //profile.GetRoot().createString("Callsign").SetValue("Джейсон \"Wolf\" Скотт");
        ////value.SetValue("Джейсон \"Wolf\" Скотт");

        //profile.GetRoot().createContainer("Options");
        //profile.GetRoot().createContainer("Controls");
        //Debug.Log("Generated profile: " + profile);
        //Debug.Log(profile.GetRoot().createString("Callsign").GetValue());

        stdlogic_dll.DllMain();
        Debug.Log(stdlogic_dll.mpAiData);
    }
    [MenuItem("Tests/UDB/gdx loader")]
    static void LoadGDX()
    {
        string gdx = "Wind Warriors";
        stormdata_dll.LoadAll();
        var start = System.DateTime.Now;
        var gs = DataCoreFactory.CreateGameSet(ProductDefs.GetPI().getHddFile(gdx), null, new TestErrorLog());
        if (gs != null)
            Debug.Log("[" + gdx + "] [OK]");
        else
            Debug.Log("[" + gdx + "] [FAILED]");
        var end = System.DateTime.Now;

        Debug.Log("Loading time: " + (end - start));
        Debug.Log(gs.getDescription());
        stormdata_dll.ReleaseAll();
    }

    private class TestErrorLog : ILoadErrorLog
    {
        public void AddRef()
        {
            throw new System.NotImplementedException();
        }

        public void addWarning(int error_code, string name, string action)
        {
            Debug.Log(string.Format("{0} {1} {2}", error_code, name, action));
        }

        public int RefCount()
        {
            throw new System.NotImplementedException();
        }

        public int Release()
        {
            throw new System.NotImplementedException();
        }
    }
}
public class EditorImport
{
    private bool WasLoaded;
    public EditorImport()
    {
        Init();
    }
    ~EditorImport()
    {
        Dispose();
    }
    public void Init()
    {
        if (stormdata_dll.DataDB == null) { WasLoaded = true; stormdata_dll.LoadAll(); }
        if (renderer_dll.dll_data == null) { WasLoaded = true; renderer_dll.Attach(); }

        log = new LOG();

        mpRenderer = Renderer.CreateRenderer(null, log, null, RendererApi.RENDERER_VERSION);
        mpFpoLoader = (IFpoLoader)FpoLoader.CreateFpoLoader(mpRenderer, null, "objects2.dat", "objects.dat");
        if (mpFpoLoader == null) Debug.Log("FPO loader failed to load itself");
    }
    public void Dispose()
    {
        //mpFpoLoader.Release();
        if (WasLoaded)
        {
            stormdata_dll.ReleaseAll();
            renderer_dll.Detach();
        }
    }

    private Renderer mpRenderer;
    private IFpoLoader mpFpoLoader;
    private LOG log;
    //public GameObject ImportByFPOName(string FpoName)
    //{
    //    Vector3 pos = Vector3.zero;
    //    FPO myFPO;

    //    myFPO = mpFpoLoader.CreateFPO(FpoName);
    //    if (MaterialStorage.DefaultSolid == null) MaterialStorage.DefaultSolid = new Material(Shader.Find("HDRP/Lit"));
    //    if (MaterialStorage.DefaultTransparent == null) MaterialStorage.DefaultTransparent = new Material(Shader.Find("HDRP/Lit"));
    //    //myFPO = mpFpoLoader.CreateFPO("vs_block3_1");
    //    try
    //    {
    //        //GameObject Gobj = CreateGameObjectFull(myFPO);
    //        GameObject Gobj = DrawFpoParticle.DrawFPO(myFPO);
    //        Gobj.transform.position = pos;
    //        //ExportGameObject(Gobj);
    //        return Gobj;
    //    }
    //    catch (System.Exception e)
    //    {
    //        log.Message("Failed to gen FPO " + FpoName + e.ToString());
    //        throw;
    //    }
    //}

    //public GameObject ImportByObjectData(OBJECT_DATA od)
    //{
    //    SUBOBJ_DATA root = od.RootData;
    //    string filename = root.FileName;
    //    return ImportByFPOName(filename);
    //}

    //public GameObject ImportByObjectDataName(string ObjectDataName)
    //{
    //    OBJECT_DATA od = OBJECT_DATA.GetByName(ObjectDataName);
    //    if (od == null) return null;

    //    return ImportByObjectData(od);
    //}

    //public void ImportClass(DWORD ClassID)
    //{
    //    foreach (OBJECT_DATA oData in OBJECT_DATA.Datas)
    //    {
    //        if (oData.GetClass() != ClassID) continue;
    //        Debug.Log("Loading " + oData.FullName);
    //    }
    //}

    //public void ExportObjectClass(DWORD ClassID)
    //{
    //    Dictionary<string, GameObject> gobjs = new Dictionary<string, GameObject>();
    //    foreach (OBJECT_DATA oData in OBJECT_DATA.Datas)
    //    {
    //        if (oData.GetClass() != ClassID) continue;
    //        Debug.Log(string.Format("Exporting {0} {1}", oData.GetClassName(), oData.FullName));
    //        GameObject gobj = ImportByObjectData(oData);
    //        //gobjs.Add(oData.FullName,gobj);
    //        ExportGameObject(gobj, oData.FullName + ".fbx");
    //        GameObject.DestroyImmediate(gobj);
    //    }
    //    //renderer_dll.dll_data.ImportTextures();
    //    //foreach (var exportable in gobjs)
    //    //{
    //    //    ExportGameObject(exportable.Value, exportable.Key);
    //    //    GameObject.DestroyImmediate(exportable.Value);
    //    //}
    //}

    //public void ExportSubobjClass(DWORD ClassID)
    //{
    //    foreach (SUBOBJ_DATA sData in SUBOBJ_DATA.Datas)
    //    {
    //        if (sData.GetClass() != ClassID) continue;
    //        Debug.Log(string.Format("Exporting {0} {1}", sData.GetClassName(), sData.FullName));
    //        GameObject gobj = ImportByFPOName(sData.FileName);
    //        ExportGameObject(gobj, sData.FullName + ".fbx");
    //        GameObject.DestroyImmediate(gobj);
    //    }
    //}

    //public void ExportAllSubobjClasses()
    //{
    //    foreach (SUBOBJ_DATA sData in SUBOBJ_DATA.Datas)
    //    {
    //        string classname = sData.GetClassName();
    //        Debug.Log(string.Format("Exporting {0} {1}", classname, sData.FullName));
    //        GameObject gobj = ImportByFPOName(sData.FileName);
    //        if (!Directory.Exists(classname)) Directory.CreateDirectory(classname);
    //        ExportGameObject(gobj, classname + "/" + sData.FullName + ".fbx");
    //        GameObject.DestroyImmediate(gobj);
    //    }
    //}
    public static void ExportGameObject(UnityEngine.Object singleObject, string filePath = "e:/debug.fbx")
    {
        Type[] types = AppDomain.CurrentDomain.GetAssemblies().First(x => x.FullName == "Unity.Formats.Fbx.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null").GetTypes();
        Type optionsInterfaceType = types.First(x => x.Name == "IExportOptions");
        Type optionsType = types.First(x => x.Name == "ExportOptionsSettingsSerializeBase");
        Type temp = types.First(x => x.Name == "ExportModelSettingsSerialize");
        // Instantiate a settings object instance
        MethodInfo optionsProperty = typeof(ModelExporter).GetProperty("DefaultOptions", BindingFlags.Static | BindingFlags.NonPublic).GetGetMethod(true);
        object optionsInstance = optionsProperty.Invoke(null, null);

        // Change the export setting from ASCII to binary
        FieldInfo exportFormatField = optionsType.GetField("exportFormat", BindingFlags.Instance | BindingFlags.NonPublic);
        exportFormatField.SetValue(optionsInstance, 1);

        // Set the embed textures option to true
        FieldInfo embedTexturesField = temp.GetField("embedTextures", BindingFlags.Instance | BindingFlags.NonPublic);

        if (embedTexturesField != null)
        {
            embedTexturesField.SetValue(optionsInstance, true);
            Debug.Log("Embed textures set to true.");
            bool isEmbedTexturesSet = (bool)embedTexturesField.GetValue(optionsInstance);
            Debug.Log("Embed textures actually set to: " + isEmbedTexturesSet);
        }
        else
        {
            //optionsType.GetFields().ForEach(f => { Debug.Log(f.Name); });
            Debug.LogError("Failed to set embed textures.");
        }

        // Invoke the ExportObject method with the settings param
        MethodInfo exportObjectMethod = typeof(ModelExporter).GetMethod("ExportObject", BindingFlags.Static | BindingFlags.NonPublic, Type.DefaultBinder, new Type[] { typeof(string), typeof(UnityEngine.Object), optionsInterfaceType }, null);
        exportObjectMethod.Invoke(null, new object[] { filePath, singleObject, optionsInstance });

        Debug.Log(filePath);
    }
}
