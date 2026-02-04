using static D3DEMULATION;
using DWORD = System.UInt32;

public class VBPipeImpl : VBPipe
{

    protected IDirect3D7 d3d;

    ~VBPipeImpl()
    {
        if (d3d!=null)
        {
            (d3d).Release(); (d3d) = null;
        }
    }
    public DWORD GetFormat() { return FVF; }
    DWORD GetSize() { return (uint)max_size; }
    DWORD GetCaps() { return Caps; }

    bool Resize(int new_size) {
        throw new System.NotImplementedException();
        max_size = new_size;
    }


    VBPipeImpl() : base()
    {
        d3d = null;
    }
    bool Initialize(IDirect3D7 _d3d, int size, DWORD _Caps, DWORD _FVF = D3DFVF_TLVERTEX)
    {
        Asserts.AssertBp(vb==null);

        FVF = _FVF;
        Caps = _Caps;
        max_size = size;

        //STUB
        return true;
    }
};