import asyncpg
import uuid
from .models import (
    PillarStation,
    DronStation,
    Pillar,
    PillarToDronStation,
    Dron,
    History,
    PillarForStation,
)


def _to_pillar_station(row) -> PillarStation:
    return PillarStation(
        id=row["id"],
        latitude=row["latitude"],
        longitude=row["longitude"],
        is_alive=row["is_alive"],
    )


def _to_dron_station(row) -> DronStation:
    return DronStation(
        id=row["id"],
        latitude=row["latitude"],
        longitude=row["longitude"],
        radius=row["radius"],
        total_drone_count=row["total_drone_count"],
        total_lamps_count=row["total_lamps_count"],
    )


def _to_pillar(row) -> Pillar:
    return Pillar(
        id=row["id"],
        latitude=row["latitude"],
        longitude=row["longitude"],
        state=row["state"],
        id_pillar_station=row["id_pillar_station"],
    )


def _to_pillar_to_dron_station(row) -> PillarToDronStation:
    return PillarToDronStation(
        id=row["id"],
        id_pillar=row["id_pillar"],
        id_dron_station=row["id_dron_station"],
    )


def _to_dron(row) -> Dron:
    return Dron(
        id=row["id"],
        status=row["status"],
        last_latitude=row["last_latitude"],
        last_longitude=row["last_longitude"],
        id_dron_station=row["id_dron_station"],
    )


def _to_history(row) -> History:
    return History(
        id=row["id"],
        id_dron_station=row["id_dron_station"],
        id_pillars=row["id_pillars"],
        id_dron=row["id_dron"],
        status=row["status"],
        update_at=row["update_at"],
    )


def _to_pillar_for_station(row) -> PillarForStation:
    return PillarForStation(
        pillar_id=row["pillar_id"],
        x=row["x"],
        y=row["y"],
        state=row["state"],
        id_pillar_station=row["id_pillar_station"],
    )


async def exists_pillar_station(conn: asyncpg.Connection, id_: uuid.UUID) -> bool:
    return await conn.fetchval("SELECT exists_pillar_station($1)", id_) or False


async def get_pillar_station(
    conn: asyncpg.Connection, id_: uuid.UUID
) -> PillarStation | None:
    row = await conn.fetchrow("SELECT * FROM get_pillar_station($1)", id_)
    return _to_pillar_station(row) if row else None


async def get_all_pillar_stations(conn: asyncpg.Connection) -> list[PillarStation]:
    rows = await conn.fetch("SELECT * FROM get_all_pillar_stations()")
    return [_to_pillar_station(row) for row in rows]


async def insert_pillar_station(
    conn: asyncpg.Connection, latitude: float, longitude: float, is_alive: bool = True
) -> uuid.UUID:
    return await conn.fetchval(
        "SELECT insert_pillar_station($1, $2, $3)", latitude, longitude, is_alive
    ) or uuid.UUID("00000000-0000-0000-0000-000000000000")


async def update_pillar_station(
    conn: asyncpg.Connection,
    id_: uuid.UUID,
    latitude: float,
    longitude: float,
    is_alive: bool,
) -> None:
    await conn.execute(
        "SELECT update_pillar_station($1, $2, $3, $4)",
        id_,
        latitude,
        longitude,
        is_alive,
    )


async def exists_dron_station(conn: asyncpg.Connection, id_: uuid.UUID) -> bool:
    return await conn.fetchval("SELECT exists_dron_station($1)", id_) or False


async def get_dron_station(
    conn: asyncpg.Connection, id_: uuid.UUID
) -> DronStation | None:
    row = await conn.fetchrow("SELECT * FROM get_dron_station($1)", id_)
    return _to_dron_station(row) if row else None


async def get_all_dron_stations(conn: asyncpg.Connection) -> list[DronStation]:
    rows = await conn.fetch("SELECT * FROM get_all_dron_stations()")
    return [_to_dron_station(row) for row in rows]


