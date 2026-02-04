using System;
using UnityEngine;
using DWORD = System.UInt32;


public partial class BaseDebris : iBaseActor
{
    BaseActor myBaseActor;

    public void BaseActorInit(BaseScene s)
    {
        myBaseActor = new BaseActor(s);
        myBaseActor.SetOwner(this);
    }

    public void BaseActorDispose()
    {
        myBaseActor.Dispose();
    }

    public BaseScene rScene
    {
        get
        {
            return myBaseActor.rScene;
        }
    }
}

public partial class BaseDebris : iBaseActor
{
    //friend struct BaseCollidingForBaseDebris;
    // от BaseInterface
    public virtual object GetInterface(DWORD id)
    {
        switch (id)
        {
            case BaseActor.ID:
                return (iBaseActor)this;
            case iBaseColliding.ID:
                //return const_cast<BaseDebris*>(this).GetBaseColliding();
                return this.GetBaseColliding();
        }
        return 0;
    }

    // start parameters
    private DEBRIS_DATA DebrisData;
    private DWORD client_handle;
    private bool destroy_on_next_tick;
    // visual
    private SceneVisualizer pVis;
    private bool in_hash;
    private bool m_IsHidden;
    private float m_TimeToAppear;
    private HMember body_object;
    private bool body_hided;
    private FPO body;
    private void HideMainObject()
    {
        if (body_object != null)
        {
            rScene.DeleteHM(body_object);
            body_object = null;
        }
        body_hided = true;
    }

    iBaseColliding GetBaseColliding()
    {
        if (m_pBaseColliding == null && DebrisData != null && DebrisData.CollisionMethod != 0)
            m_pBaseColliding = new BaseCollidingForBaseDebris(this, body);
        return m_pBaseColliding;
    }
    BaseCollidingForBaseDebris m_pBaseColliding;

    // state flags
    int DetailStage; //было bool
    bool water_intersected;
    bool ground_lie;
    bool image_changed;
    bool check_flag;

    // timers
    float disappear_timer;
    void ProcessTimers(float scale)
    {
        disappear_timer -= scale;
        if (disappear_timer < 0)
        {       // пришло время исчезать
            bool dis_flag = true;
            if (DetailStage == BaseScene.DETAIL_STAGE_NONE)
            {
                ProcessEvent(DebrisEvent.DEBRIS_OWNER_DELETE, null);
                return;
            }
            if (DebrisData.VisibleDisappearFlag == 0 && body.GetFlag(RoFlags.ROF_DRAWED) != 0) dis_flag = false;  // если мы не рисовались
            if (ground_lie)
            {                                           // если лежим на земле
                if (dis_flag)
                {                                           // если можно исчезать
                    body.Org.y -= DebrisData.DisappearSpeed * scale;        // уползаем
                    float h = rScene.GroundLevel(body.Org.x, body.Org.z);
                    if (body.Org.y + body.HashRadius * 0.5 < h)                  // совсем уползли
                        ProcessEvent(DebrisEvent.DEBRIS_OWNER_DELETE, null);

                    if (!FPO2GameObject.IsValid(body.Org)) Debug.Log("Not valid BaseDebris ORG " + GetHashCode().ToString("X8"));
                }
            }
            else
            {                                                   // если летим
                if (dis_flag)                                            // и можно исчезать
                    ProcessEvent(DebrisEvent.DEBRIS_OWNER_DELETE, null);
            }
        }
    }

