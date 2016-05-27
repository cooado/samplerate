LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)
LOCAL_MODULE := libsamplerate
# LOCAL_SRC_FILES := libluajit.a
# include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_FORCE_STATIC_EXECUTABLE := true
LOCAL_MODULE := samplerate
LOCAL_C_INCLUDES := $(LOCAL_PATH)/src
LOCAL_LDLIBS := -llog

LOCAL_CPPFLAGS := -03 -ffast-math
LOCAL_SRC_FILES := ./src/samplerate.c \
				   ./src/src_linear.c \
				   ./src/src_sinc.c \
				   ./src/src_zoh.c \

# LOCAL_WHOLE_STATIC_LIBRARIES += libluajit
include $(BUILD_SHARED_LIBRARY)