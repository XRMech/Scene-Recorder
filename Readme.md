# Scene Stream Project

## Overview

The Scene Stream project is a comprehensive personal development initiative aimed at enhancing skills in Unity, front-end, and back-end programming. The project consists of three main components:

1. **ImageEncoder**: A DLL for encoding images into various formats.
2. **SceneStream Unity Project**: A Unity project for developing and experimenting with different features.
3. **SceneStreamServer**: A server for storing recordings, configuring, and processing final video outputs.

## Project Components

### 1. ImageEncoder

The ImageEncoder is a dynamic link library (DLL) designed to encode images into various formats such as JPEG, PNG, BMP, TGA, and HDR using the stb_image_write library.

#### Features:
- Encode images to different formats.
- Save images locally on the PC.
- Future support for Android file saving to a local network shared folder or NAS.

### 2. SceneStream Unity Project

The SceneStream Unity project is where the core functionalities of the Scene Stream project are being developed. It includes several scenes, each with different focuses.

#### Key Scenes:
- **SceneStreamTestScene**: This is the primary scene where recording capabilities and other core features are developed.
- **Other Scenes**: Used for experimenting with character design, controls, and other gameplay features.

#### Current State:
- Image saving works locally on PC.
- Android file saving needs further development.

### 3. SceneStreamServer

The SceneStreamServer is an optional server component designed to store recordings and handle the configuration and processing of final video outputs.

#### Features:
- WebRTC stream recording to the server.
- Action states storage in a database.
- Web front-end for admin configuration.
- End user access to download finished videos.

## Project Goals

The overarching goal is to develop a comprehensive and versatile camera recording tool for Unity. This tool will capture and compile video from multiple camera views, both virtual and real, triggered by in-game events. The final product aims to provide a robust solution for creating personalized, multi-view videos for immersive gaming experiences like LimitlessVR.

### Key Features:

1. **Multi-View Recording**:
    - **Simultaneous Capture**: Record video feeds from multiple in-game and real-world cameras concurrently.
    - **High Resolution**: Support up to 1920x1080 resolution at 60 FPS.

2. **Dynamic Action Event Triggers**:
    - **In-Game Events**: Automatically start, pause, or stop recording based on specific in-game actions and events.
    - **Metadata Logging**: Capture detailed metadata, including timestamps and camera identifiers.

3. **Post-Processing and Video Compilation**:
    - **Automated Compilation**: Merge recorded footage into a coherent video based on action event metadata.
    - **Advanced Editing Features**: Include transitions, effects, and overlays.

4. **Editor Configuration Tools**:
    - **User-Friendly Setup**: Intuitive tools within the Unity Editor to configure camera recording behaviors and event triggers.
    - **Customizable Settings**: Define recording parameters such as resolution, frame rate, and event trigger conditions.

5. **Performance Optimization**:
    - **Native Texture-to-Image Conversion**: Efficient texture processing to minimize main thread bottlenecks.
    - **Multi-Platform Support**: Compatibility with Windows, Android, HoloLens, and other platforms.

6. **Mixed Reality Integration**:
    - **Real and Virtual Footage**: Seamlessly integrate real-world camera footage with in-game camera views.
    - **Personalized Videos**: Generate personalized session videos showcasing gameplay from multiple perspectives.

## End Goal

The ultimate aim is to empower immersive gaming platforms like LimitlessVR to offer dynamic, personalized videos of gaming sessions. By combining multiple camera views and integrating real and virtual footage, the tool will enhance user engagement and provide a unique, high-quality video experience.

## Future Plans

- Finalize Android file saving and network storage.
- Further develop and debug the WebRTC stream recording and database storage.
- Build a web front-end for admin and user interactions.
- Continuously refine and optimize the Unity asset for potential release on the Unity Asset Store.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contact

For any questions or inquiries, please contact [Your Name](mailto:your.email@example.com).

---

This README file provides an overview of the Scene Stream project, detailing its components, goals, and current state. It serves as a comprehensive guide for understanding the project's scope and future development plans.
