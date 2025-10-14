# BackendDoctorWare

Backend para el sistema de gestión de consultorios médicos DoctorWare.
Desarrollado en C# con ASP.NET Core y PostgreSQL.

## Guía de Uso de Procedimientos Almacenados de Base de Datos

Este proyecto utiliza una serie de procedimientos almacenados (SPs) en PostgreSQL para estandarizar y simplificar la creación y modificación del esquema de la base de datos. A continuación se detalla cómo utilizar cada uno de ellos.

### 1. `crear_tabla`

Crea una nueva tabla en un esquema específico. Automáticamente añade una columna de clave primaria (PK) de tipo `SERIAL` llamada `id_<nombre_tabla>`.

**Parámetros:**
*   `p_esquema` (TEXT): El nombre del esquema (ej. 'public').
*   `p_tabla` (TEXT): El nombre de la tabla a crear.
*   `p_columnas_adicionales` (TEXT, opcional): Definiciones de columnas adicionales que se quieran añadir en la misma declaración `CREATE TABLE`.

**Ejemplo:**
```sql
-- Crea la tabla 'ROLES' en el esquema 'public'
CALL crear_tabla('public', 'ROLES');
```

### 2. `crear_columna`

Añade una nueva columna a una tabla existente.

**Parámetros:**
*   `p_esquema` (TEXT): El nombre del esquema.
*   `p_tabla` (TEXT): El nombre de la tabla.
*   `p_columna` (TEXT): El nombre de la nueva columna.
*   `p_definicion` (TEXT): La definición completa de la columna (tipo de dato, constraints, etc.).

**Ejemplo:**
```sql
-- Añade la columna 'nombre' a la tabla 'ROLES'
CALL crear_columna('public', 'ROLES', 'nombre', 'VARCHAR(50) NOT NULL UNIQUE');

-- Añade una columna con un valor por defecto
CALL crear_columna('public', 'ROLES', 'activo', 'BOOLEAN DEFAULT TRUE');
```

**Tipos de Datos Comunes en PostgreSQL:**

A continuación una lista de tipos de datos comunes que se pueden usar en el parámetro `p_definicion`:

*   **Tipos de Caracteres:**
    *   `VARCHAR(n)`: Cadena de caracteres de longitud variable con un límite `n`.
    *   `TEXT`: Cadena de caracteres de longitud ilimitada.

*   **Tipos Numéricos:**
    *   `INTEGER`: Número entero de 4 bytes.
    *   `BIGINT`: Número entero de 8 bytes (para números muy grandes).
    *   `NUMERIC(p, s)`: Número decimal exacto con `p` dígitos en total y `s` dígitos después del punto decimal.
    *   `SERIAL`: Alias de `INTEGER` autoincremental, ideal para claves primarias (aunque `crear_tabla` ya lo hace automáticamente).
    *   `BIGSERIAL`: Alias de `BIGINT` autoincremental.

*   **Tipos de Fecha y Hora:**
    *   `DATE`: Almacena solo la fecha.
    *   `TIME`: Almacena solo la hora del día.
    *   `TIMESTAMP`: Almacena fecha y hora, sin zona horaria.
    *   `TIMESTAMPTZ`: Almacena fecha y hora, con zona horaria (recomendado para servidores).

*   **Tipos Booleanos:**
    *   `BOOLEAN`: Almacena valores `TRUE` o `FALSE`.

*   **Otros Tipos Útiles:**
    *   `JSON`, `JSONB`: Para almacenar datos en formato JSON (`JSONB` es generalmente preferido).
    *   `UUID`: Para almacenar identificadores únicos universales.

### 3. `crear_fk`

Crea una columna de clave foránea (FK) y su respectiva constraint. El procedimiento determina automáticamente el nombre de la columna FK basándose en el nombre de la tabla padre (`id_<nombre_tabla_padre>`).

**Parámetros:**
*   `p_esquema` (TEXT): El nombre del esquema.
*   `p_tabla_hija` (TEXT): La tabla que contendrá la FK.
*   `p_tabla_padre` (TEXT): La tabla a la que se hará referencia.

