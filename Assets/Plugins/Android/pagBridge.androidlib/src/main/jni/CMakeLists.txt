cmake_minimum_required(VERSION 3.4.1)
project(pag_unity_gl_bridge)

add_library(pag_unity_gl_bridge SHARED PagUnityGlBridge.cpp)

find_library(log-lib log)
find_library(glesv2-lib GLESv2)

target_link_libraries(pag_unity_gl_bridge ${log-lib} ${glesv2-lib})
