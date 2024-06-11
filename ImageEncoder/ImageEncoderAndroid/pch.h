#include <jni.h>
#include <errno.h>

#include <string.h>
#include <unistd.h>
#include <sys/resource.h>

#include <android/log.h>

// Include standard C++ headers
#include <cstddef>   // For size_t
#include <cstdint>   // For fixed-width integer types
#include <thread>    // For std::thread
#include <iostream>  // For std::cout and std::cerr (if used for debugging)
#include <exception> // For std::exception
