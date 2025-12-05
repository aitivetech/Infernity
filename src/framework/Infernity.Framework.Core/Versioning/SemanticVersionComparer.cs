namespace Infernity.Framework.Core.Versioning;

/// <summary>Compares two <see cref="SemanticVersion"/> objects for equality.</summary>
public sealed class SemanticVersionComparer : IEqualityComparer<SemanticVersion>, IComparer<SemanticVersion>
{
    /// <inheritdoc/>
    public bool Equals(SemanticVersion? left, SemanticVersion? right)
    {
        return this.Compare(left, right) == 0;
    }

    /// <inheritdoc/>
    public int Compare(SemanticVersion? left, SemanticVersion? right)
    {
        if (left is null)
        {
            return right is null ? 0 : -1;
        }

        if (right is null)
        {
            return 1;
        }

        int majorComp;
        if (left.Major is null || right.Major is null) majorComp = 0;
        else
        {
            majorComp = left.Major.Value.CompareTo(right.Major.Value);
        }

        if (majorComp != 0)
        {
            return majorComp;
        }

        int minorComp;
        if (left.Minor is null || right.Minor is null) minorComp = 0;
        else
        {
            minorComp = left.Minor.Value.CompareTo(right.Minor.Value);
        }

        if (minorComp != 0)
        {
            return minorComp;
        }

        int patchComp;
        if (left.Patch is null || right.Patch is null) patchComp = 0;
        else
        {
            patchComp = left.Patch.Value.CompareTo(right.Patch.Value);
        }

        if (patchComp != 0)
        {
            return patchComp;
        }

        var isLeftEmpty = string.IsNullOrWhiteSpace(left.Prerelease);
        var isRightEmpty = string.IsNullOrWhiteSpace(right.Prerelease);

        if ((isLeftEmpty && isRightEmpty) 
            || (left.Prerelease == "*" && !isRightEmpty) 
            || (!isLeftEmpty && right.Prerelease == "*"))
        {
            return 0;
        }

        if (isLeftEmpty)
        {
            return +1;
        }

        if (isRightEmpty)
        {
            return -1;
        }

        var leftParts = left.Prerelease.Split(['.'], StringSplitOptions.RemoveEmptyEntries);
        var rightParts = right.Prerelease.Split(['.'], StringSplitOptions.RemoveEmptyEntries);

        for (var i = 0; i < Math.Min(leftParts.Length, rightParts.Length); i++)
        {
            var leftChar = leftParts[i];
            var rightChar = rightParts[i];

            var leftIsNum = int.TryParse(leftChar, out var componentNumVal);
            var rightIsNum = int.TryParse(rightChar, out var otherNumVal);

            if (leftIsNum && rightIsNum)
            {
                if (componentNumVal.CompareTo(otherNumVal) == 0)
                {
                    continue;
                }
                return componentNumVal.CompareTo(otherNumVal);
            }

            if (leftIsNum)
            {
                return -1;
            }

            if (rightIsNum)
            {
                return 1;
            }

            var comp = string.Compare(leftChar, rightChar, StringComparison.OrdinalIgnoreCase);
            if (comp != 0)
            {
                return comp;
            }
        }

        return leftParts.Length.CompareTo(rightParts.Length);
    }

    /// <inheritdoc/>
    public int GetHashCode(SemanticVersion obj)
    {
        return obj.GetHashCode();
    }
}