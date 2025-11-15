using System.Net;
using System.Text;

namespace DoctorWare.Services.Templates
{
    public static class EmailTemplates
    {
        public static string BuildConfirmationEmail(string? fullName, string confirmationUrl)
        {
            string greeting = string.IsNullOrWhiteSpace(fullName)
                ? "Hola,"
                : $"Hola {WebUtility.HtmlEncode(fullName)},";

            string safeUrl = confirmationUrl;

            StringBuilder sb = new StringBuilder();
            sb.Append("<table role=\"presentation\" style=\"width:100%;background-color:#f4f6fb;padding:24px 0;font-family:'Segoe UI',Arial,sans-serif;\">");
            sb.Append("<tr><td align=\"center\">");
            sb.Append("<table role=\"presentation\" style=\"max-width:520px;background-color:#ffffff;border-radius:12px;padding:32px 28px;box-shadow:0 8px 30px rgba(15,23,42,0.12);\">");

            sb.Append("<tr><td style=\"padding-bottom:16px;\">");
            sb.Append("<p style=\"margin:0;font-size:16px;color:#111827;\">").Append(greeting).Append("</p>");
            sb.Append("</td></tr>");

            sb.Append("<tr><td style=\"padding-bottom:20px;\">");
            sb.Append("<p style=\"margin:0;font-size:15px;color:#4b5563;line-height:1.6;\">");
            sb.Append("Gracias por registrarte en <strong>DoctorWare</strong>. Haz clic en el botón para confirmar tu correo electrónico y activar tu cuenta.");
            sb.Append("</p>");
            sb.Append("</td></tr>");

            sb.Append("<tr><td align=\"center\" style=\"padding:24px 0 12px;\">");
            sb.Append("<a href=\"").Append(safeUrl).Append("\" ");
            sb.Append("style=\"display:inline-block;background:linear-gradient(135deg,#2563eb,#1d4ed8);color:#ffffff;padding:14px 32px;border-radius:999px;");
            sb.Append("text-decoration:none;font-weight:600;font-size:15px;letter-spacing:0.3px;\">Confirmar correo</a>");
            sb.Append("</td></tr>");

            sb.Append("<tr><td style=\"padding-top:8px;\">");
            sb.Append("<p style=\"margin:0;font-size:13px;color:#6b7280;line-height:1.6;\">");
            sb.Append("Si el botón no funciona, copia y pega este enlace en tu navegador:<br>");
            sb.Append("<a href=\"").Append(safeUrl).Append("\" style=\"color:#2563eb;text-decoration:none;\">").Append(safeUrl).Append("</a>");
            sb.Append("</p>");
            sb.Append("</td></tr>");

            sb.Append("<tr><td style=\"padding-top:24px;\">");
            sb.Append("<p style=\"margin:0;font-size:13px;color:#9ca3af;line-height:1.6;\">");
            sb.Append("Si tú no solicitaste esta cuenta, puedes ignorar este mensaje. Este correo fue enviado automáticamente, por favor no respondas.");
            sb.Append("</p>");
            sb.Append("</td></tr>");

            sb.Append("</table>");
            sb.Append("</td></tr>");
            sb.Append("</table>");

            return sb.ToString();
        }

        public static string BuildPatientConfirmationEmail(string? fullName, string confirmationUrl)
        {
            // Por ahora reutiliza el mismo cuerpo base
            return BuildConfirmationEmail(fullName, confirmationUrl);
        }

        public static string BuildProfessionalConfirmationEmail(string? fullName, string confirmationUrl)
        {
            // Por ahora reutiliza el mismo cuerpo base
            return BuildConfirmationEmail(fullName, confirmationUrl);
        }

        public static string BuildAppointmentReminderEmail(string? fullName, DateTime date, string startTime, string? extraInfo = null)
        {
            string greeting = string.IsNullOrWhiteSpace(fullName)
                ? "Hola,"
                : $"Hola {WebUtility.HtmlEncode(fullName)},";

            string safeDate = date.ToString("dddd dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-AR"));
            string safeTime = WebUtility.HtmlEncode(startTime);

            StringBuilder sb = new StringBuilder();
            sb.Append("<table role=\"presentation\" style=\"width:100%;background-color:#f4f6fb;padding:24px 0;font-family:'Segoe UI',Arial,sans-serif;\">");
            sb.Append("<tr><td align=\"center\">");
            sb.Append("<table role=\"presentation\" style=\"max-width:520px;background-color:#ffffff;border-radius:12px;padding:32px 28px;box-shadow:0 8px 30px rgba(15,23,42,0.12);\">");

            sb.Append("<tr><td style=\"padding-bottom:16px;\">");
            sb.Append("<p style=\"margin:0;font-size:16px;color:#111827;\">").Append(greeting).Append("</p>");
            sb.Append("</td></tr>");

            sb.Append("<tr><td style=\"padding-bottom:20px;\">");
            sb.Append("<p style=\"margin:0;font-size:15px;color:#4b5563;line-height:1.6;\">");
            sb.Append("Este es un recordatorio de tu turno agendado en <strong>DoctorWare</strong>.");
            sb.Append("</p>");
            sb.Append("</td></tr>");

