using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{

    // �ǉ�
    public GameObject playerPrefab;
    public GameObject boxPrefab;
    public GameObject goalPrefab;
    public GameObject targetPrefab;
    public GameObject clearText;
    public GameObject particlePrefab;
    public GameObject wallPrefab;

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
    // ���������֐�
    //=============================================================
    bool MoveNumber(Vector2Int moveFrom, Vector2Int moveTo)
    {

        // �񎟌��z��ɑΉ�
        if (moveTo.y < 0 || moveTo.y >= field.GetLength(0)) { return false; }
        if (moveTo.x < 0 || moveTo.x >= field.GetLength(1)) { return false; }

        // �ړ���ɕǂ�����ꍇ�͉����Ȃ�
        if (field[moveTo.y, moveTo.x] != null && field[moveTo.y, moveTo.x].tag == "Wall")
        {
            return false;
        }

        // ������ɔ�������ꍇ
        if (field[moveTo.y, moveTo.x] != null && field[moveTo.y, moveTo.x].tag == "Box")
        {
            Vector2Int velocity = moveTo - moveFrom;
            Vector2Int nextMoveTo = moveTo + velocity;

            // ���̃}�X���t�B�[���h�O���ǂ܂��͔�������ꍇ�͉����Ȃ�
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

        // ���̈ړ�
        field[moveTo.y, moveTo.x] = field[moveFrom.y, moveFrom.x];

        for (int i = 0; i < 5; i++)
        {
            Instantiate(
                particlePrefab,
                new Vector3(moveFrom.x, map.GetLength(0) - moveFrom.y, 0),
                Quaternion.identity
            );
        }

        Vector3 moveToPosition = new Vector3(moveTo.x, map.GetLength(0) - moveTo.y, 0);
        field[moveTo.y, moveTo.x].GetComponent<Move>().MoveTo(moveToPosition);
        field[moveFrom.y, moveFrom.x] = null;

        return true;
    }

    //=============================================================
    // �ړ��\���ǂ������`�F�b�N����֐�
    //=============================================================
    bool CanMoveTo(Vector2Int position)
    {
        // �t�B�[���h�O�`�F�b�N
        if (position.y < 0 || position.y >= field.GetLength(0) || position.x < 0 || position.x >= field.GetLength(1))
        {
            return false;
        }

        // �ړ���ɕǂ�����ꍇ�͈ړ��s��
        if (field[position.y, position.x] != null && field[position.y, position.x].tag == "Wall")
        {
            return false;
        }

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

    //=============================================================
    // ���Z�b�g���s���֐�
    //=============================================================
    private void Reset()
    {

        // ���݂̃I�u�W�F�N�g���폜����
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

        // �����̔z�u���ēx�ݒ肷��
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
                // �i�[�ꏊ
                if (map[y, x] == 3)
                {
                    field[y, x] = Instantiate(
                        targetPrefab,
                        new Vector3(x, map.GetLength(0) - y, 0.01f),
                        Quaternion.identity
                    );
                }
                // ��
                if (map[y, x] == 4)
                {

                    field[y, x] = Instantiate(
                        wallPrefab,
                        new Vector3(x, map.GetLength(0) - y, 0.0f),
                        Quaternion.identity
                    );
                }
            }
        }

        // �N���A�e�L�X�g���\���ɂ���
        clearText.SetActive(false);
    }

    // Start is called before the first frame update
    // Start = Initilize
    void Start()
    {

        // �X�N���[�����[�h
        Screen.SetResolution(1280, 720, false);

        // �z��̎��Ԃ̍쐬�Ə�����
        map = new int[,]
        {
           {4,4,4,4,4,4,4 },
           {4,3,2,0,0,0,4 },
           {4,3,2,0,0,0,4 },
           {4,3,2,1,0,0,4 },
           {4,0,0,0,0,0,4 },
           {4,4,4,4,4,4,4 }
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
                        new Vector3(x, map.GetLength(0) - y, 0.0f),
                        Quaternion.identity
                    );
                }
                // ��
                if (map[y, x] == 2)
                {

                    field[y, x] = Instantiate(
                        boxPrefab,
                        new Vector3(x, map.GetLength(0) - y, 0.0f),
                        Quaternion.identity
                    );
                }
                // �i�[�ꏊ
                if (map[y,x] == 3)
                {

                    field[y, x] = Instantiate(
                        targetPrefab,
                        new Vector3(x, map.GetLength(0) - y, 0.01f),
                        Quaternion.identity
                    );
                }
                // ��
                if (map[y, x] == 4)
                {

                    field[y, x] = Instantiate(
                        wallPrefab,
                        new Vector3(x, map.GetLength(0) - y, 0.0f),
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
            if (CanMoveTo(playerIndex + new Vector2Int(1, 0)))
            {
                MoveNumber(
                    playerIndex,
                    playerIndex + new Vector2Int(1, 0)
                );
            }
        }

        // �����L�[���������Ƃ�
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

        // ����L�[���������Ƃ�
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

        // �����L�[���������Ƃ�
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

        // R�L�[�Ń��Z�b�g
        if (Input.GetKeyDown(KeyCode.R))
        {

            Reset();
        }

        // �N���A����
        if (IsCleard())
        {
            // �Q�[���I�u�W�F�N�g��SetActive���\�b�h���g���L����
            clearText.SetActive(true);
        }
    }
}
