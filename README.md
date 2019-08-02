# RagnarokInfo
This is a working prototype application to read Ragnarok Online game data from process memory currently is designed to function only with International Ragnarok Online.
This application is currently able to read the following information from process memory:
  • User account ID #
  • Current character name
  • Current Base/Job levels are the logged in character
  • Current Base/Job experience point values.
  • Current pet name (if out)
  • Current pet hunger status (if out)
  • Current homunculus name (if out)
  • Current homunculus hunger status (if out)

Given these data points this application is currently able to extrapolate:
  1. Calculate the amount of experience points gained.
  2. Calculate the amount of experience points gained per hour.
  3. Calculate the amount of experience points remaining to advance to the next Base/Job level.
  4. Play a 'dinner bell' sound byte at a user specified hunger level for both Pet and Homunculus.
  5. Pause all real-time calculations if a player logs out, and will resume when the player logs back in on the same character.
  6. Supports the ability to switch viewing of multiple clients for those who 'multi-client'
  
Notable features:
  • Reads a single address from .XML file that is user editable and calculates offsets to memory locations of data necessary for core functionality of the software. This streamlines the memory hunting process whenever client updates are pushed by the game developer/publisher.
  • Does not use any techniques that could be leveraged to provide an unfair advantage in competitive aspects, and is mainly designed as a quality of life improvement for features missing from the base game.
  
Planned features:
  • Chat display and forwarding feature to potential mobile client
  • Forwarding of Pet/Homunculus hunger notification to potential mobile client.

Still to do:
  • Refactor source code to be more readable and follow proper Visual C# programming practices
