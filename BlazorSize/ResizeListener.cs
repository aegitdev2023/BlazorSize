﻿using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace BlazorPro.BlazorSize
{
    public class ResizeListener
    {
        const string ns = "blazorSize";
        private readonly IJSRuntime jsRuntime;
        private readonly ResizeOptions options;
        private bool disposed;
        public ResizeListener(IJSRuntime jsRuntime, IOptions<ResizeOptions> options = null)
        {
            this.options = options.Value ?? new ResizeOptions();
            this.jsRuntime = jsRuntime;
        }

#nullable enable
        private EventHandler<BrowserWindowSize>? onResized;
        public event EventHandler<BrowserWindowSize>? OnResized
        {
            add => Subscribe(value);
            remove => Unsubscribe(value);
        }
#nullable disable

        private void Unsubscribe(EventHandler<BrowserWindowSize> value)
        {
            onResized -= value;
            if (onResized == null)
            {
                Cancel().ConfigureAwait(false);
            }
        }

        private void Subscribe(EventHandler<BrowserWindowSize> value)
        {
            if (onResized == null)
            {
                Start().ConfigureAwait(false);
            }
            onResized += value;
        }

        private async ValueTask<bool> Start() =>
            await jsRuntime.InvokeAsync<bool>($"{ns}.listenForResize", DotNetObjectReference.Create(this), options);

        private async ValueTask Cancel() =>
            await jsRuntime.InvokeVoidAsync($"{ns}.cancelListener");

        public async ValueTask<bool> MatchMedia(string mediaQuery) =>
            await jsRuntime.InvokeAsync<bool>($"{ns}.matchMedia", mediaQuery);

        public async ValueTask<BrowserWindowSize> GetBrowserWindowSize() =>
            await jsRuntime.InvokeAsync<BrowserWindowSize>($"{ns}.getBrowserWindowSize");

        [JSInvokable]
        public void RaiseOnResized(BrowserWindowSize browserWindowSize) =>
            onResized?.Invoke(this, browserWindowSize);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    onResized = null;
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
