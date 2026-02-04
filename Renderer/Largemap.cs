using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public class Largemap : ICompositeMap
{
    IBill bill;
    ITexturesDB mTextures;
    IBObject bobject;
    int MapSx, MapSy;
    float MetersPerCellX;
    float MetersPerCellY;
    readonly char[] Digits = new char[16] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

    Texture2D[,] MapArray;
    Texture2D MapDefault;

    const int NameSuffMaxLen = 16;
    string Name;
    string NameSuff;

    void SetTexture(int x, int z)
    {
        Texture2D Tex = ((x < 0) || (z < 0) || (x >= MapSx) || (z >= MapSy)) ? LoadDefTexture() : LoadCasualTexture(x, z);
        bill.SetTexture(Tex);
    }
    Texture2D LoadDefTexture()
    {
        if (MapDefault == null)
        {
            NameSuff = "Def";
            MapDefault = mTextures.CreateTexture(Hasher.HshString(Name + NameSuff));
        }
        return MapDefault;
    }
    Texture2D LoadCasualTexture(int i, int j)
    {
        Assert.IsTrue(j < MapSy && i < MapSx);
        if (MapArray[j, i] == null)
        {
            //NameSuff[0] = Digits[i];
            //NameSuff[1] = Digits[j];
            //NameSuff[2] = 0;
            NameSuff = new string(new char[] { Digits[i], Digits[j] });
            MapArray[j, i] = mTextures.CreateTexture(Hasher.HshString(Name + NameSuff));
            //    Engine::log->Message("Mapload taxture %s\n", buf);
        }
        return MapArray[j, i];
    }
    public Largemap(IBill _bill, MapData d, ITexturesDB db)
    {
        bill = _bill;
        MapArray = new Texture2D[d.MapSizeY, d.MapSizeX];
        bobject = bill.Create(4, 2);
        mTextures = db;

        MapSx = d.MapSizeX;
        MapSy = d.MapSizeY;
        MetersPerCellX = d.SizeX * 255 / (MapSx * 255 + 1f);
        MetersPerCellY = d.SizeY * 255 / (MapSy * 255 + 1f);

        int len = d.Name.Length;
        Name = d.Name;
        NameSuff = Name + len;

        bobject.SetActiveMesh(4, 2);
        bobject.SetBill(bill);


        for (int y = 0; y < MapSy; ++y)
            for (int x = 0; x < MapSx; ++x)
                MapArray[y, x] = null;
        MapDefault = null;
    }
    ~Largemap()
    {
        Dispose();
    }

    public void Dispose()
    {
        Flush();
        Name = null;
        bobject.Release();
    }
    public void Flush()
    {
        for (int j = 0; j < MapSy; j++)
            for (int i = 0; i < MapSx; i++)
            {
                if (MapArray[j, i] != null) MapArray[j, i] = null;
            }
        if (MapDefault != null)
        {
            //MapDefault->Release();
            MapDefault = null;
        }
    }
    public void Draw(float X, float Z, float Width, float Height, Color32 color)
    {
        float[] S = new float[4];
        float HalfTexel = 1f / 256f * 0.5f;
        float[] M = new float[4] { 0 + HalfTexel, 1 - HalfTexel, 1 - HalfTexel, 0 + HalfTexel };

        int temp;
        int StartX = (int)Mathf.Floor((X - Width * .5f) / MetersPerCellX),
            EndX = (int)Mathf.Floor((X + Width * .5f) / MetersPerCellX);

        int StartZ = (int)Mathf.Floor((Z - Height * .5f) / MetersPerCellY),
            EndZ = (int)Mathf.Floor((Z + Height * .5f) / MetersPerCellY);

        S[2] = MetersPerCellX;
        S[3] = MetersPerCellY;

        for (int z = StartZ; z <= EndZ; z++)
        {
            for (int x = StartX; x <= EndX; x++)
            {
                SetTexture(x, z);

                S[0] = x * MetersPerCellX;
                S[1] = z * MetersPerCellY;
                bobject.name = "Map " + x + ":" + z;
                bobject.Lock();
                //#define SetO(Id,cX,cY) bobject->SetVertO( Id, FVec2( S[0]+cX*S[2],S[1]+cY*S[3] ), FVec2(M[cX*2],M[cY*2+1]))
                //                SetO(0, 0, 0);
                //                SetO(1, 1, 0);
                //                SetO(2, 1, 1);
                //                SetO(3, 0, 1);
                //#undef SetO
                bobject.SetVertO(0, new Vector2(S[0] + 0 * S[2], S[1] + 0 * S[3]), new Vector2(M[0 * 2], M[0 * 2 + 1]));
                bobject.SetVertO(1, new Vector2(S[0] + 1 * S[2], S[1] + 0 * S[3]), new Vector2(M[1 * 2], M[0 * 2 + 1]));
                bobject.SetVertO(2, new Vector2(S[0] + 1 * S[2], S[1] + 1 * S[3]), new Vector2(M[1 * 2], M[1 * 2 + 1]));
                bobject.SetVertO(3, new Vector2(S[0] + 0 * S[2], S[1] + 1 * S[3]), new Vector2(M[0 * 2], M[1 * 2 + 1]));
                bobject.SetFace(0, 0, 1, 2);
                bobject.SetFace(1, 0, 2, 3);

                bobject.SetVertC(0, 4, color, Color.black);

                bobject.UnLock();

                bill.Draw(bobject);
            }
        }
    }

    //public void GenMap(float X, float Z, float Width, float Height, Color32 color)
    //{
    //    float[] S = new float[4];
    //    float HalfTexel = 1f / 256f * 0.5f;
    //    float[] M = new float[4] { 0 + HalfTexel, 1 - HalfTexel, 1 - HalfTexel, 0 + HalfTexel };

    //    int temp;
    //    int StartX = (int)Mathf.Floor((X - Width * .5f) / MetersPerCellX),
    //        EndX = (int)Mathf.Floor((X + Width * .5f) / MetersPerCellX);

    //    int StartZ = (int)Mathf.Floor((Z - Height * .5f) / MetersPerCellY),
    //        EndZ = (int)Mathf.Floor((Z + Height * .5f) / MetersPerCellY);

    //    S[2] = MetersPerCellX;
    //    S[3] = MetersPerCellY;

    //    Debug.Log(string.Format("StartX {0} EndX {1} MetersPerCellX {2}", StartX, EndX, MetersPerCellX));
    //    Debug.Log(string.Format("StartZ {0} EndZ {1} MetersPerCellY {2}", StartZ, EndZ, MetersPerCellY));
    //    for (int z = StartZ; z <= EndZ; z++)
    //    {
    //        for (int x = StartX; x <= EndX; x++)
    //        {
    //            SetTexture(x, z);

    //            S[0] = x * MetersPerCellX;
    //            S[1] = z * MetersPerCellY;
    //            Texture2D imp = bill.GetTexture();
    //            Debug.Log(imp);
    //            AssetDatabase.CreateAsset(imp, "Assets/" + Name + "_" + x + "-" + z + ".texture2D");
    //        }
    //    }
    //}

    public int Release()
    {
        Dispose();
        return 0;
    }
}