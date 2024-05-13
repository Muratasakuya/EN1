using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{

    // 追加
    public GameObject playerPrefab;
    public GameObject boxPrefab;
    public GameObject goalPrefab;

    public GameObject clearText;

    // レベルデザイン用の配列
    int[,] map;
    // ゲーム管理用の配列
    GameObject[,] field;

    GameObject playerObj;

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
    // 2(箱)を押す関数
    //=============================================================
    bool MoveNumber(Vector2Int moveFrom, Vector2Int moveTo)
    {

        // 二次元配列に対応
        if (moveTo.y < 0 || moveTo.y >= field.GetLength(0)) { return false; }
        if (moveTo.x < 0 || moveTo.x >= field.GetLength(1)) { return false; }

        // 箱を押す処理
        // Boxタグを持っていたら再帰処理
        if (field[moveTo.y, moveTo.x] != null && field[moveTo.y, moveTo.x].tag == "Box")
        {

            Vector2Int velocity = moveTo - moveFrom;
            bool success = MoveNumber(moveTo, moveTo + velocity);
            if (!success) { return false; }
        }

        field[moveFrom.y, moveFrom.x].transform.position =
            new Vector3(moveTo.x, field.GetLength(0) - moveTo.y, 0);
        field[moveTo.y, moveTo.x] = field[moveFrom.y, moveFrom.x];
        field[moveFrom.y, moveFrom.x] = null;

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
        for (int i=0; i<goals.Count; i++)
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

    // Start is called before the first frame update
    // Start = Initilize
    void Start()
    {

        // 配列の実態の作成と初期化
        map = new int[,]
        {
           {3,2,0,0,0 },
           {3,2,0,0,0 },
           {3,2,1,0,0 },
           {0,0,0,0,0 },
           {0,0,0,0,0 }
        };

        field = new GameObject
        [
            map.GetLength(0),
            map.GetLength(1)
        ];

        // 二重for分で二次元配列の情報を取得
        for (int y = 0; y < map.GetLength(0); y++)
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                // プレイヤー
                if (map[y, x] == 1)
                {

                    field[y, x] = Instantiate(
                        playerPrefab,
                        new Vector3(x, map.GetLength(0) - y, 0),
                        Quaternion.identity
                    );
                }
                // 箱
                if (map[y, x] == 2)
                {

                    field[y, x] = Instantiate(
                        boxPrefab,
                        new Vector3(x, map.GetLength(0) - y, 0),
                        Quaternion.identity
                    );
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

        // 右矢印キーを押したとき
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {

            Vector2Int playerIndex = GetPlayerIndex();

            MoveNumber(
            playerIndex,
            playerIndex + new Vector2Int(1, 0));
        }

        // 左矢印キーを押したとき
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Vector2Int playerIndex = GetPlayerIndex();

            MoveNumber(
            playerIndex,
            playerIndex + new Vector2Int(-1, 0));
        }

        // 上矢印キーを押したとき
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {

            Vector2Int playerIndex = GetPlayerIndex();

            MoveNumber(
            playerIndex,
            playerIndex + new Vector2Int(0, -1));
        }

        // 下矢印キーを押したとき
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Vector2Int playerIndex = GetPlayerIndex();

            MoveNumber(
            playerIndex,
            playerIndex + new Vector2Int(0, 1));
        }

        // クリア判定
        if (IsCleard())
        {
            // ゲームオブジェクトのSetActiveメソッドを使い有効化
            clearText.SetActive(true);
        }
    }
}
