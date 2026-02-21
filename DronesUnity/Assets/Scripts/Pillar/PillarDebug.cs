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
        if (id >= _pillars.Count)
        {
            Debug.LogError("There is no pillar with thid id");
            return;
        }

        _pillars[id].SetBrokenState();
        id+=2;
        _pillars[id].SetBrokenState();
        id+= 3;
        _pillars[id].SetBrokenState();
    }
}
