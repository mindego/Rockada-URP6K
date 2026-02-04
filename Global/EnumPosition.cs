using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// structures defines
/// </summary>
public class EnumPosition { 
    public Vector3 old_org, new_org;
    public float new_radius, old_radius;

    public EnumPosition()
    {
        old_radius = 0;
        old_org = new Vector3(-1000000, 0, -1000000);
    }

}
