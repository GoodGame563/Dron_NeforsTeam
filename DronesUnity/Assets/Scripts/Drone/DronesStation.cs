using Models;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[RequireComponent(typeof(DroneStationClient))]
public class DronesStation : MonoBehaviour
{
    [SerializeField] private List<Drone> _drones;
    [SerializeField] private List<PillarStation> _pillarStations;

    private DroneStationClient _client;

    public int DronesCount { get => _drones.Count; }

    public int BrokenDronesCount { get; private set; }

    private Dictionary<string, Vector3> _pillarCoordinatesDict = new Dictionary<string, Vector3>();

    private Queue<Vector3> _brokenPillarsQueue = new();

    private string _id;
    private List<string> _dronesList;

    private bool _isSubscribetToDrones;

    private void Start()
    {
        _client = GetComponent<DroneStationClient>();

        if (_id == null)
        {
            _client.RegisterStation(new Models.RegisterMessage()
            {
                Event = "register",
                Coordinates = new Coordinates()
                {
                    Latitude = transform.position.x,
                    Longtiude = transform.position.z
                },
                Radius = 1000,
                TotalDroneCount = 2,
                TotalLampsCount = -1

            });
        }
    }

    private void OnEnable()
    {
        _client.OnRegisterStationResponse += RegisterStationResponse;
    }

    #region Client

    private void RegisterStationResponse(string uuid)
    {
        _id = uuid;
    }
        
    private void RegisterDrones(List<string> dronesList)
    {
        _dronesList = dronesList;
    }

    #endregion

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
        for (int i = 0; i < _dronesList.Count; i++)
        {
            _drones[i].Initialize(_dronesList[i], transform.position);
        }

    }

    public void SayAboutBrokenPillar(string id)
    {
        _pillarCoordinatesDict.TryGetValue(id, out Vector3 targetPillarPosition);

        //перед этим еще очередь проверить отправить сначало на корды из очереди

        SendDrone(targetPillarPosition);
    }

    public void GoHomeAll()
    {
        foreach (Drone drone in _drones)
        {
            drone.GoHome();
        }
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
        droneToFly.OnDroneNeedChangingLamp += DroneReturned;
    }

    private void DroneReturned(string ID)
    {
        Drone drone = GetDroneByID(ID);



        if (drone.IsHasBrokenLamp)
        {
            drone.TakeWorkingLamp();
            
        }
    }

    private Drone GetDroneByID(string ID)
    {
        foreach (var item in _drones)
        {
            if (item.ID == ID)
            {
                return item;
            }
        }
        return null;
    }
    private void DroneReady(string droneID)
    {
        Debug.LogError("Drone ready");
        Debug.LogError("Drone ready");
        Debug.LogError("Drone ready");

        Drone drone = GetDroneByID(droneID);
        drone.OnDroneChargetEnought -= DroneReady;
        drone.OnDroneNeedChangingLamp -= DroneReady;

        Vector3 brokenPillarPos = _brokenPillarsQueue.Peek();

        if (_brokenPillarsQueue.Count == 0)
        {
            SubscribeAllDrones(false);
            _isSubscribetToDrones = false;
        }

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
                if (drone.CurrentDroneState == Drone.DronState.Charging)
                {
                    drone.OnDroneChargetEnought += DroneReady;
                }
            }
        }
        else
        {
            foreach (Drone drone in _drones)
            {
                drone.OnDroneChargetEnought -= DroneReady;
            }
        }
    }
}
