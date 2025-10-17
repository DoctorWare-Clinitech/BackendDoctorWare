-- =====================================================
-- Función para trigger de versionado
-- =====================================================
CREATE OR REPLACE FUNCTION fn_actualizar_version()
RETURNS trigger AS $$
BEGIN
    NEW."ULTIMA_ACTUALIZACION" = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- =====================================================
-- PROCEDIMIENTO: crear_tabla
-- Crea tablas con nombres en MAYÚSCULAS
-- =====================================================
CREATE OR REPLACE PROCEDURE crear_tabla(
    p_esquema TEXT,
    p_tabla TEXT,
    p_columnas_adicionales TEXT DEFAULT ''
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_tabla_mayus TEXT := UPPER(p_tabla);
    v_sql TEXT;
    v_columna_pk TEXT := 'ID_' || v_tabla_mayus;
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_schema = p_esquema
          AND table_name = v_tabla_mayus
    ) THEN
        v_sql := format(
            'CREATE TABLE %I.%I (%I SERIAL PRIMARY KEY%s%s)',
            p_esquema,
            v_tabla_mayus,
            v_columna_pk,
            CASE WHEN p_columnas_adicionales IS NOT NULL AND p_columnas_adicionales <> '' THEN ', ' ELSE '' END,
            p_columnas_adicionales
        );

        EXECUTE v_sql;
        RAISE NOTICE 'Tabla %.% creada con PK %.', p_esquema, v_tabla_mayus, v_columna_pk;
    ELSE
        RAISE NOTICE 'La tabla %.% ya existe.', p_esquema, v_tabla_mayus;
    END IF;
END;
$$;

-- =====================================================
-- PROCEDIMIENTO: crear_columna
-- Crea columnas con nombres en MAYÚSCULAS
-- =====================================================
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
          AND table_name = v_tabla_mayus
          AND column_name = v_columna_mayus
    ) THEN
        v_sql := format('ALTER TABLE %I.%I ADD COLUMN %I %s', 
                       p_esquema, 
                       v_tabla_mayus, 
                       v_columna_mayus, 
                       p_definicion);
        EXECUTE v_sql;
        RAISE NOTICE 'Columna %.%.% creada correctamente.', p_esquema, v_tabla_mayus, v_columna_mayus;
    ELSE
        RAISE NOTICE 'La columna %.%.% ya existe.', p_esquema, v_tabla_mayus, v_columna_mayus;
    END IF;
END;
$$;

-- =====================================================
-- PROCEDIMIENTO: crear_indice
-- Crea índices con nombres en MAYÚSCULAS
-- =====================================================
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
    v_columna TEXT;
    v_columna_array TEXT[];
    v_columnas_citadas TEXT;
BEGIN
    v_columnas_limpias := REPLACE(p_columnas, ' ', '');
    v_columnas_mayus := UPPER(v_columnas_limpias);

    -- Nombre del índice en MAYÚSCULAS
    v_nombre_indice := 'IDX_' || v_tabla_mayus || '_' || REPLACE(v_columnas_mayus, ',', '_');
    IF p_unique THEN
        v_nombre_indice := 'UIDX_' || v_tabla_mayus || '_' || REPLACE(v_columnas_mayus, ',', '_');
    END IF;

    -- Validar que las columnas existan
    v_columna_array := string_to_array(v_columnas_mayus, ',');
    FOREACH v_columna IN ARRAY v_columna_array
    LOOP
        IF NOT EXISTS (
            SELECT 1
            FROM information_schema.columns
            WHERE table_schema = p_esquema
              AND table_name = v_tabla_mayus
              AND column_name = v_columna
        ) THEN
            RAISE EXCEPTION 'La columna %.%.% no existe', p_esquema, v_tabla_mayus, v_columna;
        END IF;
    END LOOP;

    -- Crear listado de columnas con comillas dobles
    v_columnas_citadas := array_to_string(
        ARRAY(
            SELECT format('"%s"', trim(c))
            FROM unnest(v_columna_array) AS c
        ), ', '
    );

    -- Verificar si el índice ya existe
    IF EXISTS (
        SELECT 1
        FROM pg_indexes
        WHERE schemaname = p_esquema
          AND tablename = v_tabla_mayus
          AND indexname = v_nombre_indice
    ) THEN
        RAISE NOTICE 'El índice % ya existe.', v_nombre_indice;
        RETURN;
    END IF;

    -- Crear el índice con columnas citadas
    v_sql := format(
        'CREATE %s INDEX %I ON %I.%I USING %s (%s)',
        CASE WHEN p_unique THEN 'UNIQUE' ELSE '' END,
        v_nombre_indice,
        p_esquema,
        v_tabla_mayus,
        p_tipo_indice,
        v_columnas_citadas
    );

    EXECUTE v_sql;
    RAISE NOTICE 'Índice % creado correctamente en %.%', v_nombre_indice, p_esquema, v_tabla_mayus;
END;
$$;

