# WebSocket API

## Обзор

Сервер предоставляет три WebSocket эндпоинта для взаимодействия с различными типами клиентов:

| Эндпоинт | Тип клиента | Описание |
|----------|-------------|----------|
| `/pillar_station` | Pillar Station | Управление состоянием столбов освещения |
| `/dron_station` | Drone Station | Управление станцией дронов |
| `/frontend` | Frontend | Получение данных системы и уведомления об изменениях |

### Общая информация

- **Протокол:** WebSocket (ws:// или wss://)
- **Формат сообщений:** JSON
- **Кодировка:** UTF-8
- **Версия API:** 0.2.0

---

## 1. Pillar Station (`/pillar_station`)

Клиент для управления состоянием столбов освещения.

### Подключение

```
ws://server:port/pillar_station
```

### Авторизация

Первое сообщение **должно** быть событием `enter`:

```json
{
  "event": "enter",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000"
  }
}
```

| Поле | Тип | Описание |
|------|-----|----------|
| `event` | string | Всегда `"enter"` |
| `data.id` | UUID (string) | Идентификатор станции столбов (должен существовать в БД) |

#### Ответы сервера

**Успех:**
```json
{
  "event": "status",
  "status": "Ok",
  "message": ""
}
```

**Ошибка (неверный ID):**
```json
{
  "event": "status",
  "status": "Err",
  "message": "Ошибка в данных"
}
```

**Ошибка (первое сообщение не `enter`):**
```json
{
  "event": "status",
  "status": "Err",
  "message": "First message must be enter"
}
```
*Соединение закрывается с кодом 1003*

**Ошибка (невалидный JSON / отсутствуют поля):**
```json
{
  "event": "status",
  "status": "Err",
  "message": "You lost fields"
}
```

---

### Команды

#### change_lamp_state — Изменить состояние фонаря

Обновляет состояние столба в БД и уведомляет подключённую станцию дронов и frontend-клиентов.

**Запрос:**
```json
{
  "event": "change_lamp_state",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "status": "death"
  }
}
```

| Поле | Тип | Описание |
|------|-----|----------|
| `event` | string | Всегда `"change_lamp_state"` |
| `data.id` | UUID (string) | Идентификатор столба |
| `data.status` | string | Новое состояние: `"death"`, `"empty"`, `"alive"` |

**Ответ сервера:**
```json
{
  "event": "status",
  "status": "Ok",
  "message": ""
}
```

> **Примечание:** После изменения состояния на `"death"` или `"empty"` сервер отправляет уведомление станции дронов, за которой закреплён столб, и всем frontend-клиентам.

---

### События от сервера

#### set_mode_pillar — Уведомление о режиме обслуживания столба

Сервер отправляет это событие при изменении состояния столба, закреплённого за станцией.

**При переходе в `"death"` или `"empty"`:**
```json
{
  "event": "set_mode_pillar",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "is_hit": false,
    "is_service": true
  }
}
```

**При переходе в `"alive"`:**
```json
{
  "event": "set_mode_pillar",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "is_hit": false,
    "is_service": false
  }
}
```

| Поле | Тип | Описание |
|------|-----|----------|
| `event` | string | Всегда `"set_mode_pillar"` |
| `data.id` | UUID (string) | Идентификатор столба |
| `data.is_hit` | boolean | Флаг удара (всегда `false`) |
| `data.is_service` | boolean | Флаг сервисного режима (`true` для `"death"`/`"empty"`, `false` для `"alive"`) |

---

## 2. Drone Station (`/dron_station`)

Клиент для управления станцией дронов.

### Подключение

```
ws://server:port/dron_station
```

### Авторизация

Первое сообщение должно быть `register` (новая станция) или `enter` (существующая станция).

#### Вариант 1: Регистрация новой станции (register)

**Запрос:**
```json
{
  "event": "register",
  "data": {
    "coordinates": {
      "x": 55.751244,
      "y": 37.618423
    },
    "radius": 1000,
    "total_drone_count": 10,
    "total_lamps_count": 50
  }
}
```

| Поле | Тип | Описание |
|------|-----|----------|
| `event` | string | Всегда `"register"` |
| `data.coordinates` | object | Координаты станции |
| `data.coordinates.x` | float | Широта |
| `data.coordinates.y` | float | Долгота |
| `data.radius` | int | Радиус обслуживания (метров) |
| `data.total_drone_count` | int | Общее количество дронов |
| `data.total_lamps_count` | int | Общее количество фонарей |

**Ответ сервера:**
```json
{
  "event": "status",
  "status": "Ok",
  "message": "550e8400-e29b-41d4-a716-446655440000"
}
```
*В поле `message` возвращается UUID созданной станции*

**Дополнительно:** После успешной регистрации сервер автоматически:
1. Создаёт дронов в количестве `total_drone_count` (если их меньше, чем указано)
2. Закрепляет столбы в радиусе `radius` за станцией
3. Отправляет список созданных дронов:
```json
{
  "event": "status",
  "status": "Ok",
  "message": "[\"uuid-dron-1\", \"uuid-dron-2\", ...]"
}
```

#### Вариант 2: Вход существующей станции (enter)

**Запрос:**
```json
{
  "event": "enter",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000"
  }
}
```

| Поле | Тип | Описание |
|------|-----|----------|
| `event` | string | Всегда `"enter"` |
| `data.id` | UUID (string) | Идентификатор станции дронов |

**Ответ сервера:**
```json
{
  "event": "status",
  "status": "Ok",
  "message": ""
}
```

**Ошибка:**
```json
{
  "event": "status",
  "status": "Err",
  "message": "Ошибка в данных"
}
```

**Ошибка (первое сообщение не `register`/`enter`):**
```json
{
  "event": "status",
  "status": "Err",
  "message": "First message must be enter"
}
```
*Соединение закрывается с кодом 1003*

---

### События от сервера

#### get_drons — Список дронов станции

Отправляется сразу после успешной авторизации (`enter`/`register`).

```json
{
  "event": "get_drons",
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440010",
      "status": "in_station",
      "last_coordinates": null
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440011",
      "status": "fly",
      "last_coordinates": {
        "x": 55751244,
        "y": 37618423
      }
    }
  ]
}
```

| Поле | Тип | Описание |
|------|-----|----------|
| `event` | string | Всегда `"get_drons"` |
| `data` | array | Массив дронов |
| `data[].id` | UUID (string) | Идентификатор дрона |
| `data[].status` | string | Статус дрона |
| `data[].last_coordinates` | object \| null | Последние координаты |

#### get_pillars — Список столбов станции

Отправляется сразу после успешной авторизации (`enter`/`register`).

```json
{
  "event": "get_pillars",
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440001",
      "coordinates": {
        "x": 55751244,
        "y": 37618423
      },
      "state": "alive",
      "pillar_station_id": "550e8400-e29b-41d4-a716-446655440000",
      "id_dron_station": "550e8400-e29b-41d4-a716-446655440000",
      "last_update": "2026-02-23T12:00:00.000000"
    }
  ]
}
```

| Поле | Тип | Описание |
|------|-----|----------|
| `event` | string | Всегда `"get_pillars"` |
| `data` | array | Массив столбов |
| `data[].id` | UUID (string) | Идентификатор столба |
| `data[].coordinates` | object | Координаты столба |
| `data[].coordinates.x` | int | Широта |
| `data[].coordinates.y` | int | Долгота |
| `data[].state` | string | Состояние столба |
| `data[].pillar_station_id` | UUID (string) | ID станции столбов |
| `data[].id_dron_station` | UUID (string) | ID станции дронов |
| `data[].last_update` | string | ISO 8601 timestamp |

#### change_state_pillar — Уведомление об изменении состояния столба

Сервер отправляет это событие при изменении состояния столба, закреплённого за станцией.

```json
{
  "event": "change_state_pillar",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "status": "death"
  }
}
```

| Поле | Тип | Описание |
|------|-----|----------|
| `event` | string | Всегда `"change_state_pillar"` |
| `data.id` | UUID (string) | Идентификатор столба |
| `data.status` | string | Новое состояние: `"death"`, `"empty"` |

---

### Команды

#### change_dron_state — Изменить состояние дрона

Обновляет состояние дрона в БД и уведомляет frontend-клиентов.

**Запрос:**
```json
{
  "event": "change_dron_state",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440010",
    "status": "fly",
    "last_coordinates": {
      "x": 55751244,
      "y": 37618423
    }
  }
}
```

| Поле | Тип | Описание |
|------|-----|----------|
| `event` | string | Всегда `"change_dron_state"` |
| `data.id` | UUID (string) | Идентификатор дрона |
| `data.status` | string | Новое состояние: `"in_station"`, `"fly"`, `"broken"` |
| `data.last_coordinates` | object \| null | Последние координаты дрона |
| `data.last_coordinates.x` | int | Широта |
| `data.last_coordinates.y` | int | Долгота |

---

## 3. Frontend (`/frontend`)

Клиент для получения данных о состоянии всей системы и уведомлений об изменениях.

### Подключение

```
ws://server:port/frontend
```

### Начальные данные

При подключении сервер автоматически отправляет все данные системы:

```json
{
  "event": "all_data",
  "data": {
    "pillars": [...],
    "pillar_stations": [...],
    "dron_stations": [...]
  }
}
```

#### Структура данных

**Корневой объект:**

| Поле | Тип | Описание |
|------|-----|----------|
| `event` | string | Всегда `"all_data"` |
| `data` | object | Объект с данными системы |
| `data.pillars` | array | Массив всех столбов |
| `data.pillar_stations` | array | Массив всех станций столбов |
| `data.dron_stations` | array | Массив всех станций дронов |

**Структура столба (`pillar`):**

| Поле | Тип | Описание |
|------|-----|----------|
| `id` | UUID (string) | Идентификатор столба |
| `coordinates` | object | Координаты столба |
| `coordinates.x` | int | Широта |
| `coordinates.y` | int | Долгота |
| `state` | string | Состояние: `"empty"`, `"death"`, `"alive"` |
| `pillar_station_id` | UUID (string) | ID станции столбов |
| `last_update` | string | ISO 8601 timestamp |
| `id_dron_station` | UUID (string) \| null | ID станции дронов (если закреплён) |

**Структура станции столбов (`pillar_station`):**

| Поле | Тип | Описание |
|------|-----|----------|
| `id` | UUID (string) | Идентификатор станции |
| `coordinates` | object | Координаты станции |
| `coordinates.x` | int | Широта |
| `coordinates.y` | int | Долгота |
| `is_alive` | boolean | Статус активности |

**Структура станции дронов (`dron_station`):**

| Поле | Тип | Описание |
|------|-----|----------|
| `id` | UUID (string) | Идентификатор станции |
| `coordinates` | object | Координаты станции |
| `coordinates.x` | int | Широта |
| `coordinates.y` | int | Долгота |
| `radius` | int | Радиус обслуживания (метров) |
| `total_drone_count` | int | Общее количество дронов |
| `total_lamps_count` | int | Общее количество фонарей |
| `drons` | array | Массив дронов станции |

**Структура дрона (`dron`):**

| Поле | Тип | Описание |
|------|-----|----------|
| `id` | UUID (string) | Идентификатор дрона |
| `status` | string | Статус: `"in_station"`, `"fly"`, `"broken"` |
| `last_coordinates` | object \| null | Последние координаты |
| `last_coordinates.x` | int | Широта |
| `last_coordinates.y` | int | Долгота |

---

### События от сервера

#### change_state_pillar — Уведомление об изменении состояния столба

```json
{
  "event": "change_state_pillar",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "state": "death"
  }
}
```

| Поле | Тип | Описание |
|------|-----|----------|
| `event` | string | Всегда `"change_state_pillar"` |
| `data.id` | UUID (string) | Идентификатор столба |
| `data.state` | string | Новое состояние |

#### change_state_dron — Уведомление об изменении состояния дрона

```json
{
  "event": "change_state_dron",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440010",
    "state": "fly",
    "last_coordinates": {
      "x": 55751244,
      "y": 37618423
    }
  }
}
```

| Поле | Тип | Описание |
|------|-----|----------|
| `event` | string | Всегда `"change_state_dron"` |
| `data.id` | UUID (string) | Идентификатор дрона |
| `data.state` | string | Новое состояние |
| `data.last_coordinates` | object \| null | Последние координаты дрона |

---

### Команды

#### change_lamp_state — Изменить состояние фонаря

Аналогично команде в `/pillar_station`.

**Запрос:**
```json
{
  "event": "change_lamp_state",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440001",
    "status": "death"
  }
}
```

---

## Формат сообщений

### Общая структура запроса

Все запросы к серверу имеют единый формат:

```json
{
  "event": "<имя_события>",
  "data": { ... }
}
```

### Общая структура ответа

**Статус-сообщение:**
```json
{
  "event": "status",
  "status": "Ok" | "Err",
  "message": "<текст>"
}
```

**Сообщение с данными:**
```json
{
  "event": "<имя_события>",
  "data": { ... }
}
```

---

## Типы данных

### Состояния столба (`pillar_state`)

| Состояние | Описание |
|-----------|----------|
| `empty` | Свободен (не активен, доступен для закрепления) |
| `death` | Выключен/сломан (требует обслуживания) |
| `alive` | Активен (работает в штатном режиме) |

### Статусы дрона (`dron_status`)

| Статус | Описание |
|--------|----------|
| `in_station` | Дрон на станции (готов к вылету) |
| `fly` | Дрон в полёте (выполняет задачу) |
| `broken` | Дрон сломан (требует ремонта) |

---

## Коды закрытия WebSocket

| Код | Описание |
|-----|----------|
| 1003 | Ошибка формата данных (неверное первое сообщение, невалидный JSON) |

---

## Обработка ошибок

### Ошибки авторизации

| Ситуация | Ответ сервера | Действие |
|----------|---------------|----------|
| Первое сообщение не `enter`/`register` | `{"event": "status", "status": "Err", "message": "First message must be enter"}` | Соединение закрывается (код 1003) |
| Неверный UUID станции | `{"event": "status", "status": "Err", "message": "Ошибка в данных"}` | Соединение закрывается |
| Невалидный JSON | `{"event": "status", "status": "Err", "message": "You lost fields"}` | Соединение закрывается |

### Ошибки команд

| Ситуация | Ответ сервера |
|----------|---------------|
| Неверный формат команды | `{"event": "status", "status": "Err", "message": "You lost fields"}` |
| Несуществующий столб/дрон | Команда игнорируется (без ответа) |

---

## Примеры использования

### Python (клиент Pillar Station)

```python
import asyncio
import websockets
import json

async def pillar_client():
    async with websockets.connect("ws://localhost:8000/pillar_station") as ws:
        # Авторизация
        await ws.send(json.dumps({
            "event": "enter",
            "data": {
                "id": "550e8400-e29b-41d4-a716-446655440000"
            }
        }))
        response = await ws.recv()
        print(f"Ответ: {response}")

        # Изменение состояния фонаря
        await ws.send(json.dumps({
            "event": "change_lamp_state",
            "data": {
                "id": "550e8400-e29b-41d4-a716-446655440001",
                "status": "death"
            }
        }))
        response = await ws.recv()
        print(f"Ответ: {response}")

        # Ожидание уведомлений о режиме обслуживания
        while True:
            notification = await ws.recv()
            print(f"Уведомление: {notification}")

asyncio.run(pillar_client())
```

### Python (клиент Drone Station)

```python
import asyncio
import websockets
import json

async def drone_client():
    async with websockets.connect("ws://localhost:8000/dron_station") as ws:
        # Регистрация новой станции
        await ws.send(json.dumps({
            "event": "register",
            "data": {
                "coordinates": {"x": 55.751244, "y": 37.618423},
                "radius": 1000,
                "total_drone_count": 10,
                "total_lamps_count": 50
            }
        }))
        
        # Получение статуса регистрации (UUID станции)
        response = await ws.recv()
        print(f"Регистрация: {response}")
        
        # Получение списка созданных дронов
        response = await ws.recv()
        print(f"Дроны: {response}")
        
        # Получение списка столбов
        response = await ws.recv()
        print(f"Столбы: {response}")

        # Ожидание уведомлений об изменении состояния столбов
        while True:
            response = await ws.recv()
            print(f"Событие: {response}")

asyncio.run(drone_client())
```

### Python (клиент Frontend)

```python
import asyncio
import websockets
import json

async def frontend_client():
    async with websockets.connect("ws://localhost:8000/frontend") as ws:
        # Получение всех данных при подключении
        response = await ws.recv()
        data = json.loads(response)
        print(f"Событие: {data['event']}")

        # Доступ к данным
        pillars = data['data']['pillars']
        pillar_stations = data['data']['pillar_stations']
        dron_stations = data['data']['dron_stations']

        print(f"Всего столбов: {len(pillars)}")
        print(f"Всего станций столбов: {len(pillar_stations)}")
        print(f"Всего станций дронов: {len(dron_stations)}")

        # Ожидание уведомлений об изменениях
        while True:
            response = await ws.recv()
            update = json.loads(response)
            event = update.get('event')
            
            if event == 'change_state_pillar':
                print(f"Столб {update['data']['id']} изменил состояние на {update['data']['state']}")
            elif event == 'change_state_dron':
                print(f"Дрон {update['data']['id']} изменил состояние на {update['data']['state']}")

asyncio.run(frontend_client())
```

### JavaScript (клиент Frontend)

```javascript
const ws = new WebSocket('ws://localhost:8000/frontend');

ws.onopen = () => {
    console.log('Подключено к серверу');
};

ws.onmessage = (event) => {
    const data = JSON.parse(event.data);
    
    if (data.event === 'all_data') {
        console.log('Получены все данные:', data.data);
        // Инициализация UI
    } else if (data.event === 'change_state_pillar') {
        console.log(`Столб ${data.data.id} изменил состояние на ${data.data.state}`);
        // Обновление UI
    } else if (data.event === 'change_state_dron') {
        console.log(`Дрон ${data.data.id} изменил состояние на ${data.data.state}`);
        // Обновление UI
    }
};

ws.onerror = (error) => {
    console.error('Ошибка WebSocket:', error);
};

ws.onclose = () => {
    console.log('Соединение закрыто');
};
```

---

## Диаграмма последовательности

### Регистрация Drone Station

```
Drone Station          Server              Database
      |                   |                    |
      |--- register ----->|                    |
      |                   |--- insert_dron_station ->|
      |                   |<-------------------|
      |                   |--- create_drons -->|
      |                   |--- assign_pillars ->|
      |<-- status (UUID)--|                    |
      |<-- status (drons)-|                    |
      |<-- get_pillars ---|                    |
      |                   |                    |
```

### Изменение состояния столба

```
Pillar Station         Server              Database           Frontend
      |                   |                    |                  |
      |-- change_lamp -->|                    |                  |
      |                   |--- update_pillar ->|                  |
      |                   |<-------------------|                  |
      |                   |--------------------->| notify dron    |
      |                   |--------------------->| set_mode_pillar|
      |                   |------------------------------------->|
      |<-- status --------|                    |                  |
```
