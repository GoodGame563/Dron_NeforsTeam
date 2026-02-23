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
        if (!other.CompareTag("Drone"))
            return;

        if (CurrentState != PillarState.Broken)
            return; // ✅ больше не реагируем

        Drone newDrone = other.GetComponent<Drone>();
        if (_currentDrone != null && _currentDrone != newDrone)
            return;

        _currentDrone = newDrone;
        StartCoroutine(WaitAndtartRepare());
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
        _currentDrone.OnDroneReachedTarget -= StartSettingNewLamp; // 🔥

        _currentDrone.GoChangeLamp();
        _anim.CloseDroneLocator();
    }

    private void StartSettingNewLamp(string droneID)
    {
        _currentDrone.StartRepairAnimation(false); // SetLamp
        _currentDrone.OnDroneReachedTarget -= StartSettingNewLamp;

        CurrentState = PillarState.Working; // ✅ КЛЮЧЕВО
        _currentDrone = null;               // ✅ освобождаем столб

        Debug.Log($"Drone {_currentDrone.ID} finished repair Pillar ({ID})");
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