async def get_drons_by_station(
    conn: asyncpg.Connection, id_dron_station: uuid.UUID
) -> list[Dron]:
    rows = await conn.fetch("SELECT * FROM get_drons_by_station($1)", id_dron_station)
    return [_to_dron(row) for row in rows]


async def insert_dron_station(
    conn: asyncpg.Connection,
    latitude: float,
    longitude: float,
    radius: int,
    total_drone_count: int,
    total_lamps_count: int,
) -> uuid.UUID:
    return await conn.fetchval(
        "SELECT insert_dron_station($1, $2, $3, $4, $5)",
        latitude,
        longitude,
        radius,
        total_drone_count,
        total_lamps_count,
    ) or uuid.UUID("00000000-0000-0000-0000-000000000000")


async def update_dron_station(
    conn: asyncpg.Connection,
    id_: uuid.UUID,
    latitude: float,
    longitude: float,
    radius: int,
    total_drone_count: int,
    total_lamps_count: int,
) -> None:
    await conn.execute(
        "SELECT update_dron_station($1, $2, $3, $4, $5, $6)",
        id_,
        latitude,
        longitude,
        radius,
        total_drone_count,
        total_lamps_count,
    )


async def exists_pillar(conn: asyncpg.Connection, id_: uuid.UUID) -> bool:
    return await conn.fetchval("SELECT exists_pillar($1)", id_) or False


async def get_pillar(conn: asyncpg.Connection, id_: uuid.UUID) -> Pillar | None:
    row = await conn.fetchrow("SELECT * FROM get_pillar($1)", id_)
    return _to_pillar(row) if row else None


async def get_all_pillars(conn: asyncpg.Connection) -> list[Pillar]:
    rows = await conn.fetch("SELECT * FROM get_all_pillars()")
    return [_to_pillar(row) for row in rows]


async def insert_pillar(
    conn: asyncpg.Connection,
    latitude: float,
    longitude: float,
    state: str = "empty",
    id_pillar_station: uuid.UUID | None = None,
) -> uuid.UUID:
    return await conn.fetchval(
        "SELECT insert_pillar($1, $2, $3::pillar_state, $4)",
        latitude,
        longitude,
        state,
        id_pillar_station,
    ) or uuid.UUID("00000000-0000-0000-0000-000000000000")


async def update_pillar(
    conn: asyncpg.Connection,
    id_: uuid.UUID,
    latitude: float,
    longitude: float,
    state: str,
    id_pillar_station: uuid.UUID,
) -> None:
    await conn.execute(
        "SELECT update_pillar($1, $2, $3, $4::pillar_state, $5)",
        id_,
        latitude,
        longitude,
        state,
        id_pillar_station,
    )


async def update_pillar_state(
    conn: asyncpg.Connection,
    id_: uuid.UUID,
    state: str,
) -> None:
    await conn.execute(
        "SELECT update_pillar_state($1, $2::pillar_state)",
        id_,
        state,
    )


async def exists_pillar_to_dron_station(
    conn: asyncpg.Connection, id_: uuid.UUID
) -> bool:
    return await conn.fetchval("SELECT exists_pillar_to_dron_station($1)", id_) or False


async def get_pillar_to_dron_station(
    conn: asyncpg.Connection, id_: uuid.UUID
) -> PillarToDronStation | None:
    row = await conn.fetchrow("SELECT * FROM get_pillar_to_dron_station($1)", id_)
    return _to_pillar_to_dron_station(row) if row else None


async def insert_pillar_to_dron_station(
    conn: asyncpg.Connection, id_pillar: uuid.UUID, id_dron_station: uuid.UUID
) -> uuid.UUID:
    return await conn.fetchval(
        "SELECT insert_pillar_to_dron_station($1, $2)", id_pillar, id_dron_station
    ) or uuid.UUID("00000000-0000-0000-0000-000000000000")