**Ejemplo:**
```sql
-- En la tabla 'USUARIOS_ROLES', crea una FK 'id_usuarios' que referencia a la tabla 'USUARIOS'
CALL crear_fk('public', 'USUARIOS_ROLES', 'USUARIOS');

-- En la misma tabla, crea una FK 'id_roles' que referencia a la tabla 'ROLES'
CALL crear_fk('public', 'USUARIOS_ROLES', 'ROLES');
```
**Nota:** Si necesitas un nombre de columna de FK personalizado, debes crear la columna manualmente con `crear_columna` y la referencia incluida en la definición.
```sql
-- Ejemplo de FK con nombre personalizado
CALL crear_columna('public', 'pacientes', 'id_medico_cabecera', 'INTEGER REFERENCES public.profesionales(id_profesionales)');
```

### 4. `crear_indice`

Crea un índice en una o más columnas de una tabla.

**Parámetros:**
*   `p_esquema` (TEXT): El nombre del esquema.
*   `p_tabla` (TEXT): El nombre de la tabla.
*   `p_columnas` (TEXT): Nombres de las columnas para el índice, separadas por comas si es un índice compuesto.
*   `p_tipo_indice` (TEXT, opcional): El tipo de índice (ej. 'btree', 'hash'). Por defecto es 'btree'.
*   `p_unique` (BOOLEAN, opcional): Si es `TRUE`, crea un índice único. Por defecto es `FALSE`.

**Ejemplo:**
```sql
-- Crea un índice simple en la columna 'email' de la tabla 'USUARIOS'
CALL crear_indice('public', 'USUARIOS', 'email');

-- Crea un índice único y compuesto en la tabla 'USUARIOS_ROLES'
CALL crear_indice('public', 'USUARIOS_ROLES', 'id_usuarios,id_roles', 'btree', TRUE);
```

### 5. `agregar_versionado`

Añade columnas de auditoría (`fechacreacion`, `version`) y un trigger a una tabla.
*   `fechacreacion`: Se establece a la fecha y hora de inserción del registro.
*   `version`: Se actualiza automáticamente a la fecha y hora actual cada vez que el registro es modificado.

**Parámetros:**
*   `p_esquema` (TEXT): El nombre del esquema.
*   `p_tabla` (TEXT): El nombre de la tabla.

**Ejemplo:**
```sql
-- Agrega el control de versiones a la tabla 'ROLES'
CALL agregar_versionado('public', 'ROLES');
```

### Ejemplo Completo

A continuación, un ejemplo de cómo usar los procedimientos en conjunto para crear dos tablas relacionadas: `TIPOS_DOCUMENTO` y `PERSONAS`.

```sql
-- 1. Crear tabla de catálogo TIPOS_DOCUMENTO
CALL crear_tabla('public', 'TIPOS_DOCUMENTO');
CALL crear_columna('public', 'TIPOS_DOCUMENTO', 'codigo', 'VARCHAR(20) NOT NULL UNIQUE');
CALL crear_columna('public', 'TIPOS_DOCUMENTO', 'nombre', 'VARCHAR(100) NOT NULL');
CALL agregar_versionado('public', 'TIPOS_DOCUMENTO');
RAISE NOTICE 'Tabla TIPOS_DOCUMENTO creada.';

-- 2. Crear tabla PERSONAS
CALL crear_tabla('public', 'PERSONAS');
CALL crear_columna('public', 'PERSONAS', 'nro_documento', 'BIGINT NOT NULL');
CALL crear_columna('public', 'PERSONAS', 'nombre', 'VARCHAR(100) NOT NULL');
CALL crear_columna('public', 'PERSONAS', 'apellido', 'VARCHAR(100) NOT NULL');
CALL crear_columna('public', 'PERSONAS', 'activo', 'BOOLEAN DEFAULT TRUE');
CALL agregar_versionado('public', 'PERSONAS');

-- 3. Crear la relación (FK) entre PERSONAS y TIPOS_DOCUMENTO
CALL crear_fk('public', 'PERSONAS', 'TIPOS_DOCUMENTO');

-- 4. Crear un índice único para asegurar que no haya personas duplicadas con el mismo tipo y número de documento
CALL crear_indice('public', 'PERSONAS', 'nro_documento,id_tipos_documento', 'btree', TRUE);
RAISE NOTICE 'Tabla PERSONAS creada y relacionada.';
```