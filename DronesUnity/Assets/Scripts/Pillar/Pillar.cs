using UnityEngine;
using System;

public class Pillar : MonoBehaviour
{
    public enum PillarState
    {
        Working,
        Broken
    }

    [SerializeField] private Transform _droneStartAnimPos;

    public PillarState CurrentState { get; private set; }

    public string ID { get; private set; }

    private PillarAnimations _anim;
    private Drone _currentDrone;

    private bool _isSignalsTranslating;
    private bool _isHasLamp;

    public event Action<string> OnBroken;

    private void OnTriggerEnter(Collider other)
    {
        if (_currentDrone == null&&
            other.CompareTag("Drone")&&
            CurrentState == PillarState.Broken)
        {
            _currentDrone = other.GetComponent<Drone>();

            _currentDrone.SetTarget(_droneStartAnimPos.position, false);

            _currentDrone.OnDroneReachedTarget += StartTakingLampAnim;

            _currentDrone.Launch();
            _anim.OpenDroneLocator();
        }
    }

    public void Initialize(string newId)
    {
        ID = newId;
        CurrentState = PillarState.Working;

        _anim = GetComponent<PillarAnimations>();
    }

    public void SetState(PillarState newState)
    {
        CurrentState = newState;
    }

    public void OpenSignals()
    {
        if (_isSignalsTranslating) return;
        
        _isSignalsTranslating = true;
        //_anim.SetBool("OpenSignals", _isSignalsWorking);
    }

    public void CloseSignals()
    {
        if (!_isSignalsTranslating) return;

        _isSignalsTranslating = false;
        //_anim.SetBool("OpenSignals", _isSignalsWorking);
    }

    /// <summary>
    /// Simulate breakdown
    /// </summary>
    public void SetBrokenState()
    {
        if (CurrentState == PillarState.Broken)
        {
            Debug.LogWarning($"Pillar ({ID}) already broken!");
        }
        CurrentState = PillarState.Broken;
        Debug.LogWarning($"@Debug: You broke the Pillar {ID}");

        _anim.BrokeLamp();        

        OnBroken?.Invoke(ID);
        //так же мен€ть материалы дл€ визуализации и прочее
    }

    private void SendToHome(string id)
    {
        _currentDrone.OnTakingBrokenAnimEnded -= SendToHome;
        _currentDrone.GoHome();
        _anim.CloseDroneLocator();
        _currentDrone.OnDroneReachedTarget += StartSettingNewLamp;
    }

    private void StartSettingNewLamp(string droneID)
    {
        _currentDrone.StartRepairAnimation(true);
        _currentDrone.OnDroneReachedTarget -= StartSettingNewLamp;
        Debug.Log($"Drone {_currentDrone.ID} set new lamp to Pillar ({ID})");
        _isHasLamp = true;
        _currentDrone.OnTakingBrokenAnimEnded += SendToHome;
    }

    private void StartTakingLampAnim(string droneID)
    {
        _currentDrone.StartRepairAnimation(false);
        _currentDrone.OnDroneReachedTarget -= StartTakingLampAnim;
        Debug.Log($"Drone {_currentDrone.ID} start repair Pillar ({ID})");
        _isHasLamp = false;
        _currentDrone.OnTakingBrokenAnimEnded += SendToHome;

    }
}
