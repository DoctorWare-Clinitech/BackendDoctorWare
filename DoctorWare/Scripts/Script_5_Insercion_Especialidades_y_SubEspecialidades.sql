-- =============================================================
-- INSERCIÓN DE ESPECIALIDADES Y SUB-ESPECIALIDADES (IDEMPOTENTE)
-- Basado en el estilo de Script_4_Insercion_Datos_Base
-- Se asume PostgreSQL y tablas en esquema public con nombres en MAYÚSCULAS
-- Tablas: ESPECIALIDADES(NOMBRE, DESCRIPCION), SUB_ESPECIALIDADES(NOMBRE, ID_ESPECIALIDADES)
-- Nota: ID_ESPECIALIDADES es la FK a ESPECIALIDADES
-- =============================================================

-- =====================================
-- NORMALIZACIÓN ARGENTINA (NOMBRES CANÓNICOS)
-- Estandariza nombres previos si existen, evitando duplicados
-- =====================================
UPDATE public."ESPECIALIDADES" SET "NOMBRE" = 'Clínica Médica'
WHERE "NOMBRE" = 'Medicina Interna'
  AND NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Clínica Médica');

UPDATE public."ESPECIALIDADES" SET "NOMBRE" = 'Ortopedia y Traumatología'
WHERE "NOMBRE" = 'Traumatología y Ortopedia'
  AND NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Ortopedia y Traumatología');

UPDATE public."ESPECIALIDADES" SET "NOMBRE" = 'Medicina Familiar y General'
WHERE "NOMBRE" = 'Medicina Familiar'
  AND NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Medicina Familiar y General');

UPDATE public."ESPECIALIDADES" SET "NOMBRE" = 'Medicina del Deporte'
WHERE "NOMBRE" = 'Medicina Deportiva'
  AND NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Medicina del Deporte');

-- =====================================
-- ESPECIALIDADES (PADRES)
-- =====================================
INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Clínica Médica', 'Diagnóstico y tratamiento integral del adulto'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Clínica Médica');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Cardiología', 'Prevención, diagnóstico y tratamiento de enfermedades del corazón y vasos'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Cardiología');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Dermatología', 'Enfermedades de la piel, cabello y uñas'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Dermatología');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Pediatría', 'Atención integral de niños y adolescentes'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Pediatría');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Ginecología y Obstetricia', 'Salud integral de la mujer y embarazo'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Ginecología y Obstetricia');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Neurología', 'Trastornos del sistema nervioso central y periférico'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Neurología');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Ortopedia y Traumatología', 'Lesiones y enfermedades del aparato locomotor'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Ortopedia y Traumatología');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Oftalmología', 'Trastornos y cirugía del ojo'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Oftalmología');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Otorrinolaringología', 'Nariz, garganta, oído y estructuras relacionadas'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Otorrinolaringología');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Psiquiatría', 'Salud mental, trastornos afectivos y del comportamiento'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Psiquiatría');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Endocrinología', 'Trastornos hormonales y metabólicos'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Endocrinología');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Gastroenterología', 'Sistema digestivo y hepático'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Gastroenterología');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Neumonología', 'Trastornos respiratorios y del pulmón'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Neumonología');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Nefrología', 'Enfermedades renales y diálisis'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Nefrología');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Oncología', 'Diagnóstico y tratamiento del cáncer'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Oncología');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Reumatología', 'Trastornos reumatológicos y autoinmunes'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Reumatología');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Urología', 'Aparato urinario masculino y femenino; salud prostática'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Urología');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Cirugía General', 'Cirugía abdominal, pared abdominal y urgencias'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Cirugía General');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Medicina Familiar y General', 'Atención primaria, prevención y cuidados continuos'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Medicina Familiar y General');


