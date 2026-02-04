using System.Collections.Generic;
using UnityEngine;

public interface ICraftGroupService
{
    const uint ID = 0x1E963D39;
    public void avoidTerrain(float alt, float time);
    public void escort(float delta, List<string> groups);
    public void patrol(Vector3 center, float dist);
    public void duel(bool mEnable, float FightTime, float FightTimeBnd, float IdleTime, float IdleTimeBnd);
};

