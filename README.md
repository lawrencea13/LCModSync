# LCModSync
a mod that will allow you to sync host mods with client mods in Lethal Company

# Requirements
1. Bepinex
2. Host and client need to at least have this mod installed
3. Internet :)

# How to implement into your mod
1. Download and add as a reference
2. create a public method called "modURL" with ModSyncPlugin as a parameter
3. call getModURLandName(string URL, string modName)
4. Note that at the moment URL and modName should NOT have any spaces. URL should be a direct link to the DLL and modName should be formatted as "modname.dll" (caps are fine, just need the .dll at the end)
