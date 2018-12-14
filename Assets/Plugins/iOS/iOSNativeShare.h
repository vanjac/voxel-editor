// modified from https://github.com/ChrisMaire/unity-native-sharing

#import "UnityAppController.h"

@interface iOSNativeShare : UIViewController {
    UINavigationController *navController;
}

#ifdef __cplusplus
extern "C" {
#endif

    void showSocialSharing(char* filePath);

#ifdef __cplusplus
}
#endif

@end
