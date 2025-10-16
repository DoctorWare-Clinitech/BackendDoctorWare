namespace DoctorWare.Models
{
    public class TURNOS
    {
        public int ID_TURNOS { get; set; }

        public DateTime FECHA { get; set; }

        public TimeSpan HORA_INICIO { get; set; }

        public TimeSpan HORA_FIN { get; set; }

        public int? DURACION { get; set; }

        public string MOTIVO_CONSULTA { get; set; }

        public string NOTA_ADICIONAL { get; set; }

        public string OBSERVACION_PROFECIONAL { get; set; }

        public DateTime? FECHA_CANCELACION { get; set; }

        public string MOTIVO_CANCELACION { get; set; }

        public int ID_PACIENTES { get; set; }

        public int ID_PROFESIONALES { get; set; }

        public int ID_ESTADOS_TURNO { get; set; }

        public int ID_TIPOS_TURNO { get; set; }

        public int? ID_USUARIO_CREACION { get; set; }

        public int? ID_USUARIO_CANCELACION { get; set; }

        public DateTime FECHA_CREACION { get; set; }

        public DateTime ULTIMA_ACTUALIZACION { get; set; }
    }
}
