
DO $$
BEGIN

    -- TIPOS_DOCUMENTO
    CALL crear_tabla('public', 'TIPOS_DOCUMENTO');
    CALL crear_columna('public', 'TIPOS_DOCUMENTO', 'CODIGO', 'VARCHAR(20) NOT NULL UNIQUE');
    CALL crear_columna('public', 'TIPOS_DOCUMENTO', 'NOMBRE', 'VARCHAR(100) NOT NULL');
    CALL agregar_versionado('public', 'TIPOS_DOCUMENTO');

    -- GENEROS
    CALL crear_tabla('public', 'GENEROS');
    CALL crear_columna('public', 'GENEROS', 'NOMBRE', 'VARCHAR(100) NOT NULL UNIQUE');
    CALL agregar_versionado('public', 'GENEROS');


    -- ESTADOS_USUARIO
    CALL crear_tabla('public', 'ESTADOS_USUARIO');
    CALL crear_columna('public', 'ESTADOS_USUARIO', 'NOMBRE', 'VARCHAR(50) NOT NULL UNIQUE');
    CALL agregar_versionado('public', 'ESTADOS_USUARIO');

    -- GRUPOS_SANGUINEOS
    CALL crear_tabla('public', 'GRUPOS_SANGUINEOS');
    CALL crear_columna('public', 'GRUPOS_SANGUINEOS', 'NOMBRE', 'VARCHAR(10) NOT NULL UNIQUE');
    CALL agregar_versionado('public', 'GRUPOS_SANGUINEOS');

    -- ESTADOS_TURNO
    CALL crear_tabla('public', 'ESTADOS_TURNO');
    CALL crear_columna('public', 'ESTADOS_TURNO', 'NOMBRE', 'VARCHAR(50) NOT NULL UNIQUE');
    CALL agregar_versionado('public', 'ESTADOS_TURNO');

    -- TIPOS_TURNO
    CALL crear_tabla('public', 'TIPOS_TURNO');
    CALL crear_columna('public', 'TIPOS_TURNO', 'NOMBRE', 'VARCHAR(100) NOT NULL UNIQUE');
    CALL agregar_versionado('public', 'TIPOS_TURNO');

    -- MEDICAMENTOS
    CALL crear_tabla('public', 'MEDICAMENTOS');
    CALL crear_columna('public', 'MEDICAMENTOS', 'NOMBRE_GENERICO', 'VARCHAR(255) NOT NULL');
    CALL crear_columna('public', 'MEDICAMENTOS', 'NOMBRE_COMERCIAL', 'VARCHAR(255)');
    CALL agregar_versionado('public', 'MEDICAMENTOS');

    -- ALERGIAS
    CALL crear_tabla('public', 'ALERGIAS');
    CALL crear_columna('public', 'ALERGIAS', 'NOMBRE', 'VARCHAR(100) NOT NULL UNIQUE');
    CALL agregar_versionado('public', 'ALERGIAS');

    -- CIRUGIAS
    CALL crear_tabla('public', 'CIRUGIAS');
    CALL crear_columna('public', 'CIRUGIAS', 'NOMBRE', 'VARCHAR(255) NOT NULL UNIQUE');
    CALL agregar_versionado('public', 'CIRUGIAS');

    -- CONDICIONES_CRONICAS
    CALL crear_tabla('public', 'CONDICIONES_CRONICAS');
    CALL crear_columna('public', 'CONDICIONES_CRONICAS', 'NOMBRE', 'VARCHAR(255) NOT NULL UNIQUE');
    CALL agregar_versionado('public', 'CONDICIONES_CRONICAS');

    -- PERSONAS
    CALL crear_tabla('public', 'PERSONAS');
    CALL crear_columna('public', 'PERSONAS', 'NRO_DOCUMENTO', 'BIGINT NOT NULL');
    CALL crear_columna('public', 'PERSONAS', 'NOMBRE', 'VARCHAR(100) NOT NULL');
    CALL crear_columna('public', 'PERSONAS', 'APELLIDO', 'VARCHAR(100) NOT NULL');
    CALL crear_columna('public', 'PERSONAS', 'FECHA_NACIMIENTO', 'DATE');
    CALL crear_columna('public', 'PERSONAS', 'FOTO', 'TEXT');
    CALL crear_columna('public', 'PERSONAS', 'EMAIL_PRINCIPAL', 'VARCHAR(255) UNIQUE');
    CALL crear_columna('public', 'PERSONAS', 'TELEFONO_PRINCIPAL', 'VARCHAR(50)');
    CALL crear_columna('public', 'PERSONAS', 'CELULAR_PRINCIPAL', 'VARCHAR(50)');
    CALL crear_columna('public', 'PERSONAS', 'CALLE', 'VARCHAR(255)');
    CALL crear_columna('public', 'PERSONAS', 'NUMERO', 'VARCHAR(50)');
    CALL crear_columna('public', 'PERSONAS', 'PISO', 'VARCHAR(20)');
    CALL crear_columna('public', 'PERSONAS', 'DEPARTAMENTO', 'VARCHAR(20)');
    CALL crear_columna('public', 'PERSONAS', 'LOCALIDAD', 'VARCHAR(100)');
    CALL crear_columna('public', 'PERSONAS', 'PROVINCIA', 'VARCHAR(100)');
    CALL crear_columna('public', 'PERSONAS', 'CODIGO_POSTAL', 'VARCHAR(20)');
    CALL crear_columna('public', 'PERSONAS', 'PAIS', 'VARCHAR(100)');
    CALL crear_columna('public', 'PERSONAS', 'ACTIVO', 'BOOLEAN DEFAULT TRUE');
    CALL crear_fk('public', 'PERSONAS', 'TIPOS_DOCUMENTO');
    CALL crear_fk('public', 'PERSONAS', 'GENEROS');
    CALL agregar_versionado('public', 'PERSONAS');
    CALL crear_indice('public', 'PERSONAS', 'NRO_DOCUMENTO,ID_TIPOS_DOCUMENTO', 'btree', TRUE);

    -- USUARIOS
    CALL crear_tabla('public', 'USUARIOS');
    CALL crear_columna('public', 'USUARIOS', 'EMAIL', 'VARCHAR(255) NOT NULL UNIQUE');
    CALL crear_columna('public', 'USUARIOS', 'PASSWORD_HASH', 'TEXT NOT NULL');
    CALL crear_columna('public', 'USUARIOS', 'EMAIL_CONFIRMADO', 'BOOLEAN DEFAULT FALSE');
    CALL crear_columna('public', 'USUARIOS', 'TELEFONO_CONFIRMADO', 'BOOLEAN DEFAULT FALSE');
    CALL crear_columna('public', 'USUARIOS', 'FECHA_ULTIMO_ACCESO', 'TIMESTAMPTZ');
    CALL crear_columna('public', 'USUARIOS', 'INTENTOS_FALLIDOS', 'INTEGER DEFAULT 0');
    CALL crear_columna('public', 'USUARIOS', 'BLOQUEADO_HASTA', 'TIMESTAMPTZ');
    CALL crear_columna('public', 'USUARIOS', 'TOKEN_RECUPERACION', 'TEXT');
    CALL crear_columna('public', 'USUARIOS', 'TOKEN_EXPIRACION', 'TIMESTAMPTZ');
    CALL crear_columna('public', 'USUARIOS', 'ACTIVO', 'BOOLEAN DEFAULT TRUE');
    CALL crear_fk('public', 'USUARIOS', 'PERSONAS');
    CALL crear_fk('public', 'USUARIOS', 'ESTADOS_USUARIO');
    CALL agregar_versionado('public', 'USUARIOS');
    CALL crear_indice('public', 'USUARIOS', 'ID_PERSONAS', 'btree', TRUE);
    

    -- ROLES
    CALL crear_tabla('public', 'ROLES');
    CALL crear_columna('public', 'ROLES', 'NOMBRE', 'VARCHAR(50) NOT NULL UNIQUE');
    CALL crear_columna('public', 'ROLES', 'DESCRIPCION', 'TEXT');
    CALL crear_columna('public', 'ROLES', 'ACTIVO', 'BOOLEAN DEFAULT TRUE');
    CALL agregar_versionado('public', 'ROLES');

    -- USUARIOS_ROLES
    CALL crear_tabla('public', 'USUARIOS_ROLES');
    CALL crear_fk('public', 'USUARIOS_ROLES', 'USUARIOS');
    CALL crear_fk('public', 'USUARIOS_ROLES', 'ROLES');
    CALL agregar_versionado('public', 'USUARIOS_ROLES');
    CALL crear_indice('public', 'USUARIOS_ROLES', 'ID_USUARIOS,ID_ROLES', 'btree', TRUE);

    --ESPECIALIDADES
    CALL crear_tabla('public', 'ESPECIALIDADES');
    CALL crear_columna('public', 'ESPECIALIDADES', 'NOMBRE', 'VARCHAR(100)');
    CALL crear_columna('public', 'ESPECIALIDADES', 'DESCRIPCION', 'TEXT');
    CALL agregar_versionado('public', 'ESPECIALIDADES');

    --SUB_ESPECIALIDADES
    CALL crear_tabla('public', 'SUB_ESPECIALIDADES');
    CALL crear_columna('public', 'SUB_ESPECIALIDADES', 'NOMBRE', 'VARCHAR(100)');
    CALL crear_fk ('public', 'SUB_ESPECIALIDADES', 'ESPECIALIDADES');
    CALL agregar_versionado('public', 'ESPECIALIDADES');

    
    -- PROFESIONALES
    CALL crear_tabla('public', 'PROFESIONALES');
    CALL crear_columna('public', 'PROFESIONALES', 'MATRICULA_NACIONAL', 'VARCHAR(50) UNIQUE');
    CALL crear_columna('public', 'PROFESIONALES', 'MATRICULA_PROVINCIAL', 'VARCHAR(50) UNIQUE');
    CALL crear_columna('public', 'PROFESIONALES', 'TITULO', 'VARCHAR(255)');
    CALL crear_columna('public', 'PROFESIONALES', 'UNIVERSIDAD', 'VARCHAR(255)');
    CALL crear_columna('public', 'PROFESIONALES', 'ANIO_EGRESO', 'INTEGER');
    CALL crear_columna('public', 'PROFESIONALES', 'CUIT_CUIL', 'VARCHAR(20) UNIQUE');
    CALL crear_columna('public', 'PROFESIONALES', 'DURACION_TURNO_EN_MINUTOS', 'INTEGER DEFAULT 30');
    CALL crear_columna('public', 'PROFESIONALES', 'ACTIVO', 'BOOLEAN DEFAULT TRUE');
    CALL crear_fk('public', 'PROFESIONALES', 'PERSONAS');
    CALL agregar_versionado('public', 'PROFESIONALES');
    CALL crear_indice('public', 'PROFESIONALES', 'ID_PERSONAS', 'btree', TRUE);

    --PROFECIONAL_ESPECIALIDADES
    CALL crear_tabla('public', 'PROFESIONAL_ESPECIALIDADES');
    CALL crear_fk('public', 'PROFESIONAL_ESPECIALIDADES','PROFESIONALES');
    CALL crear_fk('public', 'PROFESIONAL_ESPECIALIDADES','ESPECIALIDADES');
    CALL agregar_versionado('public', 'PROFESIONAL_ESPECIALIDADES');


    -- PACIENTES
    CALL crear_tabla('public', 'PACIENTES');
    CALL crear_columna('public', 'PACIENTES', 'SEGURO_PROVEEDOR', 'VARCHAR(100)');
    CALL crear_columna('public', 'PACIENTES', 'SEGURO_PLAN', 'VARCHAR(100)');
    CALL crear_columna('public', 'PACIENTES', 'SEGURO_NUMERO_AFILIADO', 'VARCHAR(50)');
    CALL crear_columna('public', 'PACIENTES', 'ANTECEDENTES_FAMILIARES', 'TEXT');
    CALL crear_columna('public', 'PACIENTES', 'CONTACTO_EMERGENCIA_NOMBRE', 'VARCHAR(200)');
    CALL crear_columna('public', 'PACIENTES', 'CONTACTO_EMERGENCIA_TELEFONO', 'VARCHAR(50)');
    CALL crear_columna('public', 'PACIENTES', 'CONTACTO_EMERGENCIA_RELACION', 'VARCHAR(50)');
    CALL crear_columna('public', 'PACIENTES', 'NOTAS_GENERALES', 'TEXT');
    CALL crear_columna('public', 'PACIENTES', 'ACTIVO', 'BOOLEAN DEFAULT TRUE');
    CALL crear_fk('public', 'PACIENTES', 'PERSONAS');
    CALL crear_fk('public', 'PACIENTES', 'GRUPOS_SANGUINEOS');
    CALL crear_fk('public', 'PACIENTES', 'PROFESIONALES');
    CALL agregar_versionado('public', 'PACIENTES');
    CALL crear_indice('public', 'PACIENTES', 'ID_PERSONAS', 'btree', TRUE);

    -- SECRETARIOS
    CALL crear_tabla('public', 'SECRETARIOS');
    CALL crear_columna('public', 'SECRETARIOS', 'LEGAJO', 'VARCHAR(50) UNIQUE');
    CALL crear_columna('public', 'SECRETARIOS', 'HORARIO_ENTRADA', 'TIME');
    CALL crear_columna('public', 'SECRETARIOS', 'HORARIO_SALIDA', 'TIME');
    CALL crear_columna('public', 'SECRETARIOS', 'ACTIVO', 'BOOLEAN DEFAULT TRUE');
    CALL crear_fk('public', 'SECRETARIOS', 'PERSONAS');
    CALL agregar_versionado('public', 'SECRETARIOS');
    CALL crear_indice('public', 'SECRETARIOS', 'ID_PERSONAS', 'btree', TRUE);
   

    -- SECRETARIOS_PROFESIONALES
    CALL crear_tabla('public', 'SECRETARIOS_PROFESIONALES');
    CALL crear_fk('public', 'SECRETARIOS_PROFESIONALES', 'SECRETARIOS');
    CALL crear_fk('public', 'SECRETARIOS_PROFESIONALES', 'PROFESIONALES');
    CALL crear_columna('public', 'SECRETARIOS_PROFESIONALES', 'permisos', 'TEXT');
    CALL agregar_versionado('public', 'SECRETARIOS_PROFESIONALES');
    CALL crear_indice('public', 'SECRETARIOS_PROFESIONALES', 'ID_SECRETARIOS,ID_PROFESIONALES', 'btree', TRUE);

    -- TURNOS
    CALL crear_tabla('public', 'TURNOS');
    CALL crear_columna('public', 'TURNOS', 'FECHA', 'DATE NOT NULL');
    CALL crear_columna('public', 'TURNOS', 'HORA_INICIO', 'TIME NOT NULL');
    CALL crear_columna('public', 'TURNOS', 'HORA_FIN', 'TIME NOT NULL');
    CALL crear_columna('public', 'TURNOS', 'DURACION', 'INTEGER');
    CALL crear_columna('public', 'TURNOS', 'MOTIVO_CONSULTA', 'TEXT');
    CALL crear_columna('public', 'TURNOS', 'NOTA_ADICIONAL', 'TEXT');
    CALL crear_columna('public', 'TURNOS', 'OBSERVACION_PROFECIONAL', 'TEXT');
    CALL crear_columna('public', 'TURNOS', 'FECHA_CANCELACION', 'TIMESTAMPTZ');
    CALL crear_columna('public', 'TURNOS', 'MOTIVO_CANCELACION', 'TEXT');
    CALL crear_fk('public', 'TURNOS', 'PACIENTES');
    CALL crear_fk('public', 'TURNOS', 'PROFESIONALES');
    CALL crear_fk('public', 'TURNOS', 'ESTADOS_TURNO');
    CALL crear_fk('public', 'TURNOS', 'TIPOS_TURNO');
    CALL crear_columna('public', 'TURNOS', 'ID_USUARIO_CREACION', 'INTEGER REFERENCES public."USUARIOS"("ID_USUARIOS")');
    CALL crear_columna('public', 'TURNOS', 'ID_USUARIO_CANCELACION', 'INTEGER REFERENCES public."USUARIOS"("ID_USUARIOS")');
    CALL agregar_versionado('public', 'TURNOS');

    -- HISTORIAS_CLINICAS
    CALL crear_tabla('public', 'HISTORIAS_CLINICAS');
    CALL crear_columna('public', 'HISTORIAS_CLINICAS', 'FECHA', 'TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP');
    CALL crear_columna('public', 'HISTORIAS_CLINICAS', 'DIAGNOSTICO', 'TEXT');
    CALL crear_columna('public', 'HISTORIAS_CLINICAS', 'SINTOMA', 'TEXT');
    CALL crear_columna('public', 'HISTORIAS_CLINICAS', 'TRATAMIENTO', 'TEXT');
    CALL crear_columna('public', 'HISTORIAS_CLINICAS', 'NOTAS', 'TEXT');
    CALL crear_columna('public', 'HISTORIAS_CLINICAS', 'ADJUNTOS', 'TEXT');
    CALL crear_fk('public', 'HISTORIAS_CLINICAS', 'PACIENTES');
    CALL crear_fk('public', 'HISTORIAS_CLINICAS', 'PROFESIONALES');
    CALL crear_fk('public', 'HISTORIAS_CLINICAS', 'TURNOS');
    CALL agregar_versionado('public', 'HISTORIAS_CLINICAS');

    -- RECETAS
    CALL crear_tabla('public', 'RECETAS');
    CALL crear_columna('public', 'RECETAS', 'DOCIS', 'VARCHAR(255)');
    CALL crear_columna('public', 'RECETAS', 'FRECUENCIA', 'VARCHAR(255)');
    CALL crear_columna('public', 'RECETAS', 'DURACION', 'VARCHAR(100)');
    CALL crear_columna('public', 'RECETAS', 'INSTRUCCIONES', 'TEXT');
    CALL crear_fk('public', 'RECETAS', 'HISTORIAS_CLINICAS');
    CALL crear_fk('public', 'RECETAS', 'MEDICAMENTOS');
    CALL agregar_versionado('public', 'RECETAS');

    -- PACIENTE_ALERGIAS
    CALL crear_tabla('public', 'PACIENTE_ALERGIAS');
    CALL crear_fk('public', 'PACIENTE_ALERGIAS', 'PACIENTES');
    CALL crear_fk('public', 'PACIENTE_ALERGIAS', 'ALERGIAS');
    CALL crear_columna('public', 'PACIENTE_ALERGIAS', 'DETALLES', 'TEXT');
    CALL agregar_versionado('public', 'PACIENTE_ALERGIAS');
    CALL crear_indice('public', 'PACIENTE_ALERGIAS', 'ID_PACIENTES,ID_ALERGIAS', 'btree', TRUE);

    -- PACIENTE_CIRUGIAS
    CALL crear_tabla('public', 'PACIENTE_CIRUGIAS');
    CALL crear_fk('public', 'PACIENTE_CIRUGIAS', 'PACIENTES');
    CALL crear_fk('public', 'PACIENTE_CIRUGIAS', 'CIRUGIAS');
    CALL crear_columna('public', 'PACIENTE_CIRUGIAS', 'FECHA', 'DATE');
    CALL crear_columna('public', 'PACIENTE_CIRUGIAS', 'DETALLES', 'TEXT');
    CALL agregar_versionado('public', 'PACIENTE_CIRUGIAS');

    -- PACIENTE_CONDICIONES
    CALL crear_tabla('public', 'PACIENTE_CONDICIONES');
    CALL crear_fk('public', 'PACIENTE_CONDICIONES', 'PACIENTES');
    CALL crear_fk('public', 'PACIENTE_CONDICIONES', 'CONDICIONES_CRONICAS');
    CALL crear_columna('public', 'PACIENTE_CONDICIONES', 'DETALLES', 'TEXT');
    CALL agregar_versionado('public', 'PACIENTE_CONDICIONES');
END;
$$;
