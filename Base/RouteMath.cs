using UnityEngine;

public static class RouteMath
{
    const float CEpsilon = 0.001f;
    const float FEPS = 0.00001f;
    public static bool CCmp(float x) { return Mathf.Abs(x) < CEpsilon; }



    public static void SetupMatrix(Vector3 n_dir, Vector3 n_up, Vector3 new_org, float spd, ref MATRIX matr, bool setup_org = true)
    {
        if (spd > 1f) spd = 1f;
        float d;
        d = n_dir.magnitude;
        SetupMatrixN(n_dir, d, n_up, spd, ref matr);
        if (setup_org)
            matr.Org = new_org;
    }
    public static void SetupMatrixN(Vector3 n_dir, float nrm_dir, Vector3 n_up, float spd, ref MATRIX matr)
    {
        Vector3 diff = matr.Dir * nrm_dir;
        matr.Dir = diff + (n_dir - diff) * spd;
        matr.Dir.Normalize();

        diff = n_up - matr.Up;
        diff *= spd;
        matr.Up += diff;
        matr.Up.Normalize();

        matr.Right = Vector3.Cross(matr.Up, matr.Dir);

        matr.Up = Vector3.Cross(matr.Dir, matr.Right);
    }

    public static float ApproachValue(float src, float dst, float max_delta)
    {
        float dlt = dst - src;

        if (dlt > 0)
        {
            if (dlt > max_delta)
                dlt = max_delta;
        }
        else
          if (dlt < -3f * max_delta)
            dlt = -3f * max_delta;

        return src + dlt;
    }

    public static float ApproachVector(Vector3 _src, Vector3 _dst, bool hor , out bool to_right)
    {
        Vector3 src = _src;
        Vector3 dst = _dst;

        if (hor)
        {
            src.y = 0;
            dst.y = 0;
            float d1 = src.magnitude;
            float d2 = dst.magnitude;

            float ca = Vector3.Dot(src, dst) / (d1 * d2);
            if (dst.x * src.z - dst.z * src.x < 0f)
                to_right = false;
            else
                to_right = true;
            return ca;
        }
        else
        {
            float d1 = src.magnitude;
            float d2 = dst.magnitude;
            src /= d1;
            src *= d2;
            src.x = dst.x;
            src.z = dst.z;
            float ca = Vector3.Dot(src,dst) / Mathf.Pow(d2,2);
            if (dst.y < src.y)
                to_right = false;
            else
                to_right = true;
            return ca;
        }
    }
}