using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{

    // �ǉ�
    public GameObject playerPrefab;
    public GameObject boxPrefab;
    public GameObject goalPrefab;

    public GameObject clearText;

    // ���x���f�U�C���p�̔z��
    int[,] map;
    // �Q�[���Ǘ��p�̔z��
    GameObject[,] field;

    GameObject playerObj;

    //=============================================================
    // �v�f��������Ȃ������Ƃ���-1��������֐�
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
    // 2(��)�������֐�
    //=============================================================
    bool MoveNumber(Vector2Int moveFrom, Vector2Int moveTo)
    {

        // �񎟌��z��ɑΉ�
        if (moveTo.y < 0 || moveTo.y >= field.GetLength(0)) { return false; }
        if (moveTo.x < 0 || moveTo.x >= field.GetLength(1)) { return false; }

        // ������������
        // Box�^�O�������Ă�����ċA����
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
    // �N���A������s���֐�
    //=============================================================
    bool IsCleard()
    {

        // Vector2Int�^�̉ϒ��z��
        List<Vector2Int> goals = new List<Vector2Int>();

        for (int y = 0; y < map.GetLength(0); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                // �i�[�ꏊ���ۂ��𔻒f
                if (map[y, x] == 3)
                {
                    // �i�[�ꏊ�̃C���f�b�N�X���T���Ă���
                    goals.Add(new Vector2Int(x, y));
                }
            }
        }

        // �v�f����goals.Count�擾
        for (int i=0; i<goals.Count; i++)
        {

            GameObject f = field[goals[i].y, goals[i].x];
            if (f == null || f.tag != "Box")
            {
                // 1�ł������Ȃ�������������B��
                return false;
            }
        }

        // �������B���łȂ���Ώ����B��
        return true;
    }

    // Start is called before the first frame update
    // Start = Initilize
    void Start()
    {

        // �z��̎��Ԃ̍쐬�Ə�����
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

        // ��dfor���œ񎟌��z��̏����擾
        for (int y = 0; y < map.GetLength(0); y++)
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                // �v���C���[
                if (map[y, x] == 1)
                {

                    field[y, x] = Instantiate(
                        playerPrefab,
                        new Vector3(x, map.GetLength(0) - y, 0),
                        Quaternion.identity
                    );
                }
                // ��
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

        // �E���L�[���������Ƃ�
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {

            Vector2Int playerIndex = GetPlayerIndex();

            MoveNumber(
            playerIndex,
            playerIndex + new Vector2Int(1, 0));
        }

        // �����L�[���������Ƃ�
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Vector2Int playerIndex = GetPlayerIndex();

            MoveNumber(
            playerIndex,
            playerIndex + new Vector2Int(-1, 0));
        }

        // ����L�[���������Ƃ�
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {

            Vector2Int playerIndex = GetPlayerIndex();

            MoveNumber(
            playerIndex,
            playerIndex + new Vector2Int(0, -1));
        }

        // �����L�[���������Ƃ�
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Vector2Int playerIndex = GetPlayerIndex();

            MoveNumber(
            playerIndex,
            playerIndex + new Vector2Int(0, 1));
        }

        // �N���A����
        if (IsCleard())
        {
            // �Q�[���I�u�W�F�N�g��SetActive���\�b�h���g���L����
            clearText.SetActive(true);
        }
    }
}
