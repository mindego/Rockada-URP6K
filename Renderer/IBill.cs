using UnityEngine;

public interface IBill
{
    public IBObject Create(int num_vertices, int num_faces);//params identify maximal capacity of the object 
    public void Draw(IBObject obj);
    public ITexturesDB CreateTexturesDB(string texturesPath);
    void SetTexture(Texture2D texture);
    Texture2D GetTexture();
    ICompositeMap CreateMap(MapData md, ITexturesDB texturesDB);
    public IBClipper CreateClipper(int v, Vector2[] clip);
    void SetStyle(BillStyle billStyle);
    void PushTrasform(Matrix2D m);
    void PopTrasform();
    void SetClipping(IBClipper viewAngleClipper);
    public string DumpTransformStack();
    public IFont CreateFont(string v, float width=1, float height = 1);
    void Begin();
    void End();
}

public interface IBObject : IObject
{
    string name { get; set; }

    //virtual void Draw()=0;//draws object in currently set transform should better use Bill::Draw(IBObject*) to draw BObject
    //void Draw( int num_v, int num_f)=0;

    public void SetActiveMesh(int n_vertices, int n_faces);//number vertices and faces to be drawn

    public bool Lock();//following methods should be called after this one
    public void SetFace(int f, int v0, int v1, int v2);
    public void SetVertO(int i, Vector2 pos, Vector2 tex);
    public void SetVertO(int i, Vector2 pos);
    public void SetVertC(int i, Color32 col, Color32 spec);
    public void SetVertC(int start, int num, Color32 col, Color32 spec);
    public void UnLock();//notifies ending of object contents change ,preceiding methods can not be called after this one
    public void SetBill(IBill bill);
}