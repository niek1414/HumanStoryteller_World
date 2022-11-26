using System.Linq;
using HumanStoryteller.DebugConnection;
using HumanStoryteller.Util;
using HumanStoryteller.Views.Extention;
using UnityEngine;
using Verse;

namespace HumanStoryteller.Views; 
public class Window_CopyZone : EditWindow {
    private bool _selectNow;
    private bool _copyStructure;
    private bool _sendToStorymaker;
    private int _originX;
    private int _originZ;
    private bool _buildings;
    private bool _items;
    private bool _terrain;
    private bool _floor;
    private bool _pawns;
    private bool _plants;
    private bool _roof;

    public Window_CopyZone(bool sendToStorymaker = false, bool copyStructure = true) {
        optionalTitle = "CopyZone";
        forcePause = true;
        _sendToStorymaker = sendToStorymaker;
        doCloseX = !_sendToStorymaker;
        _copyStructure = copyStructure;
    }

    public override Vector2 InitialSize => new Vector2(630f, 590f);

    public override bool IsDebug => true;

    public override void DoWindowContents(Rect inRect) {
        var areaHome = Find.CurrentMap.areaManager.Home;
        areaHome.MarkForDraw();

        Text.Font = GameFont.Small;
        Rect typeInfo = new Rect(inRect.x, inRect.y, inRect.width, 100);
        

        if (!_sendToStorymaker) {
            Widgets.Label(typeInfo, "ZoneCopyIntro".Translate());
            
            Rect radioStructure = new Rect(typeInfo.x + 50, typeInfo.y + typeInfo.height + 10, 160, 30);
            if (Widgets.RadioButtonLabeled(radioStructure, "CopyStructure".Translate(), _copyStructure)) {
                _copyStructure = true;
            }

            Rect radioLocation = new Rect(typeInfo.x + 300, typeInfo.y + typeInfo.height + 10, 160, 30);
            if (Widgets.RadioButtonLabeled(radioLocation, "CopyLocation".Translate(), !_copyStructure)) {
                _copyStructure = false;
            }
        } else {
            Widgets.Label(typeInfo, "ZoneCopyIntroShort".Translate());
        }
        
        if (Widgets.ButtonText(new Rect(typeInfo.x, typeInfo.y + typeInfo.height + 40, 200, 30), "ClearHomeZone".Translate())) {
            var list = areaHome.ActiveCells.ToList();
            foreach (var intVec3 in list) {
                areaHome[intVec3] = false;
            }
        }

        Rect originInfo = new Rect(typeInfo.x, typeInfo.y + typeInfo.height + 85, inRect.width, 25);
        Widgets.Label(originInfo, "ZoneCopyOrigin".Translate());

        if (_selectNow) {
            var cell = UI.MouseCell();
            _originX = cell.x;
            _originZ = cell.z;
            if (Input.GetMouseButton(0)) {
                _selectNow = false;
            }
        }

        string buf = null;
        var originInputY = originInfo.y + originInfo.height;
        Widgets.Label(new Rect(originInfo.x, originInputY, 18, 30), "x:");
        Widgets.TextFieldNumeric(new Rect(originInfo.x + 18, originInputY, 40, 30), ref _originX, ref buf, -9999999f);

        Widgets.Label(new Rect(originInfo.x + 67, originInputY, 30, 30), "y: 0");

        string buf2 = null;
        Widgets.Label(new Rect(originInfo.x + 99, originInputY, 18, 30), "z:");
        Widgets.TextFieldNumeric(new Rect(originInfo.x + 120, originInputY, 40, 30), ref _originZ, ref buf2, -9999999f);

        if (Widgets.ButtonText(new Rect(originInfo.x + 165, originInputY, 100, 30), "Pick".Translate())) {
            _selectNow = true;
        }

        if (Widgets.ButtonText(new Rect(originInfo.x + 269, originInputY, 100, 30), "Clear".Translate())) {
            _originX = 0;
            _originZ = 0;
        }

        Rect startChecks = new Rect(originInfo.x, originInfo.y + originInfo.height + 50, 385, 30);

        if (_copyStructure) {
            WidgetsExtented.CheckboxBeforeLabel(startChecks, "BuildingZoneCopy".Translate(), ref _buildings);
            startChecks.y += 30;
            WidgetsExtented.CheckboxBeforeLabel(startChecks, "ItemZoneCopy".Translate(), ref _items);
            startChecks.y += 30;
            WidgetsExtented.CheckboxBeforeLabel(startChecks, "TerrainZoneCopy".Translate(), ref _terrain);
            startChecks.y += 30;
            WidgetsExtented.CheckboxBeforeLabel(startChecks, "FloorZoneCopy".Translate(), ref _floor);
            startChecks.y += 30;
            WidgetsExtented.CheckboxBeforeLabel(startChecks, "PawnsZoneCopy".Translate(), ref _pawns);
            startChecks.y += 30;
            WidgetsExtented.CheckboxBeforeLabel(startChecks, "PlantsZoneCopy".Translate(), ref _plants);
            startChecks.y += 30;
            WidgetsExtented.CheckboxBeforeLabel(startChecks, "RoofZoneCopy".Translate(), ref _roof);
            startChecks.y += 35;
            startChecks.width += 550;
            Widgets.Label(startChecks, "WarnZoneCopy".Translate());
            startChecks.y += 35;
        }

        if (_sendToStorymaker) {
            if (Widgets.ButtonText(new Rect(originInfo.x, startChecks.y, 200, 30), "SendZoneToMaker".Translate())) {
                DebugWebSocket.TrySendLocationData(_copyStructure
                    ? AreaUtil.AreaObjectsToString(new IntVec3(_originX, 0, _originZ), _buildings, _items, _terrain, _floor, _pawns, _plants, _roof)
                    : AreaUtil.AreaLocationToString(new IntVec3(_originX, 0, _originZ)));

                Find.WindowStack.TryRemove(this);
            }
            if (Widgets.ButtonText(new Rect(originInfo.x + 220, startChecks.y, 200, 30), "CancelButton".Translate())) {
                DebugWebSocket.TrySendLocationData("");
                Find.WindowStack.TryRemove(this);
            }
        } else {
            if (Widgets.ButtonText(new Rect(originInfo.x, startChecks.y, 200, 30), "ZoneCopyNow".Translate())) {
                GUIUtility.systemCopyBuffer = _copyStructure
                    ? AreaUtil.AreaObjectsToString(new IntVec3(_originX, 0, _originZ), _buildings, _items, _terrain, _floor, _pawns, _plants, _roof)
                    : AreaUtil.AreaLocationToString(new IntVec3(_originX, 0, _originZ));

                Find.WindowStack.TryRemove(this);
            }
        }
    }
}