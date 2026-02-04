using UnityEngine;
using DWORD = System.UInt32;
using UnityEngine.UIElements;


public class StrategicHUD : CommLink
{
    BaseContact selectedUnit;
    Label Coords;
    Label SelectedUnitName;
    Label SelectedUnitSide;
    Label SelectedUnitSpeed;
    Label SelectedUnitDir;
    Label SelectedUnitOrg;
    VisualElement SelectedUnitType;
    Label SelectedUnitAutopilot;

    ProgressBar SelectedUnitHealth;
    DropdownField LocationEnum;

    CameraLogicStrategic owner;


    Button OrderMove;
    Button OrderAttack;
    Button OrderHangar;
    Button OrderStop;
    bool InitDone;

    VisualElement UnitInfoWindow;
    Button UnitInfoWindowClose;
    Label UnitInfoWindowLabel;
    public StrategicHUD(CameraLogicStrategic _owner)
    {
        owner = _owner;
        InitGUI();

        //subscribe
        owner.rScene.GetCommandsApi().RegisterVariable("cam", this, VType.VAR_VECTOR);
    }

    public void Dispose()
    {
        ClearSelection();
        UnityEngine.Cursor.visible = false;
    }

    private void InitGUI()
    {
        UnityEngine.Cursor.visible = true;
        InitDone = false;
        //GameObject OrdersPanel = GameObject.Find("StrategicGUI");
        GameObject OrdersPanel = GameObject.Instantiate(MaterialStorage.myRendererConfig.StrategicGUI);
        if (OrdersPanel == null) return;
        UIDocument HUD = OrdersPanel.GetComponent<UIDocument>();
        if (HUD == null) return;

        Coords = HUD.rootVisualElement.Q<Label>("CoordsLabel");
        VisualElement UnitDescription = HUD.rootVisualElement.Q<VisualElement>("UnitDescription");

        if (UnitDescription == null) return;

        SelectedUnitName = UnitDescription.Q<Label>("SelectedUnitName");
        SelectedUnitDir = UnitDescription.Q<Label>("SelectedUnitDir");
        SelectedUnitSpeed = UnitDescription.Q<Label>("SelectedUnitSpeed");
        SelectedUnitOrg = UnitDescription.Q<Label>("SelectedUnitOrg");
        SelectedUnitSide = UnitDescription.Q<Label>("SelectedUnitSide");
        SelectedUnitHealth = UnitDescription.Q<ProgressBar>("SelectedUnitHealth");
        SelectedUnitType = UnitDescription.Q<VisualElement>("SelectedUnitType");
        SelectedUnitAutopilot = UnitDescription.Q<Label>("SelectedUnitAutopilot");
        LocationEnum = HUD.rootVisualElement.Q<DropdownField>("Locations");
        //LocationEnum.RegisterValueChangedCallback(evt => owner.SwitchCameraLocation(LocationEnum.index));

        VisualElement OrderPanel = HUD.rootVisualElement.Q<VisualElement>("OrderPanel");
        if (OrderPanel == null) return;

        OrderMove = OrderPanel.Q<Button>("OrderMove");
        OrderAttack = OrderPanel.Q<Button>("OrderAttack");
        OrderHangar = OrderPanel.Q<Button>("OrderHangar");
        OrderStop = OrderPanel.Q<Button>("OrderStop");

        OrderHangar.clicked += OrderHangar_clicked;
        OrderStop.clicked += OrderStop_clicked;
        OrderMove.clicked += OrderMove_clicked;

        #region UnitInfo windows
        UnitInfoWindow = HUD.rootVisualElement.Q<VisualElement>("UnitInfoWindow");
        UnitInfoWindowClose = UnitInfoWindow.Q<Button>("UnitInfoWindowButtonClose");
        UnitInfoWindowLabel = UnitInfoWindow.Q<Label>("UnitInfoWindowText");
        UnitInfoWindowClose.clicked += UnitInfoWindowClose_clicked;
        #endregion
        InitDone = true;

    }