    void ProcessEvent(DebrisEvent ev_type, TraceResult tr)
    {
        Debug.LogFormat("Debris ev_type: {0} TraceResult {1}",ev_type, tr);
        // process life event
        switch (ev_type)
        {
            case DebrisEvent.DEBRIS_OWNER_DELETE:
                MakeDamage(null);
                destroy_on_next_tick = true;
                break;
            case DebrisEvent.DEBRIS_OWNER_COLLIDE:
                if (tr != null)
                {
                    if (tr.coll_object == null)
                    {
                        if (tr.ground_type == TerrainDefs.GT_WATER && !water_intersected)
                        {
                            speed *= 0.5f;
                            Debug.LogFormat("Speed of debris {0} - {1}",GetHashCode().ToString("X8"),speed);
                            mRotateOffset = Vector3.zero;
                            acceleration *= DebrisData.WaterGravity;
                            water_intersected = true;
                        }
                    }
                    MakeDamage(tr);
                    //Debug.LogFormat("MakeDamage @ ",tr);
                    if (visual != null) visual.OnCollide(tr, local_y);
                }
                break;
        }
    }
    void SetupVariables()
    {
        m_TimeToAppear = Mathf.Abs(RandomGenerator.Rand01() * (DebrisData.MaxAppearTimer - DebrisData.MinAppearTimer)) + DebrisData.MinAppearTimer;

        // body parameters
        body_hided = false;

        if (pVis != null)
            visual = pVis.CreateVisualDebris(DebrisData, this, in_hash);

        // state flags
        ground_lie = false;
        image_changed = false;
        water_intersected = false;

        if (DebrisData.DisappearType == DEBRIS_DATA.DISAPPEAR_AFTER_TOUCH)
            disappear_timer = 30.0f;
        else
            disappear_timer = Mathf.Abs(RandomGenerator.Rand01() * (DebrisData.MaxDisappearTimer - DebrisData.MinDisappearTimer)) + DebrisData.MinDisappearTimer;

        // moving now
        acceleration = new Vector3(0, -STORM_DATA.GAcceleration, 0);
        // collide flag 
        if (DebrisData.CollisionMethod == 0) body.ClearFlag(HashFlags.OF_GROUP_COLLIDABLE);

        if (DebrisData.RotateOffset < 0.01)
            mRotateOffset = Vector3.zero;
        else
            mRotateOffset = Distr.Sphere() * DebrisData.RotateOffset;

        goukecoeff = RandomGenerator.Rand01() * (DebrisData.GoukeMax - DebrisData.GoukeMin) + DebrisData.GoukeMin;
        local_height = 0;
        rotate_accel_flag = false;
        rotate_speed = 0;
        norma2_speed = 0.0f;
        if (DebrisData.RotateType != DEBRIS_DATA.DEBRIS_ROTATE_NOTHING)
        {
            if (DebrisData.RandomRotateAxis != 0)
                rotate_axis = Distr.Sphere();
            else
                rotate_axis = DebrisData.RotateAxis;
            switch (DebrisData.RotateType)
            {
                case DEBRIS_DATA.DEBRIS_ROTATE_CONTINUES:
                    rotate_accel_flag = true;
                    target_rotate_speed = Mathf.Abs(RandomGenerator.Rand01() * (DebrisData.RotateMaxSpeed - DebrisData.RotateMinSpeed) + DebrisData.RotateMinSpeed);
                    break;
                case DEBRIS_DATA.DEBRIS_ROTATE_IMMEDIATELY:
                    rotate_speed = Mathf.Abs(RandomGenerator.Rand01() * (DebrisData.RotateMaxSpeed - DebrisData.RotateMinSpeed) + DebrisData.RotateMinSpeed);
                    break;
            }
        }
        else
        {
            rotate_axis = Vector3.forward;
            rotate_speed = 0;
        }
        if (mass > 0.01 && DebrisData.RotateType == DEBRIS_DATA.DEBRIS_ROTATE_CONTINUES)
        {
            target_rotate_speed *= 250.0f / mass;
            //rScene.Message("%p Massa %f",this,mass);
            if (rotate_speed > target_rotate_speed) rotate_speed = target_rotate_speed;
            else
              if (rotate_speed < -target_rotate_speed) rotate_speed = -target_rotate_speed;
            //rScene.Message("setup %p %f %f %f %f %f",this,rotate_speed,rotate_axis.x,rotate_axis.y,rotate_axis.z,rotate_axis.Norma());
        }
        // jump on create
        GetMinY();
        float alt = rScene.SurfaceLevel(body.Org.x, body.Org.z);
        if (DebrisData.JumpOnCreate != 0 && body.Org.y + local_y - alt <= 0.0f)
            body.Org.y = alt - local_y;
        if (!FPO2GameObject.IsValid(body.Org)) Debug.LogError("Not valid BaseDebris ORG! " + GetHashCode().ToString("X8"));
        //rScene.Message("setupb %p %f %f %f %f %f",this,rotate_speed,rotate_axis.x,rotate_axis.y,rotate_axis.z,rotate_axis.Norma());
    }
    void Appear()
    {
        Asserts.AssertBp(IsHidden());

        if (in_hash)
            body_object = rScene.CreateHM(body);

        if (visual != null)
            visual.Appear();

        m_IsHidden = false;
    }
    bool IsHidden() { return m_IsHidden; }

    // physics
    public Vector3 speed, acceleration;
    float norma2_speed;
    float mass, local_height, local_y;
    float goukecoeff;
    bool rotate_accel_flag;
    //Vector3 rotate_axis;

