using UnityEngine;
using System;
using System.Collections;

public class Drone : MonoBehaviour
{
    public enum DronState
    {
        Ready,
        Flying,
        Broken,
        GoChangeLamp,
        HaveWorkingLamp
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

    private Vector3 _horizontalVelocity;
    private float _verticalVelocity;

    float _rotationSpeed = 120f;
    private Vector3 _descendRotationVelocity;

    private Coroutine _flightCoroutine;
    private Coroutine _hoverCoroutine;
    private Coroutine _moveToAnimPosCoroutine;

    private bool _isHovering = false;
    private bool _isGoHome;
    public bool IsHasBrokenLamp { get; private set; }

    private Animator _anim;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        _anim = GetComponent<Animator>();
    }

    public void Initialize(string newId, Vector3 stationCoordinates)
    {
        _stationCoordinates = stationCoordinates;
        
        ID = newId;

        CurrentDroneState = DronState.Ready;
        CurrentFlightState = FlightSubState.Hovering; // Начальное состояние - зависание на земле

        Debug.Log($"Drone {ID} Initialized! _stationCoordinates = {_stationCoordinates}");
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
        _isGoHome = true;
        Vector3 stationPos = new(_stationCoordinates.x, _stationCoordinates.y, _stationCoordinates.z);
        CurrentDroneState = DronState.Ready;
        SetTarget(stationPos, false);
        Launch();
        Debug.LogWarning($"Drone ({ID}) go home");
        _anim.applyRootMotion = true;
    }

