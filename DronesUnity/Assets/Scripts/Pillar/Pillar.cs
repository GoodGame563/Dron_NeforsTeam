using UnityEngine;
using System;

public class Pillar : MonoBehaviour
{
    public enum PillarState
    {
        Working,
        Broken
    }

    public PillarState CurrentState { get; private set; }

    public string ID { get; private set; }

    private Animator _animator;

    private bool _isSignalsWorking;

    public event Action<string> OnBroken;

    public void Initialize(string newId)
    {
        ID = newId;
        CurrentState = PillarState.Working;

        _animator = GetComponent<Animator>();
    }

    public void SetState(PillarState newState)
    {
        CurrentState = newState;
    }

    public void OpenSignals()
    {
        if (_isSignalsWorking) return;
        
        _isSignalsWorking = true;
        _animator.SetBool("OpenSignals", _isSignalsWorking);
    }

    public void CloseSignals()
    {
        if (!_isSignalsWorking) return;

        _isSignalsWorking = false;
        _animator.SetBool("OpenSignals", _isSignalsWorking);
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

        

        OnBroken?.Invoke(ID);
        //так же мен€ть материалы дл€ визуализации и прочее
    }
}
