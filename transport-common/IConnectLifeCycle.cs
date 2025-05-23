﻿using transport_common.Exceptions;

namespace transport_common
{
    public abstract class IConnectLifeCycle : IDisposable
    {

        private volatile ConnectStatus _status = ConnectStatus.NEW;
        public ConnectStatus Status => _status;

        private volatile bool _disposed = false;
        public bool Disposed => _disposed;

        private event EventHandler<(long key, ConnectStatus status)> _ConnectionStatusChanged = (sender, objetc) => { };
        public event EventHandler<(long key, ConnectStatus status)> ConnectionStatusChanged
        {
            add => _ConnectionStatusChanged += value;
            remove => _ConnectionStatusChanged -= value;
        }

        private readonly SemaphoreSlim ObjectLock = new(1, 1); // 对象锁

        public abstract long GetKey();

        public async Task Init()
        {
            await ObjectLock.WaitAsync();
            try
            {
                if (Status != ConnectStatus.NEW) throw new LifeCycleException();
                ChangeStatus(ConnectStatus.INIT);
                await InitAsync();
            }
            finally
            {
                ObjectLock.Release();
            }

        }
        protected abstract Task InitAsync();


        public async Task Connect()
        {
            await ObjectLock.WaitAsync();
            try
            {
                if (Status != ConnectStatus.INIT) throw new LifeCycleException();
                ChangeStatus(ConnectStatus.CONNECTING);
                await ConnectAsync();
            }
            finally
            {
                ObjectLock.Release();
            }
        }
        protected abstract Task ConnectAsync();


        public async Task Disconnect()
        {
            await ObjectLock.WaitAsync();
            try
            {
                ChangeStatus(ConnectStatus.DISCONNECT);
                await DisconnectAsync();
            }
            finally
            {
                ObjectLock.Release();
            }

        }
        protected abstract Task DisconnectAsync();

        public async Task Destory()
        {
            _disposed = true;

            // 先判断是否需要 Disconnect（不加锁），避免递归死锁
            bool needDisconnect = Status != ConnectStatus.DISCONNECT;

            if (needDisconnect)
            {
                await Disconnect(); // 这里自己内部加锁
            }

            await ObjectLock.WaitAsync();
            try
            {
                ChangeStatus(ConnectStatus.DESTROY);
                await DestroyAsync();
            }
            finally
            {
                ObjectLock.Release();
            }
        }

        public Task DestroyAsync()
        {
            Dispose();
            ObjectLock.Dispose();
            return Task.CompletedTask;
        }

        public abstract void Dispose();

        protected void ChangeStatus(ConnectStatus status)
        {
            lock (this)
            {
                if (status == _status) return;
                if (Status == ConnectStatus.DESTROY) return;
                _status = status;
                _ConnectionStatusChanged?.Invoke(this, (GetKey(), Status));
            }
        }
    }
}
