# v0.3.2 通用打字反馈与云端同步

新增未标注字母、数字、常用符号、退格、回车和 Tab 的通用敲击，三处无标签键区交替响应。保留游戏键精确定位；通用敲击优先于之前持续按住的键，动作结束后返回仍按住的游戏键。只保留短时动作强度、轮换位置与优先级，不保留输入字符或按键历史；模式关闭即清零。

最终 EXE 通过 177 项应用诊断、62 项输入/图层检查、28 项自由摆放和 27 项边缘回归。新增检查涵盖字母、主键盘/数字小键盘数字、符号、退格/Tab/回车、输入法相关虚拟按键、游戏键与通用敲击优先级、连续敲击轮换和关闭清零。通用打字的原生深色背景画面已检查。以上为方法驱动与原生合成证据；未将其称为真实键鼠自动化，具体游戏和混合 DPI 未全面覆盖。系统 Raw Input 通道沿用此前用户验证的方案。

发布副本按白名单同步 C# 源码、构建、测试、当前所需同人素材和提示词；独立编译成功。未上传官方参考集合、桌面截图、本地图库、配置或凭据。以下上传包已重新下载核验，字节数、SHA-256、全部 ZIP 条目 CRC、单应用根目录及包内 EXE 与受测文件一致。

| 文件 | 字节 | SHA-256 |
| --- | ---: | --- |
| v0.3.2/MichellePet.exe | 29203456 | 8ABFAC6BD61C475843E4962EEF9D51648D2570DD707522D6C79B5C6DFDB14B7E |
| MichellePet-v0.3.2-win.zip | 28942234 | DF33397A5AF2C3DE9F0CF5DD62406907D3530E53C137D6CF9BE89325CEB7D22A |

包内仅 EXE、使用说明、构建元数据和制作方案。package.json 的 published=false 是构建时快照，不是云端状态。正式下载入口为 GitHub v0.3.2 发行版；下方“仅本地”“未上传”等表述均是各版当时的历史记录。云端同步记录另存 releases/v0.3.2-verification.json。

# v0.3.1 边缘键定位与左右手修订

用户反馈：Ctrl、Shift 等边缘键手部够不到；鼠标右手比键盘左手大，手指位置和左右手解剖方向不对。

根因：旧手部目标把键帽 X 偏移缩小到 30% 并限制在 ±12，Y 不随键位变化；组合键取加权平均使手停在键之间；前臂和手掌用同一仿射伸缩，伸手距离会改变手掌大小。旧鼠标手素材的拇指方向也不匹配人物正面的右手。

修订：目标直接采用旋转 180° 后键帽的 XY 中心及按压偏移；以当前有效按键中最近按下者为动作目标，其他按住的键仍亮起。自动重复不抢目标，释放后回到仍按住的键。只在固定大小的内存状态中维护动作优先级，不生成输入历史或文件日志。

内置 image_gen 重新生成 hands-v3.png：同尺度的前臂、键盘左手、握鼠标右手；正面视角两只拇指朝内，右手食指和中指分别搭在鼠标两个按钮上，其他手指位于侧边。源图及完整提示词、裁切与支点见 assets/companion/hands-v3-manifest.json。手掌统一固定 .09 的场景缩放，只旋转和平移；前臂单独连接袖口和手腕，不用整只手拉伸实现伸手。未使用的新旧素材均不覆盖历史原图。人物头身及自由摆放逻辑保持。

最终 EXE 通过 177 项原生应用诊断、47 项输入/图层/键位检查、28 项自由摆放和 27 项边缘回归。新增覆盖全部 11 个可见键帽落点、W 持续时新增 Ctrl、重复 W 不抢 Ctrl、松开 Ctrl 返回 W、右 Shift 和模式关闭清零。检查了 Ctrl/Shift/Space 代表画面与原生深浅背景。证据位于本机 TEMP/michelle-v031-final，不公开桌面截图。这些自动检查不冒充物理键鼠；系统输入读取通道沿用 v0.3.0 已由用户验证的版本，当前新增的动作优先级通过状态回归验证。

