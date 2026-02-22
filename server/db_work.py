import asyncpg
import uuid


async def exists_pillar_station(conn: asyncpg.Connection, id_: uuid.UUID):
    return await conn.fetchval("SELECT exists_pillar_station($1)", id_)


async def get_pillar_station(conn: asyncpg.Connection, id_: uuid.UUID):
    row = await conn.fetchrow("SELECT * FROM get_pillar_station($1)", id_)
    return dict(row) if row else None


async def insert_pillar_station(
    conn: asyncpg.Connection, latitude: float, longitude: float, is_alive: bool = True
):
    return await conn.fetchval(
        "SELECT insert_pillar_station($1, $2, $3)", latitude, longitude, is_alive
    )


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


async def exists_dron_station(conn: asyncpg.Connection, id_: uuid.UUID):
    return await conn.fetchval("SELECT exists_dron_station($1)", id_)


async def get_dron_station(conn: asyncpg.Connection, id_: uuid.UUID):
    row = await conn.fetchrow("SELECT * FROM get_dron_station($1)", id_)
    return dict(row) if row else None


async def get_drons_by_station(conn: asyncpg.Connection, id_dron_station: uuid.UUID):
    rows = await conn.fetch("SELECT * FROM get_drons_by_station($1)", id_dron_station)
    return [dict(row) for row in rows]


async def insert_dron_station(
    conn: asyncpg.Connection,
    latitude: float,
    longitude: float,
    radius: int,
    total_drone_count: int,
    total_lamps_count: int,
):
    return await conn.fetchval(
        "SELECT insert_dron_station($1, $2, $3, $4, $5)",
        latitude,
        longitude,
        radius,
        total_drone_count,
        total_lamps_count,
    )


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


async def exists_pillar(conn: asyncpg.Connection, id_: uuid.UUID):
    return await conn.fetchval("SELECT exists_pillar($1)", id_)


async def get_pillar(conn: asyncpg.Connection, id_: uuid.UUID):
    row = await conn.fetchrow("SELECT * FROM get_pillar($1)", id_)
    return dict(row) if row else None


async def insert_pillar(
    conn: asyncpg.Connection,
    latitude: float,
    longitude: float,
    state: str = "empty",
    id_pillar_station: uuid.UUID | None = None,
):
    return await conn.fetchval(
        "SELECT insert_pillar($1, $2, $3::pillar_state, $4)",
        latitude,
        longitude,
        state,
        id_pillar_station,
    )


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

async def exists_pillar_to_dron_station(conn: asyncpg.Connection, id_: uuid.UUID):
    return await conn.fetchval("SELECT exists_pillar_to_dron_station($1)", id_)


async def get_pillar_to_dron_station(conn: asyncpg.Connection, id_: uuid.UUID):
    row = await conn.fetchrow("SELECT * FROM get_pillar_to_dron_station($1)", id_)
    return dict(row) if row else None


async def insert_pillar_to_dron_station(
    conn: asyncpg.Connection, id_pillar: uuid.UUID, id_dron_station: uuid.UUID
):
    return await conn.fetchval(
        "SELECT insert_pillar_to_dron_station($1, $2)", id_pillar, id_dron_station
    )


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
):
    await conn.execute("CALL assign_pillars_to_dron_station_flat($1)", dron_station_id)


async def get_pillars_for_dron_station(
    conn: asyncpg.Connection, dron_station_id: uuid.UUID
):
    rows = await conn.fetch(
        """
        SELECT * 
        FROM get_pillars_for_dron_station_flat($1)
        ORDER BY y DESC, x
        """,
        dron_station_id,
    )

    return [dict(row) for row in rows]

async def get_id_dron_station_by_pillar(
    conn: asyncpg.Connection, pillar_id: uuid.UUID
):
    row = await conn.fetchval("""SELECT id_dron_station
	FROM public.pillar_to_dron_station where id_pillar = $1 limit 1""", pillar_id)
    return row


async def exists_dron(conn: asyncpg.Connection, id_: uuid.UUID):
    return await conn.fetchval("SELECT exists_dron($1)", id_)


async def get_dron(conn: asyncpg.Connection, id_: uuid.UUID):
    row = await conn.fetchrow("SELECT * FROM get_dron($1)", id_)
    return dict(row) if row else None


async def insert_dron(
    conn: asyncpg.Connection,
    status: str = "in_station",
    last_latitude: float | None = None,
    last_longitude: float | None = None,
    id_dron_station: uuid.UUID | None = None,
):
    return await conn.fetchval(
        "SELECT insert_dron($1::dron_status, $2, $3, $4)",
        status,
        last_latitude,
        last_longitude,
        id_dron_station,
    )


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


async def exists_history(conn: asyncpg.Connection, id_: uuid.UUID):
    return await conn.fetchval("SELECT exists_history($1)", id_)


async def get_history(conn: asyncpg.Connection, id_: uuid.UUID):
    row = await conn.fetchrow("SELECT * FROM get_history($1)", id_)
    return dict(row) if row else None


async def insert_history(
    conn: asyncpg.Connection,
    id_dron_station: uuid.UUID,
    id_pillars: uuid.UUID,
    id_dron: uuid.UUID,
    status: str,
):
    return await conn.fetchval(
        "SELECT insert_history($1, $2, $3, $4::history_status)",
        id_dron_station,
        id_pillars,
        id_dron,
        status,
    )


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