    public void ClearSelection()
    {
        UnselectUnit();
        OrderMove.style.display = DisplayStyle.None;
        OrderAttack.style.display = DisplayStyle.None;
        OrderHangar.style.display = DisplayStyle.None;
        OrderStop.style.display = DisplayStyle.None;

        SelectedUnitName.style.display = DisplayStyle.None;
        SelectedUnitSide.style.display = DisplayStyle.None;
        SelectedUnitHealth.style.display = DisplayStyle.None;

        UnitInfoWindow.style.display = DisplayStyle.None;
    }

    private void OrderMove_clicked()
    {

        ((SingleBFAi)selectedUnitAI).SetDestination(new Vector3(134195.671875f, 200f, 357748.5f), 200);
        Debug.Log("Moving to " + ((SingleBFAi)selectedUnitAI).GetDestination());
    }

    private void UnitInfoWindowClose_clicked()
    {
        UnitInfoWindow.style.display = DisplayStyle.None;
    }

    private void OrderStop_clicked()
    {
        if (selectedUnit == null) return;
        BaseCraft unit = (BaseCraft)selectedUnit.GetUnit().GetInterface(BaseCraft.ID);
        if (unit != null)
        {
            BaseCraftAi ai = (BaseCraftAi)selectedUnitAI;
            if (ai == null) return;
            Debug.Log("Stopping! " + ai);
            ai.Pause(true);
            return;
        }
        Debug.Log("Me doing nothing!");
    }

    private void OrderHangar_clicked()
    {
        if (selectedUnit == null) return;

        ((HostScene)owner.rScene).GetMissionAI().GetContactInfo(selectedUnit, out DWORD grp_id, out DWORD un_index, out DWORD side);
        var GroupAi = ((HostScene)owner.rScene).GetMissionAI().GetGroupByID(grp_id);
        if (GroupAi == null) return;

        Debug.Log(selectedUnit.GetInterface(BaseHangar.ID));
        BaseHangar hangar = (BaseHangar)selectedUnit.GetInterface(BaseHangar.ID);
        if (hangar == null)
        {
            Debug.Log(string.Format("Can't convert into hangar {0} {1}", GroupAi.GetGroupData().Callsign, un_index));
            return;

        }

        if (hangar.isDoorClosed())
        {
            Debug.Log(string.Format("{0} {1} this is Supreme Command, open hangar", GroupAi.GetGroupData().Callsign, un_index));
            //hangar.onDataPacket(BaseStaticPackets.SDP_HANGAR_OPEN_DOOR);
            hangar.OpenDoor();
        }
        if (hangar.isDoorOpened())
        {
            Debug.Log(string.Format("{0} {1} this is Supreme Command, close hangar", GroupAi.GetGroupData().Callsign, un_index));
            //hangar.onDataPacket(BaseStaticPackets.SDP_HANGAR_CLOSE_DOOR);
            hangar.CloseDoor();
        }
    }

    IGroupAi selectedGroupAI;
    GROUP_DATA selectedGroupData;
    UNIT_DATA selectedUnitData;
    IBaseUnitAi selectedUnitAI;
    DWORD selectedUnitId;
    DWORD selectedGroupID;

    private void SelectUnit(BaseContact cnt)
    {
        if (cnt == null) return;
        selectedUnit = cnt;
        ((HostScene)owner.rScene).GetMissionAI().GetContactInfo(cnt, out selectedGroupID, out selectedUnitId, out DWORD side);
        selectedGroupAI = ((HostScene)owner.rScene).GetMissionAI().GetGroupByID(selectedGroupID);
        selectedGroupData = selectedGroupAI.GetGroupData();
        selectedUnitData = selectedGroupAI.GetUnitDataByIndex(selectedUnitId);
        selectedUnitAI = selectedGroupAI.GetUnitAiByData(selectedUnitData);
    }
    private void UnselectUnit()
    {
        selectedUnit = null;
        selectedGroupAI = null;
        selectedGroupData = null;
        selectedUnitData = null;
        selectedUnitAI = null;
        selectedGroupID = Constants.THANDLE_INVALID;
        selectedUnitId = Constants.THANDLE_INVALID;
    }
}
//public class StrategicHUD : CommLink
//{
//    BaseContact selectedUnit;
//    Label Coords;
//    Label SelectedUnitName;
//    Label SelectedUnitSide;
//    Label SelectedUnitSpeed;
//    Label SelectedUnitDir;
//    Label SelectedUnitOrg;
//    VisualElement SelectedUnitType;
//    Label SelectedUnitAutopilot;

