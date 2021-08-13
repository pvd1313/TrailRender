using Unity.Collections;
using UnityEngine;

namespace TrailRendering
{
    public class GunHandler : MonoBehaviour
    {
        [SerializeField] private Gun _gun;
        [SerializeField] private int _maxSegmentPerTrail;
        [SerializeField] private float _segmentLength;
        [SerializeField] private ProjectileTrailRenderer _trailRenderer;
        
        private TrailBuffer _trails;
        private NativeArray<Vector3> _lastSegments;
        
        private void Start()
        {
            _trails = new TrailBuffer(_gun.maxProjectileCount, _maxSegmentPerTrail, _segmentLength);
            _lastSegments = new NativeArray<Vector3>(_gun.maxProjectileCount, Allocator.Persistent);
            _trailRenderer.SetBuffer(_trails);
            
            _gun.onProjectileCreated += OnProjectileCreated;
            _gun.onProjectileMoved += OnProjectileMoved;
            _gun.onProjectileRemoved += OnProjectileRemoved;
        }

        private void OnProjectileCreated(int index, ref Gun.Projectile projectile)
        {
            _trails.particles[index] = projectile.position;
            _trails.segments[index * _maxSegmentPerTrail] = projectile.position;
            _trails.segmentsCounts[index] = 1;
            _lastSegments[index] = projectile.position;
        }
        
        private void OnProjectileMoved(int index, ref Gun.Projectile projectile)
        {
            _trails.particles[index] = projectile.position;

            var prevSegmentPos = _lastSegments[index];
            var direction = projectile.position - prevSegmentPos;
            var mag = direction.magnitude;
            if (mag > _trails.segmentLength)
            {
                var segmentPos = prevSegmentPos + (direction / mag) * _trails.segmentLength;
                var segmentCount = _trails.segmentsCounts[index];
                var segmentOffset = index * _trails.maxSegmentPerTrail + segmentCount % _trails.maxSegmentPerTrail;
                _trails.segments[segmentOffset] = segmentPos;
                _trails.segmentsCounts[index] = segmentCount + 1;
                _lastSegments[index] = segmentPos;
            }
        }
        
        private void OnProjectileRemoved(int index, ref Gun.Projectile projectile)
        {
            _trails.segmentsCounts[index] = 0;
        }

        private void OnDestroy()
        {
            _trails.Dispose();
            _lastSegments.Dispose();
            _gun.onProjectileCreated -= OnProjectileCreated;
            _gun.onProjectileMoved -= OnProjectileMoved;
            _gun.onProjectileRemoved -= OnProjectileRemoved;
        }
    }
}