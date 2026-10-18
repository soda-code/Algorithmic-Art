# 算法动画上位机（Algorithmic Art）

基于 **.NET 8 + WinForms + GDI+ 双缓冲** 的算法可视化桌面应用，用于把经典算法、线性代数、机器学习的执行过程逐步渲染成动画。

## 功能概览

左侧导航采用「大类下拉 + 子分类列表」两层结构。

### 经典算法

| 分类 | 内容 |
| --- | --- |
| 查找算法 | 顺序、二分、插值、斐波那契、跳跃、指数、分块、哈希 |
| 排序算法 | 冒泡、选择、插入、归并、快速、堆 |
| 信号算法 | 信号生成、滑动平均、二阶滤波、卡尔曼滤波、FFT、DFT |
| 控制算法 | 蛇形清扫、PID 阶跃响应 |
| 机器学习算法 | K-Means、K 近邻、线性回归、梯度下降 |

### 线性代数

| 分类 | 内容 |
| --- | --- |
| 矩阵计算 | 加法、减法、数乘、相乘、转置、求逆 |
| 方程组计算 | 高斯消元、LU 分解 |
| 其他 | 幂迭代法、PCA 主成分 |

## 界面与交互

- **无边框圆角窗口**：自绘标题栏，支持拖动、双击最大化、边缘缩放
- **动画回放**：播放 / 暂停 / 单步 / 后退 / 进度拖动 / 0.5x~2x 调速
- **画布渲染器**：柱状图、波形图、频谱图、栅格地图、轨迹图、PID 曲线、散点图、矩阵表格、2D 向量
- **坐标轴**：所有画布均带横纵坐标数值刻度
- **数据配置**：数据量 10~5000、矩阵行/列 2~6
- **数据导入导出**：支持 CSV 导入（信号 / 数组 / 坐标 / 点集）与全局导出
- **界面主题**：浅灰底 + 白色圆角卡片，支持设置窗口背景图片

## 构建与运行

```powershell
# 开发运行
dotnet run --project src/AlgorithmicArt

# 发布（框架依赖，需 .NET 8 Desktop Runtime）
dotnet publish src/AlgorithmicArt/AlgorithmicArt.csproj -c Release -r win-x64 `
  --self-contained false -p:PublishSingleFile=true -o publish

# 发布（自包含，免运行时）
dotnet publish src/AlgorithmicArt/AlgorithmicArt.csproj -c Release -r win-x64 `
  --self-contained true -p:PublishSingleFile=true `
  -p:EnableCompressionInSingleFile=true -o publish-standalone
```

## 工程结构

```
src/AlgorithmicArt/
├── Core/
│   ├── Algorithm/        算法接口、步骤录制、场景构建、算法实现
│   ├── Replay/           回放引擎与状态机
│   └── Dispatch/         数据分发
├── Communication/        串口与协议解析
├── Render/               各类动画渲染器
├── Data/                 配置存储、CSV 导入、日志
└── UI/
    ├── MainForm.cs       主窗体（自绘标题栏 + 双层导航 + 状态栏）
    ├── Pages/            算法动画页
    └── Controls/         圆角按钮、圆角面板、自定义下拉、矩阵/向量画布等
```

## 性能设计

- **按需快照**：排序算法只在步骤被采纳时才克隆数组
- **自适应采样**：按「快照总量约 50MB」预算自动计算录制间隔
- **异步生成**：场景构建与步骤录制在后台线程执行，界面不卡顿
- **可取消**：切换分类或算法时通过取消令牌立即中止旧任务
- **像素聚合**：数据量超过画布像素宽度时按像素列合并绘制

## 技术栈

- .NET 8 / WinForms
- GDI+ 双缓冲自绘
- System.IO.Ports（串口）
- System.Text.Json（配置持久化）

## 许可

仅用于学习与算法演示。
