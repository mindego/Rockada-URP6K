using static LIGHTTYPE;
using static LIGHTBHVR;
using UnityEngine;
using UnityEngine.Assertions;
using static D3DEMULATION;
using static D3DEMULATION.D3DLIGHTTYPE;


public enum LIGHTTYPE
{
    LT_NONE = 0,
    LT_POINT = 1,
    LT_SPOT = 2,
    LT_DIRECTIONAL = 3
};
public enum LIGHTBHVR
{
    LB_STATIC,
    LB_DYNAMIC
};
public class LIGHT : IObject
{
    //friend class LightManager;

    LIGHTTYPE type;
    LIGHTBHVR bev;

    public LightID id;

    Vector3 mPosition;
    Vector3 mDirection;

    Color mDiffuse;
    Color mAmbient;
    Color mSpecular;

    Color mObsoleteColor;
    float mOO_R2;
    float mRadius;

    float mIntensity;

    int mRefCount;

    public D3DLightData GetD3DLight()
    {
        D3DLightData l = new D3DLightData();

        l.d3d_desc.dcvDiffuse.a = 1;
        l.d3d_desc.dcvDiffuse.r = mDiffuse.r * mIntensity;
        l.d3d_desc.dcvDiffuse.g = mDiffuse.g * mIntensity;
        l.d3d_desc.dcvDiffuse.b = mDiffuse.b * mIntensity;
        l.d3d_desc.dcvAmbient.a = 1;
        l.d3d_desc.dcvAmbient.r = mAmbient.r * mIntensity;
        l.d3d_desc.dcvAmbient.g = mAmbient.g * mIntensity;
        l.d3d_desc.dcvAmbient.b = mAmbient.b * mIntensity;
        l.d3d_desc.dcvSpecular.a = 1;
        l.d3d_desc.dcvSpecular.r = mSpecular.r * mIntensity;
        l.d3d_desc.dcvSpecular.g = mSpecular.g * mIntensity;
        l.d3d_desc.dcvSpecular.b = mSpecular.b * mIntensity;

        l.d3d_desc.dvPosition = new D3DVECTOR(0, 0, 0);
        l.d3d_desc.dvDirection = new D3DVECTOR(0, 0, 0);
        l.d3d_desc.dvRange = 0;
        l.d3d_desc.dvFalloff = 0;
        l.d3d_desc.dvAttenuation0 = 0;
        l.d3d_desc.dvAttenuation1 = 0;
        l.d3d_desc.dvAttenuation2 = 0;
        l.d3d_desc.dvTheta = 0;
        l.d3d_desc.dvPhi = 0;

        switch (type)
        {
            case LT_POINT:
                l.d3d_desc.dltType = D3DLIGHT_POINT;
                l.d3d_desc.dvPosition = ToD3DVECTOR(Engine.WorldToEngineWorld(mPosition));//(D3DVECTOR)(((MATRIX)(ENGINE::Camera)).ExpressPoint(Org));
                l.d3d_desc.dvRange = mRadius;
                SetAttenuation(mRadius, ref l.d3d_desc);
                break;
            case LT_SPOT:
                l.d3d_desc.dltType = D3DLIGHT_SPOT;
                l.d3d_desc.dvPosition = ToD3DVECTOR(Engine.EngineCamera.ExpressPoint(mPosition));
                l.d3d_desc.dvDirection = ToD3DVECTOR(Engine.EngineCamera.ExpressVector(mDirection));
                l.d3d_desc.dvRange = mRadius;
                l.d3d_desc.dvAttenuation0 = 1;
                l.d3d_desc.dvAttenuation1 = 0;
                l.d3d_desc.dvAttenuation2 = 0;
                l.d3d_desc.dvFalloff = 0;
                l.d3d_desc.dvTheta = .01f;
                l.d3d_desc.dvPhi = .5f;
                break;
            case LT_DIRECTIONAL:
                l.d3d_desc.dltType = D3DLIGHT_DIRECTIONAL;
                l.d3d_desc.dvDirection = ToD3DVECTOR(mDirection);//(D3DVECTOR)(((MATRIX)(ENGINE::Camera)).ExpressVector(Dir));
                break;
        }
        return l;
    }

    void SetAttenuation(float R, ref D3DLIGHT7 l)
    {
        float RMID = R * .2f, RMAX = R;
        float delta = .5f, eps = .01f;
        /*
        l.d3d_desc.dvAttenuation0=1;
        l.d3d_desc.dvAttenuation2=(1/(eps*RMAX)-1/(delta*RMID))/(RMAX-RMID)+1/(RMAX*RMID);
        l.d3d_desc.dvAttenuation1=(RMID/(eps*RMAX)-RMAX/(delta*RMID))/(RMID-RMAX)-(RMID+RMAX)/(RMID*RMAX);
        */
        l.dvAttenuation0 = 1;
        l.dvAttenuation2 = (1 / eps - 1) / (R * R);
        l.dvAttenuation1 = 0;

    }

    D3DVECTOR ToD3DVECTOR(Vector3 v)
    {
        return new D3DVECTOR(v.x, v.y, v.z);
    }
    ~LIGHT() { }
    public LIGHT(LIGHTTYPE type = LT_POINT, LIGHTBHVR behav = LB_DYNAMIC)
    {
        this.type = type;
        bev = behav;
        mRefCount = 1;

        mIntensity = 1;
        mDiffuse = mAmbient = mSpecular = Color.white;

        switch (type)
        {
            case LT_POINT:
                mRadius = 1;
                break;
            case LT_SPOT:
                mRadius = 1;
                break;
            case LT_DIRECTIONAL:
                break;
        }

        id = new LightID();
        id.index = null;
    }

    public static LIGHT Create(LIGHTTYPE type = LT_POINT, LIGHTBHVR behav = LB_DYNAMIC)
    {
        return new LIGHT(type, behav);
    }

    public LIGHTTYPE GetLightType() { return type; }

    public float GetRadius() { return mRadius; }
    public void SetRadius(float r) { mRadius = r; }

    public float GetIntensity() { return mIntensity; }
    public void SetIntensity(float i) { mIntensity = i; }

    public Color GetColor() { return mDiffuse; }
    public void SetColor(Color c) { mDiffuse = mAmbient = mSpecular = c; }

    public Color GetDiffuse() { return mDiffuse; }
    public void SetDiffuse(Color c) { mDiffuse = c; }

    public Color GetAmbient() { return mAmbient; }
    public void SetAmbient(Color c) { mAmbient = c; }

    public Color GetSpecular() { return mSpecular; }
    public void SetSpecular(Color c) { mSpecular = c; }

    public Vector3 GetPosition() { return mPosition; }
    public void SetPosition(Vector3 pos) { mPosition = pos; }
    public Vector3 GetDirection() { return mDirection; }
    public void SetDirection(Vector3 dir) { mDirection = dir; }

    public geombase.Sphere GetBoundingSphere()
    {
        return new geombase.Sphere(mPosition, mRadius);
    }

    //D3DLightData GetD3DLight();

    public void AddRef()
    {
        mRefCount++;
    }
    public int Release()
    {
        Assert.IsTrue(mRefCount > 0);
        mRefCount--;
        if (mRefCount != 0)
            return mRefCount;
        this.Destroy();
        return 0;
    }
    public void Destroy() { }
};

public class LightID
{
    public LightIndex index;
    int frame_counter;
    int refcount;
    public LightID() { refcount = 0; frame_counter = 0; }
    public void AddRef(LightIndex i, int c) { index = i; frame_counter = c; refcount++; }

    public bool UpToDate(int time) { return time == frame_counter && refcount != 0; }
};
