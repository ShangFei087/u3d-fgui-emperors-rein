
# ====生命周期

OnInit : 初始化只调一次

OnOpen: 每次打开弹窗时调用

OnClose:  每次关闭弹窗时调用

OnTop: 页面被置顶时调用

InitParam： 初始化后，页面打开时，多语言切换时，都被调用。



# ====页面结构

Anchor/Animator

-- Background
-- Base
-- Midground
-- Foreground
-- Effect Midground
-- Effect Foreground
-- Panel

Animator
--Anchor
---- Background 背景
		BG
		NPC
---- Base
		Slot Machine (模块)
		 -- Reels
				Reel (1)
					Symbols
						Symbol Move (1)
						Symbol (1)
						Symbol (2)
						Symbol (3)
						Symbol (4)
						Symbol (5)
						Symbol (6)
						Symbol Move (2)		
				Reel (2)
				Reel (3)
				Reel (4)
				Reel (5)
				
			Slot Cover
			Pay Lines			
---- Midground
---- Foreground
---- Effect Midground
---- Effect Foreground
---- Panel (模块)




# fgui 层级

Slot BG

Slot Machine (模块)
		 -- Reels
				Reel (1)
					Symbols
						Animator : (发大特效)
							Symbol Move (1)
							Symbol (1)： 最终可见图标
							Symbol (2)： 最终可见图标
							Symbol (3)： 最终可见图标
							Symbol (4)
								image
								AnchorSymbolEffect (Hit / Appear)
								AnchorBorderEffect
							Symbol (5)
							Symbol (6)
							Symbol Move (2)		
				Reel (2)
				Reel (3)
				Reel (4)
				Reel (5)
				
			Slot Cover
			Pay Lines
				Line1
				Line2
				....

Frame
Expectation ： （火球移动，某列滚轮框特效， symbol悬浮固定）
Panel





# ==== 一局游戏概念


## 基本概念
Turn

Spin

Bonus(FreeSpin、Jackpot、MiniGame)

FreeSpin(Bouns)


## 前后逻辑


Turn

  Spin 
  
   滚轮动画 - 中奖动画
   
   
   Game Jackpot
   ...
   end Game Jackpot
   
   
   Online Jackpot
   ...
   end Online Jackpot
   
   mini Game
   ..
   end mini Game
   
   ======= 本剧数据入库
   
   Free Spin
   ... 7 Spin
   end Free Spin
  
  
  end Spin

end Turn





# ====命名规范：


## 关键字

Slot Machine
Reel： 滚轮
Frame： Reel的边框
Symbol： 图标
Border ： symbol的边框


### Jackpot

Grand
Mega
Major
Minor
Mini


### Panel
Bet: 押注
Win: 得分
Credit: 彩分
Help: 帮助
TotalWin: 总得分
PayTable： 说明页
BetMax: 最大压注
BetUp:
BetDown:


Next: 下一页
Prev: 上一页

Spin: 开玩按钮
Stop:
Auto:

Panel： 操作面板


btnExit //退出
btnPrev //上一页
btnNext //下一页
btnHelp  //帮助i
btnSound  //声音
btnPayTable  //说明页
btnBetUp  //加注
btnBetDown //减注
btnBetMax //最大注
btnSpin  //spin按钮
btnLine // 选线
btnApostar //单线彩分值
btnSwitch  //功能选择按钮


### Slot Machine

BS: 正常游戏滚轮
FS: 免费游戏滚轮

Symbol： 图标
Reel： 滚轮
PayLine： 赢线



Turn: 一局游戏
Bonus： 额外奖
FreeSpin: 免费游戏
Wild: 鬼牌/百搭
Jackpot: 彩金


Page: 不带半透遮罩，不需要其他页面作为背景
Popup：带半透遮罩，需要其他页面作为背景

PageGameLoading: 游戏加载界面
PageGameMain： 主游戏界面
PopupJackpotOnLine: 联网彩金弹窗
PopupJackpotGame: 游戏彩金弹窗
PopupBigWin：  大奖弹窗
PopupFreeSpinTrigger
PopupFreeSpinResult
PageFreeBonusGame1
PageFreeBonusGame2
PageFreeBonusGame3


# ==== 命名规范：

## U3d项目资源命名规范：
* 文件夹名： 大驼峰，可加空格
* 预制体名： 大驼峰，可加空格
* 图片、json文件、字体： 小写+下划线

