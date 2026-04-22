# SDAS 当前未完善项（2026-04-10）

> 本文档用于回答“还有哪些没完善”，并给出优先级。

## P0（建议下一步优先完成）
1. **真实 Unity 集成验证缺失**
   - 尚未在 Unity Editor 执行完整 PlayMode / EditMode 测试流程。
   - Editor Handles、Runtime 重定向、JSON 读写均需要实际场景回归。

2. **求解器精度与性能基准未建立**
   - 缺少章节数（10/20/50）对应耗时、收敛质量、失败率基准。
   - 缺少与 DoD 对应的自动化指标采集。

3. **Link 约束仍是简化实现**
   - 当前 `Cost_Link` 主要按章节 pivot 距离惩罚，尚未严格实现 `LastArea -> FirstArea` 的相对位移/旋转锁。

## P1（功能完整性提升）
1. **Overlap 成本较粗糙**
   - 目前使用重叠次数比值近似；未计算真实重叠面积比例。

2. **Safety 约束边界情况**
   - 当前使用点内测 + 点到边距离，缺少“边穿越边界/障碍”检测（线段与多边形相交判定）。

3. **SpaceCoordinator 章节切换策略可扩展**
   - 尚未接入“触发区驱动切换”、“按玩家速度动态调节平滑比例”等策略。

## P2（工程化增强）
1. **数据版本化与迁移**
   - JSON 缺少 schema version 与迁移器。

2. **编辑器 UX 提升**
   - Site/Chapter 编辑器缺少批量校验面板（闭合多边形检测、自交检测、非法点提示）。

3. **测试覆盖率提升**
   - 缺少 solver 端到端测试、editor utility 测试、runtime bootstrap 异常路径测试。

## 本次已补的稳健性修复
- `SDASRuntimeBootstrap` 增加了 `SpaceCoordinator` 空引用保护和 JSON 加载异常处理。
- `SolverCostEvaluator` 增加了 `siteData` 和 `siteData.obstacles` 的空值保护，降低运行时空引用风险。