//    ProgressBar SelectedUnitHealth;
//    DropdownField LocationEnum;

//    CameraLogicStrategic owner;


//    Button OrderMove;
//    Button OrderAttack;
//    Button OrderHangar;
//    Button OrderStop;
//    bool InitDone;

//    VisualElement UnitInfoWindow;
//    Button UnitInfoWindowClose;
//    Label UnitInfoWindowLabel;
//    public StrategicHUD(CameraLogicStrategic owner)
//    {
//        InitDone = false;
//        this.owner = owner;

//        GameObject OrdersPanel = GameObject.Find("StrategicGUI");
//        if (OrdersPanel == null) return;
//        UIDocument HUD = OrdersPanel.GetComponent<UIDocument>();
//        if (HUD == null) return;

//        Coords = HUD.rootVisualElement.Q<Label>("CoordsLabel");
//        VisualElement UnitDescription = HUD.rootVisualElement.Q<VisualElement>("UnitDescription");

//        if (UnitDescription == null) return;

//        SelectedUnitName = UnitDescription.Q<Label>("SelectedUnitName");
//        SelectedUnitDir = UnitDescription.Q<Label>("SelectedUnitDir");
//        SelectedUnitSpeed = UnitDescription.Q<Label>("SelectedUnitSpeed");
//        SelectedUnitOrg = UnitDescription.Q<Label>("SelectedUnitOrg");
//        SelectedUnitSide = UnitDescription.Q<Label>("SelectedUnitSide");
//        SelectedUnitHealth = UnitDescription.Q<ProgressBar>("SelectedUnitHealth");
//        SelectedUnitType = UnitDescription.Q<VisualElement>("SelectedUnitType");
//        SelectedUnitAutopilot = UnitDescription.Q<Label>("SelectedUnitAutopilot");
//        LocationEnum = HUD.rootVisualElement.Q<DropdownField>("Locations");
//        LocationEnum.RegisterValueChangedCallback(evt => owner.SwitchCameraLocation(LocationEnum.index));

//        VisualElement OrderPanel = HUD.rootVisualElement.Q<VisualElement>("OrderPanel");
//        if (OrderPanel == null) return;

//        OrderMove = OrderPanel.Q<Button>("OrderMove");
//        OrderAttack = OrderPanel.Q<Button>("OrderAttack");
//        OrderHangar = OrderPanel.Q<Button>("OrderHangar");
//        OrderStop = OrderPanel.Q<Button>("OrderStop");

//        OrderHangar.clicked += OrderHangar_clicked;
//        OrderStop.clicked += OrderStop_clicked;
//        OrderMove.clicked += OrderMove_clicked;

//        #region UnitInfo windows
//        UnitInfoWindow = HUD.rootVisualElement.Q<VisualElement>("UnitInfoWindow");
//        UnitInfoWindowClose = UnitInfoWindow.Q<Button>("UnitInfoWindowButtonClose");
//        UnitInfoWindowLabel = UnitInfoWindow.Q<Label>("UnitInfoWindowText");
//        UnitInfoWindowClose.clicked += UnitInfoWindowClose_clicked;
//        #endregion
//        InitDone = true;

//        //subscribe
//        owner.rScene.GetCommandsApi().RegisterVariable("cam", this, VType.VAR_VECTOR);

//    }

//    private void OrderMove_clicked()
//    {

