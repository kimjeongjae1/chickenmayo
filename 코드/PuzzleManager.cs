using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Linq;


public enum Dir
{
    Up, Right, Down, Left
}


[System.Serializable]
public struct InitPuzzle //ó���� ������ ����
{
    public PuzzleType type;
    public Vector2Int coordinate;
}


public class PuzzleManager : MonoBehaviour
{
    [Header("[���� ����Ŀ]")]
    [SerializeField]
    private PuzzleMaker maker;
    public PuzzleMaker Maker => maker;

    [Header("[���� �Ŵ���]")]
    [SerializeField]
    private SoundManager soundManager;
    public SoundManager SoundManager =>soundManager;

    [Header("[UI �Ŵ���]")]
    [SerializeField]
    private PuzzleUIManager uiManager;

    [Header("[���� ��ǥ ����]")]
    public int targetScore; // ��ǥ ����
    private bool isGameCleared = false;

    private int point;
    public int Point
    {
        get { return point; }
        set
        {
            point = value;
            uiManager.SetPointUI(point);

            // ��ǥ ���� �޼� ���� Ȯ��
            if (!isGameCleared && point >= targetScore)
            {
                GameClear();
            }
        }
    }

    private Puzzle[,] puzzles;

    private Puzzle selectPuzzle;

    public int X => Maker.X;
    public int Y => Maker.Y;

    [SerializeField]
    private float hintTime;

    public bool isClick = false;
    public bool isProcess = true;


    public Puzzle SelectPuzzle
    {
        get
        {
            return selectPuzzle;
        }

        set
        {
            selectPuzzle = value;
        }
    }
    private void Start()
    {
        // �� �̸��� ���� ��ǥ ���� ����
        string currentScene = SceneManager.GetActiveScene().name;
        switch (currentScene)
        {
            case "EasyScene":
                targetScore = 10000; // Easy ��� ��ǥ ����
                break;
            case "NormalScene":
                targetScore = 20000; // Normal ��� ��ǥ ����
                break;
            case "HardScene":
                targetScore = 30000; // Hard ��� ��ǥ ����
                break;
            default:
                targetScore = 500; // �⺻ ��ǥ ����
                break;
        }
        uiManager.SetTargetScoreUI(targetScore); // ��ǥ ������ UI�� ǥ��
    }

    // ���� Ŭ���� ó��
    private void GameClear()
    {
        isGameCleared = true;
        Debug.Log("Game Cleared!");

        // UI ǥ�� (���û���)
        uiManager.ShowGameClearUI();

        // ���� �ð� �� Ŭ���� ȭ������ �̵�
        StartCoroutine(WaitAndLoadGameClearScene());
    }
    private IEnumerator WaitAndLoadGameClearScene()
    {
        yield return new WaitForSeconds(2.0f); // 2�� ���
        SceneManager.LoadScene("GameClearScene"); // Ŭ���� ȭ������ ��ȯ
    }
    //���� �ʱ�ȭ
    public void InitPuzzles(int x, int y)
    {
        if (puzzles != null) return;

        puzzles = new Puzzle[x, y];

    }

    //��ǥ�� ���� ��������, OutOfIndex �˻�
    public Puzzle GetPuzzle(int x, int y)
    {
        if (IsOutOfIndex(x, y)) return null;

        return puzzles[x, y];
    }

    //�÷��������� �˻� �� ��������
    public Puzzle GetColorPuzzle(int x, int y)
    {
        if (IsOutOfIndex(x, y) || puzzles[x, y].color == PuzzleColor.None) return null;

        return puzzles[x, y];
    }

    //���� �迭�� ����, OutOfIndex �˻�
    public bool SetPuzzle(int x, int y, Puzzle newPuzzle)
    {
        if (IsOutOfIndex(x, y)) return false;

        puzzles[x, y] = newPuzzle;
        return true;
    }

    //���� �迭 ������ ������ �������� Ȯ��
    public bool IsOutOfIndex(int x, int y)
    {
        if (x < 0 || y < 0 || x >= X || y >= Y) return true;

        return false;
    }


    #region ���� ä���, ��Ʈ���� Ž��

    Coroutine fillco = null;

