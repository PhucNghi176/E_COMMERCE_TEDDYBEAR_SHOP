﻿using System.ComponentModel.DataAnnotations;

namespace CONTRACT.CONTRACT.PERSISTENCE.DependencyInjection.Options;
public record SqlServerRetryOptions
{
    [Required] [Range(5, 20)] public int MaxRetryCount { get; init; }
    [Required] [Timestamp] public TimeSpan MaxRetryDelay { get; init; }
    public int[]? ErrorNumbersToAdd { get; init; }
}