//        ((SingleBFAi)selectedUnitAI).SetDestination(new Vector3(134195.671875f, 200f, 357748.5f), 200);
//        Debug.Log("Moving to " + ((SingleBFAi)selectedUnitAI).GetDestination());
//    }

//    private void UnitInfoWindowClose_clicked()
//    {
//        UnitInfoWindow.style.display = DisplayStyle.None;
//    }
//    private void OrderStop_clicked()
//    {
//        if (selectedUnit == null) return;
//        BaseCraft unit = (BaseCraft)selectedUnit.GetUnit().GetInterface(BaseCraft.ID);
//        if (unit != null)
//        {
//            BaseCraftAi ai = (BaseCraftAi)selectedUnitAI;
//            if (ai == null) return;
//            Debug.Log("Stopping! " + ai);
//            ai.Pause(true);
//            return;
//        }
//        Debug.Log("Me doing nothing!");
//    }

//    private void OrderHangar_clicked()
//    {
//        if (selectedUnit == null) return;

//        ((HostScene)owner.rScene).GetMissionAI().GetContactInfo(selectedUnit, out DWORD grp_id, out DWORD un_index, out DWORD side);
//        var GroupAi = ((HostScene)owner.rScene).GetMissionAI().GetGroupByID(grp_id);
//        if (GroupAi == null) return;

//        Debug.Log(selectedUnit.GetInterface(BaseHangar.ID));
//        BaseHangar hangar = (BaseHangar)selectedUnit.GetInterface(BaseHangar.ID);
//        if (hangar == null)
//        {
//            Debug.Log(string.Format("Can't convert into hangar {0} {1}", GroupAi.GetGroupData().Callsign, un_index));
//            return;

//        }

//        if (hangar.isDoorClosed())
//        {
//            Debug.Log(string.Format("{0} {1} this is Supreme Command, open hangar", GroupAi.GetGroupData().Callsign, un_index));
//            hangar.onDataPacket(BaseStaticPackets.SDP_HANGAR_OPEN_DOOR);
//        }
//        if (hangar.isDoorOpened())
//        {
//            Debug.Log(string.Format("{0} {1} this is Supreme Command, close hangar", GroupAi.GetGroupData().Callsign, un_index));
//            hangar.onDataPacket(BaseStaticPackets.SDP_HANGAR_CLOSE_DOOR);
//        }
//    }

//    private IBaseUnitAi GetUnitAI(iContact cnt)
//    {
//        ((HostScene)owner.rScene).GetMissionAI().GetContactInfo(cnt, out DWORD grp_id, out DWORD un_index, out DWORD side);
//        var GroupAI = ((HostScene)owner.rScene).GetMissionAI().GetGroupByID(grp_id);
//        var ud = GroupAI.GetUnitDataByIndex(un_index);
//        var UnitAI = GroupAI.GetUnitAiByData(ud);
//        return UnitAI;
//    }

//    IGroupAi selectedGroupAI;
//    GROUP_DATA selectedGroupData;
//    UNIT_DATA selectedUnitData;
//    IBaseUnitAi selectedUnitAI;
//    DWORD selectedUnitId;
//    DWORD selectedGroupID;

//    private void SelectUnit(BaseContact cnt)
//    {
//        if (cnt == null) return;
//        selectedUnit = cnt;
//        ((HostScene)owner.rScene).GetMissionAI().GetContactInfo(cnt, out selectedGroupID, out selectedUnitId, out DWORD side);
//        selectedGroupAI= ((HostScene)owner.rScene).GetMissionAI().GetGroupByID(selectedGroupID);
//        selectedGroupData = selectedGroupAI.GetGroupData();
//        selectedUnitData = selectedGroupAI.GetUnitDataByIndex(selectedUnitId);
//        selectedUnitAI = selectedGroupAI.GetUnitAiByData(selectedUnitData);
//    }
//    private void UnselectUnit()
//    {
//        selectedUnit = null;
//        selectedGroupAI = null;
//        selectedGroupData = null;
//        selectedUnitData = null;
//        selectedUnitAI = null;
//        selectedGroupID = Constants.THANDLE_INVALID;
//        selectedUnitId = Constants.THANDLE_INVALID;
//    }


