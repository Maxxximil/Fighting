using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Скрипт Фаербола
public class Fireball : MonoBehaviour
{
    public float BulletSpeed = 10f;

    uint owner;
    bool inited;
    bool hit;
    Vector3 target;

    //Инициализация фаербола
    public void Init(uint owner, Vector3 target)
    {
        //айдишник того кто запустил
        this.owner = owner;
        //Цель куда лететь
        this.target = target;
        hit = false;
        inited = true;
    }

    //Логика движения фаербола
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
                        if (hit)
                        {
                            break;
                        }
                        player.CmdChangeHealth(player.Health - 1);
                        player.CmdCheckWin();
                        hit = true;
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
