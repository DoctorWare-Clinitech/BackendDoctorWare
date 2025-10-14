
DO $$
BEGIN

    RAISE NOTICE 'Iniciando creación de tablas para DoctorWare...';

    -- TIPOS_DOCUMENTO
    CALL crear_tabla('public', 'TIPOS_DOCUMENTO');
    CALL crear_columna('public', 'TIPOS_DOCUMENTO', 'codigo', 'VARCHAR(20) NOT NULL UNIQUE');
    CALL crear_columna('public', 'TIPOS_DOCUMENTO', 'nombre', 'VARCHAR(100) NOT NULL');
    CALL agregar_versionado('public', 'TIPOS_DOCUMENTO');

    -- GENEROS
    CALL crear_tabla('public', 'GENEROS');
    CALL crear_columna('public', 'GENEROS', 'nombre', 'VARCHAR(100) NOT NULL UNIQUE');
    CALL agregar_versionado('public', 'GENEROS');


    -- ESTADOS_USUARIO
    CALL crear_tabla('public', 'ESTADOS_USUARIO');
    CALL crear_columna('public', 'ESTADOS_USUARIO', 'nombre', 'VARCHAR(50) NOT NULL UNIQUE');
    CALL agregar_versionado('public', 'ESTADOS_USUARIO');

    -- GRUPOS_SANGUINEOS
    CALL crear_tabla('public', 'GRUPOS_SANGUINEOS');
    CALL crear_columna('public', 'GRUPOS_SANGUINEOS', 'nombre', 'VARCHAR(10) NOT NULL UNIQUE');
    CALL agregar_versionado('public', 'GRUPOS_SANGUINEOS');

    -- ESTADOS_TURNO
    CALL crear_tabla('public', 'ESTADOS_TURNO');
    CALL crear_columna('public', 'ESTADOS_TURNO', 'nombre', 'VARCHAR(50) NOT NULL UNIQUE');
    CALL agregar_versionado('public', 'ESTADOS_TURNO');

    -- TIPOS_TURNO
    CALL crear_tabla('public', 'TIPOS_TURNO');
    CALL crear_columna('public', 'TIPOS_TURNO', 'nombre', 'VARCHAR(100) NOT NULL UNIQUE');
    CALL agregar_versionado('public', 'TIPOS_TURNO');

    -- MEDICAMENTOS
    CALL crear_tabla('public', 'MEDICAMENTOS');
    CALL crear_columna('public', 'MEDICAMENTOS', 'nombre_generico', 'VARCHAR(255) NOT NULL');
    CALL crear_columna('public', 'MEDICAMENTOS', 'nombre_comercial', 'VARCHAR(255)');
    CALL agregar_versionado('public', 'MEDICAMENTOS');

    -- ALERGIAS
    CALL crear_tabla('public', 'ALERGIAS');
    CALL crear_columna('public', 'ALERGIAS', 'nombre', 'VARCHAR(100) NOT NULL UNIQUE');
    CALL agregar_versionado('public', 'ALERGIAS');

    -- CIRUGIAS
    CALL crear_tabla('public', 'CIRUGIAS');
    CALL crear_columna('public', 'CIRUGIAS', 'nombre', 'VARCHAR(255) NOT NULL UNIQUE');
    CALL agregar_versionado('public', 'CIRUGIAS');

    -- CONDICIONES_CRONICAS
    CALL crear_tabla('public', 'CONDICIONES_CRONICAS');
    CALL crear_columna('public', 'CONDICIONES_CRONICAS', 'nombre', 'VARCHAR(255) NOT NULL UNIQUE');
    CALL agregar_versionado('public', 'CONDICIONES_CRONICAS');

    -- PERSONAS
    CALL crear_tabla('public', 'PERSONAS');
    CALL crear_columna('public', 'PERSONAS', 'nro_documento', 'BIGINT NOT NULL');
    CALL crear_columna('public', 'PERSONAS', 'nombre', 'VARCHAR(100) NOT NULL');
    CALL crear_columna('public', 'PERSONAS', 'apellido', 'VARCHAR(100) NOT NULL');
    CALL crear_columna('public', 'PERSONAS', 'fecha_nacimiento', 'DATE');
    CALL crear_columna('public', 'PERSONAS', 'foto', 'TEXT');
    CALL crear_columna('public', 'PERSONAS', 'email_principal', 'VARCHAR(255) UNIQUE');
    CALL crear_columna('public', 'PERSONAS', 'telefono_principal', 'VARCHAR(50)');
    CALL crear_columna('public', 'PERSONAS', 'celular_principal', 'VARCHAR(50)');
    CALL crear_columna('public', 'PERSONAS', 'calle', 'VARCHAR(255)');
    CALL crear_columna('public', 'PERSONAS', 'numero', 'VARCHAR(50)');
    CALL crear_columna('public', 'PERSONAS', 'piso', 'VARCHAR(20)');
    CALL crear_columna('public', 'PERSONAS', 'departamento', 'VARCHAR(20)');
    CALL crear_columna('public', 'PERSONAS', 'localidad', 'VARCHAR(100)');
    CALL crear_columna('public', 'PERSONAS', 'provincia', 'VARCHAR(100)');
    CALL crear_columna('public', 'PERSONAS', 'codigo_postal', 'VARCHAR(20)');
    CALL crear_columna('public', 'PERSONAS', 'pais', 'VARCHAR(100)');
    CALL crear_columna('public', 'PERSONAS', 'activo', 'BOOLEAN DEFAULT TRUE');
    CALL crear_fk('public', 'PERSONAS', 'TIPOS_DOCUMENTO');
    CALL crear_fk('public', 'PERSONAS', 'GENEROS');
    CALL agregar_versionado('public', 'PERSONAS');
    CALL crear_indice('public', 'PERSONAS', 'nro_documento,id_tipos_documento', 'btree', TRUE);

    -- USUARIOS
    CALL crear_tabla('public', 'USUARIOS');
    CALL crear_columna('public', 'USUARIOS', 'email', 'VARCHAR(255) NOT NULL UNIQUE');
    CALL crear_columna('public', 'USUARIOS', 'password_hash', 'TEXT NOT NULL');
    CALL crear_columna('public', 'USUARIOS', 'email_confirmado', 'BOOLEAN DEFAULT FALSE');
    CALL crear_columna('public', 'USUARIOS', 'telefono_confirmado', 'BOOLEAN DEFAULT FALSE');
    CALL crear_columna('public', 'USUARIOS', 'fecha_ultimo_acceso', 'TIMESTAMPTZ');
    CALL crear_columna('public', 'USUARIOS', 'intentos_fallidos', 'INTEGER DEFAULT 0');
    CALL crear_columna('public', 'USUARIOS', 'bloqueado_hasta', 'TIMESTAMPTZ');
    CALL crear_columna('public', 'USUARIOS', 'token_recuperacion', 'TEXT');
    CALL crear_columna('public', 'USUARIOS', 'token_expiracion', 'TIMESTAMPTZ');
    CALL crear_columna('public', 'USUARIOS', 'activo', 'BOOLEAN DEFAULT TRUE');
    CALL crear_fk('public', 'USUARIOS', 'PERSONAS');
    CALL crear_fk('public', 'USUARIOS', 'ESTADOS_USUARIO');
    CALL agregar_versionado('public', 'USUARIOS');
    CALL crear_indice('public', 'USUARIOS', 'id_personas', 'btree', TRUE);
    

    -- ROLES
    CALL crear_tabla('public', 'ROLES');
    CALL crear_columna('public', 'ROLES', 'nombre', 'VARCHAR(50) NOT NULL UNIQUE');
    CALL crear_columna('public', 'ROLES', 'descripcion', 'TEXT');
    CALL crear_columna('public', 'ROLES', 'activo', 'BOOLEAN DEFAULT TRUE');
    CALL agregar_versionado('public', 'ROLES');    

    -- USUARIOS_ROLES
    CALL crear_tabla('public', 'USUARIOS_ROLES');
    CALL crear_fk('public', 'USUARIOS_ROLES', 'USUARIOS');
    CALL crear_fk('public', 'USUARIOS_ROLES', 'ROLES');
    CALL agregar_versionado('public', 'USUARIOS_ROLES');
    CALL crear_indice('public', 'USUARIOS_ROLES', 'id_usuarios,id_roles', 'btree', TRUE);

    -- PROFESIONALES
    CALL crear_tabla('public', 'PROFESIONALES');
    CALL crear_columna('public', 'PROFESIONALES', 'matricula_nacional', 'VARCHAR(50) UNIQUE');
    CALL crear_columna('public', 'PROFESIONALES', 'matricula_provincial', 'VARCHAR(50) UNIQUE');
    CALL crear_columna('public', 'PROFESIONALES', 'especialidad', 'VARCHAR(100)');
    CALL crear_columna('public', 'PROFESIONALES', 'sub_especialidad', 'VARCHAR(100)');
    CALL crear_columna('public', 'PROFESIONALES', 'titulo', 'VARCHAR(255)');
    CALL crear_columna('public', 'PROFESIONALES', 'universidad', 'VARCHAR(255)');
    CALL crear_columna('public', 'PROFESIONALES', 'anio_egreso', 'INTEGER');
    CALL crear_columna('public', 'PROFESIONALES', 'cuit_cuil', 'VARCHAR(20) UNIQUE');
    CALL crear_columna('public', 'PROFESIONALES', 'duracion_turno_minutos', 'INTEGER DEFAULT 30');
    CALL crear_columna('public', 'PROFESIONALES', 'activo', 'BOOLEAN DEFAULT TRUE');
    CALL crear_fk('public', 'PROFESIONALES', 'PERSONAS');
    CALL agregar_versionado('public', 'PROFESIONALES');
    CALL crear_indice('public', 'PROFESIONALES', 'id_personas', 'btree', TRUE);

    -- PACIENTES
    CALL crear_tabla('public', 'PACIENTES');
    CALL crear_columna('public', 'PACIENTES', 'seguro_proveedor', 'VARCHAR(100)');
    CALL crear_columna('public', 'PACIENTES', 'seguro_plan', 'VARCHAR(100)');
    CALL crear_columna('public', 'PACIENTES', 'seguro_numero_afiliado', 'VARCHAR(50)');
    CALL crear_columna('public', 'PACIENTES', 'antecedentes_familiares', 'TEXT');
    CALL crear_columna('public', 'PACIENTES', 'contacto_emergencia_nombre', 'VARCHAR(200)');
    CALL crear_columna('public', 'PACIENTES', 'contacto_emergencia_telefono', 'VARCHAR(50)');
    CALL crear_columna('public', 'PACIENTES', 'contacto_emergencia_relacion', 'VARCHAR(50)');
    CALL crear_columna('public', 'PACIENTES', 'notas_generales', 'TEXT');
    CALL crear_columna('public', 'PACIENTES', 'activo', 'BOOLEAN DEFAULT TRUE');
    CALL crear_fk('public', 'PACIENTES', 'PERSONAS');
    CALL crear_fk('public', 'PACIENTES', 'GRUPOS_SANGUINEOS');
    CALL crear_fk('public', 'PACIENTES', 'PROFESIONALES');
    CALL agregar_versionado('public', 'PACIENTES');
    CALL crear_indice('public', 'PACIENTES', 'id_personas', 'btree', TRUE);

    -- SECRETARIOS
    CALL crear_tabla('public', 'SECRETARIOS');
    CALL crear_columna('public', 'SECRETARIOS', 'legajo', 'VARCHAR(50) UNIQUE');
    CALL crear_columna('public', 'SECRETARIOS', 'horario_entrada', 'TIME');
    CALL crear_columna('public', 'SECRETARIOS', 'horario_salida', 'TIME');
    CALL crear_columna('public', 'SECRETARIOS', 'activo', 'BOOLEAN DEFAULT TRUE');
    CALL crear_fk('public', 'SECRETARIOS', 'PERSONAS');
    CALL agregar_versionado('public', 'SECRETARIOS');
    CALL crear_indice('public', 'SECRETARIOS', 'id_personas', 'btree', TRUE);
   

    -- SECRETARIOS_PROFESIONALES
    CALL crear_tabla('public', 'SECRETARIOS_PROFESIONALES');
    CALL crear_fk('public', 'SECRETARIOS_PROFESIONALES', 'SECRETARIOS');
    CALL crear_fk('public', 'SECRETARIOS_PROFESIONALES', 'PROFESIONALES');
    CALL crear_columna('public', 'SECRETARIOS_PROFESIONALES', 'permisos', 'TEXT');
    CALL agregar_versionado('public', 'SECRETARIOS_PROFESIONALES');
    CALL crear_indice('public', 'SECRETARIOS_PROFESIONALES', 'id_secretarios,id_profesionales', 'btree', TRUE);

    -- TURNOS
    CALL crear_tabla('public', 'TURNOS');
    CALL crear_columna('public', 'TURNOS', 'fecha', 'DATE NOT NULL');
    CALL crear_columna('public', 'TURNOS', 'hora_inicio', 'TIME NOT NULL');
    CALL crear_columna('public', 'TURNOS', 'hora_fin', 'TIME NOT NULL');
    CALL crear_columna('public', 'TURNOS', 'duracion_minutos', 'INTEGER');
    CALL crear_columna('public', 'TURNOS', 'motivo_consulta', 'TEXT');
    CALL crear_columna('public', 'TURNOS', 'notas_adicionales', 'TEXT');
    CALL crear_columna('public', 'TURNOS', 'observaciones_profesional', 'TEXT');
    CALL crear_columna('public', 'TURNOS', 'fecha_cancelacion', 'TIMESTAMPTZ');
    CALL crear_columna('public', 'TURNOS', 'motivo_cancelacion', 'TEXT');
    CALL crear_fk('public', 'TURNOS', 'PACIENTES');
    CALL crear_fk('public', 'TURNOS', 'PROFESIONALES');
    CALL crear_fk('public', 'TURNOS', 'ESTADOS_TURNO');
    CALL crear_fk('public', 'TURNOS', 'TIPOS_TURNO');
    CALL crear_columna('public', 'turnos', 'id_usuario_creacion', 'INTEGER REFERENCES public.usuarios(id_usuarios)');
    CALL crear_columna('public', 'turnos', 'id_usuario_cancelacion', 'INTEGER REFERENCES public.usuarios(id_usuarios)');
    CALL agregar_versionado('public', 'TURNOS');

    -- HISTORIAS_CLINICAS
    CALL crear_tabla('public', 'HISTORIAS_CLINICAS');
    CALL crear_columna('public', 'HISTORIAS_CLINICAS', 'fecha', 'TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP');
    CALL crear_columna('public', 'HISTORIAS_CLINICAS', 'diagnostico', 'TEXT');
    CALL crear_columna('public', 'HISTORIAS_CLINICAS', 'sintomas', 'TEXT');
    CALL crear_columna('public', 'HISTORIAS_CLINICAS', 'tratamiento', 'TEXT');
    CALL crear_columna('public', 'HISTORIAS_CLINICAS', 'notas', 'TEXT');
    CALL crear_columna('public', 'HISTORIAS_CLINICAS', 'adjuntos_url', 'TEXT');
    CALL crear_fk('public', 'HISTORIAS_CLINICAS', 'PACIENTES');
    CALL crear_fk('public', 'HISTORIAS_CLINICAS', 'PROFESIONALES');
    CALL crear_fk('public', 'HISTORIAS_CLINICAS', 'TURNOS');
    CALL agregar_versionado('public', 'HISTORIAS_CLINICAS');    

    -- RECETAS
    CALL crear_tabla('public', 'RECETAS');
    CALL crear_columna('public', 'RECETAS', 'dosis', 'VARCHAR(255)');
    CALL crear_columna('public', 'RECETAS', 'frecuencia', 'VARCHAR(255)');
    CALL crear_columna('public', 'RECETAS', 'duracion', 'VARCHAR(100)');
    CALL crear_columna('public', 'RECETAS', 'instrucciones', 'TEXT');
    CALL crear_fk('public', 'RECETAS', 'HISTORIAS_CLINICAS');
    CALL crear_fk('public', 'RECETAS', 'MEDICAMENTOS');
    CALL agregar_versionado('public', 'RECETAS');

    -- PACIENTE_ALERGIAS
    CALL crear_tabla('public', 'PACIENTE_ALERGIAS');
    CALL crear_fk('public', 'PACIENTE_ALERGIAS', 'PACIENTES');
    CALL crear_fk('public', 'PACIENTE_ALERGIAS', 'ALERGIAS');
    CALL crear_columna('public', 'PACIENTE_ALERGIAS', 'detalles', 'TEXT');
    CALL agregar_versionado('public', 'PACIENTE_ALERGIAS');
    CALL crear_indice('public', 'PACIENTE_ALERGIAS', 'id_pacientes,id_alergias', 'btree', TRUE);

    -- PACIENTE_CIRUGIAS
    CALL crear_tabla('public', 'PACIENTE_CIRUGIAS');
    CALL crear_fk('public', 'PACIENTE_CIRUGIAS', 'PACIENTES');
    CALL crear_fk('public', 'PACIENTE_CIRUGIAS', 'CIRUGIAS');
    CALL crear_columna('public', 'PACIENTE_CIRUGIAS', 'fecha', 'DATE');
    CALL crear_columna('public', 'PACIENTE_CIRUGIAS', 'detalles', 'TEXT');
    CALL agregar_versionado('public', 'PACIENTE_CIRUGIAS');

    -- PACIENTE_CONDICIONES
    CALL crear_tabla('public', 'PACIENTE_CONDICIONES');
    CALL crear_fk('public', 'PACIENTE_CONDICIONES', 'PACIENTES');
    CALL crear_fk('public', 'PACIENTE_CONDICIONES', 'CONDICIONES_CRONICAS');
    CALL crear_columna('public', 'PACIENTE_CONDICIONES', 'detalles', 'TEXT');
    CALL agregar_versionado('public', 'PACIENTE_CONDICIONES');
END;
$$;
