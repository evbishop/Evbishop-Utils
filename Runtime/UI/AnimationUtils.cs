using UnityEngine;

namespace Evbishop.Runtime.UI
{
    public static class AnimationUtils
    {
        public static Vector3 GetTargetPosition(RectTransform target, EDirection moveDirection, Vector3 startPosition, Vector3 targetLocalScale, Vector3 targetLocalEulerAngles)
        {
            if (target == null) return Vector3.zero;

            var parent = target.parent.GetComponent<RectTransform>();
            if (parent == null) return startPosition;

            parent.ForceUpdateRectTransforms();

            // Cache frequently used values
            var parentRect = parent.rect;
            var targetRect = target.rect;
            var targetPivot = target.pivot;
            var scaledWidth = targetRect.width * targetLocalScale.x;
            var scaledHeight = targetRect.height * targetLocalScale.y;

            // Calculate anchor-based offsets once
            var minAnchorOffsetX = parentRect.width * (1f - targetPivot.x) * target.anchorMin.x;
            var maxAnchorOffsetX = parentRect.width * targetPivot.x * target.anchorMax.x;
            var minAnchorOffsetY = parentRect.height * (1f - targetPivot.y) * target.anchorMin.y;
            var maxAnchorOffsetY = parentRect.height * targetPivot.y * target.anchorMax.y;

            // Calculate position based on direction
            var (position, xDirection, yDirection) = CalculateDirectionalPosition(
                moveDirection,
                startPosition,
                parentRect,
                scaledWidth,
                scaledHeight,
                targetPivot,
                minAnchorOffsetX,
                maxAnchorOffsetX,
                minAnchorOffsetY,
                maxAnchorOffsetY
            );

            // Apply rotation offset if necessary
            if (!Mathf.Approximately(0, targetLocalEulerAngles.z))
            {
                position = ApplyRotationOffset(
                    position,
                    targetLocalEulerAngles.z,
                    scaledWidth,
                    scaledHeight,
                    xDirection,
                    yDirection
                );
            }

            return position;
        }

        private static (Vector3 position, float xDir, float yDir) CalculateDirectionalPosition(
            EDirection moveDirection,
            Vector3 startPosition,
            Rect parentRect,
            float scaledWidth,
            float scaledHeight,
            Vector2 targetPivot,
            float minAnchorOffsetX,
            float maxAnchorOffsetX,
            float minAnchorOffsetY,
            float maxAnchorOffsetY)
        {
            // Calculate base offsets
            float xOffset = CalculateHorizontalOffset(moveDirection, parentRect.width, scaledWidth, targetPivot, minAnchorOffsetX, maxAnchorOffsetX);
            float yOffset = CalculateVerticalOffset(moveDirection, parentRect.height, scaledHeight, targetPivot, minAnchorOffsetY, maxAnchorOffsetY);

            var x = startPosition.x;
            var y = startPosition.y;
            var z = startPosition.z;

            return moveDirection switch
            {
                EDirection.Left => (new Vector3(-xOffset, y, z), -1, 0),
                EDirection.Right => (new Vector3(xOffset, y, z), 1, 0),
                EDirection.Top => (new Vector3(x, yOffset, z), 0, 1),
                EDirection.Bottom => (new Vector3(x, -yOffset, z), 0, -1),
                EDirection.TopLeft => (new Vector3(-xOffset, yOffset, z), -1, 1),
                EDirection.TopCenter => (new Vector3(0, yOffset, z), 0, 1),
                EDirection.TopRight => (new Vector3(xOffset, yOffset, z), 1, 1),
                EDirection.MiddleLeft => (new Vector3(-xOffset, 0, z), -1, 0),
                EDirection.MiddleCenter => (new Vector3(0, 0, z), 0, 0),
                EDirection.MiddleRight => (new Vector3(xOffset, 0, z), 1, 0),
                EDirection.BottomLeft => (new Vector3(-xOffset, -yOffset, z), -1, -1),
                EDirection.BottomCenter => (new Vector3(0, -yOffset, z), 0, -1),
                EDirection.BottomRight => (new Vector3(xOffset, -yOffset, z), 1, -1),
                _ => (startPosition, 0, 0)
            };
        }

        private static float CalculateHorizontalOffset(
            EDirection direction,
            float parentWidth,
            float scaledWidth,
            Vector2 targetPivot,
            float minAnchorOffset,
            float maxAnchorOffset)
        {
            switch (direction)
            {
                case EDirection.Left:
                case EDirection.TopLeft:
                case EDirection.MiddleLeft:
                case EDirection.BottomLeft:
                    return (scaledWidth * (1f - targetPivot.x)) + minAnchorOffset + maxAnchorOffset;

                case EDirection.Right:
                case EDirection.TopRight:
                case EDirection.MiddleRight:
                case EDirection.BottomRight:
                    return parentWidth + (scaledWidth * targetPivot.x) - minAnchorOffset - maxAnchorOffset;

                default:
                    return 0;
            }
        }

        private static float CalculateVerticalOffset(
            EDirection direction,
            float parentHeight,
            float scaledHeight,
            Vector2 targetPivot,
            float minAnchorOffset,
            float maxAnchorOffset)
        {
            switch (direction)
            {
                case EDirection.Top:
                case EDirection.TopLeft:
                case EDirection.TopCenter:
                case EDirection.TopRight:
                    return parentHeight + (scaledHeight * targetPivot.y) - minAnchorOffset - maxAnchorOffset;

                case EDirection.Bottom:
                case EDirection.BottomLeft:
                case EDirection.BottomCenter:
                case EDirection.BottomRight:
                    return (scaledHeight * (1f - targetPivot.y)) + minAnchorOffset + maxAnchorOffset;

                default:
                    return 0;
            }
        }

        private static Vector3 ApplyRotationOffset(
            Vector3 position,
            float rotation,
            float width,
            float height,
            float xDirection,
            float yDirection)
        {
            // Normalize rotation to 0-180 range
            float angle = Mathf.Abs(rotation % 180);
            if (Mathf.Approximately(angle, 0)) return position;

            float theta = angle * Mathf.Deg2Rad;
            float newWidth, newHeight;

            if (Mathf.Approximately(angle, 90))
            {
                newWidth = height;
                newHeight = width;
            }
            else if (angle < 90)
            {
                newWidth = (width * Mathf.Cos(theta)) + (height * Mathf.Sin(theta));
                newHeight = (width * Mathf.Sin(theta)) + (height * Mathf.Cos(theta));
            }
            else
            {
                theta = (angle - 90) * Mathf.Deg2Rad;
                newWidth = (height * Mathf.Cos(theta)) + (width * Mathf.Sin(theta));
                newHeight = (height * Mathf.Sin(theta)) + (width * Mathf.Cos(theta));
            }

            float offsetX = (newWidth - width) / 2f;
            float offsetY = (newHeight - height) / 2f;

            return new Vector3(
                position.x + (offsetX * xDirection),
                position.y + (offsetY * yDirection),
                position.z
            );
        }
    }
}