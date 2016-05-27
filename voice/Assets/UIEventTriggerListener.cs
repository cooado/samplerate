using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using UnityEngine;
//using UnityEngine.UI;
using UnityEngine.EventSystems;
//using Water;

namespace Water
{
    /// <summary>
    ///  以添加到单元的形式添加进去，释放的时候走unity 自己的释放过程，不用为每个单独进行释放。
    ///  该类会阻隔事件，不适用于位于的scrollrect的单元 因为其需要侦听滚动事件  当item需要使用时，建议用IPointerClickHandler 实现
    /// </summary>
    public class UIEventTriggerListener : MonoBehaviour, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IUpdateSelectedHandler,
        ISelectHandler, IDeselectHandler, IMoveHandler, ISubmitHandler, ICancelHandler, IDragHandler
    //, IBeginDragHandler,   IInitializePotentialDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IScrollHandler
    {
        /// <summary>
        /// 用于与拖拽同时生效时区分使用的点击事件
        /// </summary>
        public Action<GameObject> onTap;
        public Action<GameObject> onClick;
        public Action<GameObject> onDown;
        public Action<GameObject> onEnter;
        public Action<GameObject> onExit;
        public Action<GameObject> onUp;
        public Action<GameObject> onSelect;
        public Action<GameObject> onUpdateSelect;
        public Action<GameObject, PointerEventData> onScroll;
        public Action<GameObject, PointerEventData> onEndDrag;
        public Action<GameObject, PointerEventData> onBeginDrag;
        public Action<GameObject, PointerEventData> onDrag;
        public Action<GameObject, PointerEventData> onDrop;

        static public UIEventTriggerListener Get(GameObject go)
        {
            UIEventTriggerListener listener = go.GetComponent<UIEventTriggerListener>();
            if (listener == null) listener = go.AddComponent<UIEventTriggerListener>();
            return listener;
        }

        /// <summary>
        /// 拖拽距离
        /// </summary>
        private Vector2 dragDis;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (onClick != null) onClick(gameObject);

            if (onTap != null)
            {
                ///当拖拽的距离足够短时，默认认为是点击事件
                if (dragDis.x >= -1 && dragDis.x <= 1)
                    onTap(gameObject);
            }
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            if (onDown != null) onDown(gameObject);
            ///当拖拽点击事件不为空时，为拖拽点击事件判断条件进行初始化
            if (onTap != null)
                dragDis = Vector2.zero;
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            //  MLogger.Debug("移入了：" + this.name); 
            if (onEnter != null) onEnter(gameObject);
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            //   MLogger.Debug("移出了："+this.name);
            if (onExit != null) onExit(gameObject);
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if (onUp != null) onUp(gameObject);
        }


        void OnDisable()
        {
        }

        void OnEnable()
        {

        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (onEndDrag != null) onEndDrag(gameObject, eventData);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (onBeginDrag != null) onBeginDrag(gameObject, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (onDrag != null) onDrag(gameObject, eventData);
            //当拖拽点击事件不为空时，记录拖拽距离，用以判断点击事件条件是否成立
            if (onTap != null)
                this.dragDis += eventData.delta;
        }


        /// <summary>
        /// 统一销毁处理入口
        /// </summary>
        void OnDestroy()
        {
            onTap = null;
            onBeginDrag = null;
            onClick = null;
            onDown = null;
            onDrag = null;
            onDrop = null;
            onEndDrag = null;
            onEnter = null;
            onExit = null;
            onScroll = null;
            onSelect = null;
            onUp = null;
            onUpdateSelect = null;
        }



        #region  预留的



        public void OnInitializePotentialDrag(UnityEngine.EventSystems.PointerEventData eventData)
        {

        }


        public void OnCancel(BaseEventData eventData)
        {

        }


        public void OnSubmit(BaseEventData eventData)
        {

        }

        public void OnMove(AxisEventData eventData)
        {

        }

        public void OnDeselect(BaseEventData eventData)
        {

        }



        public void OnSelect(BaseEventData eventData)
        {
            if (onSelect != null) onSelect(gameObject);
        }
        public void OnUpdateSelected(BaseEventData eventData)
        {
            if (onUpdateSelect != null) onUpdateSelect(gameObject);
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (onScroll != null) onScroll(gameObject, eventData);
        }



        //public  void OnDrag(PointerEventData eventData)
        //{
        //    if (onDrag != null) onDrag(gameObject, eventData);
        //}

        public void OnDrop(PointerEventData eventData)
        {
            if (onDrop != null) onDrop(gameObject, eventData);
        }

        #endregion

    }

}