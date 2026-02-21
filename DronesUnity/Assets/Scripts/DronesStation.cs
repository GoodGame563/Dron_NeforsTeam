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

    private bool _isSubscribetToDrones;

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


        int newId = 0;
        foreach (Drone drone in _drones)
        {
            drone.Initialize(newId, transform.position);
            newId++;
        }
    }

    public void SayAboutBrokenPillar(string id)
    {
        _pillarCoordinatesDict.TryGetValue(id, out Vector3 targetPillarPosition);

        SendDrone(targetPillarPosition);
    }

    private void SendDrone(Vector3 targetPillarPosition)
    {
        Debug.Log($"@Drones station: Trying to send drone");
        Drone droneToFly = GetMostChargedDrone();

        if (droneToFly == null)
        {
            _brokenPillarsQueue.Enqueue(targetPillarPosition);

            if (!_isSubscribetToDrones)
            {
                SubscribeAllDrones(true);
                _isSubscribetToDrones = true;
            }
            Debug.LogError($"@Drones station: All drones are busy. Task added to queue");
            return;
        }

        Debug.Log($"@Drones station: free drone founded");

        droneToFly.SetTarget(targetPillarPosition);
        droneToFly.SetHeight(20f);
        droneToFly.Launch();
    }


    private void DroneCharged(int droneID)
    {
        if (_isSubscribetToDrones)
        {
            SubscribeAllDrones(false);
            _isSubscribetToDrones = false;
        }

        Vector3 brokenPillarPos = _brokenPillarsQueue.Peek();
        SendDrone(brokenPillarPos);
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

    private void SubscribeAllDrones(bool subscribe)
    {
        if (subscribe)
        {
            foreach (Drone drone in _drones)
            {
                drone.OnDroneChargetEnought += DroneCharged;
            }
        }
        else
        {
            foreach (Drone drone in _drones)
            {
                drone.OnDroneChargetEnought -= DroneCharged;
            }
        }
    }
}
