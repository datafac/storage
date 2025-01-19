using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DTOMaker.Gentime
{
    internal sealed class TokenStack
    {
        private class Disposer : IDisposable
        {
            private readonly Stack<ImmutableDictionary<string, object?>> _stack;
            public Disposer(Stack<ImmutableDictionary<string, object?>> stack) => _stack = stack;

            private volatile bool _disposed;
            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _stack.Pop();
            }
        }

        private readonly Stack<ImmutableDictionary<string, object?>> _stack = new Stack<ImmutableDictionary<string, object?>>();
        public TokenStack() => _stack.Push(ImmutableDictionary<string, object?>.Empty);
        public ImmutableDictionary<string, object?> Top => _stack.Peek();
        public IDisposable NewScope(IEnumerable<KeyValuePair<string, object?>> tokens)
        {
            _stack.Push(_stack.Peek().SetItems(tokens));
            return new Disposer(_stack);
        }
    }
}