| 文件 | 字节 | SHA-256 |
| --- | ---: | --- |
| releases/v0.3.1/MichellePet.exe | 29202944 | 5BDA2618F4731C68609D4092D40CAA28BFB232A9FF56E7A519C3D7709EFF8B18 |
| releases/MichellePet-v0.3.1-win.zip | 28944076 | 1437AED5893D6B42844CB1E26A53E7815FD44671CAA0E40A632C29E40E6AB292 |

ZIP 全条目 CRC、单应用根目录、包内 EXE 与受测 EXE 一致及本地 Markdown 链接均通过。包仅包含 EXE、使用说明、package.json 和制作方案文档。旧版保留，切换本地 v0.3.1/MichellePet.exe --companion；本次未推送 GitHub。核验记录 releases/v0.3.1-verification.json。


# v0.3.0 游戏搭子本地版 · 2026-09-17

新增经典制服游戏搭子：身体固定、左手敲键、右手操作鼠标，W/A/S/D、Q/E/R/F、Shift/Ctrl/Space 与鼠标相对移动、左右键独立响应。Raw Input 仅在模式与联动开启时注册；关闭/退出/暂停、拖动与菜单期间注销并清零。键位状态仅保留内存，不转换或记录输入文本，不上传。短时动画脉冲保留快速轻按；重复按下不反复重启动作；双侧修饰键分别维护。

根据用户即时反馈修订：键盘整体旋转 180°，字面朝人物、空格靠近人物；替换鼠标手为握持真实鼠标的新图，前臂按肩袖和手掌接触点合成。游戏搭子松手原地停留，不下落、不自动贴边，点击回应保持原位置；普通桌宠仍保留重力与收纳。三档大小沿用。新增 --companion 参数可直接进入该模式。此版是分层 2D，尚无 Live2D 网格或逐手指绑定。

素材来自内置 image_gen，原图、完整提示词与运行时裁切/支点在 assets/companion。classic-atlas.png 取身体与键盘前臂，旧鼠标前臂未使用，现用 mouse-arm-v2.png；新素材保留原始 RGBA。现有四皮肤资源及透明修复不变。

最终 EXE 验收：177 项消息循环/素材/原生窗口诊断、27 项边缘回归、31 项输入解码/状态/像素回归、28 项自由摆放回归通过。后者覆盖 160/240/360 各三处（屏幕中间、左右边缘）释放后至少 650ms 原地停留，以及点击不移位和退回普通模式后恢复落地。头部/身体区域输入前后像素不变，手与键区有变化。最终原生深浅背景与内存图在本机 TEMP/michelle-v030-final，不纳入包或公开仓库。

输入实测：NativeInputProbe 独立宿主加载受测 EXE，在空白前台窗口通过 Windows Computer Use 输入 W＋Shift、Ctrl＋Space、鼠标左右键和移动，后台联动有效且桌宠位置固定；关闭后再次输入未响应，registration=False。右键菜单实点可进入游戏搭子。用户进一步提交全部指定按键和鼠标均 YES 的测试截图，确认输入功能正常。测试宿主的 Mode 标签曾使用独立布尔标志，导致从桌宠菜单重新开启后误报 OFF；现直接读取桌宠实际模式，已编译验证。此问题不表示正式程序关掉模式后仍注册输入。

方法驱动测试不冒充真实拖拽。用户反馈与普通窗口输入实测不能覆盖所有游戏、独占全屏、反作弊环境、多屏混合 DPI 或长期稳定性。新鼠标手与自由摆放修订后重新跑完最终包自动回归及原生画面检查；键鼠读取逻辑未变。

| 文件 | 字节 | SHA-256 |
| --- | ---: | --- |
| releases/v0.3.0/MichellePet.exe | 29697536 | 65B17690778D669411D6E8CFF01E503CA3FC305606560EA96FEE3F6D54292C96 |
| releases/MichellePet-v0.3.0-win.zip | 29444135 | 70E8609A6FAAEAF712A13C67BA48DE9DDE99FC82D7C9206F8306D5A25D004224 |

ZIP 全条目 CRC、单应用根目录、包内 EXE 与受测文件逐字节一致、使用说明本地链接均通过。白名单仅 EXE、使用说明、package.json 和制作方案文档，不含测试宿主、桌面截图、官方参考、个人配置或图库。核验记录 releases/v0.3.0-verification.json。旧版本保留，v0.3.0 源码与包仅本地；GitHub 仍是此前公开的 v0.2.2。