async def update_pillar_to_dron_station(
    conn: asyncpg.Connection,
    id_: uuid.UUID,
    id_pillar: uuid.UUID,
    id_dron_station: uuid.UUID,
) -> None:
    await conn.execute(
        "SELECT update_pillar_to_dron_station($1, $2, $3)",
        id_,
        id_pillar,
        id_dron_station,
    )


async def assign_pillars_to_dron_station_and_get_count(
    conn: asyncpg.Connection, dron_station_id: uuid.UUID
) -> None:
    await conn.execute("CALL assign_pillars_to_dron_station_flat($1)", dron_station_id)


async def get_pillars_for_dron_station(
    conn: asyncpg.Connection, dron_station_id: uuid.UUID
) -> list[PillarForStation]:
    rows = await conn.fetch(
        """
        SELECT *
        FROM get_pillars_for_dron_station_flat($1)
        ORDER BY y DESC, x
        """,
        dron_station_id,
    )
    return [_to_pillar_for_station(row) for row in rows]


async def get_id_dron_station_by_pillar(
    conn: asyncpg.Connection, pillar_id: uuid.UUID
) -> uuid.UUID | None:
    row = await conn.fetchval(
        """SELECT id_dron_station
	FROM public.pillar_to_dron_station where id_pillar = $1 limit 1""",
        pillar_id,
    )
    return row


async def exists_dron(conn: asyncpg.Connection, id_: uuid.UUID) -> bool:
    return await conn.fetchval("SELECT exists_dron($1)", id_) or False


async def get_dron(conn: asyncpg.Connection, id_: uuid.UUID) -> Dron | None:
    row = await conn.fetchrow("SELECT * FROM get_dron($1)", id_)
    return _to_dron(row) if row else None


async def insert_dron(
    conn: asyncpg.Connection,
    status: str = "in_station",
    last_latitude: float | None = None,
    last_longitude: float | None = None,
    id_dron_station: uuid.UUID | None = None,
) -> uuid.UUID:
    return await conn.fetchval(
        "SELECT insert_dron($1::dron_status, $2, $3, $4)",
        status,
        last_latitude,
        last_longitude,
        id_dron_station,
    ) or uuid.UUID("00000000-0000-0000-0000-000000000000")


async def update_dron(
    conn: asyncpg.Connection,
    id_: uuid.UUID,
    status: str,
    last_latitude: float,
    last_longitude: float,
    id_dron_station: uuid.UUID,
) -> None:
    await conn.execute(
        "SELECT update_dron($1, $2::dron_status, $3, $4, $5)",
        id_,
        status,
        last_latitude,
        last_longitude,
        id_dron_station,
    )


async def exists_history(conn: asyncpg.Connection, id_: uuid.UUID) -> bool:
    return await conn.fetchval("SELECT exists_history($1)", id_) or False


async def get_history(conn: asyncpg.Connection, id_: uuid.UUID) -> History | None:
    row = await conn.fetchrow("SELECT * FROM get_history($1)", id_)
    return _to_history(row) if row else None


async def insert_history(
    conn: asyncpg.Connection,
    id_dron_station: uuid.UUID,
    id_pillars: uuid.UUID,
    id_dron: uuid.UUID,
    status: str,
) -> uuid.UUID:
    return await conn.fetchval(
        "SELECT insert_history($1, $2, $3, $4::history_status)",
        id_dron_station,
        id_pillars,
        id_dron,
        status,
    ) or uuid.UUID("00000000-0000-0000-0000-000000000000")


async def update_history(
    conn: asyncpg.Connection,
    id_: uuid.UUID,
    id_dron_station: uuid.UUID,
    id_pillars: uuid.UUID,
    id_dron: uuid.UUID,
    status: str,
) -> None:
    await conn.execute(
        "SELECT update_history($1, $2, $3, $4, $5::history_status)",
        id_,
        id_dron_station,
        id_pillars,
        id_dron,
        status,
    )