    Vector3 rotate_axis_debug;
    Vector3 rotate_axis
    {
        get { return rotate_axis_debug; }
        set
        {
            Debug.LogFormat("rotate_axis changed from {0}->{1}", rotate_axis_debug,value);
            rotate_axis_debug = value;
        }
    }
    float rotate_speed, target_rotate_speed;
    Vector3 mRotateOffset;
    void GetMinY()
    {
        local_y = CheckMinY(1E6f, body, body.MinX(), body.MinY(), body.MinZ());
        local_y = CheckMinY(local_y, body, body.MaxX(), body.MinY(), body.MinZ());
        local_y = CheckMinY(local_y, body, body.MinX(), body.MaxY(), body.MinZ());
        local_y = CheckMinY(local_y, body, body.MaxX(), body.MaxY(), body.MinZ());
        local_y = CheckMinY(local_y, body, body.MinX(), body.MinY(), body.MaxZ());
        local_y = CheckMinY(local_y, body, body.MaxX(), body.MinY(), body.MaxZ());
        local_y = CheckMinY(local_y, body, body.MinX(), body.MaxY(), body.MaxZ());
        local_y = CheckMinY(local_y, body, body.MaxX(), body.MaxY(), body.MaxZ());
    }

    float CheckMinY(float cur_min, FPO fpo, float x, float y, float z)
    {
        float miny = x * fpo.Right.y + y * fpo.Up.y + z * fpo.Dir.y;
        return (cur_min <= miny ? cur_min : miny);
    }

    void ProcessRotating(float scale)
    {
        if (rotate_accel_flag)
        {
            if (target_rotate_speed - rotate_speed > 0)
            {
                rotate_speed += DebrisData.RotateAccel * scale;
                if (rotate_speed > target_rotate_speed)
                {
                    rotate_speed = target_rotate_speed;
                    rotate_accel_flag = false;
                }
            }
            else
            {
                rotate_speed *= 0.2f * scale;
                if (rotate_speed <= 0)
                {
                    rotate_speed = 0;
                    rotate_accel_flag = false;
                }
            }
        }
        // physics
        //rScene.Message("rotating %p %f %f %f %f %f",this,rotate_speed,rotate_axis.x,rotate_axis.y,rotate_axis.z,rotate_axis.Norma());
        if (!FPO2GameObject.IsValid(body.Org)) Debug.Log("Incorrect ORG for baseDebris before rot:" + GetHashCode().ToString("X8"));
        float dx=0, dy=0, dz=0;
        Vector3 orgDebug = body.Org;
        if (rotate_speed > 0)
        {
            //float dx = (body.MinX() + body.MaxX()) * .5f + mRotateOffset.x;
            //float dy = (body.MinY() + body.MaxY()) * .5f + mRotateOffset.y;
            //float dz = (body.MinZ() + body.MaxZ()) * .5f + mRotateOffset.z;
            dx = (body.MinX() + body.MaxX()) * .5f + mRotateOffset.x;
            dy = (body.MinY() + body.MaxY()) * .5f + mRotateOffset.y;
            dz = (body.MinZ() + body.MaxZ()) * .5f + mRotateOffset.z;
            body.Org += body.Right * dx + body.Up * dy + body.Dir * dz;
            body.Rotate(rotate_axis, rotate_speed * scale);
            body.Org -= body.Right * dx + body.Up * dy + body.Dir * dz;
        }
        if (!FPO2GameObject.IsValid(body.Org))
        {
            Debug.LogFormat("Incorrect ORG for baseDebris after rot: {0}\n" +
                "OffsetVector: {1}\n" +
                "Prev val: {2}\n" +
                "rotate_axis: {3}\n" +
                "rotate_speed:{4}\n" +
                "DebrisData.RotateAccel: {5}",
                GetHashCode().ToString("X8"),
                new Vector3(dx, dy, dz),
                orgDebug,
                rotate_axis,
                rotate_speed,
                DebrisData.RotateAccel
            );
        }
    }
    void CheckSpeed()
    {
        norma2_speed = speed.sqrMagnitude;
        if (norma2_speed < 0.2f && local_height < 0.2f)
        {
            Stop();
        }
        if (norma2_speed < 130.0f && local_height < 3.0f && DebrisData.RotateSlowing != 0)
        {
            target_rotate_speed = 0;
            rotate_accel_flag = true;
        }
    }
    void Stop()
    {
        norma2_speed = 0;
        speed.Set(0, 0, 0);
        ground_lie = true;
        target_rotate_speed = 0;
        rotate_speed = 0;
    }
    void RotateRough(float h)
    {
        //TODO при body.HashRadius=0 rotate_axis приобретает феерический размах, что ведёт к некорректному пересчёту координат (+/-infinity, +/-infinity,+/-infinity) Надо выяснить почему и исправить грубый поворот обломка
        Vector3 axis_backup = rotate_axis;
        float speed_backup = rotate_speed;
        Vector3 Diff, Spd;
        Diff = new Vector3(0, body.HashRadius, 0);
        Spd = -speed * DebrisData.FrictionCoeff;
        Debug.LogFormat("0. Diff: {0} Spd: {1} speed {2} for {3}",Diff,Spd,speed,body.TextName + "#" + body.GetHashCode().ToString("X8"));
        Debug.LogFormat(".5 FPO: {0} HR: {1} SR: {2} MR: {3}", body.TextName + "#" + body.GetHashCode().ToString("X8"),body.HashRadius,body.SelfRadius, body.MaxRadius);
        Diff = Vector3.Cross(Diff, Spd);
        rotate_axis = Diff + rotate_axis * rotate_speed;
        Debug.LogFormat("1. rotate axis: {0}->{1} rotate_speed {2}->{3} {4}", ( axis_backup.x,axis_backup.y,axis_backup.z), rotate_axis, speed_backup, rotate_speed,Diff);
        rotate_speed = rotate_axis.magnitude;
        if (rotate_speed != 0)
            rotate_axis *= 1.0f / rotate_speed;
        Debug.LogFormat("2. rotate axis: {0}->{1} rotate_speed {2}->{3}", (axis_backup.x, axis_backup.y, axis_backup.z), rotate_axis, speed_backup, rotate_speed);
        if (Mathf.Abs(RandomGenerator.Rand01()) < 0.25) rotate_speed *= 0.25f;
        if (rotate_speed > DebrisData.RotateMaxSpeed) rotate_speed = DebrisData.RotateMaxSpeed;
        else if (rotate_speed < -DebrisData.RotateMaxSpeed) rotate_speed = -DebrisData.RotateMaxSpeed;
        //rScene.Message("Speed %f Axis %f %f %f",rotate_speed,rotate_axis.x,rotate_axis.y,rotate_axis.z);
        Debug.LogFormat("Final rotate axis: {0}->{1} rotate_speed {2}->{3} speed {4} FC {5} RMS {6} hr {7}", (axis_backup.x, axis_backup.y, axis_backup.z), rotate_axis, speed_backup, rotate_speed,(speed.x,speed.y,speed.z), DebrisData.FrictionCoeff,DebrisData.RotateMaxSpeed, body.HashRadius);
        
    }

