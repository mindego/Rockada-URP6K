using UnityEngine;
using UnityEngine.Assertions;
/// <summary>
/// ProjectileVisualMeshAndParticle - болванка (FPO) и дымный след
/// </summary>
class ProjectileVisualMeshAndParticle : ProjectileVisual
{
    #region от VisualBaseActor
    public override bool Update(float scale)
    {
        base.Update(scale);
        if (particle == null) return false;
        if (owner != null)
        {
            //*((MATRIX*)mesh) = owner.GetMatrix();
            particle.Update(scale, owner.GetSpeed());
            if (light != null)
            {
                light.SetPosition(owner.GetMatrix().Org);
                pVis.UpdateHM(hlight);
            }
            return true;
        }
        else
        {
            particle.Org += last_speed * scale;
            particle.Update(scale, last_speed);
            return particle.Living();
        }

    }
    #endregion
    // для Projectile
    public override void OnOwnerDelete()
    {
        //mesh = owner.GetMatrix();
        last_speed = owner.GetSpeed() * .33f;
        if (particle!=null)
        {
            particle.Die();
            particle.ToWorld();
            particle.Detach();
        }
        pVis.SubNonHashObject(mesh);
        mesh.Release();
        mesh = null;
        base.OnOwnerDelete();
    }

    // создание/удаление
    private Vector3 last_speed;
    FPO mesh;
    PARTICLE_SYSTEM particle;
    public ProjectileVisualMeshAndParticle(SceneVisualizer scene, Projectile owner, WPN_DATA wd): base(scene,owner,wd) {
        particle = null;
        mesh = null;
    
        // mesh
        mesh = pVis.rScene.CreateFPO(wd.MeshName);
        Assert.IsTrue(mesh != null);
        Debug.Log("Adding Mesh " + mesh);
        pVis.AddNonHashObject(mesh);
        //TODO Возможно, координаты для отрисовки выстрела прописываются здесь
        //*((MATRIX*)mesh) = owner->GetMatrix();
        // particle
        particle = pVis.CreateParticle(wd.ParticleName, wd.GetSpeed());
        Asserts.AssertEx(particle != null);

        //TODO Реализовать "прикрепление" партиклов к снаряду при выстреле
        //mesh.AttachObject(ref particle, new Vector3(0, 0, mesh.MinZ()), Vector3.forward, Vector3.up);
        pVis.AddNonHashObject(particle);
    }
    ~ProjectileVisualMeshAndParticle()
    {
        Dispose();
    }

    public override void Dispose()
    {
        if (particle != null)
            pVis.SubNonHashObject(particle);
        if (mesh != null)
        {
            pVis.SubNonHashObject(mesh);
            mesh.Release();
        }
        else
        {
            if (particle != null)
                particle.Release();
        }
        base.Dispose(); 
    }
}
//namespace MathConvert
//{
//    public Matrix34f FromLocus(Storm.Matrix locus)
//    {
//        return Matrix34f(Matrix3f(locus.Right, locus.Up, locus.Dir), locus.Org);
//    }
//    public Storm.Matrix ToLocus(Matrix34f tr)
//    {
//        return new Storm.Matrix(tr.pos, tr.tm[2], tr.tm[1], tr.tm[0]);
//    }
//};
