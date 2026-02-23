CREATE EXTENSION IF NOT EXISTS "uuid-ossp";


CREATE TYPE pillar_state AS ENUM ('alive', 'death', 'empty');
CREATE TYPE dron_status AS ENUM ('in_station', 'fly', 'broken');
CREATE TYPE history_status AS ENUM ('broken', 'clear', 'work', 'done');



CREATE TABLE IF NOT EXISTS pillar_stations (
    id         UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    latitude   DOUBLE PRECISION NOT NULL,
    longitude  DOUBLE PRECISION NOT NULL,
    is_alive   BOOLEAN NOT NULL DEFAULT true
);

CREATE TABLE IF NOT EXISTS dron_stations (
    id                 UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    latitude           DOUBLE PRECISION NOT NULL,
    longitude          DOUBLE PRECISION NOT NULL,
    radius             INTEGER NOT NULL,
    total_drone_count  INTEGER NOT NULL,
    total_lamps_count  INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS pillars (
    id                UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    latitude          DOUBLE PRECISION NOT NULL,
    longitude         DOUBLE PRECISION NOT NULL,
    state             pillar_state NOT NULL DEFAULT 'empty',
    id_pillar_station UUID NOT NULL
);

CREATE TABLE IF NOT EXISTS pillar_to_dron_station (
    id               UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    id_pillar        UUID NOT NULL,
    id_dron_station  UUID NOT NULL,

    CONSTRAINT fk_pillar 
        FOREIGN KEY (id_pillar) REFERENCES pillars(id) ON DELETE CASCADE,
    CONSTRAINT fk_dron_station 
        FOREIGN KEY (id_dron_station) REFERENCES dron_stations(id) ON DELETE CASCADE,

    UNIQUE(id_pillar, id_dron_station)
);

CREATE TABLE IF NOT EXISTS drons (
    id               UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    status           dron_status NOT NULL DEFAULT 'in_station',
    last_latitude    DOUBLE PRECISION,
    last_longitude   DOUBLE PRECISION,
    id_dron_station  UUID,

    CONSTRAINT fk_dron_station 
        FOREIGN KEY (id_dron_station) REFERENCES dron_stations(id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS history (
    id               UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    id_dron_station  UUID NOT NULL,
    id_pillars       UUID NOT NULL,
    id_dron          UUID NOT NULL,
    status           history_status NOT NULL,
    update_at        TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_pillars_station ON pillars(id_pillar_station);
CREATE INDEX IF NOT EXISTS idx_drons_station ON drons(id_dron_station);
CREATE INDEX IF NOT EXISTS idx_history_dron ON history(id_dron);
CREATE INDEX IF NOT EXISTS idx_history_update ON history(update_at DESC);


CREATE OR REPLACE FUNCTION exists_pillar_station(p_id UUID) RETURNS BOOLEAN AS $$
BEGIN
    RETURN EXISTS (SELECT 1 FROM pillar_stations WHERE id = p_id);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_pillar_station(p_id UUID) RETURNS SETOF pillar_stations AS $$
BEGIN
    RETURN QUERY SELECT * FROM pillar_stations WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_all_pillar_stations() RETURNS SETOF pillar_stations AS $$
BEGIN
    RETURN QUERY SELECT * FROM pillar_stations;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION insert_pillar_station(
    p_latitude DOUBLE PRECISION, 
    p_longitude DOUBLE PRECISION, 
    p_is_alive BOOLEAN DEFAULT true
) RETURNS UUID AS $$
DECLARE
    new_id UUID;
BEGIN
    INSERT INTO pillar_stations (latitude, longitude, is_alive) 
    VALUES (p_latitude, p_longitude, p_is_alive) 
    RETURNING id INTO new_id;
    RETURN new_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_pillar_station(
    p_id UUID, 
    p_latitude DOUBLE PRECISION, 
    p_longitude DOUBLE PRECISION, 
    p_is_alive BOOLEAN
) RETURNS VOID AS $$
BEGIN
    UPDATE pillar_stations 
    SET latitude = p_latitude, longitude = p_longitude, is_alive = p_is_alive 
    WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION exists_dron_station(p_id UUID) RETURNS BOOLEAN AS $$
BEGIN
    RETURN EXISTS (SELECT 1 FROM dron_stations WHERE id = p_id);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_dron_station(p_id UUID) RETURNS SETOF dron_stations AS $$
BEGIN
    RETURN QUERY SELECT * FROM dron_stations WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_all_dron_stations() RETURNS SETOF dron_stations AS $$
BEGIN
    RETURN QUERY SELECT * FROM dron_stations;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION insert_dron_station(
    p_latitude DOUBLE PRECISION, 
    p_longitude DOUBLE PRECISION, 
    p_radius INTEGER, 
    p_total_drone_count INTEGER, 
    p_total_lamps_count INTEGER
) RETURNS UUID AS $$
DECLARE
    new_id UUID;
BEGIN
    INSERT INTO dron_stations (latitude, longitude, radius, total_drone_count, total_lamps_count) 
    VALUES (p_latitude, p_longitude, p_radius, p_total_drone_count, p_total_lamps_count) 
    RETURNING id INTO new_id;
    RETURN new_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_dron_station(
    p_id UUID, 
    p_latitude DOUBLE PRECISION, 
    p_longitude DOUBLE PRECISION, 
    p_radius INTEGER, 
    p_total_drone_count INTEGER, 
    p_total_lamps_count INTEGER
) RETURNS VOID AS $$
BEGIN
    UPDATE dron_stations 
    SET latitude = p_latitude, longitude = p_longitude, radius = p_radius, 
        total_drone_count = p_total_drone_count, total_lamps_count = p_total_lamps_count 
    WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_drons_by_station(p_id_dron_station UUID) RETURNS SETOF drons AS $$
BEGIN
    RETURN QUERY SELECT * FROM drons WHERE id_dron_station = p_id_dron_station;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION exists_pillar(p_id UUID) RETURNS BOOLEAN AS $$
BEGIN
    RETURN EXISTS (SELECT 1 FROM pillars WHERE id = p_id);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_pillar(p_id UUID) RETURNS SETOF pillars AS $$
BEGIN
    RETURN QUERY SELECT * FROM pillars WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_all_pillars() RETURNS SETOF pillars AS $$
BEGIN
    RETURN QUERY SELECT * FROM pillars;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION insert_pillar(
    p_latitude DOUBLE PRECISION, 
    p_longitude DOUBLE PRECISION, 
    p_id_pillar_station UUID,
    p_state pillar_state DEFAULT 'empty'
) RETURNS UUID AS $$
DECLARE
    new_id UUID;
BEGIN
    INSERT INTO pillars (latitude, longitude, state, id_pillar_station) 
    VALUES (p_latitude, p_longitude, p_state, p_id_pillar_station) 
    RETURNING id INTO new_id;
    RETURN new_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_pillar(
    p_id UUID, 
    p_latitude DOUBLE PRECISION, 
    p_longitude DOUBLE PRECISION, 
    p_state pillar_state, 
    p_id_pillar_station UUID
) RETURNS VOID AS $$
BEGIN
    UPDATE pillars 
    SET latitude = p_latitude, longitude = p_longitude, state = p_state, 
        id_pillar_station = p_id_pillar_station 
    WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_pillar_state(
    p_id UUID, 
    p_state pillar_state
) RETURNS VOID AS $$
BEGIN
    UPDATE pillars 
    SET state = p_state 
    WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION exists_pillar_to_dron_station(p_id UUID) RETURNS BOOLEAN AS $$
BEGIN
    RETURN EXISTS (SELECT 1 FROM pillar_to_dron_station WHERE id = p_id);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_pillar_to_dron_station(p_id UUID) RETURNS SETOF pillar_to_dron_station AS $$
BEGIN
    RETURN QUERY SELECT * FROM pillar_to_dron_station WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION insert_pillar_to_dron_station(
    p_id_pillar UUID, 
    p_id_dron_station UUID
) RETURNS UUID AS $$
DECLARE
    new_id UUID;
BEGIN
    INSERT INTO pillar_to_dron_station (id_pillar, id_dron_station) 
    VALUES (p_id_pillar, p_id_dron_station) 
    RETURNING id INTO new_id;
    RETURN new_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_pillar_to_dron_station(
    p_id UUID, 
    p_id_pillar UUID, 
    p_id_dron_station UUID
) RETURNS VOID AS $$
BEGIN
    UPDATE pillar_to_dron_station 
    SET id_pillar = p_id_pillar, id_dron_station = p_id_dron_station 
    WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION exists_dron(p_id UUID) RETURNS BOOLEAN AS $$
BEGIN
    RETURN EXISTS (SELECT 1 FROM drons WHERE id = p_id);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_dron(p_id UUID) RETURNS SETOF drons AS $$
BEGIN
    RETURN QUERY SELECT * FROM drons WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION insert_dron(
    p_status dron_status DEFAULT 'in_station', 
    p_last_latitude DOUBLE PRECISION DEFAULT NULL, 
    p_last_longitude DOUBLE PRECISION DEFAULT NULL, 
    p_id_dron_station UUID DEFAULT NULL
) RETURNS UUID AS $$
DECLARE
    new_id UUID;
BEGIN
    INSERT INTO drons (status, last_latitude, last_longitude, id_dron_station) 
    VALUES (p_status, p_last_latitude, p_last_longitude, p_id_dron_station) 
    RETURNING id INTO new_id;
    RETURN new_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_dron(
    p_id UUID, 
    p_status dron_status, 
    p_last_latitude DOUBLE PRECISION, 
    p_last_longitude DOUBLE PRECISION, 
    p_id_dron_station UUID
) RETURNS VOID AS $$
BEGIN
    UPDATE drons 
    SET status = p_status, last_latitude = p_last_latitude, 
        last_longitude = p_last_longitude, id_dron_station = p_id_dron_station 
    WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;


CREATE OR REPLACE FUNCTION exists_history(p_id UUID) RETURNS BOOLEAN AS $$
BEGIN
    RETURN EXISTS (SELECT 1 FROM history WHERE id = p_id);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_history(p_id UUID) RETURNS SETOF history AS $$
BEGIN
    RETURN QUERY SELECT * FROM history WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION insert_history(
    p_id_dron_station UUID, 
    p_id_pillars UUID, 
    p_id_dron UUID, 
    p_status history_status
) RETURNS UUID AS $$
DECLARE
    new_id UUID;
BEGIN
    INSERT INTO history (id_dron_station, id_pillars, id_dron, status) 
    VALUES (p_id_dron_station, p_id_pillars, p_id_dron, p_status) 
    RETURNING id INTO new_id;
    RETURN new_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION update_history(
    p_id UUID, 
    p_id_dron_station UUID, 
    p_id_pillars UUID, 
    p_id_dron UUID, 
    p_status history_status
) RETURNS VOID AS $$
BEGIN
    UPDATE history 
    SET id_dron_station = p_id_dron_station, id_pillars = p_id_pillars, 
        id_dron = p_id_dron, status = p_status, update_at = NOW() 
    WHERE id = p_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE assign_pillars_to_dron_station_flat(
    p_dron_station_id UUID
)
LANGUAGE plpgsql
AS $$
DECLARE
    ds_x     DOUBLE PRECISION;   
    ds_y     DOUBLE PRECISION;  
    ds_radius INTEGER;
    cnt      INTEGER;
BEGIN
    SELECT longitude, latitude, radius
    INTO ds_x, ds_y, ds_radius
    FROM dron_stations
    WHERE id = p_dron_station_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Станция дронов с id % не найдена', p_dron_station_id;
    END IF;

    
    DELETE FROM pillar_to_dron_station
    WHERE id_dron_station = p_dron_station_id;

   
    INSERT INTO pillar_to_dron_station (id_pillar, id_dron_station)
    SELECT 
        p.id,
        p_dron_station_id
    FROM pillars p
    WHERE 
        SQRT(
            POWER(p.longitude - ds_x, 2) + 
            POWER(p.latitude  - ds_y, 2)
        ) <= ds_radius;

    GET DIAGNOSTICS cnt = ROW_COUNT;

END;
$$;

CREATE OR REPLACE FUNCTION get_pillars_for_dron_station_flat(
    p_dron_station_id UUID
)
RETURNS TABLE (
    pillar_id         UUID,
    x                 DOUBLE PRECISION,     
    y                 DOUBLE PRECISION,     
    state             pillar_state,
    id_pillar_station UUID
)
LANGUAGE sql
STABLE
AS $$
    SELECT 
        p.id              AS pillar_id,
        p.longitude       AS x,
        p.latitude        AS y,
        p.state,
        p.id_pillar_station
    FROM pillar_to_dron_station pt
    JOIN pillars p ON p.id = pt.id_pillar
    WHERE pt.id_dron_station = p_dron_station_id;
$$;

DO $$
DECLARE
    station_id        UUID;
    x_center          DOUBLE PRECISION := 0.0;      
    y_center          DOUBLE PRECISION;
    station_spacing   DOUBLE PRECISION := 200.0;    
    col_offset        DOUBLE PRECISION := 20.0;    
    row_spacing       DOUBLE PRECISION := 10.0;     
    pillars_per_col   INTEGER := 30;                
    i                 INTEGER;
BEGIN
    FOR i IN 1..5 LOOP
        y_center := -400.0 + (i - 1) * station_spacing;

        INSERT INTO pillar_stations (latitude, longitude, is_alive)
        VALUES (y_center, x_center, true)
        RETURNING id INTO station_id;

        RAISE NOTICE 'Создана станция % с центром (Y = %, X = %)', 
                     station_id, y_center, x_center;

        FOR j IN 0..(pillars_per_col - 1) LOOP
            INSERT INTO pillars (latitude, longitude, state, id_pillar_station)
            VALUES (
                y_center + j * row_spacing - (pillars_per_col * row_spacing / 2.0),  -- центрируем колонку по Y
                x_center - (col_offset / 2),
                'alive',
                station_id
            );
        END LOOP;

        FOR j IN 0..(pillars_per_col - 1) LOOP
            INSERT INTO pillars (latitude, longitude, state, id_pillar_station)
            VALUES (
                y_center + j * row_spacing - (pillars_per_col * row_spacing / 2.0),  -- те же Y-координаты
                x_center + (col_offset / 2),
                'alive',
                station_id
            );
        END LOOP;

        RAISE NOTICE '   → добавлено % столбов (2 × %)', 
                     pillars_per_col * 2, pillars_per_col;
    END LOOP;

    RAISE NOTICE 'Готово.';
    RAISE NOTICE 'Создано 5 станций вертикально по Y с шагом 200.';
    RAISE NOTICE 'Всего столбов: % (5 × 60)', 5 * pillars_per_col * 2;
END $$;