//    public void SetPanel(BaseContact unit)
//    {
//        if (!InitDone) return;

//        SelectedUnitName.style.display = DisplayStyle.Flex;
//        SelectedUnitSide.style.display = DisplayStyle.Flex;
//        SelectedUnitHealth.style.display = DisplayStyle.Flex;

//        SelectUnit(unit);



//        SelectedUnitName.text = ((BaseObject)unit.GetUnit()).GetObjectData().FullName;
//        if (selectedGroupData != null) SelectedUnitName.text = selectedGroupData.Callsign + " " + selectedUnitId + " " + unit.GetHandle().ToString("X8");

//        string unitSide;
//        switch ((uint)((BaseContact)unit).GetSideCode())
//        {
//            case (CampaignDefines.CS_SIDE_NEUTRAL):

//                unitSide = "Neutral";
//                break;
//            case (CampaignDefines.CS_SIDE_ALIENS):
//                unitSide = "Xenos";
//                break;
//            case (CampaignDefines.CS_SIDE_HUMANS):
//                unitSide = "Federation";
//                break;
//            case (CampaignDefines.CS_SIDE_VELIANS):
//                unitSide = "Velian";
//                break;
//            default:
//                unitSide = "Unknown";
//                break;
//        }
//        SelectedUnitSide.text = unitSide;
//        SelectedUnitHealth.value = (((BaseContact)unit).GetUnit().GetCondition() * 100);

//        OrderMove.style.display = DisplayStyle.None;
//        OrderAttack.style.display = DisplayStyle.None;
//        OrderHangar.style.display = DisplayStyle.None;
//        OrderStop.style.display = DisplayStyle.None;

//        if (selectedGroupAI != null)
//        {
//            //if (GroupAi.HaveHangars(side,HangarDefs.HANGARS_LAND_TAKEOFF)) OrderHangar.style.display = DisplayStyle.Flex;
//            if ((BaseHangar)selectedUnit.GetInterface(BaseHangar.ID) != null) OrderHangar.style.display = DisplayStyle.Flex;
//            if ((BaseCraft)selectedUnit.GetInterface(BaseCraft.ID) != null) OrderMove.style.display = DisplayStyle.Flex;
//        }

//        OrderStop.style.display = DisplayStyle.Flex;



//        //switch ((uint)unit.getUnitType())
//        //{
//        //    case CampaignDefines.CF_CLASS_STATIC:
//        //        OrderStop.style.display = DisplayStyle.Flex;
//        //        break;
//        //    case CampaignDefines.CF_CLASS_SEA_CARRIER:
//        //        OrderMove.style.display = DisplayStyle.Flex;
//        //        OrderAttack.style.display = DisplayStyle.Flex;
//        //        OrderHangar.style.display = DisplayStyle.Flex;
//        //        OrderStop.style.display = DisplayStyle.Flex;
//        //        break;
//        //    case CampaignDefines.CF_CLASS_AIR_CARRIER:
//        //        OrderMove.style.display = DisplayStyle.Flex;
//        //        OrderAttack.style.display = DisplayStyle.Flex;
//        //        OrderHangar.style.display = DisplayStyle.Flex;
//        //        OrderStop.style.display = DisplayStyle.Flex;
//        //        break;
//        //    case CampaignDefines.CF_CLASS_CRAFT:
//        //        OrderMove.style.display = DisplayStyle.Flex;
//        //        OrderAttack.style.display = DisplayStyle.Flex;
//        //        OrderStop.style.display = DisplayStyle.Flex;
//        //        break;
//        //    case CampaignDefines.CF_CLASS_VEHICLE:
//        //        OrderMove.style.display = DisplayStyle.Flex;
//        //        OrderAttack.style.display = DisplayStyle.Flex;
//        //        OrderStop.style.display = DisplayStyle.Flex;
//        //        break;
//        //}