# GitHub 首次同步 · 2026-09-17

仓库：https://github.com/turnsolesama/michelle-pet 。初始源码提交 e57a8f9；首次公开版本 v0.2.2，键鼠敲击与 Live2D 仍为计划，未启动实现。

公开内容为 C# 源码、构建脚本、诊断与边缘回归、16 张构建所需同人素材、提示词、当前素材清单及文档。官方参考集合、桌面截图、本地预览图库、临时分析文件与个人配置未上传。独立发布工作副本在本地 work/michelle-publish-20260917，原工作目录及历史包保留。

原始 v0.2.2 ZIP 上传为草稿后重新完整下载，核对 26,232,476 字节、SHA-256 CCE6DA58B2B8765745EB206E6FF064CDEE7AFDCEE8E7F4583147B4D480A0C7C1、所有 ZIP 条目 CRC、单应用根目录以及包内 EXE 与受测文件逐字节一致；通过后才转为正式 Release。未重新打包，包内状态字段保留构建时记录。公开源码白名单单独构建成功，Markdown 本地链接有效。

版本页：https://github.com/turnsolesama/michelle-pet/releases/tag/v0.2.2 。本地下载核验记录：work/michelle-v022-upload-verification.json。下方“未公开发布”为此前验收时的历史记录。

# v0.2.2 边缘重新吸附修复 · 2026-09-17

反馈：右侧难以直接收纳，但经过左侧后又可收纳。方法驱动实际 v0.2.1 EXE 复现：初始 Home 直接左右收纳均成功；从边缘展开后，新的短距离拖动返回原侧失败；经过中间后恢复。根因是 suppressDock 跨手势残留，而且同时封锁两侧，只有窗体完整离开左右 64 像素区域才清除。

修改：新的全身拖动开始时解除旧保护；展开时记录原侧与所属工作区；同一次拖动只阻止重新吸回原侧，经过足够内侧距离或换到其他工作区时解除。直接跨到另一边不受保护阻挡。素材、呼吸及原生呈现代码不变。

tests/EdgeRegression.cs 在真实 WinForms 消息循环中加载旧/新 EXE，通过公共鼠标路径共用的私有行为入口检查三档大小，每档 9 条路径：左右直接收纳、展开后新的拖动回原侧、左经中间到右、左右一次跨侧、向内展开松手不立即吸回。旧包 27 项中 12 项失败；新版 27 项全部通过。结果在本机临时目录 michelle-v022-edge.txt。没有把方法调用记为真实输入。

最终 EXE 原有 165 项诊断通过，证据目录 michelle-v022-final。检查右侧探头及全身原生深色画面正常。真实鼠标通过独立 UiProbe 加载新版尝试拖动；工具报告起点命中 Chrome Legacy Window，激活并重取截图后仅重试一次，仍报同错，未完成真实拖拽验收。混合 DPI、多屏、长期运行未测。

| 文件 | 字节 | SHA-256 |
| --- | ---: | --- |
| releases/v0.2.2/MichellePet.exe | 26455552 | FE87CC25F2A66B8C29370F33F1F48FC18B6007C73CFA26FD243307A93FC53E70 |
| releases/MichellePet-v0.2.2-win.zip | 26232476 | CCE6DA58B2B8765745EB206E6FF064CDEE7AFDCEE8E7F4583147B4D480A0C7C1 |

ZIP 全条目 CRC 通过，单应用 v0.2.2 根目录，仅 EXE、说明、package.json；包中 EXE 与受测程序逐字节一致。保留旧包，关闭验收进程并切换普通 v0.2.2 入口。源码未提交，未公开发布，截图与私人数据未打包。

# v0.2.1 修复验收 · 2026-09-17

用户反馈三项：能收纳但没有自然贴墙探头；其他服装马尾与身体间白底挡住桌面；待机左右晃动。

