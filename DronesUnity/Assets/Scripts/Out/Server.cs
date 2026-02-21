using UnityEngine;

public class Server : MonoBehaviour
{
    [SerializeField] private DronesStation _dronesStation;
    [SerializeField] private PillarStation[] _pillarsStation;
    [SerializeField] private PillarDebug _pillarDebug;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        int lastIndex = 0;

        //Инициализирует задавая каждой столб-станции
        for (int i = 0; i < _pillarsStation.Length; i++)
        {
            _pillarsStation[i].Initialize(lastIndex);
            lastIndex += _pillarsStation[i].PillarsCount;
        }

        _dronesStation.Initialize();
        _pillarDebug.Initialize(_pillarsStation);

        SubscribeAllPillarsStation(true);
    }

    private void PillarBroken(string brokenId)
    {
        _dronesStation.SayAboutBrokenPillar(brokenId);
    }

    private void SubscribeAllPillarsStation(bool subscribe)
    {
        if (subscribe)
        {
            foreach (PillarStation pilStation in _pillarsStation)
            {
                pilStation.OnPillarBroken += PillarBroken;
            }
        }
        else
        {
            foreach (PillarStation pilStation in _pillarsStation)
            {
                pilStation.OnPillarBroken -= PillarBroken;
            }
        }
    }
}
