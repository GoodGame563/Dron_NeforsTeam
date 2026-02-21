using UnityEngine;

public class Server : MonoBehaviour
{
    [SerializeField] private DronesStation _dronesStation;
    [SerializeField] private PillarStation[] _pillarsStation;

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
    }
}
