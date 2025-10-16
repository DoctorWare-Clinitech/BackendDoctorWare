namespace DoctorWare.Models
{
    public class RECETAS
    {
        public int ID_RECETAS { get; set; }

        public string DOCIS { get; set; }

        public string FRECUENCIA { get; set; }

        public string DURACION { get; set; }

        public string INSTRUCCIONES { get; set; }

        public int ID_HISTORIAS_CLINICAS { get; set; }

        public int ID_MEDICAMENTOS { get; set; }

        public DateTime FECHA_CREACION { get; set; }

        public DateTime ULTIMA_ACTUALIZACION { get; set; }
    }
}
