namespace DoctorWare.Models
{
    public class MEDICAMENTOS
    {
        public int ID_MEDICAMENTOS { get; set; }

        public string NOMBRE_GENERICO { get; set; }

        public string NOMBRE_COMERCIAL { get; set; }

        public DateTime FECHA_CREACION { get; set; }

        public DateTime ULTIMA_ACTUALIZACION { get; set; }
    }
}