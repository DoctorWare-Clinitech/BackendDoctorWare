-- =============================================================
-- Script_7_Agenda_Profesionales_y_Ausencias.sql
-- 
-- Define tablas para:
--  - Configuración de horarios de atención por profesional
--  - Gestión de ausencias/bloqueos de agenda por profesional
--
-- Usa los helpers crear_tabla/crear_columna/crear_fk/crear_indice/agregar_versionado
-- definidos en Script_1_Creacion_SP_crear.sql
-- =============================================================

DO $$
BEGIN

    -- =========================================================
    -- AGENDA_PROFESIONALES
    -- Configuración recurrente de horarios por día de la semana
    -- =========================================================
    CALL crear_tabla('public', 'AGENDA_PROFESIONALES');

    -- Relación con PROFESIONALES
    CALL crear_fk('public', 'AGENDA_PROFESIONALES', 'PROFESIONALES');

    -- Día de la semana: 0=domingo, 1=lunes, ..., 6=sábado
    CALL crear_columna('public', 'AGENDA_PROFESIONALES', 'DIA_SEMANA', 'SMALLINT NOT NULL');

    -- Horario de atención para ese día
    CALL crear_columna('public', 'AGENDA_PROFESIONALES', 'HORA_INICIO', 'TIME NOT NULL');
    CALL crear_columna('public', 'AGENDA_PROFESIONALES', 'HORA_FIN', 'TIME NOT NULL');

    -- Duración opcional de turno para ese bloque (si es NULL se usa DURACION_TURNO_EN_MINUTOS del profesional)
    CALL crear_columna('public', 'AGENDA_PROFESIONALES', 'DURACION_MINUTOS', 'INTEGER');

    -- Permite marcar si el día es laborable o no (para feriados recurrentes, etc.)
    CALL crear_columna('public', 'AGENDA_PROFESIONALES', 'ES_LABORABLE', 'BOOLEAN DEFAULT TRUE');

    -- Descripción opcional del bloque (ej.: "Consultorio mañana", "Teleconsulta")
    CALL crear_columna('public', 'AGENDA_PROFESIONALES', 'DESCRIPCION', 'TEXT');

    -- Timestamps estándar (FECHA_CREACION, ULTIMA_ACTUALIZACION)
    CALL agregar_versionado('public', 'AGENDA_PROFESIONALES');

    -- Índice para búsquedas rápidas por profesional y día
    CALL crear_indice('public', 'AGENDA_PROFESIONALES', 'ID_PROFESIONALES,DIA_SEMANA', 'btree', FALSE);


    -- =========================================================
    -- AUSENCIAS_PROFESIONALES
    -- Bloqueo de fechas/rangos donde no se pueden agendar turnos
    -- =========================================================
    CALL crear_tabla('public', 'AUSENCIAS_PROFESIONALES');

    -- Relación con PROFESIONALES
    CALL crear_fk('public', 'AUSENCIAS_PROFESIONALES', 'PROFESIONALES');

    -- Rango de fechas de la ausencia (ambos inclusive)
    CALL crear_columna('public', 'AUSENCIAS_PROFESIONALES', 'FECHA_DESDE', 'DATE NOT NULL');
    CALL crear_columna('public', 'AUSENCIAS_PROFESIONALES', 'FECHA_HASTA', 'DATE NOT NULL');

    -- Opcional: rango horario dentro del día (si se quiere bloquear solo una franja)
    CALL crear_columna('public', 'AUSENCIAS_PROFESIONALES', 'HORA_DESDE', 'TIME');
    CALL crear_columna('public', 'AUSENCIAS_PROFESIONALES', 'HORA_HASTA', 'TIME');

    -- Tipo de ausencia (vacaciones, licencia, congreso, etc.)
    CALL crear_columna('public', 'AUSENCIAS_PROFESIONALES', 'TIPO', 'VARCHAR(50)');

    -- Motivo o comentario libre
    CALL crear_columna('public', 'AUSENCIAS_PROFESIONALES', 'MOTIVO', 'TEXT');

    -- Timestamps estándar
    CALL agregar_versionado('public', 'AUSENCIAS_PROFESIONALES');

    -- Índice para consultas por profesional y rango de fechas
    CALL crear_indice('public', 'AUSENCIAS_PROFESIONALES', 'ID_PROFESIONALES,FECHA_DESDE,FECHA_HASTA', 'btree', FALSE);

END;
$$;

