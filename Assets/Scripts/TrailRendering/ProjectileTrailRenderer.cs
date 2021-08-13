using System;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace TrailRendering
{
    public class ProjectileTrailRenderer : MonoBehaviour
    {
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private float _trailWidth;
        [SerializeField] private Transform _cameraTransform;
        
        private TrailBuffer _trails;
        private Mesh _mesh;
        private VertexAttributeDescriptor[] _meshLayout;

        public void Start()
        {
            _mesh = new Mesh();
            _mesh.name = nameof(ProjectileTrailRenderer);
            _mesh.MarkDynamic();
            _meshFilter.mesh = _mesh;
            _meshLayout = new VertexAttributeDescriptor[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3)
            };
            _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 9999f);
        }

        public void SetBuffer(TrailBuffer trailBuffer)
        {
            _trails = trailBuffer;
        }

        private void LateUpdate()
        {
            var vertexCount = 0;
            var indexCount = 0;
            var vertexWriteOffsets = NativeMemory.CreateTempJobArray<int>(_trails.maxTrailCount);
            var indexWriteOffsets = NativeMemory.CreateTempJobArray<int>(_trails.maxTrailCount);
            for (var i = 0; i < _trails.maxTrailCount; i++)
            {
                var segmentCount = _trails.segmentsCounts[i];
                if (segmentCount == 0)
                {
                    continue;
                }

                vertexWriteOffsets[i] = vertexCount;
                indexWriteOffsets[i] = indexCount;

                var memorySegmentCount = Math.Min(_trails.maxSegmentPerTrail, segmentCount);
                vertexCount += (memorySegmentCount + 1) * 2;
                indexCount += memorySegmentCount * 6;
            }

            var buildMeshJob = new TrailMeshBuildingJob
            {
                inIndicesOffsets = indexWriteOffsets,
                inVerticesOffsets = vertexWriteOffsets,
                inTrailWidth = _trailWidth,
                inTrails = _trails,
                inCameraPosition = _cameraTransform.position,
                outIndices = NativeMemory.CreateTempJobArray<ushort>(indexCount),
                outVertices = NativeMemory.CreateTempJobArray<Vector3>(vertexCount)
            };
            buildMeshJob.Schedule(_trails.maxTrailCount, 32).Complete();
            
            _mesh.SetVertexBufferParams(vertexCount, _meshLayout);
            _mesh.SetIndexBufferParams(indexCount, IndexFormat.UInt16);
            _mesh.SetVertexBufferData(buildMeshJob.outVertices, 0, 0, vertexCount);
            _mesh.SetIndexBufferData(buildMeshJob.outIndices, 0, 0, indexCount);
            _mesh.subMeshCount = 1;
            _mesh.SetSubMesh(0, new SubMeshDescriptor(0, indexCount));

            vertexWriteOffsets.Dispose();
            indexWriteOffsets.Dispose();
            buildMeshJob.outIndices.Dispose();
            buildMeshJob.outVertices.Dispose();
        }
    }
}