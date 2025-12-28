# Unity 益智游戏开发文档：动态一笔连 (One-Line Link)

## 1. 项目概述 (Project Overview)

本项目是一款基于 Unity UGUI 开发的 2D 益智类连线解谜游戏。核心玩法类似于“一笔画”或“Flow Free”的变体。玩家需要在 $M \times N$ 的网格中，通过一笔连续的滑动操作，连接所有的“目标点”（皮蛋），同时避开“障碍点”（普通鸡蛋）或空白区域，必须**填满所有目标点**且**路径不中断**才算胜利。

### 1.1 核心特性 (Key Features)

*   **动态网格系统**：支持配置任意行列数（建议 $3 \times 3$ 到 $10 \times 10$），根据屏幕尺寸自动向中适配。
*   **灵活的规则配置**：
    *   **斜连开关 (`allowDiagonal`)**：关卡可配置是否允许对角线连接。
    *   **回退机制**：支持连线过程中原路退回撤销。
*   **即时判定**：手指抬起 (PointerUp) 即结算，爽快感强。
*   **移动端适配**：完全基于 UGUI EventSystem，天然支持多分辨率与触摸操作。

---

## 2. 游戏策划与交互逻辑 (Game Design & Logic)

### 2.1 游戏元素定义

| 元素名称 | 代号 (Enum) | 说明 | 视觉表现 |
| :--- | :--- | :--- | :--- |
| **目标点 (Target)** | `CellType.Target` | 必须要经过的点 (皮蛋)。 | 深色/纹理丰富的蛋，或者发光节点。 |
| **障碍/普通点 (Normal)** | `CellType.Normal` | 不可经过或仅作为阻挡的点 (根据设计也可作为必须填满的空地，当前设计为障碍)。 | 浅色/普通鸡蛋，或者石头。 |
| **起点 (Start)** | `CellType.Target` | 玩家落指的第一个目标点。 | 高亮显示。 |

> **注**：当前文档设计为“连接所有目标点”。如果是“填满所有格子”的玩法，则所有可行走的格子都是 Target。

### 2.2 连通性判定算法

系统根据关卡配置的 `allowDiagonal` 变量决定相邻判定逻辑。

假设当前点坐标 $P_1(x_1, y_1)$，目标点坐标 $P_2(x_2, y_2)$，且 $\Delta x = |x_1 - x_2|$, $\Delta y = |y_1 - y_2|$。

| 模式 | 判定条件 (IsNeighbor) | 几何意义 |
| :--- | :--- | :--- |
| **禁止斜连** (4-Direction) | $(\Delta x + \Delta y) == 1$ | 仅上下左右相邻。 |
| **允许斜连** (8-Direction) | $\max(\Delta x, \Delta y) == 1$ 且 !($\Delta x==0$ && $\Delta y==0$) | 周围8个格子均可连。 |

### 2.3 核心交互流程 (FSM)

```mermaid
graph TD
    Idle[空闲状态] -->|PointerDown (且击中目标点)| Dragging[连线中]
    Dragging -->|PointerEnter (进入新格子)| Check{判定逻辑}
    Check -->|合法新格子| AddPath[加入路径/播放音效]
    Check -->|回退到上一个格子| RemovePath[移除队尾/回退音效]
    Check -->|非法/不相邻| Ignore[忽略]
    AddPath --> Dragging
    RemovePath --> Dragging
    Dragging -->|PointerUp| Settlement[结算]
    Settlement -->|路径包含所有Target| Win[胜利/下一关]
    Settlement -->|失败| Reset[重置路径]
    Win --> Idle
    Reset --> Idle
```

---

## 3. 技术架构 (Technical Architecture)

### 3.1 目录结构规范

```text
Assets/
├── Scripts/
│   ├── Core/           # 核心逻辑
│   │   ├── GameManager.cs      # 总控，关卡加载，胜负判定
│   │   ├── GridSystem.cs       # 网格生成，坐标计算
│   │   └── InputController.cs  # 触摸事件处理
│   ├── Data/           # 数据结构
│   │   ├── LevelData.cs        # ScriptableObject 关卡配置
│   │   └── CellData.cs         # 单个格子数据模型
│   ├── UI/             # 表现层
│   │   ├── GridCellView.cs     # 单个格子UI表现
│   │   └── LineRendererUI.cs   # 连线绘制 (UILineRenderer)
└── Resources/
    ├── Levels/         # 关卡数据文件
    └── Prefabs/        # UI预制体
```

