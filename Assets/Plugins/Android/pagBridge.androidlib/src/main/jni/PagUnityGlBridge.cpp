#include <GLES2/gl2.h>
#include <android/log.h>
#include <jni.h>
#include <cstdint>
#include <cstring>
#include <deque>
#include <mutex>
#include <string>
#include <unordered_map>
#include <vector>

#ifndef UNITY_INTERFACE_EXPORT
#define UNITY_INTERFACE_EXPORT
#endif
#ifndef UNITY_INTERFACE_API
#define UNITY_INTERFACE_API
#endif

typedef void(UNITY_INTERFACE_API* UnityRenderingEvent)(int eventId);

#define LOG_TAG "PagUnityGlBridge"
#define LOGI(...) __android_log_print(ANDROID_LOG_INFO, LOG_TAG, __VA_ARGS__)
#define LOGE(...) __android_log_print(ANDROID_LOG_ERROR, LOG_TAG, __VA_ARGS__)

struct GlTextureSlot {
    GLuint texture = 0;
    GLuint fbo = 0;
    int width = 0;
    int height = 0;
    bool needCreate = false;
    int pendingWidth = 0;
    int pendingHeight = 0;
    double pendingProgress = 0.0;
};

struct PendingGlOp {
    int slotId = 0;
    int eventId = 0;
    double progress = 0.0;
    std::string instanceKey = "_default";
    int width = 0;
    int height = 0;
};

static JavaVM* g_JavaVM = nullptr;
static jclass g_PagBridgeClass = nullptr;
static jmethodID g_SetupGpuMethod = nullptr;
static jmethodID g_FlushGpuMethod = nullptr;
static jmethodID g_TryChainFlushFrame0Method = nullptr;
static jmethodID g_TeardownGpuMethod = nullptr;

static std::unordered_map<int, GlTextureSlot> g_Slots;
static int g_ActiveSlotId = 0;
static std::string g_PendingInstanceKey = "_default";

static std::deque<PendingGlOp> g_OpQueue;
static std::mutex g_OpQueueMutex;

static GlTextureSlot& GetSlot(int slotId) {
    return g_Slots[slotId];
}

static GlTextureSlot* GetActiveSlot() {
    return &g_Slots[g_ActiveSlotId];
}

static void EnqueueOp(const PendingGlOp& op) {
    std::lock_guard<std::mutex> lock(g_OpQueueMutex);
    g_OpQueue.push_back(op);
    LOGI("EnqueueOp event=%d slot=%d instance=%s", op.eventId, op.slotId, op.instanceKey.c_str());
}

static bool PopOpForEvent(int eventId, PendingGlOp& out) {
    std::lock_guard<std::mutex> lock(g_OpQueueMutex);
    for (auto it = g_OpQueue.begin(); it != g_OpQueue.end(); ++it) {
        if (it->eventId == eventId) {
            out = *it;
            g_OpQueue.erase(it);
            return true;
        }
    }
    return false;
}

static void PopAllOpsForEvent(int eventId, std::vector<PendingGlOp>& out) {
    out.clear();
    std::lock_guard<std::mutex> lock(g_OpQueueMutex);
    for (auto it = g_OpQueue.begin(); it != g_OpQueue.end();) {
        if (it->eventId == eventId) {
            out.push_back(*it);
            it = g_OpQueue.erase(it);
        } else {
            ++it;
        }
    }
}

static JNIEnv* GetJniEnv() {
    if (g_JavaVM == nullptr) {
        return nullptr;
    }

    JNIEnv* env = nullptr;
    const int status = g_JavaVM->GetEnv(reinterpret_cast<void**>(&env), JNI_VERSION_1_6);
    if (status == JNI_EDETACHED) {
        if (g_JavaVM->AttachCurrentThread(&env, nullptr) != 0) {
            LOGE("AttachCurrentThread failed");
            return nullptr;
        }
    } else if (status != JNI_OK) {
        LOGE("GetEnv failed status=%d", status);
        return nullptr;
    }

    return env;
}

static void DestroySlotResources(GlTextureSlot& slot) {
    if (slot.fbo != 0) {
        glDeleteFramebuffers(1, &slot.fbo);
        slot.fbo = 0;
    }
    if (slot.texture != 0) {
        glDeleteTextures(1, &slot.texture);
        slot.texture = 0;
    }
    slot.width = 0;
    slot.height = 0;
}

