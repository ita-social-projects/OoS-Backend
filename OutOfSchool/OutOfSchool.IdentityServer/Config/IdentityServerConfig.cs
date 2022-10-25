﻿using System;

namespace OutOfSchool.IdentityServer.Config;

public class IdentityServerConfig
{
    public const string Name = "Identity";

    public Uri Authority { get; set; }

    public string RedirectToStartPageUrl { get; set; }
    
    public string RedirectConfirmationUrl { get; set; }
}