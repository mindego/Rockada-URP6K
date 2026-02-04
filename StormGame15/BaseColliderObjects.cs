using UnityEngine;
using static RoFlags;
using static HashFlags;
/// <summary>
/// BaseColliderObjects - класс для проверки столкновений между конкретным объектом и всеми остальными
/// </summary>
public class BaseColliderObjects : BaseCollider, HashEnumer
{

    // от BaseCollider
    protected override void Prepare(float scale)
    {
        mScaleStep = scale;
        // определяем размеры и центр потенциальной сферы
        Vector3 t = mpMainCollider.GetOrg();
        t -= mpMainCollider.GetSpeed() * scale * .5f;
        float r = mpMainCollider.GetMaxSpeed() * scale * .5f + mpMainCollider.GetRadius();
        // енумерерируем все объекты в хэше в этом радиусе
        mrScene.GetHash().EnumSphere(new geombase.Sphere(t, r), ROObjectId(ROFID_FPO), this);
        // вызываем оригинальный Prepare
        base.Prepare(scale);
    }
    protected override void MakeStep(float scale)
    {
        BaseCollisionObject c;
        // двигаем всех участников
        for (c = mCollisionsList.Head(); c != null; c = c.Next())
            c.MakeStep(scale);
        // вызываем оригинальный MakeStep
        base.MakeStep(scale);
    }
    protected override void DoCollisions()
    {
        BaseCollisionObject c;
        // обрабатываем столкновения
        for (c = mCollisionsList.Head(); c != null; c = c.Next())
            UseSpeed(c.DoCollision());
        // вызываем оригинальный DoCollisions
        base.DoCollisions();
    }
    protected override void Clean()
    {
        // чистим список потенциальных столкновений
        mCollisionsList.Free();
        // вызываем оригинальный Clean
        base.Clean();
    }
    public BaseColliderObjects(BaseScene s, iBaseColliding pColliding) : base(s, pColliding) { Debug.LogFormat("New collider {0} created for {1}", this, pColliding); }
    ~BaseColliderObjects() { }

    // от HashEnumer-a
    public virtual bool ProcessElement(HMember h)
    {
        // пытаемся получить iBaseColliding
        FPO f = (FPO)(h.Object());
        //iBaseInterface intr = (iBaseInterface)f.Link;
        iBaseInterface intr = null;
        try
        {
            intr = (iBaseInterface)f.Link;
        } catch
        {
            Debug.LogErrorFormat("Failed to convert object to iBaseInterface " + f.Link);
            
        }
        if (intr == null) return true;
        iBaseColliding c = (iBaseColliding)intr.GetInterface(iBaseColliding.ID);
        if (c == null || c == mpMainCollider || c.IsReady() == false) return true;
        // добавляем новое потенциальное столкновение
        mCollisionsList.AddToTail(new BaseCollisionObject(mrScene, mpMainCollider, c, mScaleStep));
        return true;
    }

    // own
    protected TLIST<BaseCollisionObject> mCollisionsList = new TLIST<BaseCollisionObject>();// список потенциальных столкновений
};
