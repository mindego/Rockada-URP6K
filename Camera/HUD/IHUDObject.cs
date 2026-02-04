public interface IHUDObject :IHUDObjectData
{
    //bool Hide;
    //HUDTree Tree;

    //public bool Hide { get; set; }
    //public HUDTree Tree { get; set; }
    public void Update(float scale);
    public void BeginDraw();
    public void Draw();
    public void EndDraw();
    public  HudDeviceData GetData();
    public void Dispose();
    //virtual ~IHUDObject() { };

}

/// <summary>
/// Вспомогательный интерфейс, эмулирующий объявление свойств bool Hide и HUDTree Tree в интерфейсах "Шторма"
/// </summary>
public interface IHUDObjectData
{
    public bool IsHidden();
    public void SetHide(bool off);
    public void SetTree(HUDTree tree);
    public HUDTree GetTree();
}