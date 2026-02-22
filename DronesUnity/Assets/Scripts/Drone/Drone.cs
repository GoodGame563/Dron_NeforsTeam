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
        Landing         // Посадка
    }

    public DronState CurrentDroneState { get; private set; }
    public FlightSubState CurrentFlightState { get; private set; }
    public DroneLampHolderState CurrentLampHolderState { get; private set; }

    public int ID { get; private set; }

    public float BatteryChargeLevel { get; private set; }

    private Rigidbody _rb;

    private Vector3 _targetCoordinates;
    private Vector3 _stationCoordinates;

    private float _requiredHeight;

    [Header("Настройки полета")]
    [SerializeField] private float _flightSpeed = 5f; // Скорость полета
    [SerializeField] private float _heightSpeed = 5f; // Скорость набора высоты
    [SerializeField] private float _stopDistance = 0.1f; // Дистанция для остановки
    [SerializeField] private float _hoverHeightTolerance = 0.5f; // Допуск по высоте для зависания

    public event Action<int> OnDroneChargetEnought;
    public event Action<int> OnDroneReachedTarget; // Событие достижения цели
    public event Action<int> OnDroneAtHome; // Событие достижения цели
    public event Action<int, FlightSubState> OnFlightStateChanged; // Событие изменения состояния полета

    private Coroutine _flightCoroutine;
    private Coroutine _hoverCoroutine;
    private bool _isHovering = false;
    private bool _isGoHome;

    public void Initialize(int newId, Vector3 stationCoordinates)
    {
        _stationCoordinates = stationCoordinates;
        ID = newId;
        _rb = GetComponent<Rigidbody>();
        CurrentDroneState = DronState.Charging;
        CurrentFlightState = FlightSubState.Hovering; // Начальное состояние - зависание на земле
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
        SetTarget(_stationCoordinates);
        Launch();
        Debug.LogWarning($"Drone ({ID}) go home");
    }

    private IEnumerator FlySequence()
    {
        CurrentDroneState = DronState.Flying;

        // Шаг 1: Поднимаемся на заданную высоту
        SetFlightState(FlightSubState.TakingOff);
        Debug.Log($"Дрон {ID}: Начинаем подъем на высоту {_requiredHeight}");
        yield return StartCoroutine(LiftToHeight(_requiredHeight));

        // Зависаем на достигнутой высоте
        SetFlightState(FlightSubState.Hovering);
        Debug.Log($"Дрон {ID}: Зависаем на высоте {_requiredHeight}");
        StartHovering();

        // Шаг 2: Летим к целевой точке на заданной высоте
        SetFlightState(FlightSubState.Moving);
        Debug.Log($"Дрон {ID}: Летим к цели {_targetCoordinates}");
        yield return StartCoroutine(FlyToTarget(_targetCoordinates));

        // Зависаем в целевой точке
        SetFlightState(FlightSubState.Hovering);
        Debug.Log($"Дрон {ID}: Зависаем в точке назначения");
        StartHovering();

        // Шаг 3: Останавливаемся в целевой точке (плавное торможение)
        SetFlightState(FlightSubState.Landing);
        Debug.Log($"Дрон {ID}: Останавливаемся в точке назначения");
        yield return StartCoroutine(StopAtTarget());

        Debug.Log($"Дрон {ID}: Достиг цели!");
        CurrentDroneState = DronState.Charging;
        SetFlightState(FlightSubState.Hovering);

        if (_isGoHome)
        {
            OnDroneAtHome?.Invoke(ID);
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

    private IEnumerator FlyToTarget(Vector3 target)
    {
        StopHovering();

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = new Vector3(target.x, _requiredHeight, target.z);

        float journeyLength = Vector3.Distance(startPosition, targetPosition);
        float startTime = Time.time;

        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            float distanceCovered = (Time.time - startTime) * _flightSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;

            // Используем Lerp для плавного полета по прямой
            Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, fractionOfJourney);

            // Сохраняем заданную высоту
            newPosition.y = _requiredHeight;

            transform.position = newPosition;

            yield return null;
        }
    }

    private IEnumerator StopAtTarget()
    {
        StopHovering();

        Vector3 currentPosition = transform.position;
        float stopDuration = 1f; // Длительность торможения
        float elapsedTime = 0f;

        while (elapsedTime < stopDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / stopDuration;

            // Плавная остановка с использованием SmoothStep для более естественного торможения
            float smoothT = Mathf.SmoothStep(0, 1, t);
            transform.position = Vector3.Lerp(currentPosition, transform.position, smoothT);

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
        if (_hoverCoroutine != null)
        {
            StopCoroutine(_hoverCoroutine);
            _hoverCoroutine = null;
        }
    }

    private IEnumerator HoverAtCurrentHeight()
    {
        float hoverHeight = transform.position.y;
        float hoverStability = 0.1f; // Стабильность зависания (меньше значение = стабильнее)

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

    private void SetFlightState(FlightSubState newState)
    {
        if (CurrentFlightState != newState)
        {
            CurrentFlightState = newState;
            OnFlightStateChanged?.Invoke(ID, newState);

            // Останавливаем зависание при смене состояния
            if (newState != FlightSubState.Hovering)
            {
                StopHovering();
            }
        }
    }

    // Альтернативный метод остановки через Rigidbody если нужно
    private IEnumerator StopWithRigidbody()
    {
        if (_rb != null)
        {
            float stopTime = 1f;
            float elapsedTime = 0f;
            Vector3 initialVelocity = _rb.linearVelocity;

            while (elapsedTime < stopTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / stopTime;

                // Плавно уменьшаем скорость
                _rb.linearVelocity = Vector3.Lerp(initialVelocity, Vector3.zero, t);

                yield return null;
            }

            _rb.linearVelocity = Vector3.zero;
        }
    }

    #region Setters

    public void SetHeight(float requiredHeight)
    {
        _requiredHeight = requiredHeight;
    }

    public void SetTarget(Vector3 targetCoordinates)
    {
        _isGoHome = (targetCoordinates == _stationCoordinates);

        System.Random rand = new System.Random();
        //разброс в радиусе 3м от таргет точки, тк gps не точный
        float factorX = rand.Next(-300, 301) / 100;
        float factorZ = rand.Next(-300, 301) / 100;

        _targetCoordinates = new Vector3(targetCoordinates.x + factorX, 
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
            Vector3 liftPoint = new Vector3(transform.position.x, _requiredHeight, transform.position.z);
            Gizmos.DrawWireSphere(liftPoint, 0.3f);

            // Рисуем целевую точку
            Gizmos.color = Color.green;
            Vector3 targetPoint = new Vector3(_targetCoordinates.x, _requiredHeight, _targetCoordinates.z);
            Gizmos.DrawWireSphere(targetPoint, 0.5f);

            // Рисуем линию маршрута
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, liftPoint);
            Gizmos.DrawLine(liftPoint, targetPoint);

            // Визуализация состояния полета
            string stateText = $"State: {CurrentFlightState}";
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2, stateText);
#endif
        }
    }
}