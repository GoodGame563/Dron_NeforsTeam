# WebSocket API Документация

## Обзор

Сервер предоставляет два WebSocket эндпоинта для взаимодействия с различными типами клиентов:

| Эндпоинт | Тип клиента | Описание |
|----------|-------------|----------|
| `/pillar_station` | Pillar Station | Управление столбами освещения |
| `/dron_station` | Drone Station | Управление станцией дронов |

---

## 1. Pillar Station (`/pillar_station`)

Клиент для управления столбами освещения.

### Подключение

```
ws://server:port/pillar_station
```

### Процесс авторизации

Первое сообщение **должно** быть событием `enter`.

#### Запрос (enter)

```json
{
  "event": "enter",
  "client_id": "550e8400-e29b-41d4-a716-446655440000"
}
```

| Поле | Тип | Описание |
|------|-----|----------|
| `event` | string | Всегда `"enter"` |
| `client_id` | UUID (string) | Идентификатор станции столбов (должен существовать в БД) |

#### Ответ сервера

**Успех:**
```json
{
  "event": "status",
  "status": "Ok",
  "message": ""
}
```

**Ошибка (неверный client_id):**
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

### Доступные команды

#### lamp_off — Выключить фонарь

Отправляет команду на выключение фонаря столба.

**Запрос:**
```json
{
  "event": "lamp_off",
  "id_pillar": "550e8400-e29b-41d4-a716-446655440001"
}
```

| Поле | Тип | Описание |
|------|-----|----------|
| `event` | string | Всегда `"lamp_off"` |
| `id_pillar` | UUID (string) | Идентификатор столба |

**Ответ сервера:**
```json
{
  "event": "status",
  "status": "Ok",
  "message": ""
}
```

> **Примечание:** После выключения фонаря его состояние обновляется в БД на `"death"`.

---

## 2. Drone Station (`/dron_station`)

Клиент для управления станцией дронов.

### Подключение

```
ws://server:port/dron_station
```

### Процесс авторизации

Первое сообщение должно быть `register` (новая станция) или `enter` (существующая станция).

#### Вариант 1: Регистрация новой станции (register)

**Запрос:**
```json
{
  "event": "register",
  "coordinates": {
    "latitude": 55.751244,
    "longtiude": 37.618423
  },
  "radius": 1000,
  "total_drone_count": 10,
  "total_lamps_count": 50
}
```

| Поле | Тип | Описание |
|------|-----|----------|
| `event` | string | Всегда `"register"` |
| `coordinates` | object | Координаты станции |
| `coordinates.latitude` | float | Широта |
| `coordinates.longtiude` | float | Долгота |
| `radius` | int | Радиус обслуживания (метров) |
| `total_drone_count` | int | Общее количество дронов |
| `total_lamps_count` | int | Общее количество фонарей |

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
  "client_id": "550e8400-e29b-41d4-a716-446655440000"
}
```

| Поле | Тип | Описание |
|------|-----|----------|
| `event` | string | Всегда `"enter"` |
| `client_id` | UUID (string) | Идентификатор станции дронов |

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
  "message": "First message must be register or enter"
}
```
*Соединение закрывается с кодом 1003*

---

### Доступные команды

#### register_drons — Зарегистрировать дронов

Регистрирует недостающих дронов на станции (до `total_drone_count`).

**Запрос:**
```json
{
  "event": "register_drons"
}
```

**Ответ сервера:**
```json
{
  "event": "status",
  "status": "Ok",
  "message": "[\"550e8400-e29b-41d4-a716-446655440001\",\"550e8400-e29b-41d4-a716-446655440002\"]"
}
```
*В поле `message` возвращается JSON-массив с UUID новых дронов*

---

#### get_drons — Получить список дронов

Возвращает информацию о всех дронах станции.

**Запрос:**
```json
{
  "event": "get_drons"
}
```