### 3.2 关键类设计

#### A. `LevelData` (ScriptableObject)
用于配置关卡数据，便于策划直接在 Inspector 中编辑。
```csharp
[CreateAssetMenu(fileName = "Level_01", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject {
    public int rows;
    public int cols;
    public bool allowDiagonal;
    public List<Vector2Int> targetPositions; // 标记哪些格子是"皮蛋"
    public List<Vector2Int> obstaclePositions; // 标记哪些是障碍
}
```

#### B. `GridSystem`
负责根据 `LevelData` 动态生成 UI。

*   **动态适配算法**：
    在 `Start` 或 `Init` 中执行，确保网格在 `GameContainer` (RectTransform) 中居中且最大化显示。
    ```csharp
    public void SetupGrid(int rows, int cols, RectTransform container) {
        float width = container.rect.width;
        float height = container.rect.height;
        // 计算 CellSize (考虑 Padding 和 Spacing)
        // ... (此处保留原有逻辑) ...
        gridLayout.cellSize = new Vector2(finalSize, finalSize);
    }
    ```
*   **坐标转换**：需要提供 `GridIndex -> WorldPosition` 的方法供 LineRenderer 使用。

#### C. `LineRendererUI`
由于 Unity 原生 `LineRenderer` 在 UI 上层级排序困难，建议使用 **UI Extensions** 库中的 `UILineRenderer` 或通过重写 `OnPopulateMesh` 自定义 UI 组件。
*   **功能**：实时接收 `List<Vector2> pointPositions` 并绘制折线。
*   **优化**：拐角处需圆角 (Corner Rounding) 处理以提升手感。

---

## 4. 详细功能实现指南

### 4.1 输入处理 (InputController)

不建议在每个格子挂 EventTrigger，性能较差且逻辑分散。建议在 `GridLayer` 挂载一个统一的监听脚本，或者使用 **Raycast** 方式。

**方案推荐：统一 Raycast**
1.  **PointerDown**: 记录 `IsDragging = true`。发射射线检测是否击中 `GridCellView`。如果是 Target，初始化 Path。
2.  **PointerDrag (Update)**: 每一帧发射射线检测当前鼠标/手指下的 UI 物体。
    *   如果是 `GridCellView` 且与 `LastCell` 不同 -> 触发 `TryConnect(current, next)`。
3.  **PointerUp**: `IsDragging = false` -> 触发 `CheckWinCondition()`。

### 4.2 连线视觉反馈

*   **头节点**：显示高亮光圈。
*   **路径线**：具有宽度的实线，颜色建议鲜艳（如橙色/青色）。
*   **已访问节点**：当路径经过某个 Target 时，该 Target 播放并在状态上变为“已激活”。
*   **回退效果**：路径缩短时，被取消激活的节点需要恢复原状。

### 4.3 胜利判定
```csharp
public void CheckWin() {
    // 1. 检查路径长度是否等于所有目标点数量 (简单的判定，如果必须填满可以使用 targetCount)
    if (pathStack.Count == totalTargetCount) {
        Debug.Log("Game Win!");
        // 播放胜利特效 -> 加载下一关
    } else {
        // 播放失败抖动动画 -> 重置路径
        ResetPath();
    }
}
```

---

## 5. 扩展与优化 (Future Scope)

1.  **多色连线 (Flow Free 模式)**：扩展 `CellType` 支持多种颜色 ID，必须同色相连。
2.  **倒计时模式**：增加时间限制。
3.  **撤销步数限制**：增加策略性。
4.  **特效增强**：
    *   连线时增加粒子拖尾。
    *   连通时格子有弹力 (PunchScale) 动画。

---
**文档版本**: v1.1
**最后更新**: 2025-12-27
