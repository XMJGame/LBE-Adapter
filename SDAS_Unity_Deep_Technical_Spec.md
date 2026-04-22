# 大空间 VR 自动适配与逆向映射系统（SDAS）
## Unity 深度技术规格书（V1.0）

> 适用引擎：Unity 2022.3 LTS+（推荐）  
> 适用平台：OpenXR / Quest Link / PCVR  
> 核心目标：在不修改原始关卡资源坐标的前提下，将多章节虚拟空间自动适配到真实物理场地，并通过逆向映射驱动 XR Rig。

---

## 1. 核心设计哲学：空间解耦（Space Decoupling）

系统严格区分两个互为镜像的空间态，并通过 **Inverse Mapping** 实现闭环：

1. **编辑/预览态（Logic Space）**
   - 所有关卡内容在逻辑原点（或美术原始坐标）编辑。
   - 启用 Ghost Mode 时，仅在 SceneView 中“幻影摆放”。
2. **运行/执行态（Physical Mapping Space）**
   - 真实场地固定不动。
   - 系统动态更新 `XR Rig` 的 `CameraOffset/Origin` 进行玩家重定向。

### 1.1 数学定义
- 章节摆放变换：`T_chapter = (P, R)`
- 玩家补偿变换：`T_offset = inverse(T_chapter)`
- 在 2D XZ 平面求解后映射回 3D：
  - `P = (x, 0, z)`
  - `R = yaw(y)`

---

## 2. 系统模块划分

- **Module A**：Chapter Editor（关卡逻辑与拓扑编辑器）
- **Module B**：Site Mapper（物理场地映射器）
- **Module C**：Solver（动态适配求解器）
- **Module D**：Inverse Mapping Runtime（运行时空间协调器）
- **Module E**：Serialization & Fallback（数据交互与回退）

---

## 3. Module A：Chapter Editor

### 3.1 数据结构（ScriptableObject + Serializable）

```csharp
public enum AreaShapeType { Circle, Polygon }

[System.Serializable]
public class WalkArea
{
    public AreaShapeType Shape;
    public Vector3 Center;
    public float Radius;
    public List<Vector3> Vertices;
    public Vector3 Forward;
    public Vector3 Pivot;
}

[System.Serializable]
public class ChapterNode
{
    public int ChapterID;
    public string ChapterName;
    public List<WalkArea> WalkingAreas;

    public bool IsLinkedToNext;
    public bool LockRelativeTransform;
    public float PosThreshold = 0.3f;
    public float RotThreshold = 8f;
}
```

### 3.2 拓扑逻辑
- 顺序约束：`Chapter[i] -> Chapter[i+1]`
- 强绑定时启用：
  - `ΔT(i, i+1)` 恒定（在阈值内）
  - `LastArea(i)` 与 `FirstArea(i+1)` 相对位移锁定
- 阈值：
  - `posThreshold`（米）
  - `rotThreshold`（度）

### 3.3 Unity 编辑器能力
- `CustomEditor + Handles`：绘制行走区、方向箭头、Pivot。
- Inspector 批量按钮：
  - 自动计算 Pivot
  - 验证拓扑完整性
  - 导出章节 JSON

---

## 4. Module B：Site Mapper

### 4.1 场地元素定义

```csharp
public enum ObstacleType { Wall, Pillar, Temp }
public enum GatewayType { Entrance, Exit, Bidirectional }
```

- **Site Boundary**：蓝色闭合粗线（XZ 投影多边形）
- **Obstacles**：红色填充（多边形或 AABB）
- **Gateways**：入口/出口/双向（用于首尾章节吸附权重）

### 4.2 约束
- 所有 WalkingArea 必须在 Boundary 内（可有容差）
- 与 Obstacles 的最小距离不得小于 `d_min`
- 第 1 章节优先靠近入口，末章节优先靠近出口

### 4.3 Unity 可视化规范
- 边界墙：蓝色
- 障碍物：红色
- 激活行走区：绿色
- 非激活行走区：灰色

---

## 5. Module C：动态适配求解器

## 5.1 目标函数

\[
TotalCost = w_1 Cost_{Link} + w_2 Cost_{Overlap} + w_3 Cost_{Safety} + w_4 Cost_{Gravity} + w_5 Cost_{Rot}
\]

