namespace DoctorWare.Models
{
    public class SUB_ESPECIALIDADES
    {
        public int ID_SUB_ESPECIALIDADES { get; set; }

        public string NOMBRE { get; set; }

        public int ID_ESPECIALIDADES { get; set; }

        public DateTime FECHA_CREACION { get; set; }

        public DateTime ULTIMA_ACTUALIZACION { get; set; }
    }
}
