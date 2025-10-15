-- Función para trigger
CREATE OR REPLACE FUNCTION fn_actualizar_version()
RETURNS TRIGGER AS $$
BEGIN
    NEW."Version" = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Procedimiento para crear tablas (CORREGIDO)
CREATE OR REPLACE PROCEDURE crear_tabla(
    p_esquema TEXT,
    p_tabla TEXT,
    p_columnas_adicionales TEXT DEFAULT ''
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_tabla_lower TEXT := LOWER(p_tabla);
    v_sql TEXT;
    v_columna_pk TEXT := 'id_' || v_tabla_lower;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_schema = p_esquema
          AND table_name = v_tabla_lower
    ) THEN
        v_sql := format(
            'CREATE TABLE %I.%I (%I SERIAL PRIMARY KEY%s%s)',
            p_esquema,
            v_tabla_lower,
            v_columna_pk,
            CASE WHEN p_columnas_adicionales IS NOT NULL AND p_columnas_adicionales <> '' THEN ', ' ELSE '' END,
            p_columnas_adicionales
        );

        EXECUTE v_sql;
        RAISE NOTICE 'Tabla %.% creada con PK %.', p_esquema, v_tabla_lower, v_columna_pk;
    ELSE
        RAISE NOTICE 'La tabla %.% ya existe.', p_esquema, v_tabla_lower;
    END IF;
END;
$$;

-- Procedimiento para crear columnas (CORREGIDO)
CREATE OR REPLACE PROCEDURE crear_columna(
    p_esquema TEXT,
    p_tabla TEXT,
    p_columna TEXT,
    p_definicion TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_tabla_mayus TEXT := UPPER(p_tabla);
    v_columna_mayus TEXT := UPPER(p_columna);
    v_sql TEXT;
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = p_esquema
          AND table_name = LOWER(v_tabla_mayus)
          AND column_name = LOWER(v_columna_mayus)
    ) THEN
        v_sql := format('ALTER TABLE %I.%I ADD COLUMN %I %s', 
                       p_esquema, 
                       LOWER(v_tabla_mayus), 
                       LOWER(v_columna_mayus), 
                       p_definicion);
        EXECUTE v_sql;
        RAISE NOTICE 'Columna %.%.% creada correctamente.', p_esquema, LOWER(v_tabla_mayus), LOWER(v_columna_mayus);
    ELSE
        RAISE NOTICE 'La columna %.%.% ya existe.', p_esquema, LOWER(v_tabla_mayus), LOWER(v_columna_mayus);
    END IF;
END;
$$;

-- Procedimiento para crear índices (CORREGIDO)
CREATE OR REPLACE PROCEDURE crear_indice(
    p_esquema TEXT,
    p_tabla TEXT,
    p_columnas TEXT,
    p_tipo_indice TEXT DEFAULT 'btree',
    p_unique BOOLEAN DEFAULT FALSE
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_tabla_mayus TEXT := UPPER(p_tabla);
    v_nombre_indice TEXT;
    v_sql TEXT;
    v_columnas_limpias TEXT;
    v_columnas_mayus TEXT;
BEGIN
    v_columnas_limpias := REPLACE(p_columnas, ' ', '');
    v_columnas_mayus := UPPER(v_columnas_limpias);

    v_nombre_indice := 'idx_' || LOWER(v_tabla_mayus) || '_' || REPLACE(LOWER(v_columnas_limpias), ',', '_');

    IF p_unique THEN
        v_nombre_indice := 'uidx_' || LOWER(v_tabla_mayus) || '_' || REPLACE(LOWER(v_columnas_limpias), ',', '_');
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM pg_indexes
        WHERE schemaname = p_esquema
          AND tablename = LOWER(v_tabla_mayus)
          AND indexname = v_nombre_indice
    ) THEN
        v_sql := format(
            'CREATE %s INDEX %I ON %I.%I USING %s (%s)',
            CASE WHEN p_unique THEN 'UNIQUE' ELSE '' END,
            v_nombre_indice,
            p_esquema,
            LOWER(v_tabla_mayus),
            p_tipo_indice,
            LOWER(v_columnas_limpias)
        );

        EXECUTE v_sql;
        RAISE NOTICE 'Índice % creado correctamente', v_nombre_indice;
    ELSE
        RAISE NOTICE 'El índice % ya existe.', v_nombre_indice;
    END IF;
END;
$$;

