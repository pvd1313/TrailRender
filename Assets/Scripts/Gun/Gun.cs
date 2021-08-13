using UnityEngine;

public class Gun : MonoBehaviour
{
    public float rate = 10;
    public float startingVelocity = 100;
    public float maxAngle = 30;
    public float lifetime = 10;
    
    public delegate void ProjectileCallback(int index, ref Projectile projectile);
    public ProjectileCallback onProjectileCreated;
    public ProjectileCallback onProjectileRemoved;
    public ProjectileCallback onProjectileMoved;

    public int maxProjectileCount
    {
        get => _projectiles.Length;
    }

    public struct Projectile
    {
        public Vector3 position;
        public Vector3 velocity;
        public float lifetime;
    }

    private void Awake()
    {
        int maxProjectiles = Mathf.CeilToInt(lifetime * rate);
        _projectiles = new Projectile[maxProjectiles];
        _firstProjectileIndex = 0;
        _projectileCount = 0;
    }

    private void Start()
    {
        InvokeRepeating(nameof(CreateProjectile), 0, 1 / rate);

        QualitySettings.vSyncCount = 0;
    }

    private void CreateProjectile()
    {
        if (_projectileCount < _projectiles.Length)
        {
            int index = (_firstProjectileIndex + _projectileCount) % _projectiles.Length;
            ++_projectileCount;

            float theta = Random.Range(-maxAngle * Mathf.Deg2Rad, maxAngle * Mathf.Deg2Rad);
            float phi = Random.Range(0, Mathf.PI);
            Vector3 shotDirection = new Vector3(
                Mathf.Sin(theta) * Mathf.Cos(phi),
                Mathf.Sin(theta) * Mathf.Sin(phi),
                Mathf.Cos(theta));

            Vector3 velocity = shotDirection * startingVelocity;

            Projectile projectile = new Projectile();
            projectile.position = this.transform.position;
            projectile.velocity = velocity;
            projectile.lifetime = 0;
            _projectiles[index] = projectile;

            onProjectileCreated?.Invoke(index, ref _projectiles[index]);
        }
    }

    private void UpdateProjectile(int index)
    {
        SimulateProjectile(
            ref _projectiles[index].position,
            ref _projectiles[index].velocity,
            Time.deltaTime);

        _projectiles[index].lifetime += Time.deltaTime;

        onProjectileMoved?.Invoke(index, ref _projectiles[index]);

        if (index == _firstProjectileIndex &&
            _projectiles[index].lifetime >= lifetime)
        {
            onProjectileRemoved?.Invoke(index, ref _projectiles[index]);

            _firstProjectileIndex = (_firstProjectileIndex + 1) % _projectiles.Length;
            --_projectileCount;
        }
    }

    private void Update()
    {
        int firstIndex = _firstProjectileIndex;
        for (int index = 0; index < _projectileCount; ++index)
        {
            UpdateProjectile((firstIndex + index) % _projectiles.Length);
        }
    }

    public void SimulateProjectile(ref Vector3 position, ref Vector3 velocity, float deltaTime)
    {
        position += velocity * deltaTime + Physics.gravity * deltaTime * deltaTime * 0.5f;
        velocity += Physics.gravity * deltaTime;
    }

    private int _maxProjectiles;
    private Projectile[] _projectiles;
    private int _firstProjectileIndex;
    private int _projectileCount;
}