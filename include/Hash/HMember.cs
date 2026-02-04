using UnityEngine;

public class HMember
{
    const int MAX_ENUM_VALUE = 3;
    geombase.Rect rect;
    public IHashObject hash_object;
    int[] ec = new int[MAX_ENUM_VALUE];  // enumer private data
    public HMember(IHashObject r = null)
    {
        hash_object = r;
        Clear();
    }
    void Clear() { Invalidate(); for (int i = 0; i < MAX_ENUM_VALUE; ++i) ec[i] = 0; }

    // const inline
    public IHashObject Object() { return hash_object; }
    bool InHash() { return rect.Valid(); }
    public void Invalidate() { rect.Invalidate(); }
    public geombase.Rect GetRect() { return rect; }
    public void SetRect(geombase.Rect _rect) { rect = _rect; }
    public  void SetObject(IHashObject r) { hash_object = r; }

    // enumerate
    public bool MatchOnLayer(uint Flag, int layer, int cmp_sign)
    {
        if (layer<0 || layer>=MAX_ENUM_VALUE) throw new System.Exception("Out of bound layer: " + layer + " of " + ec.Length);
        //Debug.LogFormat("cmp_sign {0} ({1}!={2}) MatchFlags {3} {4}", ec[layer] != cmp_sign, ec[layer],cmp_sign,Flag.ToString("X8"), hash_object.MatchFlags(Flag));
        return (ec[layer] != cmp_sign && hash_object.MatchFlags(Flag));
    }
    public void SetLayer(int layer, int cmp_sign)
    {
        ec[layer] = cmp_sign;
    }
    //template <class C> C * Query() { return object->Query<C>(); }
};
