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
