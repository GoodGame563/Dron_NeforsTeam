using Models;
using System.Collections.Generic;
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
    private List<string> _dronesIDList;
    private List<Models.Drone> _dronesModels;

    private GetPillarsMessage _pillarsResponseMesseage;

    private bool _isSubscribetToDrones;

    private void Awake()
    {
        _client = GetComponent<DroneStationClient>();

        if (_id == string.Empty || true) //id всегда при запуске пустое
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

    #region OnEnable-OnDisable

    private void OnEnable()
    {
        _client.OnRegisterStationResponse += RegisterStationResponse;
        _client.OnRegisterDronsResponse += RegisterDrones;
        _client.OnGetDronsResponse += GetDronesModel;
        _client.OnGetPillarsResponse += GetPillarsResponse;
        _client.OnError += ServError;
    }

    private void OnDisable()
    {
        _client.OnRegisterStationResponse -= RegisterStationResponse;
        _client.OnRegisterDronsResponse -= RegisterDrones;
        _client.OnGetDronsResponse -= GetDronesModel;
        _client.OnGetPillarsResponse -= GetPillarsResponse;
        _client.OnError -= ServError;
    }

    #endregion

    #region Subscribe to Client

    private void RegisterStationResponse(string uuid)
    {
        _id = uuid;
        Debug.Log($"@Drone Station gettet UUID: {uuid}");
    }
        
    private void RegisterDrones(List<string> dronesList)
    {
        _dronesIDList = dronesList;
        Debug.Log($"@Drone Station gettet dronesList: {dronesList.Count}");
    }

    private void GetDronesModel(List<Models.Drone> models)
    {
        _dronesModels = models;
        Debug.Log($"@Drone Station gettet drones model: Count: {_dronesModels.Count}");
    }

    private void GetPillarsResponse(GetPillarsMessage message)
    {
        _pillarsResponseMesseage = message;
        Debug.Log(message);
        Debug.Log($"@Drone Station gettet pillar response: Pillars count: {_pillarsResponseMesseage.Pillars.Count}");
    }

    private void ServError(string errorMesseage)
    {
        Debug.LogError($"@Drone station: {errorMesseage}");
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
        for (int i = 0; i < _dronesIDList.Count; i++)
        {
            _drones[i].Initialize(_dronesIDList[i], transform.position);
        }

    }

    public void SayAboutBrokenPillar(string id)
    {
        _pillarCoordinatesDict.TryGetValue(id, out Vector3 targetPillarPosition);

        //перед этим еще очередь проверить отправить сначало на корды из очереди

        SendDrone(targetPillarPosition);
    }

    #region Drone cooperation

    public void GoHomeAll()
    {
        foreach (Drone drone in _drones)
        {
            drone.GoChangeLamp();
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
            drone.ChangeLampInStation();
            
        }
    }

    #endregion

    #region Get drone

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

    private Drone GetMostChargedDrone()
    {
        float maxChargeValue = float.MinValue;
        Drone mostChargedDrone = null;

        foreach (Drone drone in _drones)
        {
            if (drone.CurrentDroneState == Drone.DronState.Ready&&
                drone.BatteryChargeLevel > maxChargeValue)
            {
                mostChargedDrone = drone;
                maxChargeValue = drone.BatteryChargeLevel;
            }
        }

        return mostChargedDrone;
    }

    #endregion

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


    private void SubscribeAllDrones(bool subscribe)
    {
        if (subscribe)
        {
            foreach (Drone drone in _drones)
            {
                if (drone.CurrentDroneState == Drone.DronState.Ready)
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