    //ä��� �����ϴ� �Լ�
    public void Fill()
    {
        CheckHintTime(false);
        if (fillco == null)
        {
            fillco = StartCoroutine(FillCor());
        }
    }

    IEnumerator FillCor()
    {
       
        isProcess = true;
        bool needFill = true;

        while (needFill)
        {
            while (FillRoutine())
            {
                //�������� �ð� 0.1�� ��ٷ������
                yield return new WaitForSeconds(0.1f);
            }

            yield return CheckPuzzleCo((isNeedFill) =>
            {
                needFill = isNeedFill;
            });

            // yield return new WaitForSeconds(0.2f); //������ ������ �ð� ��ٷ��ֱ�
        }

        isProcess = false;
        fillco = null;

        CheckHintTime(true);
    }


    public bool FillRoutine()
    {
        bool isBlockMove = false;

        for (int j = Y - 2; j >= 0; j--)
        {
            for (int i = 0; i < X; i++) //�Ʒ����� ���� ���� �Ȱ� �ö�
            {

                if (puzzles[i, j] == null || !puzzles[i, j].IsMoveable()) continue;

                Puzzle curPuzzle = puzzles[i, j];
                Puzzle belowPuzzle = puzzles[i, j + 1];

                if (belowPuzzle == null) //���� ���ٸ� �׳� ����
                {
                    PuzzleChange(curPuzzle, i, j + 1);
                    isBlockMove = true;
                }
                else
                {

                    if (/*belowPuzzle.type == PuzzleType.Obstacle ||*/ CheckIsObstacle(i - 1, j) || CheckIsObstacle(i + 1, j))//�Ʒ�or���� ��ֹ��̶�� �밢�� �¿� �ϴ� Ž�� 
                    {


                        for (int diag = -1; diag <= 1; diag += 2)
                        {
                            if (i + diag < 0 || i + diag >= X) continue;

                            /*if (belowPuzzle.type == PuzzleType.Obstacle && puzzles[i + diag, j] != null) continue;*/

                            Puzzle newDiagPuzzle = puzzles[i + diag, j + 1];

                            if (newDiagPuzzle == null)
                            {
                                PuzzleChange(curPuzzle, i + diag, j + 1);
                                isBlockMove = true;

                                break;
                            }
                        }
                    }
                }
            }
        }


        //�ֻ�� ���� ��������
        for (int i = 0; i < X; i++)
        {
            if (puzzles[i, 0] == null)
            {
                Puzzle newPuzzle = Maker.MakeNewPuzzle(i, -1, PuzzleType.Normal);

                newPuzzle.SetCoordinate(i, 0);
                newPuzzle.Move(0.1f);

                isBlockMove = true;
            }
        }

        return isBlockMove;
    }

 
    //���� �̵� �� �迭,x,y�� �ٲٱ�
    void PuzzleChange(Puzzle curPuzzle, int newX, int newY)
    {
        puzzles[curPuzzle.X, curPuzzle.Y] = null;
        curPuzzle.SetCoordinate(newX, newY);
        curPuzzle.Move(0.1f);
    }

    bool CheckIsObstacle(int x, int y)
    {
        //�ε��� ���� �Ѿ���� üũ
        if (x < 0 || y < 0 || x >= this.X || y >= this.Y)
            return false;

        if (puzzles[x, y] == null || puzzles[x, y].type != PuzzleType.Obstacle)
            return false;

        return true;
    }


    //�ð�������� �˻� �迭
    private int[] dx = new int[] { 0, 1, 0, -1 };
    private int[] dy = new int[] { -1, 0, 1, 0 };


