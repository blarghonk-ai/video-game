using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// Crash & Build — Phase 1 scene builder.
/// Menu: Crash & Build > Setup Phase 1 Scene
///
/// Also auto-runs once on load if the scene doesn't exist yet.
/// </summary>
[InitializeOnLoad]
public static class PhaseOneSetup
{
    static PhaseOneSetup()
    {
        string scenePath = Application.dataPath + "/Scenes/Phase1_Sandbox.unity";
        if (!File.Exists(scenePath))
        {
            EditorApplication.delayCall += AutoSetup;
        }
        else
        {
            // Scene exists — auto-fix pink materials once per session
            // Double delayCall gives Unity two frames to fully load the scene first
            EditorApplication.delayCall += () =>
                EditorApplication.delayCall += AutoFixMaterials;
        }
    }

    static void AutoSetup()
    {
        Debug.Log("Crash & Build: Auto-building Phase 1 scene...");
        BuildScene();
    }

    static void AutoFixMaterials()
    {
        // Only fix if there are actually pink (non-URP) materials in the scene
        var renderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        bool needsFix = false;
        foreach (var r in renderers)
        {
            if (r.sharedMaterial != null &&
                !r.sharedMaterial.shader.name.Contains("Universal Render Pipeline"))
            {
                needsFix = true;
                break;
            }
        }
        if (needsFix)
        {
            Debug.Log("Crash & Build: Auto-fixing pink materials...");
            FixPinkMaterials();
        }
    }

    [MenuItem("Crash & Build/Setup Phase 1 Scene")]
    static void SetupPhase1Scene()
    {
        if (!EditorUtility.DisplayDialog(
            "Build Phase 1 Sandbox",
            "This will clear the current scene and build the Phase 1 physics sandbox.\n\nContinue?",
            "Yes, Build It!", "Cancel"))
            return;

        BuildScene();
    }

    static void BuildScene()
    {
        // ── Fresh scene ──────────────────────────────────────────
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── Build everything ─────────────────────────────────────
        AddCarPartTag();
        CreateLighting();
        CreateGround();
        CreateWall();
        CreateRamp();
        CreateBarrels();
        var car = CreateCar();
        SetupCamera(car.transform);

        // ── Save ─────────────────────────────────────────────────
        Directory.CreateDirectory(Application.dataPath + "/Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Phase1_Sandbox.unity");
        AssetDatabase.Refresh();

        Debug.Log("Crash & Build: Phase 1 scene ready! Press Play to drive. WASD = drive, Space = handbrake, R = reset.");
    }

    // ── Tag Setup ────────────────────────────────────────────────

