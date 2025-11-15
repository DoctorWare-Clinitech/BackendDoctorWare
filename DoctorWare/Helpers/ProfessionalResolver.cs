using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using DoctorWare.Exceptions;

namespace DoctorWare.Helpers
{
    public static class ProfessionalResolver
    {
        private const string Sql = @"
            select p.""ID_PROFESIONALES""
            from public.""PROFESIONALES"" p
            join public.""USUARIOS"" u on u.""ID_PERSONAS"" = p.""ID_PERSONAS""
            where u.""ID_USUARIOS"" = @uid
            limit 1;";

        public static Task<int?> TryResolveAsync(IDbConnection con, string? professionalUserId, CancellationToken ct, IDbTransaction? tx = null)
        {
            if (string.IsNullOrWhiteSpace(professionalUserId))
            {
                return Task.FromResult<int?>(null);
            }

            if (!int.TryParse(professionalUserId, out int uid))
            {
                return Task.FromResult<int?>(null);
            }

            return con.ExecuteScalarAsync<int?>(new CommandDefinition(Sql, new { uid }, transaction: tx, cancellationToken: ct));
        }

        public static async Task<int> ResolveRequiredAsync(IDbConnection con, string professionalUserId, CancellationToken ct, IDbTransaction? tx = null)
        {
            if (string.IsNullOrWhiteSpace(professionalUserId) || !int.TryParse(professionalUserId, out int _))
            {
                throw new BadRequestException("Identificador de profesional inválido.");
            }

            int? id = await TryResolveAsync(con, professionalUserId, ct, tx);
            if (!id.HasValue)
            {
                throw new NotFoundException("Profesional no encontrado.");
            }
            return id.Value;
        }
    }
}
