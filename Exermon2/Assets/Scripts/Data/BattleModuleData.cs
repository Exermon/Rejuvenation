﻿
using System;
using System.Collections.Generic;

using UnityEngine;
using Random = UnityEngine.Random;

using LitJson;

using Core.Data;
using Core.Data.Loaders;

using GameModule.Data;
using GameModule.Services;

using PlayerModule.Data;
using PlayerModule.Services;

using UI.Common.Controls.ParamDisplays;

/// <summary>
/// 战斗模块
/// </summary>
namespace BattleModule { }

/// <summary>
/// 战斗模块数据
/// </summary>
namespace BattleModule.Data {

	/// <summary>
	/// 特训效果数据
	/// </summary>
	public class TraitData : BaseData {

		/// <summary>
		/// 效果代码枚举
		/// </summary>
		public enum Code {
			Unset = 0, // 空

			DamagePlus = 1, // 攻击伤害加成
			HurtPlus = 2, // 受到伤害加成
			RecoverPlus = 3, // 回复加成

			RestRecoverPlus = 4, // 回复加成（休息据点）

			RoundEndRecover = 5, // 回合结束HP回复
			RoundStartRecover = 6, // 回合开始HP回复
			BattleEndRecover = 7, // 战斗结束HP回复
			BattleStartRecover = 8, // 战斗开始HP回复

			ParamAdd = 9, // 属性加成值
			ParamRoundAdd = 10, // 回合属性加成值
			ParamBattleAdd = 11, // 战斗属性加成值

			RoundDrawCards = 12, // 回合开始抽牌数加成
			BattleDrawCards = 13, // 战斗开始抽牌数加成
		}

		/// <summary>
		/// 属性
		/// </summary>
		[AutoConvert]
		public int code { get; protected set; }
		[AutoConvert("params")]
		public JsonData params_ { get; protected set; } // 参数（数组）

		#region 数据获取

		/// <summary>
		/// 获取指定下标下的数据
		/// </summary>
		/// <typeparam name="T">类型</typeparam>
		/// <param name="index">下标</param>
		/// <param name="default_">默认值</param>
		/// <returns></returns>
		public T get<T>(int index, T default_ = default) {
			if (params_ == null || !params_.IsArray) return default_;
			if (index < params_.Count) return DataLoader.load<T>(params_[index]);
			return default_;
		}

		/// <summary>
		/// 特性代码枚举
		/// </summary>
		/// <returns></returns>
		public Code codeEnum() {
			return (Code)code;
		}

		#endregion

		/// <summary>
		/// 构造函数
		/// </summary>
		public TraitData() { }
		public TraitData(Code code, JsonData params_) {
			this.code = (int)code; this.params_ = params_;
		}

	}

	/// <summary>
	/// 战斗者
	/// </summary>
	public class Battler : BaseData {

		/// <summary>
		/// 属性
		/// </summary>
		[SerializeField] string _name;
		[AutoConvert] public string name { get => name; set { name = value; } }

		[SerializeField] int _mhp;
		[AutoConvert] public int mhp { get => mhp; set { mhp = value; } }

		[SerializeField] float _speed;
		[AutoConvert] public float speed { get => speed; set { speed = value; } }

		[SerializeField] float _attack = 0;
		[AutoConvert] public float attack { get => attack; set { attack = value; } }

		[SerializeField] float _defense = 0;
		[AutoConvert] public float defense { get => defense; set { defense = value; } }

	}

	/// <summary>
	/// 敌人
	/// </summary>
	[Serializable]
	public class Enemy : Battler {

		/// <summary>
		/// 刷新类型
		/// </summary>
		public enum RefreshType {
			None, // 不刷新
			OnDead, // 主角死亡后重置
			Custom // 自定义
		}

		/// <summary>
		/// 行为类型
		/// </summary>
		public enum BehaviourType {
			None, // 原地不动
			Random, // 随机移动
			Close, // 靠近主角
			Far, // 远离主角
			Custom // 自定义
		}

		/// <summary>
		/// 属性
		/// </summary>
		[SerializeField] string _description;
		[AutoConvert] public string description { get => description; set { description = value; } }

