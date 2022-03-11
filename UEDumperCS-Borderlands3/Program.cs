using UEDumperCS.Engine.UE4;
using UEDumperCS.Interop;
using UEDumperCS.Utils;

using UEDumperCS_Borderlands3.Engine;

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;

Console.Title = "Borderlands 3 - Dumper";

var game = Array.Find(Process.GetProcessesByName("Borderlands3"), x => !string.IsNullOrEmpty(x.MainWindowTitle));
if (game is null)
{
    Logger.Get.Error("Could not find borderlands 3!");
    return;
}

Logger.Get.Info($"Found borderlands 3! {{ ProcessId: {game.Id}, Handle: {game.Handle.ToString("X")} }}");

var namesAddress = PatternScanner.Search(game.Handle, game.MainModule, "E8 [....] 8B CB 48 8B D0", (bytes, address) =>
{
    address += BitConverter.ToInt32(bytes, address) + 0x4 + 0x7;
    address += BitConverter.ToInt32(bytes, address) + 0x4;
    return address;
});
if (namesAddress is 0)
{
    Logger.Get.Error("Could not get global names address!");
    return;
}

var objectsAddress = PatternScanner.Search(game.Handle, game.MainModule, "E8 [....] 44 8B 0D .... 45 33 C0", (bytes, address) =>
{
    address += BitConverter.ToInt32(bytes, address) + 0x04 + 0x23;
    address += BitConverter.ToInt32(bytes, address) + 0x04;
    return address;
});
if (objectsAddress is 0)
{
    Logger.Get.Error("Could not get global objects address!");
    return;
}

Logger.Get.Info($"Resolved patterns {{ GNames: {namesAddress:X}, GObjects: {objectsAddress:X} }}");

var gnamesAddress = Kernel32.ReadMemory<nint>(game.Handle, namesAddress);
if (gnamesAddress is 0)
{
    Logger.Get.Error("Could not get gname array ptr!");
    return;
}

var gnames = Kernel32.ReadMemory<GNames>(game.Handle, gnamesAddress);
if (gnames.TNameEntryArray.NumElements <= 0 || gnames.TNameEntryArray.NumChunks <= 0)
{
    Logger.Get.Error("Invalid gname array!");
    return;
}

var nameDump = new Dictionary<int, FNameEntry>(gnames.TNameEntryArray.NumElements);

for (var i = 0; i < gnames.TNameEntryArray.NumElements; i++)
{
    var nameEntry = gnames.TNameEntryArray.GetById(game.Handle, i);
    if (nameEntry.Name is null)
        continue;

    nameDump.Add(i, nameEntry);
}

if (nameDump.Count is 0)
{
    Logger.Get.Error("Failed to dump global names!");
    return;
}

Logger.Get.Info("Writing global names to file...");

using (var sw = new StreamWriter(File.Open("NameDump.txt", FileMode.Create, FileAccess.Write, FileShare.Read)))
{
    sw.WriteLine(
        $"// Created: {DateTime.Now}\n" +
        $"//    Game: {game.ProcessName} [{game.MainModule.FileName}]\n" +
        $"// Version: {game.MainModule.FileVersionInfo.FileVersion.Replace(" ", "")}\n");

    foreach (var nameEntry in nameDump)
        sw.WriteLine($"[{nameEntry.Key.ToString().PadLeft(6, '0')}] {nameEntry.Value.Name}");
}

var objectArray = Kernel32.ReadMemory<TUObjectArray>(game.Handle, objectsAddress);
if (objectArray.ObjObjects.NumElements <= 0 || objectArray.ObjObjects.NumChunks <= 0)
{
    Logger.Get.Error("Invalid gobjects array!");
    return;
}

Logger.Get.Info("Writing global objects to file...");

using (var sw = new StreamWriter(File.Open("ObjectsDump.txt", FileMode.Create, FileAccess.Write, FileShare.Read)))
{
    sw.WriteLine(
        $"// Created: {DateTime.Now}\n" +
        $"//    Game: {game.ProcessName} [{game.MainModule.FileName}]\n" +
        $"// Version: {game.MainModule.FileVersionInfo.FileVersion.Replace(" ", "")}\n");

    for (var i = 0; i < objectArray.ObjObjects.NumElements; i++)
    {
        var objItem = objectArray.ObjObjects.GetById(game.Handle, i);
        if (objItem.Object.IsValid)
        {
            var obj = objItem.Object.Read(game.Handle);
            if (obj.VTablePointer is 0)
                continue;

            var objName = obj.GetFullName(game.Handle, ref nameDump, true);
            if (objName is null)
                continue;

            if (obj.Inner.IsValid)
            {
                var innerObj = obj.Inner.Read(game.Handle);

                objName = $"{innerObj.GetName(ref nameDump, true) ?? "<unknown>"} {objName}";
            }

            sw.WriteLine(
                $"[{i.ToString().PadLeft(6, '0')}] {objItem.Object.Pointer.ToString("X").PadLeft(16, '0')} {objName}");
        }
    }
}

Logger.Get.Info("Finished!");

struct GNames
{
    public TStaticIndirectArray<FNameEntry> TNameEntryArray;
}