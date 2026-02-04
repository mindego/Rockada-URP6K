using UnityEngine;

public class CollisionDefines
{
    public const uint COLL_VERSION = 0xF7CCA2F6;// Collision 1.1

    public const uint COLLF_WITH_GROUND = 0x00000001;
    public const uint COLLF_WITH_WATER = 0x00000002;
    public const uint COLLF_WITH_OBJECT = 0x00000004;
    public const uint COLLF_WITH_OBJECT2 = 0x00000008;
    public const uint COLLF_FIRST = 0x00000010;

    public const uint COLLF_WITH_TERRAIN = (COLLF_WITH_GROUND | COLLF_WITH_WATER);
    public const uint COLLF_ALL = (COLLF_WITH_TERRAIN | COLLF_WITH_OBJECT | COLLF_FIRST | COLLF_WITH_OBJECT2);

}

public static class CollisionUtils
{
    public static bool OOBIntersect(Vector3 rayOrg, Vector3 rayDir, Vector3 targetPos, Vector3 targetDir, Vector3 targetUp, Vector3[] bounds, out Vector3[] res)
    {
        float tMin = 0f;
        float tMax = 7000f;
        Vector2 intersectPoints;

        Vector3 bbRayDelta = targetPos - rayOrg;
        Vector3 targetRight = Vector3.Cross(targetDir, targetUp);
        res = new Vector3[2];

        if (OBBIntersectAxis(rayDir, bbRayDelta, targetRight, new Vector2(bounds[0].x, bounds[1].x), out intersectPoints))
        {
            tMin = Mathf.Max(tMin, intersectPoints.x); tMax = Mathf.Min(tMax, intersectPoints.y);
        }
        if (OBBIntersectAxis(rayDir, bbRayDelta, targetUp, new Vector2(bounds[0].y, bounds[1].y), out intersectPoints))
        {
            tMin = Mathf.Max(tMin, intersectPoints.x); tMax = Mathf.Min(tMax, intersectPoints.y);
        }

        if (OBBIntersectAxis(rayDir, bbRayDelta, targetDir, new Vector2(bounds[0].z, bounds[1].z), out intersectPoints))
        {
            tMin = Mathf.Max(tMin, intersectPoints.x); tMax = Mathf.Min(tMax, intersectPoints.y);
        }


        res[0] = rayOrg + rayDir * tMin;
        res[1] = rayOrg + rayDir * tMax;
        return tMin < tMax;
    }

    public static bool OBBIntersectAxis(Vector3 rayDir, Vector3 bbRayDelta, Vector3 axis, Vector2 bounds, out Vector2 res)
    {
        float nomLen = Vector3.Dot(axis, bbRayDelta);
        float denomLen = Vector3.Dot(rayDir, axis);
        res = new Vector2(0, 0);

        float min, max;
        if (Mathf.Abs(denomLen) > 0.00001f)
        {
            min = (nomLen + (bounds.x)) / denomLen;
            max = (nomLen + (bounds.y)) / denomLen;
            //if (min < max) { tMin = Mathf.Max(tMin, min); tMax = Mathf.Min(tMax, max); } else { tMin = Mathf.Max(tMin, max); tMax = Mathf.Min(tMax, min); }
            if (min < max) { res.x = min; res.y = max; } else { res.x = max; res.y = min; }
            Debug.Log(string.Format("Minmax {0} {1}", min, max));
            if (res.x > res.y) return false;
        }
        else
        {
            Debug.Log("No hit. Examine later");
        }

        return true;
    }
}