    void MakeDamage(TraceResult tr)
    {
        float XRadius = DebrisData.XRadius, XDamage = DebrisData.XDamage;
        EXPLOSION_INFO expl;
        if (tr != null)
        {
            if (tr.coll_object != null)
                expl = DebrisData.ExplOnTarget;
            else
                expl = DebrisData.ExplOnGround[tr.ground_type];
        }
        else
            expl = DebrisData.ExplOnEnd;
        if (expl != null)
        {
            XRadius = expl.XRadius;
            XDamage = expl.XDamage;
            Debug.LogFormat("Making debris damage {0}x{1} using explosion {2} {3}",XDamage,XRadius, expl.explosion,expl);
            if (XRadius > EXPLOSION_INFO.EXPLOSION_CUT_RADIUS)
                rScene.MakeAreaDamage(0, iBaseVictim.WeaponCodeCollisionObject, body.Org, XRadius, XDamage);
        }
    }

    IVisualDebris visual;
    bool GetVisualizeFlag(MATRIX m)
    {
        // создавать ли визуальную часть
        pVis = rScene.GetSceneVisualizer();
        if (pVis != null && DebrisData != null)
        {
            if (pVis.SkipDebris(DebrisData.AppearProbability))
                return true;
            DetailStage = pVis.GetDetailStage(m.Org);
            //if (DetailStage >= DETAIL_STAGE_QUARTER) return true;
            // создавать 
            return false;
        }
        else if (pVis != null && DebrisData != null && DebrisData.CollisionMethod != 0)
        {
            // создавать 
            return false;
        }
        Debug.Log("Ifs skipped");
        return true;
    }
    public BaseDebris(BaseScene s, FPO f, DEBRIS_DATA _data, DWORD _client_handle, float _mass)
    {
        BaseActorInit(s);
        client_handle = _client_handle;
        DebrisData = _data;
        body_object = null;
        visual = null;
        m_IsHidden = true;
        m_pBaseColliding = null;
        body = f;
        destroy_on_next_tick = GetVisualizeFlag(f);
        rScene.Message("Creating big debris {0} {1}",this,f.HashRadius);

        if (!destroy_on_next_tick)
        {
            mass = _mass;
            in_hash = true;
            SetupVariables();
            if (body != null)
                body.Link = this;
        }

    }
    public BaseDebris(BaseScene s, MATRIX m, DEBRIS_DATA _data, DWORD _client_handle)
    {
        client_handle = _client_handle;
        DebrisData = _data;
        body_object = null;
        visual = null;
        m_IsHidden = true;
        m_pBaseColliding = null;
        body = null;
        destroy_on_next_tick = GetVisualizeFlag(m);
        Asserts.AssertBp(DebrisData);
        Asserts.AssertBp(DebrisData.CollisionMethod == 0);

        if (!destroy_on_next_tick)
        {
            mass = DebrisData.Massa;
            in_hash = false;
            body = rScene.CreateFPO(DebrisData.FileName);
            body.Set(m);
            Asserts.AssertBp(body != null);
            SetupVariables();
            if (body != null)
                body.Link = this;
            //rScene.Message("setup %p %f %f %f %f %f",this,rotate_speed,rotate_axis.x,rotate_axis.y,rotate_axis.z,rotate_axis.Norma());
        }
    }