-- Procedimiento para agregar versionado (CORREGIDO)
CREATE OR REPLACE PROCEDURE agregar_versionado(
    p_esquema TEXT,
    p_tabla TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_tabla_mayus TEXT := UPPER(p_tabla);
    v_trigger_name TEXT := 'trg_actualizar_version_' || LOWER(v_tabla_mayus);
    v_sql TEXT;
BEGIN
    -- Agregar columnas de versionado
    CALL crear_columna(p_esquema, v_tabla_mayus, 'fechacreacion', 'TIMESTAMPTZ NOT NULL DEFAULT NOW()');
    CALL crear_columna(p_esquema, v_tabla_mayus, 'version', 'TIMESTAMPTZ NOT NULL DEFAULT NOW()');

    -- Crear el trigger si no existe
    IF NOT EXISTS (
        SELECT 1 FROM pg_trigger
        WHERE tgname = v_trigger_name
    ) THEN
        v_sql := format(
            'CREATE TRIGGER %I BEFORE UPDATE ON %I.%I ' ||
            'FOR EACH ROW EXECUTE FUNCTION fn_actualizar_version()',
            v_trigger_name,
            p_esquema,
            LOWER(v_tabla_mayus)
        );
        EXECUTE v_sql;
        RAISE NOTICE 'Trigger de versión % creado en la tabla %.%', v_trigger_name, p_esquema, LOWER(v_tabla_mayus);
    ELSE
        RAISE NOTICE 'El trigger de versión % ya existe en la tabla %.%', v_trigger_name, p_esquema, LOWER(v_tabla_mayus);
    END IF;
END;
$$;

-- Procedimiento para crear FKs (CORREGIDO)
CREATE OR REPLACE PROCEDURE crear_fk(
    p_esquema TEXT,
    p_tabla_hija TEXT,
    p_tabla_padre TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_tabla_hija_mayus TEXT := UPPER(p_tabla_hija);
    v_tabla_padre_mayus TEXT := UPPER(p_tabla_padre);
    v_columna_pk_padre TEXT;
    v_columna_fk TEXT;
    v_nombre_fk TEXT;
    v_sql TEXT;
BEGIN
    -- Encontrar la PRIMARY KEY de la tabla padre
    SELECT kcu.column_name INTO v_columna_pk_padre
    FROM information_schema.table_constraints tc
    JOIN information_schema.key_column_usage kcu
        ON tc.constraint_name = kcu.constraint_name
        AND tc.table_schema = kcu.table_schema
    WHERE tc.constraint_type = 'PRIMARY KEY'
      AND tc.table_schema = p_esquema
      AND tc.table_name = LOWER(v_tabla_padre_mayus)
    LIMIT 1;

    IF v_columna_pk_padre IS NULL THEN
        RAISE EXCEPTION 'No se encontró PRIMARY KEY en la tabla %.%', p_esquema, LOWER(v_tabla_padre_mayus);
    END IF;

    -- Determinar el nombre de la columna FK automáticamente
    v_columna_fk := 'id_' || LOWER(v_tabla_padre_mayus);

    -- Crear la columna FK si no existe
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = p_esquema
          AND table_name = LOWER(v_tabla_hija_mayus)
          AND column_name = v_columna_fk
    ) THEN
        v_sql := format('ALTER TABLE %I.%I ADD COLUMN %I INTEGER',
                        p_esquema, LOWER(v_tabla_hija_mayus), v_columna_fk);
        EXECUTE v_sql;
        RAISE NOTICE 'Columna %.%.% creada correctamente.', p_esquema, LOWER(v_tabla_hija_mayus), v_columna_fk;
    END IF;

    -- Crear la restricción FK
    v_nombre_fk := 'fk_' || LOWER(v_tabla_hija_mayus) || '_' || v_columna_fk;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_type = 'FOREIGN KEY'
          AND table_schema = p_esquema
          AND table_name = LOWER(v_tabla_hija_mayus)
          AND constraint_name = v_nombre_fk
    ) THEN
        v_sql := format(
            'ALTER TABLE %I.%I ADD CONSTRAINT %I FOREIGN KEY (%I) REFERENCES %I.%I(%I)',
            p_esquema, LOWER(v_tabla_hija_mayus), v_nombre_fk, v_columna_fk,
            p_esquema, LOWER(v_tabla_padre_mayus), v_columna_pk_padre
        );
        EXECUTE v_sql;
        RAISE NOTICE 'FK % creada correctamente', v_nombre_fk;
    ELSE
        RAISE NOTICE 'La FK % ya existe.', v_nombre_fk;
    END IF;
END;
$$;