- 贴墙：替换原横框图旋转 90 度的呈现，四套衣服新建 4 张 RGBA 双姿势图，共 8 个正立左右探头姿势。左右图分开裁切，不在运行时镜像；原图保留。入场 .28 秒，完成后位置固定。提示词、尺寸、哈希和接触线见 assets/skins/wall-prompts.json、wall-manifest.json 和 README.md。
- 透明：原外边界去底遗漏了封闭空隙。增加逐图背景种子，只清理连通近白背景，白色服装取样保持原像素。alpha-seeds.csv 记录原始坐标，源图未改写；wall.png 直接保留生成 Alpha。
- 稳定：原呼吸等比缩放同时改变宽度，造成发丝左右往返。改为宽度/水平中心固定、脚底固定，缓慢纵向呼吸最大 .35 像素；继续保留每帧 UpdateLayeredWindow SIZE 修复。

最终 EXE 编译无警告，165 项诊断通过，包括背景空隙、白色服装保留、整周期水平宽度/脚底固定，以及原有换装、睡眠、贴边、尺寸、拖动状态、落地和菜单检查。最终证据位于本机临时目录 michelle-v021-final：20 张内存图、20 张桌面图、20 张原生深色背景图、20 张原生浅色背景图。深色下逐张复核角色空隙与贴墙姿势，浅色代表组合另外复核。

原生截图使用实际 EXE 和真实 WinForms 消息循环；深浅背景为诊断辅助窗，并非内存图拼接。首轮右下角有其他应用浮条遮挡，最终全身截图移到屏幕中间后重新取得，遮挡图未作为最终画面依据。

真实鼠标：独立 UiProbe.cs 反射加载最终 EXE，临时启用任务栏入口以便工具选择窗口。实点右键 → 收纳到边缘 → 左侧贴墙探头，显示新姿势；实点探头后展开全身成功。一次截图 ID 失效后重新观察并重试成功。真实拖拽、混合 DPI、多屏、长期运行未覆盖，不把方法驱动拖拽当作实鼠验证。

| 文件 | 字节 | SHA-256 |
| --- | ---: | --- |
| releases/v0.2.1/MichellePet.exe | 26455552 | AF63CA72A5BC7E44B2F9E7A8E9A42C4AB95AC8D329E3084C4726C1AF787C93AE |
| releases/MichellePet-v0.2.1-win.zip | 26232179 | F2803FC6308B7D89C31FB61B9E7DFA9CDAE876C084214BA474EB9337F239E6A4 |

ZIP CRC 全条目通过，单应用 v0.2.1 根目录，仅 EXE、使用说明与校验清单；ZIP 内 EXE 与实际诊断文件逐字节一致。截图、官方参考、生成原始资料及个人配置不入包。保留 v0.1.0 / v0.2.0；源码未提交，未公开发布。关闭验收入口后切换普通 v0.2.1 入口。

# v0.2.0 本地交付检查

## 2026-09-17 原生桌面复核

沿用同一 v0.2.0 EXE，未修改应用源码、素材或历史交付包。桌面解锁后重新执行诊断，62 项全部通过；逐张检查四套皮肤的待机、开心、睡眠、左侧和右侧抓框共 20 张实际桌面区域截图，确认人物、透明背景、表情、服装和气泡刷新正常。此次结果补齐下方 9 月 16 日记录中的原生合成证据。

通过独立 UiProbe.cs 验收入口反射加载该 EXE，仅启用现有 Inspectable 标记与任务栏入口，使原生窗口能被自动化工具选中。真实鼠标点击已验证经典装开心回应、右键切换甜点美梦和进入睡眠。验收入口不进入用户交付包；默认工具窗口本身无法被该自动化工具选中。

真实拖拽未通过验收：工具报告拖拽起点落在 Explorer 桌面，激活窗口、刷新画面后重试仍失败；未把方法驱动的拖拽测试计作物理输入通过。其余换肤组合与收纳通过程序状态诊断及实际桌面截图验证，没有声称全部菜单都已逐项实点。混合 DPI、多屏和长期运行仍未覆盖。

本机证据：临时目录 michelle-native-20260917-363f392b1f4c4b3aaca57d1930421e45，含 checks.txt、20 张内存图和 20 张桌面区域图；不进入 ZIP。完成后关闭独立验收进程，启动原始 releases/v0.2.0/MichellePet.exe。现有包内说明保留历史记录，本节为后续验收补充。

## 2026-09-16 构建记录