static void CreateSlotResources(GlTextureSlot& slot, int slotId, int width, int height) {
    DestroySlotResources(slot);
    if (width <= 0 || height <= 0) {
        LOGE("CreateSlotResources invalid size %dx%d", width, height);
        return;
    }

    glGenTextures(1, &slot.texture);
    glBindTexture(GL_TEXTURE_2D, slot.texture);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, width, height, 0, GL_RGBA, GL_UNSIGNED_BYTE, nullptr);
    glBindTexture(GL_TEXTURE_2D, 0);

    glGenFramebuffers(1, &slot.fbo);
    glBindFramebuffer(GL_FRAMEBUFFER, slot.fbo);
    glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, slot.texture, 0);
    const GLenum status = glCheckFramebufferStatus(GL_FRAMEBUFFER);
    glBindFramebuffer(GL_FRAMEBUFFER, 0);

    if (status != GL_FRAMEBUFFER_COMPLETE) {
        LOGE("FBO incomplete status=0x%x slot=%d", status, slotId);
        DestroySlotResources(slot);
        return;
    }

    slot.width = width;
    slot.height = height;
    LOGI("CreateSlotResources ok slot=%d %dx%d tex=%u fbo=%u",
         slotId, width, height, slot.texture, slot.fbo);
}

static bool CallSetupGpuSurfaceOnRenderThread(GlTextureSlot& slot, int slotId, const std::string& instanceKey) {
    JNIEnv* env = GetJniEnv();
    if (env == nullptr || g_PagBridgeClass == nullptr || g_SetupGpuMethod == nullptr) {
        LOGE("CallSetupGpuSurface JNI not ready");
        return false;
    }

    if (slot.texture == 0 || slot.width <= 0 || slot.height <= 0) {
        LOGE("CallSetupGpuSurface invalid texture slot=%d %u %dx%d",
             slotId, slot.texture, slot.width, slot.height);
        return false;
    }

    jstring jInstanceKey = env->NewStringUTF(instanceKey.c_str());
    const jboolean ok = env->CallStaticBooleanMethod(
        g_PagBridgeClass,
        g_SetupGpuMethod,
        static_cast<jint>(slot.texture),
        static_cast<jint>(slot.width),
        static_cast<jint>(slot.height),
        jInstanceKey);

    if (jInstanceKey != nullptr) {
        env->DeleteLocalRef(jInstanceKey);
    }

    if (env->ExceptionCheck()) {
        env->ExceptionDescribe();
        env->ExceptionClear();
        LOGE("CallSetupGpuSurface JNI exception slot=%d", slotId);
        return false;
    }

    LOGI("CallSetupGpuSurfaceOnRenderThread ok=%d slot=%d tex=%u instance=%s",
         ok == JNI_TRUE, slotId, slot.texture, instanceKey.c_str());
    return ok == JNI_TRUE;
}

static bool CallFlushGpuFrameOnRenderThread(double progress, int slotId, const std::string& instanceKey) {
    JNIEnv* env = GetJniEnv();
    if (env == nullptr || g_PagBridgeClass == nullptr || g_FlushGpuMethod == nullptr) {
        LOGE("CallFlushGpuFrame JNI not ready");
        return false;
    }

    jstring jInstanceKey = env->NewStringUTF(instanceKey.c_str());
    const jboolean ok = env->CallStaticBooleanMethod(
        g_PagBridgeClass,
        g_FlushGpuMethod,
        static_cast<jdouble>(progress),
        jInstanceKey);

    if (jInstanceKey != nullptr) {
        env->DeleteLocalRef(jInstanceKey);
    }

    if (env->ExceptionCheck()) {
        env->ExceptionDescribe();
        env->ExceptionClear();
        LOGE("CallFlushGpuFrame JNI exception progress=%f slot=%d", progress, slotId);
        return false;
    }

    return ok == JNI_TRUE;
}

