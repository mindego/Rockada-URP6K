using System.Collections.Generic;
using Unity.Hierarchy;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using static StormUnityRenderer;
using static UnityEngine.ParticleSystem;
using DWORD = System.UInt32;

public class ParticleGameObjectContainer
{
    public GameObject myGameObject;
    ParticleSystem m_ParticleSystem;
    Particle[] m_Particles;

    RO myRO;
    PARTICLE_SYSTEM pss;
    PARTICLE_DATA pd;
    public ParticleGameObjectContainer()
    {

    }

    public void Init(RO r)
    {
        myRO = r;
        pss = (PARTICLE_SYSTEM)myRO;
        pd = pss.Data;

        myGameObject = StormUnityRenderer.PS2Unity(myRO);
        m_ParticleSystem = myGameObject.GetComponent<ParticleSystem>();

        m_Particles = new Particle[pd.MaxParts];

        //if (myRO.Parent != null)
        //{
        //    bool hasParentGobj = StormUnityRenderer.SceneFPOContainers.TryGetValue(myRO.Parent.GetHashCode(), out FPO2GameObject parent);
        //    if (hasParentGobj)
        //    {
        //        myGameObject.transform.parent = parent.myGameObject.transform;
        //        myGameObject.transform.localRotation = Quaternion.LookRotation(myRO.Dir, myRO.Up);
        //        myGameObject.transform.localPosition = myRO.Org;
        //    }

        //}
        myGameObject.transform.parent = StormUnityRendererFPOGameObject.GetCurrentTransform();
        myGameObject.transform.localRotation = Quaternion.LookRotation(myRO.Dir, myRO.Up);
        myGameObject.transform.localPosition = myRO.Org;
    }

    public void Move()
    {
        if (myGameObject == null) return;
        if (!FrustrumCulling(myRO.Top().Org)) return;
        //myGameObject.transform.localRotation = Quaternion.LookRotation(myRO.Dir, myRO.Up);
        //myGameObject.transform.position = Engine.ToCameraReference(myRO.Top().Org+myRO.Org);
        //myGameObject.transform.localPosition = myRO.Org;
    }