    IEnumerator CheckPuzzleCo(Action<bool> callBack)
    {
        bool isDestroyBlock = false;

        List<Puzzle> itemPuzzles = new List<Puzzle>();
        List<Puzzle> destroyPuzzles = new List<Puzzle>();
        Queue<Puzzle> searchQueue = new Queue<Puzzle>();

        for (int j = 0; j < Y; j++)
        {
            for (int i = 0; i < X; i++)
            {

                if (puzzles[i, j] == null || puzzles[i, j].color == PuzzleColor.None) continue;

                HashSet<Puzzle> visitPuzzles = new HashSet<Puzzle>();
                searchQueue.Enqueue(puzzles[i, j]);
                visitPuzzles.Add(puzzles[i, j]);

                PuzzleType rewardType = PuzzleType.Empty;

                while (searchQueue.Count != 0)
                {
                    Puzzle curPuzzle = searchQueue.Dequeue();

                    List<List<Puzzle>> findPuzzles = new List<List<Puzzle>>();

                    List<Puzzle> up = new List<Puzzle>();
                    List<Puzzle> right = new List<Puzzle>();
                    List<Puzzle> down = new List<Puzzle>();
                    List<Puzzle> left = new List<Puzzle>();

                    findPuzzles.Add(up);
                    findPuzzles.Add(right);
                    findPuzzles.Add(down);
                    findPuzzles.Add(left);

                    //���� ���񿡼� �����¿� Ž��
                    for (int k = 0; k < 4; k++)
                    {
                        int newX = curPuzzle.X + dx[k];
                        int newY = curPuzzle.Y + dy[k];

                        do
                        {
                            Puzzle newPuzzle = GetPuzzle(newX, newY);

                            if (newPuzzle == null || curPuzzle.color != newPuzzle.color) break;

                            //�湮���� ���� �����̶�� ť�� �־���
                            if (visitPuzzles.Add(newPuzzle))
                            {
                                searchQueue.Enqueue(newPuzzle);
                            }

                            if (!itemPuzzles.Contains(newPuzzle) || !destroyPuzzles.Contains(newPuzzle))
                                findPuzzles[k].Add(newPuzzle);


                            newX += dx[k];
                            newY += dy[k];

                        } while (true);
                    }

                    //���⼭���� ������ �������ǿ� �������� üũ

                    if ((findPuzzles[0].Count + findPuzzles[1].Count + findPuzzles[2].Count + findPuzzles[3].Count) < 2) continue;

                    //���κ��� �Ǵ��� üũ(5��)
                    if (rewardType != PuzzleType.Rainbow &&
                        ((findPuzzles[0].Count + findPuzzles[2].Count >= 4) || (findPuzzles[1].Count + findPuzzles[3].Count >= 4)))
                    {
                        itemPuzzles.Clear();
                        itemPuzzles.Add(curPuzzle);
                        rewardType = PuzzleType.Rainbow;

                        if (findPuzzles[0].Count + findPuzzles[2].Count >= 4)
                        {
                            itemPuzzles.AddRange(findPuzzles[0]);
                            itemPuzzles.AddRange(findPuzzles[2]);
                        }

                        if (findPuzzles[1].Count + findPuzzles[3].Count >= 4)
                        {
                            itemPuzzles.AddRange(findPuzzles[1]);
                            itemPuzzles.AddRange(findPuzzles[3]);
                        }
                    }
                    // L�� �Ǵ��� üũ(��ź)
                    else if ((rewardType == PuzzleType.Empty || (int)rewardType < 4)
                        && ((findPuzzles[0].Count >= 2 || findPuzzles[2].Count >= 2) && (findPuzzles[1].Count >= 2 || findPuzzles[3].Count >= 2))) //L��
                    {

                        itemPuzzles.Clear();
                        rewardType = PuzzleType.Bomb;
                        itemPuzzles.Add(curPuzzle);

                        for (int bombindex = 0; bombindex < 4; bombindex++)
                        {
                            if (findPuzzles[bombindex].Count >= 2)
                            {
                                itemPuzzles.AddRange(findPuzzles[bombindex]);
                            }
                        }


                    }
                    //4�� �Ǵ��� üũ
                    else if ((rewardType == PuzzleType.Empty || (int)rewardType < 2)
                        && ((findPuzzles[0].Count + findPuzzles[2].Count >= 3) || (findPuzzles[1].Count + findPuzzles[3].Count >= 3)))
                    {

                        itemPuzzles.Clear();
                        itemPuzzles.Add(curPuzzle);

                        if ((findPuzzles[0].Count + findPuzzles[2].Count >= 3))
                        {
                            rewardType = PuzzleType.Vertical;
                            itemPuzzles.AddRange(findPuzzles[0]);
                            itemPuzzles.AddRange(findPuzzles[2]);
                        }
                        else if (findPuzzles[1].Count + findPuzzles[3].Count >= 3)
                        {
                            rewardType = PuzzleType.Horizontal;
                            itemPuzzles.AddRange(findPuzzles[1]);
                            itemPuzzles.AddRange(findPuzzles[3]);

                        }

                    }
                    //�� �ȵǸ� ������ �ϴ��� üũ
                    else
                    {

                        if (findPuzzles[0].Count + findPuzzles[2].Count >= 2)
                        {
                            if (!destroyPuzzles.Contains(curPuzzle))
                                destroyPuzzles.Add(curPuzzle);

                            destroyPuzzles.AddRange(findPuzzles[0]);
                            destroyPuzzles.AddRange(findPuzzles[2]);
                        }

                        if (findPuzzles[1].Count + findPuzzles[3].Count >= 2)
                        {
                            if (!destroyPuzzles.Contains(curPuzzle))
                                destroyPuzzles.Add(curPuzzle);

                            destroyPuzzles.AddRange(findPuzzles[1]);
                            destroyPuzzles.AddRange(findPuzzles[3]);
                        }
                    }


                }
                //bfs��


                if (destroyPuzzles.Count >= 1)
                {

                    isDestroyBlock = true;

                    if (rewardType != PuzzleType.Empty)
                    {
                        Puzzle itemPuzzle = Maker.MakeNewPuzzle(itemPuzzles[0].X, itemPuzzles[0].Y, rewardType, itemPuzzles[0].color);

                        Action<bool, UnityEngine.Events.UnityAction> action = null;

                        foreach (Puzzle puzzle in itemPuzzles)
                        {
                            if (puzzle != null && puzzle != itemPuzzle)
                            {
                                if (puzzle.X != itemPuzzle.X || puzzle.Y != itemPuzzle.Y)
                                {
                                    SetPuzzle(puzzle.X, puzzle.Y, null);
                                }

                                if (puzzle.type == PuzzleType.Normal)
                                    puzzle.Move(itemPuzzle.X, itemPuzzle.Y, 0.1f, () => puzzle.Pop(true));
                                else
                                    action += puzzle.Pop;
                            }
                        }

                        action?.Invoke(false, null);

                        PopRoutine(destroyPuzzles);

                        SetPuzzle(itemPuzzle.X, itemPuzzle.Y, itemPuzzle);
                        itemPuzzles.Clear();
                    }
                    else
                    {
                        PopRoutine(destroyPuzzles);
                    }

                    destroyPuzzles.Clear();

                }

            }

            yield return null;
        }

        //��ġ�� �ð���ŭ ��ٷ������.

        if (isDestroyBlock)
        {
            //�������� �Ҹ�
            //soundManager.PlayEffect(6);
            yield return new WaitForSeconds(0.1f);
        }

        callBack?.Invoke(isDestroyBlock);

    }

