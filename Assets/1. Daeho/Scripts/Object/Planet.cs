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
    /// 현재 위치로 인덱스 설정
    /// </summary>
    public void SetIndexToPosition()
    {
        game_logic.planet_objs[index_x, index_y] = null;

        index_x = Mathf.RoundToInt(transform.position.x);
        index_y = Mathf.RoundToInt(transform.position.y);

        game_logic.planet_objs[index_x, index_y] = this;
    }
    /// <summary>
    /// 현재 인덱스 설정
    /// </summary>
    /// <param name="index">설정할 인덱스 벡터</param>
    public void SetIndexToPosition(Vector2 index)
    {
        game_logic.planet_objs[index_x, index_y] = null;

        index_x = Mathf.RoundToInt(index.x);
        index_y = Mathf.RoundToInt(index.y);

        game_logic.planet_objs[index_x, index_y] = this;
    }

    /// <summary>
    /// 시작 시 커지는 애니메이션
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
    /// 인접한 방향으로 이동하는 함수
    /// </summary>
    /// <param name="direction">이동할 방향</param>
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
    /// 원래 자리로 되돌리거나 현재 자리에 정착시키는 함수
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
