//
//  NativeMobileStorageBridge.m
//  NativeMobileStorageApple
//
//  Created by Gabriel Busto on 1/2/20.
//  Copyright © 2020 Ash Games. All rights reserved.
//

#import <Foundation/Foundation.h>
// This header should be uncommented inside of Xcode!
//#include <NativeMobileStorageApple/NativeMobileStorageApple-Swift.h>
// This header should be uncommented inside of Unity!
#include "NativeMobileStorageApple-Swift.h"

char* NSStringToCharArray(NSString *string) {
    char *return_string = NULL;
    
    const char *_string = [string UTF8String];
    size_t string_len = strlen(_string) + 1;
    
    return_string = (char *)malloc(string_len);
    if (NULL == return_string) {
        return NULL;
    }
    
    strncpy(return_string, _string, string_len);
    return return_string;
}

extern "C" {
    char* _getApplicationDirectory() {
        NSString *directory = [[NSString alloc] init];
        directory = [[NativeMobileStorage shared] getApplicationDirectory];
        return NSStringToCharArray(directory);
    }
}
