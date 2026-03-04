using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EffortEstimator
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            string json = """
            {
                "task_description": "Integração com gateway de pagamento",
                "technical_complexity": "complex",
                "team_knowledge": "intermediate",
                "external_integrations": { "count": 2, "complexity": "high" },
                "external_dependencies": { "count": 1, "team_reliability": "medium" }
            }
            """;

            if (args.Length > 0 && args[0] == "--stdin")
                json = Console.In.ReadToEnd();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var input   = JsonSerializer.Deserialize<TaskInput>(json, options)!;

            PertResult result = PertEngine.Estimate(input);

            Console.WriteLine($"\n  TASK:            {result.TaskDescription}");
            Console.WriteLine($"  Otimista (O):      {result.Optimistic:F1} h");
            Console.WriteLine($"  Mais Provável (M): {result.MostLikely:F1} h");
            Console.WriteLine($"  Pessimista (P):    {result.Pessimistic:F1} h");
            Console.WriteLine($"\n  PERT = (O + 4M + P) / 6 = {result.PertHours:F1} h");
            Console.WriteLine($"  Story Points:      {result.StoryPoints} (Fibonacci)");
            Console.WriteLine($"  Desvio Padrão:     {result.StandardDeviation:F1} h");
            Console.WriteLine($"  Intervalo 68%:     {result.ConfidenceRange.Low:F1} h — {result.ConfidenceRange.High:F1} h");
            Console.WriteLine($"  Risco:             {result.RiskLevel}\n");
        }
    }
}