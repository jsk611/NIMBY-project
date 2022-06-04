using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone : MonoBehaviour
{
    public Player owner;
    public SpriteRenderer spr;
    [SerializeField] Vector2 size;
    public int trashNum;
    private void Awake()
    {
        spr = GetComponent<SpriteRenderer>();
    }

    public int Check()
    {
        trashNum = 0;
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, size, 0, LayerMask.GetMask("Trash"));
        foreach(var i in hits)
        {
            trashNum += i.GetComponent<Trash>().size;
        }
        return trashNum;
    }
}
