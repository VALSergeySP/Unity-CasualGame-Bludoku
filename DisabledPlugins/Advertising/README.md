# Disabled advertising plugins

YsoNetwork, its MAX adapter, and YNDependencies.xml are stored outside Assets
so Unity does not import them or include them in Android/iOS builds.

The active Android Gradle templates no longer declare AppLovin MAX, its mediation
adapters, their helper dependencies/repositories, or the old advertising ID dependency.
The stale MAX scripting define and explicit AD_ID permission were removed.

To verify the change, create a new Android build using Unity's Clean Build option.
Previously generated Gradle projects and already-built APK/AAB files still contain
the old dependencies until rebuilt.

Restoring these files to Assets requires intentionally restoring/configuring their
SDK dependencies as well.