    public void GoChangeLamp()
    {
        _isGoHome = true;
        Vector3 stationPos = new(_stationCoordinates.x, _stationCoordinates.y, _stationCoordinates.z);
        CurrentDroneState = DronState.GoChangeLamp;
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
            Debug.Log($"CurrentLampCoord {CurrentLampCoord}");
        }
        _anim.applyRootMotion = false;
    }

    public void ChangeLampInStation()
    {
        _anim.SetTrigger("ChangeLamp");

        IsHasBrokenLamp = false;
    }

    public void OnChangeToWorkingLampAnimEnded()
    {
        CurrentDroneState = DronState.HaveWorkingLamp;
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
        //CurrentDroneState = DronState.Flying;

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

        //// Шаг 3: Спускаемся на высоту лампы (_lampHeight)
        //SetFlightState(FlightSubState.Descending);
        //Debug.Log($"Дрон {ID}: Спускаемся на высоту лампы {_lampHeight}");
        //StopHovering();
        //yield return StartCoroutine(DescendToLampHeight());

        SetFlightState(FlightSubState.Descending);
        StopHovering();

        // Шаг 4: Зависаем на высоте лампы
        SetFlightState(FlightSubState.Hovering);
        Debug.Log($"Дрон {ID}: Зависаем на высоте лампы в точке назначения");
        //StartHovering();

        Debug.Log($"Дрон {ID}: Достиг цели и находится на высоте лампы!");
        //CurrentDroneState = DronState.Charging;
        //SetFlightState(FlightSubState.Hovering);
        
        if (_isGoHome)
        {
            if (CurrentDroneState == DronState.GoChangeLamp)
            {
                OnDroneNeedChangingLamp?.Invoke(ID);
                CurrentDroneState = DronState.HaveWorkingLamp;
                Debug.Log($"Drone need to change lamp");
            }
            else if(CurrentDroneState == DronState.HaveWorkingLamp)
            {
                SetTarget(CurrentLampCoord, false);
                Launch();
                CurrentDroneState = DronState.Flying;
            }
            else
            {
                CurrentDroneState = DronState.Ready;
                OnDroneReady?.Invoke(ID);
                Debug.Log($"Drone landed to station and now ready");
                //здесь он должен начать ждать след задания
            }
        }
        else
        {
            OnDroneReachedTarget?.Invoke(ID);
        }
    }

    #region Fly process

    public void MoveToStartAnimPos(Transform targetTransform, float moveSpeed = 5f, float rotationSpeed = 120f)
    {
        if (_moveToAnimPosCoroutine != null)
            StopCoroutine(_moveToAnimPosCoroutine);

        _moveToAnimPosCoroutine = StartCoroutine(MoveToStartAnimPosCoroutine(targetTransform, moveSpeed, rotationSpeed));
    }

    private IEnumerator MoveToStartAnimPosCoroutine(Transform targetTransform, float moveSpeed, float rotationSpeed)
    {
        Vector3 velocity = Vector3.zero;

        while (Vector3.Distance(transform.position, targetTransform.position) > 0.05f)
        {
            // Плавное перемещение
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetTransform.position,
                ref velocity,
                0.6f,
                moveSpeed
            );

            // Плавный поворот только по Y
            Vector3 direction = Vector3.zero;
            direction.y = 0; // игнорируем вертикаль
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.identity;
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            yield return null;
        }

        // Точная позиция и поворот в конце
        transform.position = targetTransform.position;
        Vector3 finalDir = (targetTransform.position - transform.position).normalized;
        finalDir.y = 0;
        if (finalDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.identity;

        _moveToAnimPosCoroutine = null;
        OnDroneReachedTarget?.Invoke(ID);
    }

    private IEnumerator LiftToHeight(float targetHeight)
    {
        StopHovering();

        while (Mathf.Abs(transform.position.y - targetHeight) > _hoverHeightTolerance)
        {
            float y = Mathf.SmoothDamp(
                transform.position.y,
                targetHeight,
                ref _verticalVelocity,
                0.6f,
                _heightSpeed
            );

            transform.position = new Vector3(
                transform.position.x,
                y,
                transform.position.z
            );

            yield return null;
        }

        transform.position = new Vector3(
            transform.position.x,
            targetHeight,
            transform.position.z
        );
    }

    private IEnumerator DescendToLampHeight()
    {
        StopHovering();

        float targetHeight = !IsHasBrokenLamp ? _lampHeight : 5f;

        Vector3 targetPosition = new Vector3(
            TargetCoordinates.x,
            targetHeight,
            TargetCoordinates.z
        );

        while (Vector3.Distance(transform.position, targetPosition) > _stopDistance)
        {
            // 1️⃣ Плавный спуск
            float y = Mathf.SmoothDamp(
                transform.position.y,
                targetHeight,
                ref _verticalVelocity,
                0.6f,
                _heightSpeed
            );

            transform.position = new Vector3(
                TargetCoordinates.x,
                y,
                TargetCoordinates.z
            );

            // 2️⃣ Поворот только по Y
            Vector3 lookDir = (TargetCoordinates - transform.position).normalized;
            lookDir.y = 0; // игнорируем вертикаль
            if (lookDir.sqrMagnitude > 0.001f)
            {
                    Quaternion targetRotation = Quaternion.Euler(Vector3.zero);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    _rotationSpeed * Time.deltaTime
                );
            }

            yield return null;
        }


        // 3️⃣ Финальная точка и поворот
        transform.position = targetPosition;
        Vector3 finalDir = (TargetCoordinates - transform.position).normalized;
        finalDir.y = 0;
        if (finalDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    private IEnumerator FlyToTarget(Vector3 target)
    {
        StopHovering();



        Vector3 targetXZ = new Vector3(
            target.x,
            transform.position.y, // фиксированная высота
            target.z
        );

        while (Vector2.Distance(
                   new Vector2(transform.position.x, transform.position.z),
                   new Vector2(targetXZ.x, targetXZ.z)) > _stopDistance)
        {
            // 1️⃣ Плавное перемещение XZ
            Vector3 newXZ = Vector3.SmoothDamp(
                new Vector3(transform.position.x, 0, transform.position.z),
                new Vector3(targetXZ.x, 0, targetXZ.z),
                ref _horizontalVelocity,
                0.8f,
                _flightSpeed
            );

            transform.position = new Vector3(
                newXZ.x,
                _reservedFlyHight,
                newXZ.z
            );

            // 2️⃣ Поворот только по оси Y
            Vector3 direction = (TargetCoordinates - transform.position).normalized;
            direction.y = 0; // игнорируем Y
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    _rotationSpeed * Time.deltaTime
                );
            }

            yield return null;
        }

        // 3️⃣ Финальная точка и точный поворот
        transform.position = new Vector3(
            target.x,
            _reservedFlyHight,
            target.z
        );

        Vector3 finalDir = (TargetCoordinates - transform.position).normalized;
        finalDir.y = 0;
        if (finalDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(finalDir);
        OnDroneReachedTarget?.Invoke(ID);
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
        //_rb.isKinematic = false;
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
        if (targetCoordinates != _stationCoordinates)
        {
            CurrentLampCoord = targetCoordinates;


            Debug.Log($"CurrentLampCoord {CurrentLampCoord}");
        }

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