using UnityEngine;
using DWORD = System.UInt32;
/// <summary>
/// ***************************** базовый класс для вызрывов**********************************
/// </summary>
class Explosion : VisualBaseActor
{
    EXPLOSION_DATA data;
    EXPLOSION_DATA next_expl;
    PARTICLE_SYSTEM myobject;
    ILight light;
    //TRef<ILenzFlare2> m_Flare;
    HMember hlight;
    HMember m_hFlare;
    float timer;
    I3DSoundEvent sound;
    //AudioClip sound;
    Vector3 speed;
    float speedf;
    IDecal decal;
    DWORD mId;

    //void UpdateOwner() { owner = rScene.GetUnit(owner_handle); }
    public void CreateCommonEffects()
    {
        //TODO Реализовать отрисовку взрыва
        return;
        //        // light
        //        if (par_object == null) return;
        //        if (data->LightData1.mRadius > EXPL_LIGHT_CUT_RADIUS)
        //        {
        //            light = pVis->CreateLight(data->LightData1);
        //            if (light)
        //            {
        //                light->SetPosition(object->Org);
        //                light->SetRadius(data->LightData1.mRadius);
        //                hlight = pVis->CreateHM(light->GetHashObject());
        //                // Flare
        //                m_Flare = (pVis->GetSceneConfig()->v_flares && Rand01() < data->myFlareProbability) ? pVis->createFlare(data->Flare) : 0;
        //                if (m_Flare)
        //                {
        //                    m_Flare->SetPosition(object->Org);
        //                    if (data->myHashed)
        //                        m_hFlare = pVis->CreateHM(m_Flare->GetHashObject());
        //                    else
        //                        pVis->registerObject(m_Flare->GetHashObject());
        //                    m_Flare->SetColor(data->LightData1.mColor);
        //                    m_Flare->activate(true);
        //                }
        //            }
        //        }
        //        //sound
        //#pragma message("  MIKHA: здесь привязка взрыва к объекту!")
        //        I3DSoundEventController* ctr =
        //          CreateSoundCtrWrapper(&object->Org, 0, mId);

        //        sound = pVis->Get3DSound()->LoadEvent(
        //          "Explosion", data->FullName, "Create", data->LoopedSound, true, ctr);

        //        ctr->Release();

        //        if (sound) sound->Start();
    }
    public void DeleteCommonEffects()
    {
        //TODO Реализовать отрисовку взрыва
        //DeleteLight();
        //if (m_hFlare)
        //{
        //    AssertBp(data->myHashed);
        //    pVis->DeleteHM(m_hFlare);
        //    m_hFlare = 0;
        //}
        //else if (m_Flare)
        //    pVis->unregisterObject(m_Flare->GetHashObject());


        //m_Flare = 0;
        //SafeRelease(sound); sound = 0;
    }
    public void DeleteLight()
    {
        //TODO Реализовать отрисовку взрыва
        //if (hlight != null)
        //{
        //    pVis.DeleteHM(hlight);
        //    hlight = null;
        //    SafeRelease(light);
        //    light = null;
        //}
        //if (m_Flare)
        //    m_Flare->SetIntensity(0);
    }
    // функции
    public Explosion(SceneVisualizer _scene, EXPLOSION_DATA _pd, Vector3 Org, Vector3 Dir, Vector3 Speed, DWORD id) : base(_scene)
    {
        Debug.Log("Me new explosion " + GetHashCode().ToString("X8") + " @ " + Org + " " + _pd);
        hlight = null;
        light = null;
        m_hFlare = null;
        sound = null;
        decal = null;
        mId = id;

        data = _pd;

        timer = data.LifeTime;
        speed = Speed;
        speedf = speed.magnitude;
        if (speedf != 0)
            speed /= speedf;

        //TODO Реализовать отрисовку взрыва
        //par_object = pVis.CreateParticle(data.Particle, 0);
        //if (par_object != null)
        //{

        //    pVis.AddNonHashObject(par_object);
        //    // ориентация объекта
        //    par_object.SetHorizontal(data.Vertical ? Vector3.up : Dir);
        //    // положение объекта
        //    Vector3 dt = Vector3.zero;
        //    if (data.Delta.x) dt += par_object->Right * data.Delta.x;
        //    if (data.Delta.y) dt += par_object->Up * data.Delta.y;
        //    if (data.Delta.z) dt += par_object->Dir * data.Delta.z;
        //    par_object.Org = Org + dt;

        //    // мелкий мусор
        //    if (RandomGenerator.Rand01() < data.Probability)
        //    {
        //        DEBRIS_SET ds = GetRandomListElem(data.DebrisSetsList);
        //        if (ds != 0)
        //            ds.Create(pVis, par_object);
        //    }
        //    // декаль
        //    if (data.DecalData1.draw_script != 0xFFFFFFFF)
        //    {
        //        pVis.CreateVisualDecal(data.DecalData1, par_object);
        //    }

        //}

        //TODO Удалить отрисовку PlaceHolder'а
        myobject = pVis.CreateParticle(data.Particle, 0);
        if (myobject != null)
        {

            pVis.AddNonHashObject(myobject);
            // ориентация объекта
            myobject.SetHorizontal(data.Vertical ? Vector3.up : Dir);
            // положение объекта
            Vector3 dt = Vector3.zero;
            if (data.Delta.x != 0) dt += myobject.Right * data.Delta.x;
            if (data.Delta.y != 0) dt += myobject.Up * data.Delta.y;
            if (data.Delta.z != 0) dt += myobject.Dir * data.Delta.z;
            myobject.Org = Org + dt;

            //Debug.Log("KABOOM! @" + Org);
            //// мелкий мусор
            //if (RandomGenerator.Rand01() < data.Probability)
            //{
            //    DEBRIS_SET ds = GetRandomListElem(data.DebrisSetsList);
            //    if (ds != 0)
            //        ds.Create(pVis, myobject);
            //}
            //// декаль
            //if (data.DecalData1.draw_script != 0xFFFFFFFF)
            //{
            //    pVis.CreateVisualDecal(data.DecalData1, myobject);
            //}

        }

        //next_expl = data.ExplChain[0];
        next_expl = data.ExplChain.Head();
        CreateCommonEffects();
    }
    ~Explosion()
    {
        Dispose();
        //TODO Реализовать завершение отрисовки взрыва
        //DeleteCommonEffects();
        //if (par_object!=null)
        //{
        //    pVis.SubNonHashObject(par_object);
        //    par_object->Release();
        //}
    }
    private bool isDisposed = false;
    public override void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;
        Debug.Log("Disposing of explosion " + GetHashCode().ToString("X8") + "(explosion destructor)");
        DeleteCommonEffects();

