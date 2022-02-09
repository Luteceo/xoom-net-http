﻿// Copyright (c) 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;

namespace Vlingo.Xoom.Http.Resource
{
    public class MediaTypeNotSupportedException : Exception
    {
        private readonly string _mediaType;

        public MediaTypeNotSupportedException(string mediaType)
        {
            _mediaType = mediaType;
        }

        public override string Message => $"No mapper registered for the following media mimeType: {_mediaType}";
    }
}
