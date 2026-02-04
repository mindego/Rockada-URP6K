using UnityEngine;

public class ERasterizer<T> : RasterizeEnumer
{
    T myobject;
    //bool (T::* action) (int, int);
    TerrainAction action;
    public ERasterizer(T _object, TerrainAction _action)
    {
        myobject = _object;
        action = _action;
    }
    public bool ProcessElement(int x, int z)
    {
        //return myobject.action(x, z);
        //Debug.Log(string.Format("Processing {0}:{1}", x, z));
        return action(x, z);
    }
};
