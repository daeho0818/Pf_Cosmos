using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyEffect : MonoBehaviour
{

    public GameObject EffectPrefab;

    public List<GameObject> effects = new List<GameObject>();
    public List<GameObject> effects_temp = new List<GameObject>();

    List<Vector3> endPos = new List<Vector3>();


    public Transform effectEndPos;

    public int i_type;

    void Start()
    {
        effectEndPos = GameObject.FindGameObjectWithTag("EndPos").transform;
        Effect(1, 15, transform.position);
    }

    void Update()
    {
        switch (i_type)
        {
            case 0:

                break;
            case 1:
                if (effects.Count > 0)
                {
                    for (int i = 0; i < effects.Count; i++)
                    {
                        effects[i].transform.rotation = LookAt2D(effects[i].transform, endPos[i], 15);
                        effects[i].transform.position = Vector3.Lerp(effects[i].transform.position, endPos[i], Time.deltaTime * 6f);
                    }
                }
                break;
            case 2:
                if (effects_temp.Count > 0)
                {
                    for (int i = 0; i < effects_temp.Count; i++)
                    {
                        if (effects_temp[i] != null)
                        {
                            effects_temp[i].transform.rotation = LookAt2D(effects_temp[i].transform, effectEndPos.position, 15);
                            effects_temp[i].transform.position = Vector3.Lerp(effects_temp[i].transform.position, effectEndPos.position, Time.deltaTime * 10f);
                        }
                    }
                }
                else i_type = 0;
                break;
            default:
                break;
        }

        if (transform.childCount == 0) Destroy(this.gameObject);
    }

    public void Effect(float radius, int effectCount, Vector3 origin)
    {
        effects.Clear();
        endPos.Clear();

        for (int i = 0; i < effectCount; i++)
        {
            effects.Add(GenerateEffect(radius, origin, i));
        }

        StartCoroutine(EffectControll());
    }

    GameObject GenerateEffect(float radius, Vector3 originPos, int index)
    {

        endPos.Add(new Vector3(Random.Range(-radius, radius), Random.Range(-radius, radius)) + transform.position);

        GameObject retObj = Instantiate(EffectPrefab, originPos, Quaternion.identity, transform);
        retObj.GetComponent<EnergyParticle>().index = index;

        return retObj;
    }

    IEnumerator EffectControll()
    {
        i_type = 1;
        yield return new WaitForSeconds(0.4f);
        i_type = 2;
        StartCoroutine(EffectEnd());
    }

    IEnumerator EffectEnd()
    {
        for (int i = 0; i < effects.Count; i++)
        {
            effects_temp.Add(effects[i]);
            yield return new WaitForSeconds(0.03f);
        }
    }

    Quaternion LookAt2D(Transform start, Vector3 target, float rotateSpeed)
    {
        Vector2 direction = new Vector2(start.position.x - target.x, start.position.y - target.y);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Quaternion angleAxis = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        Quaternion rotation = Quaternion.Slerp(start.rotation, angleAxis, rotateSpeed * Time.deltaTime);

        return rotation;
    }
}
