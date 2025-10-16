namespace DoctorWare.Models
{
    public class PACIENTE_CIRUGIAS
    {
        public int ID_PACIENTE_CIRUGIAS { get; set; }

        public int ID_PACIENTES { get; set; }

        public int ID_CIRUGIAS { get; set; }

        public DateTime? FECHA { get; set; }

        public string DETALLES { get; set; }

        public DateTime FECHA_CREACION { get; set; }

        public DateTime ULTIMA_ACTUALIZACION { get; set; }
    }
}
