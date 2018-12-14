// modified from https://github.com/ChrisMaire/unity-native-sharing

#import "iOSNativeShare.h"

@implementation iOSNativeShare {
}

extern UIViewController* UnityGetGLViewController();

-(id) initWithFilePath:(char*)filePath {
	self = [super init];
	if( !self ) return self;

    NSString *mFilePath = [[NSString alloc] initWithUTF8String:filePath];
	NSURL *formattedURL = [NSURL fileURLWithPath:mFilePath];
	NSMutableArray *items = [NSMutableArray new];
	[items addObject:formattedURL];

	UIActivityViewController *activity = [[UIActivityViewController alloc] initWithActivityItems:items applicationActivities:Nil];
	[activity setValue:@"" forKey:@"subject"];

	UIViewController *rootViewController = UnityGetGLViewController();
    // if iPhone
    if (UI_USER_INTERFACE_IDIOM() == UIUserInterfaceIdiomPhone) {
          [rootViewController presentViewController:activity animated:YES completion:Nil];
    } else { // if iPad
        // Change Rect to position Popover
        UIPopoverController *popup = [[UIPopoverController alloc] initWithContentViewController:activity];
        [popup presentPopoverFromRect:CGRectMake(rootViewController.view.frame.size.width/2,
												 rootViewController.view.frame.size.height/4, 0, 0)
			inView:rootViewController.view permittedArrowDirections:UIPopoverArrowDirectionAny animated:YES];
    }
    return self;
}

# pragma mark - C API
iOSNativeShare* instance;

void showSocialSharing(char* filePath) {
	instance = [[iOSNativeShare alloc] initWithFilePath:filePath];
}

@end
