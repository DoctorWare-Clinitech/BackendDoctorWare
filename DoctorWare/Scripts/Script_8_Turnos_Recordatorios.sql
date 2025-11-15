-- =============================================================
-- Script_8_Turnos_Recordatorios.sql
--
-- Agrega columnas a TURNOS para soportar recordatorios de turno
-- por email (MVP):
--   - RECORDATORIO_ENVIADO (BOOLEAN)
--   - FECHA_RECORDATORIO   (TIMESTAMPTZ)
--
-- Usa helpers de Script_1 (crear_columna).
-- =============================================================

DO $$
BEGIN
    -- Indicador de recordatorio enviado
    CALL crear_columna('public', 'TURNOS', 'RECORDATORIO_ENVIADO', 'BOOLEAN DEFAULT FALSE');

    -- Fecha/hora en la que se envió el recordatorio
    CALL crear_columna('public', 'TURNOS', 'FECHA_RECORDATORIO', 'TIMESTAMPTZ');
END;
$$;