		[SerializeField] RefreshType _refreshType = RefreshType.OnDead;
		[AutoConvert] public RefreshType refreshType { get => refreshType; set { refreshType = value; } }

		[SerializeField] float _criticalRange;
		[AutoConvert] public float criticalRange { get => criticalRange; set { criticalRange = value; } }

		[SerializeField] BehaviourType _generalBehaviour = BehaviourType.None;
		[AutoConvert] public BehaviourType generalBehaviour { get => generalBehaviour; set { generalBehaviour = value; } }

		[SerializeField] BehaviourType _criticalBehaviour = BehaviourType.Close;
		[AutoConvert] public BehaviourType criticalBehaviour { get => criticalBehaviour; set { criticalBehaviour = value; } }

	}

	/// <summary>
	/// 主角
	/// </summary>
	public class Actor : Battler {

		/// <summary>
		/// 默认属性
		/// </summary>
		public const int DefaultMHP = 6; // 初始体力值
		public const float DefaultAttack = 0; // 初始力量
		public const float DefaultDefense = 0; // 初始格挡

		/// <summary>
		/// 控制模式
		/// </summary>
		public enum ControlType {
			Present, Past, Both
		}

		/// <summary>
		/// 属性
		/// </summary>
		[AutoConvert]
		public ControlType control { get; protected set; } = ControlType.Present;
		[AutoConvert]
		public RuntimeActor runtimeActor { get; protected set; }

		/// <summary>
		/// 玩家
		/// </summary>
		public Player player;

		/// <summary>
		/// 切换控制
		/// </summary>
		public void switchControl() {
			switch (control) {
				case ControlType.Present:
					control = ControlType.Past; break;
				case ControlType.Past:
					control = ControlType.Present; break;
			}
		}

		/// <summary>
		/// 构造函数
		/// </summary>
		public Actor() { }
		public Actor(Player player) {
			name = player.name;
			mhp = DefaultMHP;
			attack = DefaultAttack;
			defense = DefaultDefense;

			this.player = player;
			runtimeActor = new RuntimeActor();
		}
	}

	/// <summary>
	/// 技能
	/// </summary>
	[Serializable]
	public class Skill : BaseData {

		/// <summary>
		/// 攻击类型
		/// </summary>
		public enum Type {
			ShortRange, LongRange
		}

		/// <summary>
		/// 属性
		/// </summary>
		[SerializeField] string _name;
		[AutoConvert] public string name { get => name; set { name = value; } }

		[SerializeField] string _description;
		[AutoConvert] public string description { get => description; set { description = value; } }

		[SerializeField] Type _type = Type.ShortRange;
		[AutoConvert] public Type type { get => type; set { type = value; } }

		[SerializeField] float _frequency;
		[AutoConvert] public float frequency { get => frequency; set { frequency = value; } }

		[SerializeField] float _speed;
		[AutoConvert] public float speed { get => speed; set { speed = value; } }

		[SerializeField] float _range;
		[AutoConvert] public float range { get => range; set { range = value; } }

		[SerializeField] float _power;
		[AutoConvert] public float power { get => power; set { power = value; } }

	}

	#region 运行时数据

	/// <summary>
	/// 运行时BUFF
	/// </summary>
	public class RuntimeBuff : BaseData {

		/// <summary>
		/// 属性
		/// </summary>
		[AutoConvert]
		public int paramId { get; protected set; } // 属性ID
		[AutoConvert]
		public int value { get; protected set; } // 改变数值
		[AutoConvert]
		public double rate { get; protected set; } // 改变比率
		[AutoConvert]
		public float seconds { get; protected set; } // BUFF持续时间

		/// <summary>
		/// 是否为Debuff
		/// </summary>
		/// <returns></returns>
		public bool isDebuff() {
			return value < 0 || rate < 1;
		}

		/// <summary>
		/// BUFF是否过期
		/// </summary>
		/// <returns></returns>
		public bool isOutOfDate() {
			return seconds == 0;
		}

		/// <summary>
		/// 更新
		/// </summary>
		public void update() {
			if (seconds > 0) seconds -= Time.deltaTime;
		}

