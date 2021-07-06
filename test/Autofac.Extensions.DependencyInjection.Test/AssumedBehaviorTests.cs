﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Autofac.Extensions.DependencyInjection.Test
{
    /// <summary>
    /// Additional tests to illustrate undocumented yet assumed behaviors in
    /// the Microsoft.Extensions.DependencyInjection container/scope.
    /// </summary>
    public abstract class AssumedBehaviorTests
    {
        [Fact]
        public void DisposingScopeAlsoDisposesServiceProvider()
        {
            // You can't resolve things from a scope's service provider
            // if you dispose the scope.
            var services = new ServiceCollection().AddScoped<DisposeTracker>();
            var rootProvider = CreateServiceProvider(services);
            var scope = rootProvider.CreateScope();
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<DisposeTracker>());
            scope.Dispose();
            Assert.Throws<ObjectDisposedException>(() => scope.ServiceProvider.GetRequiredService<DisposeTracker>());
        }

        [Fact]
        public void DisposingScopeAndProviderOnlyDisposesObjectsOnce()
        {
            // Disposing the service provider and then the scope only
            // runs one disposal on the resolved objects.
            var services = new ServiceCollection().AddScoped<DisposeTracker>();
            var rootProvider = CreateServiceProvider(services);
            var scope = rootProvider.CreateScope();
            var tracker = scope.ServiceProvider.GetRequiredService<DisposeTracker>();
            ((IDisposable)scope.ServiceProvider).Dispose();
            Assert.True(tracker.Disposed);
            Assert.Equal(1, tracker.DisposeCount);
            scope.Dispose();
            Assert.Equal(1, tracker.DisposeCount);
        }

        [Fact]
        public void DisposingScopeServiceProviderStopsNewScopes()
        {
            // You can't create a new child scope if you've disposed of
            // the parent scope service provider.
            var rootProvider = CreateServiceProvider(new ServiceCollection());
            var scope = rootProvider.CreateScope();
            ((IDisposable)scope.ServiceProvider).Dispose();
            Assert.Throws<ObjectDisposedException>(() => scope.ServiceProvider.CreateScope());
        }

        [Fact]
        public void DisposingScopeServiceProviderStopsScopeResolutions()
        {
            // You can't resolve things from a scope if you dispose the
            // scope's service provider.
            var services = new ServiceCollection().AddScoped<DisposeTracker>();
            var rootProvider = CreateServiceProvider(services);
            var scope = rootProvider.CreateScope();
            Assert.NotNull(scope.ServiceProvider.GetRequiredService<DisposeTracker>());
            ((IDisposable)scope.ServiceProvider).Dispose();
            Assert.Throws<ObjectDisposedException>(() => scope.ServiceProvider.GetRequiredService<DisposeTracker>());
        }

        [Fact]
        public void ResolvedProviderNotSameAsParent()
        {
            // Resolving a provider from another provider yields a new object.
            // (It's not just returning "this" - it's a different IServiceProvider.)
            var parent = CreateServiceProvider(new ServiceCollection());
            var resolved = parent.GetRequiredService<IServiceProvider>();
            Assert.NotSame(parent, resolved);
        }

        [Fact]
        public void ResolvedProviderUsesSameScopeAsParent()
        {
            // Resolving a provider from another provider will still resolve
            // items from the same scope.
            var services = new ServiceCollection().AddScoped<DisposeTracker>();
            var root = CreateServiceProvider(services);
            var scope = root.CreateScope();
            var parent = scope.ServiceProvider;
            var resolved = parent.GetRequiredService<IServiceProvider>();
            Assert.Same(parent.GetRequiredService<DisposeTracker>(), resolved.GetRequiredService<DisposeTracker>());
        }

        [Fact]
        public void ServiceProviderWillNotResolveAfterDispose()
        {
            // You can't resolve things from a service provider
            // if you dispose it.
            var services = new ServiceCollection().AddScoped<DisposeTracker>();
            var rootProvider = CreateServiceProvider(services);
            Assert.NotNull(rootProvider.GetRequiredService<DisposeTracker>());
            ((IDisposable)rootProvider).Dispose();
            Assert.Throws<ObjectDisposedException>(() => rootProvider.GetRequiredService<DisposeTracker>());
        }

        [Fact]
        public async ValueTask ServiceProviderDisposesAsync()
        {
            // You can't resolve things from a service provider
            // if you dispose it.
            var services = new ServiceCollection().AddScoped<AsyncDisposeTracker>();
            var rootProvider = CreateServiceProvider(services);
            var tracker = rootProvider.GetRequiredService<AsyncDisposeTracker>();
            var asyncDisposer = (IAsyncDisposable)rootProvider;

            await asyncDisposer.DisposeAsync();

            Assert.True(tracker.AsyncDisposed);
            Assert.False(tracker.SyncDisposed);
        }

        protected abstract IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection);

        private class DisposeTracker : IDisposable
        {
            public int DisposeCount { get; set; }

            public bool Disposed { get; set; }

            public void Dispose()
            {
                Disposed = true;
                DisposeCount++;
            }
        }

        private class AsyncDisposeTracker : IDisposable, IAsyncDisposable
        {
            public bool SyncDisposed { get; set; }

            public bool AsyncDisposed { get; set; }

            public void Dispose()
            {
                SyncDisposed = true;
            }

            public async ValueTask DisposeAsync()
            {
                await Task.Delay(1);

                AsyncDisposed = true;
            }
        }
    }
}
