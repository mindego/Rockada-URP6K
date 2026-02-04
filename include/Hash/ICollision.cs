using UnityEngine;
public interface ICollision : IRefMem
{
    // data access
    public CollisionData GetData(ObjId id);
    // collisions and his results
    public CollideInfo Collide(HMember who_collided, int Flags = (int)CollisionDefines.COLLF_ALL);
    public TraceInfo TraceLine(Geometry.Line line, HMember ignored = null, int Flags = (int)CollisionDefines.COLLF_ALL);
    public CollideInfo CollideSphere(Vector3 org, float radius);
    public bool CollideObjSphere(IHashObject obj, Vector3 o, float r = 0);
};