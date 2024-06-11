#define STB_IMAGE_WRITE_IMPLEMENTATION
#include "pch.h"
#include <iostream>
#include <vector>
#include <thread>

extern "C" {
    struct ImageBuffer {
        unsigned char* data;
        size_t size;
    };

    void WriteToBuffer(void* context, void* data, int size) {
        ImageBuffer* buffer = (ImageBuffer*)context;
        size_t newSize = buffer->size + size;
        buffer->data = (unsigned char*)realloc(buffer->data, newSize);
        memcpy(buffer->data + buffer->size, data, size);
        buffer->size = newSize;
    }

    __declspec(dllexport) void EncodeToJPG(const char* filePath, unsigned char* imageData, int width, int height, int channels, int quality) {
        std::thread([=]() {
            try {
                stbi_write_jpg(filePath, width, height, channels, imageData, quality);
                std::cout << "Successfully wrote JPG to " << filePath << std::endl;
            }
            catch (const std::exception& e) {
                std::cerr << "Exception in EncodeToJPG: " << e.what() << std::endl;
            }
            catch (...) {
                std::cerr << "Unknown exception in EncodeToJPG" << std::endl;
            }
            }).detach();
    }

    __declspec(dllexport) void EncodeToPNG(const char* filePath, unsigned char* imageData, int width, int height, int channels, int stride) {
        std::thread([=]() {
            try {
                stbi_write_png(filePath, width, height, channels, imageData, stride);
                std::cout << "Successfully wrote PNG to " << filePath << std::endl;
            }
            catch (const std::exception& e) {
                std::cerr << "Exception in EncodeToPNG: " << e.what() << std::endl;
            }
            catch (...) {
                std::cerr << "Unknown exception in EncodeToPNG" << std::endl;
            }
            }).detach();
    }

    __declspec(dllexport) void EncodeToBMP(const char* filePath, unsigned char* imageData, int width, int height, int channels) {
        std::thread([=]() {
            try {
                stbi_write_bmp(filePath, width, height, channels, imageData);
                std::cout << "Successfully wrote BMP to " << filePath << std::endl;
            }
            catch (const std::exception& e) {
                std::cerr << "Exception in EncodeToBMP: " << e.what() << std::endl;
            }
            catch (...) {
                std::cerr << "Unknown exception in EncodeToBMP" << std::endl;
            }
            }).detach();
    }

    __declspec(dllexport) ImageBuffer EncodeToJPGBuffer(unsigned char* imageData, int width, int height, int channels, int quality) {
        ImageBuffer buffer = { nullptr, 0 };
        stbi_write_jpg_to_func(WriteToBuffer, &buffer, width, height, channels, imageData, quality);
        return buffer;
    }

    __declspec(dllexport) void FreeBuffer(ImageBuffer buffer) {
        free(buffer.data);
    }
}
