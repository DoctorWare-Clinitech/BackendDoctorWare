namespace DoctorWare.Models
{
    public class PERSONAS
    {
        public int ID_PERSONAS { get; set; }

        public long NRO_DOCUMENTO { get; set; }

        public string NOMBRE { get; set; }

        public string APELLIDO { get; set; }

        public DateTime? FECHA_NACIMIENTO { get; set; }

        public string FOTO { get; set; }

        public string EMAIL_PRINCIPAL { get; set; }

        public string TELEFONO_PRINCIPAL { get; set; }

        public string CELULAR_PRINCIPAL { get; set; }

        public string CALLE { get; set; }

        public string NUMERO { get; set; }

        public string PISO { get; set; }

        public string DEPARTAMENTO { get; set; }

        public string LOCALIDAD { get; set; }

        public string PROVINCIA { get; set; }

        public string CODIGO_POSTAL { get; set; }

        public string PAIS { get; set; }

        public bool ACTIVO { get; set; }

        public int ID_TIPOS_DOCUMENTO { get; set; }

        public int ID_GENEROS { get; set; }

        public DateTime FECHA_CREACION { get; set; }

        public DateTime ULTIMA_ACTUALIZACION { get; set; }
    }
}