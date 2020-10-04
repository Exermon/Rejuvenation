﻿using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using UI.Common.Controls.ItemDisplays;

using MapModule.Data;

namespace UI.MapSystem.Controls {

	using Windows;

	/// <summary>
	/// 对话框显示
	/// </summary>
	[RequireComponent(typeof(DialogWindow))]
	[RequireComponent(typeof(OptionContainer))]
	public class MessageDisplay : ItemDisplay<DialogMessage>{

		/// <summary>
		/// 文本打印间隔时间
		/// </summary>
		const float CharPrintDeltaTime = 0.1f;

		/// <summary>
		/// 外部组件设置
		/// </summary>
		public Text dialogText;
		public Text nameText;
		public Image bust;

		/// <summary>
		/// 内部组件设置
		/// </summary>
		[RequireTarget]
		[HideInInspector]
		public DialogWindow window;
		[RequireTarget]
		[HideInInspector]
		public OptionContainer optionContainer;

		/// <summary>
		/// 内部变量定义
		/// </summary>
		bool stopPrintReq = false; // 停止打印请求（打印到最后一个）

		/// <summary>
		/// 属性
		/// </summary>
		public bool printing { get; protected set; } = false; // 当前是否打印中

		#region 数据操作

		/// <summary>
		/// 选项数目
		/// </summary>
		/// <returns></returns>
		public int optionCount() {
			return item.options.Count;
		}

		/// <summary>
		/// 物品改变回调
		/// </summary>
		protected override void onItemChanged() {
			base.onItemChanged();
			if (printing) stopPrint();
			optionContainer.setItems(item.options);
		}

		/// <summary>
		/// 物品清除回调
		/// </summary>
		protected override void onItemClear() {
			base.onItemClear();
			if (printing) stopPrint();
			optionContainer.clearItems();
		}

		#endregion

		#region 界面绘制

		/// <summary>
		/// 绘制物品
		/// </summary>
		/// <param name="item"></param>
		protected override void drawExactlyItem(DialogMessage item) {
			base.drawExactlyItem(item);
			drawMessage(item); drawBust(item);
		}

		/// <summary>
		/// 绘制信息
		/// </summary>
		/// <param name="item"></param>
		void drawMessage(DialogMessage item) {
			nameText.text = item.name;

			doRoutine(printMessage(item.message));
		}

		/// <summary>
		/// 绘制立绘
		/// </summary>
		/// <param name="item"></param>
		void drawBust(DialogMessage item) {
			var bust = item.bust();

			this.bust.gameObject.SetActive(bust != null);
			this.bust.overrideSprite = bust;
		}

		/// <summary>
		/// 绘制空物品
		/// </summary>
		protected override void drawEmptyItem() {
			base.drawEmptyItem();
			dialogText.text = nameText.text = "";
			bust.gameObject.SetActive(false);
			bust.overrideSprite = null;
		}

		/// <summary>
		/// 停止打印
		/// </summary>
		public void stopPrint() {
			stopPrintReq = true;
		}

		/// <summary>
		/// 打印信息
		/// </summary>
		/// <returns></returns>
		IEnumerator printMessage(string message) {
			onPrintStart();

			foreach (var c in message) {
				dialogText.text += c;
				if (stopPrintReq) {
					dialogText.text = message;
					break;
				}
				yield return new WaitForSeconds(CharPrintDeltaTime);
			}

			onPrintEnd();
		}

		/// <summary>
		/// 打印开始回调
		/// </summary>
		void onPrintStart() {
			printing = true;
			dialogText.text = "";
			optionContainer.deactivate();
		}

		/// <summary>
		/// 打印结束回调
		/// </summary>
		void onPrintEnd() {
			stopPrintReq = printing = false;
			optionContainer.activate();
		}

		#endregion

	}
}
