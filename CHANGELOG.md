# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.2] - 2026-02-26
### Added
- Added a storage memory optimization mode: when enabled, the size of each unique log will take up approximately 3 times less space
- Added a ReadMe file for easy opening in Unity with examples for a quick start (README.asset)
- [2026-01-19] Completed documentation for: JP
- [2026-01-19] Completed documentation for: CN

### Changed
- Changing internal namespaces to avoid syntax errors
- The Unity.Collections internal access class CollectionsMemory has been moved to a separate namespace and renamed to avoid conflicts with other plugins or custom wrappers for the same purpose.

### Fixed
- Fixed a rare bug where log access was called in late OnDestroy methods when the application was stopped.

## [1.0.1] - 2026-01-13
### Added
- Improved ECS examples
- Added a check for the maximum number of logs when writing.

### Fixed
- No domain reboot mode: retrieving logs after exiting Play Mode now works correctly. Previously, it only remembered the first session.
- Fixed the Profiler.MissingCase error when triggering the log overflow check.
- Fixed OpenUPM link
- Fixed demo scene serialization

## [1.0.0] - 2026-01-11
### Added
- Initial commit