static bool CallTryChainAndFlushFrame0OnRenderThread(double finishedProgress, int slotId,
                                                      const std::string& instanceKey) {
    JNIEnv* env = GetJniEnv();
    if (env == nullptr || g_PagBridgeClass == nullptr || g_TryChainFlushFrame0Method == nullptr) {
        LOGE("CallTryChainAndFlushFrame0 JNI not ready");
        return false;
    }

    jstring jInstanceKey = env->NewStringUTF(instanceKey.c_str());
    const jboolean ok = env->CallStaticBooleanMethod(
        g_PagBridgeClass,
        g_TryChainFlushFrame0Method,
        static_cast<jdouble>(finishedProgress),
        jInstanceKey);

    if (jInstanceKey != nullptr) {
        env->DeleteLocalRef(jInstanceKey);
    }

    if (env->ExceptionCheck()) {
        env->ExceptionDescribe();
        env->ExceptionClear();
        LOGE("CallTryChainAndFlushFrame0 JNI exception progress=%f slot=%d", finishedProgress, slotId);
        return false;
    }

    return ok == JNI_TRUE;
}

static bool CallTeardownGpuSurfaceOnRenderThread(int slotId, const std::string& instanceKey) {
    JNIEnv* env = GetJniEnv();
    if (env == nullptr || g_PagBridgeClass == nullptr || g_TeardownGpuMethod == nullptr) {
        LOGE("CallTeardownGpuSurface JNI not ready");
        return false;
    }

    jstring jInstanceKey = env->NewStringUTF(instanceKey.c_str());
    const jboolean ok = env->CallStaticBooleanMethod(
        g_PagBridgeClass,
        g_TeardownGpuMethod,
        jInstanceKey);

    if (jInstanceKey != nullptr) {
        env->DeleteLocalRef(jInstanceKey);
    }

    if (env->ExceptionCheck()) {
        env->ExceptionDescribe();
        env->ExceptionClear();
        LOGE("CallTeardownGpuSurface JNI exception slot=%d", slotId);
        return false;
    }

    LOGI("CallTeardownGpuSurfaceOnRenderThread ok=%d slot=%d instance=%s",
         ok == JNI_TRUE, slotId, instanceKey.c_str());
    return ok == JNI_TRUE;
}

static void ProcessSetupOp(const PendingGlOp& op) {
    g_ActiveSlotId = op.slotId;
    g_PendingInstanceKey = op.instanceKey;
    GlTextureSlot& slot = GetSlot(op.slotId);
    if (slot.fbo != 0) {
        glBindFramebuffer(GL_FRAMEBUFFER, slot.fbo);
    }
    CallSetupGpuSurfaceOnRenderThread(slot, op.slotId, op.instanceKey);
    glBindFramebuffer(GL_FRAMEBUFFER, 0);
}

static void ProcessFlushOp(const PendingGlOp& op) {
    g_ActiveSlotId = op.slotId;
    g_PendingInstanceKey = op.instanceKey;
    GlTextureSlot& slot = GetSlot(op.slotId);
    const double progress = op.progress;
    if (slot.fbo != 0) {
        glBindFramebuffer(GL_FRAMEBUFFER, slot.fbo);
    }
    bool flushOk = false;
    if (progress >= 0.999) {
        const bool chained = CallTryChainAndFlushFrame0OnRenderThread(progress, op.slotId, op.instanceKey);
        if (!chained) {
            flushOk = CallFlushGpuFrameOnRenderThread(progress, op.slotId, op.instanceKey);
        } else {
            flushOk = true;
        }
    } else {
        flushOk = CallFlushGpuFrameOnRenderThread(progress, op.slotId, op.instanceKey);
    }
    if (flushOk) {
        // Flush 路径唯一 glFinish；native 播放状态由 Unity HandleGpuFrameReady → OnGpuFlushCompleted 回写。
        glFinish();
    }
    glBindFramebuffer(GL_FRAMEBUFFER, 0);
}

static void ProcessTeardownOp(const PendingGlOp& op) {
    g_ActiveSlotId = op.slotId;
    g_PendingInstanceKey = op.instanceKey;
    GlTextureSlot& slot = GetSlot(op.slotId);
    if (slot.fbo != 0) {
        glBindFramebuffer(GL_FRAMEBUFFER, slot.fbo);
    }
    CallTeardownGpuSurfaceOnRenderThread(op.slotId, op.instanceKey);
    glFinish();
    glBindFramebuffer(GL_FRAMEBUFFER, 0);
}

enum RenderEventId {
    kEventCreateTexture = 1,
    kEventFinishFrame = 2,
    kEventSetupPagGpu = 3,
    kEventFlushPagGpu = 4,
    kEventTeardownPagGpu = 5,
};

