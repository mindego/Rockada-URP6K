using System.Collections.Generic;

public static class IParticleAnimation { //TODO перенести в отдельный static class для inline функций
    //public static void updateAnimation(AnyRTab<IAnimation> lca, float scale, bool visible)
    public static void updateAnimation(List<IAnimation> lca, float scale, bool visible)
    {
        for (int i = 0; i < lca.Count;)
        {
            if (!lca[i].update(scale, visible))
                //lca.erase(i);
                lca.RemoveAt(i);
            else
                i++;
        }
    }
}