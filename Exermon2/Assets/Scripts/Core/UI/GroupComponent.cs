﻿
using System.Collections.Generic;
using UnityEngine;

using Core.UI.Utils;

namespace Core.UI {

    /// <summary>
    /// 组合类型视图
    /// </summary>
    /// <remarks>
    /// 当一个父物体里面需要包含多个子物体（必须为属于同一种预制件的物体）
    /// 且该子物体挂载了T脚本，该脚本能够对子物体进行统一管理。
    /// </remarks>
    /// <typeparam name="T">一个 Unity 中的任意组件</typeparam>
    public class GroupComponent<T> : GeneralComponent where T : MonoBehaviour {

		/// <summary>
		/// 原始信息
		/// </summary>
		protected class OriginInfo {

			/// <summary>
			/// 属性
			/// </summary>
			T component;

			Transform parent;
			string tag; int layer;

			/// <summary>
			/// 初始化
			/// </summary>
			public OriginInfo(T component) {
				var obj = component.gameObject;
				this.component = component;

				parent = obj.transform.parent;
				layer = obj.layer; tag = obj.tag;
			}

			/// <summary>
			/// 重置
			/// </summary>
			/// <param name="obj"></param>
			public void reset() {
				var obj = component.gameObject;

				obj.transform.parent = parent;
				obj.layer = layer; obj.tag = tag;
			}
		}

        /// <summary>
        /// 外部组件设置
        /// </summary>
        public Transform container;
        public T[] presetSubViews = new T[0];

        /// <summary>
        /// 外部变量设置
        /// </summary>
        public string subViewNameFormat = ""; // 子视图名称

        /// <summary>
        /// 预制件设置
        /// </summary>
        public GameObject subViewPrefab; // 子预制件

		/// <summary>
		/// 内部变量声明
		/// </summary>
		protected List<T> subViews = new List<T>(); // 子视图
		protected Dictionary<T, OriginInfo> oriInfos = 
			new Dictionary<T, OriginInfo>(); // 原始数据

		/// <summary>
		/// RectTransform 容器
		/// </summary>
		public RectTransform rectContainer => container as RectTransform;
			
		#region 初始化

		/// <summary>
		/// 初次打开时初始化（子类中重载）
		/// </summary>
		protected override void initializeOnce() {
            base.initializeOnce();
            initializeSubViews();
        }

        /// <summary>
        /// 初始化子视图
        /// </summary>
        void initializeSubViews() {
            if (presetSubViews.Length > 0)
                initializeSubViewsByPreset();
            else initializeSubViewsByAutoFind();
        }

        /// <summary>
        /// 通过预设来初始化子视图
        /// </summary>
        void initializeSubViewsByPreset() {
            for (int i = 0; i < presetSubViews.Length; i++)
                onSubViewCreated(presetSubViews[i], i);
        }

        /// <summary>
        /// 通过查找来初始化子视图
        /// </summary>
        void initializeSubViewsByAutoFind() {
            int index = subViews.Count; string name; T subView;
            container = container ?? transform;
            while (true) {
                name = subViewName(index);
                subView = SceneUtils.find<T>(container, name);
                if (subView == null) break;
                onSubViewCreated(subView, index++);
            }
        }

		#endregion

		#region 调试
		
		/// <summary>
		/// 显示所有的物品项
		/// </summary>
		public void debugShowItemDisplays() {
			debugLog(subViewsCount() + ": " + string.Join(", ", subViews));
		}

		#endregion

		#region 数据控制

		/// <summary>
		/// 子视图名称
		/// </summary>
		/// <param name="index">索引</param>
		/// <returns>名称</returns>
		string subViewName(int index) {
            string format = "{0}";
            if (subViewNameFormat.Length > 0)
                format = subViewNameFormat;
            else if (subViewPrefab)
                format = subViewPrefab.name + format;
            return string.Format(format, index + 1);
        }

        /// <summary>
        /// 获取子视图数
        /// </summary>
        /// <returns></returns>
        public int subViewsCount() {
            return subViews.Count;
		}

		/// <summary>
		/// 获取子视图数组
		/// </summary>
		/// <returns>子视图数组</returns>
		public T[] getSubViews() {
			return subViews.ToArray();
		}

		/// <summary>
		/// 获取子视图数组
		/// </summary>
		/// <returns>子视图数组</returns>
		public bool hasSubView(T sub) {
			return subViews.Contains(sub);
		}

		/// <summary>
		/// 获取子视图数组
		/// </summary>
		/// <returns>子视图数组</returns>
		public T getSubView(int index) {
			if (index >= 0 && index < subViews.Count)
				return subViews[index];
			return null;
		}

