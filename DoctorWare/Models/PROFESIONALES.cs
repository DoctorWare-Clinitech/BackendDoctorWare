namespace DoctorWare.Models
{
    public class PROFESIONALES
    {
        public int ID_PROFESIONALES { get; set; }

        public string MATRICULA_NACIONAL { get; set; }

        public string MATRICULA_PROVINCIAL { get; set; }

        public string TITULO { get; set; }

        public string UNIVERSIDAD { get; set; }

        public int? ANIO_EGRESO { get; set; }

        public string CUIT_CUIL { get; set; }

        public int DURACION_TURNO_EN_MINUTOS { get; set; }

        public bool ACTIVO { get; set; }

        public int ID_PERSONAS { get; set; }

        public DateTime FECHA_CREACION { get; set; }

        public DateTime ULTIMA_ACTUALIZACION { get; set; }
    }
}