    // �߰��� ����� ��Ʈ���� ��ƾ
    public void PopRoutine(List<Puzzle> destroyPuzzles)
    {

        soundManager.PlayPopEffect(PuzzleType.Normal);

        foreach (Puzzle puzzle in destroyPuzzles)
        {
            if (puzzle != null)
            {
                if (puzzle.type == PuzzleType.Normal)
                {
                    for (int obstacleIndex = 0; obstacleIndex < 4; obstacleIndex++)
                    {
                        if (CheckIsObstacle(puzzle.X + dx[obstacleIndex], puzzle.Y + dy[obstacleIndex]))
                        {
                            GetPuzzle(puzzle.X + dx[obstacleIndex], puzzle.Y + dy[obstacleIndex]).Pop();
                        }
                    }
                }

                puzzle.Pop();
            }
        }

    }



    #endregion


    #region ���� ����

    //���� ���� 
    public void SwapPuzzle(Puzzle swapPuzzle)
    {
        //�����ʿ��� ȣ���ϴ°���. �� swapPuzzle�� ������ ������
        isClick = false;

        int newX = selectPuzzle.X;
        int newY = selectPuzzle.Y;

        if ((newX == swapPuzzle.X && (newY == swapPuzzle.Y - 1 || newY == swapPuzzle.Y + 1))
            || (newY == swapPuzzle.Y && (newX == swapPuzzle.X - 1 || newX == swapPuzzle.X + 1)))
        {
            CheckHintTime(false);
            StartCoroutine(SwapPuzzleCor(newX, newY, swapPuzzle));
        }
    }