-- Ampliación de ESPECIALIDADES adicionales
INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Alergia e Inmunología', 'Alergias, inmunodeficiencias e inmunoterapia'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Alergia e Inmunología');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Anestesiología', 'Anestesia, dolor agudo y cuidados perioperatorios'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Anestesiología');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Cirugía Cardiovascular', 'Cirugía del corazón y grandes vasos'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Cirugía Cardiovascular');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Cirugía Plástica', 'Cirugía reconstructiva y estética'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Cirugía Plástica');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Neurocirugía', 'Cirugía del sistema nervioso central y periférico'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Neurocirugía');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Diagnóstico por Imágenes', 'Radiología, eco, TAC, RMN e intervencionismo'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Diagnóstico por Imágenes');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Medicina Nuclear', 'Diagnóstico y terapia con radioisótopos'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Medicina Nuclear');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Hematología', 'Trastornos de la sangre y ganglios'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Hematología');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Infectología', 'Enfermedades infecciosas y control de infecciones'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Infectología');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Geriatría', 'Atención integral del adulto mayor'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Geriatría');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Medicina Física y Rehabilitación', 'Fisiatría y rehabilitación integral'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Medicina Física y Rehabilitación');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Medicina del Trabajo', 'Salud ocupacional y ergonomía'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Medicina del Trabajo');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Medicina del Deporte', 'Prevención y tratamiento en deportistas'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Medicina del Deporte');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Genética Médica', 'Diagnóstico y consejo genético, genómica clínica'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Genética Médica');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Toxicología', 'Toxicología clínica, ocupacional y ambiental'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Toxicología');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Medicina Legal', 'Pericias médicas y medicina forense'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Medicina Legal');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Angiología y Cirugía Vascular', 'Enfermedad vascular arterial y venosa'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Angiología y Cirugía Vascular');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Medicina Intensiva', 'Cuidados críticos del adulto'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Medicina Intensiva');

INSERT INTO public."ESPECIALIDADES" ("NOMBRE", "DESCRIPCION")
SELECT 'Nutrición', 'Nutrición clínica y soporte nutricional'
WHERE NOT EXISTS (SELECT 1 FROM public."ESPECIALIDADES" WHERE "NOMBRE" = 'Nutrición');

-- =====================================
-- SUB-ESPECIALIDADES (HIJAS)
-- Para idempotencia: verificación por (NOMBRE, ID_ESPECIALIDADES)
-- =====================================

-- NORMALIZACIÓN DE SUB_ESPECIALIDADES (Argentina)
-- Evita duplicados al renombrar; condiciona por especialidad
UPDATE public."SUB_ESPECIALIDADES" s
SET "NOMBRE" = 'Hemodinamia y Cardiología Intervencionista'
FROM public."ESPECIALIDADES" e
WHERE s."NOMBRE" = 'Cardiología Intervencionista'
  AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
  AND e."NOMBRE" = 'Cardiología'
  AND NOT EXISTS (
      SELECT 1 FROM public."SUB_ESPECIALIDADES" s2
      WHERE s2."NOMBRE" = 'Hemodinamia y Cardiología Intervencionista'
        AND s2."ID_ESPECIALIDADES" = s."ID_ESPECIALIDADES"
  );

UPDATE public."SUB_ESPECIALIDADES" s
SET "NOMBRE" = 'Electrofisiología y Marcapasos'
FROM public."ESPECIALIDADES" e
WHERE s."NOMBRE" = 'Electrofisiología'
  AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
  AND e."NOMBRE" = 'Cardiología'
  AND NOT EXISTS (
      SELECT 1 FROM public."SUB_ESPECIALIDADES" s2
      WHERE s2."NOMBRE" = 'Electrofisiología y Marcapasos'
        AND s2."ID_ESPECIALIDADES" = s."ID_ESPECIALIDADES"
  );

UPDATE public."SUB_ESPECIALIDADES" s
SET "NOMBRE" = 'Ecocardiografía'
FROM public."ESPECIALIDADES" e
WHERE s."NOMBRE" = 'Imagen Cardiovascular'
  AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
  AND e."NOMBRE" = 'Cardiología'
  AND NOT EXISTS (
      SELECT 1 FROM public."SUB_ESPECIALIDADES" s2
      WHERE s2."NOMBRE" = 'Ecocardiografía'
        AND s2."ID_ESPECIALIDADES" = s."ID_ESPECIALIDADES"
  );