2026-09-16。新增甜点美梦、桃心卫士、绮星梦使三款官方参考 Q 版皮肤，连同经典装共 16 张素材。接通睡眠/唤醒、左右抓框、沿边滑动、向内拖出、换肤保持姿势及三档尺寸。源码未提交，未公开发布；v0.1.0 文件和包保持原样。

| 文件 | 字节 | SHA-256 |
| --- | ---: | --- |
| releases/v0.2.0/MichellePet.exe | 26004480 | BC4320F83392B4B8D616D7315D93091249B928B0E8121FF97DA4F692FB814DDF |
| releases/MichellePet-v0.2.0-win.zip | 25792790 | FF59AAD9DF41B9E9B10B9DC76357B438C92AD8AE00944CFF90416991A2CDA96C |

最终 EXE 编译无警告，真实 WinForms 消息循环下 62 项诊断通过。覆盖四套皮肤的透明角像素、睡眠/唤醒、睡眠换肤、收纳换肤、左侧大小往返、左右贴边、沿边移动、向内拖出及展开后防止立即重新吸附；另覆盖落地速度清零、落地位置稳定、全身尺寸脚底锚点、暂停恢复与退出菜单。

运行时内存帧已目检：四套待机、睡姿和左右抓框均保持服装，抓框线已裁到接触位置。预览见 docs/v0.2.0-preview.png。所有图片的保存路径、像素尺寸与 SHA-256 见 assets/skins/manifest.json；生成提示词见 assets/skins/prompts.json。

**本轮原生桌面合成未验收通过**：诊断桌面图及 screenshot 技能整屏截图均为黑屏，同时检测到 LogonUI 进程；推断为锁屏环境限制。不能拿内存帧替代实际桌面画面。本轮也没有真实鼠标输入、混合 DPI、多屏和长期运行证据。测试通过不代表这些项目已通过。

最终诊断路径：本机临时目录 michelle-v020-final-0d7384711b214fd091313b594e5e01c1。截图只留临时目录，不进入 ZIP。ZIP 全条目可解压读取，只有单应用 v0.2.0 根目录，内容为 EXE、使用说明、校验清单。官方参考图、个人设置和本机截图不打包。

## v0.1.0 历史记录

2026-09-16。新增独立 michelle-pet 应用，未修改原 codex-pet，源码未提交，未公开发布。

最终入口：releases/v0.1.0/MichellePet.exe。已启动该确切路径。

| 文件 | 字节 | SHA-256 |
| --- | ---: | --- |
| MichellePet.exe | 3385344 | 20C1C27A8F7A3CAB551EF9165E940EB3239FBA2AE4E781DE31353370AB6EF57A |
| MichellePet-v0.1.0-win.zip | 3353303 | 07870A2AF2864114002DED88AD647ED7AD06B091B5AA3F07B8814D915A92C99D |

系统 C# 编译通过。最终 EXE 在真实 WinForms 消息循环中通过 8 项诊断：透明角像素、落地速度清零并停在工作区、落地后一秒原生窗口位置稳定、160/360/240 尺寸脚底锚点、主动回应恢复动画、退出项位于菜单底部。

检查待机和开心的实际桌面区域截图，确认原生合成显示人物、背景透明及表情/气泡刷新；内存透明图另外确认白色衣服保留。证据位于本机临时目录 michelle-pet-v010-check 与 michelle-pet-v010-check-final，未放入交付包。源图仍可能在细小闭合发丝间隙留有白底，未宣称精细 Alpha 完成。

ZIP 全部条目成功读取，单一 v0.1.0 根目录，仅 EXE、使用说明和包校验清单；官方参考、桌面截图、个人配置均不进入包。

限制：诊断通过方法驱动，不等同于真实鼠标点击/拖拽验收；本次未覆盖真人操作、混合 DPI、多显示器长期运行。当前为两帧加整体变换动画，未制作睡姿、走路、换装或抓框。造型由默认选项产生，待用户体验确认。

本次交付实际切换入口：releases/v0.3.0/MichellePet.exe --companion，旧 v0.2.2 进程已退出，历史包保留。


## English edition v0.3.2-en — 2026-09-17