static void UNITY_INTERFACE_API OnRenderEvent(int eventId) {
    if (eventId == kEventSetupPagGpu) {
        std::vector<PendingGlOp> batch;
        PopAllOpsForEvent(kEventSetupPagGpu, batch);
        if (batch.empty()) {
            PendingGlOp fallback;
            fallback.slotId = g_ActiveSlotId;
            fallback.instanceKey = g_PendingInstanceKey;
            LOGE("OnRenderEvent: missing queued setup op, fallback slot=%d", fallback.slotId);
            ProcessSetupOp(fallback);
            return;
        }
        LOGI("OnRenderEvent: setup batch count=%zu", batch.size());
        for (const PendingGlOp& setupOp : batch) {
            ProcessSetupOp(setupOp);
        }
        return;
    }

    if (eventId == kEventFlushPagGpu) {
        std::vector<PendingGlOp> batch;
        PopAllOpsForEvent(kEventFlushPagGpu, batch);
        if (batch.empty()) {
            PendingGlOp fallback;
            fallback.slotId = g_ActiveSlotId;
            fallback.instanceKey = g_PendingInstanceKey;
            fallback.progress = GetActiveSlot()->pendingProgress;
            LOGE("OnRenderEvent: missing queued flush op, fallback slot=%d", fallback.slotId);
            ProcessFlushOp(fallback);
            return;
        }
        LOGI("OnRenderEvent: flush batch count=%zu", batch.size());
        for (const PendingGlOp& flushOp : batch) {
            ProcessFlushOp(flushOp);
        }
        return;
    }

    if (eventId == kEventTeardownPagGpu) {
        std::vector<PendingGlOp> batch;
        PopAllOpsForEvent(kEventTeardownPagGpu, batch);
        if (batch.empty()) {
            PendingGlOp fallback;
            fallback.slotId = g_ActiveSlotId;
            fallback.instanceKey = g_PendingInstanceKey;
            LOGE("OnRenderEvent: missing queued teardown op, fallback slot=%d", fallback.slotId);
            ProcessTeardownOp(fallback);
            return;
        }
        LOGI("OnRenderEvent: teardown batch count=%zu", batch.size());
        for (const PendingGlOp& teardownOp : batch) {
            ProcessTeardownOp(teardownOp);
        }
        return;
    }

    PendingGlOp op;
    const bool hasOp = PopOpForEvent(eventId, op);
    if (!hasOp) {
        op.slotId = g_ActiveSlotId;
        op.instanceKey = g_PendingInstanceKey;
    }

    g_ActiveSlotId = op.slotId;
    g_PendingInstanceKey = op.instanceKey;
    GlTextureSlot& slot = GetSlot(op.slotId);

    if (eventId == kEventCreateTexture) {
        const int width = hasOp ? op.width : slot.pendingWidth;
        const int height = hasOp ? op.height : slot.pendingHeight;
        if (width > 0 && height > 0) {
            CreateSlotResources(slot, op.slotId, width, height);
            slot.needCreate = false;
            slot.pendingWidth = 0;
            slot.pendingHeight = 0;
        }
        return;
    }

    if (eventId == kEventFinishFrame) {
        glFinish();
    }
}