UPDATE public."SUB_ESPECIALIDADES" s
SET "NOMBRE" = 'Insuficiencia Cardíaca y Trasplante'
FROM public."ESPECIALIDADES" e
WHERE s."NOMBRE" = 'Insuficiencia Cardíaca'
  AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
  AND e."NOMBRE" = 'Cardiología'
  AND NOT EXISTS (
      SELECT 1 FROM public."SUB_ESPECIALIDADES" s2
      WHERE s2."NOMBRE" = 'Insuficiencia Cardíaca y Trasplante'
        AND s2."ID_ESPECIALIDADES" = s."ID_ESPECIALIDADES"
  );

UPDATE public."SUB_ESPECIALIDADES" s
SET "NOMBRE" = 'Oncodermatología'
FROM public."ESPECIALIDADES" e
WHERE s."NOMBRE" = 'Dermato-Oncología'
  AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
  AND e."NOMBRE" = 'Dermatología'
  AND NOT EXISTS (
      SELECT 1 FROM public."SUB_ESPECIALIDADES" s2
      WHERE s2."NOMBRE" = 'Oncodermatología'
        AND s2."ID_ESPECIALIDADES" = s."ID_ESPECIALIDADES"
  );

UPDATE public."SUB_ESPECIALIDADES" s
SET "NOMBRE" = 'Dermatología Estética'
FROM public."ESPECIALIDADES" e
WHERE s."NOMBRE" = 'Estética'
  AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
  AND e."NOMBRE" = 'Dermatología'
  AND NOT EXISTS (
      SELECT 1 FROM public."SUB_ESPECIALIDADES" s2
      WHERE s2."NOMBRE" = 'Dermatología Estética'
        AND s2."ID_ESPECIALIDADES" = s."ID_ESPECIALIDADES"
  );

-- Cirugía Plástica: Estética -> Cirugía Estética
UPDATE public."SUB_ESPECIALIDADES" s
SET "NOMBRE" = 'Cirugía Estética'
FROM public."ESPECIALIDADES" e
WHERE s."NOMBRE" = 'Estética'
  AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
  AND e."NOMBRE" = 'Cirugía Plástica'
  AND NOT EXISTS (
      SELECT 1 FROM public."SUB_ESPECIALIDADES" s2
      WHERE s2."NOMBRE" = 'Cirugía Estética'
        AND s2."ID_ESPECIALIDADES" = s."ID_ESPECIALIDADES"
  );

UPDATE public."SUB_ESPECIALIDADES" s
SET "NOMBRE" = 'Córnea y Cirugía Refractiva'
FROM public."ESPECIALIDADES" e
WHERE s."NOMBRE" = 'Córnea y Quirúrgica Refractiva'
  AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
  AND e."NOMBRE" = 'Oftalmología'
  AND NOT EXISTS (
      SELECT 1 FROM public."SUB_ESPECIALIDADES" s2
      WHERE s2."NOMBRE" = 'Córnea y Cirugía Refractiva'
        AND s2."ID_ESPECIALIDADES" = s."ID_ESPECIALIDADES"
  );

UPDATE public."SUB_ESPECIALIDADES" s
SET "NOMBRE" = 'Alergia Otorrinolaringológica'
FROM public."ESPECIALIDADES" e
WHERE s."NOMBRE" = 'Alergia ORL'
  AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
  AND e."NOMBRE" = 'Otorrinolaringología'
  AND NOT EXISTS (
      SELECT 1 FROM public."SUB_ESPECIALIDADES" s2
      WHERE s2."NOMBRE" = 'Alergia Otorrinolaringológica'
        AND s2."ID_ESPECIALIDADES" = s."ID_ESPECIALIDADES"
  );

UPDATE public."SUB_ESPECIALIDADES" s
SET "NOMBRE" = 'Medicina del Sueño (ORL)'
FROM public."ESPECIALIDADES" e
WHERE s."NOMBRE" = 'Trastornos del Sueño (ORL)'
  AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
  AND e."NOMBRE" = 'Otorrinolaringología'
  AND NOT EXISTS (
      SELECT 1 FROM public."SUB_ESPECIALIDADES" s2
      WHERE s2."NOMBRE" = 'Medicina del Sueño (ORL)'
        AND s2."ID_ESPECIALIDADES" = s."ID_ESPECIALIDADES"
  );

