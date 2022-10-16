using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class CardUI : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    public enum CardStates { Default, Return, SkipRight, SkipLeft, Dragging, Hided, Skipped }

    public float rotateSpeed;

    public bool isDragging;
    public RectTransform rect;

    private Vector3 presPosition;
    public Animator anim;

    public Image mainImage;
    public Image gradient;
    public TMP_Text leftText;
    public TMP_Text rightText;

    public bool cardActive;
    public float returnSmoothTime = 0.3F;
    public float skipSmoothTime = 0.3F;
    private Vector2 velocity = Vector2.zero;
    private Vector3 velocity3 = Vector3.zero;
    public CardStates state = CardStates.Hided;

    private bool isPlayStartSound;

    public Vector3 cardStartCenter;
    public Vector3 cardShowCenter;
    public float dist = 0;

    public CardInfo cardInfo;

    private void UpdateState()
    {
        //Vector2 absScreenPosition = new Vector2(rect.anchoredPosition.x + rect.rect.width, 0);
        //print(absScreenPosition);
        dist = Vector2.Distance(rect.anchoredPosition, cardStartCenter);
        if (Vector2.Distance(rect.anchoredPosition, cardStartCenter) < 80 && state == CardStates.Hided && !isPlayStartSound)
        {
            isPlayStartSound = true;
            //SoundManagerController.inst.PlaySound("showCard");
        }
        if (state == CardStates.Hided) return;
        Vector3 delta = rect.anchoredPosition3D - cardShowCenter;
        if (isDragging)
        {
            state = CardStates.Dragging;
        }
        
        else if (Mathf.Abs(delta.x) < ControllerUI.rect.x / 4)
        {
            if (Vector3.Distance(rect.anchoredPosition3D, cardShowCenter) < 0.1f)
            {
                rect.anchoredPosition3D = cardShowCenter;
                state = CardStates.Default;
                return;
            }
            state = CardStates.Return;
        }
        else
        {
            if (delta.x < 0)
            {
                state = CardStates.SkipLeft;
            }
            else
            {
                state = CardStates.SkipRight;
            }
        }
    }

    public void OnShowAnimEnded()
    {
        anim.enabled = false;
        state = CardStates.Default;
    }

    public void ShowCard()
    {
        //rect.anchoredPosition = Vector2.zero;
        rect.anchoredPosition3D = cardStartCenter;
        cardActive = true;
        anim.enabled = true;
        anim.Rebind();
    }

    private void LateUpdate()
    {
        UpdateState();
        Vector2 mousePos = (presPosition - Input.mousePosition) / ControllerUI.scaleMultiplyer;
        switch (state)
        {
            case CardStates.Dragging:
                rect.anchoredPosition -= mousePos;
                break;
            case CardStates.Return:
                Vector3 newPosition = Vector2.SmoothDamp(rect.anchoredPosition3D, cardShowCenter, ref velocity, returnSmoothTime);
                rect.anchoredPosition3D = new Vector3(newPosition.x, newPosition.y, cardShowCenter.z);
                break;
            case CardStates.Hided:
                rect.anchoredPosition3D = Vector3.SmoothDamp(rect.anchoredPosition3D, cardStartCenter, ref velocity3, returnSmoothTime);
                break;
            //case CardStates.Return:
            //    rect.anchoredPosition = Vector2.SmoothDamp(rect.anchoredPosition, Vector2.zero, ref velocity, returnSmoothTime);
            //    break;
            case CardStates.SkipRight:
                rect.anchoredPosition = Vector2.SmoothDamp(rect.anchoredPosition, new Vector2(Screen.width / 2 + rect.rect.width, rect.anchoredPosition.y), ref velocity, skipSmoothTime);
                break;
            case CardStates.SkipLeft:
                rect.anchoredPosition = Vector2.SmoothDamp(rect.anchoredPosition, new Vector2(-Screen.width / 2 - rect.rect.width, rect.anchoredPosition.y), ref velocity, skipSmoothTime);
                break;
        }
        if (state != CardStates.Hided)
        {
            rect.localRotation = Quaternion.Euler(rect.localEulerAngles.x, rect.localEulerAngles.y, -rect.anchoredPosition.x / ControllerUI.rect.x * rotateSpeed);
            float alpha = Mathf.Clamp(Mathf.Abs(rect.anchoredPosition.x) / (ControllerUI.rect.x / 4), 0, 0.75f);
            gradient.color = new Color(gradient.color.r, gradient.color.g, gradient.color.b, alpha);
            if (rect.anchoredPosition.x < 0)
            {
                leftText.alpha = 0;
                rightText.alpha = EasingFunction.EaseInOutQuint(0, 1, alpha);
            }
            else
            {
                leftText.alpha = EasingFunction.EaseInOutQuint(0, 1, alpha);
                rightText.alpha = 0;
            }
        }
        if ((state == CardStates.SkipLeft || state == CardStates.SkipRight) && Mathf.Abs(rect.anchoredPosition.x) >= Screen.width / 2 + rect.rect.width / 2)
        {
            if(state == CardStates.SkipLeft)
            {
                cardInfo.LeftChoose();
            }

            if (state == CardStates.SkipRight)
            {
                cardInfo.RightChoose();
            }
            SoundManagerController.inst.PlaySound("cardSkip");
            ModelController.ChangeMonths(1);
            ControllerUI.inst.cardManagerUI.CreateCard();
            Destroy(gameObject);
        }
        presPosition = Input.mousePosition;
    }

    private void OnMouseDown()
    {
        presPosition = Input.mousePosition;
        isDragging = true;
    }

    private void OnMouseUp()
    {
        isDragging = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        presPosition = Input.mousePosition;
        isDragging = true;
    }
}
