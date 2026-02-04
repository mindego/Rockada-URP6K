using UnityEngine;

static class routemath
{
    public static float Acos(float a) { if (a > 1f) a = 1f; if (a < -1f) a = -1f; return Mathf.Acos(a); }

    public static float getVAngle(float len, float height) { return (len > 0) ? Mathf.Asin(height / len) : 0; }

    public static float getHAngle(Vector3 v1, Vector3  v2 )
    {
        Vector3 t1 = new Vector3(v1.x, 0, v1.z);
        Vector3  t2 = new Vector3(v2.x, 0, v2.z);
        float lt1 = t1.magnitude;
        float lt2 = t2.magnitude;

        float angle = (lt1 > 0 && lt2 > 0) ? Acos((Vector3.Dot(t1 , t2)) / (lt1 * lt2)) : 0;

        if (t2.x * t1.z - t2.z * t1.x < 0f)
            angle *= -1;
        return angle;
    }

    public static float getVAngle(Vector3 v1)
    {        return getVAngle(v1.magnitude, v1.y);
    }
}