UPDATE public."SUB_ESPECIALIDADES" s
SET "NOMBRE" = 'Trastornos Cognitivos y Demencias'
FROM public."ESPECIALIDADES" e
WHERE s."NOMBRE" = 'Demencias y Cognición'
  AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
  AND e."NOMBRE" = 'Neurología'
  AND NOT EXISTS (
      SELECT 1 FROM public."SUB_ESPECIALIDADES" s2
      WHERE s2."NOMBRE" = 'Trastornos Cognitivos y Demencias'
        AND s2."ID_ESPECIALIDADES" = s."ID_ESPECIALIDADES"
  );

UPDATE public."SUB_ESPECIALIDADES" s
SET "NOMBRE" = 'Radioterapia Oncológica'
FROM public."ESPECIALIDADES" e
WHERE s."NOMBRE" = 'Oncología Radioterápica'
  AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
  AND e."NOMBRE" = 'Oncología'
  AND NOT EXISTS (
      SELECT 1 FROM public."SUB_ESPECIALIDADES" s2
      WHERE s2."NOMBRE" = 'Radioterapia Oncológica'
        AND s2."ID_ESPECIALIDADES" = s."ID_ESPECIALIDADES"
  );

-- Cardiología
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Hemodinamia y Cardiología Intervencionista', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Cardiología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Hemodinamia y Cardiología Intervencionista'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Electrofisiología y Marcapasos', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Cardiología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Electrofisiología y Marcapasos'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Insuficiencia Cardíaca y Trasplante', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Cardiología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Insuficiencia Cardíaca y Trasplante'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Ecocardiografía', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Cardiología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Ecocardiografía'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Cardiología Preventiva', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Cardiología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Cardiología Preventiva'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Rehabilitación Cardiovascular', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Cardiología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Rehabilitación Cardiovascular'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Cardiopatías Congénitas del Adulto', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Cardiología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Cardiopatías Congénitas del Adulto'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Cardio-Oncología', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Cardiología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Cardio-Oncología'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Cardiología Estructural', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Cardiología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Cardiología Estructural'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Cardiología Deportiva', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Cardiología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Cardiología Deportiva'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Dermatología
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Dermatología Pediátrica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Dermatología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Dermatología Pediátrica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Oncodermatología', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Dermatología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Oncodermatología'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Tricología', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Dermatología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Tricología'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Cirugía Dermatológica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Dermatología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Cirugía Dermatológica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Dermatopatología', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Dermatología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Dermatopatología'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Inmunodermatología', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Dermatología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Inmunodermatología'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Dermatología Estética', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Dermatología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Dermatología Estética'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Psoriasis y Enfermedades Inflamatorias', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Dermatología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Psoriasis y Enfermedades Inflamatorias'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Fotodermatología', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Dermatología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Fotodermatología'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Pediatría
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Neonatología', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Pediatría'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Neonatología'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Gastroenterología Pediátrica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Pediatría'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Gastroenterología Pediátrica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Infectología Pediátrica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Pediatría'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Infectología Pediátrica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Nefrología Pediátrica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Pediatría'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Nefrología Pediátrica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Neumonología Pediátrica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Pediatría'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Neumonología Pediátrica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Endocrinología Pediátrica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Pediatría'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Endocrinología Pediátrica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Neurología Pediátrica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Pediatría'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Neurología Pediátrica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Hemato-Oncología Pediátrica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Pediatría'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Hemato-Oncología Pediátrica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Terapia Intensiva Pediátrica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Pediatría'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Terapia Intensiva Pediátrica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Ginecología y Obstetricia
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Medicina Materno-Fetal', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Ginecología y Obstetricia'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Medicina Materno-Fetal'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Reproducción Asistida', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Ginecología y Obstetricia'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Reproducción Asistida'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Ginecología Oncológica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Ginecología y Obstetricia'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Ginecología Oncológica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Uroginecología', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Ginecología y Obstetricia'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Uroginecología'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Endocrinología Reproductiva', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Ginecología y Obstetricia'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Endocrinología Reproductiva'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Ginecología Infanto-Juvenil', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Ginecología y Obstetricia'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Ginecología Infanto-Juvenil'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Patología Cervical y Colposcopía', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Ginecología y Obstetricia'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Patología Cervical y Colposcopía'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Neurología
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Epilepsia', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Neurología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Epilepsia'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Neurología Vascular', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Neurología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Neurología Vascular'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Neuroinmunología', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Neurología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Neuroinmunología'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Trastornos del Movimiento', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Neurología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Trastornos del Movimiento'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Cefaleas', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Neurología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Cefaleas'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Trastornos Cognitivos y Demencias', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Neurología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Trastornos Cognitivos y Demencias'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Neuromuscular', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Neurología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Neuromuscular'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Neurofisiología Clínica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Neurología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Neurofisiología Clínica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Medicina del Sueño (Neurología)', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Neurología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Medicina del Sueño (Neurología)'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Ortopedia y Traumatología
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Columna', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Ortopedia y Traumatología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Columna'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Rodilla', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Ortopedia y Traumatología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Rodilla'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Hombro', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Ortopedia y Traumatología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Hombro'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Codo', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Ortopedia y Traumatología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Codo'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Cadera y Pelvis', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Ortopedia y Traumatología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Cadera y Pelvis'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Mano', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Ortopedia y Traumatología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Mano'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Pie y Tobillo', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Ortopedia y Traumatología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Pie y Tobillo'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Ortopedia Infantil', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Ortopedia y Traumatología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Ortopedia Infantil'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Oncológica (Ortopédica)', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Ortopedia y Traumatología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Oncológica (Ortopédica)'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Traumatología del Deporte', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Ortopedia y Traumatología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Traumatología del Deporte'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Artroplastia', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Ortopedia y Traumatología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Artroplastia'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Oftalmología
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Retina y Vítreo', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Oftalmología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Retina y Vítreo'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Glaucoma', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Oftalmología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Glaucoma'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Córnea y Cirugía Refractiva', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Oftalmología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Córnea y Cirugía Refractiva'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Oculoplastia y Órbita', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Oftalmología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Oculoplastia y Órbita'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Estrabismo', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Oftalmología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Estrabismo'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Uveítis', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Oftalmología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Uveítis'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Baja Visión', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Oftalmología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Baja Visión'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Neurooftalmología', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Oftalmología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Neurooftalmología'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Clínica Médica
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Medicina Hospitalaria', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Clínica Médica'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Medicina Hospitalaria'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Medicina Ambulatoria', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Clínica Médica'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Medicina Ambulatoria'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Medicina Perioperatoria', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Clínica Médica'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Medicina Perioperatoria'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Medicina del Viajero', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Clínica Médica'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Medicina del Viajero'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Otorrinolaringología
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Rinología', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Otorrinolaringología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Rinología'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Laringología', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Otorrinolaringología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Laringología'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Otología', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Otorrinolaringología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Otología'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Cirugía de Cabeza y Cuello', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Otorrinolaringología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Cirugía de Cabeza y Cuello'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Alergia Otorrinolaringológica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Otorrinolaringología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Alergia Otorrinolaringológica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Medicina del Sueño (ORL)', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Otorrinolaringología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Medicina del Sueño (ORL)'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Foniatría y Voz', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Otorrinolaringología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Foniatría y Voz'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Psiquiatría
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Psiquiatría Infanto-Juvenil', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Psiquiatría'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Psiquiatría Infanto-Juvenil'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Psiquiatría Geriátrica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Psiquiatría'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Psiquiatría Geriátrica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Adicciones', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Psiquiatría'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Adicciones'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Enlace e Interconsulta', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Psiquiatría'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Enlace e Interconsulta'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Urgencias Psiquiátricas', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Psiquiatría'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Urgencias Psiquiátricas'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Trastornos del Ánimo', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Psiquiatría'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Trastornos del Ánimo'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Trastornos de Ansiedad', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Psiquiatría'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Trastornos de Ansiedad'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Psiquiatría Forense', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Psiquiatría'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Psiquiatría Forense'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Endocrinología
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Diabetes y Metabolismo', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Endocrinología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Diabetes y Metabolismo'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Patología Tiroidea', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Endocrinología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Patología Tiroidea'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Neuroendocrinología', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Endocrinología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Neuroendocrinología'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Metabolismo Óseo', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Endocrinología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Metabolismo Óseo'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Obesidad', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Endocrinología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Obesidad'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Lípidos', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Endocrinología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Lípidos'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Gastroenterología
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Hepatología', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Gastroenterología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Hepatología'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Endoscopia Digestiva', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Gastroenterología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Endoscopia Digestiva'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Enfermedad Inflamatoria Intestinal', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Gastroenterología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Enfermedad Inflamatoria Intestinal'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Motilidad Digestiva', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Gastroenterología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Motilidad Digestiva'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Pancreatología', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Gastroenterología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Pancreatología'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Esófago y ERGE', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Gastroenterología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Esófago y ERGE'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Neumonología
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Medicina del Sueño', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Neumonología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Medicina del Sueño'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Asma y EPOC', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Neumonología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Asma y EPOC'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Enfermedad Intersticial Pulmonar', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Neumonología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Enfermedad Intersticial Pulmonar'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Hipertensión Pulmonar', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Neumonología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Hipertensión Pulmonar'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Broncoscopia Intervencionista', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Neumonología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Broncoscopia Intervencionista'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Nefrología
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Trasplante Renal', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Nefrología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Trasplante Renal'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Hemodiálisis', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Nefrología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Hemodiálisis'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Diálisis Peritoneal', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Nefrología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Diálisis Peritoneal'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Oncología
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Oncología Clínica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Oncología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Oncología Clínica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Radioterapia Oncológica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Oncología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Radioterapia Oncológica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Oncología de Cabeza y Cuello', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Oncología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Oncología de Cabeza y Cuello'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Oncología Pediátrica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Oncología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Oncología Pediátrica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Oncología Torácica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Oncología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Oncología Torácica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Oncología Digestiva', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Oncología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Oncología Digestiva'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Oncología Genitourinaria', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Oncología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Oncología Genitourinaria'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Oncología Ginecológica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Oncología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Oncología Ginecológica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Neurooncología', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Oncología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Neurooncología'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Oncología Cutánea', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Oncología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Oncología Cutánea'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Sarcomas', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Oncología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Sarcomas'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Reumatología
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Reumatología Pediátrica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Reumatología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Reumatología Pediátrica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Enfermedades Autoinmunes Sistémicas', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Reumatología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Enfermedades Autoinmunes Sistémicas'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Vasculitis', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Reumatología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Vasculitis'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Osteoporosis y Metabolismo Óseo', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Reumatología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Osteoporosis y Metabolismo Óseo'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Urología
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Endourología', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Urología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Endourología'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Uro-Oncología', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Urología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Uro-Oncología'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Andrología', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Urología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Andrología'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Urología Femenina y Uroginecología', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Urología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Urología Femenina y Uroginecología'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Litiasis', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Urología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Litiasis'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Reconstructiva', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Urología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Reconstructiva'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Neurourología', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Urología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Neurourología'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Urología Pediátrica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Urología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Urología Pediátrica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Cirugía General
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Coloproctología', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Cirugía General'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Coloproctología'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Cirugía Bariátrica y Metabólica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Cirugía General'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Cirugía Bariátrica y Metabólica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'HPB (Hepato-Pancreato-Biliar)', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Cirugía General'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'HPB (Hepato-Pancreato-Biliar)'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Pared Abdominal y Hernias', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Cirugía General'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Pared Abdominal y Hernias'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Esofagogástrica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Cirugía General'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Esofagogástrica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Cirugía Mínimamente Invasiva', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Cirugía General'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Cirugía Mínimamente Invasiva'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Trauma y Urgencias', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Cirugía General'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Trauma y Urgencias'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Medicina Familiar
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Cuidados Paliativos', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Medicina Familiar y General'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Cuidados Paliativos'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Salud Comunitaria', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Medicina Familiar y General'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Salud Comunitaria'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Medicina del Adulto Mayor', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Medicina Familiar y General'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Medicina del Adulto Mayor'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Salud Materno-Infantil', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Medicina Familiar y General'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Salud Materno-Infantil'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Medicina del Adolescente', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Medicina Familiar y General'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Medicina del Adolescente'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Alergia e Inmunología
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Alergia Pediátrica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Alergia e Inmunología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Alergia Pediátrica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Inmunodeficiencias Primarias', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Alergia e Inmunología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Inmunodeficiencias Primarias'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Alergia Alimentaria', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Alergia e Inmunología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Alergia Alimentaria'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Urticaria y Angioedema', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Alergia e Inmunología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Urticaria y Angioedema'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Anestesiología
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Anestesia Cardiovascular', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Anestesiología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Anestesia Cardiovascular'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Neuroanestesia', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Anestesiología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Neuroanestesia'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Anestesia Obstétrica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Anestesiología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Anestesia Obstétrica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Anestesia Pediátrica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Anestesiología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Anestesia Pediátrica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Cirugía Cardiovascular
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Cirugía Aórtica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Cirugía Cardiovascular'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Cirugía Aórtica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Cirugía Coronaria', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Cirugía Cardiovascular'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Cirugía Coronaria'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Cirugía Plástica
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Reconstructiva', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Cirugía Plástica'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Reconstructiva'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Cirugía Estética', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Cirugía Plástica'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Cirugía Estética'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Neurocirugía
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Columna (Neurocirugía)', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Neurocirugía'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Columna (Neurocirugía)'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Base de Cráneo', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Neurocirugía'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Base de Cráneo'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Diagnóstico por Imágenes
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Radiología Intervencionista', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Diagnóstico por Imágenes'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Radiología Intervencionista'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Neurorradiología', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Diagnóstico por Imágenes'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Neurorradiología'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Radiología Musculoesquelética', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Diagnóstico por Imágenes'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Radiología Musculoesquelética'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Medicina Nuclear
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'PET/CT', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Medicina Nuclear'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'PET/CT'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Terapia con Radioisótopos', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Medicina Nuclear'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Terapia con Radioisótopos'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Hematología
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Hemostasia y Trombosis', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Hematología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Hemostasia y Trombosis'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Trasplante de Médula Ósea', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Hematología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Trasplante de Médula Ósea'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Infectología
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'VIH/SIDA', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Infectología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'VIH/SIDA'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Infecciones Nosocomiales', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Infectología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Infecciones Nosocomiales'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Enfermedades Tropicales', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Infectología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Enfermedades Tropicales'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Geriatría
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Fragilidad y Caídas', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Geriatría'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Fragilidad y Caídas'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Deterioro Cognitivo', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Geriatría'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Deterioro Cognitivo'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Medicina Física y Rehabilitación
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Rehabilitación Neurológica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Medicina Física y Rehabilitación'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Rehabilitación Neurológica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Rehabilitación Musculoesquelética', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Medicina Física y Rehabilitación'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Rehabilitación Musculoesquelética'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Medicina del Trabajo
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Ergonomía y Riesgo Laboral', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Medicina del Trabajo'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Ergonomía y Riesgo Laboral'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Medicina del Deporte
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Traumatología del Deporte', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Medicina del Deporte'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Traumatología del Deporte'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Genética Médica
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Genética Clínica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Genética Médica'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Genética Clínica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Consejo Genético', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Genética Médica'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Consejo Genético'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Toxicología
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Toxicología Clínica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Toxicología'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Toxicología Clínica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Medicina Legal
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Forense Clínica', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Medicina Legal'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Forense Clínica'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Angiología y Cirugía Vascular
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Endovascular', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Angiología y Cirugía Vascular'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Endovascular'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Venosa y Linfática', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Angiología y Cirugía Vascular'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Venosa y Linfática'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Medicina Intensiva
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Neurointensivo', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Medicina Intensiva'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Neurointensivo'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Cardiovascular (UCO/UTI)', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Medicina Intensiva'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Cardiovascular (UCO/UTI)'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);

-- Nutrición
INSERT INTO public."SUB_ESPECIALIDADES" ("NOMBRE", "ID_ESPECIALIDADES")
SELECT 'Soporte Nutricional', e."ID_ESPECIALIDADES"
FROM public."ESPECIALIDADES" e
WHERE e."NOMBRE" = 'Nutrición'
AND NOT EXISTS (
    SELECT 1 FROM public."SUB_ESPECIALIDADES" s
    WHERE s."NOMBRE" = 'Soporte Nutricional'
      AND s."ID_ESPECIALIDADES" = e."ID_ESPECIALIDADES"
);
