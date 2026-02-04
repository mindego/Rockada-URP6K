using System;
using UnityEngine;
/// <summary>
/// in-game interface for SFGss
/// </summary>
class BaseSfg : iSfg
{

    // от iSf
    public virtual Vector3 GetCenter()
    {
        return mSfCenter;
    }
    public virtual float GetRadius()
    {
        return mySfgData.myRadius;
    }
    public virtual bool IsOn()
    {
        return mIsOn;
    }
    public virtual bool IsInRange(Vector3 o)
    {
        float d2 = (o - mSfCenter).sqrMagnitude;
        return (d2 < myRadius2);
    }

    // от iSfg
    public virtual void TurnOn()
    {
        if (!mIsOn)
        {
            mIsOn = true;
            show(true);
            mrScene.registerSfg(getISf());
            myImageChanged = myParent.GetFpo().SetMainImage(3, 0, 0) == 3;
        }
    }
    public virtual void TurnOff()
    {
        if (mIsOn)
        {
            mrScene.unregisterSfg(getISf());
            show(false);
            mIsOn = false;
            if (myImageChanged)
            {
                myParent.ResetImage();
                myImageChanged = false;
            }
        }
    }
    public virtual void show(bool showOn)
    {
        Asserts.Assert(mIsOn);
        if (mrScene.GetSceneVisualizer() != null)
        {
            if (showOn)
            {
                mrScene.GetSceneVisualizer().AddNonHashObject(mpSfObject);
            }
            else
            {
                mrScene.GetSceneVisualizer().SubNonHashObject(mpSfObject);

            }
        }
    }
    // own
    private SUB_SFG_DATA mySfgData;
    BaseScene mrScene;
    bool mIsOn;
    Fpo mpSfObject;
    float myRadius2;
    BaseSubobj myParent;
    bool myImageChanged = false;

    iSfg getISf() { return this; }
    protected Vector3 mSfCenter;
    public bool isImageChanged() { return myImageChanged; }
    public BaseSfg(BaseScene s, SUB_SFG_DATA sd, BaseSubobj parent)
    {
        mrScene = s;
        mIsOn = false;
        mpSfObject = null;
        mySfgData = sd;
        myParent = parent;
        myRadius2 = Mathf.Pow(mySfgData.myRadius, 2);
    }
    ~BaseSfg()
    {
        TurnOff();
        //  SafeRelease(mpSfObject);
    }
    public bool IsCameraInRange(Vector3 o)
    {
        return IsInRange(o);
    }

    public void prepare()
    {
        MATRIX tmp = new MATRIX(Vector3.zero, Vector3.forward,-Vector3.up,Vector3.right);
        mpSfObject = mrScene.CreateFPO2(mySfgData.myName);
        if (mpSfObject == null)
            throw new Exception(string.Format("SFG \"{0}\": cannot create SF object!", mySfgData.FullName));
        //mpSfObject.tm = Storm.Math.FromLocus(tmp); //MathConvert? //TODO - сделать правильные координаты ГПП
        
        TurnOn();
    }
    public void update(float scale, Vector3 pos)
    {
        mSfCenter = pos;
        //if (mpSfObject!=null)
        //    mpSfObject.tm.pos = pos;
        //TODO - сделать правильные координаты ГПП
    }
};