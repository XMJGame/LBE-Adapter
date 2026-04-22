# SDAS 分步实现计划（Unity）

## Step 1（已完成）
- 建立运行时核心数据模型（Chapter / Site / MappingResult）
- 建立 XZ 平面基础几何工具（SAT 重叠判定、点到线段距离）
- 建立逆向映射数学工具（章节摆放 -> XR Rig 逆向补偿）
- 建立 JSON 序列化读写工具

## Step 2（已完成）
- 实现 Solver 输入输出 DTO 与 Cost BreakDown 结构
- 实现 Link / Overlap / Safety / Gravity / Rot 五项成本计算器
- 实现贪婪初始化求解流程

## Step 3（进行中）
- 增加全局优化（模拟退火或 Beam Search）
- 增加 fallback 动态阈值调节

## Step 4
- 实现 Chapter Editor 与 Site Mapper 的 Gizmo 与 Handles 编辑器工具

## Step 5
- 实现 Runtime SpaceCoordinator（硬切 + 平滑校准）
- 接入 XROrigin
