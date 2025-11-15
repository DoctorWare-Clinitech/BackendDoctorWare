using System;

namespace DoctorWare.Models
{
    public class AUSENCIAS_PROFESIONALES
    {
        public int ID_AUSENCIAS_PROFESIONALES { get; set; }

        public int ID_PROFESIONALES { get; set; }

        public DateTime FECHA_DESDE { get; set; }

        public DateTime FECHA_HASTA { get; set; }

        public TimeSpan? HORA_DESDE { get; set; }

        public TimeSpan? HORA_HASTA { get; set; }

        public string? TIPO { get; set; }

        public string? MOTIVO { get; set; }

        public DateTime FECHA_CREACION { get; set; }

        public DateTime ULTIMA_ACTUALIZACION { get; set; }
    }
}