    private void UpdateParent()
    {
        //GameObject parentGameObject = null;
        //if (myRO.Parent != null)
        //{
        //    bool hasParentGobj = StormUnityRenderer.SceneFPOContainers.TryGetValue(myRO.Parent.GetHashCode(), out FPO2GameObject parent);
        //    if (hasParentGobj)
        //    {
        //        parentGameObject = parent.myGameObject;
        //        myGameObject.transform.parent = parentGameObject.transform;
        //    }

        //}
        //else
        //{
        //    if (myGameObject.transform.parent != null) { myGameObject.transform.parent = null; }
        //}
        //myGameObject.transform.parent = StormUnityRendererFPOGameObject.GetCurrentTransform();
        myGameObject.transform.parent = StormUnityRenderer.GetCurrentTransform();
    }
    public void Update()
    {
        UpdateParent();
        UpdateVisual();
        Move();
    }
    public void UpdateVisual()
    {
        if (myRO == null) return;
        if (m_ParticleSystem == null) return;
        if (pss.Num == 0)
        {
            return;
        }

        //if (!FrustrumCulling()) return;

        A_PARTICLE Pa = pss.Par[pss.Younger];
        int currentIndex = pss.Younger;
        int MaxP = pss.Data.MaxParts - 1;

        //for (int j = pss.Num; j != 0; --j)
        //{
        //    if (Pa.LivedFor == 0)
        //    {
        //        EmitParams emitParams = new EmitParams();
        //        emitParams.position = Pa.Org;
        //        emitParams.velocity = Pa.Speed;
        //        m_ParticleSystem.Emit(emitParams, 1);
        //    }

        //    currentIndex = currentIndex == 0 ? MaxP : currentIndex--;
        //    Pa = pss.Par[currentIndex];
        //}

        int particles_num = m_ParticleSystem.particleCount;
        int part_delta = pss.Num - particles_num;
        int j;
        //m_ParticleSystem.Emit(emitParams, part_delta);
        for (j = 0; j < part_delta; j++)
        {
            EmitParams emitParams = new EmitParams();
            emitParams.position = Vector3.zero;
            emitParams.velocity = Vector3.zero;
            m_ParticleSystem.Emit(emitParams, 1);
        }

        particles_num = m_ParticleSystem.GetParticles(m_Particles);

        //int particles_num = pss.Num;
        //m_Particles = Alloca.ANewN<Particle>(particles_num);

        //Pa = pss.Par[pss.Younger];
        //currentIndex = pss.Younger;
        //for (j = pss.Num; j != 0; --j)
        //{
        //    if (j >= particles_num) continue;
        //    int idx = (int)Mathf.Clamp(Pa.LivedFor, 0, 255);
        //    float size = pd.Size[idx];
        //    if (size > PARTICLE_SYSTEM.maxsize) size = PARTICLE_SYSTEM.maxsize;

        //    Color32 color = IndexToColor(pd.Color[idx]);

        //    Particle UnityParticle = m_Particles[j];
        //    UnityParticle.startSize = size;
        //    UnityParticle.startColor = color;
        //    UnityParticle.startLifetime = 255-idx;

        //    UnityParticle.position = !pss.isLocal() ? myGameObject.transform.rotation * Pa.Org : myGameObject.transform.rotation * (myRO.Org - Pa.Org);
        //    UnityParticle.velocity = Pa.Speed;

        //    m_Particles[j] = UnityParticle;

        //    //currentIndex = currentIndex == 0 ? MaxP : --currentIndex;
        //    //currentIndex = Mathf.Clamp(currentIndex - 1, 0, MaxP);
        //    currentIndex = (int)Mathf.Repeat(currentIndex - 1, MaxP);

        //    //Debug.Log("currentIndex " + currentIndex);
        //    Pa = pss.Par[currentIndex];
        //}

        for (j = 0; j < particles_num; j++)
        {
            //Particle UnityParticle = m_Particles[j];
            if (j >= pss.Num)
            {
                m_Particles[j].startLifetime = -1;
                continue;
            }
            Pa = pss.Par[j];

            if (Pa.LivedFor > 255)
            {
                m_Particles[j].startLifetime = -1;
                continue;
            }
            int idx = (int)Mathf.Clamp(Pa.LivedFor, 0, 255);
            float size = pd.Size[idx];
            if (size > PARTICLE_SYSTEM.maxsize) size = PARTICLE_SYSTEM.maxsize;

            Color32 color = IndexToColor(pd.Color[idx]);
            m_Particles[j].startSize = size;
            m_Particles[j].startColor = color;
            m_Particles[j].startLifetime = 255 - idx;

            //m_Particles[j].position = !pss.isLocal() ? myGameObject.transform.rotation * Pa.Org : myGameObject.transform.rotation * (myRO.Org - Pa.Org);
            //m_Particles[j].position = myGameObject.transform.rotation * Pa.Org;
            //m_Particles[j] = UnityParticle;
                        
            //if (myGameObject.name == "HangarDoorSteam") Debug.LogFormat("HangarDoorSteam Particle[{0}] pos {1} emitter {3} local? {2}", j, Pa.Org, pss.isLocal() ? "yes" : "no",main.simulationSpace);
            //if (myGameObject.name == "HangarDoorSteam") Debug.LogFormat("HangarDoorSteam Particle[{0}] transormed pos {1} emitter {3} local? {2}", j, DebugPos, pss.isLocal() ? "yes" : "no", myRO.Org);
            //if (myGameObject.name == "HumanGPlaneDuza") Debug.LogFormat("HumanGPlaneDuza Particle {0} pos {1} local? {2}", j, Pa.Org, pss.isLocal() ? "yes" : "no");

            //HangarDoorSteam Particle[1] pos(26835.15, 20.92, 22137.74) emitter(-16.39, -0.12, 60.70) local? no
            //HangarDoorSteam Particle[1] transormed pos(-24825.23, 24333.30, -21.05) emitter(-16.39, -0.12, 60.70) local? no

            m_Particles[j].position = !pss.isLocal() ?
                Engine.ToCameraReference(Pa.Org) //If Global coords
                :
                myGameObject.transform.rotation * (myRO.Org + Pa.Org); //If Local coords
        }

        m_ParticleSystem.SetParticles(m_Particles, particles_num);
    }

    private static Dictionary<uint, Color32> partColors = new Dictionary<uint, Color32>();
    private static Color32 IndexToColor(DWORD colorValue)
    {
        if (!partColors.ContainsKey(colorValue))
        {
            byte a = (byte)((colorValue >> 24) & 0xFF);
            byte r = (byte)((colorValue >> 16) & 0xFF);
            byte g = (byte)((colorValue >> 8) & 0xFF);
            byte b = (byte)((colorValue) & 0xFF);
            partColors[colorValue] = new Color32(r, g, b, a);
        }


        return partColors[colorValue];
    }

    public void SetLoop(bool loop)
    {
        MainModule main = m_ParticleSystem.main;
        main.loop = loop;
    }
    public bool SomeAlive()
    {
        return pss.Living();
    }
}