		/// <summary>
		/// 构造函数
		/// </summary>
		public RuntimeBuff() { }
		public RuntimeBuff(int paramId,
			int value = 0, double rate = 1, float seconds = 0) {
			this.paramId = paramId; this.value = value;
			this.rate = rate; this.seconds = seconds;
		}

	}

	/// <summary>
	/// 战斗者
	/// </summary>
	public abstract class RuntimeBattler : BaseData {

		/// <summary>
		/// 属性ID约定
		/// </summary>
		public const int MHPParamId = 1;
		public const int AttackParamId = 2;
		public const int DefenseParamId = 3;
		public const int SpeedParamId = 4;

		public const int MaxParamCount = 4;

		/// <summary>
		/// 属性
		/// </summary>
		[AutoConvert]
		public int hp { get; protected set; }
		[AutoConvert]
		public List<RuntimeBuff> buffs { get; protected set; } = new List<RuntimeBuff>();
		
		/// <summary>
		/// 是否玩家
		/// </summary>
		/// <returns></returns>
		public virtual bool isActor() { return false; }

		/// <summary>
		/// 是否敌人
		/// </summary>
		/// <returns></returns>
		public virtual bool isEnemy() { return false; }

		/// <summary>
		/// 获取战斗者
		/// </summary>
		/// <returns></returns>
		public abstract Battler battler { get; }

		#region 属性控制

		#region 属性快捷定义

		/// <summary>
		/// 快捷定义
		/// </summary>
		public int mhp => (int)Math.Round(param(MHPParamId));
		public float attack => param(AttackParamId);
		public float defense => param(DefenseParamId);
		public float speed => param(SpeedParamId);

		/// <summary>
		/// 基础最大生命值
		/// </summary>
		/// <returns></returns>
		protected virtual int baseMHP() { return battler.mhp; }

		/// <summary>
		/// 基础力量
		/// </summary>
		/// <returns></returns>
		protected virtual float baseAttack() { return battler.attack; }

		/// <summary>
		/// 基础格挡
		/// </summary>
		/// <returns></returns>
		protected virtual float baseDefense() { return battler.defense; }

		/// <summary>
		/// 基础速度
		/// </summary>
		/// <returns></returns>
		protected virtual float baseSpeed() { return battler.speed; }

		#endregion

		#region HP控制

		#region HP变化显示

		/// <summary>
		/// HP该变量
		/// </summary>
		public class DeltaHP {

			public int value = 0; // 值

			public bool critical = false; // 是否暴击
			public bool miss = false; // 是否闪避

			/// <summary>
			/// 构造函数
			/// </summary>
			/// <param name="value"></param>
			public DeltaHP(int value = 0, bool critical = false, bool miss = false) {
				this.value = value; this.critical = critical; this.miss = miss;
			}
		}

		/// <summary>
		/// HP变化量
		/// </summary>
		DeltaHP _deltaHP = null;
		public DeltaHP deltaHP {
			get {
				var res = _deltaHP;
				_deltaHP = null; return res;
			}
		}

		/// <summary>
		/// 设置闪避标志
		/// </summary>
		public void setMissFlag() {
			_deltaHP = _deltaHP ?? new DeltaHP();
			_deltaHP.miss = true;
		}

		/// <summary>
		/// 设置暴击标志
		/// </summary>
		public void setCriticalFlag() {
			_deltaHP = _deltaHP ?? new DeltaHP();
			_deltaHP.critical = true;
		}

		/// <summary>
		/// 设置值变化
		/// </summary>
		/// <param name="value"></param>
		public void setHPChange(int value) {
			_deltaHP = _deltaHP ?? new DeltaHP();
			_deltaHP.value += value;
		}

		#endregion

		/// <summary>
		/// 改变HP
		/// </summary>
		/// <param name="value">目标值</param>
		/// <param name="show">是否显示</param>
		public void changeHP(int value, bool show = true) {
			var oriHp = hp;
			hp = Mathf.Clamp(value, 0, mhp);
			if (show) setHPChange(hp - oriHp);
			if (hp <= 0) onDie();
		}

		/// <summary>
		/// 增加HP
		/// </summary>
		/// <param name="value">增加值</param>
		/// <param name="show">是否显示</param>
		public void addHP(int value, bool show = true) {
			changeHP(hp + value, show);
		}

