using System;
using Unity.Collections;
using UnityEngine;

namespace TrailRendering
{
    public struct TrailBuffer : IDisposable
    {
        public readonly int maxTrailCount;
        public readonly int maxSegmentPerTrail;
        public readonly float segmentLength;
        
        public NativeArray<int> segmentsCounts;
        public NativeArray<Vector3> segments;
        public NativeArray<Vector3> particles;

        public TrailBuffer(int maxTrailCount, int maxSegmentPerTrail, float segmentLength)
        {
            this.segmentLength = segmentLength;
            this.maxTrailCount = maxTrailCount;
            this.maxSegmentPerTrail = maxSegmentPerTrail;

            segmentsCounts = new NativeArray<int>(maxTrailCount, Allocator.Persistent);
            segments = new NativeArray<Vector3>(maxTrailCount * maxSegmentPerTrail, Allocator.Persistent);
            particles = new NativeArray<Vector3>(maxTrailCount, Allocator.Persistent);
        }

        public void Dispose()
        {
            segmentsCounts.Dispose();
            segments.Dispose();
            particles.Dispose();
        }
    }
}