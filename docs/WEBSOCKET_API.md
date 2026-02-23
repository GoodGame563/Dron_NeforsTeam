# WebSocket API

## Обзор

Сервер предоставляет три WebSocket эндпоинта для взаимодействия с различными типами клиентов:

| Эндпоинт | Тип клиента | Описание |
|----------|-------------|----------|
| `/pillar_station` | Pillar Station | Управление состоянием столбов освещения |
| `/dron_station` | Drone Station | Управление станцией дронов |
| `/frontend` | Frontend | Получение данных системы и уведомления об изменениях |

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
| `empty` | Свободен |
| `death` | Выключен/сломан |
| `alive` | Активен |

### Статусы дрона (`dron_status`)

| Статус | Описание |
|--------|----------|
| `in_station` | Дрон на станции |
| `fly` | Дрон в полёте |
| `broken` | Дрон сломан |

---

## Коды закрытия WebSocket

| Код | Описание |
|-----|----------|
| 1003 | Ошибка формата данных (неверное первое сообщение) |

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
        response = await ws.recv()
        print(f"Ответ: {response}")

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
            print(f"Обновление: {update}")

asyncio.run(frontend_client())
```
