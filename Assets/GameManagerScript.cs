using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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
    int[,] map1;
    int[,] map2;
    int[,] map3;

    // 現在のシーン
    int currentStage = 1;

    // ターゲットの数
    int targetIndex = 0;

    // ゲーム管理用の配列
    GameObject[,] field;
    GameObject[] target;

    GameObject playerObj;

    List<GameObject> allObj = new List<GameObject>();

    // クリア状態を追跡するフラグ
    bool isCleared = false;
    bool isLastStage = false;

    //=============================================================
    // ステージ切り替えを行う関数
    //=============================================================
    void SwitchStage(int stage)
    {

        // ステージに応じてマップを切り替える
        switch (stage)
        {
            /*===============================================*/
            // ステージ1
            case 1:

                map = map1;

                break;
            /*===============================================*/
            // ステージ2
            case 2:

                map = map2;

                break;
            /*===============================================*/
            // ステージ3
            case 3:

                map = map3;

                break;
        }

        field = new GameObject[map.GetLength(0), map.GetLength(1)];

        // 新しいステージを初期化
        InitializeField();
    }

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

        if (map[moveTo.y, moveTo.x] == 3 && field[moveTo.y, moveTo.x].tag == "Box")
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
            for (int x = 0; x < map.GetLength(1); x++)
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

        allObj.Clear();

        SetOtherObjectsActive(false);

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

        for (int i = 0; i < targetIndex; i++)
        {
            Destroy(target[i]);
        }

        SwitchStage(currentStage);

        // クリア状態をリセット
        isCleared = false;
    }

    //=============================================================
    // 座標の初期化を行う関数
    //=============================================================
    void InitializeField()
    {

        for (int i = 0; i < targetIndex; i++)
        {
            Destroy(target[i]);
        }

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

                    targetIndex++;
                }
                if (map[y, x] == 4)
                {
                    field[y, x] = Instantiate(wallPrefab, position, Quaternion.identity);
                }
            }
        }

        target = new GameObject[targetIndex];
        int targetNum = targetIndex;

        for (int y = 0; y < map.GetLength(0); y++)
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                if (map[y, x] == 3)
                {

                    Vector3 position = new Vector3(x, -y, 0) - offset;

                    targetNum--;
                    if (targetNum < 0)
                    {
                        break;
                    }

                    target[targetNum] = Instantiate(targetPrefab, position, Quaternion.LookRotation(new Vector3(90.0f, 0.0f, 0.0f)));

                }
            }
        }

        clearText.SetActive(false);
    }

    //=============================================================
    // Text以外のオブジェクトを非表示にする関数
    //=============================================================
    void SetOtherObjectsActive(bool active)
    {

        for (int i = 0; i < targetIndex; i++)
        {
            Destroy(target[i]);
        }

        foreach (GameObject obj in allObj)
        {

            Destroy(obj);
        }
    }

    // Start is called before the first frame update
    void Start()
    {

        // スクリーンモード
        Screen.SetResolution(1280, 720, false);

        // 配列の実態の作成と初期化
        // ステージ1
        map1 = new int[,]
        {
           {4,4,4,4,4,4,4 },
           {4,3,2,0,0,0,4 },
           {4,3,2,0,0,0,4 },
           {4,3,2,1,0,0,4 },
           {4,0,0,0,0,0,4 },
           {4,4,4,4,4,4,4 }
        };

        // ステージ2
        map2 = new int[,]
        {
           {4,4,4,4,4,4,4,4,4 },
           {4,0,0,0,0,0,0,0,4 },
           {4,0,0,3,2,0,2,0,4 },
           {4,0,0,0,0,2,3,0,4 },
           {4,0,1,0,0,0,0,0,4 },
           {4,0,3,0,0,0,0,0,4 },
           {4,4,4,4,4,4,4,4,4 }
        };

        // ステージ3
        map3 = new int[,]
        {
           {4,4,4,4,4,4,4,4,4,4 },
           {4,0,0,0,0,0,0,0,0,4 },
           {4,0,3,0,2,0,0,3,0,4 },
           {4,0,0,1,4,4,0,0,0,4 },
           {4,0,0,0,0,2,2,0,0,4 },
           {4,4,3,0,0,0,0,0,0,4 },
           {4,0,4,0,0,4,4,0,4,4 },
           {4,4,4,4,4,4,4,4,4,4 }
        };

        // ステージ1を初期化
        SwitchStage(1);
    }

    // Update is called once per frame
    void Update()
    {

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

        if (!isLastStage)
        {
            // Rキーでリセット
            if (Input.GetKeyDown(KeyCode.R))
            {
                Reset();
            }
        }

        for (int y = 0; y < map.GetLength(0); y++)
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                if (field[y, x] != null)
                {
                    allObj.Add(field[y, x]);
                }
            }
        }

        // クリア判定
        if (IsCleard() && !isCleared)
        {
            // クリア状態に設定
            isCleared = true;

            // クリアの音
            clearSE.Play();
        }

        else if (currentStage < 3 && IsCleard() && isCleared)
        {

            // クリア状態に設定
            isCleared = true;

            // クリアの音
            clearSE.Play();

            // Text以外のオブジェクトを非表示にする
            SetOtherObjectsActive(false);

            // ステージクリア時の処理
            currentStage++;
            SwitchStage(currentStage);
        }
        else if (currentStage == 3 && IsCleard() && isCleared)
        {
            // クリア状態に設定
            isCleared = true;

            // ゲームオブジェクトのSetActiveメソッドを使い有効化
            clearText.SetActive(true);

            // クリアの音
            clearSE.Play();

            // Text以外のオブジェクトを非表示にする
            SetOtherObjectsActive(false);

            isLastStage = true;
        }

        if (isLastStage)
        {
            // 最初から
            if (Input.GetKeyDown(KeyCode.Return))
            {
                currentStage = 1;
                Reset();
            }
        }
    }
}