**Ответ сервера:**
```json
{
  "event": "status",
  "status": "Ok",
  "message": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440001",
      "status": "in_station",
      "last_coordinates": {
        "latitude": 55.751244,
        "longtiude": 37.618423
      }
    },
    {
      "id": "550e8400-e29b-41d4-a716-446655440002",
      "status": "fly",
      "last_coordinates": null
    }
  ]
}
```

| Поле дрона | Тип | Описание |
|------------|-----|----------|
| `id` | UUID (string) | Идентификатор дрона |
| `status` | string | Статус дрона: `"in_station"`, `"fly"`, `"broken"` |
| `last_coordinates` | object\|null | Последние координаты |

---

#### get_pillars — Получить список столбов

Возвращает информацию о столбах, закреплённых за станцией дронов.

**Запрос:**
```json
{
  "event": "get_pillars"
}
```

**Ответ сервера:**
```json
{
  "event": "status",
  "status": "Ok",
  "message": [
    {
      "pillar_id": "550e8400-e29b-41d4-a716-446655440010",
      "x": 55.751244,
      "y": 37.618423,
      "state": "empty",
      "pillar_station_id": "550e8400-e29b-41d4-a716-446655440000"
    },
    {
      "pillar_id": "550e8400-e29b-41d4-a716-446655440011",
      "x": 55.755000,
      "y": 37.620000,
      "state": "death",
      "pillar_station_id": "550e8400-e29b-41d4-a716-446655440000"
    }
  ]
}
```

| Поле столба | Тип | Описание |
|-------------|-----|----------|
| `pillar_id` | UUID (string) | Идентификатор столба |
| `x` | float | Широта |
| `y` | float | Долгота |
| `state` | string | Состояние: `"empty"`, `"death"`, `"occupied"` |
| `pillar_station_id` | UUID (string) | ID станции столбов |

> **Примечание:** Перед возвратом списка вызывается процедура назначения столбов на станцию.

---

## Обработка ошибок

### Ошибки валидации

При неверном формате сообщения:

```json
{
  "event": "status",
  "status": "Err",
  "message": "You lost fields"
}
```

### Отключение клиента

При отключении клиента сервер выводит в лог:
```
Клиент столб станция/{client_id} отключился
```
или
```
Клиент дрон станция/{client_id} отключился
```

---

## Типы данных

### Статусы дрона

| Статус | Описание |
|--------|----------|
| `in_station` | Дрон на станции |
| `fly` | Дрон в полёте |
| `broken` | Дрон сломан |

### Состояния столба

| Состояние | Описание |
|-----------|----------|
| `empty` | Свободен |
| `death` | Выключен/сломан |
| `occupied` | Занят |

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
            "client_id": "550e8400-e29b-41d4-a716-446655440000"
        }))
        response = await ws.recv()
        print(f"Ответ: {response}")
        
        # Выключение фонаря
        await ws.send(json.dumps({
            "event": "lamp_off",
            "id_pillar": "550e8400-e29b-41d4-a716-446655440001"
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
            "coordinates": {
                "latitude": 55.751244,
                "longtiude": 37.618423
            },
            "radius": 1000,
            "total_drone_count": 10,
            "total_lamps_count": 50
        }))
        response = await ws.recv()
        print(f"Ответ: {response}")
        
        # Регистрация дронов
        await ws.send(json.dumps({"event": "register_drons"}))
        response = await ws.recv()
        print(f"Дроны: {response}")
        
        # Получение списка дронов
        await ws.send(json.dumps({"event": "get_drons"}))
        response = await ws.recv()
        print(f"Список дронов: {response}")
        
        # Получение списка столбов
        await ws.send(json.dumps({"event": "get_pillars"}))
        response = await ws.recv()
        print(f"Столбы: {response}")

asyncio.run(drone_client())
```

---

## Коды закрытия WebSocket

| Код | Описание |
|-----|----------|
| 1003 | Ошибка формата данных (неверное первое сообщение) |
