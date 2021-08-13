using System.Runtime.CompilerServices;
using Unity.Collections;

namespace TrailRendering
{
    public struct NativeMemory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeArray<T> CreateTempJobArray<T>(int length) where T : unmanaged
        {
            return new NativeArray<T>(length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        }
    }
}