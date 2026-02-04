using System.Collections;
using System.IO;
using UnityEngine;

//#define MAX_MEMBER_COUNT       10
//#define FORMATION_COUNT        6

//#define iFormationLine         0x8ED9E437
//#define iFormationColoumn      0xF9367CFD
//#define iFormationWedge        0xE7AC1743
//#define iFormationVee          0x4DDD7EDA
//#define iFormationEchelonRight 0x4A75C3F9
//#define iFormationEchelonLeft  0x1EA00CD6
public abstract class FormationDefs
{
    public struct MyObjId
    {
        public string name;
        public uint obj_id;
        public static explicit operator ObjId(MyObjId d) => new ObjId(d.name, d.obj_id);

        public MyObjId(string _name, uint _obj_id)
        {
            name = _name;
            obj_id = _obj_id;
        }
    };
    public const int MAX_MEMBER_COUNT = 10;
    public const int FORMATION_COUNT = 6;

    public const uint iFormationLine = 0x8ED9E437;
    public const uint iFormationColoumn = 0xF9367CFD;
    public const uint iFormationWedge = 0xE7AC1743;
    public const uint iFormationVee = 0x4DDD7EDA;
    public const uint iFormationEchelonRight = 0x4A75C3F9;
    public const uint iFormationEchelonLeft = 0x1EA00CD6;

    public static MyObjId[] items_formations = {
        new MyObjId("Line"            , iFormationLine        ),
        new MyObjId("Coloumn"         , iFormationColoumn     ),
        new MyObjId( "Wedge"           , iFormationWedge       ),
        new MyObjId( "Vee"             , iFormationVee         ),
        new MyObjId( "EchelonRight"    , iFormationEchelonRight),
        new MyObjId( "EchelonLeft"     , iFormationEchelonLeft ),
    };

    public static string GetFormationNameFromId(uint id)
    {
        for (int i = 0; i < FORMATION_COUNT; i++)
            if (items_formations[i].obj_id == id)
                return items_formations[i].name;
        return null;
    }
}
public class FormationInfo : IStormImportable<FormationInfo>
{
    public const int MAX_MEMBER_COUNT = 10;
    public Vector3[] Delta = new Vector3[MAX_MEMBER_COUNT];

    public FormationInfo Import(Stream st)
    {
        int memberCount = (int)(st.Length / (sizeof(float) * 3));
        Delta = StormFileUtils.ReadStructs<Vector3>(st, 0, memberCount);
        return this;
    }
}
//#define PATROL_DIM 17

//#define iPatrolRoute 0x9636F300

public class PatrolInfo : IStormImportable<PatrolInfo>
{
    public const int PATROL_DIM = 17;
    public const uint iPatrolRoute = 0x9636F300;

    public Vector3[] Delta = new Vector3[PATROL_DIM];

    public PatrolInfo Import(Stream st)
    {
        int memberCount = (int)(st.Length / (sizeof(float) * 3));
        Delta = StormFileUtils.ReadStructs<Vector3>(st, 0, memberCount);
        return this;
    }
}

