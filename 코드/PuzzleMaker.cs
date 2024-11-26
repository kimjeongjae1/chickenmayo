using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleMaker : MonoBehaviour
{
    //���� �׸�(�÷�)
    [Header("[���� �׸�]")]
    [SerializeField]
    private PuzzleColor gameTheme;

    [Header("[���� �Ŵ���]")]
    [SerializeField]
    private PuzzleManager manager;

    [Header("[���� ����]")]
    [SerializeField]
    private TextAsset puzzleData;

    //���� ���μ��� ũ��
    [SerializeField]
    private int x;
    [SerializeField]
    private int y;

    public int X => x;
    public int Y => y;

    [SerializeField]
    private int puzzleSpacing; //���� ����

    [SerializeField]
    private RectTransform puzzleBackFrame;
    [SerializeField]
    private RectTransform puzzleFrame;

    [Header("[������]")]
    [SerializeField]
    private GameObject puzzleBackPrefab;
    [SerializeField]
    private GameObject[] puzzlePrefab;

    [Header("[���� ��������Ʈ]")]
    public Sprite[] puzzleSprs;
    public Sprite[] verticalSprs;
    public Sprite[] horizontalSprs;
    public Sprite[] puzzleBackSprs;
    public Sprite[] frameSprs;

    [Header("[�ʱ� ���� ����]")]
    [SerializeField]
    private InitPuzzle[] initPuzzles;

    private Vector2 puzzleSize;

    private ObjectPool<Puzzle> puzzlePool;
    public ObjectPool<Puzzle> PuzzlePool => puzzlePool;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        puzzleSize = puzzleBackPrefab.GetComponent<RectTransform>().sizeDelta;

        //������ ����
        if(puzzleData != null )
        {
            string[] data = puzzleData.text.Split("\n");

            gameTheme = (PuzzleColor)int.Parse(data[0]);
            this.x = int.Parse(data[1].Split("/")[0]);
            this.y = int.Parse(data[1].Split("/")[1]);

            manager.InitPuzzles(x, y);
            puzzlePool = new ObjectPool<Puzzle>(x * y, MakePoolPuzzle, PoolInitAction, PoolReturnAction);

            for (int j = 0; j < y; j++)
            {
                string[] puzzleData = data[j+2].Split("/");

                for (int i = 0; i < x; i++)
                {
                    manager.SetPuzzle(i, j, MakeNewPuzzle(i, j, (PuzzleType)int.Parse(puzzleData[i][0].ToString()), (PuzzleColor)int.Parse(puzzleData[i][1].ToString())));
                    
                }
            }
        }
        else
        {
            manager.InitPuzzles(x, y);
            puzzlePool = new ObjectPool<Puzzle>(x * y, MakePoolPuzzle, PoolInitAction, PoolReturnAction);
        }


        //�ܰ��� ����
        puzzleBackFrame.sizeDelta = puzzleFrame.sizeDelta = new Vector2((puzzleSize.x * x) + puzzleSpacing, (puzzleSize.y * y) + puzzleSpacing);
        puzzleBackFrame.GetComponent<Image>().sprite = frameSprs[(int)gameTheme];

     


        //���� ��� ����
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                Instantiate(puzzleBackPrefab, puzzleBackFrame.transform).GetComponent<PuzzleBackGround>().Init(GetPos(i, j), puzzleBackSprs[(int)gameTheme]);
            }
        }


        
        //�ʱ� ���� ������ �ִٸ� ����
        for (int i = 0; i < initPuzzles.Length; i++)
        {
            Puzzle newInitPuzzle = MakeNewPuzzle(initPuzzles[i].coordinate.x, initPuzzles[i].coordinate.y,initPuzzles[i].type);
            manager.SetPuzzle(initPuzzles[i].coordinate.x, initPuzzles[i].coordinate.y,newInitPuzzle);
        }
        
        manager.Fill();
    }

    //���ο� ���� ����
    public Puzzle MakeNewPuzzle(int x, int y, PuzzleType type, PuzzleColor color = PuzzleColor.None)
    {
        Puzzle newPuzzle = null;

        if(type == PuzzleType.Normal)
        {
            newPuzzle = puzzlePool.BorrowFromPool();
        }
        else
        {
           newPuzzle = Instantiate(puzzlePrefab[(int)type], puzzleFrame.transform).GetComponent<Puzzle>();
        }

        newPuzzle.Init(x, y, this.manager, color);

        return newPuzzle;
    }

    //��ǥ���� �´� ���� ��ġ ����
    public Vector2 GetPos(int x, int y)
    {
        return new Vector2(x * puzzleSize.x + puzzleSpacing / 2, -y * puzzleSize.y - puzzleSpacing / 2);
    }



    #region ������Ʈ Ǯ


    //Ǯ ���� ���� 
    public Puzzle MakePoolPuzzle()
    {
        Puzzle newPuzzle = Instantiate(puzzlePrefab[0], puzzleFrame.transform).GetComponent<Puzzle>();
        newPuzzle.gameObject.SetActive(false);

        return newPuzzle;
    }

    //Ǯ �����ֱ� �� ������ �Լ�
    public void PoolInitAction(Puzzle puzzle)
    {
        puzzle.gameObject.SetActive(true);
    }

    //Ǯ ���������� ������ �Լ�
    public void PoolReturnAction(Puzzle puzzle)
    {
        puzzle.gameObject.SetActive(false);
    }



    #endregion

}
