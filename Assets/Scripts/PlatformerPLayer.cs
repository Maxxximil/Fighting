using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class PlatformerPLayer : NetworkBehaviour
{
    [SyncVar(hook = nameof(SyncHealth))] int _synchHealth;

    public GameObject BulletPrefab;

    public float speed = 250.0f;
    public float jumpForce = 12.0f;
    public int Health;
    public GameObject[] HealthGos;
    
    private Rigidbody2D _body;
    private Animator _anim;
    private BoxCollider2D _box;
    private bool _jump = false;
    void Start()
    {
        _body = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _box = GetComponent<BoxCollider2D>();
    }

    private void SyncHealth(int oldValue, int newValue)
    {
        Health = newValue;
    }

    [Server]
    public void ChangeHealthValue(int newValue)
    {
        _synchHealth = newValue;

        if (_synchHealth <= 0)
        {
            NetworkServer.Destroy(gameObject);
        }
    }

    [Server]
    public void SpawnBullet(uint owner, Vector3 target)
    {
        GameObject bulletGO = Instantiate(BulletPrefab, transform.position, Quaternion.identity);
        NetworkServer.Spawn(bulletGO);
        bulletGO.GetComponent<Bullet>().Init(owner, target);
    }

    [Server]
    public void OutOfMap()
    {
        NetworkServer.Destroy(gameObject);
    }

    [Command]
    public void CmdOutOfMap()
    {
        OutOfMap();
    }

    [Command]
    public void CmdSpawnBullet(uint owner, Vector3 target)
    {
        SpawnBullet(owner, target);
    }

    [Command]
    public void CmdChangeHealth(int newValue)
    {
        ChangeHealthValue(newValue);
    }

    void Update()
    {
        if (isOwned)
        {
            #region Movement
            float deltaX = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
            Vector2 movement = new Vector2(deltaX, 0);
            transform.Translate(movement);

            if(Mathf.Abs(deltaX) > 0)
            {
                _anim.SetBool("SetSpeed", true);
            }
            else
            {
                _anim.SetBool("SetSpeed", false);
            }

            Vector3 max = _box.bounds.max;
            Vector3 min = _box.bounds.min;
            Vector2 corner1 = new Vector2(max.x, min.y - .1f);
            Vector2 corner2 = new Vector2(min.x, min.y - .2f);
            Collider2D hit = Physics2D.OverlapArea(corner1, corner2);
            bool grounded = false;
            if (hit != null)
            {
                grounded = true;
            }
            _body.gravityScale = grounded && deltaX == 0 ? 0 : 1;
            if (grounded && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)))
            {
                _body.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                _jump = false;
            }

            MovingPlatform platform = null;
            if (hit != null)
            {
                platform = hit.GetComponent<MovingPlatform>();
            }
            if (platform != null)
            {
                transform.parent = platform.transform;
            }
            else
            {
                transform.parent = null;
            }
            //_anim.SetFloat("Speed", Mathf.Abs(deltaX));
            Vector3 pScale = Vector3.one;
            if (platform != null)
            {
                pScale = platform.transform.localScale;
            }
            if (deltaX != 0)
            {
                transform.localScale = new Vector3(Mathf.Sign(deltaX) * 3 / pScale.x, 3 / pScale.y, 1);
            }
            #endregion

            if (Input.GetKeyDown(KeyCode.H))
            {
                if (isServer)
                {
                    ChangeHealthValue(Health - 1);
                }
                else
                {
                    CmdChangeHealth(Health-1);
                }
            }

            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                Vector3 pos = Input.mousePosition;
                pos.z = 10f;
                pos = Camera.main.ScreenToWorldPoint(pos);

                if (isServer)
                    SpawnBullet(netId, pos);
                else
                    CmdSpawnBullet(netId, pos);
            }
        }

        for (int i = 0; i < HealthGos.Length; i++)
        {
            HealthGos[i].SetActive(!(Health - 1 < i));
        }     
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(isOwned && collision.CompareTag("OutMap"))
        {
            if (isServer)
            {
                OutOfMap();
            }
            else
            {
                CmdOutOfMap();
            }
        }
    }

    public void Jump()
    {
        _jump = true;
    }
}