		/// <summary>
		/// 增加HP
		/// </summary>
		/// <param name="rate">增加率</param>
		/// <param name="show">是否显示</param>
		public void addHP(double rate, bool show = true) {
			changeHP((int)Math.Round(hp + mhp * rate), show);
		}

		/// <summary>
		/// 增加HP
		/// </summary>
		/// <param name="rate">增加率</param>
		/// <param name="show">是否显示</param>
		public void addHPInRestNode(double rate, bool show = true) {
			var traits = filterTraits(TraitData.Code.RestRecoverPlus);

			int val = sumTraits(traits);
			rate += sumTraits(traits, 1) / 100.0;
			addHP((int)Math.Round(mhp * rate + val));
		}

		/// <summary>
		/// 回复所有HP
		/// </summary>
		/// <param name="show">是否显示</param>
		public void recoverAll(bool show = true) {
			changeHP(mhp, show);
		}

		/// <summary>
		/// 是否死亡
		/// </summary>
		/// <returns></returns>
		public bool isDead() {
			return hp <= 0;
		}

		#endregion

		#region 属性统一接口

		/// <summary>
		/// 基本属性值
		/// </summary>
		/// <param name="paramId">属性ID</param>
		/// <returns>属性值</returns>
		public virtual float baseParam(int paramId) {
			switch (paramId) {
				case MHPParamId: return baseMHP();
				case AttackParamId: return baseAttack();
				case DefenseParamId: return baseDefense();
				case SpeedParamId: return baseSpeed();
				default: return 0;
			}
		}

		/// <summary>
		/// Buff附加值
		/// </summary>
		/// <param name="paramId">属性ID</param>
		/// <returns></returns>
		public float buffValue(int paramId) {
			int value = 0;
			foreach (var buff in buffs)
				if (buff.paramId == paramId && !buff.isOutOfDate()) value += buff.value;
			return value;
		}

		/// <summary>
		/// Buff附加率
		/// </summary>
		/// <param name="paramId">属性ID</param>
		/// <returns></returns>
		public double buffRate(int paramId) {
			double rate = 1;
			foreach (var buff in buffs)
				if (buff.paramId == paramId && !buff.isOutOfDate()) rate *= buff.rate;
			return rate;
		}

		/// <summary>
		/// 特性属性
		/// </summary>
		/// <param name="paramId">属性ID</param>
		/// <returns></returns>
		public virtual float traitParamVal(int paramId) {
			return sumTraits<int>(TraitData.Code.ParamAdd, paramId);
		}

		/// <summary>
		/// 特性属性
		/// </summary>
		/// <param name="paramId">属性ID</param>
		/// <returns></returns>
		public virtual double traitParamRate(int paramId) {
			return multTraits<int>(TraitData.Code.ParamAdd, paramId);
		}

		/// <summary>
		/// 额外属性
		/// </summary>
		/// <param name="paramId">属性ID</param>
		/// <returns></returns>
		public virtual float extraParam(int paramId) {
			return 0;
		}

		/// <summary>
		/// 属性值
		/// </summary>
		/// <param name="paramId">属性ID</param>
		/// <returns>属性值</returns>
		public float param(int paramId) {
			var base_ = baseParam(paramId) + traitParamVal(paramId) + buffValue(paramId);
			var rate = buffRate(paramId) * traitParamRate(paramId);
			var extra = extraParam(paramId);
			return (int)Math.Round((base_) * rate + extra);
		}

		/// <summary>
		/// 属性相加
		/// </summary>
		/// <param name="paramId">属性ID</param>
		/// <param name="value">增加值</param>
		public void addParam(int paramId, int value) {
			addBuff(paramId, value);
		}
		public void addParam(int paramId, int value, double rate) {
			addBuff(paramId, value, rate);
		}

		/// <summary>
		/// 属性相乘
		/// </summary>
		/// <param name="paramId">属性ID</param>
		/// <param name="rate">比率</param>
		public void multParam(int paramId, double rate) {
			addBuff(paramId, 0, rate);
		}

