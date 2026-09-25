Contributor Guide
=================

Prerequisites
-------------
To build the plugin, you'll need .NET SDK 8.0 or later.

Pull requests
-------------
Open PRs against `main`. The GitHub PR template asks for a summary, Tasks.md id, and a short test plan. Prefer one task (or a small coherent slice) per PR.

Build
-----
Use the following shell command. It will build a plugin ZIP archive in `build/distributions`.
```console
$ ./gradlew :buildPlugin
```

### Run local IDE
To run a test instance of Rider with your plugin, use the following shell command:
```console
$ ./gradlew :runIde
```

Test
----
To run the tests, use the following shell command:
```console
$ ./gradlew :check
```

Upgrade Rider Version
---------------------
To upgrade the IDE version targeted by the plugin, follow these steps.

1. Update the `riderSdkVersion` in the `gradle.properties`.
2. Update the `kotlin` version in the `versions` section of the `gradle/libs.versions.toml` (see the comment there for the link to the corresponding documentation).
