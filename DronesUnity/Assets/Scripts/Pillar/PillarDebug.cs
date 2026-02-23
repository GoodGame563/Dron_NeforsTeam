using UnityEngine;
using System.Collections.Generic;

public class PillarDebug : MonoBehaviour
{
    private List<Pillar> _pillars = new();

    public void Initialize(PillarStation[] sstations)
    {
        foreach (PillarStation station in sstations)
        {
            Pillar[] pillars = station.GetPillarsArray();

            foreach (Pillar pil in pillars)
            {
                _pillars.Add(pil);
            }
        }
         
    }

    public void BrokePillar(int id)
    {
        System.Random rand = new();

        id = rand.Next(0, _pillars.Count);
        if (_pillars[id].CurrentState != Pillar.PillarState.Broken)
        {
            _pillars[id].SetBrokenState();
        }
    }
}