		/// <summary>
		/// 伤害加成
		/// </summary>
		public int damagePlusVal() {
			return sumTraits(TraitData.Code.DamagePlus);
		}
		public double damagePlusRate() {
			return multTraits(TraitData.Code.DamagePlus);
		}

		/// <summary>
		/// 受伤加成
		/// </summary>
		public int hurtPlusVal() {
			return sumTraits(TraitData.Code.HurtPlus);
		}
		public double hurtPlusRate() {
			return multTraits(TraitData.Code.HurtPlus);
		}

		/// <summary>
		/// 回复加成
		/// </summary>
		public int recoverPlusVal() {
			return sumTraits(TraitData.Code.RecoverPlus);
		}
		public double recoverPlusRate() {
			return multTraits(TraitData.Code.RecoverPlus);
		}

		#endregion

		#endregion

		#region 特性控制

		/// <summary>
		/// 获取所有特性
		/// </summary>
		/// <returns></returns>
		public virtual List<TraitData> traits() {
			return new List<TraitData>();
			//return statesTraits();
		}

		///// <summary>
		///// 所有状态特性
		///// </summary>
		//List<TraitData> statesTraits() {
		//	var res = new List<TraitData>();
		//	foreach (var state in states)
		//		res.AddRange(state.Value.traits());
		//	return res;
		//}

		/// <summary>
		/// 获取特定特性
		/// </summary>
		/// <param name="code">特性枚举</param>
		/// <returns>返回符合条件的特性</returns>
		public List<TraitData> filterTraits(TraitData.Code code) {
			return traits().FindAll(trait => trait.code == (int)code);
		}
		/// <param name="param">参数取值</param>
		/// <param name="id">参数下标</param>
		public List<TraitData> filterTraits<T>(TraitData.Code code, T param, int id = 0) {
			return traits().FindAll(trait => trait.code == (int)code
				&& Equals(trait.get<T>(id), param));
		}

		/// <summary>
		/// 特性值求和
		/// </summary>
		/// <param name="code">特性枚举</param>
		/// <param name="index">特性参数索引</param>
		/// <param name="base_">求和基础值</param>
		/// <returns></returns>
		public int sumTraits(TraitData.Code code, int index = 0, int base_ = 0) {
			return sumTraits(filterTraits(code), index, base_);
		}
		/// <param name="param">参数取值</param>
		/// <param name="id">参数下标</param>
		public int sumTraits<T>(TraitData.Code code, T param, int id = 0, int index = 1, int base_ = 0) {
			return sumTraits(filterTraits(code, param, id), index, base_);
		}
		public int sumTraits(List<TraitData> traits, int index = 0, int base_ = 0) {
			var res = base_;
			foreach (var trait in traits)
				res += trait.get(index, 0);
			return res;
		}

		/// <summary>
		/// 特性值求积
		/// </summary>
		/// <param name="code">特性枚举</param>
		/// <param name="index">特性参数索引</param>
		/// <param name="base_">概率基础值</param>
		/// <returns></returns>
		public double multTraits(TraitData.Code code, int index = 1, int base_ = 100) {
			return multTraits(filterTraits(code), index, base_);
		}
		/// <param name="param">参数取值</param>
		/// <param name="id">参数下标</param>
		public double multTraits<T>(TraitData.Code code, T param, int id = 0, int index = 2, int base_ = 100) {
			return multTraits(filterTraits(code, param, id), index, base_);
		}
		public double multTraits(List<TraitData> traits, int index = 1, int base_ = 100) {
			var res = 1.0;
			foreach (var trait in traits)
				res *= (base_ + trait.get(index, 0)) / 100.0;
			return res;
		}

		#endregion

		#region Buff控制

		/// <summary>
		/// 状态是否改变
		/// </summary>
		List<RuntimeBuff> _addedBuffs = new List<RuntimeBuff>();
		public List<RuntimeBuff> addedBuffs {
			get {
				var res = _addedBuffs;
				_addedBuffs.Clear(); return res;
			}
		}

		#region Buff变更

