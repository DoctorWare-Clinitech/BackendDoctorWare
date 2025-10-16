namespace DoctorWare.Models
{
    public class USUARIOS
    {
        public int ID_USUARIOS { get; set; }

        public string EMAIL { get; set; }

        public string PASSWORD_HASH { get; set; }

        public bool EMAIL_CONFIRMADO { get; set; }

        public bool TELEFONO_CONFIRMADO { get; set; }

        public DateTime? FECHA_ULTIMO_ACCESO { get; set; }

        public int INTENTOS_FALLIDOS { get; set; }

        public DateTime? BLOQUEADO_HASTA { get; set; }

        public string TOKEN_RECUPERACION { get; set; }

        public DateTime? TOKEN_EXPIRACION { get; set; }

        public bool ACTIVO { get; set; }

        public int ID_PERSONAS { get; set; }

        public int ID_ESTADOS_USUARIO { get; set; }

        public DateTime FECHA_CREACION { get; set; }

        public DateTime ULTIMA_ACTUALIZACION { get; set; }
    }
}
