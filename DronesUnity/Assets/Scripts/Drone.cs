using UnityEngine;
using System;

public class Drone : MonoBehaviour
{
    public enum DronState
    {
        Charging,
        Flying,
        Broken
    }

    public enum DroneLampHolderState
    {
        Empty,
        HasBrokenLamp,
        HasWorkingLamp
    }

    public DronState CurrentDroneState { get; private set; }
    public DroneLampHolderState CurrentLampHolderState { get; private set; }

    public float BatteryChargeLevel { get; private set; }

    private Rigidbody _rb;

    private Vector3 _targetCoordinates;
    private Vector3 _stationCoordinates;

    private float _requiredHeight;

    public event Action OnDroneChargetEnoght;

    public void Initialize(Vector3 stationCoordinates)
    {
        _stationCoordinates = stationCoordinates;
        _rb = GetComponent<Rigidbody>();
    }

    public void Launch()
    {
        if (CurrentDroneState == DronState.Broken ||
            CurrentDroneState == DronState.Flying)
        {
            Debug.LogError($"Can't launch. Drone is {CurrentDroneState}");
            return;
        }


    }

    #region Setters

    public void SetHeight(float requiredHeight)
    {
        _requiredHeight = requiredHeight;
    }

    public void SetTarget(Vector3 targetCoordinates)
    {
        _targetCoordinates = targetCoordinates;
    }

    #endregion

    
}