		/// <summary>
		/// 添加Buff
		/// </summary>
		/// <param name="paramId">属性ID</param>
		/// <param name="value">变化值</param>
		/// <param name="rate">变化率</param>
		/// <param name="turns">持续回合</param>
		/// <returns>返回添加的Buff</returns>
		public RuntimeBuff addBuff(int paramId,
			int value = 0, double rate = 1, int turns = 0) {
			return addBuff(new RuntimeBuff(paramId, value, rate, turns));
		}
		public RuntimeBuff addBuff(RuntimeBuff buff) {
			buffs.Add(buff); onBuffAdded(buff);
			_addedBuffs.Add(buff);

			return buff;
		}

		/// <summary>
		/// 移除Buff
		/// </summary>
		/// <param name="index">Buff索引</param>
		public void removeBuff(int index, bool force = false) {
			var buff = buffs[index];
			buffs.RemoveAt(index);
			_addedBuffs.Remove(buff);

			onBuffRemoved(buff, force);
		}
		/// <param name="buff">Buff对象</param>
		public void removeBuff(RuntimeBuff buff, bool force = false) {
			buffs.Remove(buff);
			_addedBuffs.Remove(buff);

			onBuffRemoved(buff, force);
		}

		/// <summary>
		/// 移除多个满足条件的Buff
		/// </summary>
		/// <param name="p">条件</param>
		public void removeBuffs(Predicate<RuntimeBuff> p, bool force = true) {
			for (int i = buffs.Count - 1; i >= 0; --i)
				if (p(buffs[i])) removeBuff(i, force);
		}

		/// <summary>
		/// 清除所有Debuff
		/// </summary>
		public void removeDebuffs(bool force = true) {
			removeBuffs(buff => buff.isDebuff(), force);
		}

		/// <summary>
		/// 清除所有Buff
		/// </summary>
		public void clearBuffs(bool force = true) {
			for (int i = buffs.Count - 1; i >= 0; --i)
				removeBuff(i, force);
		}

		/// <summary>
		/// 是否处于指定条件的Buff
		/// </summary>
		public bool containsBuff(int paramId) {
			return buffs.Exists(buff => buff.paramId == paramId);
		}
		public bool containsBuff(Predicate<RuntimeBuff> p) {
			return buffs.Exists(p);
		}

		#endregion

		#region Buff判断

		/// <summary>
		/// 是否处于指定条件的Debuff
		/// </summary>
		public bool containsDebuff(int paramId) {
			return buffs.Exists(buff => buff.isDebuff() && buff.paramId == paramId);
		}

		/// <summary>
		/// 是否存在Debuff
		/// </summary>
		public bool anyDebuff() {
			return buffs.Exists(buff => buff.isDebuff());
		}

		/// <summary>
		/// 获取指定条件的Buff
		/// </summary>
		public RuntimeBuff getBuff(int paramId) {
			return buffs.Find(buff => buff.paramId == paramId);
		}
		public RuntimeBuff getBuff(Predicate<RuntimeBuff> p) {
			return buffs.Find(p);
		}

		/// <summary>
		/// 获取指定条件的Buff（多个）
		/// </summary>
		public List<RuntimeBuff> getBuffs(Predicate<RuntimeBuff> p) {
			return buffs.FindAll(p);
		}

		#endregion

		#endregion

		#region 行动控制

		/// <summary>
		/// 行动序列
		/// </summary>
		Queue<RuntimeAction> actions = new Queue<RuntimeAction>();

		/// <summary>
		/// 当前行动
		/// </summary>
		/// <returns></returns>
		public virtual RuntimeAction currentAction() {
			if (actions.Count <= 0) return null;
			var action = actions.Dequeue();
			return action;
		}

		/// <summary>
		/// 增加行动
		/// </summary>
		/// <param name="action">行动</param>
		public void addAction(RuntimeAction action) {
			actions.Enqueue(action);
		}
		public void addAction(Skill skill) {
			addAction(new RuntimeAction(this, skill));
		}

		/// <summary>
		/// 处理指定行动
		/// </summary>
		public virtual void processAction(RuntimeAction action) {

		}

		/// <summary>
		/// 清除所有行动
		/// </summary>
		public void clearActions() {
			actions.Clear();
		}

		#endregion

		#region 回调控制

		/// <summary>
		/// 战斗开始回调
		/// </summary>
		public virtual void onBattleStart() {
			clearBuffs();
		}

