using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TaskApi.Model.Domain.Entities;

namespace TaskApi.Services
{
    public static class JqlFilterParser
    {
        private static readonly Regex ClausePattern =
            new(
                @"(\w+)\s*(=|!=|in|is\s+not|is)\s*(\([^)]+\)|""[^""]+""|[^\s]+)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static IEnumerable<TaskItem> Apply(
            IEnumerable<TaskItem> tasks,
            string jqlQuery,
            Guid userId,
            IEnumerable<Team>? teams = null)
        {
            if (string.IsNullOrWhiteSpace(jqlQuery))
            {
                return tasks;
            }

            var clauses = ParseClauses(jqlQuery);
            var result = tasks.AsEnumerable();

            foreach (var (field, op, rawValue) in clauses)
            {
                var value = Unquote(rawValue).Trim();
                var opLower = op.ToLowerInvariant();

                if (field.Equals("team", StringComparison.OrdinalIgnoreCase))
                {
                    result = MatchTeam(result, opLower, value, userId, teams);
                    continue;
                }

                result = field.ToLowerInvariant() switch
                {
                    "status" => result.Where(
                        x => x.Status.ToString()
                            .Equals(value, StringComparison.OrdinalIgnoreCase)),
                    "priority" => result.Where(
                        x => x.Priority.ToString()
                            .Equals(value, StringComparison.OrdinalIgnoreCase)),
                    "issuetype" or "type" => result.Where(
                        x => x.IssueType.ToString()
                            .Equals(value, StringComparison.OrdinalIgnoreCase)),
                    "assignee" when value.Equals(
                        "me",
                        StringComparison.OrdinalIgnoreCase) =>
                        result.Where(x => x.AssigneeId == userId),
                    "assignee" when value.Equals(
                        "unassigned",
                        StringComparison.OrdinalIgnoreCase) =>
                        result.Where(x => x.AssigneeId == null),
                    "assignee" when Guid.TryParse(value, out var assigneeId) =>
                        result.Where(x => x.AssigneeId == assigneeId),
                    "reporter" when value.Equals(
                        "me",
                        StringComparison.OrdinalIgnoreCase) =>
                        result.Where(x => x.ReporterId == userId),
                    "resolution" when value.Equals(
                        "empty",
                        StringComparison.OrdinalIgnoreCase) =>
                        result.Where(x => x.Resolution == null),
                    "resolution" => result.Where(
                        x => x.Resolution != null &&
                            x.Resolution.ToString()!
                                .Equals(value, StringComparison.OrdinalIgnoreCase)),
                    _ => result
                };
            }

            return result;
        }

        private static IEnumerable<TaskItem> MatchTeam(
            IEnumerable<TaskItem> tasks,
            string op,
            string value,
            Guid userId,
            IEnumerable<Team>? teams)
        {
            var valueLower = value.ToLowerInvariant();

            // 1. Check for null/empty
            if (valueLower == "null" || valueLower == "empty")
            {
                if (op == "=" || op == "is")
                {
                    return tasks.Where(t => t.TeamId == null);
                }
                else if (op == "!=" || op == "is not")
                {
                    return tasks.Where(t => t.TeamId != null);
                }
            }

            // 2. Resolve teams list
            var teamList = teams?.ToList() ?? new List<Team>();

            // 3. Check for currentUserTeam()
            if (valueLower.StartsWith("currentuserteam"))
            {
                var userTeamIds = teamList
                    .Where(t => t.Members.Any(m => m.UserId == userId))
                    .Select(t => t.TeamId)
                    .ToList();

                if (op == "=" || op == "in")
                {
                    return tasks.Where(t => t.TeamId.HasValue && userTeamIds.Contains(t.TeamId.Value));
                }
                else if (op == "!=")
                {
                    return tasks.Where(t => !t.TeamId.HasValue || !userTeamIds.Contains(t.TeamId.Value));
                }
            }

            // 4. Check for IN list, e.g. ("Platform", "Infra")
            var searchValues = new List<string>();
            if (value.StartsWith("(") && value.EndsWith(")"))
            {
                var inner = value[1..^1];
                // Split by comma, unquote each
                var parts = inner.Split(',');
                foreach (var part in parts)
                {
                    searchValues.Add(Unquote(part.Trim()));
                }
            }
            else
            {
                searchValues.Add(value);
            }

            // Find all matching team IDs
            var matchedTeamIds = new HashSet<Guid>();
            foreach (var val in searchValues)
            {
                if (Guid.TryParse(val, out var teamId))
                {
                    matchedTeamIds.Add(teamId);
                }
                else
                {
                    // Find team by name
                    var matchingTeams = teamList
                        .Where(t => t.Name.Equals(val, StringComparison.OrdinalIgnoreCase))
                        .Select(t => t.TeamId);
                    foreach (var id in matchingTeams)
                    {
                        matchedTeamIds.Add(id);
                    }
                }
            }

            if (op == "=" || op == "in")
            {
                return tasks.Where(t => t.TeamId.HasValue && matchedTeamIds.Contains(t.TeamId.Value));
            }
            else if (op == "!=")
            {
                return tasks.Where(t => !t.TeamId.HasValue || !matchedTeamIds.Contains(t.TeamId.Value));
            }

            return tasks;
        }

        private static IEnumerable<(string Field, string Operator, string Value)> ParseClauses(
            string jqlQuery)
        {
            var normalized =
                jqlQuery
                    .Replace(" AND ", " ", StringComparison.OrdinalIgnoreCase)
                    .Replace(" and ", " ");

            foreach (Match match in ClausePattern.Matches(normalized))
            {
                yield return (match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
            }
        }

        private static string Unquote(string value)
        {
            if (value.Length >= 2 &&
                value.StartsWith('"') &&
                value.EndsWith('"'))
            {
                return value[1..^1];
            }

            return value;
        }
    }
}