    public override string ToString()
    {
        return "BaseDebris:" +
            "\nclearing: " + destroy_on_next_tick +
            "\nbody: " + (body == null ? "Null" : body) +
            "\nbody_object: " + (body_object == null ? "Null" : body_object) +
            "\nm_IsHidden: " + m_IsHidden +
            "\nDebrisData names: " + (DebrisData == null? "FAILED!":DebrisData.FullName)
            ;
    }

    private bool IsDisposed = false;
    ~BaseDebris()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true;

        if (m_pBaseColliding != null)
            m_pBaseColliding.Dispose();
        //rScene.Message("Deleting debris %p",this);
        if (visual != null) visual.OnOwnerDelete();  // notify visual
        HideMainObject();
        if (body != null) body.Release();

        BaseActorDispose();
    }
    public virtual bool Move(float scale)
    {
        //rScene.Message("Moving debris checks");
        //rScene.Message("destroy_on_next_tick? " + destroy_on_next_tick.ToString());
        if (destroy_on_next_tick) return false;

        //rScene.Message("IsHidden? " + IsHidden());
        if (IsHidden())
        {
            m_TimeToAppear -= scale;
            if (m_TimeToAppear > 0)
                return true;
            Appear();
            scale = -m_TimeToAppear;//we'll update on the rest of time 
            if (visual == null)
                return false;
        }
        //rScene.Message("Moving debris {0}",this);
        //if (body) rScene.Message("debris %p org(%f %f %f) dir(%f %f %f) up(%f %f %f) right(%f %f %f)",this,body.Org.x,body.Org.y,body.Org.z,body.Dir.x,body.Dir.y,body.Dir.z,body.Up.x,body.Up.y,body.Up.z,body.Right.x,body.Right.y,body.Right.z);
        if (body == null) return false;
        //rScene.Message("debris %p speed(%f %f %f)",this,speed.x,speed.y,speed.z);

        ProcessTimers(scale);
        if (DetailStage <= BaseScene.DETAIL_STAGE_HALF && !body_hided)
        {
            //rScene.Message("Rotating BaseDebris {0} axis {1} speed {2}/3", GetHashCode().ToString("X8"), rotate_axis, rotate_speed, target_rotate_speed);
            ProcessRotating(scale);
            GetMinY();
        }
        body.ClearFlag(RoFlags.ROF_DRAWED);
        if (body_hided || ground_lie) return true;
        if (DebrisData.AlwaysLie != 0) return true;
        // move
        //rScene.Message("Moving BaseDebris {0} {1} + {2}", GetHashCode().ToString("X8"),body.Org, speed * scale);
        check_flag = false;
        speed += acceleration * scale;
        body.Org += speed * scale;
        // process air friction like particles
        if (DebrisData.AirFrictionAffected != 0)
        {
            float d = (1.0f - Mathf.Sqrt(norma2_speed) * DebrisData.AirFriction * scale);
            if (d < 0) d = 0;
            float ly = speed.y;
            speed *= d;
            speed.y = ly;
            check_flag = true;
        }
        // rotate and move near camera
        if (DetailStage <= BaseScene.DETAIL_STAGE_HALF)
        {
            TraceResult tr = water_intersected ? rScene.GroundLevelTr(body.Org.x, body.Org.z) : rScene.SurfaceLevelTr(body.Org.x, body.Org.z);
            local_height = body.Org.y + local_y - tr.dist;  // count height//min.y
            if (local_height <= 0.0f)
            {                   // if near ground
                if (!image_changed)
                {
                    image_changed = true;
                    body.SetImage(2, (int)RoFlags.FSI_ROUND_UP, 0, true);
                }
                //body.Org.y = tr.dist-local_y;
                if (!water_intersected && tr.ground_type == TerrainDefs.GT_WATER)
                {
                    ProcessEvent(DebrisEvent.DEBRIS_OWNER_COLLIDE, tr);
                    return true;
                }
                if (DebrisData.DisappearType == DEBRIS_DATA.DISAPPEAR_AFTER_TOUCH)
                {
                    ProcessEvent(DebrisEvent.DEBRIS_OWNER_COLLIDE, tr);
                    ProcessEvent(DebrisEvent.DEBRIS_OWNER_DELETE, null);
                    return true;
                }
                if (water_intersected)
                {
                    Stop();
                    return true;
                }
                Vector3 local_normal = tr.Normal(true);
                float y;
                y = Vector3.Dot(local_normal, speed); // count cos between GroundNormal and speed
                if (y < 0)
                {  // if > 90 collision detected
                    /*if (DebrisData.CheckCollisionType == DEBRIS_COLLTYPE_PRECISE) 
                      RotatePrecise (tr.dist);
                    else */
                    RotateRough(tr.dist);
                    if (rotate_accel_flag) rotate_accel_flag = false;
                    speed -= local_normal * (2.0f * y);
                    speed *= goukecoeff;
                    check_flag = true;
                    ProcessEvent(DebrisEvent.DEBRIS_OWNER_COLLIDE, tr);
                }
            }
            /*else {       // here friction using
              if (y < 0.3 && y > -0.3) {
                speed *= DebrisData . FrictionCoeff;
                check_flag = 1;
              }
            } */
            if (check_flag) CheckSpeed();
            if (body_object != null)
                rScene.UpdateHM(body_object);
        }
        return true;
    }

    public virtual void Update(float scale) { }


    public float GetNorma2() { return norma2_speed; }
    public int GetDetailStage() { return DetailStage; } //было bool
    public Vector3 GetBodySpeed() { return speed; }
    public FPO GetBody() { return body; }
    public IVisualDebris GetVisual() { return visual; }
    public bool GetGroundLie() { return ground_lie; }

    public void SetCornerSpeed(Vector3 r, float _speed)
    {
        Vector3 temp = rotate_axis * rotate_speed + r * _speed;
        rotate_speed = temp.magnitude;
        if (rotate_speed < 0.001)
            rotate_axis = Vector3.forward;
        else
            rotate_axis = temp / rotate_speed;
    }
    public void AddAcceleration(Vector3 v) { acceleration += v; }
    public void AddSpeed(Vector3 Spd) { speed += Spd; norma2_speed = speed.sqrMagnitude; }
    public MATRIX GetPos() { return body; }
    public void SetPos(MATRIX m) { body.Set(m); }
    public void SetSpeed(Vector3 _speed) { speed = _speed; }
};

enum DebrisEvent : uint
{
    DEBRIS_OWNER_DELETE,
    DEBRIS_INTERSECT_WATER,
    DEBRIS_OWNER_COLLIDE,
    ForceDebrisDword = 0xFFFFFFFF
};

public class BaseCollidingForBaseDebris : BaseCollidingForFPO
{
    public BaseCollidingForBaseDebris(BaseDebris debris, FPO fpo) : base(fpo)
    {

        m_pBaseDebris = debris;
    }

    public override Vector3 GetSpeedFor(Vector3 v)
    {
        return m_pBaseDebris.speed;
    }

    /// <summary>
    /// BaseCollidingForRBuilding - от iBaseInterface
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override object GetInterface(DWORD id)
    {
        return m_pBaseDebris.GetInterface(id);
    }

    internal void Dispose()
    {
        Debug.Log("Disposing of BaseCollidingForBaseDebris " + this);
    }

    private BaseDebris m_pBaseDebris;
};