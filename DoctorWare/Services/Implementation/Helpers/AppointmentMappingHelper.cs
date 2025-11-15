using System;
using System.Collections.Generic;

namespace DoctorWare.Services.Implementation.Helpers
{
    internal static class AppointmentMappingHelper
    {
        private static readonly Dictionary<string, string> DbStatusToFront = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Programado"] = "scheduled",
            ["Confirmado"] = "confirmed",
            ["En Espera"] = "in_progress",
            ["Atendido"] = "completed",
            ["Cancelado por Paciente"] = "cancelled",
            ["Cancelado por Profesional"] = "cancelled",
            ["Ausente"] = "no_show",
            ["Reprogramado"] = "scheduled"
        };

        private static readonly Dictionary<string, string> FrontStatusToDb = new(StringComparer.OrdinalIgnoreCase)
        {
            ["scheduled"] = "Programado",
            ["confirmed"] = "Confirmado",
            ["in_progress"] = "En Espera",
            ["completed"] = "Atendido",
            ["cancelled"] = "Cancelado por Profesional",
            ["no_show"] = "Ausente"
        };

        private static readonly Dictionary<string, string> DbTypeToFront = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Consulta"] = "first_visit",
            ["Seguimiento"] = "follow_up",
            ["Cirugía"] = "routine",
            ["Vacunación"] = "routine",
            ["Control General"] = "routine",
            ["Estudio"] = "specialist"
        };

        private static readonly Dictionary<string, string> FrontTypeToDb = new(StringComparer.OrdinalIgnoreCase)
        {
            ["first_visit"] = "Consulta",
            ["follow_up"] = "Seguimiento",
            ["emergency"] = "Consulta",
            ["routine"] = "Control General",
            ["specialist"] = "Estudio"
        };

        public static string MapDbStatusToFront(string dbNombre)
        {
            string front;
            return DbStatusToFront.TryGetValue(dbNombre, out front) ? front : "scheduled";
        }

        public static string MapFrontStatusToDb(string front)
        {
            string db;
            return FrontStatusToDb.TryGetValue(front, out db) ? db : "Programado";
        }

        public static string MapDbTypeToFront(string dbNombre)
        {
            string front;
            return DbTypeToFront.TryGetValue(dbNombre, out front) ? front : "routine";
        }

        public static string MapFrontTypeToDb(string front)
        {
            string db;
            return FrontTypeToDb.TryGetValue(front, out db) ? db : "Consulta";
        }

        public static string FormatTimeSpan(TimeSpan time)
            => new DateTime(time.Ticks).ToString("HH:mm");
    }
}
