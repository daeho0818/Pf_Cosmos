using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyParticle : MonoBehaviour
{

    public int index;
    EnergyEffect parents;

    private void Start()
    {
        parents = GetComponentInParent<EnergyEffect>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("EndPos") && parents.i_type == 2)
        {
            Destroy(parents.effects_temp[index]);
        }
    }
}
