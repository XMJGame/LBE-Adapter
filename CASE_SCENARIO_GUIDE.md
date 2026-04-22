# SDAS 案例场景：美术侧 + 适配侧联调（7 章节 / 200㎡）

## 1. 你要的假数据
- 文件：`Assets/SDAS/Samples/SevenChapters_200sqm.json`
- 内容：
  - 7 个章节（Circle + Polygon 混合）
  - 200㎡ 场地（20m x 10m）
  - 2 个障碍物 + 入口/出口 Gateway

## 2. 美术侧怎么用（Authoring）
1. 用该 JSON 作为参考，在 Unity 内建立 7 个 `ChapterAuthoring` 物体。
2. 每个章节对应一个 scene block（入口、森林、遗迹、桥、峡谷、洞穴、出口）。
3. 将场地设置为 20m x 10m 边界，并摆放 2 个障碍。
4. 通过 `ChapterAuthoringEditor` / `SiteAuthoringEditor` 微调点位。

## 3. 适配侧怎么测（Adapter）
1. 在 `SDASRuntimeBootstrap` 里：
   - `useJsonAsInput = true`
   - `jsonPath = Assets/SDAS/Samples/SevenChapters_200sqm.json`
2. 点 `Solve And Bind`。
3. 成功后点 `Apply First Chapter`，确认 XR offset 生效。
4. 调用 `ApplyChapter(1..7)` 逐章节验证。

## 4. 自动化测试
- `Assets/SDAS/Tests/SampleScenarioSolverTests.cs`
- 该测试会：
  1) 读取样例 JSON
  2) 运行 Solver
  3) 断言 7 章节都有 mapping result
  4) 导出 `SevenChapters_200sqm.Result.json`

## 5. 预期产物
- 输入：`Assets/SDAS/Samples/SevenChapters_200sqm.json`
- 输出：`Assets/SDAS/Samples/SevenChapters_200sqm.Result.json`

