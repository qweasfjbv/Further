using Further.Props;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("Missile Settings")]
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private int missileInitCount;
    [SerializeField] private int missileShootCount;
    [SerializeField] private float missileLifetime;
    [SerializeField] private float missileVelocity;
    [SerializeField] private float fireCooltime;

    private float missileSize;
    private int missileBoundCount;
    private int missileCount;

    public float FireCooltime { get => fireCooltime; set => fireCooltime = value; }
    public float MissileSize { get => missileSize; set => missileSize = value; }
    public int MissileBoundCount { get => missileBoundCount; set => missileBoundCount = value; }
    public int MissileCount { get => missileCount; set=> missileCount = value; }    

    private Transform poolRoot = null;
    private bool isOnMissileCooltime = false;

    private void Awake()
    {
        SetupObjectPool();
    }
    private void SetupObjectPool()
    {
        poolRoot = new GameObject("_PoolRoot").transform;
        poolRoot.position = transform.position;
        poolRoot.rotation = transform.rotation;
        poolRoot.parent = transform;

        for (int i = 0; i < missileInitCount; i++)
        {
            var tmpGo = Instantiate(missilePrefab, poolRoot);
            tmpGo.SetActive(false);
        }

    }

    private void Update()
    {
        if (!isOnMissileCooltime)
        {
            isOnMissileCooltime = true;
            FireMissile(missileShootCount);
            Invoke(nameof(ReleaseFire), fireCooltime);
        }
    }

    private void ReleaseFire()
    {
        isOnMissileCooltime = false;
    }

    private void FireMissile(int count)
    {
        float spreadAngle = 30f;
        float angleStep = spreadAngle / (count - 1);

        for (int i = 0; i < count; i++)
        {
            float angle = (i - (count - 1) / 2f) * angleStep;
            Debug.Log(angle);
            var tmpMissile = GetOrGenerateMissile();
            Vector3 direction = Quaternion.Euler(0, 0, angle) * transform.up;
            tmpMissile.OnFire(poolRoot, missileLifetime, GetComponent<Rigidbody>().velocity + direction * missileVelocity);
        }

    }

    private Missile GetOrGenerateMissile()
    {
        if(poolRoot == null)
        {
            Debug.LogError("Pool Root is NULL!");
            return null;
        }

        if (poolRoot.transform.childCount == 0)     // Pool Object Empty
        {
            for (int i = 0; i < missileInitCount; i++)
            {
                var tmpGo = Instantiate(missilePrefab, poolRoot);
                tmpGo.SetActive(false);
            }
        }

        return poolRoot.GetChild(0).GetComponent<Missile>();
    }
}