## FGUI项目资源命名规范：

* 图片： 小写+下划线
* 包名： 大驼峰，不带缩写
* 文件夹名： 小驼峰
* 元件名： 大驼峰，不带缩写
* 变量名称： 小驼峰，带缩写


## 美术图片名

example_xxx.png

title_win.png
title_total_win.png
title_bet.png
title_jp1.png

btn_spin_bg.png
btn_spin_fg.png
btn_spin_border.png

icon_spin.png
icon_stop.png
icon_auto.png


icon_help.png
icon_sound.png

font_win_tip_1.png
font_win_tip_2.png


free_spin_trigger_fg.png
free_spin_trigger_bg.png

free_spin_result_fg.png
free_spin_result_bg.png

big_win_bg.png
big_win_fg.png
big_win_btn.png

pop_bg.png
pop_fg.png



# ====FUGI UI节点规范：
Anchor/Animator
---- Icon
---- Title

# ====U3D-FUGI UI节点规范：

Root
-- Example (设计图纸大小，如200x212)
-- Anchor/Animator
---- Spine
---- Effect
---- Image
---- Text

# ====Spin/粒子/3D模型 使用Wrapper(包装器)的规范：


Fgui锚点组件：
AnchorXXX
  -- holder （白色图片）
  -- example （转载器）


## AnchorXXX节点：

* 组件尺寸改为1*1（size=(1,1) 避免遮挡主面板控制器）、设置为可以点击！（避免影响子节点的点击功能）

* AnchorXXX节点，用来定位“Spin/粒子/3D模型”在页面中的位置！
【注意】： 改变“Spin/粒子/3D模型”位置时，请直接修改AnchorXXX节点在页面中的位置！

* 在Fgui场景中出现的AnchorXXX（非动C#脚本动态加载的AnchorXXX），必须单独做！保证每个AnchorXXX/example 转载器存放的“效果图”不同且“发布时清除"

## holder节点、example节点：
* holder节点、example节点；设置参数为  轴心(0.5,0.5） -- 同时作为锚点 -- 位置（0,0）

* holder设为不可见

* example转载器，设置为“自动大小”、“发布时清除”（用于清除美术效果图，减少包体大小）

* example节点，是用来装载“美术效果图的”（将spine效果图截取一帧整图，作为效果图，填入examlpe装载器中）

* example节点显示的效果，就是软件运行时，“Spin/粒子/3D模型”挂在holder节点上显示的效果！

* 允许修改example节点的“缩放”、“倾斜”属性！ 软件运行时，将复制example节点的属性（“缩放”、“倾斜”）给holder节点。
【注意】： 如果要修改“Spin/粒子/3D模型”的“缩放”、“倾斜”属性。请直接修改example节点。




fgui格式
	image 图形
	icon 装载器 (装载Spine设计图后选择自动大小)
需处理操作：
两者都需要将锚点设置为中心 ，将组件尺寸改为1*1（避免遮挡主面板控制器）图形须像装载器一样设置 然后设为不可见

unity
通过找到指定组件 通过 GameCommon.FguiUtils.AddSpine 传入指定组件 spine预制体两个参数实现挂载spine动效到指定层级（与fgui中层级相同）
spine动效会与fgui中icon的大小 缩放 等相关 可以前往fgui中改变icon的相关设置


# 版本号定义：

1.0.2  1.0.3


debug 0 2  release 1 3 5

1.0.0  1.1.0   1.2.0   1.3.0  1.4.99

1.1.X

1.3.X




# 打包




* 导出工程到当下目录
E:\work4\u3d-fgui-emperors-rein\TheOutput\ExportProject


* 拷贝4个文件夹
E:\work4\u3d-fgui-emperors-rein\TheOutput\ExportProject\unityLibrary\src\main
--assets
--Il2CppOutputProject
--jniLibs
--jniStaticLibs

* 覆盖同名文件夹
E:\work4\u3d-fgui-emperors-rein\TheOutput\TargetProject\unityLibrary\src\main



* 修改版本号：
build.gradle:

        versionCode 1000111      //   X 0-999 0-999
        versionName '1.0.111'    hofixkey  // 热更   appkey + hofixkey
		
	
* 打包	
Build/Clean Project

Generate.....

秘钥： test123456





# 推币机后台：
* 账号密码设置
* 机台号 线号
* 打码？？
* 硬件测试
* mqtt远程控制
















