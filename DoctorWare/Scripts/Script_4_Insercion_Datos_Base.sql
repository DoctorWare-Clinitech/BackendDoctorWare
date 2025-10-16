-- =============================================================
-- INSERCIÓN DE DATOS BASE CON CONTROL DE DUPLICADOS
-- =============================================================

-- =====================================
-- TIPOS_DOCUMENTO
-- =====================================
INSERT INTO public.tipos_documento (codigo, nombre)
SELECT 'DNI', 'Documento Nacional de Identidad' WHERE NOT EXISTS (SELECT 1 FROM public.tipos_documento WHERE codigo = 'DNI');
INSERT INTO public.tipos_documento (codigo, nombre)
SELECT 'LE', 'Libreta de Enrolamiento' WHERE NOT EXISTS (SELECT 1 FROM public.tipos_documento WHERE codigo = 'LE');
INSERT INTO public.tipos_documento (codigo, nombre)
SELECT 'LC', 'Libreta Cívica' WHERE NOT EXISTS (SELECT 1 FROM public.tipos_documento WHERE codigo = 'LC');
INSERT INTO public.tipos_documento (codigo, nombre)
SELECT 'CI', 'Cédula de Identidad' WHERE NOT EXISTS (SELECT 1 FROM public.tipos_documento WHERE codigo = 'CI');
INSERT INTO public.tipos_documento (codigo, nombre)
SELECT 'PAS', 'Pasaporte' WHERE NOT EXISTS (SELECT 1 FROM public.tipos_documento WHERE codigo = 'PAS');
INSERT INTO public.tipos_documento (codigo, nombre)
SELECT 'CUIL', 'Código Único de Identificación Laboral' WHERE NOT EXISTS (SELECT 1 FROM public.tipos_documento WHERE codigo = 'CUIL');
INSERT INTO public.tipos_documento (codigo, nombre)
SELECT 'CUIT', 'Código Único de Identificación Tributaria' WHERE NOT EXISTS (SELECT 1 FROM public.tipos_documento WHERE codigo = 'CUIT');
INSERT INTO public.tipos_documento (codigo, nombre)
SELECT 'RUN', 'Rol Único Nacional (Chile)' WHERE NOT EXISTS (SELECT 1 FROM public.tipos_documento WHERE codigo = 'RUN');
INSERT INTO public.tipos_documento (codigo, nombre)
SELECT 'CE', 'Carné de Extranjería' WHERE NOT EXISTS (SELECT 1 FROM public.tipos_documento WHERE codigo = 'CE');

-- =====================================
-- GENEROS
-- =====================================
INSERT INTO public.generos (nombre)
SELECT 'Masculino' WHERE NOT EXISTS (SELECT 1 FROM public.generos WHERE nombre = 'Masculino');
INSERT INTO public.generos (nombre)
SELECT 'Femenino' WHERE NOT EXISTS (SELECT 1 FROM public.generos WHERE nombre = 'Femenino');
INSERT INTO public.generos (nombre)
SELECT 'Otro' WHERE NOT EXISTS (SELECT 1 FROM public.generos WHERE nombre = 'Otro');
INSERT INTO public.generos (nombre)
SELECT 'No Binario' WHERE NOT EXISTS (SELECT 1 FROM public.generos WHERE nombre = 'No Binario');
INSERT INTO public.generos (nombre)
SELECT 'Prefiere no decirlo' WHERE NOT EXISTS (SELECT 1 FROM public.generos WHERE nombre = 'Prefiere no decirlo');

-- =====================================
-- ESTADOS_USUARIO
-- =====================================
INSERT INTO public.estados_usuario (nombre)
SELECT 'Activo' WHERE NOT EXISTS (SELECT 1 FROM public.estados_usuario WHERE nombre = 'Activo');
INSERT INTO public.estados_usuario (nombre)
SELECT 'Inactivo' WHERE NOT EXISTS (SELECT 1 FROM public.estados_usuario WHERE nombre = 'Inactivo');
INSERT INTO public.estados_usuario (nombre)
SELECT 'Pendiente de Confirmación' WHERE NOT EXISTS (SELECT 1 FROM public.estados_usuario WHERE nombre = 'Pendiente de Confirmación');
INSERT INTO public.estados_usuario (nombre)
SELECT 'Bloqueado' WHERE NOT EXISTS (SELECT 1 FROM public.estados_usuario WHERE nombre = 'Bloqueado');
INSERT INTO public.estados_usuario (nombre)
SELECT 'Suspendido' WHERE NOT EXISTS (SELECT 1 FROM public.estados_usuario WHERE nombre = 'Suspendido');
INSERT INTO public.estados_usuario (nombre)
SELECT 'Eliminado' WHERE NOT EXISTS (SELECT 1 FROM public.estados_usuario WHERE nombre = 'Eliminado');