            sb.Append("<tr><td style=\"padding:12px 0;\">");
            sb.Append("<table role=\"presentation\" style=\"width:100%;border-radius:10px;background-color:#f3f4ff;padding:16px 18px;\">");
            sb.Append("<tr><td style=\"font-size:14px;color:#111827;\">");
            sb.Append("<p style=\"margin:0 0 6px 0;\"><strong>Fecha:</strong> ").Append(WebUtility.HtmlEncode(safeDate)).Append("</p>");
            sb.Append("<p style=\"margin:0 0 6px 0;\"><strong>Hora:</strong> ").Append(safeTime).Append("</p>");
            if (!string.IsNullOrWhiteSpace(extraInfo))
            {
                sb.Append("<p style=\"margin:0;\"><strong>Detalle:</strong> ").Append(WebUtility.HtmlEncode(extraInfo)).Append("</p>");
            }
            sb.Append("</td></tr>");
            sb.Append("</table>");
            sb.Append("</td></tr>");

            sb.Append("<tr><td style=\"padding-top:24px;\">");
            sb.Append("<p style=\"margin:0;font-size:13px;color:#6b7280;line-height:1.6;\">");
            sb.Append("Si no podés asistir, por favor cancelá o reprogramá tu turno con anticipación.");
            sb.Append("</p>");
            sb.Append("</td></tr>");

            sb.Append("<tr><td style=\"padding-top:16px;\">");
            sb.Append("<p style=\"margin:0;font-size:12px;color:#9ca3af;line-height:1.6;\">");
            sb.Append("Este mensaje es informativo y fue enviado automáticamente por DoctorWare. No respondas a este correo.");
            sb.Append("</p>");
            sb.Append("</td></tr>");

            sb.Append("</table>");
            sb.Append("</td></tr>");
            sb.Append("</table>");

            return sb.ToString();
        }

        public static string BuildPasswordResetEmail(string? fullName, string resetUrl)
        {
            string greeting = string.IsNullOrWhiteSpace(fullName)
                ? "Hola,"
                : $"Hola {WebUtility.HtmlEncode(fullName)},";

            string safeUrl = resetUrl;

            StringBuilder sb = new StringBuilder();
            sb.Append("<table role=\"presentation\" style=\"width:100%;background-color:#f4f6fb;padding:24px 0;font-family:'Segoe UI',Arial,sans-serif;\">");
            sb.Append("<tr><td align=\"center\">");
            sb.Append("<table role=\"presentation\" style=\"max-width:520px;background-color:#ffffff;border-radius:12px;padding:32px 28px;box-shadow:0 8px 30px rgba(15,23,42,0.12);\">");

            sb.Append("<tr><td style=\"padding-bottom:16px;\">");
            sb.Append("<p style=\"margin:0;font-size:16px;color:#111827;\">").Append(greeting).Append("</p>");
            sb.Append("</td></tr>");

            sb.Append("<tr><td style=\"padding-bottom:20px;\">");
            sb.Append("<p style=\"margin:0;font-size:15px;color:#4b5563;line-height:1.6;\">");
            sb.Append("Recibimos una solicitud para restablecer tu contraseña de <strong>DoctorWare</strong>. Haz clic en el botón para continuar.");
            sb.Append("</p>");
            sb.Append("</td></tr>");

            sb.Append("<tr><td align=\"center\" style=\"padding:24px 0 12px;\">");
            sb.Append("<a href=\"").Append(safeUrl).Append("\" ");
            sb.Append("style=\"display:inline-block;background:linear-gradient(135deg,#2563eb,#1d4ed8);color:#ffffff;padding:14px 32px;border-radius:999px;");
            sb.Append("text-decoration:none;font-weight:600;font-size:15px;letter-spacing:0.3px;\">Restablecer contraseña</a>");
            sb.Append("</td></tr>");

            sb.Append("<tr><td style=\"padding-top:8px;\">");
            sb.Append("<p style=\"margin:0;font-size:13px;color:#6b7280;line-height:1.6;\">");
            sb.Append("Si el botón no funciona, copia y pega este enlace en tu navegador:<br>");
            sb.Append("<a href=\"").Append(safeUrl).Append("\" style=\"color:#2563eb;text-decoration:none;\">").Append(safeUrl).Append("</a>");
            sb.Append("</p>");
            sb.Append("</td></tr>");

            sb.Append("<tr><td style=\"padding-top:24px;\">");
            sb.Append("<p style=\"margin:0;font-size:13px;color:#9ca3af;line-height:1.6;\">");
            sb.Append("Si tú no solicitaste este cambio, ignora este mensaje. El enlace expirará en unas horas por seguridad.");
            sb.Append("</p>");
            sb.Append("</td></tr>");

            sb.Append("</table>");
            sb.Append("</td></tr>");
            sb.Append("</table>");

            return sb.ToString();
        }
    }
}