//        UnitInfoWindow.style.display = DisplayStyle.Flex;
//        UnitInfoWindowLabel.text = GenerateUnitDescription();
//    }

//    public void Draw(CameraData rData)
//    {
//        if (!InitDone) return;
//        Coords.text = rData.myCamera.Org.ToString();
//        if (selectedUnit != null)
//        {
//            SelectedUnitOrg.text = "Org: " + selectedUnit.GetUnit().GetOrg().ToString();
//            SelectedUnitDir.text = "Dir: " + selectedUnit.GetUnit().GetDir().ToString();

//            //if (selectedUnit.GetUnit().GetType() == typeof(BaseCraft))
//            //{
//            //    BaseCraft myCraft = (BaseCraft)selectedUnit.GetUnit();
//            //    SelectedUnitSpeed.text = "Speed: " + myCraft.GetSpeed().ToString();
//            //    SelectedUnitDir.text = "ThrustOut: " + myCraft.ThrustOut.ToString();
//            //    //SelectedUnitDir.text = "Controls: " + myCraft.Controls.ToString();
//            //    Vector3 localpos = myCraft.GetOrg() - Engine.EngineCamera.Org;
//            //    //Debug.DrawLine(localpos, localpos + (myCraft.ThrustOut * 50), Color.red);
//            //    //Debug.DrawLine(localpos, localpos + (myCraft.ThrustOut * 50), Color.red);
//            //    Debug.DrawRay(localpos, myCraft.ThrustOut * 50, Color.red);
//            //    Debug.DrawRay(localpos, myCraft.Thrust.normalized * 50, Color.cyan);
//            //    Debug.DrawRay(localpos, ((BaseCraftAutopilotFlyTo) myCraft.pAutopilot).TgtDir.normalized*50,Color.green);

//            //    SelectedUnitAutopilot.text = myCraft.pAutopilot.ToString() + " " + myCraft.pAutopilot.GetState();
//            //}
//            if (selectedUnit.GetUnit().GetType() == typeof(BaseCraft))
//            {
//                BaseCraft myCraft = (BaseCraft)selectedUnit.GetUnit();
//                SelectedUnitSpeed.text = "Speed: " + myCraft.GetSpeed().ToString();
//                SelectedUnitDir.text = "ThrustOut: " + myCraft.ThrustOut.ToString();
//                //SelectedUnitDir.text = "Controls: " + myCraft.Controls.ToString();
//                Vector3 localpos = myCraft.GetOrg() - Engine.EngineCamera.Org;
//                //Debug.DrawLine(localpos, localpos + (myCraft.ThrustOut * 50), Color.red);
//                //Debug.DrawLine(localpos, localpos + (myCraft.ThrustOut * 50), Color.red);
//                Debug.DrawRay(localpos, myCraft.ThrustOut * 50, Color.red);
//                Debug.DrawRay(localpos, myCraft.Thrust.normalized * 50, Color.cyan);
//                //Debug.DrawRay(localpos, ((BaseCraftAutopilotFlyTo)myCraft.pAutopilot).TgtDir.normalized * 50, Color.green);

//                SelectedUnitAutopilot.text = myCraft.pAutopilot.ToString() + " " + myCraft.pAutopilot.GetState();
//            }

//            if (selectedUnit.GetUnit().GetType() == typeof(BaseVehicle))
//            {
//                BaseVehicle myCraft = (BaseVehicle)selectedUnit.GetUnit();
//                SelectedUnitSpeed.text = "Speed: " + myCraft.GetSpeed().ToString();
//               // SelectedUnitDir.text = "ThrustOut: " + myCraft.ThrustOut.ToString();
//                //SelectedUnitDir.text = "Controls: " + myCraft.Controls.ToString();
//                Vector3 localpos = myCraft.GetOrg() - Engine.EngineCamera.Org;

