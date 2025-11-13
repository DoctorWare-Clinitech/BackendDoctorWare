-- =============================================================
-- INSERCIÓN DE ROLES (IDEMPOTENTE)
-- Basado en el estilo de Script_4_Insercion_Datos_Base
-- Tabla: public."ROLES" (NOMBRE, DESCRIPCION, ACTIVO)
-- =============================================================

-- ADMIN
INSERT INTO public."ROLES" ("NOMBRE", "DESCRIPCION", "ACTIVO")
SELECT 'ADMIN', 'Administrador del sistema', TRUE
WHERE NOT EXISTS (SELECT 1 FROM public."ROLES" WHERE "NOMBRE" = 'ADMIN');

-- SECRETARIO
INSERT INTO public."ROLES" ("NOMBRE", "DESCRIPCION", "ACTIVO")
SELECT 'SECRETARIO', 'Usuario de secretaría con gestión de turnos y pacientes', TRUE
WHERE NOT EXISTS (SELECT 1 FROM public."ROLES" WHERE "NOMBRE" = 'SECRETARIO');

-- PACIENTE
INSERT INTO public."ROLES" ("NOMBRE", "DESCRIPCION", "ACTIVO")
SELECT 'PACIENTE', 'Acceso a su perfil, turnos e historia clínica', TRUE
WHERE NOT EXISTS (SELECT 1 FROM public."ROLES" WHERE "NOMBRE" = 'PACIENTE');

-- PROFESIONAL
INSERT INTO public."ROLES" ("NOMBRE", "DESCRIPCION", "ACTIVO")
SELECT 'PROFESIONAL', 'Profesional de la salud con acceso clínico', TRUE
WHERE NOT EXISTS (SELECT 1 FROM public."ROLES" WHERE "NOMBRE" = 'PROFESIONAL');

