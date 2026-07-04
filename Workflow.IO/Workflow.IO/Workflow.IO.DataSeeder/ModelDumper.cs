using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Workflow.IO.DataSeeder
{
    public class ModelDumper
    {
        public static void Dump()
        {
            var assemblies = new[]
            {
                Assembly.Load("UserApi"),
                Assembly.Load("ProjectApi"),
                Assembly.Load("TaskApi"),
                Assembly.Load("CommentApi"),
                Assembly.Load("FileApi"),
                Assembly.Load("NotificationApi"),
                Assembly.Load("ActivityApi"),
                Assembly.Load("AnalyticsApi"),
                Assembly.Load("JwtAuthenticationManager")
            };

            var dbContextTypes = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(DbContext).IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();

            var schema = new Dictionary<string, object>();

            foreach (var contextType in dbContextTypes)
            {
                var dbSets = contextType.GetProperties()
                    .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                    .ToList();

                var tables = new List<object>();

                foreach (var set in dbSets)
                {
                    var entityType = set.PropertyType.GetGenericArguments()[0];
                    var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                        .Select(p => new { p.Name, Type = p.PropertyType.Name })
                        .ToList();

                    tables.Add(new { TableName = set.Name, EntityName = entityType.Name, Properties = properties });
                }

                schema[contextType.Name] = tables;
            }

            var json = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText("schema_dump.json", json);
            Console.WriteLine("Schema dumped to schema_dump.json");
        }
    }
}
