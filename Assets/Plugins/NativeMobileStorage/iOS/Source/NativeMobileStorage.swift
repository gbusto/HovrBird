//
//  NativeMobileStorage.swift
//  NativeMobileStorageApple
//
//  Created by Gabriel Busto on 1/2/20.
//  Copyright © 2020 Ash Games. All rights reserved.
//

import Foundation

@objc public class NativeMobileStorage: NSObject {
    
    @objc static public var shared = NativeMobileStorage()
    
    @objc public func getApplicationDirectory() -> String {
        let path = FileManager.default.urls(for: .applicationSupportDirectory, in: .userDomainMask).first!
        let fullPath = path.appendingPathComponent(Bundle.main.bundleIdentifier!, isDirectory: true).path
        
        // Check if the path has already been created
        if false == FileManager.default.fileExists(atPath: fullPath) {
            // Create it
            do {
                try FileManager.default.createDirectory(atPath: fullPath, withIntermediateDirectories: true, attributes: nil)
                print("iOS: Directory '\(fullPath)' didn't exist. Just created it!")
            }
            catch {
                print("iOS: Error: Failed to create directory '\(fullPath)': \(error)")
            }
        }
            
        print("iOS: Returning Library app support path: '\(fullPath)'")
        return fullPath
    }
}
