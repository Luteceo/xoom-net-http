// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Vlingo.Xoom.Common;
using Vlingo.Xoom.Http.Resource;
using Vlingo.Xoom.Http.Tests.Sample.User.Model;
using Vlingo.Xoom.Wire.Nodes;
using Xunit;
using Xunit.Abstractions;

namespace Vlingo.Xoom.Http.Tests.Resource
{
    public class ClientTest : ResourceTestFixtures
    {
        private static readonly AtomicInteger NextPort = new AtomicInteger(8087);
        
        private readonly ITestOutputHelper _output;
        private Client _client;
        private int _expectedHeaderCount;
        private Response _expectedResponse;
        private ResponseHeader _location;
        private readonly IServer _server;

        [Fact]
        public void TestThatCorrelatingClientDelivers()
        {
            var user = JohnDoeUserSerialized;

            var safely = new TestResponseConsumer(_output);
            var access = safely.AfterCompleting(1);
            var unknown = new UnknownResponseConsumer(access, _output);
            var known = new KnownResponseConsumer(access);

            var config = Client.Configuration.DefaultedExceptFor(
                World.Stage,
                Address.From(Host.Of("localhost"), NextPort.Get(), AddressType.None), unknown);
            _client = Client.Using(config) ;

            _client.RequestWith(
                    Request
                        .Has(Method.Post)
                        .And("/users".ToMatchableUri())
                        .And(RequestHeader.WithHost("localhost"))
                        .And(RequestHeader.WithContentLength(user))
                        .And(RequestHeader.WithKeepAlive())
                        .And(Body.From(user)))
                .AndThenConsume(TimeSpan.FromMilliseconds(5000), Response.Of(ResponseStatus.RequestTimeout), response => _expectedResponse = response)
                .AndThenConsume(response => _expectedHeaderCount = response.Headers.Count)
                .AndThenConsume(response => _location = response.Headers.HeaderOf(ResponseHeader.Location))
                .AndThenConsume(known.Consume);

            var responseCount = access.ReadFrom<int>("responseCount");
            var unknownResponseCount = access.ReadFrom<int>("unknownResponseCount");

            Assert.Equal(1, responseCount);
            Assert.NotNull(_expectedResponse);
            Assert.Equal(ResponseStatus.Created, _expectedResponse.Status);
            Assert.Equal(3, _expectedHeaderCount);
            Assert.NotNull(_location);
            Assert.Equal(0, unknownResponseCount);
        }
        
        [Fact]
        public void TestThatRoundRobinClientDelivers()
        {
            var safely = new TestResponseConsumer(_output);
            var access = safely.AfterCompleting(10);
            var unknown = new UnknownResponseConsumer(access, _output);
            var known = new KnownResponseConsumer(access);

            var config = Client.Configuration.DefaultedExceptFor(
                World.Stage,
                Address.From(Host.Of("localhost"), NextPort.Get(), AddressType.None), unknown);
            config.TestInfo(true);

            _client =
                Client.Using(
                config,
                Client.ClientConsumerType.RoundRobin,
                5);

            for (var count = 0; count < 100; ++count)
            {
                var user = count % 2 == 0 ? UniqueJohnDoe() : UniqueJaneDoe();
                _client.RequestWith(
                        Request
                            .Has(Method.Post)
                            .And("/users".ToMatchableUri())
                            .And(RequestHeader.WithHost("localhost"))
                            .And(RequestHeader.WithContentLength(user))
                            .And(RequestHeader.WithKeepAlive())
                            .And(Body.From(user)))
                    .AndThenConsume(response => known.Consume(response));
            }
            
            var responseCount = access.ReadFromExpecting("responseCount", 100, 10000);
            var total = access.ReadFrom<int>("totalAllResponseCount");
            var unknownResponseCount = access.ReadFrom<int>("unknownResponseCount");
            var clientCounts = access.ReadFrom<Dictionary<string, int>>("responseClientCounts");
            
            Assert.Equal(100, total);
            Assert.Equal(100, responseCount);
            Assert.Equal(0, unknownResponseCount);
            
            foreach (var id in clientCounts.Keys)
            {
                var clientCount = clientCounts[id];
                Assert.Equal(20, clientCount);
            }
        }
        
        [Fact]
        public void TestThatLoadBalancingClientDelivers()
        {
            var safely = new TestResponseConsumer(_output);
            var access = safely.AfterCompleting(100);
            var unknown = new UnknownResponseConsumer(access, _output);
            var known = new KnownResponseConsumer(access);

            var config = Client.Configuration.DefaultedExceptFor(
                World.Stage,
                Address.From(Host.Of("localhost"), NextPort.Get(), AddressType.None), unknown);
            config.TestInfo(true);

            _client =
                Client.Using(
                    config,
                    Client.ClientConsumerType.LoadBalancing,
                    5);

            for (var count = 0; count < 100; ++count)
            {
                var user = count % 2 == 0 ? UniqueJohnDoe() : UniqueJaneDoe();
                _client.RequestWith(
                        Request
                            .Has(Method.Post)
                            .And("/users".ToMatchableUri())
                            .And(RequestHeader.WithHost("localhost"))
                            .And(RequestHeader.WithContentLength(user))
                            .And(RequestHeader.WithKeepAlive())
                            .And(Body.From(user)))
                    .AndThenConsume(response => known.Consume(response));
            }

            var responseCount = access.ReadFromExpecting("responseCount", 100, 10000);
            var total = access.ReadFrom<int>("totalAllResponseCount");
            var unknownResponseCount = access.ReadFrom<int>("unknownResponseCount");
            var clientCounts = access.ReadFrom<Dictionary<string, int>>("responseClientCounts");

            Assert.Equal(100, total);
            Assert.Equal(100, responseCount);
            Assert.Equal(0, unknownResponseCount);

            var totalClientCounts = 0;
            foreach (var id in clientCounts.Keys)
            {
                var clientCount = clientCounts[id];
                totalClientCounts += clientCount;
            }

            Assert.Equal(100, totalClientCounts);
        }

        public ClientTest(ITestOutputHelper output) : base(output)
        {
            _output = output;
            UserStateFactory.ResetId();

            _server = ServerFactory.StartWith(World.Stage, Resources, NextPort.IncrementAndGet(), new Configuration.SizingConf(1, 10, 100, 10240), new Configuration.TimingConf(2, 2, 100));
            Assert.True(_server.StartUp().Await(TimeSpan.FromMilliseconds(500L)));
        }

        public override void Dispose()
        {
            _output.WriteLine($"{DateTimeOffset.Now:MM/dd/yyyy hh:mm:ss.fff} Disposing client...");
            _client?.Close();
            _server?.Stop();

            base.Dispose();
        }
    }
}