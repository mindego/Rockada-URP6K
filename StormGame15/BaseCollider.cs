using UnityEngine;

/// <summary>
/// BaseCollider - базовый класс для проверки столкновений между конкретным объектом и всеми остальными
/// </summary>
public class BaseCollider : VisualBaseActor
{
    // Константы
    const float MaxInterpolationStep = .5f; //meters
    const float Craft2GroundK = .25f;

    // от VisualBaseActor
    public override bool Update(float scale)
    {
        // проверяем столкновения
        CheckCollisions(scale);
        // здесь должен быть апдейт всяких звуков и проче фигни
        return (mpMainCollider != null);
    }

    // own API
    /// <summary>
    /// создать стокновения и отмотать назад
    /// </summary>
    /// <param name="scale"></param>
    protected virtual void Prepare(float scale)
    {
        mpMainCollider.Rewind(scale);
    }
    /// <summary>
    /// пошаговая интерполяция
    /// </summary>
    /// <param name="scale"></param>
    protected virtual void MakeStep(float scale)
    {
        mpMainCollider.MakeStep(scale);
    }
    /// <summary>
    /// непосредственно проверка столкновения
    /// </summary>
    protected virtual void DoCollisions()
    {
        string[] TerrainCollideExpl ={
            "HumanPlaneCollideWATER",
            "HumanPlaneCollideSAND",
            "HumanPlaneCollideDESERT",
            "HumanPlaneCollideGROUND",
            "HumanPlaneCollideGRASS",
            "HumanPlaneCollideROCKS",
            "HumanPlaneCollideSNOW",
            "HumanPlaneCollidePEAK",
          };
        string WaterCollideExpl = "HumanPlaneCollideWATER";

        // вызываем точную ф-цию проверки столкновения
        FPO pFPO = mpMainCollider.GetFpo();

        const int max_results = 256;
        //CollideResult[] res = new CollideResult[max_results];
        CollideResult[] res = Alloca.ANewN<CollideResult>(max_results);
        //#define MWorld MATRIX(VZero,VDir,VUp,VRight)
        MATRIX MWorld = new MATRIX(Vector3.zero, Vector3.forward, Vector3.up, Vector3.right);
        int n_ground = pFPO.CheckGroundCollision(MWorld, mrScene.GetTerrain(), ref res);
        int n_water = pFPO.CheckWaterCollision(MWorld, mrScene.GetTerrain(), ref res, n_ground);
        int n = n_ground + n_water;
        // обрабатываем результат
        if (n > 0)
        {

            if (n > max_results)
            {
                Asserts.Assert(false);
            }

            TPLIST<iBaseVictim> VictimsList = new TPLIST<iBaseVictim>();
            float damage = 0;
            Vector3 center = new Vector3(.0f, .0f, .0f);
            Vector3 normal = new Vector3(.0f, .0f, .0f);
            iBaseColliding coll = mpMainCollider;

            for (int i = 0; i < n; i++)
            {
                iBaseInterface intr = (iBaseInterface)res[i].caller_fpo.Link;
                iBaseVictim vict = intr != null ? (iBaseVictim)intr.GetInterface(iBaseVictim.ID) : null;

                if (vict != null && VictimsList.Find(vict) == null)
                    VictimsList.AddToTail(vict);

                center = res[i].org;
                normal = res[i].normal;
                normal.Normalize();
                // считаем физику
                Vector3 spd = coll.GetSpeedFor(center);
                float V1n = Vector3.Dot(spd, normal);

                //float dmg = BaseCollider::GetDamage(coll.GetWeight(), V1n, .0f, Craft2GroundK);
                float dmg = GetDamage(coll.GetWeight(), V1n, .0f, Craft2GroundK);

                if (V1n < .0f)
                {
                    //float U1n = BaseCollider::GetU1(V1n, .0f, Craft2GroundK);
                    float U1n = GetU1(V1n, .0f, Craft2GroundK);

                    float force = U1n - V1n;

                    if (vict != null)
                    {
                        float dm = Mathf.Min(dmg, vict.GetLife());
                        force *= dm / dmg;
                        dmg = dm;
                    }

                    //      spd-=normal*V1n;
                    //      Accel-=spd*(Craft2GroundDragC*(U1n-V1n));
                    coll.ApplyForce(normal * force, center);

                    // Effect
                    string expl_name;
                    if (i < n_ground)
                    {
                        Asserts.AssertBp(res[i].ground_type < 8 && res[i].ground_type >= 0);
                        expl_name = TerrainCollideExpl[res[i].ground_type];
                    }
                    else
                    {
                        expl_name = WaterCollideExpl;
                    }

                    CreateEffect(EXPLOSION_DATA.GetByName(expl_name, false),
                        center, spd, spd, normal);
                }
                // наносим повреждения
                damage += dmg;
            }
            // наносим повреждения
            if (VictimsList.Counter() != 0)
            {
                Debug.LogFormat("Ground damage for {0} subobjects of {1} at {2}", VictimsList.Counter(), mpMainCollider.GetFpo(), res[0].org);
                
                float d = damage / (VictimsList.Counter() * 2.0f);
                TPLIST_ELEM<iBaseVictim> l;
                for (l = VictimsList.Head(); l != null; l = l.Next())
                {
                    Debug.Log(l.Data());
                    l.Data().AddDamage(Constants.THANDLE_INVALID, iBaseVictim.WeaponCodeCollisionGround, d);
                }
            }
        }
    }
    /// <summary>
    /// конец процесса проверки
    /// </summary>
    protected virtual void Clean() { }
    /// <summary>
    /// "главный" объект умер
    /// </summary>
    public virtual void Release()
    {
        mpMainCollider = null;
    }
    public static iBaseVictim GetVictim(FPO f)
    {
        // пытаемся получить iBaseVictim
        iBaseInterface intr = (iBaseInterface)f.Link;
        if (intr == null) return null;
        iBaseVictim vict = (iBaseVictim)intr.GetInterface(iBaseVictim.ID);
        if (vict == null) return null;

        return vict;
    }
    public static float GetU1(float V1n, float V2n, float K)
    {
        return (V2n + (V2n - V1n) * K);
    }
    public static float GetDamage(float M1, float V1n, float V2n, float K)
    {
        return (M1 * Mathf.Pow((V1n - V2n), 2) * (1.0f - Mathf.Pow(K, 2)) * .0001f);
    }
    public BaseCollider(BaseScene s, iBaseColliding pColliding) : base(s.GetSceneVisualizer())
    {
        mrScene = s;
        mpMainCollider = pColliding;
    }
    ~BaseCollider() { }

