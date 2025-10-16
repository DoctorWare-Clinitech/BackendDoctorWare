namespace DoctorWare.Models
{
    public class SECRETARIOS
    {
        public int ID_SECRETARIOS { get; set; }

        public string LEGAJO { get; set; }

        public TimeSpan? HORARIO_ENTRADA { get; set; }

        public TimeSpan? HORARIO_SALIDA { get; set; }

        public bool ACTIVO { get; set; }

        public int ID_PERSONAS { get; set; }

        public DateTime FECHA_CREACION { get; set; }

        public DateTime ULTIMA_ACTUALIZACION { get; set; }
    }
}
