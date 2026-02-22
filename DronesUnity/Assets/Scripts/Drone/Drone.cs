using UnityEngine;
using System;
using System.Collections;

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

    public enum FlightSubState
    {
        TakingOff,      // Взлет
        Hovering,       // Зависание
        Moving,         // Движение к цели
        Descending,     // Спуск к лампе
        Landing         // Посадка
    }

    public DronState CurrentDroneState { get; private set; }
    public FlightSubState CurrentFlightState { get; private set; }
    public DroneLampHolderState CurrentLampHolderState { get; private set; }

    public Vector3 TargetCoordinates;
    public Vector3 CurrentLampCoord;

    public string ID { get; private set; }

    public float BatteryChargeLevel { get; private set; }

    private Rigidbody _rb;


    private Vector3 _stationCoordinates;

    private float _reservedFlyHight;
    private float _lampHeight = 11.861f;

    [Header("Настройки полета")]
    [SerializeField] private float _flightSpeed = 5f; // Скорость полета
    [SerializeField] private float _heightSpeed = 5f; // Скорость набора высоты
    [SerializeField] private float _stopDistance = 0.1f; // Дистанция для остановки
    [SerializeField] private float _hoverHeightTolerance = 00f; // Допуск по высоте для зависания

    public event Action<string> OnDroneChargetEnought;
    public event Action<string> OnDroneReachedTarget; // Событие достижения цели
    public event Action<string> OnDroneNeedChangingLamp; // Событие достижения цели
    public event Action<string, FlightSubState> OnFlightStateChanged; // Событие изменения состояния полета

    public event Action<string> OnDroneReady;

    public event Action<string> OnTakingBrokenAnimEnded;
    public event Action<string> OnTakingWorkingAnimEnded;

    private Coroutine _flightCoroutine;
    private Coroutine _hoverCoroutine;
    private bool _isHovering = false;
    private bool _isGoHome;
    public bool IsHasBrokenLamp { get; private set; }

    private Animator _anim;

    public void Initialize(string newId, Vector3 stationCoordinates)
    {
        _stationCoordinates = stationCoordinates;
        ID = newId;
        _rb = GetComponent<Rigidbody>();
        CurrentDroneState = DronState.Charging;
        CurrentFlightState = FlightSubState.Hovering; // Начальное состояние - зависание на земле

        _anim = GetComponent<Animator>();
        Debug.Log($"Drone {ID} Initialized");
    }

    public void Launch()
    {
        if (CurrentDroneState == DronState.Broken ||
            CurrentDroneState == DronState.Flying)
        {
            Debug.LogError($"Can't launch. Drone is {CurrentDroneState}");
            return;
        }

        if (_flightCoroutine != null)
            StopCoroutine(_flightCoroutine);

        Debug.Log($"Drone ({ID}) launched");

        _flightCoroutine = StartCoroutine(FlySequence());
    }

    public void GoHome()
    {
        Vector3 stationPos = new(_stationCoordinates.x, _stationCoordinates.y, _stationCoordinates.z);
        SetTarget(stationPos, false);
        Launch();
        Debug.LogWarning($"Drone ({ID}) go home");
        _anim.applyRootMotion = true;
    }

    public void StartRepairAnimation(bool isHasLamp)
    {
        if (isHasLamp)
        {
            _anim.SetTrigger("SetLamp");

            IsHasBrokenLamp = false;
        }
        else
        {
            _anim.SetTrigger("TakeLamp");
            IsHasBrokenLamp = true;
            CurrentLampCoord = TargetCoordinates;
        }
        _anim.applyRootMotion = false;
    }

    public void TakeWorkingLamp()
    {
        _anim.SetTrigger("ChangeLamp");
        IsHasBrokenLamp = false;
    }

    public void OnChangeWorkingLampAnimEnded()
    {
        SetTarget(CurrentLampCoord);
        
        Launch();
    }

    public void OnTakeBrokenLampAnimEnded()
    {
        OnTakingBrokenAnimEnded?.Invoke(ID);
    }

    public void OnChangingLampAnimEnded()
    {
        OnTakingBrokenAnimEnded?.Invoke(ID);
    }

    private IEnumerator FlySequence()
    {
        CurrentDroneState = DronState.Flying;

        // Шаг 1: Поднимаемся на заданную высоту
        SetFlightState(FlightSubState.TakingOff);
        Debug.Log($"Дрон {ID}: Начинаем подъем на высоту {_reservedFlyHight}");
        yield return StartCoroutine(LiftToHeight(_reservedFlyHight));

        // Зависаем на достигнутой высоте
        SetFlightState(FlightSubState.Hovering);
        Debug.Log($"Дрон {ID}: Зависаем на высоте {_reservedFlyHight}");
        StartHovering();

        // Шаг 2: Летим к целевой точке на заданной высоте
        SetFlightState(FlightSubState.Moving);
        Debug.Log($"Дрон {ID}: Летим к цели {TargetCoordinates}");
        yield return StartCoroutine(FlyToTarget(TargetCoordinates));

        // Шаг 3: Спускаемся на высоту лампы (_lampHeight)
        SetFlightState(FlightSubState.Descending);
        Debug.Log($"Дрон {ID}: Спускаемся на высоту лампы {_lampHeight}");
        StopHovering();
        yield return StartCoroutine(DescendToLampHeight());

        // Шаг 4: Зависаем на высоте лампы
        SetFlightState(FlightSubState.Hovering);
        Debug.Log($"Дрон {ID}: Зависаем на высоте лампы в точке назначения");
        StartHovering();

        Debug.Log($"Дрон {ID}: Достиг цели и находится на высоте лампы!");
        CurrentDroneState = DronState.Charging;
        SetFlightState(FlightSubState.Hovering);

        if (_isGoHome)
        {
            if (IsHasBrokenLamp)
            {
                OnDroneNeedChangingLamp?.Invoke(ID);
            }
            else
            {
                OnDroneReady?.Invoke(ID);
                //здесь он должен начать ждать
            }
        }
        else
        {
            OnDroneReachedTarget?.Invoke(ID);
        }
    }

    #region Fly process

    private IEnumerator LiftToHeight(float targetHeight)
    {
        StopHovering();

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = new Vector3(startPosition.x, targetHeight, startPosition.z);
        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float startTime = Time.time;

        while (Mathf.Abs(transform.position.y - targetHeight) > _hoverHeightTolerance)
        {
            float distanceCovered = (Time.time - startTime) * _heightSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;

            // Используем Lerp для плавного подъема
            Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);

            // Проверяем, не превысили ли мы целевую высоту
            if (Mathf.Abs(newPosition.y - targetHeight) < 0.01f)
            {
                newPosition.y = targetHeight;
            }

            transform.position = newPosition;

            yield return null;
        }

        // Фиксируем конечную позицию
        Vector3 finalPosition = transform.position;
        finalPosition.y = targetHeight;
        transform.position = finalPosition;
    }

    private IEnumerator DescendToLampHeight()
    {
        float startHeight = transform.position.y;
        float targetHeight;
        if (!IsHasBrokenLamp)
        {
            targetHeight = _lampHeight;
        }else
        {
            //height of drone-station
            targetHeight = 5f;
        }
        Vector3 targetPosition = new Vector3(TargetCoordinates.x, targetHeight, TargetCoordinates.z);
        _rb.isKinematic = true;


        float journeyLength = Mathf.Abs(startHeight - targetHeight);
        float startTime = Time.time;

        while (Mathf.Abs(transform.position.y - targetHeight) > _hoverHeightTolerance)
        {
            float distanceCovered = (Time.time - startTime) * _heightSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;

            // Используем Lerp для плавного спуска
            Vector3 newPosition = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime);

            // Сохраняем X и Z координаты (не меняем их во время спуска)
            newPosition.x = TargetCoordinates.x;
            newPosition.z = TargetCoordinates.z;

            transform.position = newPosition;

            yield return null;
        }

        // Фиксируем конечную позицию
        Vector3 finalPosition = transform.position;
        finalPosition.y = targetHeight;
        finalPosition.x = TargetCoordinates.x;
        finalPosition.z = TargetCoordinates.z;
        transform.position = finalPosition;
    }

    private IEnumerator FlyToTarget(Vector3 target)
    {
        StopHovering();

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = new Vector3(target.x, _reservedFlyHight, target.z);

        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float startTime = Time.time;

        while (Mathf.Abs(Vector3.Distance(transform.position, targetPosition)) > 0.5f)
        {
            float distanceCovered = (Time.time - startTime) * _flightSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;

            // Используем Lerp для плавного полета по прямой
            Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);

            // Сохраняем заданную высоту
            newPosition.y = _reservedFlyHight;

            transform.position = newPosition;

            yield return null;
        }
    }

    #endregion

    #region Hovering

    // Метод для зависания на текущей высоте
    private void StartHovering()
    {
        if (_isHovering) return;

        _isHovering = true;
        if (_hoverCoroutine != null)
            StopCoroutine(_hoverCoroutine);

        _hoverCoroutine = StartCoroutine(HoverAtCurrentHeight());
    }

    private void StopHovering()
    {
        _isHovering = false;
        _rb.isKinematic = false;
        if (_hoverCoroutine != null)
        {
            StopCoroutine(_hoverCoroutine);
            _hoverCoroutine = null;
        }
    }

    private IEnumerator HoverAtCurrentHeight()
    {
        float hoverHeight = transform.position.y;
        //float hoverStability = 0.1f; // Стабильность зависания (меньше значение = стабильнее)

        while (_isHovering && CurrentFlightState == FlightSubState.Hovering)
        {
            // Плавно корректируем высоту если отклонились
            if (Mathf.Abs(transform.position.y - hoverHeight) > _hoverHeightTolerance)
            {
                Vector3 currentPos = transform.position;
                currentPos.y = Mathf.Lerp(currentPos.y, hoverHeight, Time.deltaTime * _heightSpeed);
                transform.position = currentPos;
            }

            yield return null;
        }
    }

    // Метод для проверки, находится ли дрон в полете
    public bool IsFlying()
    {
        return CurrentDroneState == DronState.Flying;
    }

    // Метод для проверки, зависает ли дрон
    public bool IsHovering()
    {
        return _isHovering && CurrentFlightState == FlightSubState.Hovering;
    }

    #endregion



    #region Setters

    private void SetFlightState(FlightSubState newState)
    {
        if (CurrentFlightState != newState)
        {
            CurrentFlightState = newState;
            OnFlightStateChanged?.Invoke(ID, newState);

            //Останавливаем зависание при смене состояния
            if (newState != FlightSubState.Hovering)
            {
                StopHovering();
            }
        }
    }

    public void SetHeight(float requiredHeight)
    {
        _reservedFlyHight = requiredHeight;
    }

    public void SetTarget(Vector3 targetCoordinates, bool doRandom = true)
    {
        _isGoHome = (targetCoordinates == _stationCoordinates);

        float factorX = 0f;
        float factorZ = 0f;

        if (doRandom)
        {
            System.Random rand = new System.Random();
            //разброс в радиусе 3м от таргет точки, тк gps не точный
            factorX = rand.Next(-300, 301) / 100;
            factorZ = rand.Next(-300, 301) / 100;
        }

        TargetCoordinates = new Vector3(targetCoordinates.x + factorX,
                                         targetCoordinates.y,
                                         targetCoordinates.z + factorZ);
    }

    #endregion

    // Визуализация маршрута в редакторе
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            // Рисуем точку подъема
            Gizmos.color = Color.yellow;
            Vector3 liftPoint = new Vector3(transform.position.x, _reservedFlyHight, transform.position.z);
            Gizmos.DrawWireSphere(liftPoint, 0.3f);

            // Рисуем целевую точку на высоте полета
            Gizmos.color = Color.green;
            Vector3 targetPoint = new Vector3(TargetCoordinates.x, _reservedFlyHight, TargetCoordinates.z);
            Gizmos.DrawWireSphere(targetPoint, 0.5f);

            // Рисуем точку на высоте лампы
            Gizmos.color = Color.cyan;
            Vector3 lampPoint = new Vector3(TargetCoordinates.x, _lampHeight, TargetCoordinates.z);
            Gizmos.DrawWireSphere(lampPoint, 0.5f);

            // Рисуем линию маршрута
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, liftPoint);
            Gizmos.DrawLine(liftPoint, targetPoint);
            Gizmos.DrawLine(targetPoint, lampPoint);

            // Визуализация состояния полета
            string stateText = $"State: {CurrentFlightState}";
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2, stateText);
#endif
        }
    }
}