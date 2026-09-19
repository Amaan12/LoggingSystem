# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-09-19

### Added
- Core `LoggerSO` ScriptableObject for category-based logging channels with customizable prefixes, solid colors, and per-character gradients.
- Zero-allocation friendly overloads (`Func<string>` for lazy evaluation and `Action<StringBuilder>` for buffer reuse).
- Compile-time log stripping support in player builds via the `DISABLELOGS` scripting define symbol.
- `LogService` static service locator with automatic initialization before scene load.
- `LoggerRegistrySO` ScriptableObject for managing and organizing channel assets.
- `BuildLogger` on-screen runtime GUI console monitor for development builds.
- `LogAllTest` and `LogTest` utility components and Editor menu item (`Tools > Logging > Log All Loggers`).
- Package sample containing 29 pre-configured category channels and registry asset organized in the `Resources/Logger` structure.