		/// <summary>
		/// 获取子视图索引
		/// </summary>
		/// <param name="subView">子视图</param>
		/// <returns>返回指定子视图索引</returns>
		public int getIndex(T subView) {
            return subViews.IndexOf(subView);
        }

		#endregion

		#region 界面控制

		/// <summary>
		/// 添加子视图
		/// </summary>
		/// <param name="subView"></param>
		public bool addSubView(T sub) {
			// 是否存在
			if (subViews.Contains(sub)) return false;

			// 数组添加
			subViews.Add(sub);
			oriInfos.Add(sub, new OriginInfo(sub));

			// 调整
			adjustNewSubView(sub);

			return true;
		}

		/// <summary>
		/// 调整新加入的子视图
		/// </summary>
		/// <param name="sub"></param>
		protected virtual void adjustNewSubView(T sub) {
			sub.transform.parent = container;
			sub.transform.SetAsLastSibling();
			sub.gameObject.layer = gameObject.layer;
			sub.gameObject.tag = gameObject.tag;
		}

		/// <summary>
		/// 移除子视图
		/// </summary>
		/// <param name="subView"></param>
		public bool removeSubView(T sub) {
			// 是否不存在
			if (!subViews.Contains(sub)) return false;

			// 重置
			if (oriInfos.ContainsKey(sub))
				oriInfos[sub]?.reset();

			// 数组删除
			subViews.Remove(sub);
			oriInfos.Remove(sub);

			return true;
		}
		public bool removeSubView(int index) {
			// 判断是否不存在
			if (index < 0 || index >= subViews.Count) return false;
			return removeSubView(subViews[index]);
		}

		/// <summary>
		/// 获取预制件
		/// </summary>
		/// <param name="index">索引</param>
		/// <returns></returns>
		protected virtual GameObject getSubViewPerfab(int index) {
			return subViewPrefab;
		}

		/// <summary>
		/// 创建物品显示组件
		/// </summary>
		/// <param name="index">索引</param>
		protected virtual T createSubView(int index) {
            var res = getOrCreateSubView(index);
            return res;
        }

        /// <summary>
        /// 获取或者创建一个子视图
        /// </summary>
        /// <returns>ItemDisplay</returns>
        T getOrCreateSubView(int index) {
            if (index < subViews.Count) return subViews[index];
            var obj = Instantiate(getSubViewPerfab(index), container);
            var sub = SceneUtils.get<T>(obj);

            obj.name = subViewName(index);
            onSubViewCreated(sub, index);
            return sub;
        }

        /// <summary>
        /// 释放一个子视图
        /// </summary>
        /// <param name="index"></param>
        protected void destroySubView(int index) {
            if (index < subViews.Count) {
                Destroy(subViews[index].gameObject);
                onSubViewDestroyed(index);
            }
        }

        /// <summary>
        /// 子视图创建回调
        /// </summary>
        /// <param name="sub">子视图</param>
        /// <param name="index">索引</param>
        protected virtual void onSubViewCreated(T sub, int index) {
			addSubView(sub);
		}

		/// <summary>
		/// 子视图销毁回调
		/// </summary>
		/// <param name="index">索引</param>
		protected virtual void onSubViewDestroyed(int index) {
			removeSubView(subViews[index]);
		}

		/// <summary>
		/// 刷新所有子视图
		/// </summary>
		void refreshSubViews() {
            for (int i = 0; i < subViews.Count; ++i)
                refreshSubView(subViews[i], i);
        }

        /// <summary>
        /// 刷新子视图
        /// </summary>
        /// <param name="sub">子视图</param>
        protected virtual void refreshSubView(T sub, int index) {
			(sub as GeneralComponent)?.requestRefresh(true);
        }

        /// <summary>
        /// 清空所有子视图
        /// </summary>
        void clearSubViews() {
            for (int i = 0; i < subViews.Count; ++i)
                clearSubView(subViews[i]);
        }

        /// <summary>
        /// 刷新子视图
        /// </summary>
        /// <param name="sub">子视图</param>
        protected virtual void clearSubView(T sub) {
			(sub as GeneralComponent)?.requestClear(true);
        }

        /// <summary>
        /// 刷新视窗
        /// </summary>
        protected override void refresh() {
            base.refresh();
            refreshSubViews();
        }

        /// <summary>
        /// 清除描述
        /// </summary>
        protected override void clear() {
            base.clear();
            clearSubViews();
        }

        #endregion
    }
}