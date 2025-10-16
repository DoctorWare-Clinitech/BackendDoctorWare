namespace DoctorWare.Models
{
    public class PROFESIONAL_ESPECIALIDADES
    {
        public int ID_PROFESIONAL_ESPECIALIDADES { get; set; }

        public int ID_PROFESIONALES { get; set; }

        public int ID_ESPECIALIDADES { get; set; }

        public DateTime FECHA_CREACION { get; set; }

        public DateTime ULTIMA_ACTUALIZACION { get; set; }
    }
}
