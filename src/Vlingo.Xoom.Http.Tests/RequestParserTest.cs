﻿// Copyright (c) 2012-2021 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Text;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Http.Tests.Resource;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Xoom.Http.Tests
{
    public class RequestParserTest : ResourceTestFixtures
    {
        private readonly List<string> _uniqueBodies = new List<string>();

        [Fact]
        public void TestThatSingleResponseParses()
        {
            var parser = RequestParser.ParserFor(ToByteBuffer(PostJohnDoeUserMessage));

            Assert.True(parser.HasCompleted);
            Assert.True(parser.HasFullRequest());
            Assert.False(parser.IsMissingContent);
            Assert.False(parser.HasMissingContentTimeExpired((long) DateExtensions.GetCurrentMillis() + 100));

            var request = parser.FullRequest();

            Assert.NotNull(request);
            Assert.True(request.Method.IsPost());
            Assert.Equal("/users", request.Uri.PathAndQuery);
            Assert.True(request.Version.IsHttp1_1());
            Assert.Equal(JohnDoeUserSerialized, request.Body.Content);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(200)]
        public void TestThatMultipleResponsesParse(int requests)
        {
            var parser = RequestParser.ParserFor(ToByteBuffer(MultipleRequestBuilder(requests)));

            Assert.True(parser.HasCompleted);
            Assert.True(parser.HasFullRequest());
            Assert.False(parser.IsMissingContent);
            Assert.False(parser.HasMissingContentTimeExpired((long) DateExtensions.GetCurrentMillis() + 100));

            var count = 0;
            var bodyIterator = _uniqueBodies.GetEnumerator();
            while (parser.HasFullRequest())
            {
                ++count;
                var request = parser.FullRequest();

                Assert.NotNull(request);
                Assert.True(request.Method.IsPost());
                Assert.Equal("/users", request.Uri.PathAndQuery);
                Assert.True(request.Version.IsHttp1_1());
                Assert.True(bodyIterator.MoveNext());
                var body = bodyIterator.Current;
                Assert.Equal(body, request.Body.Content);
            }

            Assert.Equal(requests, count);
            
            bodyIterator.Dispose();
        }

        [Fact]
        public void TestThatTwoHundredResponsesParseNextSucceeds()
        {
            var manyRequests = MultipleRequestBuilder(200);

            var totalLength = manyRequests.Length;
            var random = new Random();
            var alteringEndIndex = 1024;
            var parser = RequestParser.ParserFor(ToByteBuffer(manyRequests.Substring(0, alteringEndIndex)));
            var startingIndex = alteringEndIndex;

            while (startingIndex < totalLength)
            {
                var randomLength = random.Next(512) + 1;
                alteringEndIndex = startingIndex + randomLength + (int)(DateExtensions.GetCurrentMillis() % startingIndex);
                if (alteringEndIndex > totalLength)
                {
                    alteringEndIndex = totalLength;
                }

                parser.ParseNext(ToByteBuffer(manyRequests.Substring(startingIndex, alteringEndIndex - startingIndex)));
                startingIndex = alteringEndIndex;
            }

            Assert.True(parser.HasCompleted);
            Assert.True(parser.HasFullRequest());
            Assert.False(parser.IsMissingContent);
            Assert.False(parser.HasMissingContentTimeExpired((long) DateExtensions.GetCurrentMillis() + 100));

            var count = 0;
            var bodyIterator = _uniqueBodies.GetEnumerator();
            while (parser.HasFullRequest())
            {
                ++count;
                var request = parser.FullRequest();

                Assert.NotNull(request);
                Assert.True(request.Method.IsPost());
                Assert.Equal("/users", request.Uri.PathAndQuery);
                Assert.True(request.Version.IsHttp1_1());
                Assert.True(bodyIterator.MoveNext());
                var body = bodyIterator.Current;
                Assert.Equal(body, request.Body.Content);
            }

            Assert.Equal(200, count);
            
            bodyIterator.Dispose();
        }

        private string MultipleRequestBuilder(int amount)
        {
            var builder = new StringBuilder();

            for (var idx = 1; idx <= amount; ++idx)
            {
                var body = (idx % 2 == 0) ? UniqueJaneDoe() : UniqueJohnDoe();
                _uniqueBodies.Add(body);
                builder.Append(PostRequestCloseFollowing(body));
            }

            return builder.ToString();
        }

        public RequestParserTest(ITestOutputHelper output) : base(output)
        {
        }
    }
}