using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace OutOfSchool.AuthServer.Tests.Util;

public class TestServerCallContext : ServerCallContext
{
    protected override IDictionary<object, object> UserStateCore { get; }

    public TestServerCallContext(IDictionary<object, object> userState)
    {
        this.UserStateCore = userState;
    }
    
    // Following methods & properties are not used in test
    protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
    {
        return Task.CompletedTask;
    }

    protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions options)
    {
        throw new InvalidOperationException("Not used in test");
    }

    protected override string MethodCore { get; }
    protected override string HostCore { get; }
    protected override string PeerCore { get; }
    protected override DateTime DeadlineCore { get; }
    protected override Metadata RequestHeadersCore { get; }
    protected override CancellationToken CancellationTokenCore { get; }
    protected override Metadata ResponseTrailersCore { get; }
    protected override Status StatusCore { get; set; }
    protected override WriteOptions WriteOptionsCore { get; set; }
    protected override AuthContext AuthContextCore { get; }
}