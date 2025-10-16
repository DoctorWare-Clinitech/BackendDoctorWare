namespace DoctorWare.Models
{
    public class PACIENTE_ALERGIAS
    {
        public int ID_PACIENTE_ALERGIAS { get; set; }

        public int ID_PACIENTES { get; set; }

        public int ID_ALERGIAS { get; set; }

        public string DETALLES { get; set; }

        public DateTime FECHA_CREACION { get; set; }

        public DateTime ULTIMA_ACTUALIZACION { get; set; }
    }
}
