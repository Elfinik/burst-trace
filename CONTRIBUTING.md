# Contributing to BurstTrace

First off, thanks for taking the time to contribute!

## Development Environment
* Project allows Unity 2022.3 LTS or newer.
* Ensure the **Burst** package is installed and enabled.

## Submitting Pull Requests 
1. Ensure all tests pass. 
2. Do not bump the version number in `package.json`. 
3. Follow the existing code style (brackets on new lines, clear variable names). 
4. Comment on the new code whenever possible.

## Reporting Issues / Bugs
When creating an issue, please use the provided templates. If reporting a crash, please attach the full Editor.log or Player.log.
Please include:
* Unity version.
* Burst package version.
* Platform (Editor/Windows/Android/etc).
* A minimal code snippet to reproduce the issue.


## Running Tests
This project relies on Unity Test Runner for verification.
All tests are located in the `Tests` folder and run in **PlayMode**.
To run tests, clone the repository and open it as a Unity project. The tests are inside the `Tests` folder.


1. Open your Unity Project.
2. Go to **Window > General > Test Runner**.
3. Select the **PlayMode** tab.
4. Click **Run All**.
   * *Note:* Ensure that `Jobs > Burst > Enable Compilation` is checked to test the plugin in real Burst conditions.