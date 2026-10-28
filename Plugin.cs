using System.Numerics;
using System.Text.Json;
using Dalamud.Bindings.ImGui;
using Dalamud.Configuration;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
namespace GearSetsCompanion;
[Serializable] public sealed class Config : IPluginConfiguration {
 public int Version {get;set;}=1;
 public string Folder {get;set;}=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"Bots","FFXIVMinion64","LuaMods","JacobGearPlanner");
 public bool Enabled {get;set;}=true;
}
public sealed record Plan(int schemaVersion,string character,bool highlight,uint[] itemIds);
public sealed unsafe class Plugin : IDalamudPlugin {
 readonly IDalamudPluginInterface pi; readonly IFramework framework; readonly IPlayerState player; readonly IDataManager data; readonly IGameGui gui; readonly ICommandManager commands;
 readonly Config config; Plan? plan; DateTime next; bool open; string status="Waiting for the Minion plan."; string lastArmoire="";
 readonly HashSet<uint> ids=[];
 public Plugin(IDalamudPluginInterface pi,IFramework framework,IPlayerState player,IDataManager data,IGameGui gui,ICommandManager commands){
 this.pi=pi;this.framework=framework;this.player=player;this.data=data;this.gui=gui;this.commands=commands;
 config=pi.GetPluginConfig() as Config ?? new();framework.Update+=Update;pi.UiBuilder.Draw+=Draw;pi.UiBuilder.OpenConfigUi+=Open;
 commands.AddHandler("/gearsets",new CommandInfo((_,_)=>Open()){HelpMessage="Open Gear Sets Companion settings."});
 }
 void Open()=>open=true;
 string Identity=>player.CharacterName+"@"+player.HomeWorld.RowId;
 void Update(IFramework _){
 if(DateTime.UtcNow<next)return;next=DateTime.UtcNow.AddSeconds(1);
 ids.Clear();if(!player.IsLoaded){plan=null;lastArmoire="";return;}
 try{
 var path=Path.Combine(config.Folder,"plan.json");
 plan=File.Exists(path)?JsonSerializer.Deserialize<Plan>(File.ReadAllText(path)):null;
 if(plan is {schemaVersion:1} && plan.character==Identity){foreach(var id in plan.itemIds.Take(10000))if(id>0)ids.Add(id);status=$"{ids.Count} planned item IDs loaded.";}else{plan=null;status="No matching character plan.";}
 var cabinet=&UIState.Instance()->Cabinet;if(!cabinet->IsCabinetLoaded())return;
 var owned=new List<uint>();foreach(var row in data.GetExcelSheet<Lumina.Excel.Sheets.Cabinet>())if(cabinet->IsItemInCabinet(row.RowId))owned.Add(row.Item.RowId);
 var fingerprint=Identity+":"+string.Join(',',owned);if(fingerprint==lastArmoire)return;
 if(!Directory.Exists(config.Folder)){status="Choose the existing JacobGearPlanner folder.";return;}
 var output=Path.Combine(config.Folder,"armoire.json");var temp=output+".tmp";
 File.WriteAllText(temp,JsonSerializer.Serialize(new{schemaVersion=1,character=Identity,complete=true,scannedAt=DateTimeOffset.UtcNow.ToUnixTimeSeconds(),itemIds=owned}));File.Move(temp,output,true);lastArmoire=fingerprint;
 }catch(Exception e){status=e.Message;plan=null;ids.Clear();}
 }
 void Draw(){
 if(open){ImGui.SetNextWindowSize(new(580,220),ImGuiCond.FirstUseEver);if(ImGui.Begin("Gear Sets Companion",ref open)){
 var enabled=config.Enabled;if(ImGui.Checkbox("Enable planned-item highlights",ref enabled)){config.Enabled=enabled;pi.SavePluginConfig(config);}
 ImGui.TextWrapped("Green outlines identify items used by your selected Minion plans. Enable the matching toggle in Plan too.");
 var folder=config.Folder;if(ImGui.InputText("Planner folder",ref folder,1024)){config.Folder=folder;lastArmoire="";pi.SavePluginConfig(config);}
 ImGui.TextWrapped(status);ImGui.TextWrapped("Armoire ownership updates when the game has loaded the armoire. No items are moved.");
 }ImGui.End();}
 if(!config.Enabled||plan?.highlight!=true||!player.IsLoaded||plan.character!=Identity||ids.Count==0)return;
 for(int page=0;page<4;page++)Grid("InventoryGrid"+page+"E",page);
 var buddy=(AddonInventoryBuddy*)gui.GetAddonByName("InventoryBuddy").Address;
 if(buddy!=null&&buddy->IsVisible&&buddy->TabIndex<=1){for(int i=0;i<70;i++){
 var bag=InventoryManager.Instance()->GetInventoryContainer((InventoryType)((buddy->TabIndex==0?4000:4100)+i/35));if(bag==null||!bag->IsLoaded||i%35>=bag->Size)continue;
 if(ids.Contains(bag->Items[i%35].GetBaseItemId()))Outline(buddy->Slots[i].Value,buddy->Scale);
 }}
 var normal=(AddonInventory*)gui.GetAddonByName("Inventory").Address;if(normal!=null&&normal->IsVisible&&normal->TabIndex is >=0 and <4)Grid("InventoryGrid",normal->TabIndex);
 }

 void Grid(string name,int page){
 var grid=(AddonInventoryGrid*)gui.GetAddonByName(name).Address;if(grid==null||!grid->IsVisible)return;
 var bag=InventoryManager.Instance()->GetInventoryContainer((InventoryType)page);if(bag==null||!bag->IsLoaded)return;
 for(int i=0;i<Math.Min(35,bag->Size);i++){if(!ids.Contains(bag->Items[i].GetBaseItemId()))continue;Outline(grid->Slots[i].Value,grid->Scale);
 }
 }
 void Outline(AtkComponentDragDrop* slot,float scale){
 if(slot==null)return;var node=slot->AtkDragDropInterface.ComponentNode;if(node==null||!node->IsVisible())return;
 float x=0,y=0;slot->AtkDragDropInterface.GetScreenPosition(&x,&y);var size=36*scale;
 var a=new Vector2(x,y)+ImGui.GetMainViewport().Pos;ImGui.GetForegroundDrawList().AddRect(a,a+new Vector2(size),0xFF40FF40,4,ImDrawFlags.None,2);
 }
 public void Dispose(){framework.Update-=Update;pi.UiBuilder.Draw-=Draw;pi.UiBuilder.OpenConfigUi-=Open;commands.RemoveHandler("/gearsets");}
}
