# 米雪儿键鼠联动与 Live2D 制作方案

v0.3.5：两款搭子都换用圆角键帽、猫耳鼠标垫和猫爪装饰。右键 → 搭子造型，可选经典制服或喵萌元气 · 宿舍；宿舍款使用独立裸手和奶油橙鼠标。切换搭子造型保留位置、大小、暂停和联动开关，造型选择仅在本次运行有效。新增素材及支点见 assets/companion/dorm-manifest.json。以下保留早期方案和后续 Live2D 路线。

v0.3.2：未单独画出的字母、数字和常用编辑/符号键触发通用敲击；仅保留短时动作脉冲与三处轮换位置，不读取输入文本、不保存键值历史。WASD 等专用游戏键继续精确定位。

v0.3.1：键盘手按旋转后的键帽中心移动，组合键优先跟随最近按下且仍有效的键；鼠标手与键盘手改为同尺度独立手掌，前臂单独连接。手掌不再随伸手距离缩放。源图和支点见 assets/companion/hands-v3-manifest.json；经典制服仍是分层 2D，尚未逐手指绑定。

2026-09-17 更新：v0.3.0 已完成经典制服的分层键鼠版，下面保留后续 Live2D 与软件路线。当前是 WinForms 原生透明窗、固定身体与两只独立前臂；整只手按压与小范围移位，未做逐手指网格或 Live2D 模型。没有安装额外制作软件。

入口：右键 → 游戏搭子 · 经典制服，另有“键鼠联动”开关。W/A/S/D、Q/E/R/F、Shift/Ctrl/空格和鼠标相对移动、左右键已接通。Raw Input 仅维护指定游戏按键当前状态与短时动画进度；退出/关闭/暂停时注销，菜单与拖动期间暂时停止读取，不生成按键日志。后台普通窗口验收通过，游戏兼容性待实机体验。

实现依据：[RegisterRawInputDevices](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerrawinputdevices)、[RAWKEYBOARD](https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-rawkeyboard)、[RAWMOUSE](https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-rawmouse)。图集保留生成 Alpha，原图/提示词/裁切/支点见 assets/companion；键盘为程序绘制，头和身体像素在组合输入前后保持一致。

游戏搭子支持自由摆放：松手原地停留，不自动下落或吸附屏幕边缘；点击反馈也不改变位置。普通桌宠的落地与收纳保持原样。键盘按人物朝向旋转 180°，空格靠近角色，鼠标手替换为手掌搭在鼠标上的姿势。

## 可见效果

第一组建议使用经典制服，米雪儿坐在小键盘和鼠标后方。先把这套的手部接触点、按下/抬起动作和鼠标跟随调好，再扩展其他服装。

| 输入（建议，可调整） | 角色动作 |
| --- | --- |
| W/A/S/D | 左手在对应键区按下，保持时维持按压，松开回位 |
| Q/E/R/F | 左手短距离移到技能键区再按下 |
| Shift/Ctrl/空格 | 修饰键/空格对应手指动作，允许和方向键同时按住 |
| 鼠标移动 | 右手与鼠标小范围同步位移，快速移动采用平滑限幅 |
| 鼠标左右键 | 右手按压，与左手动作可同时发生 |

建议增加独立的“键鼠联动”开关；退出模式或暂停时清零手部状态。只维护约定按键的当前按下状态，不保存输入文本或按键历史，不上传。功能开关与键位设置可后续放到可视化面板。

## 两条实现路线

### 现有桌宠增加分层图片动画

继续使用当前程序，新增游戏搭子模式和独立手臂图层。优点是保留现有换装、拖动和收纳。它属于分层 2D 动画，不冒称 Live2D。身体和桌面位置固定，手臂独立运动；不可用整张人物缩放代替敲击。

键盘与鼠标均可走 Windows Raw Input 后台事件；使用鼠标相对位移，避免仅跟随桌面光标而在锁定光标的游戏中不动。需要实测前后台、组合键、持续按住、松开、鼠标点击与游戏窗口切换，不能保证所有游戏/全屏模式都兼容。

微软依据：[Using Raw Input](https://learn.microsoft.com/en-us/windows/win32/inputdev/using-raw-input)。

### 真正的 Live2D

先准备分层 PSD：脸、眼睛、眉毛、嘴、前后发、双马尾、躯干、左右上臂/前臂/手、键盘、独立按键、鼠标。关节后方被遮挡的内容需要补画，单张 PNG 不能自动变成完整绑定模型。

在 Cubism 绑定手臂位置、按压、鼠标位移、眨眼、呼吸和发丝物理；输入程序再驱动这些参数。按压与鼠标参数分别控制，避免持续按键反复重启动画或松开后手卡住。示例参数命名为本项目建议，并非软件内置规范：KeyPressW/A/S/D、SkillPress、SpacePress、MouseMoveX/Y、MouseLeft/Right。

依据：[素材拆分](https://docs.live2d.com/en/cubism-editor-manual/divide-the-material/)、[PSD 导入](https://docs.live2d.com/en/cubism-editor-manual/psd-import/)。

## 软件选择与下载入口

| 软件 | 在本项目中解决什么 | 获取 |
| --- | --- | --- |
| BongoCat（ayangweb） | 快速体验键盘、鼠标、手柄联动，支持自定义模型；模型需按其格式制作，不能直接导入当前 EXE | [项目与下载入口](https://github.com/ayangweb/BongoCat) |
| Live2D Cubism Editor | 真正的 Live2D 网格、变形器、手臂动作、表情与头发物理制作；有 FREE 与 PRO 试用 | [官方下载安装页](https://www.live2d.com/en/cubism/download/editor/) |
| CLIP STUDIO PAINT / Photoshop | 原画修整、拆层、补画和 PSD 准备；这两款的 PSD 得到 Cubism 官方兼容确认 | [CSP 下载](https://www.clipstudio.net/en/dl/)、[Cubism PSD 要求](https://docs.live2d.com/en/cubism-editor-manual/precautions-for-psd-data/) |
| VTube Studio | 运行并调试完成的 Live2D 模型；热键/鼠标输入及插件参数可用于联动，它不是模型绘制与绑定软件 | [官网](https://denchisoft.com/)、[官方文档](https://github.com/DenchiSoft/VTubeStudio/wiki)、[参数 API](https://github.com/DenchiSoft/VTubeStudio) |

建议：需要快速可用的敲键效果时先采用第一条；希望长期亲自精修动作时采用 CSP/Photoshop → Cubism → 运行宿主的流程。现有白底、穿帮、关节不自然等问题分别在素材与绑定阶段解决，安装播放器本身不能自动修好。

旧 Bongo-Cat-Mver 作者已明确不再维护该仓库，作为已有模型的参考，不作为新项目首选。依据：[作者仓库说明](https://github.com/MMmmmoko/Bongo-Cat-Mver)。
