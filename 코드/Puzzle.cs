using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public enum PuzzleType
{
    Normal, Obstacle, Horizontal, Vertical, Bomb, Rainbow, Empty
}

public enum PuzzleColor
{
    Blue, Green, Red, Purple, Bear, Bee, BirdBlue, Cow, Dog, Frog, Mouse, Panda, None
}

public class Puzzle : MonoBehaviour
{

    [Header("[���� ������Ƽ]")]
    //��ǥ
    private int x;
    private int y;

    public int X => x;
    public int Y => y;

    public PuzzleType type;
    public PuzzleColor color;

    private Image myImage;

    [Header("[������Ʈ]")]
    public RectTransform myRect;
    [SerializeField]
    protected Animator animator;
    protected PuzzleManager manager;

    private MoveablePuzzle moveable;
    private Coroutine coFlicker = null;

    public bool isRainbowType => this.type == PuzzleType.Rainbow;
    public bool isBombType => this.type == PuzzleType.Bomb;

    private void Awake()
    {
        myImage = GetComponentInChildren<Image>();
        moveable = this.GetComponent<MoveablePuzzle>();
    }

    //�����ϼ� �ִ� �������� Ȯ��
    public bool IsMoveable()
    {
        return moveable != null;
    }

    //���� ��ǥ ����
    public void SetCoordinate(int newX, int newY)
    {
        this.x = newX;
        this.y = newY;
    }

    //������ġ ����
    public void SetPos(Vector2 pos)
    {
        myRect.anchoredPosition = pos;
    }

    //x,y��ǥ�� �̵� -> �ݹ� �Լ�
    public void Move(int x, int y, float fillTime, UnityAction callback)
    {
        moveable.Move(x, y, fillTime, callback);
    }


    //���� ����x,y ��ġ�� �����̱�
    public void Move(float fillTime)
    {
        moveable.Move(fillTime);
    }

    //���� x,y��ġ�� �����ϰ� �����̱�
    public void SetAndMove(int newX, int newY)
    {
        SetCoordinate(newX, newY);
        Move(0.1f);
    }

    //������ ������ ����
    public void SetColor(PuzzleColor color)
    {
        switch (type)
        {
            case PuzzleType.Normal:
                myImage.sprite = manager.Maker.puzzleSprs[(int)color];
                break;
            case PuzzleType.Horizontal:
                myImage.sprite = manager.Maker.horizontalSprs[(int)color];
                break;
            case PuzzleType.Vertical:
                myImage.sprite = manager.Maker.verticalSprs[(int)color];
                break;
        }

        this.color = color;
    }

    //�� �������� ����
    public void SetColor()
    {
        int rand = Random.Range(0, manager.Maker.puzzleSprs.Length);

        myImage.sprite = manager.Maker.puzzleSprs[rand];

        color = (PuzzleColor)rand;
    }

    //���� �ʱ�ȭ
    public void Init(int x, int y, PuzzleManager manager, PuzzleColor newColor = PuzzleColor.None)
    {
        this.manager = manager;

        if (IsMoveable())
        {
            moveable.SetManager(manager);
        }

        if (this.color != PuzzleColor.None)
        {
            if (newColor == PuzzleColor.None)
            {
                SetColor();
            }
            else
            {
                SetColor(newColor);
            }
        }

        SetCoordinate(x, y);
        SetPos(manager.Maker.GetPos(x, y));
    }



    //���� ��Ʈ����
    public virtual void Pop(bool isIgnoreEffect = false, UnityAction callBack = null)
    {
        if (manager.GetPuzzle(X, Y) == this)
        {
            manager.SetPuzzle(X, Y, null);
        }

        if (isIgnoreEffect)
        {
            Destroy(this.gameObject);
        }
        else
        {
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.SetTrigger("DefaultTrigger"); // ���� Trigger ���
            StartCoroutine(DestroyAfterAnimation()); // �ִϸ��̼� �� ����
        }

        manager.Point += 100;
        callBack?.Invoke();
    }
    private IEnumerator DestroyAfterAnimation()
    {
        // ���� �ִϸ��̼� ���� ���� ��������
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // �ִϸ��̼��� ���� ���� ���°� �ƴ϶�� �⺻ ��� �ð� ���� (0.5��)
        float animationLength = stateInfo.IsName("ShrinkAndDestroy") ? stateInfo.length : 0.5f;

        // �ִϸ��̼� �ð���ŭ ���
        yield return new WaitForSeconds(animationLength);

        Debug.Log($"Destroying block: {gameObject.name}"); // ������ �α�
        Destroy(this.gameObject); // ���� ������Ʈ ����
    }


    //�ִϸ��̼��� ���� �� ó��
    public void EndDestroyAnimation()
    {
        if (this.type == PuzzleType.Normal)
        {
            manager.Maker.PuzzlePool.ReturnToPool(this);
            animator.cullingMode = AnimatorCullingMode.CullCompletely;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    //������ ���� Ȯ��
    public virtual bool CheckItemCombination(Puzzle swapPuzzle)
    {

        if(swapPuzzle.isRainbowType || swapPuzzle.isBombType)
        {
            swapPuzzle.Pop();
            return true;
        }

        return false;
    }



    //���� ��Ʈ�� �����Ÿ���
    public void Flicker(bool isStart = true, float flickerSpeed = 1.0f)
    {
        if (isStart)
        {
            if (coFlicker != null)
            {
                StopCoroutine(coFlicker);
            }

            coFlicker = StartCoroutine(FlickerCoroutine(flickerSpeed));
        }
        else
        {
            if (coFlicker != null)
            {
                StopCoroutine(coFlicker);
            }

            Color color = myImage.color;

            color.a = 1.0f;
            myImage.color = color;

        }
    }

    IEnumerator FlickerCoroutine(float flickerSpeed)
    {
        while (true)
        {
            while (myImage.color.a > 0.25f)
            {
                Color color = myImage.color;

                color.a -= Time.deltaTime * flickerSpeed;
                myImage.color = color;

                yield return null;
            }

            while (myImage.color.a < 1.0f)
            {

                Color color = myImage.color;

                color.a += Time.deltaTime * flickerSpeed;
                myImage.color = color;

                yield return null;
            }

            yield return null;
        }


    }


    private void OnDestroy()
    {
        manager.Point += 100;
    }



}