- Local English edition: translated menus, tray text, speech bubbles and application errors; added an English guide and build switch. Shared behavior and artwork are unchanged. English outfit labels are descriptive fan translations.
- Final English EXE: 177 diagnostic assertions, 62 input/render checks, 28 free-placement checks, 27 edge checks and 34 English layout/menu checks passed on Windows. The 160-pixel speech bubbles fit a single line. Native menu, outfit submenu, awake/sleep bubbles and companion screenshots were reviewed locally.
- EnglishLayoutRegression loads the release EXE and exercises menu actions through methods. Diagnostics capture actual native composition; these results do not claim new physical-input, game, mixed-DPI, multi-monitor or long-duration coverage.
- Delivery: separate English runtime and full-project ZIPs; SHA-256, ZIP CRC, single-root structure, source manifest and embedded EXE identity verified. Existing published Chinese ZIPs retained unchanged. Desktop captures and harness executables excluded.
- Run `releases/v0.3.2-en/MichellePet.exe`; exit another edition first. No desktop shortcut or running installation was replaced. English packages are local only, not published to GitHub.


## English edition v0.3.2-en-r1 — 2026-09-17

- Corrected the English character name to Michele in the title, tray, greeting, startup error, English guide and program/package filenames. This r1 build shares all behavior and artwork with the previous English edition.
- Final English EXE: 177 diagnostic assertions, 62 input/render checks, 28 free-placement checks, 27 edge checks and 34 English layout/menu checks passed on Windows. The 160-pixel speech bubbles fit a single line. Native menu, outfit submenu, awake/sleep bubbles and companion screenshots were reviewed locally.
- EnglishLayoutRegression loads the release EXE and exercises menu actions through methods. Diagnostics capture actual native composition; these results do not claim new physical-input, game, mixed-DPI, multi-monitor or long-duration coverage.
- Delivery: separate English runtime and full-project ZIPs; SHA-256, ZIP CRC, single-root structure, source manifest and embedded EXE identity verified. Existing published Chinese ZIPs retained unchanged. Desktop captures and harness executables excluded.
- Run `releases/v0.3.2-en-r1/MichelePet.exe`; exit another edition first. No desktop shortcut or running installation was replaced. English packages are local only, not published to GitHub.


## v0.3.3 bilingual — 2026-09-18

- One EXE now contains Chinese and English. The permanent `语言 / Language` submenu switches text immediately; it preserves active bubbles, geometry, outfit, sleep, edge docking, companion mode, pause and input-link choices. English character name: Michele.
- Language is the only saved preference, stored under LocalApplicationData/MichelePet/language.txt. Startup defaults to Chinese, restores the saved language and supports temporary --lang=zh / --lang=en overrides. A failed preference write leaves the current session usable and displays a localized notification.
- Final release EXE passed 177 diagnostic assertions in each language, 62 input/render checks, 28 free-placement checks, 27 edge checks, 54 language/state/persistence checks and 36 English/native menu layout checks. Reviewed native Chinese/English menus and the language picker. Persistence tests use a private temporary path; diagnostics do not read or write the real user preference.
- These are method-driven state tests, actual native-composition captures and isolated file-persistence checks. No new claim of physical dragging, specific-game, anti-cheat, mixed-DPI/multi-monitor or long-run acceptance.
- Release binary: releases/v0.3.3/MichelePet.exe. Runtime and full-project packages are built from the tested EXE and reviewed public source inventory. Private screenshots, reference galleries, preferences and compiled probes are excluded. Older packages remain unchanged. Download verification is recorded locally after publication.


## v0.3.4 icon — 2026-09-19

- Added a generated Michele portrait badge based on the existing classic outfit. Preserved the generated PNG and encoded nine sizes (16, 20, 24, 32, 40, 48, 64, 128, 256) in a 32-bit DIB ICO. DIB frames avoid the .NET Framework small-PNG-icon decoding issue found during validation.
- The EXE contains the icon as a Windows resource and a managed resource. The window uses a 32-pixel portrait; the tray selects SystemInformation.SmallIconSize. Both icon objects are disposed on exit. Character animation and bilingual behavior are unchanged.
- Final EXE passed 20 icon checks: Windows shell extraction at every size matches the source ICO, and window/tray resources match the portrait. Extracted 16/32/256-pixel icons were visually reviewed. Also passed 62 input/render, 28 placement, 27 edge and 54 language/persistence checks. No new physical-drag or multi-monitor coverage claimed.
- Local delivery: releases/v0.3.4/MichelePet.exe, runtime ZIP and full-project ZIP. The existing bilingual desktop shortcut is updated to this EXE after verification; its old target is backed up. Historical packages are retained. v0.3.4 has not been published to GitHub.


