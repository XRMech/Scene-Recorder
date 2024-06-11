#define STB_IMAGE_WRITE_IMPLEMENTATION

#include "stb_image_write.h"
#include <thread>
#include <vector>
#include <iostream>
#include <cstddef>   // For size_t
#include <cstdint>   // For fixed-width integer types
#include <jni.h>
#include <android/log.h>

#define LOG_TAG "ImageEncoder"
#define LOGI(...) __android_log_print(ANDROID_LOG_INFO, LOG_TAG, __VA_ARGS__)
#define LOGE(...) __android_log_print(ANDROID_LOG_ERROR, LOG_TAG, __VA_ARGS__)

extern "C" {
    // Custom write function to handle memory buffers
    void memory_write_func(void* context, void* data, int size) {
        std::vector<unsigned char>* buffer = static_cast<std::vector<unsigned char>*>(context);
        buffer->insert(buffer->end(), static_cast<unsigned char*>(data), static_cast<unsigned char*>(data) + size);
    }

    // Functions to encode images to memory
    std::vector<unsigned char> encode_to_jpg_memory(unsigned char* imageData, int width, int height, int channels, int quality) {
        std::vector<unsigned char> buffer;
        stbi_write_jpg_to_func(memory_write_func, &buffer, width, height, channels, imageData, quality);
        return buffer;
    }

    std::vector<unsigned char> encode_to_png_memory(unsigned char* imageData, int width, int height, int channels, int stride) {
        std::vector<unsigned char> buffer;
        stbi_write_png_to_func(memory_write_func, &buffer, width, height, channels, imageData, stride);
        return buffer;
    }

    std::vector<unsigned char> encode_to_bmp_memory(unsigned char* imageData, int width, int height, int channels) {
        std::vector<unsigned char> buffer;
        stbi_write_bmp_to_func(memory_write_func, &buffer, width, height, channels, imageData);
        return buffer;
    }

    // JNI wrapper functions to return encoded data as byte arrays
    JNIEXPORT jbyteArray JNICALL Java_com_example_myapp_ImageEncoder_encodeToJPGMemory(JNIEnv* env, jobject obj, jbyteArray imageData, jint width, jint height, jint channels, jint quality) {
        jbyte* data = env->GetByteArrayElements(imageData, nullptr);
        std::vector<unsigned char> buffer = encode_to_jpg_memory(reinterpret_cast<unsigned char*>(data), width, height, channels, quality);

        jbyteArray result = env->NewByteArray(buffer.size());
        env->SetByteArrayRegion(result, 0, buffer.size(), reinterpret_cast<jbyte*>(buffer.data()));

        env->ReleaseByteArrayElements(imageData, data, JNI_ABORT);
        return result;
    }

    JNIEXPORT jbyteArray JNICALL Java_com_example_myapp_ImageEncoder_encodeToPNGMemory(JNIEnv* env, jobject obj, jbyteArray imageData, jint width, jint height, jint channels, jint stride) {
        jbyte* data = env->GetByteArrayElements(imageData, nullptr);
        std::vector<unsigned char> buffer = encode_to_png_memory(reinterpret_cast<unsigned char*>(data), width, height, channels, stride);

        jbyteArray result = env->NewByteArray(buffer.size());
        env->SetByteArrayRegion(result, 0, buffer.size(), reinterpret_cast<jbyte*>(buffer.data()));

        env->ReleaseByteArrayElements(imageData, data, JNI_ABORT);
        return result;
    }

    JNIEXPORT jbyteArray JNICALL Java_com_example_myapp_ImageEncoder_encodeToBMPMemory(JNIEnv* env, jobject obj, jbyteArray imageData, jint width, jint height, jint channels) {
        jbyte* data = env->GetByteArrayElements(imageData, nullptr);
        std::vector<unsigned char> buffer = encode_to_bmp_memory(reinterpret_cast<unsigned char*>(data), width, height, channels);

        jbyteArray result = env->NewByteArray(buffer.size());
        env->SetByteArrayRegion(result, 0, buffer.size(), reinterpret_cast<jbyte*>(buffer.data()));

        env->ReleaseByteArrayElements(imageData, data, JNI_ABORT);
        return result;
    }
}
