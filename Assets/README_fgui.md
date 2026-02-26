包的发布文件名。这个文件名与包名称可以不同，如果这里留空，那么使用包的名称。当我们载入包时，需要使用这里设定的文件名，而当创建对象时，需要使用包名称。例如
    //这里的file_name是发布的文件名。
    UIPackage.AddPackage('file_name'); 

    //这里的Package1是包的名称。
    UIPackage.CreateObject("Package1','Component1'); 
	
	
	
	
GameRes
	
	
/A/Console/fguis/Console/
/A/Console/ABs/C.png
/A/Console/Prefabs/C.prefab









# fgui框架：

* 将fgui的每个包的所有资源打成一个ab包
* 通过文件夹路劲，来加载ab包（"Assets/GameRes/Games/Console/FGUIs"）
* ui基类使用 Window, 在window的基础上做个基类 WindowBase