//                Vector3 localDest = ((IUnitAi)selectedUnitAI).GetDestination() - Engine.EngineCamera.Org;
//                //Vector3 localDest = myCraft.GetAutopilot().mDest - Engine.EngineCamera.Org;
//                //localDest.y = localpos.y;
//                Debug.DrawLine(localpos, localDest, Color.white);

//                //Debug.DrawLine(localpos, localpos + (myCraft.ThrustOut * 50), Color.red);
//                //Debug.DrawLine(localpos, localpos + (myCraft.ThrustOut * 50), Color.red);
//                //Debug.DrawRay(localpos, myCraft.ThrustOut * 50, Color.red);
//                //Debug.DrawRay(localpos, myCraft.Thrust.normalized * 50, Color.cyan);
//                //Debug.DrawRay(localpos, ((BaseVehicleAutopilot)myCraft.GetAutopilot()).TgtDir.normalized * 50, Color.green);

//                SelectedUnitAutopilot.text = "AP: " + myCraft.GetAutopilot().ToString() + ":" + myCraft.GetAutopilot().GetState();
//                //SelectedUnitAutopilot.style.fontSize = 20 / SelectedUnitAutopilot.text.Length;
//            }


//            UnitInfoWindowLabel.text = GenerateUnitDescription();
//        }
//    }

//    internal void ClearLocations()
//    {
//        if (!InitDone) return;
//        LocationEnum.choices.Clear();
//    }
//    internal void AddLocation(string cameraLocation)
//    {
//        if (!InitDone) return;
//        LocationEnum.choices.Add(cameraLocation);
//    }

//    internal void ClearSelection()
//    {
//        UnselectUnit();
//        OrderMove.style.display = DisplayStyle.None;
//        OrderAttack.style.display = DisplayStyle.None;
//        OrderHangar.style.display = DisplayStyle.None;
//        OrderStop.style.display = DisplayStyle.None;

//        SelectedUnitName.style.display = DisplayStyle.None;
//        SelectedUnitSide.style.display = DisplayStyle.None;
//        SelectedUnitHealth.style.display = DisplayStyle.None;

//        UnitInfoWindow.style.display = DisplayStyle.None;
//    }

//    internal string GenerateUnitDescription()
//    {
//        if (selectedUnit == null) return "No unit selected";

//        StringBuilder sb = new StringBuilder();
//        sb.AppendLine(selectedGroupData.Callsign + " " + selectedUnitId);
//        sb.AppendLine("Contact name: " + selectedUnit.GetName());
//        sb.AppendLine(((BaseObject)selectedUnit.GetUnit()).GetObjectData().FullName);
//        sb.AppendLine("GroupAI:" + selectedGroupAI.ToString());
//        sb.AppendLine("AI:" + selectedUnitAI.ToString());
//        try
//        {
//            //sb.AppendLine("Target pos: " + ((SingleBFAi)selectedUnitAI).GetDestination());
//            sb.AppendLine("Target pos: " + ((IUnitAi)selectedUnitAI).GetDestination());
//        } catch
//        {
//            sb.AppendLine("AI not supported GetDestination()");
//        }
//        sb.AppendLine("Pitch " + selectedUnit.GetUnit().GetPitchAngle().ToString());
//        sb.AppendLine("Roll " + selectedUnit.GetUnit().GetRollAngle().ToString());
//        sb.AppendLine("Heading " + selectedUnit.GetUnit().GetHeadingAngle().ToString());
//        //sb.AppendLine("Script: " + selectedGroupData.AiScript);
//        if (selectedUnit.GetUnit().GetType() == typeof(BaseCraft))
//        {
//            BaseCraft myCraft = (BaseCraft)selectedUnit.GetUnit();
//            sb.AppendLine(myCraft.pAutopilot.ToString());
//            sb.Append(myCraft.pAutopilot.Describe());
//        }

//            return sb.ToString();
//    }
//    internal void Hide()
//    {
//        //throw new System.NotImplementedException();
//    }


//    public object OnVariable(uint id, object value) { Debug.Log("Var "+ id.ToString("X8") +" updated!"); return null; }
//}