    IEnumerator SwapPuzzleCor(int newX, int newY, Puzzle swapPuzzle)
    {
        //�����ʿ��� ȣ���ϴ°���. �� swapPuzzle�� ������ ������
        isProcess = true;
        soundManager.PlayEffect(0);
        selectPuzzle.SetAndMove(swapPuzzle.X, swapPuzzle.Y);
        swapPuzzle.SetAndMove(newX, newY);


        if (selectPuzzle.CheckItemCombination(swapPuzzle))
        {
             yield return new WaitForSeconds(0.1f);
             Fill();
        }
        else
        {
            StartCoroutine(CheckPuzzleCo((isNeedFill) =>
            {
                //�ݹ�
                if (isNeedFill)
                {
                    Fill();
                }
                else
                {
                    CheckHintTime(true);
                    soundManager.PlayEffect(0);
                    swapPuzzle.SetAndMove(selectPuzzle.X, selectPuzzle.Y);
                    selectPuzzle.SetAndMove(newX, newY);
                    isProcess = false;
                    selectPuzzle = null;
                }

            }));

        }

        yield return null;

    }

    public bool isSpecialMatch(Puzzle p, Puzzle p2)
    {
        if (p.type == PuzzleType.Normal || p.type == PuzzleType.Horizontal || p.type == PuzzleType.Vertical) return false;
        if (p2.type == PuzzleType.Normal || p2.type == PuzzleType.Horizontal || p2.type == PuzzleType.Vertical) return false;

        if (p.type == PuzzleType.Rainbow)
        {
            switch (p2.type)
            {
                case PuzzleType.Bomb:
                    break;
                case PuzzleType.Rainbow:
                    break;
            }
        }
        else if (p.type == PuzzleType.Bomb)
        {
            switch (p2.type)
            {
                case PuzzleType.Bomb:
                    break;
                case PuzzleType.Rainbow:
                    break;
            }
        }

        return true;
    }
    #endregion


    #region ��ġ������ ���� Ž�� ��ɵ�

    private Coroutine coHintTimeCheck;
    private List<Puzzle> hintPuzzles = new List<Puzzle>();

    //�����ð����� �Է¾����� ��Ʈ�ִ� �ð� üũ
    public void CheckHintTime(bool isCheck)
    {
        if (coHintTimeCheck != null)
        {
            StopCoroutine(coHintTimeCheck);
        }

        FlickerPuzzles(false);

        if (isCheck)
        {
            coHintTimeCheck = StartCoroutine(CheckHintTimeCoroutine());
        }
    }

    IEnumerator CheckHintTimeCoroutine()
    {
        float time = 0.0f;

        while (time < hintTime)
        {
            time += Time.deltaTime;

            yield return null;
        }

        yield return FindMatchablePuzzle();
    }

    //ã�� ��Ʈ ����� ����/����
    public void FlickerPuzzles(bool isFlicker)
    {
        foreach (Puzzle p in hintPuzzles)
        {
            if (p != null)
            {
                p.Flicker(isFlicker);
            }

        }

        if (isFlicker == false)
        {
            hintPuzzles.Clear();
        }
    }

    //���� �÷� ���� ã��
    public Puzzle FindSameColor(Puzzle puzzle, int index, PuzzleColor color, Dir dir)
    {
        Puzzle findPuzzle = null;

        switch (dir)
        {
            case Dir.Up:
                findPuzzle = GetPuzzle(puzzle.X, puzzle.Y - index);
                break;

            case Dir.Right:
                findPuzzle = GetPuzzle(puzzle.X + index, puzzle.Y);
                break;

            case Dir.Down:
                findPuzzle = GetPuzzle(puzzle.X, puzzle.Y + index);
                break;

            case Dir.Left:
                findPuzzle = GetPuzzle(puzzle.X - index, puzzle.Y);
                break;
        }

        if (findPuzzle == null || findPuzzle.type == PuzzleType.Obstacle || findPuzzle.color != color) return null;

        return findPuzzle;

    }




