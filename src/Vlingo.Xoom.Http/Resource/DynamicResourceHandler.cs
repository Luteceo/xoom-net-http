// Copyright © 2012-2022 VLINGO LABS. All rights reserved.
//
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v. 2.0. If a copy of the MPL
// was not distributed with this file, You can obtain
// one at https://mozilla.org/MPL/2.0/.

using Vlingo.Xoom.Actors;
using Vlingo.Xoom.Common;

namespace Vlingo.Xoom.Http.Resource;

/// <summary>
/// An optional base class that may be used by resources configured using
/// the fluent API provided by <see cref="ResourceBuilder"/>.
/// </summary>
public abstract class DynamicResourceHandler
{
    /// <summary>
    /// Constructs my default state with the <see cref="Stage"/>
    /// </summary>
    /// <param name="stage">The Stage that manages my state and Actor-based execution</param>
    protected DynamicResourceHandler(Stage stage)
    {
        Stage = stage;
        Logger = Stage.World.DefaultLogger;
    }

    /// <summary>
    /// Gets the <see cref="Resource"/> that maps my behaviors, which are fluently
    /// configured using a <see cref="ResourceBuilder"/>.
    /// </summary>
    public abstract Resource Routes { get; }

    /// <summary>
    /// Gets <see cref="ICompletesEventually"/> for the current context.
    /// </summary>
    protected internal ICompletesEventually? Completes => Context?.Completes;
        
    /// <summary>
    /// Gets <see cref="ContentType"/> which is by default <c>"text/plain", "us-ascii"</c>
    /// </summary>
    protected internal ContentType ContentType => ContentType.Of("text/plain", "us-ascii");

    /// <summary>
    /// Used by the internal <see cref="IServer"/> runtime to set <see cref="Context"/>
    /// for the current <see cref="Request"/> that will be subsequently handling.
    /// </summary>
    /// <param name="context">The <see cref="Context"/> for the current <see cref="Request"/> that will be subsequently handling</param>
    protected internal void SetContext(Context context) => Context = context;
        
    /// <summary>
    /// Gets the current <see cref="Context"/>.
    /// </summary>
    protected internal Context? Context { get; private set; }
        
    /// <summary>
    /// Answer a <see cref="Response"/> with the <see cref="ResponseStatus"/> and <paramref name="entity"/>
    /// with a <code>Content-Type</code> header per my <code>ContentType</code>, which may be overridden.
    /// </summary>
    /// <param name="status">The status of the response</param>
    /// <param name="entity">The string entity of the response</param>
    /// <returns><see cref="Response"/></returns>
    protected Response EntityResponseOf(ResponseStatus status, string entity) =>
        EntityResponseOf(status, Headers.Empty<ResponseHeader>(), entity);

    /// <summary>
    /// Answer a <see cref="Response"/> with the <see cref="ResponseStatus"/> and <paramref name="entity"/>
    /// with a <code>Content-Type</code> header per my <code>ContentType</code>, which may be overridden.
    /// </summary>
    /// <param name="status">The status of the response</param>
    /// <param name="headers">The <see cref="Headers{ResponseHeader}"/> to which the <code>Content-Type</code> header is appended</param>
    /// <param name="entity">The string entity of the response</param>
    /// <returns><see cref="Response"/></returns>
    protected Response EntityResponseOf(ResponseStatus status, Headers<ResponseHeader> headers, string entity) =>
        Response.Of(status, headers.And(ContentType.ToResponseHeader()), entity);

    /// <summary>
    /// Gets the <see cref="ILogger"/> which is the default logger of the <see cref="World"/>
    /// </summary>
    protected internal ILogger Logger { get; }
        
    /// <summary>
    /// Gets the <see cref="Stage"/>, within which my backing <see cref="Actor"/> resides.
    /// </summary>
    protected internal Stage Stage { get; }

    /// <summary>
    /// Gets the <see cref="Scheduler"/> owned by <see cref="Stage"/>
    /// </summary>
    protected internal Scheduler Scheduler => Stage.Scheduler;
}