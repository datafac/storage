using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DTOMaker.Gentime
{
    public static class AssemblyExtensions
    {
        public static string[] GetTemplate(this Assembly assembly, string templateName)
        {
            using var stream = assembly.GetManifestResourceStream(templateName);
            if (stream is null) throw new ArgumentException($"Template '{templateName}' not found", nameof(templateName));
            var result = new List<string>();
            using var reader = new StreamReader(stream);
            string? line;
            while ((line = reader.ReadLine()) is not null)
            {
                result.Add(line);
            }
            return result.ToArray();
        }
    }
}