-- =====================================================
-- PROCEDIMIENTO: agregar_versionado
-- Agrega columnas FECHACREACION y VERSION
-- =====================================================
CREATE OR REPLACE PROCEDURE agregar_versionado(
    p_esquema TEXT,
    p_tabla TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_tabla_mayus TEXT := UPPER(p_tabla);
    v_trigger_name TEXT := 'TRG_ACTUALIZAR_VERSION_' || v_tabla_mayus;
    v_sql TEXT;
BEGIN
    -- Asegurarse que la función de trigger exista
    IF NOT EXISTS (SELECT 1 FROM pg_proc WHERE proname = 'fn_actualizar_version') THEN
         RAISE EXCEPTION 'La función de trigger "fn_actualizar_version" no existe.';
    END IF;

    -- Agregar columna FECHACREACION
    CALL crear_columna(p_esquema, v_tabla_mayus, 'FECHA_CREACION', 'TIMESTAMPTZ NOT NULL DEFAULT NOW()');

    -- Agregar columna VERSION
    CALL crear_columna(p_esquema, v_tabla_mayus, 'ULTIMA_ACTUALIZACION', 'TIMESTAMPTZ NOT NULL DEFAULT NOW()');

    -- Crear el trigger si no existe
    IF NOT EXISTS (
        SELECT 1 FROM pg_trigger
        WHERE tgname = v_trigger_name
    ) THEN
        v_sql := format(
            'CREATE TRIGGER %I BEFORE UPDATE ON %I.%I FOR EACH ROW EXECUTE FUNCTION fn_actualizar_version()',
            v_trigger_name,
            p_esquema,
            v_tabla_mayus
        );
        EXECUTE v_sql;
        RAISE NOTICE 'Trigger de versión % creado en la tabla %.%', v_trigger_name, p_esquema, v_tabla_mayus;
    ELSE
        RAISE NOTICE 'El trigger de versión % ya existe en la tabla %.%', v_trigger_name, p_esquema, v_tabla_mayus;
    END IF;
END;
$$;

-- =====================================================
-- PROCEDIMIENTO: crear_fk (CON PARÁMETRO OPCIONAL)
-- Crea FK con nombre de columna personalizable
-- =====================================================
CREATE OR REPLACE PROCEDURE crear_fk(
    p_esquema TEXT,
    p_tabla_hija TEXT,
    p_tabla_padre TEXT,
    p_nombre_columna_fk TEXT DEFAULT NULL
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
      AND tc.table_name = v_tabla_padre_mayus
    LIMIT 1;

    IF v_columna_pk_padre IS NULL THEN
        RAISE EXCEPTION 'No se encontró PRIMARY KEY en la tabla %.%', p_esquema, v_tabla_padre_mayus;
    END IF;

    RAISE NOTICE 'PK detectada en %.%: %', p_esquema, v_tabla_padre_mayus, v_columna_pk_padre;

    -- Determinar el nombre de la columna FK
    IF p_nombre_columna_fk IS NOT NULL THEN
        v_columna_fk := UPPER(p_nombre_columna_fk);
    ELSE
        -- Nombre automático: ID_TABLA_PADRE (sin la S del plural si existe)
        v_columna_fk := 'ID_' || 
            UPPER(CASE
                WHEN p_tabla_padre LIKE '%s' THEN LEFT(p_tabla_padre, LENGTH(p_tabla_padre) - 1)
                ELSE p_tabla_padre
            END);
    END IF;

    RAISE NOTICE 'Columna FK a crear: %', v_columna_fk;

    -- Crear la columna FK si no existe
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = p_esquema
          AND table_name = v_tabla_hija_mayus
          AND column_name = v_columna_fk
    ) THEN
        v_sql := format('ALTER TABLE %I.%I ADD COLUMN %I INTEGER',
                        p_esquema, v_tabla_hija_mayus, v_columna_fk);
        EXECUTE v_sql;
        RAISE NOTICE 'Columna %.%.% creada correctamente.', p_esquema, v_tabla_hija_mayus, v_columna_fk;
    ELSE
        RAISE NOTICE 'La columna %.%.% ya existe.', p_esquema, v_tabla_hija_mayus, v_columna_fk;
    END IF;

    -- Crear la restricción FK
    v_nombre_fk := 'FK_' || v_tabla_hija_mayus || '_' || v_columna_fk;

    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE constraint_type = 'FOREIGN KEY'
          AND table_schema = p_esquema
          AND table_name = v_tabla_hija_mayus
          AND constraint_name = v_nombre_fk
    ) THEN
        v_sql := format(
            'ALTER TABLE %I.%I ADD CONSTRAINT %I FOREIGN KEY (%I) REFERENCES %I.%I(%I)',
            p_esquema, v_tabla_hija_mayus, v_nombre_fk, v_columna_fk,
            p_esquema, v_tabla_padre_mayus, v_columna_pk_padre
        );
        EXECUTE v_sql;
        RAISE NOTICE 'FK % creada: %.%(%I) -> %.%(%I)',
                     v_nombre_fk,
                     p_esquema, v_tabla_hija_mayus, v_columna_fk,
                     p_esquema, v_tabla_padre_mayus, v_columna_pk_padre;
    ELSE
        RAISE NOTICE 'La FK % ya existe.', v_nombre_fk;
    END IF;
END;
$$;
