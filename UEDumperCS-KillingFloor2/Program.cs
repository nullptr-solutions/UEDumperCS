using UEDumperCS.Engine.UE3;
using UEDumperCS.Interop;
using UEDumperCS.Utils;

using UEDumperCS_KillingFloor2.Engine;

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;

Console.Title = "Killing Floor 2 - Dumper";

Logger.Get.EnableFileLogging();

var game = Process.GetProcessesByName("KFGame").FirstOrDefault();
if (game is null)
{
    Logger.Get.Error("Could not find killing floor 2");
    return;
}

Logger.Get.Info($"Found killing floor 2! {{ pid: {game.Id}, handle: {game.Handle:X} }}");

var namesAddress = PatternScanner.Search(game.Handle, game.MainModule, "E8 [....] 48 83 CB FF 45 85 F6", (bytes, address) =>
{
    address += BitConverter.ToInt32(bytes, address) + 0x4 + 0x7a;
    address += BitConverter.ToInt32(bytes, address + 0x3) + 0x7;
    return address;
});
if (namesAddress == IntPtr.Zero)
{
    Logger.Get.Error("Could not find global names address");
    return;
}

var objectsAddress = PatternScanner.Search(game.Handle, game.MainModule, "E8 [....] E8 .... 4D 85 E4", (bytes, address) =>
{
    address += BitConverter.ToInt32(bytes, address) + 0x4 + 0x233;
    address += BitConverter.ToInt32(bytes, address + 0x3) + 0x7;
    return address;
});
if (objectsAddress == IntPtr.Zero)
{
    Logger.Get.Error("Could not find global objects address");
    return;
}

Logger.Get.Info($"Resolved patterns {{ GNames: {namesAddress.ToString("X")}, GObjects: {objectsAddress.ToString("X")} }}");

var gnames = Kernel32.ReadMemory<GNames>(game.Handle, namesAddress);
if (gnames.Names.Num is 0)
{
    Logger.Get.Error("Global names count was 0");
    return;
}

var nameDump = new Dictionary<int, FNameEntry>(gnames.Names.Num);

for (var i = 0; i < gnames.Names.Num; i++)
{
    if (!gnames.Names.IsValidIndex(i))
        continue;

    var nameEntry = gnames.Names.Read(game.Handle, i, true, out _);
    if (nameEntry.Name is null)
        continue;

    nameDump.Add(i, nameEntry);
}

if (nameDump.Count is 0)
{
    Logger.Get.Error("Failed to dump global names");
    return;
}

Logger.Get.Info("Writing global names to file...");

using (var sw = new StreamWriter(File.Open("NameDump.txt", FileMode.Create, FileAccess.Write, FileShare.Read)))
{
    sw.WriteLine(
        $"// Created: {DateTime.Now}\n" +
        $"//    Game: {game.ProcessName} [{game.MainModule.FileName}]\n" +
        $"// Version: {game.MainModule.FileVersionInfo.FileVersion.Replace(" ", "")}\n");

    foreach (var name in nameDump)
        sw.WriteLine($"[{name.Key.ToString().PadLeft(6, '0')}] {name.Value.Name}");
}

var gobjects = Kernel32.ReadMemory<GObjects>(game.Handle, objectsAddress);
if (gobjects.Objects.Num is 0)
{
    Logger.Get.Error("Global objects count was 0");
    return;
}

Logger.Get.Info("Writing global objects to file...");

using (var sw = new StreamWriter(File.Open("ObjectsDump.txt", FileMode.Create, FileAccess.Write, FileShare.Read)))
{
    sw.WriteLine(
        $"// Created: {DateTime.Now}\n" +
        $"//    Game: {game.ProcessName} [{game.MainModule.FileName}]\n" +
        $"// Version: {game.MainModule.FileVersionInfo.FileVersion.Replace(" ", "")}\n");

    for (var i = 0; i < gobjects.Objects.Num; i++)
    {
        if (!gobjects.Objects.IsValidIndex(i))
            continue;

        var obj = gobjects.Objects.Read(game.Handle, i, true, out var ptr);
        if (obj.VTablePointer is 0 || ptr is 0)
            continue;

        var objName = obj.GetFullName(game.Handle, ref nameDump, true);
        if (objName is null)
            continue;

        if (obj.Inner.IsValid)
        {
            var innerObj = obj.Inner.Read(game.Handle);

            objName = $"{innerObj.GetName(ref nameDump, true) ?? "<unknown>"} {objName}";
        }

        sw.WriteLine($"[{i.ToString().PadLeft(6, '0')}] {ptr.ToString("X").PadLeft(16, '0')} {objName}");
    }
}

Logger.Get.Info("Finished dumping");

struct GNames
{
    public TArray<FNameEntry> Names;
}

struct GObjects
{
    public TArray<UObject> Objects;
}