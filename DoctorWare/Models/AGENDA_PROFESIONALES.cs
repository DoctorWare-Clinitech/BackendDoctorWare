using System;

namespace DoctorWare.Models
{
    public class AGENDA_PROFESIONALES
    {
        public int ID_AGENDA_PROFESIONALES { get; set; }

        public int ID_PROFESIONALES { get; set; }

        public short DIA_SEMANA { get; set; }

        public TimeSpan HORA_INICIO { get; set; }

        public TimeSpan HORA_FIN { get; set; }

        public int? DURACION_MINUTOS { get; set; }

        public bool ES_LABORABLE { get; set; }

        public DateTime FECHA_CREACION { get; set; }

        public DateTime ULTIMA_ACTUALIZACION { get; set; }
    }
}

