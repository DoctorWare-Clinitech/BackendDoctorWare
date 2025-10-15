-- =====================================================
-- PROCEDIMIENTO: eliminar_tabla
-- =====================================================
CREATE OR REPLACE PROCEDURE eliminar_tabla(
    p_esquema TEXT,
    p_tabla TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_tabla_mayus TEXT := UPPER(p_tabla);
BEGIN
    EXECUTE format('DROP TABLE IF EXISTS %I.%I CASCADE', p_esquema, v_tabla_mayus);
    RAISE NOTICE 'Tabla %.% eliminada.', p_esquema, v_tabla_mayus;
EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION 'Error al eliminar la tabla %.%: %', p_esquema, v_tabla_mayus, SQLERRM;
END;
$$;

-- =====================================================
-- PROCEDIMIENTO: eliminar_columna
-- =====================================================
CREATE OR REPLACE PROCEDURE eliminar_columna(
    p_esquema TEXT,
    p_tabla TEXT,
    p_columna TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_tabla_mayus TEXT := UPPER(p_tabla);
    v_columna_mayus TEXT := UPPER(p_columna);
BEGIN
    EXECUTE format('ALTER TABLE %I.%I DROP COLUMN IF EXISTS %I', p_esquema, v_tabla_mayus, v_columna_mayus);
    RAISE NOTICE 'Columna %.%.% eliminada.', p_esquema, v_tabla_mayus, v_columna_mayus;
EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION 'Error al eliminar la columna %.%.%: %', p_esquema, v_tabla_mayus, v_columna_mayus, SQLERRM;
END;
$$;

-- =====================================================
-- PROCEDIMIENTO: eliminar_indice
-- =====================================================
CREATE OR REPLACE PROCEDURE eliminar_indice(
    p_esquema TEXT,
    p_nombre_indice TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_indice_mayus TEXT := UPPER(p_nombre_indice);
BEGIN
    EXECUTE format('DROP INDEX IF EXISTS %I.%I', p_esquema, v_indice_mayus);
    RAISE NOTICE 'Índice %.% eliminado.', p_esquema, v_indice_mayus;
EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION 'Error al eliminar el índice %.%: %', p_esquema, v_indice_mayus, SQLERRM;
END;
$$;

-- =====================================================
-- PROCEDIMIENTO: eliminar_fk
-- =====================================================
CREATE OR REPLACE PROCEDURE eliminar_fk(
    p_esquema TEXT,
    p_tabla TEXT,
    p_nombre_fk TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_tabla_mayus TEXT := UPPER(p_tabla);
    v_fk_mayus TEXT := UPPER(p_nombre_fk);
BEGIN
    EXECUTE format('ALTER TABLE %I.%I DROP CONSTRAINT IF EXISTS %I', p_esquema, v_tabla_mayus, v_fk_mayus);
    RAISE NOTICE 'Constraint FK % en la tabla %.% eliminado.', v_fk_mayus, p_esquema, v_tabla_mayus;
EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION 'Error al eliminar la FK % en la tabla %.%: %', v_fk_mayus, p_esquema, v_tabla_mayus, SQLERRM;
END;
$$;

-- =====================================================
-- PROCEDIMIENTO: renombrar_tabla
-- =====================================================
CREATE OR REPLACE PROCEDURE renombrar_tabla(
    p_esquema TEXT,
    p_nombre_actual TEXT,
    p_nombre_nuevo TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_nombre_actual_mayus TEXT := UPPER(p_nombre_actual);
    v_nombre_nuevo_mayus TEXT := UPPER(p_nombre_nuevo);
BEGIN
    EXECUTE format('ALTER TABLE IF EXISTS %I.%I RENAME TO %I', p_esquema, v_nombre_actual_mayus, v_nombre_nuevo_mayus);
    RAISE NOTICE 'Tabla %.% renombrada a %.%', p_esquema, v_nombre_actual_mayus, p_esquema, v_nombre_nuevo_mayus;
EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION 'Error al renombrar la tabla %.%: %', p_esquema, v_nombre_actual_mayus, SQLERRM;
END;
$$;

-- =====================================================
-- PROCEDIMIENTO: renombrar_columna
-- =====================================================
CREATE OR REPLACE PROCEDURE renombrar_columna(
    p_esquema TEXT,
    p_tabla TEXT,
    p_nombre_columna_actual TEXT,
    p_nombre_columna_nuevo TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_tabla_mayus TEXT := UPPER(p_tabla);
    v_columna_actual_mayus TEXT := UPPER(p_nombre_columna_actual);
    v_columna_nueva_mayus TEXT := UPPER(p_nombre_columna_nuevo);
BEGIN
    EXECUTE format('ALTER TABLE IF EXISTS %I.%I RENAME COLUMN %I TO %I', 
                   p_esquema, v_tabla_mayus, v_columna_actual_mayus, v_columna_nueva_mayus);
    RAISE NOTICE 'Columna %.%.% renombrada a %', p_esquema, v_tabla_mayus, v_columna_actual_mayus, v_columna_nueva_mayus;
EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION 'Error al renombrar la columna %.%.%: %', p_esquema, v_tabla_mayus, v_columna_actual_mayus, SQLERRM;
END;
$$;

-- =====================================================
-- PROCEDIMIENTO: modificar_tipo_columna
-- =====================================================
CREATE OR REPLACE PROCEDURE modificar_tipo_columna(
    p_esquema TEXT,
    p_tabla TEXT,
    p_columna TEXT,
    p_nuevo_tipo TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_tabla_mayus TEXT := UPPER(p_tabla);
    v_columna_mayus TEXT := UPPER(p_columna);
BEGIN
    EXECUTE format('ALTER TABLE IF EXISTS %I.%I ALTER COLUMN %I TYPE %s USING %I::%s',
                   p_esquema, v_tabla_mayus, v_columna_mayus, p_nuevo_tipo, v_columna_mayus, p_nuevo_tipo);
    RAISE NOTICE 'Tipo de la columna %.%.% modificado a %s.', p_esquema, v_tabla_mayus, v_columna_mayus, p_nuevo_tipo;
EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION 'Error al modificar el tipo de la columna %.%.%: %', p_esquema, v_tabla_mayus, v_columna_mayus, SQLERRM;
END;
$$;

-- =====================================================
-- PROCEDIMIENTO: agregar_constraint_check
-- =====================================================
CREATE OR REPLACE PROCEDURE agregar_constraint_check(
    p_esquema TEXT,
    p_tabla TEXT,
    p_nombre_constraint TEXT,
    p_condicion TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_tabla_mayus TEXT := UPPER(p_tabla);
    v_constraint_mayus TEXT := UPPER(p_nombre_constraint);
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints
        WHERE table_schema = p_esquema
          AND table_name = v_tabla_mayus
          AND constraint_name = v_constraint_mayus
    ) THEN
        EXECUTE format('ALTER TABLE %I.%I ADD CONSTRAINT %I CHECK (%s)',
                       p_esquema, v_tabla_mayus, v_constraint_mayus, p_condicion);
        RAISE NOTICE 'Constraint CHECK % agregado a la tabla %.%', v_constraint_mayus, p_esquema, v_tabla_mayus;
    ELSE
        RAISE NOTICE 'El constraint % ya existe en la tabla %.%', v_constraint_mayus, p_esquema, v_tabla_mayus;
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION 'Error al agregar constraint CHECK en %.%: %', p_esquema, v_tabla_mayus, SQLERRM;
END;
$$;

-- =====================================================
-- PROCEDIMIENTO: agregar_valor_default
-- =====================================================
CREATE OR REPLACE PROCEDURE agregar_valor_default(
    p_esquema TEXT,
    p_tabla TEXT,
    p_columna TEXT,
    p_valor_default TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_tabla_mayus TEXT := UPPER(p_tabla);
    v_columna_mayus TEXT := UPPER(p_columna);
BEGIN
    EXECUTE format('ALTER TABLE %I.%I ALTER COLUMN %I SET DEFAULT %s',
                   p_esquema, v_tabla_mayus, v_columna_mayus, p_valor_default);
    RAISE NOTICE 'Valor default % agregado a la columna %.%.%', p_valor_default, p_esquema, v_tabla_mayus, v_columna_mayus;
EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION 'Error al agregar valor default en %.%.%: %', p_esquema, v_tabla_mayus, v_columna_mayus, SQLERRM;
END;
$$;

-- =====================================================
-- PROCEDIMIENTO: quitar_valor_default
-- =====================================================
CREATE OR REPLACE PROCEDURE quitar_valor_default(
    p_esquema TEXT,
    p_tabla TEXT,
    p_columna TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_tabla_mayus TEXT := UPPER(p_tabla);
    v_columna_mayus TEXT := UPPER(p_columna);
BEGIN
    EXECUTE format('ALTER TABLE %I.%I ALTER COLUMN %I DROP DEFAULT',
                   p_esquema, v_tabla_mayus, v_columna_mayus);
    RAISE NOTICE 'Valor default eliminado de la columna %.%.%', p_esquema, v_tabla_mayus, v_columna_mayus;
EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION 'Error al quitar valor default de %.%.%: %', p_esquema, v_tabla_mayus, v_columna_mayus, SQLERRM;
END;
$$;

-- =====================================================
-- PROCEDIMIENTO: hacer_columna_nullable
-- =====================================================
CREATE OR REPLACE PROCEDURE hacer_columna_nullable(
    p_esquema TEXT,
    p_tabla TEXT,
    p_columna TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_tabla_mayus TEXT := UPPER(p_tabla);
    v_columna_mayus TEXT := UPPER(p_columna);
BEGIN
    EXECUTE format('ALTER TABLE %I.%I ALTER COLUMN %I DROP NOT NULL',
                   p_esquema, v_tabla_mayus, v_columna_mayus);
    RAISE NOTICE 'Columna %.%.% ahora permite valores NULL', p_esquema, v_tabla_mayus, v_columna_mayus;
EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION 'Error al modificar columna %.%.%: %', p_esquema, v_tabla_mayus, v_columna_mayus, SQLERRM;
END;
$$;

-- =====================================================
-- PROCEDIMIENTO: hacer_columna_not_null
-- =====================================================
CREATE OR REPLACE PROCEDURE hacer_columna_not_null(
    p_esquema TEXT,
    p_tabla TEXT,
    p_columna TEXT
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_tabla_mayus TEXT := UPPER(p_tabla);
    v_columna_mayus TEXT := UPPER(p_columna);
BEGIN
    EXECUTE format('ALTER TABLE %I.%I ALTER COLUMN %I SET NOT NULL',
                   p_esquema, v_tabla_mayus, v_columna_mayus);
    RAISE NOTICE 'Columna %.%.% ahora es NOT NULL', p_esquema, v_tabla_mayus, v_columna_mayus;
EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION 'Error al modificar columna %.%.%: %', p_esquema, v_tabla_mayus, v_columna_mayus, SQLERRM;
END;
$$;