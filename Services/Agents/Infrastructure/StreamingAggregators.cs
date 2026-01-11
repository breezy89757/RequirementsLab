using System;
using System.Collections.Generic;
using System.Linq;

namespace RequirementsLab.Services.Agents.Infrastructure;

/// <summary>
/// Provides a set of streaming aggregation functions for processing sequences of input values in a stateful type.
/// Borrowed from Microsoft Agent Framework for high-performance UI rendering.
/// </summary>
public static class StreamingAggregators
{
    // Accumulate string chunks into a full message (Standard Chat Streaming)
    public static Func<string?, string, string?> AppendString()
    {
        return Aggregate;

        static string? Aggregate(string? current, string next)
        {
            return (current ?? "") + next;
        }
    }

    // Keep only the latest value (for Status updates)
    public static Func<T?, T, T?> Last<T>()
    {
        return (current, next) => next;
    }

    // Collect all items into a list (for Tool Calls or Thoughts)
    public static Func<List<T>?, T, List<T>?> Collect<T>()
    {
        return Aggregate;
        
        static List<T>? Aggregate(List<T>? list, T item)
        {
            list ??= new List<T>();
            list.Add(item);
            return list;
        }
    }
}