    //��ġ�Ҽ��ִ� ���� ã��
    IEnumerator FindMatchablePuzzle()
    {
        // 5�� -> L�� -> 4�� -> 3�� ��. ������ �� �Ѽ��� ����.

        try
        {
            if (FindMatch(5) || FindMatchL() || FindMatch(4) || FindMatch(3))
            {
                FlickerPuzzles(true);
                yield break;
            }

            for (int j = 0; j < Y; j++)
            {
                for (int i = 0; i < X; i++)
                {
                    if (puzzles[i, j] != null)
                    {
                        puzzles[i, j].Pop();
                    }
                }
            }

            Fill();
        }
        catch
        {
            yield break;
        }

        yield return null;
        
    }


    //5,4,3 ��� Ž��
    public bool FindMatch(int MatchCount)
    {

        for (int j = 0; j < Y; j++)
        {
            for (int i = 0; i < X; i++)
            {
                List<Puzzle> findPuzzle = new List<Puzzle>();
                Puzzle curPuzzle = puzzles[i, j];

                if (curPuzzle == null || curPuzzle.color == PuzzleColor.None) continue;

                if (!IsOutOfIndex(i + MatchCount - 1, j))
                {
                    for (int k = 1; k < MatchCount; k++)
                    {
                        findPuzzle.Add(GetPuzzle(i + k, j));
                    }

                    if (findPuzzle.FindAll(x => x.color == curPuzzle.color).Count == MatchCount - 2)
                    {
                        Puzzle anotherPuzzle = findPuzzle.Find(x => x.color != curPuzzle.color);
                        findPuzzle.Remove(anotherPuzzle);

                        for (int h = 0; h < 2; h++)
                        {
                            if (FindSameColor(anotherPuzzle, 1, curPuzzle.color, h == 0 ? Dir.Up : Dir.Down) != null)
                            {
                                hintPuzzles.Add(curPuzzle);
                                hintPuzzles.Add(FindSameColor(anotherPuzzle, 1, curPuzzle.color, h == 0 ? Dir.Up : Dir.Down));
                                hintPuzzles.AddRange(findPuzzle);
                                return true;
                            }
                        }

                        if (MatchCount == 3 && anotherPuzzle.X == curPuzzle.X + 2)
                        {
                            if (FindSameColor(anotherPuzzle, 1, curPuzzle.color, Dir.Right) != null)
                            {
                                hintPuzzles.Add(curPuzzle);
                                hintPuzzles.Add(FindSameColor(anotherPuzzle, 1, curPuzzle.color, Dir.Right));
                                hintPuzzles.AddRange(findPuzzle);
                                return true;
                            }

                        }
                    }
                }

                findPuzzle.Clear();

                if (!IsOutOfIndex(i, j + MatchCount - 1))
                {
                    for (int k = 1; k < MatchCount; k++)
                    {
                        findPuzzle.Add(GetPuzzle(i, j + k));
                    }

                    if (findPuzzle.FindAll(x => x.color == curPuzzle.color).Count == MatchCount - 2)
                    {
                        Puzzle anotherPuzzle = findPuzzle.Find(x => x.color != curPuzzle.color);
                        findPuzzle.Remove(anotherPuzzle);


                        for (int h = 0; h < 2; h++)
                        {
                            if (FindSameColor(anotherPuzzle, 1, curPuzzle.color, h == 0 ? Dir.Right : Dir.Left) != null)
                            {
                                hintPuzzles.Add(curPuzzle);
                                hintPuzzles.Add(FindSameColor(anotherPuzzle, 1, curPuzzle.color, h == 0 ? Dir.Right : Dir.Left));
                                hintPuzzles.AddRange(findPuzzle);
                                return true;
                            }
                        }

                        if (MatchCount == 3 && anotherPuzzle.Y == curPuzzle.Y + 2)
                        {
                            if (FindSameColor(anotherPuzzle, 1, curPuzzle.color, Dir.Down) != null)
                            {
                                hintPuzzles.Add(curPuzzle);
                                hintPuzzles.Add(FindSameColor(anotherPuzzle, 1, curPuzzle.color, Dir.Down));
                                hintPuzzles.AddRange(findPuzzle);
                                return true;
                            }

                        }
                    }
                }
            }
        }


        return false;
    }