    static void AddCarPartTag()
    {
        var tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tagsProp = tagManager.FindProperty("tags");

        for (int i = 0; i < tagsProp.arraySize; i++)
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == "CarPart") return;

        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = "CarPart";
        tagManager.ApplyModifiedProperties();
    }

    // ── World Objects ────────────────────────────────────────────

    static void CreateLighting()
    {
        var sun = new GameObject("Sun");
        var light = sun.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.color = new Color(1f, 0.97f, 0.88f);
        light.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(52f, -30f, 0f);
    }

    static void CreateGround()
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.position = new Vector3(0f, -0.25f, 0f);
        ground.transform.localScale = new Vector3(200f, 0.5f, 200f);
        SetColor(ground, new Color(0.33f, 0.52f, 0.22f)); // grass green
    }

    static void CreateWall()
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "CrashWall";
        wall.transform.position = new Vector3(0f, 2.5f, 38f);
        wall.transform.localScale = new Vector3(20f, 5f, 1.2f);
        SetColor(wall, new Color(0.85f, 0.18f, 0.18f)); // red
    }

    static void CreateRamp()
    {
        var ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ramp.name = "Ramp";
        ramp.transform.position = new Vector3(0f, 0.45f, 18f);
        ramp.transform.localScale = new Vector3(10f, 0.3f, 8f);
        ramp.transform.rotation = Quaternion.Euler(-16f, 0f, 0f);
        SetColor(ramp, new Color(0.9f, 0.62f, 0.1f)); // orange
    }

    static void CreateBarrels()
    {
        // A row of barrels to knock over — kids love this
        float[] xPositions = { -5f, -2.5f, 0f, 2.5f, 5f };
        foreach (float x in xPositions)
        {
            var barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.name = "Barrel";
            barrel.transform.position = new Vector3(x, 0.6f, 8f);
            barrel.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            SetColor(barrel, new Color(0.8f, 0.4f, 0.1f)); // rusty orange

            // Give barrels physics so they roll when hit
            var rb = barrel.AddComponent<Rigidbody>();
            rb.mass = 50f;
        }
    }

    // ── Car ──────────────────────────────────────────────────────

    static GameObject CreateCar()
    {
        // ── Root GameObject ──────────────────────────────────────
        var car = new GameObject("Car");
        car.transform.position = new Vector3(0f, 1.1f, -12f);

        var rb = car.AddComponent<Rigidbody>();
        rb.mass = 1200f;
        rb.linearDamping    = 0.1f;
        rb.angularDamping   = 0.05f;
        rb.centerOfMass     = new Vector3(0f, -0.4f, 0f);
        rb.interpolation    = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Car body collider (on the root so impact forces register here)
        var bodyCol     = car.AddComponent<BoxCollider>();
        bodyCol.size    = new Vector3(1.8f, 0.65f, 4.0f);
        bodyCol.center  = new Vector3(0f, 0.1f, 0f);

        car.AddComponent<CarController>();
        car.AddComponent<CarPartBreaker>();
        car.AddComponent<CarReset>();

        // ── Car body mesh ────────────────────────────────────────
        var body = CreateChildCube(car, "CarBody",
            new Vector3(0f, 0.05f, 0f),
            new Vector3(1.8f, 0.65f, 4.0f),
            new Color(0.18f, 0.42f, 0.9f)); // bright blue
        Object.DestroyImmediate(body.GetComponent<BoxCollider>()); // root collider handles this

        // Cab / roof
        var roof = CreateChildCube(car, "CarRoof",
            new Vector3(0f, 0.58f, -0.3f),
            new Vector3(1.5f, 0.5f, 2.2f),
            new Color(0.15f, 0.38f, 0.8f)); // slightly darker blue
        Object.DestroyImmediate(roof.GetComponent<BoxCollider>());

        // ── Wheels ───────────────────────────────────────────────
        // (localPosition relative to car root)
        var wheelPositions = new Vector3[]
        {
            new Vector3(-0.92f, -0.2f,  1.4f),  // [0] Front Left
            new Vector3( 0.92f, -0.2f,  1.4f),  // [1] Front Right
            new Vector3(-0.92f, -0.2f, -1.4f),  // [2] Rear  Left
            new Vector3( 0.92f, -0.2f, -1.4f),  // [3] Rear  Right
        };
        string[] wNames = { "FL", "FR", "RL", "RR" };

        var colliders = new WheelCollider[4];
        var meshes    = new Transform[4];

        for (int i = 0; i < 4; i++)
        {
            colliders[i] = CreateWheelCollider(car, $"Wheel_{wNames[i]}_Collider", wheelPositions[i]);
            meshes[i]    = CreateWheelMesh(car, $"Wheel_{wNames[i]}_Mesh", wheelPositions[i]).transform;
        }

        // ── Wire CarController fields ────────────────────────────
        var cc = car.GetComponent<CarController>();
        cc.frontLeftWheel  = colliders[0];
        cc.frontRightWheel = colliders[1];
        cc.rearLeftWheel   = colliders[2];
        cc.rearRightWheel  = colliders[3];
        cc.frontLeftMesh   = meshes[0];
        cc.frontRightMesh  = meshes[1];
        cc.rearLeftMesh    = meshes[2];
        cc.rearRightMesh   = meshes[3];

        return car;
    }

    static WheelCollider CreateWheelCollider(GameObject parent, string name, Vector3 localPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.identity;

        var wc = go.AddComponent<WheelCollider>();
        wc.radius = 0.35f;
        wc.mass   = 30f;
        wc.suspensionDistance = 0.2f;
        wc.suspensionSpring = new JointSpring
        {
            spring         = 25000f,
            damper         = 1500f,
            targetPosition = 0.5f
        };
        wc.forwardFriction  = FrictionCurve(0.4f, 1.0f, 0.8f, 0.75f, 1.5f);
        wc.sidewaysFriction = FrictionCurve(0.2f, 1.0f, 0.5f, 0.75f, 2.0f);

        return wc;
    }

    static GameObject CreateWheelMesh(GameObject parent, string name, Vector3 localPos)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = name;
        go.transform.SetParent(parent.transform);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.Euler(0f, 0f, 90f); // orient wheel
        go.transform.localScale    = new Vector3(0.35f, 0.15f, 0.35f);
        go.tag = "CarPart"; // allows CarPartBreaker to detach wheels
        SetColor(go, new Color(0.12f, 0.12f, 0.12f)); // black rubber
        Object.DestroyImmediate(go.GetComponent<CapsuleCollider>()); // WheelCollider handles physics
        return go;
    }

    // ── Camera ───────────────────────────────────────────────────

    static void SetupCamera(Transform carTransform)
    {
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        camGO.AddComponent<Camera>();
        camGO.AddComponent<AudioListener>();

        // Start positioned behind and above the car
        camGO.transform.position = carTransform.position + new Vector3(0f, 4f, -11f);
        camGO.transform.LookAt(carTransform.position + Vector3.up);

        var follow = camGO.AddComponent<SimpleCameraFollow>();
        follow.target = carTransform;
    }

    // ── Helpers ──────────────────────────────────────────────────

    static GameObject CreateChildCube(GameObject parent, string name,
        Vector3 localPos, Vector3 localScale, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent.transform);
        go.transform.localPosition = localPos;
        go.transform.localScale    = localScale;
        SetColor(go, color);
        return go;
    }

    static WheelFrictionCurve FrictionCurve(
        float extSlip, float extVal, float asymSlip, float asymVal, float stiffness)
        => new WheelFrictionCurve
        {
            extremumSlip   = extSlip,
            extremumValue  = extVal,
            asymptoteSlip  = asymSlip,
            asymptoteValue = asymVal,
            stiffness      = stiffness
        };

    static void SetColor(GameObject go, Color color)
    {
        var rend = go.GetComponent<Renderer>();
        if (rend == null) return;
        rend.sharedMaterial = MakeURPMaterial(color);
    }

    static Material MakeURPMaterial(Color color)
    {
        // Try every known URP shader name (Unity 6 uses "Universal Render Pipeline/Lit")
        string[] urpShaders = {
            "Universal Render Pipeline/Lit",
            "Universal Render Pipeline/Simple Lit",
            "Universal Render Pipeline/Unlit"
        };
        Shader shader = null;
        foreach (var name in urpShaders)
        {
            shader = Shader.Find(name);
            if (shader != null) break;
        }
        // Final fallback — Standard shader (works without URP, but shows pink with URP active)
        shader = shader ?? Shader.Find("Standard");
        if (shader == null) return null;

        var mat = new Material(shader);
        mat.color = color;
        // URP uses _BaseColor for the main color property
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", color);
        return mat;
    }

    // ── Fix Materials menu item ───────────────────────────────────
    // Run this if the scene looks pink/magenta (URP shader wasn't ready on first build)

    [MenuItem("Crash & Build/Fix Pink Materials")]
    static void FixPinkMaterials()
    {
        var renderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        int fixed_count = 0;

        foreach (var rend in renderers)
        {
            // Skip anything that already has a proper URP material
            if (rend.sharedMaterial == null) continue;
            if (rend.sharedMaterial.shader.name.Contains("Universal Render Pipeline")) continue;

            // Re-read the color from the existing material and rebuild with URP shader
            Color col = rend.sharedMaterial.HasProperty("_Color")
                ? rend.sharedMaterial.GetColor("_Color")
                : Color.white;

            var newMat = MakeURPMaterial(col);
            if (newMat != null)
            {
                rend.sharedMaterial = newMat;
                fixed_count++;
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"Crash & Build: Fixed {fixed_count} pink materials. Scene saved.");
    }
}
