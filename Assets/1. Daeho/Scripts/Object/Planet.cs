using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    public GameLogic game_logic { get; set; }
    public int planet_id;
    public int index_x;/* { get; private set; }*/
    public int index_y;/* { get; private set; }*/
    public bool is_checked = false;

    public bool hint_animation { get; set; } = false;

    Vector2 target_move_position;
    void Start()
    {
        target_move_position = transform.position;

        SetIndexToPosition();

        StartCoroutine(StartAnimation());
    }

    float sin_value = 1;
    void Update()
    {
        name = $"{index_x}, {index_y}";

        if (hint_animation)
        {
            sin_value += Time.deltaTime * 300;
            transform.localScale += (Vector3)Vector2.one * (Mathf.Sin(sin_value * Mathf.Deg2Rad) / 500);
        }
        else if(transform.localScale.x <= 0.01f)
        {
            transform.localScale -= Vector3.down / 30;
        }
    }

    /// <summary>
    /// ���� ��ġ�� �ε��� ����
    /// </summary>
    public void SetIndexToPosition()
    {
        game_logic.planet_objs[index_x, index_y] = null;

        index_x = Mathf.RoundToInt(transform.position.x);
        index_y = Mathf.RoundToInt(transform.position.y);

        game_logic.planet_objs[index_x, index_y] = this;
    }
    /// <summary>
    /// ���� �ε��� ����
    /// </summary>
    /// <param name="index">������ �ε��� ����</param>
    public void SetIndexToPosition(Vector2 index)
    {
        game_logic.planet_objs[index_x, index_y] = null;

        index_x = Mathf.RoundToInt(index.x);
        index_y = Mathf.RoundToInt(index.y);

        game_logic.planet_objs[index_x, index_y] = this;
    }

    /// <summary>
    /// ���� �� Ŀ���� �ִϸ��̼�
    /// </summary>
    /// <returns></returns>
    IEnumerator StartAnimation()
    {
        WaitForSeconds second = new WaitForSeconds(0.01f);

        while (Vector2.Distance(transform.localScale, Vector2.one) > 0.01f)
        {
            transform.localScale += new Vector3(0.05f, 0.05f);
            yield return second;
        }
    }

    bool is_moving = false;
    /// <summary>
    /// ������ �������� �̵��ϴ� �Լ�
    /// </summary>
    /// <param name="direction">�̵��� ����</param>
    /// <returns></returns>
    public IEnumerator MoveAnimation(Vector3 direction)
    {
        WaitForSeconds second = new WaitForSeconds(0.01f);

        is_moving = true;

        target_move_position = transform.position + direction;

        while (Vector2.Distance(transform.position, target_move_position) > 0.05f)
        {
            transform.position = Vector2.MoveTowards(transform.position, target_move_position, 0.1f);
            yield return second;
        }

        transform.position = target_move_position;

        is_moving = false;
    }

    /// <summary>
    /// ���� �ڸ��� �ǵ����ų� ���� �ڸ��� ������Ű�� �Լ�
    /// </summary>
    /// <returns></returns>
    public IEnumerator ReturnAnimation(Vector3 direction)
    {
        WaitForSeconds second = new WaitForSeconds(0.01f);

        while (is_moving)
        {
            yield return second;
        }

        target_move_position = transform.position - direction;

        while (Vector2.Distance(transform.position, target_move_position) > 0.05f)
        {
            transform.position = Vector2.MoveTowards(transform.position, target_move_position, 0.1f);
            yield return second;
        }

        transform.position = target_move_position;

        SetIndexToPosition();
    }

    public void HintAnimation(bool animation)
    {
        hint_animation = animation;
    }
}
