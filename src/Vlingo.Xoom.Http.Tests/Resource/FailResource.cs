// Copyright © 2012-2023 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Common;
using Vlingo.Xoom.Http.Resource;
using Xunit.Abstractions;

namespace Vlingo.Xoom.Http.Tests.Resource;

public class FailResource : ResourceHandler
{
    private readonly ITestOutputHelper _output;

    public FailResource(ITestOutputHelper output) => _output = output;

    public ICompletes<Response> Query()
    {
        _output.WriteLine("QUERY");
        return Xoom.Common.Completes.WithFailure(Response.Of(ResponseStatus.BadRequest));
    }

    public override Http.Resource.Resource Routes() 
        => ResourceBuilder.Resource("Failure API", ResourceBuilder.Get("/fail").Handle(Query));
}