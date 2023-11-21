# LCModSync
a mod that will allow you to sync host mods with client mods in Lethal Company

# Note
Until security measures are in place, all download links to compiled releases are removed. Once everything is resolved, they will be brought back up.

# Requirements
1. Bepinex
2. Host and client need to at least have this mod installed
3. Internet :)

# How to implement into your mod
I removed the dependency requirement, now you just send a message back to my plugin when you receive it.
```cs
 public void sendModInfo()
 {
     foreach (var plugin in Chainloader.PluginInfos)
     {
         if (plugin.Value.Metadata.GUID.Contains("ModSync"))
         {
             try
             {
                 List<string> list = new List<string>
                 {
                     "GameMasterDevs",
                     "GameMaster"
                 };
                 plugin.Value.Instance.BroadcastMessage("getModInfo", list, UnityEngine.SendMessageOptions.DontRequireReceiver);
             }
             catch (Exception e)
             {
                 // ignore mod if error, removing dependency
                 mls.LogInfo($"Failed to send info to ModSync, go yell at Minx");
             }
             break;
         }
        
     }
 }

```

# Video Tutorial
https://www.youtube.com/watch?v=Zq8herBrzWI