-- =====================================
-- GRUPOS_SANGUINEOS
-- =====================================
INSERT INTO public.grupos_sanguineos (nombre)
SELECT 'A+' WHERE NOT EXISTS (SELECT 1 FROM public.grupos_sanguineos WHERE nombre = 'A+');
INSERT INTO public.grupos_sanguineos (nombre)
SELECT 'A-' WHERE NOT EXISTS (SELECT 1 FROM public.grupos_sanguineos WHERE nombre = 'A-');
INSERT INTO public.grupos_sanguineos (nombre)
SELECT 'B+' WHERE NOT EXISTS (SELECT 1 FROM public.grupos_sanguineos WHERE nombre = 'B+');
INSERT INTO public.grupos_sanguineos (nombre)
SELECT 'B-' WHERE NOT EXISTS (SELECT 1 FROM public.grupos_sanguineos WHERE nombre = 'B-');
INSERT INTO public.grupos_sanguineos (nombre)
SELECT 'AB+' WHERE NOT EXISTS (SELECT 1 FROM public.grupos_sanguineos WHERE nombre = 'AB+');
INSERT INTO public.grupos_sanguineos (nombre)
SELECT 'AB-' WHERE NOT EXISTS (SELECT 1 FROM public.grupos_sanguineos WHERE nombre = 'AB-');
INSERT INTO public.grupos_sanguineos (nombre)
SELECT 'O+' WHERE NOT EXISTS (SELECT 1 FROM public.grupos_sanguineos WHERE nombre = 'O+');
INSERT INTO public.grupos_sanguineos (nombre)
SELECT 'O-' WHERE NOT EXISTS (SELECT 1 FROM public.grupos_sanguineos WHERE nombre = 'O-');

-- =====================================
-- ESTADOS_TURNO
-- =====================================
INSERT INTO public.estados_turno (nombre)
SELECT 'Programado' WHERE NOT EXISTS (SELECT 1 FROM public.estados_turno WHERE nombre = 'Programado');
INSERT INTO public.estados_turno (nombre)
SELECT 'Confirmado' WHERE NOT EXISTS (SELECT 1 FROM public.estados_turno WHERE nombre = 'Confirmado');
INSERT INTO public.estados_turno (nombre)
SELECT 'Cancelado por Paciente' WHERE NOT EXISTS (SELECT 1 FROM public.estados_turno WHERE nombre = 'Cancelado por Paciente');
INSERT INTO public.estados_turno (nombre)
SELECT 'Cancelado por Profesional' WHERE NOT EXISTS (SELECT 1 FROM public.estados_turno WHERE nombre = 'Cancelado por Profesional');
INSERT INTO public.estados_turno (nombre)
SELECT 'Ausente' WHERE NOT EXISTS (SELECT 1 FROM public.estados_turno WHERE nombre = 'Ausente');
INSERT INTO public.estados_turno (nombre)
SELECT 'Atendido' WHERE NOT EXISTS (SELECT 1 FROM public.estados_turno WHERE nombre = 'Atendido');
INSERT INTO public.estados_turno (nombre)
SELECT 'Reprogramado' WHERE NOT EXISTS (SELECT 1 FROM public.estados_turno WHERE nombre = 'Reprogramado');
INSERT INTO public.estados_turno (nombre)
SELECT 'En Espera' WHERE NOT EXISTS (SELECT 1 FROM public.estados_turno WHERE nombre = 'En Espera');

-- =====================================
-- TIPOS_TURNO
-- =====================================
INSERT INTO public.tipos_turno (nombre)
SELECT 'Consulta' WHERE NOT EXISTS (SELECT 1 FROM public.tipos_turno WHERE nombre = 'Consulta');
INSERT INTO public.tipos_turno (nombre)
SELECT 'Estudio' WHERE NOT EXISTS (SELECT 1 FROM public.tipos_turno WHERE nombre = 'Estudio');
INSERT INTO public.tipos_turno (nombre)
SELECT 'Cirugía' WHERE NOT EXISTS (SELECT 1 FROM public.tipos_turno WHERE nombre = 'Cirugía');
INSERT INTO public.tipos_turno (nombre)
SELECT 'Seguimiento' WHERE NOT EXISTS (SELECT 1 FROM public.tipos_turno WHERE nombre = 'Seguimiento');
INSERT INTO public.tipos_turno (nombre)
SELECT 'Vacunación' WHERE NOT EXISTS (SELECT 1 FROM public.tipos_turno WHERE nombre = 'Vacunación');
INSERT INTO public.tipos_turno (nombre)
SELECT 'Control General' WHERE NOT EXISTS (SELECT 1 FROM public.tipos_turno WHERE nombre = 'Control General');

