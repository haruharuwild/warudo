using System;
using System.Collections.Generic;
using UnityEngine;
using Warudo.Core;
using Warudo.Core.Server;
using Warudo.Core.Data;
using Warudo.Core.Graphs;
using Warudo.Core.Attributes;
using Warudo.Core.Data.Models;
using Warudo.Plugins.Core;
using Warudo.Plugins.Core.Assets;
using Warudo.Plugins.Core.Assets.Cinematography;
using Warudo.Core.Localization;

[NodeType(
    Id = "5de8b109-1777-42f4-8b98-7f45cadfe309",
    Title = "Rotate Camera Around Character",
    Category = "Harutora script"
)]
public class RotateCameraAroundCharacterNode : Node {

    private const float RotationSpeedMin = -1800f;
    private const float RotationSpeedMax = 1800f;
    private const float RotationSpeedDefault = 90f;

    private bool isRotating = false;
    private float? angle = null;
    private TransformData cameraInitialTD = null;
    private TransformData targetInitialTD = null;
    private CameraAsset.CameraControlMode? cameraCtrlModeBackup = null;

    [DataInput]
    [Label("CAMERA")]
    // Only show active camera
    [AssetFilter(nameof(FilterCameraAsset))]
    // Disable camera change while rotating
    [DisabledIf(nameof(IsRotating))]
    public CameraAsset CameraAsset;
    protected bool FilterCameraAsset(CameraAsset camera) {
        return camera.Active;
    }

    [DataInput]
    [Label("ASSET")]
    // Only show active asset
    [AssetFilter(nameof(FilterAsset))]
    // Disable target change while rotating
    [DisabledIf(nameof(IsRotating))]
    public GameObjectAsset TargetAsset;
    protected bool FilterAsset(GameObjectAsset asset) {
        return asset.Active;
    }

    [DataInput]
    [Label("DEGREE_PER_SECONDS")]
    [FloatSlider(RotationSpeedMin, RotationSpeedMax)]
    public float RotationSpeed = RotationSpeedDefault;

    [DataOutput]
    [Label("IS_ROTATING")]
    public bool IsRotating() => isRotating;

    // for debug
    [DataOutput]
    public float Rot() => RotationSpeed;
    [DataOutput]
    public Vector3 CameraInitialPosition() => cameraInitialTD.Position;
    [DataOutput]
    public Vector3 CameraPosition() => CameraAsset.Transform.Position;
    [DataOutput]
    public Vector3 CharaPosition() => CameraAsset.FocusCharacter.Transform.Position;
//    [DataOutput]
//    public Vector3 CameraTransformPosition() => CameraAsset.GameObject.transform.position;
    
    [FlowInput]
    [Label("ENTER")]
    public Continuation Enter() {
        if (CameraAsset.IsNullOrInactiveOrDisabled()) {
            return Exit;
        }
        if (TargetAsset.IsNullOrInactiveOrDisabled()) {
            return Exit;
        }
        // Record initial position of character and camera
        if (cameraInitialTD is null) {
            cameraInitialTD = StructuredData.Create<TransformData>();
            cameraInitialTD.CopyFrom(CameraAsset.Transform);
        }
        if (targetInitialTD is null) {
            targetInitialTD = StructuredData.Create<TransformData>();
            targetInitialTD.CopyFrom(TargetAsset.Transform);
        }
        // Force FreeLook mode while rotating
        if (cameraCtrlModeBackup is null) {
            cameraCtrlModeBackup = CameraAsset.ControlMode;
            CameraAsset.SetDataInput(nameof(CameraAsset.ControlMode), CameraAsset.CameraControlMode.FreeLook);
        }

        if (angle is null) angle = 0.0f;
        isRotating = true;
        return Exit;
    }

    [FlowInput]
    [Label("STOP")]
    public Continuation Stop() {
        // Restore camera mode
        // If the original mode is OrbitCharacter, camera position adjustment occurs upon return
        if (cameraCtrlModeBackup is not null) {
            CameraAsset.SetDataInput(nameof(CameraAsset.ControlMode), cameraCtrlModeBackup);
            cameraCtrlModeBackup = null;
        }
        if (cameraInitialTD is not null) cameraInitialTD = null;
        if (targetInitialTD is not null) targetInitialTD = null;
        if (angle is not null) angle = null;
        isRotating = false;
        return Exit;
    }

    [FlowOutput]
    [Label("EXIT")]
    public Continuation Exit;

    // Reproduction of Transform's RotateAround: Rotate the camera around the character by angle
    private TransformData RotateAround(TransformData cameraTD, TransformData characterTD, float angle) {
        TransformData newCameraTD = StructuredData.Create<TransformData>();
        Vector3 charUp = characterTD.RotationQuaternion * Vector3.up;
        Quaternion camRotateQ = Quaternion.AngleAxis(angle, charUp);
        newCameraTD.Position = camRotateQ * (cameraTD.Position - characterTD.Position) + characterTD.Position;
        newCameraTD.RotationQuaternion = camRotateQ * cameraTD.RotationQuaternion;
        return newCameraTD;
    }

    protected override void OnCreate() {
        base.OnCreate();
        Watch(nameof(RotationSpeed), OnRotationSpeedChanged);
        // localize
        LocalizationManager localizationManager = Context.LocalizationManager;
        localizationManager.SetLocalizedString("DEGREE_PER_SECONDS", "en", "Angular Velocity(deg/s)");
        localizationManager.SetLocalizedString("DEGREE_PER_SECONDS", "ja", "角速度(deg/s)");
        localizationManager.SetLocalizedString("IS_ROTATING", "en", "Is Rotating");
        localizationManager.SetLocalizedString("IS_ROTATING", "ja", "回転中");
    }

    public override void OnUpdate() {
        base.OnUpdate();
        if (isRotating) {
            // Rotate relative to the initial position
            angle = (angle + RotationSpeed * Time.deltaTime) % 360f;
            TransformData newCameraTD = RotateAround(cameraInitialTD, targetInitialTD, (float)angle);
            CameraAsset.SetDataInput(nameof(CameraAsset.Transform), newCameraTD);
        }
    }

    public void OnRotationSpeedChanged() {
        RotationSpeed = Math.Clamp(RotationSpeed, RotationSpeedMin, RotationSpeedMax);
        Broadcast();
    }
}
