using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float BulletSpeed = 10f;

    uint owner;
    bool inited;
    Vector3 target;

    public void Init(uint owner, Vector3 target)
    {
        this.owner = owner;
        this.target = target;
        inited = true;
        Debug.Log("BulletInit");
    }

    private void Update()
    {
        if (inited)
        {

            transform.Translate((target - transform.position).normalized /*0.04f*/ * Time.deltaTime * BulletSpeed);

            foreach (var item in Physics2D.OverlapCircleAll(transform.position, 0.5f))
            {
                //Debug.Log("Overplap items: " + item.name);
                Player player = item.GetComponent<Player>();
                if (player)
                {
                    if (player.netId != owner)
                    {
                        player.CmdChangeHealth(player.Health - 1);
                        Destroy(this.gameObject);
                        //NetworkServer.Destroy(gameObject);
                    }
                }
            }

            if (Vector3.Distance(transform.position, target) < 0.1f)
            {
                Destroy(this.gameObject);
                //NetworkServer.Destroy(gameObject);
            }
        }
    }
}
