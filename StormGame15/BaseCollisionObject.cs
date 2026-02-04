using UnityEngine;
/// <summary>
/// BaseCollisionObject - класс для хранения и обработки столкновений с объектами
/// </summary>
public class BaseCollisionObject : TLIST_ELEM<BaseCollisionObject>
{
    const float Object2ObjectK = .25f;
    private TLIST_ELEM_IMP<BaseCollisionObject> myTLIST = new TLIST_ELEM_IMP<BaseCollisionObject>();

    protected BaseScene m_Scene;
    iBaseColliding mpFirstCollider;
    iBaseColliding mpSecondCollider;
    public BaseCollisionObject(BaseScene Scene, iBaseColliding first, iBaseColliding second, float scale)
    {
        m_Scene = Scene;
        mpFirstCollider = first;
        mpSecondCollider = second;
        mpSecondCollider.Rewind(scale);
    }
    public float DoCollision()
    {
        // вызываем проверку столкновений
        if (mpFirstCollider.GetFpo() == null || mpSecondCollider.GetFpo() == null) return .0f;
        //CollideResult[] res = new CollideResult[256];
        CollideResult[] res= Alloca.ANewN<CollideResult>(256);
        int n = mpFirstCollider.GetFpo().Collide(mpSecondCollider.GetFpo(), mpSecondCollider.GetFpo(), ref res);
        // обрабатываем результат
        if (n > 0)
        {
            Vector3 center = new Vector3(.0f, .0f, .0f);
            Vector3 normal = new Vector3(.0f, .0f, .0f);
            for (int i = 0; i < n; i++)
            {
                iBaseVictim ColliderVictim = BaseCollider.GetVictim(res[i].caller_fpo);
                iBaseVictim CollidedVictim = BaseCollider.GetVictim(res[i].collided_fpo);
                center = res[i].org;
                normal = res[i].normal;
                normal.Normalize();
                normal = -normal;
                // считаем физику
                Vector3 Sp1 = mpFirstCollider.GetSpeedFor(center);
                float V1n = Vector3.Dot(Sp1, normal);
                Vector3 Sp2 = mpSecondCollider.GetSpeedFor(center);
                float V2n = Vector3.Dot(Sp2, normal);
                if (V1n - V2n < .0f)
                {
                    float U1n = BaseCollider.GetU1(
                      V1n,
                      V2n, Object2ObjectK);
                    float U2n = BaseCollider.GetU1(
                      V2n,
                      V1n, Object2ObjectK);

                    float dmg = BaseCollider.GetDamage(mpFirstCollider.GetWeight(), V1n, V2n, Object2ObjectK);

                    float forceLimA =
                        ColliderVictim != null ? Mathf.Min(ColliderVictim.GetLife(), dmg) / dmg : 1;

                    float forceLimB =
                        CollidedVictim != null ? Mathf.Min(CollidedVictim.GetLife(), dmg) / dmg : 1;

                    mpFirstCollider.ApplyForce(normal * ((U1n - V1n) * forceLimA), center);
                    mpSecondCollider.ApplyForce(normal * ((U2n - V2n) * forceLimB), center);

                    CreateEffect(center, Sp1, Sp1, normal);


                    if (ColliderVictim != null) ColliderVictim.AddDamage(Constants.THANDLE_INVALID, iBaseVictim.WeaponCodeCollisionObject, dmg);
                    if (CollidedVictim != null) CollidedVictim.AddDamage(Constants.THANDLE_INVALID, iBaseVictim.WeaponCodeCollisionObject, dmg);
                }
            }
        }
        // возвращаем скорость сближения
        return (mpSecondCollider.GetMaxSpeed() + mpFirstCollider.GetMaxSpeed());
    }
    public void MakeStep(float scale)
    {
        mpSecondCollider.MakeStep(scale);
    }
    public void CreateEffect(Vector3 Org, Vector3 Dir, Vector3 Speed, Vector3 Up)
    {
        EXPLOSION_DATA data = EXPLOSION_DATA.GetByName("HumanPlaneCollideObject", false);
        if (data == null) return;

        Vector3 EOrg = Org;
        //Assert(cmpPrecise(1, Up.Norma2(), 20));
        Vector3 EUp = Up; EUp.Normalize();
        Vector3 ESpeed = Speed;

        m_Scene.GetSceneVisualizer().
          CreateVisualExplosion(data, EOrg, EUp, ESpeed, 0);
    }

    public BaseCollisionObject Next()
    {
        return ((TLIST_ELEM<BaseCollisionObject>)myTLIST).Next();
    }

    public BaseCollisionObject Prev()
    {
        return ((TLIST_ELEM<BaseCollisionObject>)myTLIST).Prev();
    }

    public void SetNext(BaseCollisionObject t)
    {
        ((TLIST_ELEM<BaseCollisionObject>)myTLIST).SetNext(t);
    }

    public void SetPrev(BaseCollisionObject t)
    {
        ((TLIST_ELEM<BaseCollisionObject>)myTLIST).SetPrev(t);
    }

    public void Dispose()
    {
        throw new System.NotImplementedException();
    }
}
