using Dapper;
using DoctorWare.Data.Interfaces;
using DoctorWare.DTOs.Requests.Patients;
using DoctorWare.DTOs.Response.Patients;
using DoctorWare.Services.Interfaces;
using System.Data;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DoctorWare.Services.Implementation
{
    public class PatientsService : DoctorWare.Services.Interfaces.IPatientsService
    {
        private readonly IDbConnectionFactory factory;
        private readonly IDataProtectionService dataProtectionService;

        public PatientsService(IDbConnectionFactory factory, IDataProtectionService dataProtectionService)
        {
            this.factory = factory;
            this.dataProtectionService = dataProtectionService;
        }

        public async Task<IEnumerable<PatientDto>> GetAsync(string? name, string? dni, string? email, string? phone, string? professionalUserId, bool? isActive, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();

            int? idProfesional = null;
            if (!string.IsNullOrWhiteSpace(professionalUserId))
            {
                int uidParsed;
                if (int.TryParse(professionalUserId, out uidParsed))
                {
                const string sqlProf = @"select p.""ID_PROFESIONALES"" from public.""PROFESIONALES"" p
                                          join public.""USUARIOS"" u on u.""ID_PERSONAS"" = p.""ID_PERSONAS""
                                         where u.""ID_USUARIOS"" = @uid limit 1";
                    idProfesional = await con.ExecuteScalarAsync<int?>(sqlProf, new { uid = uidParsed });
                }
            }

            List<string> filters = new List<string>();
            DynamicParameters p = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(name))
            {
                filters.Add("(upper(per.\"NOMBRE\") like upper(@q) or upper(per.\"APELLIDO\") like upper(@q))");
                p.Add("q", $"%{name}%");
            }
            if (!string.IsNullOrWhiteSpace(dni))
            {
                long ndocParsed;
                if (long.TryParse(dni, out ndocParsed))
                {
                    filters.Add("per.\"NRO_DOCUMENTO\" = @dni");
                    p.Add("dni", ndocParsed);
                }
            }
            if (!string.IsNullOrWhiteSpace(email))
            {
                filters.Add("upper(per.\"EMAIL_PRINCIPAL\") like upper(@em)");
                p.Add("em", $"%{email}%");
            }
            if (!string.IsNullOrWhiteSpace(phone))
            {
                filters.Add("(per.\"TELEFONO_PRINCIPAL\" like @ph or per.\"CELULAR_PRINCIPAL\" like @ph)");
                p.Add("ph", $"%{phone}%");
            }
            if (idProfesional.HasValue)
            {
                filters.Add("pa.\"ID_PROFESIONALES\" = @idProf");
                p.Add("idProf", idProfesional.Value);
            }
            if (isActive.HasValue)
            {
                filters.Add("pa.\"ACTIVO\" = @act");
                p.Add("act", isActive.Value);
            }

            string where = filters.Count > 0 ? (" where " + string.Join(" and ", filters)) : string.Empty;

            string sql = $@"
                select
                    pa.""ID_PACIENTES"" as IdPaciente,
                    per.""ID_PERSONAS"" as IdPersona,
                    per.""NOMBRE"", per.""APELLIDO"",
                    per.""EMAIL_PRINCIPAL"", coalesce(per.""CELULAR_PRINCIPAL"", per.""TELEFONO_PRINCIPAL"") as Telefono,
                    per.""NRO_DOCUMENTO"", per.""FECHA_NACIMIENTO"",
                    gen.""NOMBRE"" as Genero,
                    per.""CALLE"", per.""NUMERO"", per.""LOCALIDAD"", per.""PROVINCIA"", per.""CODIGO_POSTAL"", per.""PAIS"",
                    pa.""CONTACTO_EMERGENCIA_NOMBRE"", pa.""CONTACTO_EMERGENCIA_TELEFONO"", pa.""CONTACTO_EMERGENCIA_RELACION"",
                    gs.""NOMBRE"" as GrupoSanguineo,
                    pa.""SEGURO_PROVEEDOR"", pa.""SEGURO_PLAN"", pa.""SEGURO_NUMERO_AFILIADO"",
                    pa.""ID_PROFESIONALES"" as IdProf,
                    pa.""NOTAS_GENERALES"", pa.""ACTIVO"",
                    pa.""FECHA_CREACION"", pa.""ULTIMA_ACTUALIZACION"",
                    u.""ID_USUARIOS"" as IdUsuarioProf
                from public.""PACIENTES"" pa
                join public.""PERSONAS"" per on per.""ID_PERSONAS"" = pa.""ID_PERSONAS""
                left join public.""GENEROS"" gen on gen.""ID_GENEROS"" = per.""ID_GENEROS""
                left join public.""GRUPOS_SANGUINEOS"" gs on gs.""ID_GRUPOS_SANGUINEOS"" = pa.""ID_GRUPOS_SANGUINEOS""
                left join public.""PROFESIONALES"" prof on prof.""ID_PROFESIONALES"" = pa.""ID_PROFESIONALES""
                left join public.""USUARIOS"" u on u.""ID_PERSONAS"" = prof.""ID_PERSONAS""{where}
                order by per.""APELLIDO"", per.""NOMBRE""";

            IEnumerable<dynamic> rows = await con.QueryAsync(sql, p);

            // Obtener datos médicos relacionados (alergias, condiciones, cirugías) por lote
            int[] idsPacientes = rows.Select(r => (int)r.IdPaciente).ToArray();
            Dictionary<int, List<string>> alergiasPorPaciente = await LoadAlergiasAsync(con, idsPacientes);
            Dictionary<int, List<string>> condicionesPorPaciente = await LoadCondicionesAsync(con, idsPacientes);
            Dictionary<int, List<string>> cirugiasPorPaciente = await LoadCirugiasAsync(con, idsPacientes);

            IEnumerable<PatientDto> list = rows.Select(r => new PatientDto
            {
                Id = Convert.ToString(r.IdPaciente),
                UserId = null,
                Name = $"{(string)r.NOMBRE} {(string)r.APELLIDO}".Trim(),
                Email = r.EMAIL_PRINCIPAL as string,
                Phone = r.Telefono as string ?? string.Empty,
                Dni = Convert.ToString(r.NRO_DOCUMENTO),
                DateOfBirth = r.FECHA_NACIMIENTO as DateTime?,
                Gender = MapGenero((string?)r.Genero),
                Address = new AddressDto
                {
                    Street = CombineStreet(r.CALLE as string, r.NUMERO as string),
                    City = r.LOCALIDAD as string ?? string.Empty,
                    State = r.PROVINCIA as string ?? string.Empty,
                    ZipCode = r.CODIGO_POSTAL as string ?? string.Empty,
                    Country = r.PAIS as string ?? string.Empty
                },
                EmergencyContact = new EmergencyContactDto
                {
                    Name = DecryptNullable(r.CONTACTO_EMERGENCIA_NOMBRE as string) ?? string.Empty,
                    Phone = DecryptNullable(r.CONTACTO_EMERGENCIA_TELEFONO as string) ?? string.Empty,
                    Relationship = DecryptNullable(r.CONTACTO_EMERGENCIA_RELACION as string) ?? string.Empty
                },
                MedicalInsurance = (r.SEGURO_PROVEEDOR == null && r.SEGURO_PLAN == null && r.SEGURO_NUMERO_AFILIADO == null)
                    ? null
                    : new MedicalInsuranceDto
                    {
                        Provider = r.SEGURO_PROVEEDOR as string ?? string.Empty,
                        PlanName = r.SEGURO_PLAN as string ?? string.Empty,
                        MemberNumber = r.SEGURO_NUMERO_AFILIADO as string ?? string.Empty,
                        ValidUntil = null
                    },
                MedicalInfo = new MedicalInfoDto
                {
                    BloodType = r.GrupoSanguineo as string,
                    Allergies = alergiasPorPaciente.TryGetValue((int)r.IdPaciente, out List<string> als) ? als : new List<string>(),
                    ChronicConditions = condicionesPorPaciente.TryGetValue((int)r.IdPaciente, out List<string> cc) ? cc : new List<string>(),
                    CurrentMedications = new List<string>(),
                    Surgeries = cirugiasPorPaciente.TryGetValue((int)r.IdPaciente, out List<string> cir) ? cir : new List<string>(),
                    FamilyHistory = null
                },
                ProfessionalId = Convert.ToString(r.IdUsuarioProf ?? r.IdProf),
                Notes = DecryptNullable(r.NOTAS_GENERALES as string),
                IsActive = (bool)r.ACTIVO,
                CreatedAt = (DateTime)r.FECHA_CREACION,
                UpdatedAt = (DateTime)r.ULTIMA_ACTUALIZACION
            });

            return list;
        }

        public async Task<PatientDto?> GetByIdAsync(string id, CancellationToken ct)
        {
            IEnumerable<PatientDto> list = await GetAsync(null, null, null, null, null, null, ct);
            return list.FirstOrDefault(x => x.Id == id);
        }

        public async Task<PatientDto> CreateAsync(CreatePatientRequest request, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();
            using IDbTransaction tx = con.BeginTransaction();

            // Insert PERSONAS
            const string insertPer = @"
                insert into public.""PERSONAS"" (
                    ""NRO_DOCUMENTO"", ""NOMBRE"", ""APELLIDO"", ""FECHA_NACIMIENTO"",
                    ""EMAIL_PRINCIPAL"", ""TELEFONO_PRINCIPAL"", ""CELULAR_PRINCIPAL"",
                    ""CALLE"", ""NUMERO"", ""LOCALIDAD"", ""PROVINCIA"", ""CODIGO_POSTAL"", ""PAIS"",
                    ""ACTIVO"", ""ID_TIPOS_DOCUMENTO"", ""ID_GENEROS""
                ) values (
                    @dni, @nombre, @apellido, @fechaNacimiento,
                    @email, @telefono, @celular,
                    @calle, @numero, @localidad, @provincia, @cp, @pais,
                    true, (select ""ID_TIPOS_DOCUMENTO"" from public.""TIPOS_DOCUMENTO"" where ""CODIGO"" = 'DNI' limit 1),
                    (select ""ID_GENEROS"" from public.""GENEROS"" where ""NOMBRE"" = @generoDb limit 1)
                ) returning ""ID_PERSONAS""";

            string[] names = SplitName(request.Name);
            string generoDb = MapGeneroToDb(request.Gender);
            long dParsed;
            long dni = long.TryParse(request.Dni, out dParsed) ? dParsed : 0L;

            int idPersona = await con.ExecuteScalarAsync<int>(insertPer, new
            {
                dni,
                nombre = names[0],
                apellido = names[1],
                fechaNacimiento = request.DateOfBirth,
                email = request.Email,
                telefono = request.Phone,
                celular = request.Phone,
                calle = request.Address.Street,
                numero = ExtractStreetNumber(request.Address.Street, request.Address.ZipCode),
                localidad = request.Address.City,
                provincia = request.Address.State,
                cp = request.Address.ZipCode,
                pais = request.Address.Country,
                generoDb
            }, tx);

            // Resolver profesional
            int? idProfesional = null;
            if (!string.IsNullOrWhiteSpace(request.ProfessionalId))
            {
                int uid;
                if (int.TryParse(request.ProfessionalId, out uid))
                {
                    const string sqlProf = @"select p.""ID_PROFESIONALES"" from public.""PROFESIONALES"" p
                                          join public.""USUARIOS"" u on u.""ID_PERSONAS"" = p.""ID_PERSONAS""
                                         where u.""ID_USUARIOS"" = @uid limit 1";
                    idProfesional = await con.ExecuteScalarAsync<int?>(sqlProf, new { uid }, tx);
                }
            }

            // Sanguíneo por defecto 'O+'
            int idGrupo = await con.ExecuteScalarAsync<int>("select \"ID_GRUPOS_SANGUINEOS\" from public.\"GRUPOS_SANGUINEOS\" where \"NOMBRE\" = 'O+' limit 1", transaction: tx);

            // Insert PACIENTES
            const string insertPac = @"
                insert into public.""PACIENTES"" (
                    ""SEGURO_PROVEEDOR"", ""SEGURO_PLAN"", ""SEGURO_NUMERO_AFILIADO"",
                    ""CONTACTO_EMERGENCIA_NOMBRE"", ""CONTACTO_EMERGENCIA_TELEFONO"", ""CONTACTO_EMERGENCIA_RELACION"",
                    ""NOTAS_GENERALES"", ""ACTIVO"", ""ID_PERSONAS"", ""ID_GRUPOS_SANGUINEOS"", ""ID_PROFESIONALES""
                ) values (
                    @segProv, @segPlan, @segNum,
                    @ecn, @ect, @ecr,
                    @notas, true, @idPer, @idGs, @idProf
                ) returning ""ID_PACIENTES""";

            int idPaciente = await con.ExecuteScalarAsync<int>(insertPac, new
            {
                segProv = request.MedicalInsurance?.Provider,
                segPlan = request.MedicalInsurance?.PlanName,
                segNum = request.MedicalInsurance?.MemberNumber,
                ecn = EncryptNullable(request.EmergencyContact.Name),
                ect = EncryptNullable(request.EmergencyContact.Phone),
                ecr = EncryptNullable(request.EmergencyContact.Relationship),
                notas = EncryptNullable(request.Notes),
                idPer = idPersona,
                idGs = idGrupo,
                idProf = idProfesional
            }, tx);

            tx.Commit();

            PatientDto? created = await GetByIdAsync(idPaciente.ToString(), ct) ?? throw new InvalidOperationException("Patient not found after create");
            return created;
        }

        public async Task<PatientDto> UpdateAsync(string id, UpdatePatientRequest request, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();

            // Obtener ids relacionados
            const string getIds = @"select pa.""ID_PERSONAS"" as IdPer from public.""PACIENTES"" pa where pa.""ID_PACIENTES"" = @id limit 1";
            int? idPer = await con.ExecuteScalarAsync<int?>(getIds, new { id = int.Parse(id) });
            if (!idPer.HasValue)
            {
                throw new KeyNotFoundException("Paciente no encontrado");
            }

            // Update PERSONAS
            List<string> setPer = new List<string>();
            DynamicParameters pPer = new DynamicParameters();
            pPer.Add("idPer", idPer.Value);
            if (request.Name != null)
            {
                string[] names = SplitName(request.Name);
                setPer.Add("\"NOMBRE\" = @n");
                setPer.Add("\"APELLIDO\" = @a");
                pPer.Add("n", names[0]);
                pPer.Add("a", names[1]);
            }
            if (request.Email != null)
            {
                setPer.Add("\"EMAIL_PRINCIPAL\" = @em"); pPer.Add("em", request.Email);
            }
            if (request.Phone != null)
            {
                setPer.Add("\"CELULAR_PRINCIPAL\" = @ph"); pPer.Add("ph", request.Phone);
                setPer.Add("\"TELEFONO_PRINCIPAL\" = @ph");
            }
            if (request.Address != null)
            {
                setPer.Add("\"CALLE\" = @calle"); pPer.Add("calle", request.Address.Street);
                setPer.Add("\"NUMERO\" = @num"); pPer.Add("num", ExtractStreetNumber(request.Address.Street, request.Address.ZipCode));
                setPer.Add("\"LOCALIDAD\" = @loc"); pPer.Add("loc", request.Address.City);
                setPer.Add("\"PROVINCIA\" = @prov"); pPer.Add("prov", request.Address.State);
                setPer.Add("\"CODIGO_POSTAL\" = @cp"); pPer.Add("cp", request.Address.ZipCode);
                setPer.Add("\"PAIS\" = @pais"); pPer.Add("pais", request.Address.Country);
            }
            if (setPer.Count > 0)
            {
                string sqlPer = $"update public.\"PERSONAS\" set {string.Join(", ", setPer)}, \"ULTIMA_ACTUALIZACION\" = NOW() where \"ID_PERSONAS\" = @idPer";
                await con.ExecuteAsync(sqlPer, pPer);
            }

            // Update PACIENTES
            List<string> setPa = new List<string>();
            DynamicParameters pPa = new DynamicParameters();
            pPa.Add("id", int.Parse(id));
            if (request.EmergencyContact != null)
            {
                setPa.Add("\"CONTACTO_EMERGENCIA_NOMBRE\" = @ecn"); pPa.Add("ecn", EncryptNullable(request.EmergencyContact.Name));
                setPa.Add("\"CONTACTO_EMERGENCIA_TELEFONO\" = @ect"); pPa.Add("ect", EncryptNullable(request.EmergencyContact.Phone));
                setPa.Add("\"CONTACTO_EMERGENCIA_RELACION\" = @ecr"); pPa.Add("ecr", EncryptNullable(request.EmergencyContact.Relationship));
            }
            if (request.MedicalInsurance != null)
            {
                setPa.Add("\"SEGURO_PROVEEDOR\" = @sp"); pPa.Add("sp", request.MedicalInsurance.Provider);
                setPa.Add("\"SEGURO_PLAN\" = @pl"); pPa.Add("pl", request.MedicalInsurance.PlanName);
                setPa.Add("\"SEGURO_NUMERO_AFILIADO\" = @num"); pPa.Add("num", request.MedicalInsurance.MemberNumber);
            }
            if (request.Notes != null)
            {
                setPa.Add("\"NOTAS_GENERALES\" = @notes"); pPa.Add("notes", EncryptNullable(request.Notes));
            }
            if (request.IsActive.HasValue)
            {
                setPa.Add("\"ACTIVO\" = @act"); pPa.Add("act", request.IsActive.Value);
            }
            if (setPa.Count > 0)
            {
                string sqlPa = $"update public.\"PACIENTES\" set {string.Join(", ", setPa)}, \"ULTIMA_ACTUALIZACION\" = NOW() where \"ID_PACIENTES\" = @id";
                await con.ExecuteAsync(sqlPa, pPa);
            }

            PatientDto? updated = await GetByIdAsync(id, ct) ?? throw new InvalidOperationException("Patient not found after update");
            return updated;
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken ct)
        {
            using IDbConnection con = factory.CreateConnection();
            const string sql = "delete from public.\"PACIENTES\" where \"ID_PACIENTES\" = @id";
            int affected = await con.ExecuteAsync(sql, new { id = int.Parse(id) });
            return affected > 0;
        }

        public async Task<IEnumerable<PatientSummaryDto>> GetSummaryAsync(string? professionalUserId, CancellationToken ct)
        {
            IEnumerable<PatientDto> patients = await GetAsync(null, null, null, null, professionalUserId, null, ct);
            // Basic summary (sin cálculos avanzados)
            return patients.Select(p => new PatientSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                Age = p.Age,
                Dni = p.Dni,
                Phone = p.Phone,
                LastAppointment = null,
                NextAppointment = null,
                TotalAppointments = 0,
                ActiveConditions = p.MedicalInfo.ChronicConditions.Count
            });
        }

        private static string MapGenero(string? dbGenero)
        {
            return dbGenero switch
            {
                "Masculino" => "male",
                "Femenino" => "female",
                "Otro" => "other",
                _ => "prefer_not_to_say"
            };
        }

        private static string MapGeneroToDb(string front)
        {
            return front switch
            {
                "male" => "Masculino",
                "female" => "Femenino",
                "other" => "Otro",
                _ => "Prefiere no decirlo"
            };
        }

        private static string CombineStreet(string? calle, string? numero)
            => string.Join(" ", new[] { calle ?? string.Empty, numero ?? string.Empty }.Where(s => !string.IsNullOrWhiteSpace(s)));

        private static string[] SplitName(string? fullName)
        {
            string cleaned = (fullName ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(cleaned))
            {
                return new[] { string.Empty, string.Empty };
            }
            string[] parts = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
            {
                return new[] { parts[0], string.Empty };
            }
            string nombre = parts[0];
            string apellido = string.Join(" ", parts.Skip(1));
            return new[] { nombre, apellido };
        }

        private static string ExtractStreetNumber(string street, string zip)
        {
            // Intento simple: tomar último token numérico
            string[] parts = (street ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string? last = parts.LastOrDefault();
            return last != null && int.TryParse(last, out _) ? last : string.Empty;
        }

        private string? EncryptNullable(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
            return dataProtectionService.Encrypt(value);
        }

        private string? DecryptNullable(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            return dataProtectionService.Decrypt(value) ?? value;
        }

        private static async Task<Dictionary<int, List<string>>> LoadAlergiasAsync(IDbConnection con, int[] ids)
        {
            if (ids.Length == 0)
            {
                return new Dictionary<int, List<string>>();
            }
            const string sql = @"select pa.""ID_PACIENTES"" as IdPac, a.""NOMBRE"" as Nombre
                                 from public.""PACIENTE_ALERGIAS"" pa
                                 join public.""ALERGIAS"" a on a.""ID_ALERGIAS"" = pa.""ID_ALERGIAS""
                                 where pa.""ID_PACIENTES"" = any(@ids)";
            IEnumerable<dynamic> rows = await con.QueryAsync(sql, new { ids });
            return rows.GroupBy(r => (int)r.IdPac).ToDictionary(g => g.Key, g => g.Select(x => (string)x.Nombre).ToList());
        }

        private static async Task<Dictionary<int, List<string>>> LoadCondicionesAsync(IDbConnection con, int[] ids)
        {
            if (ids.Length == 0)
            {
                return new Dictionary<int, List<string>>();
            }
            const string sql = @"select pc.""ID_PACIENTES"" as IdPac, c.""NOMBRE"" as Nombre
                                 from public.""PACIENTE_CONDICIONES"" pc
                                 join public.""CONDICIONES_CRONICAS"" c on c.""ID_CONDICIONES_CRONICAS"" = pc.""ID_CONDICIONES_CRONICAS""
                                 where pc.""ID_PACIENTES"" = any(@ids)";
            IEnumerable<dynamic> rows = await con.QueryAsync(sql, new { ids });
            return rows.GroupBy(r => (int)r.IdPac).ToDictionary(g => g.Key, g => g.Select(x => (string)x.Nombre).ToList());
        }

        private static async Task<Dictionary<int, List<string>>> LoadCirugiasAsync(IDbConnection con, int[] ids)
        {
            if (ids.Length == 0)
            {
                return new Dictionary<int, List<string>>();
            }
            const string sql = @"select pc.""ID_PACIENTES"" as IdPac, c.""NOMBRE"" as Nombre
                                 from public.""PACIENTE_CIRUGIAS"" pc
                                 join public.""CIRUGIAS"" c on c.""ID_CIRUGIAS"" = pc.""ID_CIRUGIAS""
                                 where pc.""ID_PACIENTES"" = any(@ids)";
            IEnumerable<dynamic> rows = await con.QueryAsync(sql, new { ids });
            return rows.GroupBy(r => (int)r.IdPac).ToDictionary(g => g.Key, g => g.Select(x => (string)x.Nombre).ToList());
        }
    }
}
