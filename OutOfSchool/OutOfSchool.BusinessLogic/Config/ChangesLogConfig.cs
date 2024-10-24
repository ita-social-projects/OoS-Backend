﻿namespace OutOfSchool.BusinessLogic.Config;

public class ChangesLogConfig
{
    public const string Name = "ChangesLog";

    public IReadOnlyDictionary<string, string[]> TrackedProperties { get; set; }
}