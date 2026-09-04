using System.Text;

namespace SecurityAwareness.Application.Services;

public static class CsvHelper
{
    /// <summary>
    /// Parse CSV text into rows of fields. Handles quoted fields with embedded commas.
    /// </summary>
    public static List<List<string>> Parse(string csv)
    {
        var rows = new List<List<string>>();
        var current = new List<string>();
        var field = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < csv.Length; i++)
        {
            var c = csv[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < csv.Length && csv[i + 1] == '"') { field.Append('"'); i++; }
                    else inQuotes = false;
                }
                else field.Append(c);
            }
            else
            {
                if (c == '"') inQuotes = true;
                else if (c == ',') { current.Add(field.ToString()); field.Clear(); }
                else if (c == '\r') { /* skip */ }
                else if (c == '\n')
                {
                    current.Add(field.ToString()); field.Clear();
                    rows.Add(current); current = new List<string>();
                }
                else field.Append(c);
            }
        }
        if (field.Length > 0 || current.Count > 0)
        {
            current.Add(field.ToString());
            rows.Add(current);
        }
        return rows;
    }

    public static string Escape(string field)
    {
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }
}