## v0.3.5 companion outfits — 2026-09-19

- Both companions use rounded cream keycaps, cat-ear mouse mats and paw details. Classic has a cool blue palette; the dorm variant has warm cream/orange. Keyboard rotation and key contact geometry are preserved.
- Added a layered 2D dorm companion based on the official 喵萌元气 artwork, with independently generated bare hands. The user requested an official dorm outfit without specifying its name; this choice was communicated as an assumption. The source reference stays local and is excluded from distribution. This is fan artwork, not an official model or Live2D rig.
- The bilingual Gaming Buddy Outfit submenu switches poses while preserving location, size, pause and input-link state. Exiting restores the prior normal outfit. Companion outfit choice lasts for the session.
- The final EXE passed 62 input/render, 28 placement, 27 edge, 54 language, 39 English layout, 21 companion outfit checks and 177 diagnostic assertions. Render and native-window images were reviewed on light/dark backgrounds, including dorm sizes 160/240/360. Tests use method-driven actions and menu invocation; this does not claim new physical-input, actual-game or multi-monitor coverage.
- Local v0.3.5 delivery includes runtime and full-project ZIPs with byte/hash/CRC/source-manifest checks. Existing historical packages are retained. GitHub remains at v0.3.3; v0.3.5 has not been published.

## v0.3.6 long hair — 2026-09-19

- Reduced bulky twin-tail roots and outward curls in both Gaming Buddy body sprites, retaining long hair, existing costume/face placement, sleeves and the independent hand atlases. Original sprites remain in assets/companion; new versioned artwork and edit brief are recorded in longhair-manifest.json. Generated with built-in imagegen, original RGBA retained.
- Final EXE: 31,980,544 bytes; SHA-256 CFE5140886790EA71AF22801B3621179A1B07ADAED644287151A35721C53E9B8. Passed 62 input/render checks and 21 two-outfit checks. Reviewed classic composition and native dorm on a dark background, including style/size/placement behavior from the existing harness. These are method-driven checks, not physical input. The initial harness invocation failed because its output directory was missing; created that directory and reran successfully without application changes.
- Started the existing v0.3.5 EXE with --companion for the user's requested functional trial. Process path and responsiveness verified. v0.3.6 is separately built and ready for testing; the running v0.3.5 and its desktop shortcut were left in place to avoid interrupting that trial. No ZIP or GitHub update in this step. Normal pet sprites and icon are unchanged.

- User accepted the v0.3.6 switch: verified final EXE hash, backed up the v0.3.5 desktop shortcut under archives/michele-v036-shortcut, updated its target/icon while preserving arguments, and launched v0.3.6 with --companion. No prior pet process was running. Process path and responsiveness verified; physical interaction awaits the user's trial.

## v0.3.7 final dorm portrait — 2026-09-19

- Final accepted design: Classic Uniform uses the original classic-atlas.png with its twin ponytails. Only the dorm companion uses dorm-body-longhair.png. Both use the rounded desk, keycaps, cat-ear mouse mat and paw details. The application, tray and shortcut icon use the dorm long-hair portrait, calico sleep mask and orange-white outfit; the icon is encoded in nine 32-bit DIB sizes.
- The final EXE passed 20 icon, 62 input/render, 28 placement, 27 edge, 54 language, 39 English layout and 21 companion outfit checks, plus 177 diagnostic assertions. Embedded resources were hash-matched to original Classic, long-hair Dorm and the dorm ICO. Native icon extraction at 32/256 pixels, classic composition and the dorm desktop composition were visually reviewed. Input/state tests use method calls; actual game and mixed-DPI multi-monitor compatibility are not claimed.
- Release delivery uses a whitelist of source files and runtime resources. Official reference images, desktop evidence, personal configuration and rejected interim artwork are excluded. Runtime and full-project archives use single-app roots and carry identical final executable bytes. Checksums and source commit are recorded alongside the release packages. Original local versions remain available for rollback.
