# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.3.0] - 2024-11-26
### Added
- .Net 9 as target framework
### Removed
- .Net 6/7 as target framework
### Changed
- Code maintenance

## [1.2.0] - 2023-11-17
### Added
- .Net 8 as target as framework
### Fixed
- Ambiguous use of `IPNetwork` when targeting .Net 8

## [1.1.1] - 2023-04-22
### Changed
- Replaced HasSet.Any lookup with HashSet.Contains

## [1.1.0] - 2023-04-22
### Changed
- Internal list of IPAddress to HashSet for faster lookup
- Internal list of IPNetwork to trie based IPNetworkCollection for faster lookup
- Nuget symbols file format to snupkg
### Removed
- Null-check for RemoteIPAddress: should never be null when being used with YARP
- IPFilterPolicy.BlockUnknownRemoteIP: Remote IP should never be null when being used with YARP

## [1.0.3] - 2023-04-18
### Added
- Initial release
- IPAddress based filtering (allow- and block-mode)
- IPNetwork based filtering (allow- and block-mode)
- Global- and per-route filtering