        if (myobject != null)
        {
            pVis.SubNonHashObject(myobject);
            myobject.Release();
        }
        base.Dispose();
    }
    public override bool Update(float scale)
    {
        if (myobject == null) return false;
        //EngineDebug.DebugConsoleFormat("Processing {0} {1} living {2} timer {3} scale {4}",
        //    myobject.GetType().ToString(), myobject.GetHashCode().ToString("X8"),
        //    myobject.Living().ToString(),
        //    timer,
        //    scale

        //    );
        timer -= scale;
        myobject.Org += speed * speedf * scale;
        speedf -= speedf * speedf * data.SpeedCoeff * scale;
        if (speedf < 0) speedf = 0f;
        Vector3 local_speed = speed * speedf;

        // explosion chain
        while (next_expl != null && next_expl.Timer < data.LifeTime - timer)
        {
            pVis.CreateVisualExplosion(next_expl, myobject.Org, myobject.Dir, local_speed, mId);
            next_expl = data.ExplChain.Next(next_expl);
        }
        bool alive = false;

        // update light
        //if (hlight && data->LightD > EXPL_LIGHTD_CUT_RADIUS)
        //{
        //    float ltime = data->LifeTime - timer;
        //    float ld = data->LightD * ltime / data->LightData1.mRadius;

        //    if (ld > 1)
        //        DeleteLight();
        //    else
        //    {
        //        float intence = 1 - ld;
        //        light->SetIntensity(intence);
        //        light->SetRadius(data->LightData1.mRadius * intence);
        //        pVis->UpdateHM(hlight);
        //        alive = true;

        //        if (m_Flare) m_Flare->SetIntensity(intence);
        //    }
        //}

        if (myobject.Living())
        {
            myobject.Update(scale, local_speed);
            if (timer < 0)
                myobject.Die();
            alive = true;
        }

        if (sound != null && sound.IsPlaying())
            alive = true;

        return alive;
    }

}
