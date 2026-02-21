using UnityEngine;

public class PillarStation : MonoBehaviour
{
    [SerializeField] private Pillar[] _pillars;

    public int PillarsCount { get => _pillars.Length; }

    public void Initialize(int rangeStart)
    {
        for (int i = 0; i < _pillars.Length; i++)
        {
            _pillars[i].Initialize(rangeStart.ToString());
            rangeStart++;
            Debug.Log($"@{name}: {_pillars[i].name} id: {_pillars[i].ID}");
        }
    }







    public Pillar[] GetPillarsArray()
    {
        return _pillars;
    }

    private void SubscribeAllPillars(bool subscribe)
    {
        if (subscribe)
        {
            foreach (Pillar pillar in _pillars)
            {

            }
        }
        else
        {

        }
    }
}