- `Cost_Link`：章节连接误差（强绑定权重高）
- `Cost_Overlap`：行走区重叠率超过 `MaxOverlapRatio` 后惩罚
- `Cost_Safety`：距障碍/边界低于 `d_min` 急剧增大
- `Cost_Gravity`：章节群重心偏离场地中心惩罚（新增）
- `Cost_Rot`：旋转成本（每度成本 > 每米平移成本，新增）

### 5.2 计算核心
- **碰撞/重叠检测**：SAT（分离轴定理）
- **求解策略**（推荐两阶段）：
  1. 贪婪初始化（按拓扑序）
  2. 全局优化（Simulated Annealing / Beam Search）

### 5.3 建议参数
- `w1=3.0, w2=5.0, w3=8.0, w4=2.0, w5=4.0`
- `MaxOverlapRatio=0.15`
- `d_min=0.4m`
- 旋转步进：`1°`
- 平移步进：`0.1m`

### 5.4 结果输出
为每个章节输出：
- `WorldPos`
- `WorldRot`
- `InverseOffset`
- `InverseRotation`

---

## 6. Module D：逆向映射运行时机制

### 6.1 Ghost Mode
- **On**：章节实例按 `T_chapter` 放置（用于调试/验证）
- **Off**：章节回到原始资源坐标（美术真实状态）

### 6.2 Redirection 策略
1. **Instant Hard Cut（硬切）**
   - 用于强绑定章节切换点。
   - 进入触发区后一次性更新 `XROrigin` 偏移。
2. **Smooth Calibration（平滑校准）**
   - 非强绑定章节或大场地。
   - 每帧校准 `0.1%~0.5%` 偏移，降低体感突变。

### 6.3 运行时协调器接口

```csharp
public interface ISpaceCoordinator
{
    void ApplyChapter(int chapterId);
    void ApplyOffset(Vector3 inverseOffset, float inverseYaw);
    void SetGhostMode(bool enabled);
}
```

---

## 7. Module E：JSON 持久化与回退

### 7.1 JSON 结构

```json
{
  "Chapters": [],
  "SiteData": {
    "Boundary": [],
    "Obstacles": [],
    "Gateways": []
  },
  "MappingResult": [
    {
      "ChapterID": 1,
      "WorldPos": [0, 0, 0],
      "WorldRot": [0, 0, 0],
      "InverseOffset": [0, 0, 0],
      "InverseRotation": [0]
    }
  ]
}
```

### 7.2 Fallback 机制
当求解器在预算迭代内失败：
1. 降低 `posTolerance`（更宽松连接）
2. 提高 `MaxOverlapRatio`（有限重叠）
3. 降低 `w5`（允许更多旋转）
4. 给出分级告警：Info / Warning / Critical

---

## 8. 工程目录建议（Unity）

```text
Assets/
  SDAS/
    Runtime/
      Core/
      Solver/
      Mapping/
      Serialization/
    Editor/
      ChapterEditor/
      SiteMapper/
      Gizmos/
    Data/
      ScriptableObjects/
      Json/
    Samples/
```

---

## 9. 调试与监控

- SceneView 同屏显示：
  - 虚拟玩家坐标（逻辑空间）
  - 物理场地坐标（映射空间）
- Runtime HUD：
  - 当前章节 ID
  - 当前 offset / yaw
  - 总成本与各子成本
  - 最近一次 fallback 原因

---

## 10. 验收标准（Definition of Done）

1. 章节数 ≥ 20 时，求解时间 < 5s（PC 开发机）。
2. 在障碍密集场景中，`Cost_Safety` 约束生效，无非法穿越。
3. 强绑定章节切换误差：
   - 位置误差 ≤ `posThreshold`
   - 角度误差 ≤ `rotThreshold`
4. Ghost Mode 开关后，资源坐标可逆且无漂移。
5. 导出 JSON 可被运行时 100% 重建映射结果。

---

## 11. 后续扩展建议

- 加入玩家行为热力图，动态反哺 `Cost_Gravity`。
- 引入多目标进化算法（NSGA-II）生成 Pareto 解集。
- 支持多楼层（Y 轴分层）与电梯/楼梯连接约束。

