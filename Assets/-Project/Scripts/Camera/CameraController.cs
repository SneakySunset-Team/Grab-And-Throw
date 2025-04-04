using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    public CinemachineCamera virtualCamera;
    public Camera mainCamera;
    public CinemachineTargetGroup targetGroup;

    [Header("Zones")]
    [Range(0f, 1f)]
    public float innerZoneWidth = 0.2f;
    [Range(0f, 1f)]
    public float innerZoneHeight = 0.2f;
    [Range(0f, 1f)]
    public float outerZoneWidth = 0.8f;
    [Range(0f, 1f)]
    public float outerZoneHeight = 0.8f;

    [Header("Zoom Parameters")]
    public float minFOV = 8f;
    public float maxFOV = 25f;
    public float framingSize = .24f;
    public float minFramingSize = .2f;
    public float maxFramingSize = .28f;
    public float zoomSpeed = 2f;

    private CinemachineGroupFraming groupFraming;
    private CinemachineRotationComposer rotationComposer;
    private Rect screenRect;
    private Vector2 screenCenter;
    private bool playersInInnerZone = true;
    private bool playerNearOuterZone = false;

    void Start()
    {
        // Get Cinemachine components using the updated Unity 6 method
        groupFraming = virtualCamera.GetComponent<CinemachineGroupFraming>();
        rotationComposer = virtualCamera.GetComponent<CinemachineRotationComposer>();

        ConfigureDeadZoneAndHardLimits();

        screenRect = new Rect(0, 0, Screen.width, Screen.height);
        screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
    }

    void LateUpdate()
    {
        if (targetGroup == null || targetGroup.Targets.Count == 0)
            return;

        AnalyzePlayerPositions();
        AdjustCamera();
    }

    void ConfigureDeadZoneAndHardLimits()
    {
        if (rotationComposer != null)
        {
            rotationComposer.Composition.DeadZone.Size = new Vector2(innerZoneWidth, innerZoneHeight);

            rotationComposer.Composition.HardLimits.Size = new Vector2(outerZoneWidth, outerZoneHeight);

            rotationComposer.Damping = new Vector2(0.5f, 0.5f);
        }

        if (groupFraming != null)
        {
            groupFraming.FramingMode = CinemachineGroupFraming.FramingModes.HorizontalAndVertical;
            groupFraming.SizeAdjustment = CinemachineGroupFraming.SizeAdjustmentModes.DollyThenZoom;
            groupFraming.DollyRange = new Vector2(-100f, 100f); 
            groupFraming.FovRange = new Vector2(minFOV, maxFOV);
            groupFraming.Damping = 2f;
            groupFraming.FramingSize = framingSize; 
        }
    }

    void AnalyzePlayerPositions()
    {
        playersInInnerZone = true;
        playerNearOuterZone = false;

        Rect innerZoneRect = new Rect(
            screenCenter.x - (Screen.width * innerZoneWidth / 2f),
            screenCenter.y - (Screen.height * innerZoneHeight / 2f),
            Screen.width * innerZoneWidth,
            Screen.height * innerZoneHeight
        );

        Rect outerZoneRect = new Rect(
            screenCenter.x - (Screen.width * outerZoneWidth / 2f),
            screenCenter.y - (Screen.height * outerZoneHeight / 2f),
            Screen.width * outerZoneWidth,
            Screen.height * outerZoneHeight
        );

        foreach (var target in targetGroup.Targets)
        {
            if (target.Object == null)
                continue;

            Vector3 screenPos = mainCamera.WorldToScreenPoint(target.Object.position);

            if (!innerZoneRect.Contains(new Vector2(screenPos.x, screenPos.y)))
                playersInInnerZone = false;

            if (!outerZoneRect.Contains(new Vector2(screenPos.x, screenPos.y)))
                playerNearOuterZone = true;
        }

        AnalyzeQuadrants();
    }

    void AnalyzeQuadrants()
    {
        if (targetGroup.Targets.Count == 0)
            return;

        int[] quadrantCounts = new int[4] { 0, 0, 0, 0 };

        foreach (var target in targetGroup.Targets)
        {
            if (target.Object == null)
                continue;

            Vector3 screenPos = mainCamera.WorldToScreenPoint(target.Object.position);

            // Determine quadrant (0: bottom-left, 1: bottom-right, 2: top-left, 3: top-right)
            bool isRight = screenPos.x > screenCenter.x;
            bool isTop = screenPos.y > screenCenter.y;
            int quadrant = (isTop ? 2 : 0) + (isRight ? 1 : 0);

            quadrantCounts[quadrant]++;
        }

        AdjustCameraByQuadrants(quadrantCounts);
    }

    void AdjustCameraByQuadrants(int[] quadrantCounts)
    {
        int majorityQuadrant = -1;
        int maxCount = 0;

        for (int i = 0; i < 4; i++)
        {
            if (quadrantCounts[i] > maxCount)
            {
                maxCount = quadrantCounts[i];
                majorityQuadrant = i;
            }
        }

        int topCount = quadrantCounts[2] + quadrantCounts[3];
        int bottomCount = quadrantCounts[0] + quadrantCounts[1];
        int leftCount = quadrantCounts[0] + quadrantCounts[2];
        int rightCount = quadrantCounts[1] + quadrantCounts[3];

        Vector2 targetOffset = Vector2.zero;

        // Adjust based on halves
        if (topCount > bottomCount)
        {
            targetOffset.y = 0.1f;  // Slight upward shift
        }
        else if (bottomCount > topCount)
        {
            targetOffset.y = -0.1f; // Slight downward shift
        }

        if (rightCount > leftCount)
        {
            targetOffset.x = 0.1f;  // Slight rightward shift
        }
        else if (leftCount > rightCount)
        {
            targetOffset.x = -0.1f; // Slight leftward shift
        }

        // If one quadrant has a clear majority, apply diagonal shift
        if (maxCount > (targetGroup.Targets.Count / 2))
        {
            bool isRight = (majorityQuadrant == 1 || majorityQuadrant == 3);
            bool isTop = (majorityQuadrant == 2 || majorityQuadrant == 3);

            targetOffset.x = isRight ? 0.15f : -0.15f;
            targetOffset.y = isTop ? 0.15f : -0.15f;
        }

        // Apply offset to GroupFraming CenterOffset
        if (groupFraming != null)
        {
            groupFraming.CenterOffset = Vector2.Lerp(
                groupFraming.CenterOffset,
                targetOffset,
                Time.deltaTime * 2f
            );
        }
    }

    void AdjustCamera()
    {
        // Adjust zoom based on player positions
        if (groupFraming != null)
        {
            // If all players are in inner zone, tighten framing
            if (playersInInnerZone)
            {
                // Gradually reduce framing size
                groupFraming.FramingSize = Mathf.Lerp(
                    groupFraming.FramingSize,
                    minFramingSize, 
                    Time.deltaTime * zoomSpeed
                );
            }
            // If a player is near outer zone, widen framing
            else if (playerNearOuterZone)
            {
                // Gradually increase framing size
                groupFraming.FramingSize = Mathf.Lerp(
                    groupFraming.FramingSize,
                    maxFramingSize, 
                    Time.deltaTime * zoomSpeed
                );
            }
            // Otherwise maintain moderate framing
            else
            {
                groupFraming.FramingSize = Mathf.Lerp(
                    groupFraming.FramingSize,
                    framingSize,
                    Time.deltaTime * zoomSpeed
                );
            }
        }
    }
}