-- =====================================
-- MEDICAMENTOS
-- =====================================
INSERT INTO public.medicamentos (nombre_generico, nombre_comercial)
SELECT 'Ibuprofeno', 'Actron' WHERE NOT EXISTS (SELECT 1 FROM public.medicamentos WHERE nombre_generico = 'Ibuprofeno');
INSERT INTO public.medicamentos (nombre_generico, nombre_comercial)
SELECT 'Paracetamol', 'Tafirol' WHERE NOT EXISTS (SELECT 1 FROM public.medicamentos WHERE nombre_generico = 'Paracetamol');
INSERT INTO public.medicamentos (nombre_generico, nombre_comercial)
SELECT 'Amoxicilina', 'Amoxidal' WHERE NOT EXISTS (SELECT 1 FROM public.medicamentos WHERE nombre_generico = 'Amoxicilina');
INSERT INTO public.medicamentos (nombre_generico, nombre_comercial)
SELECT 'Loratadina', 'Clarityne' WHERE NOT EXISTS (SELECT 1 FROM public.medicamentos WHERE nombre_generico = 'Loratadina');
INSERT INTO public.medicamentos (nombre_generico, nombre_comercial)
SELECT 'Omeprazol', 'Gastec' WHERE NOT EXISTS (SELECT 1 FROM public.medicamentos WHERE nombre_generico = 'Omeprazol');
INSERT INTO public.medicamentos (nombre_generico, nombre_comercial)
SELECT 'Losartán', 'Cozaar' WHERE NOT EXISTS (SELECT 1 FROM public.medicamentos WHERE nombre_generico = 'Losartán');
INSERT INTO public.medicamentos (nombre_generico, nombre_comercial)
SELECT 'Metformina', 'Glucophage' WHERE NOT EXISTS (SELECT 1 FROM public.medicamentos WHERE nombre_generico = 'Metformina');
INSERT INTO public.medicamentos (nombre_generico, nombre_comercial)
SELECT 'Simvastatina', 'Zocor' WHERE NOT EXISTS (SELECT 1 FROM public.medicamentos WHERE nombre_generico = 'Simvastatina');
INSERT INTO public.medicamentos (nombre_generico, nombre_comercial)
SELECT 'Salbutamol', 'Ventolin' WHERE NOT EXISTS (SELECT 1 FROM public.medicamentos WHERE nombre_generico = 'Salbutamol');
INSERT INTO public.medicamentos (nombre_generico, nombre_comercial)
SELECT 'Clopidogrel', 'Plavix' WHERE NOT EXISTS (SELECT 1 FROM public.medicamentos WHERE nombre_generico = 'Clopidogrel');

-- =====================================
-- ALERGIAS
-- =====================================
INSERT INTO public.alergias (nombre)
SELECT 'Polen' WHERE NOT EXISTS (SELECT 1 FROM public.alergias WHERE nombre = 'Polen');
INSERT INTO public.alergias (nombre)
SELECT 'Ácaros del polvo' WHERE NOT EXISTS (SELECT 1 FROM public.alergias WHERE nombre = 'Ácaros del polvo');
INSERT INTO public.alergias (nombre)
SELECT 'Maní' WHERE NOT EXISTS (SELECT 1 FROM public.alergias WHERE nombre = 'Maní');
INSERT INTO public.alergias (nombre)
SELECT 'Mariscos' WHERE NOT EXISTS (SELECT 1 FROM public.alergias WHERE nombre = 'Mariscos');
INSERT INTO public.alergias (nombre)
SELECT 'Penicilina' WHERE NOT EXISTS (SELECT 1 FROM public.alergias WHERE nombre = 'Penicilina');
INSERT INTO public.alergias (nombre)
SELECT 'Látex' WHERE NOT EXISTS (SELECT 1 FROM public.alergias WHERE nombre = 'Látex');
INSERT INTO public.alergias (nombre)
SELECT 'Picaduras de Abeja' WHERE NOT EXISTS (SELECT 1 FROM public.alergias WHERE nombre = 'Picaduras de Abeja');
INSERT INTO public.alergias (nombre)
SELECT 'Frutilla' WHERE NOT EXISTS (SELECT 1 FROM public.alergias WHERE nombre = 'Frutilla');
INSERT INTO public.alergias (nombre)
SELECT 'Chocolate' WHERE NOT EXISTS (SELECT 1 FROM public.alergias WHERE nombre = 'Chocolate');

