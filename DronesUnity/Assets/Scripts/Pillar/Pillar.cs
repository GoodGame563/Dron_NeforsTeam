using UnityEngine;
using System;
using System.Collections;

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

    private Coroutine _moveToAnimPosCoroutine;

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
            StartCoroutine(WaitAndtartRepare());
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
        //так же менять материалы для визуализации и прочее
    }

    private void SendTotationToChangeLamp(string id)
    {
        _currentDrone.OnTakingBrokenAnimEnded -= SendTotationToChangeLamp;
        _currentDrone.GoChangeLamp();
        _anim.CloseDroneLocator();
        _currentDrone.OnDroneReachedTarget += StartSettingNewLamp;
    }

    private void StartSettingNewLamp(string droneID)
    {
        _currentDrone.StartRepairAnimation(false);
        _currentDrone.OnDroneReachedTarget -= StartSettingNewLamp;
        Debug.Log($"Drone {_currentDrone.ID} set new lamp to Pillar ({ID})");
        _isHasLamp = true;
        //_currentDrone.OnTakingBrokenAnimEnded += SendToHome;
    }

    private void StartTakingLampAnim(string droneID)
    {
        _currentDrone.StartRepairAnimation(true);
        _currentDrone.OnDroneReachedTarget -= StartTakingLampAnim;
        Debug.Log($"Drone {_currentDrone.ID} start repair Pillar ({ID})");
        _isHasLamp = false;
        _currentDrone.OnTakingBrokenAnimEnded += SendTotationToChangeLamp;

    }

    private IEnumerator WaitAndtartRepare()
    {
        yield return new WaitForSeconds(3f);

        // Плавный полёт к стартовой позиции анимации
        _currentDrone.MoveToStartAnimPos(_droneStartAnimPos);

        _currentDrone.OnDroneReachedTarget += StartTakingLampAnim;

        _anim.OpenDroneLocator();
    }

}
