# RagnarokInfo
## Note: As of 07/04/2021 - I will no longer be maintaining this application regularly due to the implementation of EasyAntiCheat which prevents this application from using standard memory inspection methods to retrieve the necessary data for processing from in-game memory. The business logic of this application is still valid; however, with no access to the data required from game memory, this application will receive updates sparingly and only with large changes. The code is functional but a single line is omitted and for the reasons stated above will not publicly provide it. I provide this for educational purposes only.
### Description
This is a working prototype application to read Ragnarok Online game data from process memory currently is designed to function only with International Ragnarok Online.
This application is currently able to read the following information from process memory:
  - User account ID #
  - Current character name
  - Current Base/Job levels are the logged in character
  - Current Base/Job experience point values.
  - Current pet name (if out)
  - Current pet hunger status (if out)
  - Current homunculus name (if out)
  - Current homunculus hunger status (if out)

Given these data points this application is currently able to extrapolate in addition to the above:
  1. Calculate the amount of experience points gained.
  2. Calculate the amount of experience points gained per hour.
  3. Calculate the amount of experience points remaining to advance to the next Base/Job level.
  4. Play a 'dinner bell' sound byte at a user specified hunger level for both Pet and Homunculus.
  5. Pause all real-time calculations if a player logs out, and will resume when the player logs back in on the same character.
  6. Supports the ability to switch viewing of multiple clients for those who 'multi-client'
  
### Built with
- Visual C#
- .NET 4.5.2
- WPF
- Extended.Wpf.Toolkit NuGet Package
- Fody NuGet Package
- Costura.Fody NuGet Package
- WindowsAPICodePack NuGet Package

### Notable features
  - Reads a single address from .XML file that is user editable and calculates offsets to memory locations of data necessary for core functionality of the software. This streamlines the memory hunting process whenever client updates are pushed by the game developer/publisher.
  - Does not use any techniques that could be leveraged to provide an unfair advantage in competitive aspects, and is mainly designed as a quality of life improvement for features missing from the base game.
  - Coded as a Windows Presentation Foundation application targetting .NET 4.5.2 making it compatible with Windows 7 and higher operating systems.
  
### Planned features
  1. Chat display and forwarding feature to potential mobile client
  2. Forwarding of Pet/Homunculus hunger notification to potential mobile client

### Still to do
  1. Refactor source code to be more readable and follow proper Visual C# programming practices, separate interaction and business logic
  2. UI refinements, more customizability of the display window
  3. Potentially migrate to a UWP application for Windows 10 only

### Screenshots
| Start Menu |
| :--: |
![Start Menu](./Screenshots/007.jpg)


| Main Window |
| :--: |
![Running](./Screenshots/001.jpg) ![Logged Out](./Screenshots/003.jpg) 
![Running with EXP Calc](./Screenshots/005.jpg)

| Pet Window |
| :--: |
![Running](./Screenshots/002.jpg) ![Inactive](./Screenshots/004.jpg) 

| Settings Window |
| :--: |
![Default](./Screenshots/006.jpg)
