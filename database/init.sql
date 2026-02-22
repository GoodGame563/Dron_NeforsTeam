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
    id_pillar_station UUID NOT NULL,
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
    update_at        TIMESTAMPTZ NOT NULL DEFAULT NOW(),
);

CREATE INDEX IF NOT EXISTS idx_pillars_station ON pillars(id_pillar_station);
CREATE INDEX IF NOT EXISTS idx_drons_pillars ON drons(id_pillars);
CREATE INDEX IF NOT EXISTS idx_drons_station ON drons(id_dron_station);
CREATE INDEX IF NOT EXISTS idx_history_dron ON history(id_dron);
CREATE INDEX IF NOT EXISTS idx_history_update ON history(update_at DESC);