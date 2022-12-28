using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Bullet : NetworkBehaviour
{
    public float BulletSpeed = 5f;

    uint owner;
    bool inited;
    Vector3 target;

    [Server]
    public void Init(uint owner, Vector3 target)
    {
        this.owner = owner;
        this.target = target;
        inited = true;
        Debug.Log("BulletInit");
    }

    [Command]
    public void CmdInit(uint owner, Vector3 target)
    {
        Debug.Log("BulletCmdInit");

        Init(owner, target);
        //this.owner = owner;
        //this.target = target;
        //inited = true;

    }

    //[Command]
    //public void CmdFireballMove()
    //{
    //    RpcFireballMove();
    //}

    //[ClientRpc]
    //public void RpcFireballMove()
    //{
    //    Debug.Log("Bullet update");
    //    transform.Translate((target - transform.position).normalized /*0.04f*/ * Time.deltaTime * BulletSpeed);

    //    foreach (var item in Physics2D.OverlapCircleAll(transform.position, 0.5f))
    //    {
    //        Player player = item.GetComponent<Player>();
    //        if (player)
    //        {
    //            if (player.netId != owner)
    //            {
    //                player.ChangeHealthValue(player.Health - 1);
    //                NetworkServer.Destroy(gameObject);
    //            }
    //        }
    //    }

    //    if (Vector3.Distance(transform.position, target) < 0.1f)
    //    {
    //        NetworkServer.Destroy(gameObject);
    //    }
    //}

    private void Update()
    {
        if (inited && isServer) 
        {
            
            Debug.Log("Bullet update");
            transform.Translate((target - transform.position).normalized /*0.04f*/ * Time.deltaTime * BulletSpeed);

            foreach (var item in Physics2D.OverlapCircleAll(transform.position, 0.5f))
            {
                Player player = item.GetComponent<Player>();
                if (player)
                {
                    if (player.netId != owner)
                    {
                        player.ChangeHealthValue(player.Health - 1);
                        NetworkServer.Destroy(gameObject);
                    }
                }
            }

            if (Vector3.Distance(transform.position, target) < 0.1f)
            {
                NetworkServer.Destroy(gameObject);
            }
        }
    }

    
}
