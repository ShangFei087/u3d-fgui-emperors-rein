LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)
LOCAL_MODULE := pag_unity_gl_bridge
LOCAL_SRC_FILES := PagUnityGlBridge.cpp
LOCAL_LDLIBS := -llog -lGLESv2
include $(BUILD_SHARED_LIBRARY)
