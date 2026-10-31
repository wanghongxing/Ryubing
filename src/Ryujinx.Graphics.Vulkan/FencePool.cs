using Silk.NET.Vulkan;
using System;
using System.Collections.Concurrent;

namespace Ryujinx.Graphics.Vulkan
{
    class FencePool : IDisposable
    {
        private const int MaxCachedFences = 32;
        private readonly Vk _api;
        private readonly Device _device;
        private readonly ConcurrentStack<Fence> _free;
        private volatile bool _disposed;

        public FencePool(Vk api, Device device)
        {
            _api = api;
            _device = device;
            _free = new ConcurrentStack<Fence>();
        }

        public unsafe Fence Acquire()
        {
            if (!_free.TryPop(out Fence fence))
            {
                FenceCreateInfo fenceCreateInfo = new()
                {
                    SType = StructureType.FenceCreateInfo,
                };

                _api.CreateFence(_device, in fenceCreateInfo, null, out fence).ThrowOnError();
            }

            return fence;
        }

        public unsafe void Release(Fence fence)
        {
            if (!_disposed && _free.Count < MaxCachedFences)
            {
                _api.ResetFences(_device, 1, &fence).ThrowOnError();
                _free.Push(fence);
                return;
            }

            DestroyFence(fence);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            
            _disposed = true;

            while (_free.TryPop(out Fence fence))
            {
                DestroyFence(fence);
            }
        }

        private void DestroyFence(Fence fence)
        {
            _api.DestroyFence(_device, fence, Span<AllocationCallbacks>.Empty);
        }
    }
}
