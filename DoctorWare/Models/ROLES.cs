namespace DoctorWare.Models
{
    public class ROLES
    {
        public int ID_ROLES { get; set; }

        public string NOMBRE { get; set; }

        public string DESCRIPCION { get; set; }

        public bool ACTIVO { get; set; }

        public DateTime FECHA_CREACION { get; set; }

        public DateTime ULTIMA_ACTUALIZACION { get; set; }
    }
}