		/// <summary>
		/// 行动开始回调
		/// </summary>
		/// <param name="round">回合数</param>
		public virtual void onActionStart(RuntimeAction action) {
			_addedBuffs.Clear();
		}

		#region BUFF回调

		/// <summary>
		/// BUFF添加回调
		/// </summary>
		public virtual void onBuffAdded(RuntimeBuff buff) { }

		/// <summary>
		/// BUFF移除回调
		/// </summary>
		public virtual void onBuffRemoved(RuntimeBuff buff, bool force = false) { }

		#endregion

		/// <summary>
		/// 当前行动结束回调
		/// </summary>
		public virtual void onActionEnd(RuntimeAction action) { }

		/// <summary>
		/// 死亡回调
		/// </summary>
		protected virtual void onDie() { }

		/// <summary>
		/// 每帧更新
		/// </summary>
		/// <param name="round">回合数</param>
		public virtual void update() {
			updateBuffs();
		}

		/// <summary>
		/// 更新BUFF
		/// </summary>
		void updateBuffs() {
			for (int i = buffs.Count - 1; i >= 0; --i) {
				var buff = buffs[i]; buff.update();
				if (buff.isOutOfDate()) removeBuff(i);
			}
		}

		#endregion

	}

	/// <summary>
	/// 战斗玩家
	/// </summary>
	public class RuntimeActor : RuntimeBattler {

		/// <summary>
		/// 战斗者
		/// </summary>
		public override Battler battler => PlayerService.Get().actor;

	}

	/// <summary>
	/// 战斗敌人
	/// </summary>
	public class RuntimeEnemy : RuntimeBattler {

		/// <summary>
		/// 属性
		/// </summary>
		[AutoConvert]
		public int enemyId { get; protected set; }

		/// <summary>
		/// 自定义敌人
		/// </summary>
		public Enemy customEnemy = null;

		/// <summary>
		/// 战斗者
		/// </summary>
		public override Battler battler => customEnemy ?? enemy();

		/// <summary>
		/// 获取标签实例
		/// </summary>
		/// <returns></returns>
		protected CacheAttr<Enemy> enemy_ = null;
		protected Enemy _enemy_() {
			return poolGet<Enemy>(enemyId);
		}
		public Enemy enemy() {
			return enemy_?.value();
		}

		/// <summary>
		/// 构造函数
		/// </summary>
		public RuntimeEnemy() { }
		public RuntimeEnemy(int enemyId) { this.enemyId = enemyId; }
	}

	/// <summary>
	/// 运行时行动
	/// </summary>
	public class RuntimeAction : BaseData {

		/// <summary>
		/// 属性
		/// </summary>
		[AutoConvert]
		public RuntimeBattler subject { get; protected set; } // 主体
		[AutoConvert]
		public Skill skill { get; protected set; } // 技能

		/// <summary>
		/// 结果
		/// </summary>
		public RuntimeActionResult[] results { get; protected set; } = null;

		/// <summary>
		/// 构造函数
		/// </summary>
		public RuntimeAction() { }
		public RuntimeAction(RuntimeBattler subject, Skill skill) {
			this.subject = subject; this.skill = skill;
		}
	}

	/// <summary>
	/// 运行时行动结果
	/// </summary>
	public class RuntimeActionResult : BaseData {

		/// <summary>
		/// 属性
		/// </summary>
		[AutoConvert]
		public int hpDamage { get; set; }
		[AutoConvert]
		public int hpRecover { get; set; }
		[AutoConvert]
		public int hpDrain { get; set; }

		/// <summary>
		/// 状态/Buff变更
		/// </summary>
		[AutoConvert]
		public RuntimeBuff[] addBuffs { get; set; }

		/// <summary>
		/// 行动
		/// </summary>
		public RuntimeAction action { get; protected set; }

		/// <summary>
		/// 所属目标
		/// </summary>
		public RuntimeBattler object_ { get; protected set; }

		/// <summary>
		/// 构造函数
		/// </summary>
		public RuntimeActionResult() { }
		public RuntimeActionResult(RuntimeBattler object_, RuntimeAction action) {
			this.object_ = object_; this.action = action;
		}
	}

	#endregion

}
