﻿// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Actors;

namespace Vlingo.Xoom.Http.Resource
{
    /// <summary>
    /// Sends <code>Request</code> messages in behalf of a client.
    /// </summary>
    public interface IRequestSender : IStoppable
    {
        /// <summary>
        /// Sends the <paramref name="request"/>.
        /// </summary>
        /// <param name="request">The <code>Request</code> to send.</param>
        void SendRequest(Request request);
    }
}
