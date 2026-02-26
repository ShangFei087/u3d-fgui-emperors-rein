复制G300中的MachineDataG300Controller代码 改名以及空间名   

在你们各自的预制体控制器中 我将Mock改名为DataController  将你们新建的代码附上去（原来的代码取消掉）

然后在你们项目工程中主游戏面板（PageGameMain）逻辑将MachineDataG200Controller改为你们自己新建的代码名称

在你们新建的的MachineDataGXXXXController代码中去修改模拟中奖数据文件路径  比如mock中Assets/GameRes/_Common/Game Maker/_Mock/Resources/g4000/g4000__slot_spin__00.json

你们的JP中奖动效应该是fgui中的 我已帮你们修改代码 只需去各自的customModel中修改对应Appear 和 Hit 动效路径

你们各自的slotmach控制器中 取消了Reel4000 4001 统一用Reel01     SymBol同理    （我已处理）
