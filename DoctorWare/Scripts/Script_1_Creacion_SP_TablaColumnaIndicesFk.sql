DO $$
BEGIN
    -- Función de trigger para actualizar la columna de versión en cada UPDATE.
    -- La función es llamada por los triggers de las tablas versionadas.
    IF NOT EXISTS (SELECT 1 FROM pg_proc WHERE proname = 'actualizar_columna_version') THEN
        CREATE OR REPLACE FUNCTION actualizar_columna_version()
        RETURNS TRIGGER AS $function$
        BEGIN
            -- Asigna la fecha y hora actual a la columna 'Version' del nuevo registro.
            NEW.Version = NOW();
            RETURN NEW;
        END;
        $function$ LANGUAGE plpgsql;
        RAISE NOTICE 'Función de trigger actualizar_columna_version creada.';
    END IF;

    -- Procedimiento para crear una tabla si no existe.
    -- Agrega automáticamente una clave primaria llamada ID_<NOMBRE_TABLA_MAYUS>.
    -- Uso: CALL crear_tabla('public', 'mi_tabla', 'nombre TEXT, edad INT');
    IF NOT EXISTS (SELECT 1 FROM pg_proc WHERE proname = 'crear_tabla') THEN
        CREATE OR REPLACE PROCEDURE crear_tabla(
            p_esquema TEXT,
            p_tabla TEXT,
            p_columnas_adicionales TEXT DEFAULT ''
        )
        LANGUAGE plpgsql
        AS $procedure$
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
        $procedure$;
        RAISE NOTICE 'Procedimiento crear_tabla creado.';
    END IF;

    -- Procedimiento para agregar una columna a una tabla si la columna no existe.
    -- Uso: CALL crear_columna('public', 'mi_tabla', 'nueva_columna', 'VARCHAR(100)');
    IF NOT EXISTS (SELECT 1 FROM pg_proc WHERE proname = 'crear_columna') THEN
        CREATE OR REPLACE PROCEDURE crear_columna(
            p_esquema TEXT,
            p_tabla TEXT,
            p_columna TEXT,
            p_definicion TEXT
        )
        LANGUAGE plpgsql
        AS $procedure$
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
                v_sql := format('ALTER TABLE %I.%I ADD COLUMN %I %s', p_esquema, v_tabla_mayus, v_columna_mayus, p_definicion);
                EXECUTE v_sql;
                RAISE NOTICE 'Columna %.%:% creada correctamente.', p_esquema, v_tabla_mayus, v_columna_mayus;
            ELSE
                RAISE NOTICE 'La columna %.%:% ya existe.', p_esquema, v_tabla_mayus, v_columna_mayus;
            END IF;
        END;
        $procedure$;
        RAISE NOTICE 'Procedimiento crear_columna creado.';
    END IF;
	
    -- Procedimiento para crear automáticamente una clave foránea infiriendo nombres.
    -- Uso: CALL crear_fk('public', 'tabla_hija', 'tabla_padre');
    IF NOT EXISTS (SELECT 1 FROM pg_proc WHERE proname = 'crear_fk') THEN
        CREATE OR REPLACE PROCEDURE crear_fk(
            p_esquema TEXT,
            p_tabla_hija TEXT,
            p_tabla_padre TEXT,
            p_nombre_columna_fk TEXT DEFAULT NULL
        )
        LANGUAGE plpgsql
        AS $procedure$
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
                RAISE NOTICE 'Columna %.%:% creada correctamente.', p_esquema, v_tabla_hija_mayus, v_columna_fk;
            ELSE
                RAISE NOTICE 'La columna %.%:% ya existe.', p_esquema, v_tabla_hija_mayus, v_columna_fk;
            END IF;

            -- Crear la restricción FK
            v_nombre_fk := format('FK_%s_%s', v_tabla_hija_mayus, v_columna_fk);

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
        $procedure$;
        RAISE NOTICE 'Procedimiento crear_fk_auto creado.';
    END IF;
	
    -- Procedimiento para crear automáticamente un índice en una o más columnas.
    -- Uso: CALL crear_indice('public', 'mi_tabla', 'nombre,apellido');
    IF NOT EXISTS (SELECT 1 FROM pg_proc WHERE proname = 'crear_indice') THEN
        CREATE OR REPLACE PROCEDURE crear_indice(
            p_esquema TEXT,
            p_tabla TEXT,
            p_columnas TEXT,
            p_tipo_indice TEXT DEFAULT 'btree',
            p_unique BOOLEAN DEFAULT FALSE
        )
        LANGUAGE plpgsql
        AS $procedure$
        DECLARE
            v_tabla_mayus TEXT := UPPER(p_tabla);
            v_nombre_indice TEXT;
            v_sql TEXT;
            v_columnas_limpias TEXT;
            v_columnas_mayus TEXT;
        BEGIN
            v_columnas_limpias := REPLACE(p_columnas, ' ', '');
            v_columnas_mayus := UPPER(v_columnas_limpias);

            v_nombre_indice := 'IDX_' || v_tabla_mayus || '_' || REPLACE(v_columnas_mayus, ',', '_');

            IF p_unique THEN
                v_nombre_indice := 'UIDX_' || v_tabla_mayus || '_' || REPLACE(v_columnas_mayus, ',', '_');
            END IF;

            DECLARE
                v_columna TEXT;
                v_columna_array TEXT[];
            BEGIN
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
                        RAISE EXCEPTION 'La columna %.%:% no existe', p_esquema, v_tabla_mayus, v_columna;
                    END IF;
                END LOOP;
            END;

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

            v_sql := format(
                'CREATE %s INDEX %I ON %I.%I USING %s (%s)',
                CASE WHEN p_unique THEN 'UNIQUE' ELSE '' END,
                v_nombre_indice,
                p_esquema,
                v_tabla_mayus,
                p_tipo_indice,
                v_columnas_mayus
            );

            EXECUTE v_sql;
            RAISE NOTICE 'Índice % creado: % USING % en columna(s): %',
                         v_nombre_indice,
                         CASE WHEN p_unique THEN 'UNIQUE' ELSE 'NORMAL' END,
                         UPPER(p_tipo_indice),
                         v_columnas_mayus;
        END;
        $procedure$;
        RAISE NOTICE 'Procedimiento crear_indice_auto creado.';
    END IF;

    -- Procedimiento para agregar versionado a una tabla existente.
    -- Agrega las columnas FechaCreacion y Version, y un trigger para actualizar 'Version' en cada UPDATE.
    -- Uso: CALL agregar_versionado('public', 'mi_tabla');
    IF NOT EXISTS (SELECT 1 FROM pg_proc WHERE proname = 'agregar_versionado') THEN
        CREATE OR REPLACE PROCEDURE agregar_versionado(
            p_esquema TEXT,
            p_tabla TEXT
        )
        LANGUAGE plpgsql
        AS $procedure$
        DECLARE
            v_tabla_mayus TEXT := UPPER(p_tabla);
            v_trigger_name TEXT := 'trg_actualizar_version_' || v_tabla_mayus;
            v_sql TEXT;
        BEGIN
            -- Asegurarse que la función de trigger exista
            IF NOT EXISTS (SELECT 1 FROM pg_proc WHERE proname = 'actualizar_columna_version') THEN
                 RAISE EXCEPTION 'La función de trigger "actualizar_columna_version" no existe. Asegúrese de que el script la cree primero.';
            END IF;

            -- Agregar columna FechaCreacion
            CALL crear_columna(p_esquema, v_tabla_mayus, 'FechaCreacion', 'TIMESTAMPTZ NOT NULL DEFAULT NOW()');

            -- Agregar columna Version
            CALL crear_columna(p_esquema, v_tabla_mayus, 'Version', 'TIMESTAMPTZ NOT NULL DEFAULT NOW()');

            -- Crear el trigger si no existe
            IF NOT EXISTS (
                SELECT 1 FROM pg_trigger
                WHERE tgname = v_trigger_name
            ) THEN
                v_sql := format(
                    'CREATE TRIGGER %I BEFORE UPDATE ON %I.%I FOR EACH ROW EXECUTE FUNCTION actualizar_columna_version()',
                    v_trigger_name,
                    p_esquema,
                    v_tabla_mayus
                );
                EXECUTE v_sql;
                RAISE NOTICE 'Trigger de versión % creado en la tabla %.%. ', v_trigger_name, p_esquema, v_tabla_mayus;
            ELSE
                RAISE NOTICE 'El trigger de versión % ya existe en la tabla %.%.', v_trigger_name, p_esquema, v_tabla_mayus;
            END IF;
        END;
        $procedure$;
        RAISE NOTICE 'Procedimiento agregar_versionado creado.';
    END IF;

    -- Procedimiento para modificar el tipo de dato o definición de una columna.
    -- Uso: CALL modificar_columna('public', 'mi_tabla', 'mi_columna', 'VARCHAR(255)');
    IF NOT EXISTS (SELECT 1 FROM pg_proc WHERE proname = 'modificar_columna') THEN
        CREATE OR REPLACE PROCEDURE modificar_columna(
            p_esquema TEXT,
            p_tabla TEXT,
            p_columna TEXT,
            p_nueva_definicion TEXT
        )
        LANGUAGE plpgsql
        AS $procedure$
        DECLARE
            v_tabla_mayus TEXT := UPPER(p_tabla);
            v_columna_mayus TEXT := UPPER(p_columna);
            v_sql TEXT;
        BEGIN
            IF EXISTS (
                SELECT 1 FROM information_schema.columns
                WHERE table_schema = p_esquema
                  AND table_name = v_tabla_mayus
                  AND column_name = v_columna_mayus
            ) THEN
                v_sql := format('ALTER TABLE %I.%I ALTER COLUMN %I TYPE %s',
                                p_esquema, v_tabla_mayus, v_columna_mayus, p_nueva_definicion);
                EXECUTE v_sql;
                RAISE NOTICE 'Columna %.%:% modificada a tipo %.', p_esquema, v_tabla_mayus, v_columna_mayus, p_nueva_definicion;
            ELSE
                RAISE NOTICE 'No se puede modificar: la columna %.%:% no existe.', p_esquema, v_tabla_mayus, v_columna_mayus;
            END IF;
        END;
        $procedure$;
        RAISE NOTICE 'Procedimiento modificar_columna creado.';
    END IF;

    -- Procedimiento para eliminar una columna de una tabla.
    -- Uso: CALL eliminar_columna('public', 'mi_tabla', 'columna_inutil');
    IF NOT EXISTS (SELECT 1 FROM pg_proc WHERE proname = 'eliminar_columna') THEN
        CREATE OR REPLACE PROCEDURE eliminar_columna(
            p_esquema TEXT,
            p_tabla TEXT,
            p_columna TEXT
        )
        LANGUAGE plpgsql
        AS $procedure$
        DECLARE
            v_tabla_mayus TEXT := UPPER(p_tabla);
            v_columna_mayus TEXT := UPPER(p_columna);
            v_sql TEXT;
        BEGIN
            IF EXISTS (
                SELECT 1 FROM information_schema.columns
                WHERE table_schema = p_esquema
                  AND table_name = v_tabla_mayus
                  AND column_name = v_columna_mayus
            ) THEN
                v_sql := format('ALTER TABLE %I.%I DROP COLUMN %I', p_esquema, v_tabla_mayus, v_columna_mayus);
                EXECUTE v_sql;
                RAISE NOTICE 'Columna %.%:% eliminada correctamente.', p_esquema, v_tabla_mayus, v_columna_mayus;
            ELSE
                RAISE NOTICE 'La columna %.%:% no existe, no se eliminó.', p_esquema, v_tabla_mayus, v_columna_mayus;
            END IF;
        END;
        $procedure$;
        RAISE NOTICE 'Procedimiento eliminar_columna creado.';
    END IF;

    -- Procedimiento para eliminar una tabla.
    -- Uso: CALL eliminar_tabla('public', 'mi_tabla');
    IF NOT EXISTS (SELECT 1 FROM pg_proc WHERE proname = 'eliminar_tabla') THEN
        CREATE OR REPLACE PROCEDURE eliminar_tabla(
            p_esquema TEXT,
            p_tabla TEXT
        )
        LANGUAGE plpgsql
        AS $procedure$
        DECLARE
            v_tabla_mayus TEXT := UPPER(p_tabla);
            v_sql TEXT;
        BEGIN
            IF EXISTS (
                SELECT 1 FROM information_schema.tables
                WHERE table_schema = p_esquema
                  AND table_name = v_tabla_mayus
            ) THEN
                v_sql := format('DROP TABLE %I.%I', p_esquema, v_tabla_mayus);
                EXECUTE v_sql;
                RAISE NOTICE 'Tabla %.% eliminada correctamente.', p_esquema, v_tabla_mayus;
            ELSE
                RAISE NOTICE 'La tabla %.% no existe, no se eliminó.', p_esquema, v_tabla_mayus;
            END IF;
        END;
        $procedure$;
        RAISE NOTICE 'Procedimiento eliminar_tabla creado.';
    END IF;

    -- Procedimiento para eliminar una restricción de clave foránea.
    -- Uso: CALL eliminar_fk('public', 'tabla_hija', 'fk_tablahija_id_padre');
    IF NOT EXISTS (SELECT 1 FROM pg_proc WHERE proname = 'eliminar_fk') THEN
        CREATE OR REPLACE PROCEDURE eliminar_fk(
            p_esquema TEXT,
            p_tabla TEXT,
            p_nombre_fk TEXT
        )
        LANGUAGE plpgsql
        AS $procedure$
        DECLARE
            v_tabla_mayus TEXT := UPPER(p_tabla);
            v_nombre_fk_mayus TEXT := UPPER(p_nombre_fk);
            v_sql TEXT;
        BEGIN
            IF EXISTS (
                SELECT 1
                FROM information_schema.table_constraints
                WHERE constraint_type = 'FOREIGN KEY'
                  AND table_schema = p_esquema
                  AND table_name = v_tabla_mayus
                  AND constraint_name = v_nombre_fk_mayus
            ) THEN
                v_sql := format('ALTER TABLE %I.%I DROP CONSTRAINT %I',
                                p_esquema, v_tabla_mayus, v_nombre_fk_mayus);
                EXECUTE v_sql;
                RAISE NOTICE 'FK % eliminada correctamente.', v_nombre_fk_mayus;
            ELSE
                RAISE NOTICE 'La FK % no existe, no se eliminó.', v_nombre_fk_mayus;
            END IF;
        END;
        $procedure$;
        RAISE NOTICE 'Procedimiento eliminar_fk creado.';
    END IF;

    -- Procedimiento para eliminar un índice por columnas.
    -- Uso: CALL eliminar_indice('public', 'mi_tabla', 'columna1,columna2');
    IF NOT EXISTS (SELECT 1 FROM pg_proc WHERE proname = 'eliminar_indice') THEN
        CREATE OR REPLACE PROCEDURE eliminar_indice(
            p_esquema TEXT,
            p_tabla TEXT,
            p_columnas TEXT
        )
        LANGUAGE plpgsql
        AS $procedure$
        DECLARE
            v_tabla_mayus TEXT := UPPER(p_tabla);
            v_columnas_limpias TEXT := REPLACE(p_columnas, ' ', '');
            v_columnas_mayus TEXT := UPPER(v_columnas_limpias);
            v_nombre_indice TEXT;
            v_nombre_indice_unique TEXT;
            v_sql TEXT;
            v_indice_encontrado TEXT;
        BEGIN
            -- Construir nombres de índice posibles basados en la convención de 'crear_indice'
            v_nombre_indice := 'IDX_' || v_tabla_mayus || '_' || REPLACE(v_columnas_mayus, ',', '_');
            v_nombre_indice_unique := 'UIDX_' || v_tabla_mayus || '_' || REPLACE(v_columnas_mayus, ',', '_');

            -- Buscar el índice por los nombres construidos
            SELECT indexname INTO v_indice_encontrado
            FROM pg_indexes
            WHERE schemaname = p_esquema
              AND tablename = v_tabla_mayus
              AND (indexname = v_nombre_indice OR indexname = v_nombre_indice_unique)
            LIMIT 1;

            IF v_indice_encontrado IS NOT NULL THEN
                v_sql := format('DROP INDEX %I.%I', p_esquema, v_indice_encontrado);
                EXECUTE v_sql;
                RAISE NOTICE 'Índice %.% eliminado correctamente.', p_esquema, v_indice_encontrado;
            ELSE
                RAISE NOTICE 'No se encontró un índice para la tabla %.% con las columnas %.', p_esquema, v_tabla_mayus, p_columnas;
            END IF;
        END;
        $procedure$;
        RAISE NOTICE 'Procedimiento eliminar_indice_por_columnas creado.';
    END IF;
END;
$$;