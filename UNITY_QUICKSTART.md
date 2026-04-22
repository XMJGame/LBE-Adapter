# SDAS 在 Unity 中快速使用指南

> 适用：当前仓库已实现的 SDAS Runtime + Editor 工具链。

## 1) 导入与准备
1. 打开 Unity（建议 2022.3 LTS）。
2. 将本仓库 `Assets/SDAS` 拷贝进你的 Unity 项目。
3. 确认 Package：
   - `Input System`（推荐）
   - XR 项目可选 `XR Interaction Toolkit` / `OpenXR`

## 2) 在场景放置核心对象
1. 新建空物体 `SDAS_Root`。
2. 挂载组件：
   - `SDASRuntimeBootstrap`
3. 新建空物体 `SpaceCoordinator`，挂载：
   - `SpaceCoordinator`
4. 在 `SDASRuntimeBootstrap.spaceCoordinator` 指向上面的 `SpaceCoordinator`。

## 3) 配置 Chapter（章节）
1. 为每个章节新建空物体，例如 `Chapter_01`。
2. 挂载 `ChapterAuthoring`。
3. 在 Inspector 通过按钮：
   - `Add Circle Area`
   - `Add Polygon Area`
4. 在 SceneView 中拖拽 Handle 调整：
   - area center / radius
   - polygon 顶点
   - pivot
5. 在 `SDASRuntimeBootstrap.chapterAuthorings` 列表里添加所有章节对象。

## 4) 配置 Site（物理场地）
1. 新建空物体 `Site`。
2. 挂载 `SiteAuthoring`。
3. 在 Inspector：
   - `Add Boundary Vertex` 添加边界点
   - `Add Obstacle` 添加障碍物
4. SceneView 中拖拽边界与障碍顶点。
5. 将 `Site` 指给 `SDASRuntimeBootstrap.siteAuthoring`。

## 5) 运行求解
1. 在 `SDASRuntimeBootstrap` 中配置：
   - `useJsonAsInput = false`（先走场景数据）
   - `jsonPath = sdas_mapping.json`（可选导出）
   - `solverConfig` 与 `fallbackPolicy` 参数
2. 点击组件 ContextMenu：`Solve And Bind`。
3. Console 若出现 `Solve success`，表示求解完成并已绑定 `SpaceCoordinator`。

## 6) 应用章节映射
1. 先测试：点击 ContextMenu `Apply First Chapter`。
2. 或在代码里调用：
   - `spaceCoordinator.ApplyChapter(chapterId)`

## 7) XR Rig 接入（两种方式）
1. **Transform 模式（默认）**
   - 在 `SpaceCoordinator.xrRigOffset` 指定一个用于偏移的 Transform。
2. **XROrigin 模式（推荐）**
   - 在 `SpaceCoordinator.xrOriginComponent` 指向你的 XROrigin 组件。
   - 系统会通过反射自动寻找 offset transform。

## 8) JSON 输入模式
1. 将 `useJsonAsInput = true`。
2. `jsonPath` 指向已有 SDAS JSON。
3. 点击 `Solve And Bind`，将读取 JSON 后执行求解/绑定。

## 9) 常见问题
1. 报错 `SpaceCoordinator is not assigned`
   - 给 `SDASRuntimeBootstrap.spaceCoordinator` 正确赋值。
2. 报错 `No mapping result available`
   - 先执行 `Solve And Bind` 再调用 `ApplyFirstChapter`。
3. 报错 JSON 路径问题
   - 检查 `jsonPath` 是否为空、文件是否存在。

## 10) 推荐第一轮验证流程
1. 仅 2~3 个章节 + 简单矩形边界。
2. 先用 Circle Area 跑通。
3. 成功后再加 Polygon 和障碍。
4. 最后接入真实 XR Rig / XROrigin。

## 11) 导出数据怎么用（重点）

### 11.1 导出文件里有什么
当你执行 `Solve And Bind` 且 `jsonPath` 非空时，系统会把当前数据写到 JSON，核心是：
- `Chapters`：章节定义（行走区、约束等）
- `SiteData`：边界、障碍、网关
- `MappingResult`：每个章节最终求解结果（`worldPos/worldRotEuler/inverseOffset/inverseYaw`）

`MappingResult` 中字段含义：
- `worldPos/worldRotEuler`：章节在编辑器中的摆放结果（逻辑空间 -> 场地映射）
- `inverseOffset/inverseYaw`：给 XR Rig 的补偿（运行时真正要用）

### 11.2 运行时如何使用导出数据
你有两种方式：

1) **继续用 Bootstrap（最简单）**
- 设置 `useJsonAsInput = true`
- `jsonPath` 指向导出的 JSON
- 再点 `Solve And Bind`（会读取 JSON 作为输入并重新求解绑定）

2) **你自己的游戏流程直接消费 MappingResult（推荐）**
- 游戏加载 JSON 后，拿到 `mappingResult` 列表。
- 玩家进入章节时调用：
  - `spaceCoordinator.ApplyChapter(chapterId)`（按章节 ID 应用）
  - 或直接 `ApplyOffset(inverseOffset, inverseYaw)`（自定义切换逻辑）

示例伪代码：

```csharp
var projectData = SDASJsonSerializer.LoadFromFile(path);
spaceCoordinator.BindData(projectData.chapters, projectData.mappingResult);

// 当玩家进入 chapter 3
spaceCoordinator.ApplyChapter(3);
```

### 11.3 常见数据使用误区
1. **只用 `worldPos` 不用 `inverseOffset`**（错误）
   - 运行时应该驱动 XR Rig 的是 inverse 数据。
2. **chapterId 对不上**
   - 触发区 ID 与 `MappingResult.chapterId` 必须一致。
3. **导出后又手动改章节拓扑但不重算**
   - 章节或场地改动后，必须重新 `Solve And Bind` 再导出。
