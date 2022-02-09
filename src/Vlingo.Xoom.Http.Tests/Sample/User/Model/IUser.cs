﻿// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Http.Tests.Sample.User.Model
{
    public interface IUser
    {
        ICompletes<UserState> WithContact(Contact contact);
        ICompletes<UserState> WithName(Name name);
    }
}