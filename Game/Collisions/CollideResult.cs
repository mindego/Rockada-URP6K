using UnityEngine;

public class CollideResult
{
    public CollideClass coll_cls;
    public FPO caller_fpo;    // подобъект, для которого был вызван Collide
    //Вообще-то это union следующих двух переменных (первая - ссылка)
    public FPO collided_fpo;  // подобъект, с которым столкнулись
    public int ground_type;   // тип земли

    public Vector3 org;           // "точка столкновения" в мировых координатах
    public Vector3 normal;        // нормаль в "точка столкновения" в мировых координатах
}
