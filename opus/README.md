Currently using libopus 1.3.1 (released Apr 12, 2019). Download source from `https://opus-codec.org/downloads/` and drop `opus` directory in here.

It's important that opus is built with floating point support. Fixed point is not used.

## Windows

Build dlls using the VS2015 solution.

## Mac

Run:

```
./configure
make
make install
```

Find the `.dylib` file in `/usr/local/lib`.

## Android

Build `.so` libraries using the Ndk with the following command:

`ndk-build APP_BUILD_SCRIPT=Android.mk NDK_APPLICATION_MK=Application.mk NDK_PROJECT_PATH=build`

(Android.mk taken from https://github.com/xuan9/Opus-Android)

## iOS

Run the `build-libopus.sh` script. (Based on https://github.com/chrisballinger/Opus-iOS).

Make sure to update the script when the SDK is updated.
