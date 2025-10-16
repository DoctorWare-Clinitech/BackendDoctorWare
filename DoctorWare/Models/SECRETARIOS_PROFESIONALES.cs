namespace DoctorWare.Models
{
    public class SECRETARIOS_PROFESIONALES
    {
        public int ID_SECRETARIOS_PROFESIONALES { get; set; }

        public int ID_SECRETARIOS { get; set; }

        public int ID_PROFESIONALES { get; set; }

        public string PERMISOS { get; set; }

        public DateTime FECHA_CREACION { get; set; }

        public DateTime ULTIMA_ACTUALIZACION { get; set; }
    }
}
