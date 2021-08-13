using UnityEngine;

// Don't pay attention to this class, it's used for rendering purposes only

[RequireComponent(typeof(Gun))]
[RequireComponent(typeof(ParticleSystem))]
public class ProjectileRenderer : MonoBehaviour
{
    private void Awake()
    {
        _gun = GetComponent<Gun>();
        _particleSystem = GetComponent<ParticleSystem>();
        _particles = new ParticleSystem.Particle[_particleSystem.main.maxParticles];
    }

    private void OnEnable()
    {
        _gun.onProjectileCreated += OnProjectileCreated;
    }

    private void OnDisable()
    {
        _gun.onProjectileCreated -= OnProjectileCreated;
    }

    private void OnProjectileCreated(int index, ref Gun.Projectile projectile)
    {
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        emitParams.position = projectile.position;
        emitParams.velocity = projectile.velocity;
        _particleSystem.Emit(emitParams, 1);
    }

    private void LateUpdate()
    {
        int particleCount = _particleSystem.GetParticles(_particles);
        if (particleCount > 0)
        {
            for (int index = 0; index < particleCount; ++index)
            {
                Vector3 position = _particles[index].position;
                Vector3 velocity = _particles[index].velocity;
                _gun.SimulateProjectile(ref position, ref velocity, Time.deltaTime);
                _particles[index].velocity = velocity;
            }

            _particleSystem.SetParticles(_particles, particleCount);
        }
    }

    private Gun _gun;
    private ParticleSystem _particleSystem;
    private ParticleSystem.Particle[] _particles;
}