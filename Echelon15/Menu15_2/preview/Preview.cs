public static class Preview
{

    public static void initialize()
    {

    }

    public class ViewPortForControls
    {
        public virtual void Show(bool _shown) { }
        public virtual void SetViewport(float[] VP) { }
        public virtual void Update(float scale) { }
        public virtual void Draw() { }
        ~ViewPortForControls() { }
    };
}