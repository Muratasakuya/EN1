using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{

    ///////////////////////////////////////////////////////////////////////////////////////////////////////
    ///
    ///                             　　　　　宣言
    ///
    ///////////////////////////////////////////////////////////////////////////////////////////////////////



    // 追加
    public GameObject playerPrefab;
    public GameObject boxPrefab;
    public GameObject goalPrefab;
    public GameObject targetPrefab;
    public GameObject clearText;
    public GameObject particlePrefab;
    public GameObject wallPrefab;

    public AudioSource playerMoveSE;
    public AudioSource boxPushSE;
    public AudioSource boxPlaceSE;
    public AudioSource clearSE;

    // レベルデザイン用の配列
    int[,] map;
    // ゲーム管理用の配列
    GameObject[,] field;

    GameObject playerObj;

    // クリア状態を追跡するフラグ
    bool isCleared = false;



    //=============================================================
    // 要素が見つからなかったときに-1を代入する関数
    //=============================================================
    Vector2Int GetPlayerIndex()
    {
        for (int y = 0; y < field.GetLength(0); y++)
        {
            for (int x = 0; x < field.GetLength(1); x++)
            {
                // nullCheck
                if (field[y, x] == null)
                    continue;

                if (field[y, x].tag == "Player")
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return new Vector2Int(-1, -1);
    }

    //=============================================================
    // 箱を押す関数
    //=============================================================
    bool MoveNumber(Vector2Int moveFrom, Vector2Int moveTo)
    {
       if (moveTo.y < 0 || moveTo.y >= field.GetLength(0)) { return false; }
        if (moveTo.x < 0 || moveTo.x >= field.GetLength(1)) { return false; }

        if (field[moveTo.y, moveTo.x] != null && field[moveTo.y, moveTo.x].tag == "Wall")
        {
            return false;
        }

        if (field[moveTo.y, moveTo.x] != null && field[moveTo.y, moveTo.x].tag == "Box")
        {
            Vector2Int velocity = moveTo - moveFrom;
            Vector2Int nextMoveTo = moveTo + velocity;

            if (nextMoveTo.y < 0 || nextMoveTo.y >= field.GetLength(0) || nextMoveTo.x < 0 || nextMoveTo.x >= field.GetLength(1))
            {
                return false;
            }

            if (field[nextMoveTo.y, nextMoveTo.x] != null &&
                (field[nextMoveTo.y, nextMoveTo.x].tag == "Box" || field[nextMoveTo.y, nextMoveTo.x].tag == "Wall"))
            {
                return false;
            }

            bool success = MoveNumber(moveTo, nextMoveTo);
            if (!success) { return false; }
        }

        field[moveTo.y, moveTo.x] = field[moveFrom.y, moveFrom.x];

        if (field[moveTo.y, moveTo.x] != null && field[moveTo.y, moveTo.x].tag == "Box")
        {
            boxPushSE.Play();
        }

        for (int i = 0; i < 5; i++)
        {
            Instantiate(
                particlePrefab,
                new Vector3(moveFrom.x - map.GetLength(1) / 2f + 0.5f, map.GetLength(0) / 2f - 0.5f - moveFrom.y, 0),
                Quaternion.identity
            );
        }

        Vector3 moveToPosition = new Vector3(moveTo.x - map.GetLength(1) / 2f + 0.5f, map.GetLength(0) / 2f - 0.5f - moveTo.y, 0);
        field[moveTo.y, moveTo.x].GetComponent<Move>().MoveTo(moveToPosition);
        field[moveFrom.y, moveFrom.x] = null;

        playerMoveSE.Play();

        if (map[moveTo.y, moveTo.x] == 3)
        {
            boxPlaceSE.Play();
        }

        return true;
    }

    //=============================================================
    // 移動可能かどうかをチェックする関数
    //=============================================================
    bool CanMoveTo(Vector2Int position)
    {
        // フィールド外チェック
        if (position.y < 0 || position.y >= field.GetLength(0) || position.x < 0 || position.x >= field.GetLength(1))
        {
            return false;
        }

        // 移動先に壁がある場合は移動不可
        if (field[position.y, position.x] != null && field[position.y, position.x].tag == "Wall")
        {
            return false;
        }

        return true;
    }

    //=============================================================
    // クリア判定を行う関数
    //=============================================================
    bool IsCleard()
    {
        // Vector2Int型の可変長配列
        List<Vector2Int> goals = new List<Vector2Int>();

        for (int y = 0; y < map.GetLength(0); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                // 格納場所か否かを判断
                if (map[y, x] == 3)
                {
                    // 格納場所のインデックスを控えておく
                    goals.Add(new Vector2Int(x, y));
                }
            }
        }

        // 要素数はgoals.Count取得
        for (int i = 0; i < goals.Count; i++)
        {
            GameObject f = field[goals[i].y, goals[i].x];
            if (f == null || f.tag != "Box")
            {
                // 1でも箱がなかったら条件未達成
                return false;
            }
        }

        // 条件未達成でなければ条件達成
        return true;
    }

    //=============================================================
    // リセットを行う関数
    //=============================================================
    void Reset()
    {
        // 現在のオブジェクトを削除する
        for (int y = 0; y < field.GetLength(0); y++)
        {
            for (int x = 0; x < field.GetLength(1); x++)
            {
                if (field[y, x] != null)
                {
                    Destroy(field[y, x]);
                    field[y, x] = null;
                }
            }
        }

        InitializeField();

        // クリアテキストを非表示にする
        clearText.SetActive(false);

        // クリア状態をリセット
        isCleared = false;
    }

    //=============================================================
    // 座標の初期化を行う関数
    //=============================================================
    void InitializeField()
    {
        Vector3 offset = new Vector3(map.GetLength(1) / 2f - 0.5f, -(map.GetLength(0) / 2f - 0.5f), 0);

        for (int y = 0; y < map.GetLength(0); y++)
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                Vector3 position = new Vector3(x, -y, 0) - offset;

                if (map[y, x] == 1)
                {
                    field[y, x] = Instantiate(playerPrefab, position, Quaternion.identity);
                }
                if (map[y, x] == 2)
                {
                    field[y, x] = Instantiate(boxPrefab, position, Quaternion.identity);
                }
                if (map[y, x] == 3)
                {
                    field[y, x] = Instantiate(targetPrefab, position + new Vector3(0, 0, 0.01f), Quaternion.identity);
                }
                if (map[y, x] == 4)
                {
                    field[y, x] = Instantiate(wallPrefab, position, Quaternion.identity);
                }
            }
        }
    }

    //=============================================================
    // Text以外のオブジェクトを非表示にする関数
    //=============================================================
    void SetOtherObjectsActive(bool active)
    {
        // プレイヤー、ボックス、目標、壁などのオブジェクトをループして非表示にする
        for (int y = 0; y < field.GetLength(0); y++)
        {
            for (int x = 0; x < field.GetLength(1); x++)
            {
                // field[y, x] がnullでない場合と、tagが"Text"でない場合に非表示にする
                if (field[y, x] != null && field[y, x].tag != "Text")
                {
                    field[y, x].SetActive(active);
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {

        // スクリーンモード
        Screen.SetResolution(1280, 720, false);

        // 配列の実態の作成と初期化
        map = new int[,]
        {
           {4,4,4,4,4,4,4 },
           {4,3,2,0,0,0,4 },
           {4,3,2,0,0,0,4 },
           {4,3,2,1,0,0,4 },
           {4,0,0,0,0,0,4 },
           {4,4,4,4,4,4,4 }
        };

        field = new GameObject[map.GetLength(0), map.GetLength(1)];

        InitializeField();

        clearText.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

        // クリア判定
        if (IsCleard() && !isCleared)
        {
            // クリア状態に設定
            isCleared = true;

            // ゲームオブジェクトのSetActiveメソッドを使い有効化
            clearText.SetActive(true);

            // クリアの音
            clearSE.Play();

            // Text以外のオブジェクトを非表示にする
            SetOtherObjectsActive(false);
        }

        if (!IsCleard())
        {
            // 右矢印キーを押したとき
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                Vector2Int playerIndex = GetPlayerIndex();
                if (CanMoveTo(playerIndex + new Vector2Int(1, 0)))
                {
                    MoveNumber(
                        playerIndex,
                        playerIndex + new Vector2Int(1, 0)
                    );
                }
            }

            // 左矢印キーを押したとき
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Vector2Int playerIndex = GetPlayerIndex();
                if (CanMoveTo(playerIndex + new Vector2Int(-1, 0)))
                {
                    MoveNumber(
                        playerIndex,
                        playerIndex + new Vector2Int(-1, 0)
                    );
                }
            }

            // 上矢印キーを押したとき
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                Vector2Int playerIndex = GetPlayerIndex();
                if (CanMoveTo(playerIndex + new Vector2Int(0, -1)))
                {
                    MoveNumber(
                        playerIndex,
                        playerIndex + new Vector2Int(0, -1)
                    );
                }
            }

            // 下矢印キーを押したとき
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                Vector2Int playerIndex = GetPlayerIndex();
                if (CanMoveTo(playerIndex + new Vector2Int(0, 1)))
                {
                    MoveNumber(
                        playerIndex,
                        playerIndex + new Vector2Int(0, 1)
                    );
                }
            }
        }

        // Rキーでリセット
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reset();
        }
    }
}