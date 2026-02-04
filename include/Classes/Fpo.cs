using geombase;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using static HashFlags;

//public class FlagHolder 
//{
//    uint flags;

//    public bool MatchGroup(uint f)
//    {
//        return GetFlag(OF_GROUP_MASK & f) !=0;
//    }
//    public bool MatchType(uint f) { return GetFlag(OF_USER_MASK & f)!=0; }
//    // this nmethod used by hash system
//    public bool MatchFlags(uint f) { return MatchGroup(f) && MatchType(f); }

//    public uint GetFlag(uint f) { return flags & f; }
//    public uint SetFlag(uint f) { flags |= f; return f; }
//    public void ClearFlag(uint f) { flags &= ~f; }
//}


//public abstract class Fpo : TmTree,IHashObject
public abstract class Fpo : TmTree
{
    //Debug
    public string Name;
    //global constants
    public const int FPORF_PASS_VALID = 0x00000001;
    public const int FPORF_SPHERE_VALID = 0x00000002;

    // flags used by SetImage method
    public const int FSI_EQUAL_LINKS = 0x00000001;
    public const int FSI_NONEQUAL_LINKS = 0x00000002;
    public const int FSI_IGNORE_LINKS = 0x00000003;
    public const int FSI_FORCE = 0x00000004;
    public const int FSI_ROUND_UP = 0x00000010;
    public const int FSI_ROUND_DOWN = 0x00000020;

    new public const uint ID = 0x869BD2A0;

    public FpoData data;

    public int image;

    // Renderring
    Flags r_flags;      //  render flags
    Sphere r_sphere;     //  render sphere
    public IRData[] r_images = new IRData[4];  //  loaded rendering images (2xlod)

    // Collision
    Sphere c_sphere;    //  collision sphere
    public CollisionData[] boxes = new CollisionData[4];


    // Game related

    public object link;

    void SetLink(object t)
    {
        link = t;
    }

    int GetImage() { return image; }
    int SetImage(int nImage, Flags f, object link_etalon)
    {
        if (image == nImage && f.Get(FSI_FORCE) == 0) return image;
        bool link_ok =
          f.Get(FSI_IGNORE_LINKS) != 0 ||
          (f.Get(FSI_EQUAL_LINKS) != 0 && link == link_etalon) ||
          (f.Get(FSI_NONEQUAL_LINKS) != 0 && link != link_etalon);
        if (link_ok == false) return image;

        SwitchToImage(nImage, f);

        for (TmTree i = Begin(); i != null; i = i.Next())
        {
            Fpo p = CvtTMT(i, TMTID_FPO);
            if (p != null) p.SetImage(nImage, f, link_etalon);
        }

        return image;
    }

    Fpo CvtTMT(TmTree tmt, uint TMTID_)
    {
        Asserts.Assert(tmt.MatchGroup(OF_GROUP_TMT));
        return tmt.GetFlag(TMTID_) != 0 ? (Fpo)tmt : null;
    }
    int SwitchToImage(int nImage, Flags f)
    {
        if (image != nImage)
        {
            if (f.Get(FSI_ROUND_UP) != 0)
                while (r_images[nImage] != null && nImage < 3) ++nImage;

            if (f.Get(FSI_ROUND_DOWN) != 0)
                while (r_images[nImage] == null && nImage > 0) --nImage;

            Asserts.Assert(nImage >= 0 && nImage <= 3);
            image = nImage;
            SetCSphere();
        }
        return image;
    }


    // rendering help methods

    //похоже, не используются:
    //void SetParentRFlag(int f);
    //int ResetRFlags();

    //  Collision Methods
    Sphere GetImageSphere() { return data.images[image].GetBSphere(); }
    bool CollideSphere(Sphere s, bool simple = false)
    {
        s.o = tm.TransformPoint(s.o, false);
        if (!geombase.Inline.IsIntersected(s, c_sphere))
            return false;

        if (simple || boxes[image] == null || CollideClDataWithSphere(boxes[image], s))
            return true;

        for (var i = Begin(); i!=null; i=i.Next())
        {
            Fpo f = CvtTMT(i, TMTID_FPO);
            if (f!=null && f.CollideSphere(s, simple))
                return true;
        }
        return false;
    }

    public static bool CollideClDataWithSphere(CollisionData cd, Sphere sp)
    {
        if (!geombase.Inline.IsIntersected(cd.b_sphere, sp))
            return false;

        short[] groups = cd.GetGroups();
        geombase.Plane[] planes = cd.GetPlanes();

        for (int i = 0; i < cd.n_groups; i++)
        {
            int numpl = groups[i];
            if (BoxSphereCollision(sp, numpl, planes))
                return true;
            planes = TrimArray(planes, numpl + 1);
        }

        return false;
    }

    private static geombase.Plane[] TrimArray(geombase.Plane[] planes, int num)
    {
        List<geombase.Plane> tmpList = new List<geombase.Plane>(planes);
        tmpList.RemoveRange(0, num);
        return tmpList.ToArray();
    }

    static bool BoxSphereCollision(Sphere sp, int n_pl, geombase.Plane[] pl)
    {
        // check first distance2 to bound_sphere of box 
        if (!geombase.Inline.IsIntersected(sp, new Sphere(pl[0].n, pl[0].d)))
            return false;

        for (int i = 1; i < n_pl; i++)
        {
            if (!geombase.Inline.IsIntersected(sp, pl[i]))
            {
                return false;
            }
        }
        return true;

    }

    public void SetCSphere()
    {
        c_sphere = boxes[image] != null ? boxes[image].b_sphere : GetImageSphere();
        h_radius = c_sphere.r + c_sphere.o.magnitude;
        //HashRadius() = c_sphere.r + c_sphere.o.magnitude;
    }

    // FPO wrapper 
    Vector3 Min() { return data.images[image].min; }
    Vector3 Max() { return data.images[image].max; }

    Vector3 Org() { return tm.pos; }
    Vector3 Left() { return tm.tm[0]; }
    Vector3 Up() { return tm.tm[1]; }
    Vector3 Dir() { return tm.tm[2]; }

    void UpDir2Left() { tm.tm[0] = Vector3.Cross(tm.tm[1], tm.tm[2]); }
    void DirLeft2Up() { tm.tm[1] = Vector3.Cross(tm.tm[2], tm.tm[0]); }
    void LeftUp2Dir() { tm.tm[2] = Vector3.Cross(tm.tm[0], tm.tm[1]); }

    public Fpo() : base(TMTID_FPO) { }
    //public override void Destroy()
    //{
    //    throw new NotImplementedException();
    //}

    //STUB
    //TODO Реализовать корректно класс Fpo
}
