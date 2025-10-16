namespace DoctorWare.Models
{
    public class PACIENTE_CONDICIONES
    {
        public int ID_PACIENTE_CONDICIONES { get; set; }

        public int ID_PACIENTES { get; set; }

        public int ID_CONDICIONES_CRONICAS { get; set; }

        public string DETALLES { get; set; }

        public DateTime FECHA_CREACION { get; set; }

        public DateTime ULTIMA_ACTUALIZACION { get; set; }
    }
}