    //L��� Ž��
    public bool FindMatchL()
    {
        List<Puzzle> findPuzzle = new List<Puzzle>();


        for (int j = 0; j < Y; j++)
        {
            for (int i = 0; i < X; i++)
            {
                Puzzle curPuzzle = puzzles[i, j];

                if (curPuzzle == null || curPuzzle.color == PuzzleColor.None) continue;
                if (IsOutOfIndex(i, j + 3)) continue;

                (bool isTrueShape, Puzzle[] puzzleList) result = isLShape(i, j);

                if (result.isTrueShape) // ���� �ǾƷ� üũ
                {
                    findPuzzle.AddRange(result.puzzleList);

                    hintPuzzles.AddRange(findPuzzle);
                    return true;
                }

                result = isReverseLShape(i, j);

                if (result.isTrueShape) // ���� �ǾƷ� üũ
                {
                    findPuzzle.AddRange(result.puzzleList);
                    hintPuzzles.AddRange(findPuzzle);
                    return true;
                }



            }
        }

        //LshapeŽ��

        (bool, Puzzle[]) isLShape(int x, int y)
        {
            if (IsOutOfIndex(x + 2, y) || IsOutOfIndex(x, y + 2)) return (false, null);

            Puzzle curpuzzle = puzzles[x, y];

            if (puzzles[x, y + 1].color == curpuzzle.color && puzzles[x, y + 2].color != curpuzzle.color
                && puzzles[x + 1, y + 2].color == curpuzzle.color && puzzles[x + 2, y + 2].color == curpuzzle.color)
            {

                if (!IsOutOfIndex(x - 1, y + 2) && puzzles[x - 1, y + 2].color == curpuzzle.color)
                {
                    return (true, new Puzzle[] { curpuzzle, puzzles[x, y + 1], puzzles[x + 1, y + 2], puzzles[x + 2, y + 2], puzzles[x - 1, y + 2] });
                }
                else if (!IsOutOfIndex(x, y + 3) && puzzles[x, y + 3].color == curpuzzle.color)
                {
                    return (true, new Puzzle[] { curpuzzle, puzzles[x, y + 1], puzzles[x + 1, y + 2], puzzles[x + 2, y + 2], puzzles[x, y + 3] });
                }

            }

            return (false, null);

        }

        //L������ ��� Ž��
        (bool, Puzzle[]) isReverseLShape(int x, int y)
        {
            if (IsOutOfIndex(x - 2, y) || IsOutOfIndex(x, y + 2)) return (false, null);

            Puzzle curpuzzle = puzzles[x, y];

            if (puzzles[x, y + 1].color == curpuzzle.color && puzzles[x, y + 2].color != curpuzzle.color
                && puzzles[x - 1, y + 2].color == curpuzzle.color && puzzles[x - 2, y + 2].color == curpuzzle.color)
            {
                if (!IsOutOfIndex(x + 1, y + 2) && puzzles[x + 1, y + 2].color == curpuzzle.color)
                {
                    return (true, new Puzzle[] { curpuzzle, puzzles[x, y + 1], puzzles[x - 1, y + 2], puzzles[x - 2, y + 2], puzzles[x + 1, y + 2] });
                }
                else if (!IsOutOfIndex(x, y + 3) && puzzles[x, y + 3].color == curpuzzle.color)
                {
                    return (true, new Puzzle[] { curpuzzle, puzzles[x, y + 1], puzzles[x - 1, y + 2], puzzles[x - 2, y + 2], puzzles[x, y + 3] });
                }
            }

            return (false, null);
        }

        return false;
    }


    #endregion

    #region üũ, ȿ�� �Լ�

    public void ReStart()
    {
        SceneManager.LoadSceneAsync(0);
    }


    #endregion

}

