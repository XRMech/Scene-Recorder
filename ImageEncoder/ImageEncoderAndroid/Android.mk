LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

LOCAL_MODULE := ImageEncoder
LOCAL_SRC_FILES := ImageEncoderAndroid.cpp

LOCAL_C_INCLUDES := $(LOCAL_PATH)/include

LOCAL_LDLIBS := -llog -landroid

include $(BUILD_SHARED_LIBRARY)
