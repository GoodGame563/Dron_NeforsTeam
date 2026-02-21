using UnityEngine;
using System.Collections.Generic;

public class PillarDebug : MonoBehaviour
{
    private PillarStation _station;

    private List<Pillar> _pillars;

    private void Start()
    {
        //_pillars = _station.GetPillarsArray();
    }

    public void BrokePillar(int id)
    {
        //if (id >= _pillars.Length)
        //{
        //    Debug.LogError("There is no pillar with thid id");
        //    return;
        //}

        //_pillars[id].SetBrokenState();
    }
}
