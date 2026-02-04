using System;
using UnityEngine;

class CarrierPhysics
{
    public CarrierPhysics(MATRIX pos, Vector2 speed, Vector2 aspeed, CARRIER_DATA data)
    {
        myPos = pos;
        myData = data;
        //myLSpeed = speed; //Передаются по ссылке
        //myASpeed = aspeed;
    }

    public float approachFloat(float cur, float tgt, float max_delta, float min_value, float max_value)
    {
        float delta = tgt - cur;
        float sgn = sign(delta);
        delta *= sgn;
        if (delta > max_delta)
            delta = max_delta;
        cur += sgn * delta;
        return Mathf.Clamp(cur, min_value, max_value);
    }

    public void update(Vector3 org, float time, float scale, float radius, Vector3 dir,ref Vector2 myLSpeed ,ref Vector2 myASpeed)
    {
        //Debug.Log("Debugging phys update");
        //Debug.Log("Org: " + org);
        //Debug.Log("myPos.Org: " + myPos.Org);
        Vector3 target_dir = org - myPos.Org;
        float vert_delta = target_dir.y;
        target_dir.y = 0;
        float diff_len = target_dir.magnitude;
        target_dir.y = vert_delta;

        float target_vangle = routemath.getVAngle(target_dir);
        if (Mathf.Abs(target_vangle) > myData.PitchLimit)
            target_vangle = sign(target_vangle) * myData.PitchLimit;
        float vangle = target_vangle - routemath.getVAngle(myPos.Dir);

        float hangle = routemath.getHAngle(myPos.Dir, target_dir);
        if (Mathf.Abs(hangle) > Storm.Math.GRD2RD(45f))
        {
            if (diff_len < radius * 0.5f)
            {
                if (myLSpeed.x > 0)
                    hangle = 0;
                else if (diff_len < radius * 0.1f)
                    hangle = routemath.getHAngle(myPos.Dir, dir);
            }
            diff_len = 0;
        }

        float max_speed = (time <= 0) ? myData.MaxSpeedZ : diff_len / time;
        myLSpeed.x = approachFloat(myLSpeed.x, getHSpeedFromDist(diff_len), myData.MaxAccelZ * scale, 0, max_speed);
        myLSpeed.y = approachFloat(myLSpeed.y, getVSpeedFromDist(vert_delta), myData.MaxAccelY * scale, -myData.MaxSpeedY, myData.MaxSpeedY);

        myASpeed.x = approachFloat(myASpeed.x, getASpeedFromAngle(hangle), myData.AAccelX * scale, -myData.ASpeedX, myData.ASpeedX);
        myASpeed.y = approachFloat(myASpeed.y, getASpeedFromAngle(vangle), myData.AAccelY * scale, -myData.ASpeedY, myData.ASpeedY);
    }

    private float getHSpeedFromDist(float d)
    {
        return Mathf.Pow(d, 0.8f);
    }
    private float getASpeedFromAngle(float d)
    {
        float sign = (d < 0) ? -1 : 1;
        d *= sign;
        return sign * Storm.Math.GRD2RD(Mathf.Pow(Storm.Math.RD2GRD(d), 0.75f));
    }

    private float getVSpeedFromDist(float d)
    {
        return (d < 0) ? -Mathf.Sqrt(-d) : Mathf.Sqrt(d);
    }

    private Vector2 myASpeed;
    private Vector3 myLSpeed;

    private MATRIX myPos;
    private CARRIER_DATA myData;

    public static float sign(float x) { return (x < 0) ? -1 : 1; }

    internal void updateOrg(MATRIX pos)
    {
        myPos = pos;
    }
};
