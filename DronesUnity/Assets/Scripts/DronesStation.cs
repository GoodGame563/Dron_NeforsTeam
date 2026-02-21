using UnityEngine;
using System.Collections.Generic;

public class DronesStation : MonoBehaviour
{
    [SerializeField] private List<Drone> _drones;
    [SerializeField] private List<PillarStation> _pillarStations;
    
    public int DronesCount { get => _drones.Count; }

    public int BrokenDronesCount { get; private set; }

    private Dictionary<string, Vector3> _pillarCoordinatesDict = new Dictionary<string, Vector3>();

    private Queue<Vector3> _brokenPillarsQueue = new();

    public void Initialize()
    {
        Pillar[] pillarsArray = FindObjectsOfType<Pillar>();

        foreach (Pillar pillar in pillarsArray)
        {
            if (!_pillarCoordinatesDict.ContainsKey(pillar.ID))
            {
                _pillarCoordinatesDict.Add(pillar.ID, pillar.transform.position);
            }
        }
    }

    public void SayAboutBrokenPillar(string id)
    {
        _pillarCoordinatesDict.TryGetValue(id, out Vector3 targetPillarPosition);

        Drone droneToFly = GetMostChargedDrone();

        if (droneToFly == null)
        {
            Debug.LogError($"All drones are busy");
            _brokenPillarsQueue.Enqueue(targetPillarPosition);
            return;
        }

        droneToFly.SetTarget(targetPillarPosition);
    }







    private Drone GetMostChargedDrone()
    {
        float maxChargeValue = float.MinValue;
        Drone mostChargedDrone = null;

        foreach (Drone drone in _drones)
        {
            if (drone.CurrentDroneState == Drone.DronState.Charging&&
                drone.BatteryChargeLevel > maxChargeValue)
            {
                mostChargedDrone = drone;
                maxChargeValue = drone.BatteryChargeLevel;
            }
        }

        return mostChargedDrone;
    }
}
