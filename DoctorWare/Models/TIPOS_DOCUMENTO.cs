namespace DoctorWare.Models
{
    public class TIPOS_DOCUMENTO
    {
        public int ID_TIPOS_DOCUMENTO { get; set; }

        public string CODIGO { get; set; }

        public string NOMBRE { get; set; }

        public DateTime FECHA_CREACION { get; set; }

        public DateTime ULTIMA_ACTUALIZACION { get; set; }
    }
}