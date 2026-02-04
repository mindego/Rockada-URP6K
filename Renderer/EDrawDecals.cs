using static renderer_dll;

public class EDrawDecals : HashEnumer
{
    Prof prof;
    public EDrawDecals()
    {
        prof = new Prof();
        prof.Reset();
    }
    ~EDrawDecals()  {}
    public bool ProcessElement(HMember m)
    {
        prof.AddRef();
        prof.Start();

        IHashObject o = m.Object();

        IDecalEss decal = (IDecalEss)o.Query(IDecalEss.ID);
        if (decal==null) return true;

        decal.Draw();
        decal.Release();
        /*
        int layer=decal->GetLayering();
        DecalVectorsMap::iterator v=sLayers.find(layer);
        if( sLayers.end()==v){
          v=sLayers.insert( std::pair<int,DecalVector*>(layer,new DecalVector) );
        }
        (*v).second->push_back( decal );
          */
        prof.End();
        return true;
    }

    public void Flush()
    {
        Log.Message("Enumed : {0} decals in {1} ( {2} average ).", prof.Count(), prof.Time(), prof.Avrg());
        //Log.Message("Enumed : %d decals in %d ( %d average ).", prof.Count(), prof.Time(), prof.Avrg());
        //int num=prof.Count();
        //prof.Reset();
        //prof.Start();
        //std::for_each( sLayers.begin(), sLayers.end(), FlushDecalVector);
        //prof.End();
        //Log->Message("...and drawn in %d ( %d average ).", prof.Time(), num?prof.Time()/num:0);
    }
}
