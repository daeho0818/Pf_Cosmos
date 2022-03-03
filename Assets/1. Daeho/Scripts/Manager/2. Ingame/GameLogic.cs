using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class GameLogic : MonoBehaviour
{
    [SerializeField] Planet[] prefab_planets;

    [SerializeField] Transform board;

    [SerializeField] SpriteRenderer fade_screen;

    [Space(20)]
    [SerializeField] TextMeshProUGUI score_text;
    [SerializeField] TextMeshProUGUI best_score_text;

    [Space(20)]
    [SerializeField] GameObject result_window;
    // 결과 창 (Result Window) 텍스트
    [SerializeField] TextMeshProUGUI r_score_text;
    [SerializeField] TextMeshProUGUI r_best_score_text;

    [Space(20)]
    [SerializeField] Slider timer_slider;
    [SerializeField] Slider fever_slider;

    public Vector2 cell_size => new Vector2(7, 7);
    Vector2 mouse_position => Camera.main.ScreenToWorldPoint(Input.mousePosition);
    Vector2 mouse_down_position;
    public Planet[,] planet_objs { get; set; }
    Planet selected_planet;
    Planet other_planet;
    List<Planet> hint_planets = new List<Planet>();
    public int game_score { get; private set; }
    public int game_best_score { get; private set; }

    int game_count = 0;

    float fever_guage = 0;
    float wait_touch = 0;

    bool game_start = false;
    bool game_stop = false;

    bool is_fever = false;

    enum SlideType
    {
        HORIZONTAL,
        VERTICAL,
        NONE
    }
    SlideType slide_type = SlideType.NONE;

    void Start()
    {
        timer_slider.value = 1;

        best_score_text.text = $"BEST : {PlayerPrefs.GetInt("BestScore", 0):#,0}";
        score_text.text = $"SCORE : {game_score:#,0}";

        StartCoroutine(Init());
    }
    void Update()
    {
        if (!game_start || game_stop) return;

        if (Input.GetMouseButtonDown(0) && is_slidable)
        {
            SelectPlanet();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            CheckSlide();
            wait_touch = 0;
        }
        else wait_touch += Time.deltaTime;

        if (hint_planets.Count >= 3)
        {
            foreach (var hint_planet in hint_planets)
            {
                hint_planet.HintAnimation(wait_touch >= 3);
            }
        }

        if (timer_slider.value < 0.01f)
        {
            r_best_score_text.text = $"{PlayerPrefs.GetInt("BestScore", 0):#,0}";
            r_score_text.text = $"{game_score:#,0}";

            result_window.SetActive(true);
            game_start = false;
            timer_slider.value = 0;
            SoundManager.Instance.PlayAudio("Time Over", 2);
        }
        else
        {
            timer_slider.value = Mathf.Lerp(timer_slider.value, 1 - (float)game_count / 60, 0.1f);
            fever_slider.value = (float)fever_guage / 100;

            best_score_text.text = $"BEST : {PlayerPrefs.GetInt("BestScore", 0):#,0}";
            score_text.text = $"SCORE : {game_score:#,0}";
        }
    }

    /// <summary>
    /// 게임이 시작한 뒤 연출 및 행성들 생성하는 함수
    /// </summary>
    /// <returns></returns>
    IEnumerator Init()
    {
        var images = FindObjectsOfType<Image>();

        WaitForSeconds second = new WaitForSeconds(0.01f);

        while (images[images.Length - 1].color.a < 0.99f)
        {
            foreach (var image in images)
            {
                image.color = Color.Lerp(image.color, new Color(image.color.r, image.color.g, image.color.b, 1), 0.5f);
                yield return second;
            }
        }

        var texts = FindObjectsOfType<TMPro.TextMeshProUGUI>();
        while (texts[texts.Length - 1].color.a < 0.99f)
        {
            foreach (var text in texts)
            {
                text.color = Color.Lerp(text.color, new Color(text.color.r, text.color.g, text.color.b, 1), 0.2f);
                yield return second;
            }
        }

        planet_objs = new Planet[(int)cell_size.x, (int)cell_size.y];
        int random;

        // 7 X 7 행성 생성
        for (int x = 0; x < cell_size.x; x++)
        {
            for (int y = 0; y < cell_size.y; y++)
            {
                random = Random.Range(0, prefab_planets.Length);
                planet_objs[x, y] = Instantiate(prefab_planets[random], new Vector2(x, y), Quaternion.identity, board);
                planet_objs[x, y].game_logic = this;
                planet_objs[x, y].planet_id = random;
                planet_objs[x, y].index_x = x;
                planet_objs[x, y].index_y = y;
            }
        }

        game_start = true;

        Invoke(nameof(CheckAllPlanets), 1);
        InvokeRepeating(nameof(CountUp), 1, 1);
    }

    /// <summary>
    /// 게임 카운트다운
    /// </summary>
    void CountUp()
    {
        game_count++;
    }

    /// <summary>
    /// 설정 창 열었을 때 등 게임이 멈춤
    /// </summary>
    /// <param name="stop"></param>
    public void GameStop(bool stop)
    {
        game_stop = stop;

        if (stop) CancelInvoke(nameof(CountUp));
        else InvokeRepeating(nameof(CountUp), 1, 1);
    }

    Coroutine screen_fading = null;
    bool _is_slidable = true;
    bool is_slidable
    {
        get => _is_slidable;
        set
        {
            if (value)
                StartCoroutine(WaitAndSetSlidable(value));
            else
                _is_slidable = value;

            float alpha = value ? 0 : 0.2f;

            if (screen_fading != null) StopCoroutine(screen_fading);
            screen_fading = StartCoroutine(FadeScreen(alpha, value));
        }
    }
    IEnumerator WaitAndSetSlidable(bool value)
    {
        yield return new WaitForSeconds(0.1f);
        _is_slidable = value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="alpha"></param>
    /// <param name="wait"></param>
    /// <returns></returns>
    IEnumerator FadeScreen(float alpha, bool wait)
    {
        if (wait)
            yield return new WaitForSeconds(0.1f);

        WaitForSeconds seconds = new WaitForSeconds(0.01f);

        Color target_color = new Color(fade_screen.color.r, fade_screen.color.g, fade_screen.color.b, alpha);
        while (Mathf.Abs(fade_screen.color.a - alpha) > 0.01f)
        {
            fade_screen.color = Color.Lerp(fade_screen.color, target_color, 0.5f);
            yield return seconds;
        }
    }

    /// <summary>
    /// 화면을 터치했을 때 (Down) 터치한 행성을 저장하는 함수
    /// </summary>
    void SelectPlanet()
    {
        if (!is_slidable || selected_planet) return;

        mouse_down_position = mouse_position;

        int round_x = Mathf.RoundToInt(mouse_down_position.x);
        int round_y = Mathf.RoundToInt(mouse_down_position.y);

        if (round_x >= 0 && round_x < cell_size.x && round_y >= 0 && round_y < cell_size.y)
        {
            selected_planet = planet_objs[round_x, round_y];
        }
    }

    /// <summary>
    /// 화면을 슬라이드한 뒤 손을 뗐을 때 슬라이드한 방향에 따른 블럭 로직을 구현한 함수
    /// </summary>
    void CheckSlide()
    {
        if (!selected_planet || !is_slidable) return;
        is_slidable = false;

        Vector2 mouse_up_position = mouse_position;

        float distance_x = Mathf.Abs(mouse_up_position.x - mouse_down_position.x);
        float distance_y = Mathf.Abs(mouse_up_position.y - mouse_down_position.y);

        int direction_x = 0;
        int direction_y = 0;

        // 가로로 슬라이드했을 때
        if (distance_x > distance_y && distance_x > 1 && distance_y < 1)
        {
            direction_x = mouse_up_position.x > mouse_down_position.x ? 1 : -1;
            slide_type = SlideType.HORIZONTAL;
        }
        // 세로로 슬라이드했을 때
        else if (distance_y > distance_x && distance_y > 1 && distance_x < 1)
        {
            direction_y = mouse_up_position.y > mouse_down_position.y ? 1 : -1;
            slide_type = SlideType.VERTICAL;
        }
        else
        {
            is_slidable = true;
            goto RETURN;
        }

        // 셀 크기 바깥으로 슬라이드했을 4가지 경우
        if (selected_planet.index_x == 0 && direction_x == -1) { is_slidable = true; goto RETURN; }
        else if (selected_planet.index_x == cell_size.x - 1 && direction_x == 1) { is_slidable = true; goto RETURN; }
        else if (selected_planet.index_y == 0 && direction_y == -1) { is_slidable = true; goto RETURN; }
        else if (selected_planet.index_y == cell_size.y - 1 && direction_y == 1) { is_slidable = true; goto RETURN; }

        SoundManager.Instance.PlayAudio("Move Block_2", 2);

        selected_planet.StartCoroutine(selected_planet.MoveAnimation(new Vector2(direction_x, direction_y)));

        other_planet = planet_objs[selected_planet.index_x + direction_x, selected_planet.index_y + direction_y];
        other_planet.StartCoroutine(other_planet.MoveAnimation((selected_planet.transform.position - other_planet.transform.position).normalized));

        CheckLinked(new Vector2(direction_x, direction_y));

    RETURN:
        selected_planet = null;
    }

    int[,] number_arr;
    void ReplacePlanetPostions()
    {
        number_arr = new int[(int)cell_size.x, (int)cell_size.y];

        int random_x, random_y;
        Planet[,] planets = new Planet[(int)cell_size.x, (int)cell_size.y];

        for (int x = 0; x < cell_size.x; x++)
        {
            for (int y = 0; y < cell_size.y; y++)
            {
                do
                {
                    random_x = Random.Range(0, (int)cell_size.x);
                    random_y = Random.Range(0, (int)cell_size.y);
                }
                while (number_arr[random_x, random_y] != 0);

                number_arr[random_x, random_y] = 1;

                planets[random_x, random_y] = planet_objs[x, y];
                planets[random_x, random_y].game_logic = this;
                planets[random_x, random_y].is_checked = false;
                planets[random_x, random_y].index_x = random_x;
                planets[random_x, random_y].index_y = random_y;
                planets[random_x, random_y].transform.position = new Vector2(random_x, random_y);
            }
        }

        planet_objs = planets;

        Invoke(nameof(CheckAllPlanets), 0.3f);
    }
    /// <summary>
    /// 전체 셀 중 빈 공간에 행성을 생성하는 함수
    /// </summary>
    void SpawnEmptyCell(bool duplication = false)
    {
        int random = Random.Range(0, prefab_planets.Length);

        for (int x = (int)cell_size.x - 1; x >= 0; x--)
        {
            for (int y = 0; y < (int)cell_size.y; y++)
            {
                if (planet_objs[x, y] == null)
                {
                    if (!duplication)
                        random = Random.Range(0, prefab_planets.Length);

                    planet_objs[x, y] = Instantiate(prefab_planets[random], new Vector2(x, y), Quaternion.identity, board);
                    planet_objs[x, y].game_logic = this;
                    planet_objs[x, y].planet_id = random;
                    planet_objs[x, y].index_x = x;
                    planet_objs[x, y].index_y = y;
                }
                planet_objs[x, y].is_checked = false;
            }
        }
    }

    /// <summary>
    /// 위치를 바꾼 두 행성의 좌, 우, 위, 아래 축을 검사하여 인접한 블럭이 3개 이상 있는지 검사하는 함수
    /// </summary>
    /// <param name="direction">선택한 오브젝트가 이동한 방향</param>
    public void CheckLinked(Vector3 direction)
    {
        int destroy_count = 0;
        bool planet_return = true;

        List<Planet> horizontal_planets_1 = new List<Planet>();
        List<Planet> horizontal_planets_2 = new List<Planet>();
        List<Planet> vertical_planets_1 = new List<Planet>();
        List<Planet> vertical_planets_2 = new List<Planet>();

        vertical_planets_1 = CheckAnglePlanet(selected_planet, direction, SlideType.VERTICAL);
        vertical_planets_2 = CheckAnglePlanet(other_planet, direction, SlideType.VERTICAL);

        horizontal_planets_1 = CheckAnglePlanet(selected_planet, direction, SlideType.HORIZONTAL);
        horizontal_planets_2 = CheckAnglePlanet(other_planet, direction, SlideType.HORIZONTAL);

        List<Planet>[] all_destroy_planets = new List<Planet>[4] { horizontal_planets_1, horizontal_planets_2, vertical_planets_1, vertical_planets_2 };

        bool merge_check_h = false;
        bool merge_check_v = false;
        // 이동시킨 두 행성이 같은 종류라면
        if (selected_planet.planet_id == other_planet.planet_id)
        {
            merge_check_v = slide_type == SlideType.VERTICAL;
            merge_check_h = slide_type == SlideType.HORIZONTAL;
        }

        if (merge_check_v)
        {
            vertical_planets_1.AddRange(vertical_planets_2);
            // 세로로 인접한 모든 행성을 파괴시킴
            if (vertical_planets_1.Count >= 3)
            {
                foreach (var v in vertical_planets_1)
                {
                    if (v == selected_planet)
                        selected_planet = null;
                    else if (v == other_planet)
                        other_planet = null;

                    planet_objs[v.index_x, v.index_y] = null;
                    Destroy(v.gameObject);
                    destroy_count++;
                }
                planet_return = false;
            }
            else foreach (var plannet in vertical_planets_1) plannet.is_checked = false;
        }
        else if (merge_check_h)
        {
            horizontal_planets_1.AddRange(horizontal_planets_2);
            // 가로로 인접한 모든 행성을 파괴시킴
            if (horizontal_planets_1.Count >= 3)
            {
                foreach (var h in horizontal_planets_1)
                {
                    if (h == selected_planet)
                        selected_planet = null;
                    else if (h == other_planet)
                        other_planet = null;

                    planet_objs[h.index_x, h.index_y] = null;
                    Destroy(h.gameObject);
                    destroy_count++;
                }
                planet_return = false;
            }
            else foreach (var plannet in horizontal_planets_1) plannet.is_checked = false;
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                if (all_destroy_planets[i].Count >= 3)
                {
                    foreach (var planet in all_destroy_planets[i])
                    {
                        if (planet == selected_planet)
                            selected_planet = null;
                        else if (planet == other_planet)
                            other_planet = null;

                        planet_objs[planet.index_x, planet.index_y] = null;
                        Destroy(planet.gameObject);
                        destroy_count++;
                    }
                    planet_return = false;
                }
                else foreach (var plannet in all_destroy_planets[i]) plannet.is_checked = false;
            }
        }

        if (planet_return)
        {
            selected_planet.StartCoroutine(selected_planet.ReturnAnimation(direction));
            other_planet.StartCoroutine(other_planet.ReturnAnimation((-direction).normalized));

            is_slidable = true;
            return;
        }

        if (selected_planet)
            selected_planet.SetIndexToPosition(new Vector2(selected_planet.index_x, selected_planet.index_y) + (Vector2)direction);
        if (other_planet)
            other_planet.SetIndexToPosition(new Vector2(other_planet.index_x, other_planet.index_y) - (Vector2)direction);

        PlusScore(destroy_count);


        if (game_score > PlayerPrefs.GetInt("BestScore", 0))
            PlayerPrefs.SetInt("BestScore", game_score);

        SpawnEmptyCell(is_fever);

        Invoke(nameof(CheckAllPlanets), 0.3f);
    }
    /// <summary>
    /// 한 축을 통째로 겁사하여 인접한 같은 종류의 행성 리스트를 반환하는 함수  (예시 : 세로 축으로 슬라이드하였을 때 해당 행성의 가로축을 모두 검사)
    /// </summary>
    /// <param name="base_planet">기준이 될 행성 오브젝트</param>
    /// <param name="direction">행성을 슬라이드하여 움직인 방향</param>
    /// <param name="check_angle">검사할 축 (가로, 세로)</param>
    /// <param name="include_base">기준으로 할 행성을 무조건 제거 목록에 추가</param>
    /// <returns></returns>
    List<Planet> CheckAnglePlanet(Planet base_planet, Vector2 direction, SlideType check_angle, bool include_base = false)
    {
        List<Planet> destroy_planets = new List<Planet>();
        Queue<Planet> planet_group = new Queue<Planet>();

        if (include_base) destroy_planets.Add(base_planet);

        Planet current_planet;
        Planet target_planet;
        (int, int) target_index = (0, 0);

        base_planet.is_checked = true;
        planet_group.Enqueue(base_planet);
        // 선택한 블럭 기준 한 축에 인접해있는 같은 종류의 블럭을 탐색 및 저장하는 while 문 (Flood Fill 알고리즘)
        while (planet_group.Count > 0)
        {
            current_planet = planet_group.Dequeue();

            target_index = (current_planet.index_x, current_planet.index_y);

            // 기준 행성일 경우에만 기준점 설정
            if (selected_planet && other_planet)
                if (current_planet == selected_planet)
                {
                    destroy_planets.Add(current_planet);
                    target_index.Item1 += (int)direction.x;
                    target_index.Item2 += (int)direction.y;
                }
                else if (current_planet == other_planet)
                {
                    destroy_planets.Add(current_planet);
                    target_index.Item1 -= (int)direction.x;
                    target_index.Item2 -= (int)direction.y;
                }

            // 셀 크기를 벗어난 탐색을 하지 않도록 하기 위한 조건문
            if ((check_angle == SlideType.HORIZONTAL && target_index.Item1 < cell_size.x - 1) || (check_angle == SlideType.VERTICAL && target_index.Item2 < cell_size.y - 1))
            {
                // 검사하는 방향에 따른 조건 설정
                if (check_angle == SlideType.VERTICAL)
                    target_planet = planet_objs[target_index.Item1, target_index.Item2 + 1];
                else
                    target_planet = planet_objs[target_index.Item1 + 1, target_index.Item2];

                // 기준으로 했던 행성은 탐색 대상에서 제외
                if (target_planet != other_planet && target_planet != selected_planet)
                {
                    // 바로 위 / 오른쪽에 같은 종류의 블럭이 있다면
                    if (target_planet.planet_id == current_planet.planet_id && !target_planet.is_checked)
                    {
                        planet_group.Enqueue(target_planet);
                        destroy_planets.Add(target_planet);
                        target_planet.is_checked = true;
                    }
                }
            }
            // 셀 크기를 벗어난 탐색을 하지 않도록 하기 위한 조건문
            if ((check_angle == SlideType.HORIZONTAL && target_index.Item1 > 0) || (check_angle == SlideType.VERTICAL && target_index.Item2 > 0))
            {
                if (check_angle == SlideType.VERTICAL)
                    target_planet = planet_objs[target_index.Item1, target_index.Item2 - 1];
                else
                    target_planet = planet_objs[target_index.Item1 - 1, target_index.Item2];

                if (target_planet != other_planet && target_planet != selected_planet)
                {
                    // 바로 아래 / 왼쪽에 같은 종류의 블럭이 있다면
                    if (target_planet.planet_id == current_planet.planet_id && !target_planet.is_checked)
                    {
                        planet_group.Enqueue(target_planet);
                        destroy_planets.Add(target_planet);
                        target_planet.is_checked = true;
                    }
                }
            }
        }

        return destroy_planets;
    }

    void PlusScore(int destroy_count)
    {
        switch (destroy_count)
        {
            case 3:
                game_score += 150;
                if (!is_fever)
                    fever_guage += 5;
                SoundManager.Instance.PlayAudio("Three Block_3", 2);
                break;
            case 4:
                game_score += 200;
                if (!is_fever)
                    fever_guage += 10;
                SoundManager.Instance.PlayAudio("Four Block_2", 2);
                break;
            case 5:
                game_score += 350;
                if (!is_fever)
                    fever_guage += 20;
                SoundManager.Instance.PlayAudio("Five Block_3", 2);
                break;
            case 6:
                game_score += 500;
                if (!is_fever)
                    fever_guage += 30;
                SoundManager.Instance.PlayAudio("Other Block_5", 2);
                break;
            case 7:
                game_score += 700;
                if (!is_fever)
                    fever_guage += 40;
                SoundManager.Instance.PlayAudio("Other Block_1", 2);
                break;
        }

        if (!is_fever && fever_guage >= 100)
        {
            is_fever = true;
            SoundManager.Instance.PlayAudio("Warning_1", 2);
            StartCoroutine(FeverTime());
        }
    }

    /// <summary>
    /// Fever 게이지가 다 찼을 때 다시 0으로 천천히 감소시키는 함수
    /// </summary>
    /// <returns></returns>
    IEnumerator FeverTime()
    {
        WaitForSeconds second = new WaitForSeconds(0.01f);
        while (fever_guage > 0)
        {
            fever_guage -= 0.5f;
            yield return second;
        }

        is_fever = false;
    }

    /// <summary>
    /// 7 X 7의 모든 행성을 검사하여 이어진 행성을 파괴하는 함수
    /// </summary>
    void CheckAllPlanets()
    {
        List<(Planet, SlideType)> linked_planets = new List<(Planet, SlideType)>();
        int x, y;
        bool retry = false;
        int linked_count = 1;
        int destroy_count = 0;

        // 연결된 갯수가 3개 이상일 때 해당 블럭을 기준으로 입력한 축을 검사하는 함수
        void CheckAngle(Planet target_planet, SlideType check_angle)
        {
            destroy_count = 0;

            List<Planet> linked_planet = new List<Planet>();

            linked_planet = CheckAnglePlanet(target_planet, default, check_angle, true);
            if (linked_planet.Count >= 3)
            {
                foreach (var planet in linked_planet)
                {
                    planet_objs[planet.index_x, planet.index_y] = null;
                    Destroy(planet.gameObject);
                    destroy_count++;
                }

                SpawnEmptyCell();
                retry = true;

                PlusScore(destroy_count);
            }
            else foreach (var plannet in linked_planet) plannet.is_checked = false;
        }
        void CheckMultiAngle(Planet target_planet)
        {
            destroy_count = 0;

            List<Planet> horizontal_linked_planets = new List<Planet>();
            List<Planet> vertical_linked_planets = new List<Planet>();

            horizontal_linked_planets = CheckAnglePlanet(target_planet, default, SlideType.HORIZONTAL, true);
            vertical_linked_planets = CheckAnglePlanet(target_planet, default, SlideType.VERTICAL, false);

            if (horizontal_linked_planets.Count >= 3 && vertical_linked_planets.Count >= 3)
            {
                horizontal_linked_planets.AddRange(vertical_linked_planets);

                foreach (var planet in horizontal_linked_planets)
                {
                    planet_objs[planet.index_x, planet.index_y] = null;
                    Destroy(planet.gameObject);
                    destroy_count++;
                }

                SpawnEmptyCell();
                retry = true;
                PlusScore(destroy_count);
            }
            else foreach (var plannet in horizontal_linked_planets) plannet.is_checked = false;
        }

        // 세로로 세 칸 연결되었는지 확인
        for (x = 0; x < (int)cell_size.x; x++)
        {
            for (y = 1; y < (int)cell_size.x; y++)
            {
                if (planet_objs[x, y].planet_id == planet_objs[x, y - 1].planet_id)
                {
                    linked_count++;
                }
                else
                {
                    if (linked_count >= 3)
                    {
                        for (int i = 1; i <= linked_count; i++)
                            linked_planets.Add((planet_objs[x, y - i], SlideType.VERTICAL));
                        linked_count = 1;
                    }
                }
            }
            if (linked_count >= 3)
            {
                for (int i = 1; i <= linked_count; i++)
                    linked_planets.Add((planet_objs[x, y - i], SlideType.VERTICAL));
            }
            linked_count = 1;
        }

        // 가로로 세 칸 연결되었는지 확인
        for (y = 0; y < (int)cell_size.x; y++)
        {
            for (x = 1; x < (int)cell_size.x; x++)
            {
                if (planet_objs[x, y].planet_id == planet_objs[x - 1, y].planet_id)
                {
                    linked_count++;
                }
                else
                {
                    if (linked_count >= 3)
                    {
                        for (int i = 1; i <= linked_count; i++)
                            linked_planets.Add((planet_objs[x - i, y], SlideType.HORIZONTAL));
                        linked_count = 1;
                    }
                }
            }
            if (linked_count >= 3)
            {
                for (int i = 1; i <= linked_count; i++)
                    linked_planets.Add((planet_objs[x - i, y], SlideType.HORIZONTAL));
            }
            linked_count = 1;
        }

        // 검사해야 할 방향을 구분
        var linked_planet_list = from planet in linked_planets
                                 group planet by planet.Item2 into group_by_angle
                                 select new { angle = group_by_angle.Key, planet_list = group_by_angle };

        List<Planet> vertical_planets = new List<Planet>();
        List<Planet> horizontal_planets = new List<Planet>();
        // 가로와 세로 둘 다 세 칸 이상 이어져 있으면 둘 다 지워주기 위함
        List<Planet> multi_planets = new List<Planet>();

        // 가로축, 세로축에 따라 각각의 리스트에 할당
        foreach (var linked_planet in linked_planet_list)
        {
            foreach (var planet in linked_planet.planet_list)
            {
                if (linked_planet.angle == SlideType.VERTICAL)
                {
                    vertical_planets.Add(planet.Item1);
                }
                else
                {
                    horizontal_planets.Add(planet.Item1);
                }
            }
        }

        int limit_i = vertical_planets.Count;
        int limit_j = horizontal_planets.Count;

        // 가로축과 세로축 모두가 세 칸 이상 이어져 있을 때 따로 분리
        for (int i = 0; i < limit_i; i++)
        {
            for (int j = 0; j < limit_j; j++)
            {
                if (i >= vertical_planets.Count) break;

                if (horizontal_planets[j] == vertical_planets[i])
                {
                    multi_planets.Add(horizontal_planets[j]);

                    horizontal_planets.Remove(horizontal_planets[j]);
                    vertical_planets.Remove(vertical_planets[i]);
                }

                limit_i = vertical_planets.Count;
                limit_j = horizontal_planets.Count;
            }
        }

        foreach (var planet in multi_planets)
        {
            CheckMultiAngle(planet);
        }

        foreach (var vertical_planet in vertical_planets)
        {
            CheckAngle(vertical_planet, SlideType.VERTICAL);
        }
        foreach (var horizontal_planet in horizontal_planets)
        {
            CheckAngle(horizontal_planet, SlideType.HORIZONTAL);
        }

        if (retry) Invoke(nameof(CheckAllPlanets), 0.3f);
        else
        {
            CheckCanDestroy();
            is_slidable = true;
        }
    }

    void CheckCanDestroy()
    {
        List<Planet>[] linked_planets_1;
        List<Planet>[] linked_planets_2;

        Planet planet;

        for (int x = 0; x < (int)cell_size.x; x++)
        {
            for (int y = 0; y < (int)cell_size.y - 2; y++)
            {
                if (y < (int)cell_size.y - 3)
                {
                    linked_planets_1 = new List<Planet>[6] { new List<Planet>(), new List<Planet>(), new List<Planet>(), new List<Planet>(), new List<Planet>(), new List<Planet>() };

                    for (int _y = 0; _y < 4; _y++)
                    {
                        planet = planet_objs[x, y + _y];

                        linked_planets_1[planet.planet_id].Add(planet);
                        if (linked_planets_1[planet.planet_id].Count >= 3)
                        {
                            hint_planets = linked_planets_1[planet.planet_id];
                            return;
                        }
                    }
                }

                if (x < (int)cell_size.x - 1)
                {
                    linked_planets_2 = new List<Planet>[6] { new List<Planet>(), new List<Planet>(), new List<Planet>(), new List<Planet>(), new List<Planet>(), new List<Planet>() };

                    for (int _y = 0; _y < 3; _y++)
                    {
                        if (planet_objs[x, y + _y].planet_id == planet_objs[x + 1, y + _y].planet_id)
                        {
                            planet = planet_objs[x, y + _y];
                            linked_planets_2[planet.planet_id].Add(planet);
                        }
                        else
                        {
                            planet = planet_objs[x, y + _y];
                            linked_planets_2[planet.planet_id].Add(planet);

                            planet = planet_objs[x + 1, y + _y];
                            linked_planets_2[planet.planet_id].Add(planet);
                        }

                        if (linked_planets_2[planet_objs[x, y + _y].planet_id].Count >= 3)
                        {
                            hint_planets = linked_planets_2[planet_objs[x, y + _y].planet_id];
                        }
                        if (linked_planets_2[planet_objs[x + 1, y + _y].planet_id].Count >= 3)
                        {
                            hint_planets = linked_planets_2[planet_objs[x + 1, y + _y].planet_id];
                            return;
                        }
                    }
                }
            }
        }

        for (int x = 0; x < (int)cell_size.x - 2; x++)
        {
            for (int y = 0; y < (int)cell_size.y; y++)
            {
                if (x < (int)cell_size.x - 3)
                {
                    linked_planets_1 = new List<Planet>[6] { new List<Planet>(), new List<Planet>(), new List<Planet>(), new List<Planet>(), new List<Planet>(), new List<Planet>() };

                    for (int _x = 0; _x < 4; _x++)
                    {
                        planet = planet_objs[x + _x, y];

                        linked_planets_1[planet.planet_id].Add(planet);
                        if (linked_planets_1[planet.planet_id].Count >= 3)
                        {
                            hint_planets = linked_planets_1[planet.planet_id];
                            return;
                        }
                    }
                }
                if (y < (int)cell_size.y - 1)
                {
                    linked_planets_2 = new List<Planet>[6] { new List<Planet>(), new List<Planet>(), new List<Planet>(), new List<Planet>(), new List<Planet>(), new List<Planet>() };

                    for (int _x = 0; _x < 3; _x++)
                    {
                        if (planet_objs[x + _x, y].planet_id == planet_objs[x + _x, y + 1].planet_id)
                        {
                            planet = planet_objs[x + _x, y];
                            linked_planets_2[planet.planet_id].Add(planet);
                        }
                        else
                        {
                            planet = planet_objs[x + _x, y];
                            linked_planets_2[planet.planet_id].Add(planet);

                            planet = planet_objs[x + _x, y + 1];
                            linked_planets_2[planet.planet_id].Add(planet);
                        }

                        if (linked_planets_2[planet_objs[x + _x, y].planet_id].Count >= 3)
                        {
                            hint_planets = linked_planets_2[planet_objs[x + _x, y].planet_id];
                        }
                        if (linked_planets_2[planet_objs[x + _x, y + 1].planet_id].Count >= 3)
                        {
                            hint_planets = linked_planets_2[planet_objs[x + _x, y + 1].planet_id];
                            return;
                        }
                    }
                }
            }
        }

        ReplacePlanetPostions();
    }
}