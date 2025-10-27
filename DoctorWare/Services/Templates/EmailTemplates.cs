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
    }
}