    // own
    /// <summary>
    /// где работаем
    /// </summary>
    protected BaseScene mrScene;
    /// <summary>
    /// основной объект - его столкновабельность
    /// </summary>
    protected iBaseColliding mpMainCollider;
    /// <summary>
    /// шаг интерполяции
    /// </summary>
    protected float mScaleStep;
    /// <summary>
    /// определение шага интерполяции
    /// </summary>
    /// <param name="speed"></param>
    protected void UseSpeed(float speed)
    {
        // квадрат шага интерполяции не более MaxInterpolationStep2
        Asserts.AssertBp(speed >= 0);
        if (mScaleStep * speed < MaxInterpolationStep) return;
        // находим минимальный mScaleStep (закомментировано в исходниках Шторма)
        //mScaleStep=MaxInterpolationStep/speed;
    }
    /// <summary>
    /// полный цикл проверки столкновений
    /// </summary>
    /// <param name="scale"></param>
    private void CheckCollisions(float scale)
    {
        // проверяем условия проверки столкновений
        if (scale <= 0 || mpMainCollider == null || mpMainCollider.IsReady() == false) return;
        // готовимся
        Prepare(scale);
        // цикл интерполяции
        for (; scale > .0f; scale -= mScaleStep)
        {
            // проверяем стокновения и определяем коэфф. интерполирования
            mScaleStep = scale;
            DoCollisions();
            // интерполируем с учетом коэфф.
            if (mScaleStep > scale) mScaleStep = scale;
            MakeStep(mScaleStep);
        }
        // чистимся
        Clean();
    }
    private void CreateEffect(EXPLOSION_DATA data, Vector3 Org, Vector3 Dir, Vector3 Speed, Vector3 Up)
    {
        if (data == null) return;

        Vector3 EOrg = Org;
        //Assert(cmpPrecise(1, Up.Norma2(), 20));
        Vector3 EUp = Up; EUp.Normalize(); // ????
        Vector3 ESpeed = Speed;

        mrScene.GetSceneVisualizer().CreateVisualExplosion(data, EOrg, EUp, ESpeed, 0);
    }
};
