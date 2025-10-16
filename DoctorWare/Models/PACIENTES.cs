
using System;

namespace DoctorWare.Models
{
    public class PACIENTES
    {
        public int ID_PACIENTES { get; set; }

        public string SEGURO_PROVEEDOR { get; set; }

        public string SEGURO_PLAN { get; set; }

        public string SEGURO_NUMERO_AFILIADO { get; set; }

        public string ANTECEDENTES_FAMILIARES { get; set; }

        public string CONTACTO_EMERGENCIA_NOMBRE { get; set; }

        public string CONTACTO_EMERGENCIA_TELEFONO { get; set; }

        public string CONTACTO_EMERGENCIA_RELACION { get; set; }

        public string NOTAS_GENERALES { get; set; }

        public bool ACTIVO { get; set; }

        public int ID_PERSONAS { get; set; }

        public int ID_GRUPOS_SANGUINEOS { get; set; }

        public int ID_PROFESIONALES { get; set; }

        public DateTime FECHA_CREACION { get; set; }

        public DateTime ULTIMA_ACTUALIZACION { get; set; }
    }
}
