﻿using FluentValidation;
using System;

namespace Noppes.Fluffle.Configuration;

/// <summary>
/// Configuration regarding Inkbunny
/// </summary>
[ConfigurationSection("Inkbunny")]
public class InkbunnyConfiguration : FluffleConfigurationPart<InkbunnyConfiguration>
{
    public InkbunnyCredentialsConfiguration Credentials { get; set; }

    public InkbunnySyncConfiguration Sync { get; set; }

    public InkbunnyConfiguration()
    {
        RuleFor(x => x.Credentials).NotEmpty().SetValidator(x => x.Credentials);
        RuleFor(x => x.Sync).NotEmpty().SetValidator(x => x.Sync);
    }
}

public class InkbunnyCredentialsConfiguration : AbstractValidator<InkbunnyCredentialsConfiguration>
{
    public string Username { get; set; }

    public string Password { get; set; }

    public InkbunnyCredentialsConfiguration()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class InkbunnySyncConfiguration : AbstractValidator<InkbunnySyncConfiguration>
{
    public TimeSpan? ApiThrottle { get; set; }

    public int NIdsToGoBack { get; set; }

    public TimeSpan SubmissionMinimumAge { get; set; }

    public InkbunnySyncConfiguration()
    {
        RuleFor(x => x.ApiThrottle).GreaterThan(TimeSpan.Zero);
        RuleFor(x => x.NIdsToGoBack).NotEmpty().GreaterThanOrEqualTo(1);
        RuleFor(x => x.SubmissionMinimumAge).NotEmpty().GreaterThanOrEqualTo(TimeSpan.Zero);
    }
}
