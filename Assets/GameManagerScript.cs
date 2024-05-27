using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManagerScript : MonoBehaviour
{

    ///////////////////////////////////////////////////////////////////////////////////////////////////////
    ///
    ///                             �@�@�@�@�@�錾
    ///
    ///////////////////////////////////////////////////////////////////////////////////////////////////////



    // �ǉ�
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

    // ���x���f�U�C���p�̔z��
    int[,] map;
    int[,] map1;
    int[,] map2;
    int[,] map3;

    // ���݂̃V�[��
    int currentStage = 1;

    // �^�[�Q�b�g�̐�
    int targetIndex = 0;

    // �Q�[���Ǘ��p�̔z��
    GameObject[,] field;
    GameObject[] target;

    GameObject playerObj;

    List<GameObject> allObj = new List<GameObject>();

    // �N���A��Ԃ�ǐՂ���t���O
    bool isCleared = false;
    bool isLastStage = false;

    //=============================================================
    // �X�e�[�W�؂�ւ����s���֐�
    //=============================================================
    void SwitchStage(int stage)
    {

        // �X�e�[�W�ɉ����ă}�b�v��؂�ւ���
        switch (stage)
        {
            /*===============================================*/
            // �X�e�[�W1
            case 1:

                map = map1;

                break;
            /*===============================================*/
            // �X�e�[�W2
            case 2:

                map = map2;

                break;
            /*===============================================*/
            // �X�e�[�W3
            case 3:

                map = map3;

                break;
        }

        field = new GameObject[map.GetLength(0), map.GetLength(1)];

        // �V�����X�e�[�W��������
        InitializeField();
    }

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
            for (int x = 0; x < map.GetLength(1); x++)
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
        for (int i = 0; i < goals.Count; i++)
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
    void Reset()
    {

        allObj.Clear();

        SetOtherObjectsActive(false);

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

        for (int i = 0; i < targetIndex; i++)
        {
            Destroy(target[i]);
        }

        SwitchStage(currentStage);

        // �N���A��Ԃ����Z�b�g
        isCleared = false;
    }

    //=============================================================
    // ���W�̏��������s���֐�
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
    // Text�ȊO�̃I�u�W�F�N�g���\���ɂ���֐�
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

        // �X�N���[�����[�h
        Screen.SetResolution(1280, 720, false);

        // �z��̎��Ԃ̍쐬�Ə�����
        // �X�e�[�W1
        map1 = new int[,]
        {
           {4,4,4,4,4,4,4 },
           {4,3,2,0,0,0,4 },
           {4,3,2,0,0,0,4 },
           {4,3,2,1,0,0,4 },
           {4,0,0,0,0,0,4 },
           {4,4,4,4,4,4,4 }
        };

        // �X�e�[�W2
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

        // �X�e�[�W3
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

        // �X�e�[�W1��������
        SwitchStage(1);
    }

    // Update is called once per frame
    void Update()
    {

        if (!IsCleard())
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
        }

        if (!isLastStage)
        {
            // R�L�[�Ń��Z�b�g
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

        // �N���A����
        if (IsCleard() && !isCleared)
        {
            // �N���A��Ԃɐݒ�
            isCleared = true;

            // �N���A�̉�
            clearSE.Play();
        }

        else if (currentStage < 3 && IsCleard() && isCleared)
        {

            // �N���A��Ԃɐݒ�
            isCleared = true;

            // �N���A�̉�
            clearSE.Play();

            // Text�ȊO�̃I�u�W�F�N�g���\���ɂ���
            SetOtherObjectsActive(false);

            // �X�e�[�W�N���A���̏���
            currentStage++;
            SwitchStage(currentStage);
        }
        else if (currentStage == 3 && IsCleard() && isCleared)
        {
            // �N���A��Ԃɐݒ�
            isCleared = true;

            // �Q�[���I�u�W�F�N�g��SetActive���\�b�h���g���L����
            clearText.SetActive(true);

            // �N���A�̉�
            clearSE.Play();

            // Text�ȊO�̃I�u�W�F�N�g���\���ɂ���
            SetOtherObjectsActive(false);

            isLastStage = true;
        }

        if (isLastStage)
        {
            // �ŏ�����
            if (Input.GetKeyDown(KeyCode.Return))
            {
                currentStage = 1;
                Reset();
            }
        }
    }
}