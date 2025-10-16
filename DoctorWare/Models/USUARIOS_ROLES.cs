namespace DoctorWare.Models
{
    public class USUARIOS_ROLES
    {
        public int ID_USUARIOS_ROLES { get; set; }

        public int ID_USUARIOS { get; set; }

        public int ID_ROLES { get; set; }

        public DateTime FECHA_CREACION { get; set; }

        public DateTime ULTIMA_ACTUALIZACION { get; set; }
    }
}
