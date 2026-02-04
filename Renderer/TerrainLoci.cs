using UnityEngine;
using static TerrainDefs;

public class TerrainLoci
{
    public TerrainLoci(int x, int z) { }
    public bool Initialize(Cd3d d3d)
    {
        return true;
    }
    public ITerrainLocus CreateTerrainLocus(int x, int z)
    {
        int
            bx = x / HASHEDBOXESX,
            bz = z / HASHEDBOXESZ;
        TerrainLocus locus = m_NotePool.CreateObject();
        locus.Initialize(bx, bz, bx * HASHEDBOXESX * BOX_SIZE, bz * HASHEDBOXESZ * BOX_SIZE);
        return locus;
    }
    ~TerrainLoci() { }
    private NotePool<TerrainLocus> m_NotePool = new() ;
    const int HASHEDBOXESX = 16;
    const int HASHEDBOXESZ = 16;
};


public class NotePool<T> where T:class,new()
{
    public T CreateObject()
    {
        return new T();
    }
}
//public class NotePool<T> where T : class
//{
//    NotePool()
//    {
//        mCont = mContAlloc;
//        mOuterCont = mOuterContAlloc;

//#if _DEBUG
//  mNumCreated=0;
//#endif
//    }
//    ~NotePool()
//    {
//        Asserts.AssertBp(mNumCreated == (mCont.size() + mOuterCont.size()));
//        { for (Cont::iterator it = mCont.begin(); it != mCont.end(); ++it) delete(*it); }
//        { for (OuterCont::iterator it = mOuterCont.begin(); it != mOuterCont.end(); ++it) delete(*it); }
//    }
//    T CreateObject(IObject outer = null)
//    {
//        T myobject = null;
//        if (outer != null)
//        {
//            NotifierOuter ret = null;
//            if (mOuterCont.size())
//            {
//                OuterCont::iterator it = mOuterCont.begin();
//                ret = *it;
//                mOuterCont.erase(mOuterCont.begin());
//            }
//            else
//            {
//#if _DEBUG
//    ++mNumCreated;
//#endif
//                ret = new NotifierOuter(this);
//            }
//            ret.SetOuter(outer);
//            myobject = ret;
//        }
//        else
//        {
//            Notifier ret = null;
//            if (mCont.size())
//            {
//                Cont::iterator it = mCont.begin();
//                ret = *it;
//                mCont.erase(mCont.begin());
//            }
//            else
//            {
//#if defined _DEBUG
//    ++mNumCreated;
//#endif
//                ret = new Notifier(this);
//            }
//            object= ret;
//        }
//        object->AddRef();
//        return object;
//    }
//    protected:
//  struct Notifier;
//    friend struct Notifier;
//    struct Notifier : T
//    {
//        Notifier(NotePool &pool) : mRefCount(0), mNotePool(pool) { }
//        void AddRef()
//        {
//            ++mRefCount;
//        }
//        int Release()
//        {
//            AssertBp(mRefCount);
//            --mRefCount;
//            if (mRefCount)
//                return mRefCount;
//            mNotePool.NotifyRelease(this);
//            return 0;
//        }
//        protected:
//    NotePool &mNotePool;
//    int mRefCount;
//    };

//    void NotifyRelease(Notifier* notifier)
//    {
//        mCont.push_back(notifier);
//    }

//    class NotifierOuter : T
//    {
//        NotifierOuter(NotePool pool)
//        {
//            mRefCount = 0;
//            mNotePool = pool;

//            mOuter = null;
//        }
//        IObject SetOuter(IObject outer) { return mOuter = outer; }
//        void AddRef()
//        {
//            mOuter.AddRef();
//            ++mRefCount;
//        }
//        int Release()
//        {
//            Asserts.AssertBp(mRefCount);
//            mOuter.Release();
//            --mRefCount;
//            if (mRefCount != 0)
//                return mRefCount;
//            mNotePool.NotifyRelease(this);
//            return 0;
//        }
//        protected NotePool mNotePool;
//        protected int mRefCount;
//        protected IObject mOuter;
//    };

//    void NotifyRelease(NotifierOuter notifier)
//    {
//        mOuterCont.push_back(notifier);
//    }

//    typedef wNodePool<Notifier, wListNode<Notifier> >       ContAlloc;
//    typedef wList<Notifier, wListNode<Notifier>, ContAlloc> Cont;
//    ContAlloc mContAlloc;
//    Cont mCont;

//    typedef wNodePool<NotifierOuter*, wListNode<NotifierOuter*> >       OuterContAlloc;
//    typedef wList<NotifierOuter*, wListNode<NotifierOuter*>, OuterContAlloc> OuterCont;
//    OuterContAlloc mOuterContAlloc;
//    OuterCont mOuterCont;

//#if defined _DEBUG
//  int mNumCreated;
//#endif

//};