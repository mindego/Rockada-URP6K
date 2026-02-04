public abstract class TerrExpBase : IMesh
{
    public int GetNumParts() { return 1; }
    public MeshDesc GetPartDesc(int part)
    {
        Asserts.AssertBp(part == 0);
        if (!mDescValid)
            RetrieveDesc();
        return mDesc;
    }
    protected abstract void RetrieveDesc();
    protected virtual void Done()
    {
        Asserts.AssertBp(mRefCount == 0);
        mDescValid = false;
    }

    protected iTerrain mTerrain;

    protected MeshDesc mDesc;
    protected bool mDescValid;

    protected int mRefCount;

    public TerrExpBase()
    {
        mTerrain = null;
        mDescValid = false;
        mRefCount = 0;
    }

    public bool IsInUse() { return mRefCount != 0; }

    public void AddRef()
    {
        mRefCount++;
    }
    public int Release()
    {
        Asserts.AssertBp(mRefCount);
        mRefCount--;
        if (mRefCount != 0)
            return mRefCount;
        Done();
        return 0;
    }

    public virtual void Dispose() { }

    public abstract bool CopyVertices(int part, uint FVF, Strided pvertices);
    public abstract bool CopyIndices(int part, ref ushort[] pindices);
    public abstract bool CopyLocation(int part, ref Matrix34f m);
};