-- =====================================
-- CIRUGIAS
-- =====================================
INSERT INTO public.cirugias (nombre)
SELECT 'Apendicectomía' WHERE NOT EXISTS (SELECT 1 FROM public.cirugias WHERE nombre = 'Apendicectomía');
INSERT INTO public.cirugias (nombre)
SELECT 'Colecistectomía' WHERE NOT EXISTS (SELECT 1 FROM public.cirugias WHERE nombre = 'Colecistectomía');
INSERT INTO public.cirugias (nombre)
SELECT 'Hernioplastia' WHERE NOT EXISTS (SELECT 1 FROM public.cirugias WHERE nombre = 'Hernioplastia');
INSERT INTO public.cirugias (nombre)
SELECT 'Cesárea' WHERE NOT EXISTS (SELECT 1 FROM public.cirugias WHERE nombre = 'Cesárea');
INSERT INTO public.cirugias (nombre)
SELECT 'Reemplazo de cadera' WHERE NOT EXISTS (SELECT 1 FROM public.cirugias WHERE nombre = 'Reemplazo de cadera');
INSERT INTO public.cirugias (nombre)
SELECT 'Bypass gástrico' WHERE NOT EXISTS (SELECT 1 FROM public.cirugias WHERE nombre = 'Bypass gástrico');
INSERT INTO public.cirugias (nombre)
SELECT 'Cirugía de columna' WHERE NOT EXISTS (SELECT 1 FROM public.cirugias WHERE nombre = 'Cirugía de columna');
INSERT INTO public.cirugias (nombre)
SELECT 'Amigdalectomía' WHERE NOT EXISTS (SELECT 1 FROM public.cirugias WHERE nombre = 'Amigdalectomía');
INSERT INTO public.cirugias (nombre)
SELECT 'Cataratas' WHERE NOT EXISTS (SELECT 1 FROM public.cirugias WHERE nombre = 'Cataratas');

-- =====================================
-- CONDICIONES_CRONICAS
-- =====================================
INSERT INTO public.condiciones_cronicas (nombre)
SELECT 'Hipertensión Arterial' WHERE NOT EXISTS (SELECT 1 FROM public.condiciones_cronicas WHERE nombre = 'Hipertensión Arterial');
INSERT INTO public.condiciones_cronicas (nombre)
SELECT 'Diabetes Mellitus Tipo 2' WHERE NOT EXISTS (SELECT 1 FROM public.condiciones_cronicas WHERE nombre = 'Diabetes Mellitus Tipo 2');
INSERT INTO public.condiciones_cronicas (nombre)
SELECT 'Asma' WHERE NOT EXISTS (SELECT 1 FROM public.condiciones_cronicas WHERE nombre = 'Asma');
INSERT INTO public.condiciones_cronicas (nombre)
SELECT 'Hipotiroidismo' WHERE NOT EXISTS (SELECT 1 FROM public.condiciones_cronicas WHERE nombre = 'Hipotiroidismo');
INSERT INTO public.condiciones_cronicas (nombre)
SELECT 'Artritis Reumatoide' WHERE NOT EXISTS (SELECT 1 FROM public.condiciones_cronicas WHERE nombre = 'Artritis Reumatoide');
INSERT INTO public.condiciones_cronicas (nombre)
SELECT 'EPOC' WHERE NOT EXISTS (SELECT 1 FROM public.condiciones_cronicas WHERE nombre = 'EPOC');
INSERT INTO public.condiciones_cronicas (nombre)
SELECT 'Insuficiencia Cardíaca' WHERE NOT EXISTS (SELECT 1 FROM public.condiciones_cronicas WHERE nombre = 'Insuficiencia Cardíaca');
INSERT INTO public.condiciones_cronicas (nombre)
SELECT 'Enfermedad Celíaca' WHERE NOT EXISTS (SELECT 1 FROM public.condiciones_cronicas WHERE nombre = 'Enfermedad Celíaca');
INSERT INTO public.condiciones_cronicas (nombre)
SELECT 'Obesidad' WHERE NOT EXISTS (SELECT 1 FROM public.condiciones_cronicas WHERE nombre = 'Obesidad');
INSERT INTO public.condiciones_cronicas (nombre)
SELECT 'Depresión' WHERE NOT EXISTS (SELECT 1 FROM public.condiciones_cronicas WHERE nombre = 'Depresión');
