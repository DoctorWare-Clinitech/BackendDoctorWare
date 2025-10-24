namespace DoctorWare.DTOs.Response
{
    /// <summary>
    /// Resultado paginado genérico para listados con total y metadatos de paginación.
    /// </summary>
    /// <typeparam name="T">Tipo de los elementos listados.</typeparam>
    public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
}

