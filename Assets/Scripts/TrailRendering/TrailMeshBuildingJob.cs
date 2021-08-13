using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TrailRendering
{
    [BurstCompile]
    public struct TrailMeshBuildingJob : IJobParallelFor
    {
        [ReadOnly] public TrailBuffer inTrails;
        [ReadOnly] public NativeArray<int> inIndicesOffsets;
        [ReadOnly] public NativeArray<int> inVerticesOffsets;
        [ReadOnly] public float inTrailWidth;
        [ReadOnly] public Vector3 inCameraPosition;

        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<Vector3> outVertices;
        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<ushort> outIndices;

        private int _trailIndex;
        private int _segmentCount;
        
        public void Execute(int trailIndex)
        {
            _trailIndex = trailIndex;
            _segmentCount = inTrails.segmentsCounts[trailIndex];
            if (_segmentCount == 0)
            {
                return;
            }
            
            var vertexOffset = inVerticesOffsets[trailIndex];
            var indexOffset = inIndicesOffsets[trailIndex];
            var particlePosition = inTrails.particles[trailIndex];

            var memorySegmentCount = Math.Min(inTrails.maxSegmentPerTrail, _segmentCount);
            if (memorySegmentCount == 1)
            {
                StartTrail(ref vertexOffset, ref indexOffset, GetTrailPoint(0), particlePosition);
                return;
            }

            var prevPoint = GetTrailPoint(1);
            StartTrail(ref vertexOffset, ref indexOffset, GetTrailPoint(0), prevPoint);
            
            for (var i = 2; i < memorySegmentCount; i++)
            {
                var newPoint = GetTrailPoint(i);
                ContinueTrail(ref vertexOffset, ref indexOffset, prevPoint, newPoint);
                prevPoint = newPoint;
            }
            
            ContinueTrail(ref vertexOffset, ref indexOffset, prevPoint, particlePosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly Vector3 GetTrailPoint(int pointIndex)
        {
            if (_segmentCount > inTrails.maxSegmentPerTrail)
            {
                var offset = (_segmentCount + pointIndex) % inTrails.maxSegmentPerTrail;
                return inTrails.segments[_trailIndex * inTrails.maxSegmentPerTrail + offset];
            }
            
            return inTrails.segments[_trailIndex * inTrails.maxSegmentPerTrail + pointIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ContinueTrail(ref int vertexOffset, ref int indexOffset, Vector3 pointA, Vector3 pointB)
        {
            FlushQuadIndices(ref indexOffset, vertexOffset - 2);

            var minusCameraForward = inCameraPosition - pointB;
            var trailDirection = pointB - pointA;
            var trailUp = Vector3.Cross(minusCameraForward, trailDirection).normalized * inTrailWidth;

            outVertices[vertexOffset++] = pointB - trailUp;
            outVertices[vertexOffset++] = pointB + trailUp;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StartTrail(ref int vertexOffset, ref int indexOffset, Vector3 pointA, Vector3 pointB)
        {
            FlushQuadIndices(ref indexOffset, vertexOffset);

            var minusCameraForward = inCameraPosition - pointB;
            var trailDirection = pointB - pointA;
            var trailUp = Vector3.Cross(minusCameraForward, trailDirection).normalized * inTrailWidth;

            outVertices[vertexOffset++] = pointA - trailUp;
            outVertices[vertexOffset++] = pointA + trailUp;
            outVertices[vertexOffset++] = pointB - trailUp;
            outVertices[vertexOffset++] = pointB + trailUp;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FlushQuadIndices(ref int indexOffset, int vertexOffset)
        {
            outIndices[indexOffset++] = (ushort) (vertexOffset + 0);
            outIndices[indexOffset++] = (ushort) (vertexOffset + 2);
            outIndices[indexOffset++] = (ushort) (vertexOffset + 3);
            outIndices[indexOffset++] = (ushort) (vertexOffset + 1);
            outIndices[indexOffset++] = (ushort) (vertexOffset + 0);
            outIndices[indexOffset++] = (ushort) (vertexOffset + 3);
        }
    }
}