extern "C" {

JNIEXPORT jint JNICALL JNI_OnLoad(JavaVM* vm, void* /*reserved*/) {
    g_JavaVM = vm;
    JNIEnv* env = nullptr;
    if (vm->GetEnv(reinterpret_cast<void**>(&env), JNI_VERSION_1_6) != JNI_OK) {
        LOGE("JNI_OnLoad GetEnv failed");
        return JNI_ERR;
    }

    jclass localClass = env->FindClass("com/lftlive/com/pag/PagBridge");
    if (localClass == nullptr) {
        LOGE("JNI_OnLoad FindClass PagBridge failed");
        return JNI_ERR;
    }

    g_PagBridgeClass = reinterpret_cast<jclass>(env->NewGlobalRef(localClass));
    env->DeleteLocalRef(localClass);

    g_SetupGpuMethod = env->GetStaticMethodID(
        g_PagBridgeClass,
        "nativeSetupGpuSurfaceOnRenderThread",
        "(IIILjava/lang/String;)Z");
    g_FlushGpuMethod = env->GetStaticMethodID(
        g_PagBridgeClass,
        "nativeFlushGpuFrameOnRenderThread",
        "(DLjava/lang/String;)Z");
    g_TryChainFlushFrame0Method = env->GetStaticMethodID(
        g_PagBridgeClass,
        "nativeTryChainAndFlushFrame0OnRenderThread",
        "(DLjava/lang/String;)Z");
    g_TeardownGpuMethod = env->GetStaticMethodID(
        g_PagBridgeClass,
        "nativeTeardownGpuSurfaceOnRenderThread",
        "(Ljava/lang/String;)Z");

    if (g_SetupGpuMethod == nullptr || g_FlushGpuMethod == nullptr
        || g_TryChainFlushFrame0Method == nullptr || g_TeardownGpuMethod == nullptr) {
        LOGE("JNI_OnLoad GetStaticMethodID failed");
        return JNI_ERR;
    }

    LOGI("JNI_OnLoad ok (multi-slot queued)");
    return JNI_VERSION_1_6;
}

UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API PagGl_GetRenderEventFunc() {
    return OnRenderEvent;
}

int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API PagGl_GetCreateTextureEventId() {
    return kEventCreateTexture;
}

int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API PagGl_GetFinishFrameEventId() {
    return kEventFinishFrame;
}

int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API PagGl_GetSetupPagGpuEventId() {
    return kEventSetupPagGpu;
}

int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API PagGl_GetFlushPagGpuEventId() {
    return kEventFlushPagGpu;
}

int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API PagGl_GetTeardownPagGpuEventId() {
    return kEventTeardownPagGpu;
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API PagGl_SetActiveSlot(int slotId) {
    g_ActiveSlotId = slotId;
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API PagGl_SetPendingInstanceKey(const char* instanceKey) {
    if (instanceKey == nullptr || instanceKey[0] == '\0') {
        g_PendingInstanceKey = "_default";
        return;
    }
    g_PendingInstanceKey = instanceKey;
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API PagGl_EnqueueCreateTexture(int slotId, int width, int height) {
    PendingGlOp op;
    op.slotId = slotId;
    op.eventId = kEventCreateTexture;
    op.width = width;
    op.height = height;
    EnqueueOp(op);
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API PagGl_EnqueueSetup(int slotId, const char* instanceKey) {
    PendingGlOp op;
    op.slotId = slotId;
    op.eventId = kEventSetupPagGpu;
    if (instanceKey != nullptr && instanceKey[0] != '\0') {
        op.instanceKey = instanceKey;
    }
    EnqueueOp(op);
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API PagGl_EnqueueFlush(int slotId, const char* instanceKey, double progress) {
    PendingGlOp op;
    op.slotId = slotId;
    op.eventId = kEventFlushPagGpu;
    op.progress = progress;
    if (instanceKey != nullptr && instanceKey[0] != '\0') {
        op.instanceKey = instanceKey;
    }
    EnqueueOp(op);
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API PagGl_EnqueueTeardown(int slotId, const char* instanceKey) {
    PendingGlOp op;
    op.slotId = slotId;
    op.eventId = kEventTeardownPagGpu;
    if (instanceKey != nullptr && instanceKey[0] != '\0') {
        op.instanceKey = instanceKey;
    }
    EnqueueOp(op);
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API PagGl_RequestCreateTexture(int width, int height) {
    GlTextureSlot& slot = *GetActiveSlot();
    slot.pendingWidth = width;
    slot.pendingHeight = height;
    slot.needCreate = true;
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API PagGl_SetPendingProgress(double progress) {
    GetActiveSlot()->pendingProgress = progress;
}

void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API PagGl_DestroyTexture() {
    GlTextureSlot& slot = *GetActiveSlot();
    slot.needCreate = false;
    DestroySlotResources(slot);
    g_Slots.erase(g_ActiveSlotId);
    LOGI("DestroyTexture slot=%d erased", g_ActiveSlotId);
}

int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API PagGl_GetPendingOpCount() {
    std::lock_guard<std::mutex> lock(g_OpQueueMutex);
    return static_cast<int>(g_OpQueue.size());
}

int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API PagGl_GetTextureId() {
    return static_cast<int>(GetActiveSlot()->texture);
}

void* UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API PagGl_GetTexturePointer() {
    return reinterpret_cast<void*>(static_cast<uintptr_t>(GetActiveSlot()->texture));
}

int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API PagGl_GetTextureWidth() {
    return GetActiveSlot()->width;
}

int UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API PagGl_GetTextureHeight() {
    return GetActiveSlot()->height;
}

}
