# Image Encoder DLL

This project is a DLL (Dynamic Link Library) for encoding images into various formats (JPEG, PNG, BMP). It uses the stb_image_write library for handling the image encoding.

## Table of Contents

- [Installation](#installation)
- [Usage](#usage)
- [Functions](#functions)
- [Dependencies](#dependencies)
- [License](#license)

## Installation

To build the project, follow these steps:

1. Clone the repository.
2. Open the solution in Visual Studio.
3. Build the project.

## Usage

Include the generated DLL in your project and use the provided functions to encode images.

### Example

#include "ImageEncoder.h"

unsigned char* imageData = // your image data
int width = // image width
int height = // image height
int channels = // number of channels (e.g., 3 for RGB, 4 for RGBA)
int quality = 90; // quality for JPEG encoding

EncodeToJPG("output.jpg", imageData, width, height, channels, quality);

## Functions
The DLL provides the following functions:

void EncodeToJPG(const char* filePath, unsigned char* imageData, int width, int height, int channels, int quality)
Encodes the provided image data to a JPEG file.

filePath: Path to save the JPEG file.
imageData: Pointer to the image data.
width: Width of the image.
height: Height of the image.
channels: Number of color channels in the image.
quality: Quality of the JPEG image (1-100).
void EncodeToPNG(const char* filePath, unsigned char* imageData, int width, int height, int channels, int stride)
Encodes the provided image data to a PNG file.

filePath: Path to save the PNG file.
imageData: Pointer to the image data.
width: Width of the image.
height: Height of the image.
channels: Number of color channels in the image.
stride: Stride (bytes per row).
void EncodeToBMP(const char* filePath, unsigned char* imageData, int width, int height, int channels)
Encodes the provided image data to a BMP file.

filePath: Path to save the BMP file.
imageData: Pointer to the image data.
width: Width of the image.
height: Height of the image.
channels: Number of color channels in the image.
ImageBuffer EncodeToJPGBuffer(unsigned char* imageData, int width, int height, int channels, int quality)
Encodes the provided image data to a JPEG buffer.

imageData: Pointer to the image data.
width: Width of the image.
height: Height of the image.
channels: Number of color channels in the image.
quality: Quality of the JPEG image (1-100).
Returns an ImageBuffer struct containing the encoded JPEG data.

void FreeBuffer(ImageBuffer buffer)
Frees the memory allocated for the image buffer.

buffer: The ImageBuffer struct to free.
Dependencies
stb_image_write.h: A single-file public domain library for writing images in various formats.
License
This project is licensed under the MIT License. See the LICENSE file for details.