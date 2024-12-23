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

    [Header("[퍼즐 프로퍼티]")]
    //좌표
    private int x;
    private int y;

    public int X => x;
    public int Y => y;

    public PuzzleType type;
    public PuzzleColor color;

    private Image myImage;

    [Header("[컴포넌트]")]
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

    //움직일수 있는 퍼즐인지 확인
    public bool IsMoveable()
    {
        return moveable != null;
    }

    //퍼즐 좌표 세팅
    public void SetCoordinate(int newX, int newY)
    {
        this.x = newX;
        this.y = newY;
    }

    //퍼즐위치 세팅
    public void SetPos(Vector2 pos)
    {
        myRect.anchoredPosition = pos;
    }

    //x,y좌표로 이동 -> 콜백 함수
    public void Move(int x, int y, float fillTime, UnityAction callback)
    {
        moveable.Move(x, y, fillTime, callback);
    }


    //퍼즐 현재x,y 위치로 움직이기
    public void Move(float fillTime)
    {
        moveable.Move(fillTime);
    }

    //퍼즐 x,y위치로 세팅하고 움직이기
    public void SetAndMove(int newX, int newY)
    {
        SetCoordinate(newX, newY);
        Move(0.1f);
    }

    //정해진 색으로 변경
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

    //색 랜덤으로 설정
    public void SetColor()
    {
        int rand = Random.Range(0, manager.Maker.puzzleSprs.Length);

        myImage.sprite = manager.Maker.puzzleSprs[rand];

        color = (PuzzleColor)rand;
    }

    //퍼즐 초기화
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



    //퍼즐 터트릴때
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
            animator.SetTrigger("DefaultTrigger"); // 단일 Trigger 사용
            StartCoroutine(DestroyAfterAnimation()); // 애니메이션 후 제거
        }

        manager.Point += 100;
        callBack?.Invoke();
    }
    private IEnumerator DestroyAfterAnimation()
    {
        // 현재 애니메이션 상태 정보 가져오기
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // 애니메이션이 실행 중인 상태가 아니라면 기본 대기 시간 설정 (0.5초)
        float animationLength = stateInfo.IsName("ShrinkAndDestroy") ? stateInfo.length : 0.5f;

        // 애니메이션 시간만큼 대기
        yield return new WaitForSeconds(animationLength);

        Debug.Log($"Destroying block: {gameObject.name}"); // 디버깅용 로그
        Destroy(this.gameObject); // 퍼즐 오브젝트 제거
    }


    //애니메이션이 끝난 후 처리
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

    //아이템 조합 확인
    public virtual bool CheckItemCombination(Puzzle swapPuzzle)
    {

        if(swapPuzzle.isRainbowType || swapPuzzle.isBombType)
        {
            swapPuzzle.Pop();
            return true;
        }

        return false;
    }



    //조합 힌트시 깜빡거리기
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
