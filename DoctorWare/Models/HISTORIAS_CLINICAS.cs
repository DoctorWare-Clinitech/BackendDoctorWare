namespace DoctorWare.Models
{
    public class HISTORIAS_CLINICAS
    {
        public int ID_HISTORIAS_CLINICAS { get; set; }

        public DateTime FECHA { get; set; }

        public string DIAGNOSTICO { get; set; }

        public string SINTOMA { get; set; }

        public string TRATAMIENTO { get; set; }

        public string NOTAS { get; set; }

        public string ADJUNTOS { get; set; }

        public int ID_PACIENTES { get; set; }

        public int ID_PROFESIONALES { get; set; }

        public int? ID_TURNOS { get; set; }

        public DateTime FECHA_CREACION { get; set; }

        public DateTime ULTIMA_ACTUALIZACION { get; set; }
    }
}
