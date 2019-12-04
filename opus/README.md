Currently using libopus 1.3.1 (released Apr 12, 2019). Download source from `https://opus-codec.org/downloads/` and drop `opus` directory in here.

It's important that opus is built with floating point support. Fixed point is not used.

## Windows

Build dlls using the VS2015 solution.

## Android

Build `.so` libraries using the Ndk with the following command:

`ndk-build APP_BUILD_SCRIPT=Android.mk NDK_APPLICATION_MK=Application.mk NDK_PROJECT_PATH=build`

(Android.mk taken from https://github.com/xuan9/Opus-Android)
